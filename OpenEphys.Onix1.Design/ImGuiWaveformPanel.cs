using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Numerics;

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
    internal sealed class ImGuiWaveformPanel : IDisposable
    {
        const float TextBoxWidth = 80;
        const int MinChannelHeight = 10;
        const int TimeChannelHeight = 25;

        static readonly double[] StandardTimeBases =
        {
            0.05, 0.1, 0.25, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0, 10.0, 20.0
        };

        static readonly double[] StandardRanges =
        {
            50, 100, 200, 500, 1000, 2000
        };

        Decimator decimatorMin;
        Decimator decimatorMax;
        Mat timeRange;
        Mat minSnap;
        Mat maxSnap;

        Mat rowOffsets;
        Mat displayMin;
        Mat displayMax;
        double timeSpan;

        int sampleRate = 30000;
        int channelHeight = 20;
        int maxSamplesPerChannel = 1920;
        double timebase = 2.0;
        double rangeAmplitude = 500;
        int colorGrouping = 1;

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
                decimatorMin.Buffer.Rows != data.Rows ||
                decimatorMin.Buffer.Cols != columns ||
                decimatorMin.InputDepth != data.Depth ||
                decimatorMin.DownsampleFactor != samplesPerBin)
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

        bool InputDoubleCombo(string label, ref double value, double[] comboItems)
        {
            var changed = false;
            var editValue = value;
            ImGui.InputDouble(label, ref editValue, "%g");
            if (changed = ImGui.IsItemDeactivatedAfterEdit())
                value = editValue;
            ImGui.SameLine(0, 0);

            var comboFlags = ImGuiComboFlags.NoPreview | ImGuiComboFlags.PopupAlignLeft;
            if (ImGui.BeginCombo(label + "C", string.Empty, comboFlags))
            {
                for (int i = 0; i < comboItems.Length; i++)
                {
                    var isSelected = value == comboItems[i];
                    if (ImGui.Selectable(comboItems[i].ToString("G"), isSelected))
                    {
                        value = comboItems[i];
                        changed = true;
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            return changed;
        }

        void BandCombo()
        {
            var singleBand = Bands.Count <= 1;
            var preview = SelectedBand >= 0 && SelectedBand < Bands.Count ? Bands[SelectedBand].Name : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##band", preview))
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    var isSelected = i == SelectedBand;
                    var (name, description) = Bands[i];
                    if (ImGui.Selectable($"{name}: {description}", isSelected) && !isSelected)
                        SelectedBand = i;
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (singleBand) ImGui.EndDisabled();
        }

        void MenuWidgets()
        {
            var tableFlags = ImGuiTableFlags.NoSavedSettings;
            if (ImGui.BeginTable("##menu", columns: 6, tableFlags))
            {
                ImGui.TableNextRow();
                ImGui.PushItemWidth(TextBoxWidth);

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##bandT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Band");
                    BandCombo();
                    ImGui.SameLine();
                    var cmr = UseCommonMedianReference;
                    if (ImGui.Checkbox("Apply CMR", ref cmr))
                        UseCommonMedianReference = cmr;
                    ImGui.EndTable();
                }

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##timebaseT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Timebase (s)");
                    // NB: a paused snapshot holds decimated data at one bin width, so its timebase
                    // cannot change until the display resumes.
                    ImGui.BeginDisabled(minSnap is not null);
                    InputDoubleCombo("##timebase", ref timebase, StandardTimeBases);
                    ImGui.EndDisabled();
                    ImGui.EndTable();
                }

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##channelHeightT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Chan. Height");
                    ImGui.DragInt(
                        "##channelHeight",
                        ref channelHeight,
                        vSpeed: 1,
                        MinChannelHeight,
                        int.MaxValue,
                        ImGuiSliderFlags.AlwaysClamp);
                    ImGui.EndTable();
                }

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##rangeT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    var rangeInputLabel = string.IsNullOrEmpty(RangeLabel) ? "Range" : $"Range ({RangeLabel})";
                    ImGui.Text(rangeInputLabel);
                    if (InputDoubleCombo("##range", ref rangeAmplitude, StandardRanges))
                        rangeAmplitude = Math.Max(1, rangeAmplitude);
                    ImGui.EndTable();
                }

                ImGui.TableNextColumn();
                var isButtonPressed = minSnap is not null;
                if (isButtonPressed)
                {
                    var buttonPressedColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
                    ImGui.PushStyleColor(ImGuiCol.Button, buttonPressedColor);
                }

                var buttonSize = new Vector2(TextBoxWidth, ImGui.GetFrameHeight() * 2);
                if (ImGui.Button("Pause", buttonSize) || ImGui.IsKeyPressed(ImGuiKey.Space))
                {
                    if (minSnap is not null)
                    {
                        minSnap = null;
                        maxSnap = null;
                    }
                    else if (decimatorMin is not null)
                    {
                        minSnap = decimatorMin.Buffer.Clone();
                        maxSnap = decimatorMax.Buffer.Clone();
                    }
                }

                if (isButtonPressed)
                    ImGui.PopStyleColor();

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##colorGroupingT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Colour Groups");
                    if (ImGui.InputInt("##colorGrouping", ref colorGrouping))
                        colorGrouping = Math.Max(1, colorGrouping);
                    ImGui.EndTable();
                }

                ImGui.PopItemWidth();
                ImGui.EndTable();
            }
        }

        /// <summary>
        /// Draws every channel into one plot whose y axis is in channel units: channel <c>i</c> is
        /// centred at <c>-i</c> with +/- range / 2 mapped to +/- 0.5, so a trace that exceeds its range
        /// runs into the neighbouring channels' bands instead of being clipped at a row edge.
        /// </summary>
        /// <remarks>
        /// The decimated buffers are transformed into those units each frame as
        /// <c>data / range + rowOffsets</c>, where <c>rowOffsets</c> is the constant <c>-i</c> term, one
        /// row per channel. Only the channels inside the table's visible scroll range are submitted.
        /// </remarks>
        /// <param name="minBuffer">Per-bin minima, one row per channel.</param>
        /// <param name="maxBuffer">Per-bin maxima, one row per channel.</param>
        unsafe void WaveformPlot(Mat minBuffer, Mat maxBuffer)
        {
            CV.AddWeighted(minBuffer, 1 / rangeAmplitude, rowOffsets, 1, 0, displayMin);
            CV.AddWeighted(maxBuffer, 1 / rangeAmplitude, rowOffsets, 1, 0, displayMax);
            displayMin.GetRawData(out IntPtr minPtr, out int minStep, out Size minShape);
            displayMax.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            timeRange.GetRawData(out IntPtr timeRangePtr, out int _, out Size _);
            var rows = minShape.Height;
            var columns = minShape.Width;

            ImPlot.PushStyleVar(ImPlotStyleVar.Padding, new Vector2(0, 0));
            ImPlot.PushStyleVar(ImPlotStyleVar.BorderSize, 0);
            ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);

            var tableFlags = ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY;
            var timePlotFlags = ImPlotFlags.CanvasOnly | ImPlotFlags.NoFrame;
            var dataPlotFlags = timePlotFlags | ImPlotFlags.NoInputs;
            var axesFlags = ImPlotAxisFlags.NoHighlight;
            var bareAxesFlags = axesFlags | ImPlotAxisFlags.NoDecorations;

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, -1)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, 10);
                ImGui.TableSetupColumn(string.Empty);
                ImGui.TableSetupScrollFreeze(0, 1);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var timeLabel = "Time";
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + TimeChannelHeight / 2);
                ImGui.Text(timeLabel);
                ImGui.TableNextColumn();
                if (ImPlot.BeginPlot(timeLabel, new(-1, TimeChannelHeight), timePlotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, bareAxesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, timeSpan, ImPlotCond.Always);
                    ImPlot.PlotInfLines(string.Empty, (float*)timeRangePtr, columns);
                    ImPlot.EndPlot();
                }

                // NB: only the channels inside the table's visible scroll range are labelled and
                // plotted. The row is one plot rows * channelHeight tall, so a channel's band starts
                // at i * channelHeight below the row.
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var rowTop = ImGui.GetCursorScreenPos().Y;
                var windowTop = ImGui.GetWindowPos().Y;
                var windowBottom = windowTop + ImGui.GetWindowSize().Y;
                var firstVisible = Math.Max(0, (int)Math.Floor((windowTop - rowTop) / channelHeight));
                var lastVisible = Math.Min(rows, (int)Math.Ceiling((windowBottom - rowTop) / channelHeight));

                var labelBuffer = stackalloc byte[32];
                var channelLabel = new StrBuilder(labelBuffer, 32);
                var labelTop = ImGui.GetCursorPosY();
                for (int i = firstVisible; i < lastVisible; i++)
                {
                    channelLabel.Reset();
                    channelLabel.Append("CH");
                    channelLabel.Append(i);
                    channelLabel.End();
                    ImGui.SetCursorPosY(labelTop + i * channelHeight + channelHeight / 2 - 5);
                    ImGui.Text(channelLabel);
                }

                ImGui.TableNextColumn();
                if (ImPlot.BeginPlot("##channels", new(-1, rows * channelHeight), dataPlotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, bareAxesFlags, bareAxesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, timeSpan, ImPlotCond.Always);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, -(rows - 1) - 0.5, 0.5, ImPlotCond.Always);
                    for (int i = firstVisible; i < lastVisible; i++)
                    {
                        var minLinePtr = (float*)((byte*)minPtr + i * minStep);
                        var maxLinePtr = (float*)((byte*)maxPtr + i * maxStep);
                        var channelColor = ImPlot.GetColormapColor(i / colorGrouping);
                        ImPlot.PushStyleColor(ImPlotCol.Line, channelColor);
                        ImPlot.PushStyleColor(ImPlotCol.Fill, channelColor);
                        ImPlot.PlotShaded(string.Empty, (float*)timeRangePtr, minLinePtr, maxLinePtr, columns);
                        ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, minLinePtr, columns);
                        ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, maxLinePtr, columns);
                        ImPlot.PopStyleColor(2);
                    }
                    ImPlot.EndPlot();
                }
                ImGui.EndTable();
            }

            ImPlot.PopStyleVar(3);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
