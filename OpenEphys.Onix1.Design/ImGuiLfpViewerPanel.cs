using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Self-contained ImGui rendering component that displays a multi-channel matrix as a stack of scrolling
    /// waveforms with peak-preserving downsampling. Draws into whatever region the caller has already opened.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The owner calls <see cref="Update"/> with each block and its sample rate, and <see cref="Draw"/>
    /// once a frame. Everything the display can be told is a property, so a control strip is one way to
    /// drive it and not a part of it.
    /// </para>
    /// <para>
    /// Ported from <c>Bonsai.Ephys.Design.WaveformVisualizer</c>, with the window and control ownership
    /// removed so it draws into whatever region the caller has opened.
    /// </para>
    /// </remarks>
    internal sealed partial class ImGuiLfpViewerPanel : IDisposable
    {
        readonly long historyBytes;

        /// <param name="historyBytes">
        /// Memory to spend on the history behind the display. The owner has already turned the seconds
        /// the user asked for into bytes, knowing its own channel count and fastest band; the seconds
        /// this buys fall back out of it once the channel count of the incoming data is known.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="historyBytes"/> is less than or equal to zero, leaving no history to move a
        /// paused view through.
        /// </exception>
        public ImGuiLfpViewerPanel(long historyBytes)
        {
            if (historyBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(historyBytes));

            this.historyBytes = historyBytes;
        }

        Decimator waveformMinDecimator;
        Decimator waveformMaxDecimator;
        Mat timeRange;
        int sampleRate = 30000;

        Mat rowOffsets;
        Mat scaledWaveformMin;
        Mat scaledWaveformMax;

        WaveformHistory history;

        bool[] channelHidden = Array.Empty<bool>();
        bool[] dragOriginal = Array.Empty<bool>();

        int expandedChannel = -1;

        int channelHeight = 20;
        int maxSamplesPerChannel = 1920;
        double timebase = 2.0;
        double rangeAmplitude = 500;

        /// <summary>
        /// Unit shown beside the amplitude range control.
        /// </summary>
        public string RangeLabel { get; set; }

        /// <summary>
        /// Seconds of data spanned by the display.
        /// </summary>
        public double Timebase
        {
            get => timebase;
            set => SetTimebase(value, PositionAtFraction(0.5f), 0.5f);
        }

        /// <summary>
        /// Height in pixels of one channel's row.
        /// </summary>
        public int ChannelHeight
        {
            get => channelHeight;
            set => channelHeight = Math.Max(MinChannelHeight, value);
        }

        /// <summary>
        /// Amplitude spanned by one channel's row, in the unit named by <see cref="RangeLabel"/>.
        /// </summary>
        public double RangeAmplitude
        {
            get => rangeAmplitude;
            set => rangeAmplitude = Math.Max(1, value);
        }

        /// <summary>
        /// Whether the display is frozen. Setting it takes or releases the paused view, which is what
        /// the history behind the display can then be moved through.
        /// </summary>
        public bool Paused
        {
            get => pauseSample >= 0;
            set
            {
                if (value == Paused) return;
                if (value) Pause();
                else Resume();
            }
        }

        /// <summary>
        /// Supplies one block of samples. Rebuilds the decimation buffers whenever the channel count,
        /// element depth or bin width no longer matches the input.
        /// </summary>
        /// <param name="data">Channel-by-sample matrix, one row per channel.</param>
        /// <param name="dataSampleRate">Sample rate of the incoming data, in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="dataSampleRate"/> is less than one, which leaves timebase without a normalizing unit.
        /// </exception>
        public void Update(Mat data, int dataSampleRate)
        {
            if (dataSampleRate < 1)
                throw new ArgumentOutOfRangeException(nameof(dataSampleRate));

            var budgetSamples = historyBytes / (data.Rows * sizeof(float));
            var capacity = (int)Math.Max(1, Math.Min(int.MaxValue, budgetSamples));
            var historyReplaced = history is null || history.Rows != data.Rows || history.Capacity != capacity;

            // NB: the history converts sample offsets to time with a single factor, so it cannot hold two
            // rates. A change of band that keeps the rate is left alone: what the history holds is what was
            // displayed, seams between band switches are included.
            var historyInvalid = historyReplaced || dataSampleRate != sampleRate;

            // NB: a paused view is read from the history, so a block that would clear it is dropped. Once
            // the view resumes, the next block still differs and is applied as usual.
            if (Paused && historyInvalid)
                return;

            sampleRate = dataSampleRate;
            if (historyReplaced)
            {
                history?.Dispose();
                history = new WaveformHistory(data.Rows, capacity);
            }

            if (historyInvalid)
            {
                history.Clear();
                RebuildTimeBases(capacity / (double)sampleRate);
            }

            var totalSamples = Math.Max(1, (int)(timebase * sampleRate));
            var samplesPerBin = (totalSamples + maxSamplesPerChannel - 1) / maxSamplesPerChannel;
            var columns = totalSamples / samplesPerBin;
            var buffersStale =
                timeRange is null ||
                waveformMinDecimator.Buffer.Rows != data.Rows ||        // # channels
                waveformMinDecimator.Buffer.Cols != columns ||          // # downsamples
                waveformMinDecimator.InputDepth != data.Depth ||        // inner type
                waveformMinDecimator.DownsampleFactor != samplesPerBin; // factor to map totalSamples -> # downsamples

            if (buffersStale)
            {
                timeRange?.Dispose();
                waveformMinDecimator?.Dispose();
                waveformMaxDecimator?.Dispose();
                rowOffsets?.Dispose();
                scaledWaveformMin?.Dispose();
                scaledWaveformMax?.Dispose();
                pannedWaveformMinDecimator?.Dispose();
                pannedWaveformMaxDecimator?.Dispose();
                waveformMinDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Min);
                waveformMaxDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Max);

                // NB: the paused view reduces out of the history into its own pair, at the same width as
                // the live one, so that changing the timebase while paused rebuilds both together.
                pannedWaveformMinDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Min);
                pannedWaveformMaxDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Max);

                // NB: the plot's own axis is the column index, so that a column lands where the
                // divisions and the cursor put it. Any other unit needs the plot's first and last column
                // to bound it, which spreads the columns over one fewer step than there are columns and
                // leaves the two placements a column apart at the narrowest timebases.
                timeRange = new Mat(1, columns, Depth.F32, 1);
                CV.Range(timeRange, 0, columns);

                rowOffsets = new Mat(data.Rows, columns, Depth.F32, 1);
                for (int i = 0; i < data.Rows; i++)
                {
                    using var row = rowOffsets.GetRow(i);
                    row.Set(Scalar.All(-i));
                }

                scaledWaveformMin = new Mat(data.Rows, columns, Depth.F32, 1);
                scaledWaveformMax = new Mat(data.Rows, columns, Depth.F32, 1);
                if (channelHidden.Length != data.Rows)
                {
                    channelHidden = new bool[data.Rows];
                    dragOriginal = new bool[data.Rows];
                }
            }

            // NB: also when only the anchor is waiting, so that a timebase change which happens to land on
            // the same column spacing still moves the window to where the gesture asked.
            if (buffersStale || anchorPending)
                RebuildWindow(samplesPerBin, columns);

            waveformMinDecimator.Process(data);
            waveformMaxDecimator.Process(data);

            if (!Paused)
                history.Write(data);
        }

        /// <summary>
        /// Draws the waveform stack into the region the caller has opened. Called once per frame.
        /// </summary>
        public void Draw()
        {
            // NB: ahead of the envelope, which resuming disposes. Handling it with the other
            // gestures would free the matrices the plot is about to read.
            if (ImGui.IsKeyPressed(ImGuiKey.Space))
                Paused = !Paused;

            if (timeRange is not null)
            {
                // NB: no vertical padding, so the plot frame lands on the region's own edges and can
                // be lined up with whatever the owner puts beside it.
                var padding = ImGui.GetStyle().WindowPadding;
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding.X, 0));
                ImGui.BeginChild("##data");
                var (waveformMin, waveformMax) = DisplayEnvelope();
                WaveformPlot(waveformMin, waveformMax);
                ImGui.EndChild();
                ImGui.PopStyleVar();
            }
        }

        /// <summary>
        /// Discards the decimation buffers so the next <see cref="Update"/> rebuilds them.
        /// </summary>
        /// <remarks>
        /// The decimators carry reduction state across calls, so anything that changes the meaning
        /// of the incoming samples has to discard them rather than let a partially filled output bin
        /// be completed from a different signal.
        /// </remarks>
        public void ResetBuffers()
        {
            timeRange?.Dispose();
            waveformMinDecimator?.Dispose();
            waveformMaxDecimator?.Dispose();
            rowOffsets?.Dispose();
            scaledWaveformMin?.Dispose();
            scaledWaveformMax?.Dispose();
            history?.Dispose();
            DisposeHistoryView();
            timeRange = null;
            waveformMinDecimator = null;
            waveformMaxDecimator = null;
            rowOffsets = null;
            scaledWaveformMin = null;
            scaledWaveformMax = null;
            history = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
