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
    /// The owner supplies <see cref="Bands"/> once and then calls <see cref="Update"/> with the matrix and
    /// sample rate of whichever band <see cref="SelectedBand"/> currently names. A probe offering a single
    /// band renders the selector disabled.
    /// </para>
    /// <para>
    /// Ported from <c>Bonsai.Ephys.Design.WaveformVisualizer</c>, with the window and control ownership
    /// removed so it draws into whatever region the caller has opened.
    /// </para>
    /// </remarks>
    internal sealed partial class ImGuiWaveformPanel : IDisposable
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
        public ImGuiWaveformPanel(long historyBytes)
        {
            if (historyBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(historyBytes));

            this.historyBytes = historyBytes;
        }

        Decimator waveformMinDecimator;
        Decimator waveformMaxDecimator;
        Mat timeRange;
        double timeSpan;
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
        /// The bands this probe offers, in display order, as a short name and a description of the
        /// passband. The closed combo shows the name; the open list shows both. The owner sets this once.
        /// </summary>
        public IReadOnlyList<(string Name, string Description)> Bands { get; set; } =
            Array.Empty<(string, string)>();

        /// <summary>
        /// Index into <see cref="Bands"/> of the band currently being drawn.
        /// </summary>
        public int SelectedBand { get; private set; }

        /// <summary>
        /// Unit shown beside the amplitude range control.
        /// </summary>
        public string RangeLabel { get; set; }

        /// <summary>
        /// Whether per-ADC common median referencing is applied to the displayed signal.
        /// </summary>
        public bool UseCommonMedianReference { get; private set; }

        // TODO: this reconstructs the change notification the widgets already return. SelectedBand and
        // UseCommonMedianReference are stored here but consumed by the owner, which therefore has to
        // poll for them. Both belong on the probe-aware panel that the split will add, next to the
        // widgets that set them, and this goes away with them.
        /// <summary>
        /// Increments whenever a control changes which signal the panel is being asked to display, so
        /// that the owner can rebind its source without knowing which control changed.
        /// </summary>
        public int SelectionRevision { get; private set; }


        /// <summary>
        /// Supplies one block of samples for the selected band. Rebuilds the decimation buffers whenever the
        /// channel count, element depth or bin width no longer matches the input.
        /// </summary>
        /// <param name="data">Channel-by-sample matrix, one row per channel.</param>
        /// <param name="bandSampleRate">Sample rate of the selected band, in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="bandSampleRate"/> is less than one, which leaves timebase without a normalizing unit.
        /// </exception>
        public void Update(Mat data, int bandSampleRate)
        {
            if (bandSampleRate < 1)
                throw new ArgumentOutOfRangeException(nameof(bandSampleRate));

            // NB: the history converts sample offsets to time with a single factor, so it cannot hold
            // two rates. A change of band that keeps the rate is left alone: what the history holds is
            // what was displayed, seams included.
            if (bandSampleRate != sampleRate)
                history?.Clear();

            sampleRate = bandSampleRate;
            var totalSamples = Math.Max(1, (int)(timebase * sampleRate));
            var samplesPerBin = (totalSamples + maxSamplesPerChannel - 1) / maxSamplesPerChannel;
            var columns = totalSamples / samplesPerBin;
            if (timeRange is null ||
                waveformMinDecimator.Buffer.Rows != data.Rows ||        // # channels
                waveformMinDecimator.Buffer.Cols != columns ||          // # downsamples
                waveformMinDecimator.InputDepth != data.Depth ||        // inner type
                waveformMinDecimator.DownsampleFactor != samplesPerBin) // factor to map totalSamples -> # downsamples
            {
                timeRange?.Dispose();
                waveformMinDecimator?.Dispose();
                waveformMaxDecimator?.Dispose();
                rowOffsets?.Dispose();
                scaledWaveformMin?.Dispose();
                scaledWaveformMax?.Dispose();
                waveformMinDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Min);
                waveformMaxDecimator = new Decimator(data, columns, samplesPerBin, ReduceOperation.Max);
                timeRange = new Mat(1, columns, Depth.F32, 1);
                CV.Range(timeRange, 0, (double)columns * samplesPerBin / sampleRate);
                timeSpan = (double)(columns - 1) * samplesPerBin / sampleRate;

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

            var budgetSamples = historyBytes / (data.Rows * sizeof(float));
            var capacity = (int)Math.Max(1, Math.Min(int.MaxValue, budgetSamples));
            if (history is null || history.Rows != data.Rows || history.Capacity != capacity)
            {
                history?.Dispose();
                history = new WaveformHistory(data.Rows, capacity);
                RebuildTimeBases(capacity / (double)sampleRate);
            }

            waveformMinDecimator.Process(data);
            waveformMaxDecimator.Process(data);

            if (!Paused)
                history.Write(data);
        }

        /// <summary>
        /// Draws the controls and the waveform stack. Called once per frame.
        /// </summary>
        public void Draw()
        {
            MenuWidgets();
            if (timeRange is not null)
            {
                ImGui.BeginChild("##data");
                var (waveformMin, waveformMax) = DisplayEnvelope();
                WaveformPlot(waveformMin, waveformMax);
                ImGui.EndChild();
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
