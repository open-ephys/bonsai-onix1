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
        Decimator decimatorMin;
        Decimator decimatorMax;
        Mat timeRange;
        double timeSpan;
        int sampleRate = 30000;

        Mat minSnap;
        Mat maxSnap;
        int sweepHeadSnap;

        Mat rowOffsets;
        Mat displayMin;
        Mat displayMax;

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
        /// Whether per-ADC common median referencing is applied to the displayed signal. Read by the
        /// owner on every frame, so it takes effect immediately.
        /// </summary>
        public bool UseCommonMedianReference { get; set; }

        /// <summary>
        /// Supplies one block of samples for the selected band. Rebuilds the decimation buffers whenever the
        /// channel count, element depth or bin width no longer matches the input.
        /// </summary>
        /// <param name="data">Channel-by-sample matrix, one row per channel.</param>
        /// <param name="bandSampleRate">Sample rate of the selected band, in Hz.</param>
        public void Update(Mat data, int bandSampleRate)
        {
            sampleRate = bandSampleRate;
            var totalSamples = Math.Max(1, (int)(timebase * sampleRate));
            var samplesPerBin = (totalSamples + maxSamplesPerChannel - 1) / maxSamplesPerChannel;
            var columns = totalSamples / samplesPerBin;
            if (timeRange is null ||
                decimatorMin.Buffer.Rows != data.Rows ||        // # channels
                decimatorMin.Buffer.Cols != columns ||          // # downsamples
                decimatorMin.InputDepth != data.Depth ||        // inner type
                decimatorMin.DownsampleFactor != samplesPerBin) // factor to map totalSamples -> # downsamples
            {
                timeRange?.Dispose();
                decimatorMin?.Dispose();
                decimatorMax?.Dispose();
                rowOffsets?.Dispose();
                displayMin?.Dispose();
                displayMax?.Dispose();
                decimatorMin = new Decimator(data, columns, samplesPerBin, ReduceOperation.Min);
                decimatorMax = new Decimator(data, columns, samplesPerBin, ReduceOperation.Max);
                timeRange = new Mat(1, columns, Depth.F32, 1);
                CV.Range(timeRange, 0, (double)columns * samplesPerBin / sampleRate);
                timeSpan = (double)(columns - 1) * samplesPerBin / sampleRate;

                rowOffsets = new Mat(data.Rows, columns, Depth.F32, 1);
                for (int i = 0; i < data.Rows; i++)
                {
                    using var row = rowOffsets.GetRow(i);
                    row.Set(Scalar.All(-i));
                }

                displayMin = new Mat(data.Rows, columns, Depth.F32, 1);
                displayMax = new Mat(data.Rows, columns, Depth.F32, 1);
                if (channelHidden.Length != data.Rows)
                {
                    channelHidden = new bool[data.Rows];
                    dragOriginal = new bool[data.Rows];
                }
            }

            decimatorMin.Process(data);
            decimatorMax.Process(data);
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
                WaveformPlot(minSnap ?? decimatorMin.Buffer, maxSnap ?? decimatorMax.Buffer);
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
            decimatorMin?.Dispose();
            decimatorMax?.Dispose();
            minSnap?.Dispose();
            maxSnap?.Dispose();
            rowOffsets?.Dispose();
            displayMin?.Dispose();
            displayMax?.Dispose();
            timeRange = null;
            decimatorMin = null;
            decimatorMax = null;
            minSnap = null;
            maxSnap = null;
            rowOffsets = null;
            displayMin = null;
            displayMax = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
