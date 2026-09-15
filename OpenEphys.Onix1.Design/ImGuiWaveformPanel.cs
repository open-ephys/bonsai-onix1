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

        Decimator decimatorMin;
        Decimator decimatorMax;
        Mat timeRange;
        Mat minSnap;
        Mat maxSnap;

        int sampleRate = 30000;
        int channelHeight = 20;
        int maxSamplesPerChannel = 1920; 
        double timebase = 2.0;
        int colorGrouping = 1;

        bool useFixedRange;
        float rangeAmplitude;
        double vMin;
        double vMax;

        /// <summary>
        /// The bands this probe offers, in display order. The owner sets this once.
        /// </summary>
        public IReadOnlyList<string> Bands { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Index into <see cref="Bands"/> of the band currently being drawn.
        /// </summary>
        public int SelectedBand { get; private set; }

        /// <summary>
        /// Unit shown beside the amplitude range control.
        /// </summary>
        public string RangeLabel { get; set; }

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
                decimatorMin = new Decimator(data, columns, samplesPerBin, ReduceOperation.Min);
                decimatorMax = new Decimator(data, columns, samplesPerBin, ReduceOperation.Max);
                timeRange = new Mat(1, columns, Depth.F32, 1);
                CV.Range(timeRange, 0, (double)columns * samplesPerBin / sampleRate);
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
            timeRange = null;
            decimatorMin = null;
            decimatorMax = null;
            minSnap = null;
            maxSnap = null;
        }

        bool InputDoubleCombo(string label, ref double value, double[] comboItems)
        {
            var changed = false;
            var editValue = value;
            ImGui.InputDouble(label, ref editValue, "%.2g");
            if (changed = ImGui.IsItemDeactivatedAfterEdit())
                value = editValue;
            ImGui.SameLine(0, 0);

            var comboFlags = ImGuiComboFlags.NoPreview | ImGuiComboFlags.PopupAlignLeft;
            if (ImGui.BeginCombo(label + "C", string.Empty, comboFlags))
            {
                for (int i = 0; i < comboItems.Length; i++)
                {
                    var isSelected = value == comboItems[i];
                    if (ImGui.Selectable(comboItems[i].ToString("G2"), isSelected))
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
            var preview = SelectedBand >= 0 && SelectedBand < Bands.Count ? Bands[SelectedBand] : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##band", preview))
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    var isSelected = i == SelectedBand;
                    if (ImGui.Selectable(Bands[i], isSelected) && !isSelected)
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
                    ImGui.EndTable();
                }

                ImGui.TableNextColumn();
                if (ImGui.BeginTable("##timebaseT", 1, tableFlags))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Timebase (s)");
                    InputDoubleCombo("##timebase", ref timebase, StandardTimeBases);
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

                    // NB: the logarithmic curve is computed from the limit range, so the bounds are
                    // stretched to cover as much scale as a 32-bit float allows.
                    ImGui.Text(rangeInputLabel);
                    ImGui.BeginDisabled(!useFixedRange);
                    if (ImGui.DragFloat(
                        "##range",
                        ref rangeAmplitude,
                        vSpeed: 1e30f,
                        vMin: 0,
                        vMax: 1e35f,
                        format: "%.3g",
                        ImGuiSliderFlags.Logarithmic))
                    {
                        UpdateRangeLimits();
                    }
                    ImGui.EndDisabled();

                    ImGui.SameLine(0);
                    ImGui.Checkbox("##autofit", ref useFixedRange);
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

        void UpdateRangeLimits()
        {
            vMin = -rangeAmplitude / 2;
            vMax = rangeAmplitude / 2;
        }

        unsafe void WaveformPlot(Mat minBuffer, Mat maxBuffer)
        {
            minBuffer.GetRawData(out IntPtr minPtr, out int minStep, out Size minShape);
            maxBuffer.GetRawData(out IntPtr maxPtr, out int maxStep, out Size maxShape);
            timeRange.GetRawData(out IntPtr timeRangePtr, out int timeRangeStep, out Size _);
            ImPlot.PushStyleVar(ImPlotStyleVar.FitPadding, new Vector2(0, 0.1f));
            ImPlot.PushStyleVar(ImPlotStyleVar.Padding, new Vector2(0, 0));
            ImPlot.PushStyleVar(ImPlotStyleVar.BorderSize, 0);

            var tableFlags = ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY;
            var dataPlotFlags = ImPlotFlags.CanvasOnly | ImPlotFlags.NoFrame;
            var axesFlags = ImPlotAxisFlags.NoHighlight | ImPlotAxisFlags.NoInitialFit | ImPlotAxisFlags.AutoFit;
            var bareAxesFlags = axesFlags | ImPlotAxisFlags.NoDecorations;

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, -1)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, 10);
                ImGui.TableSetupColumn(string.Empty);
                ImGui.TableSetupScrollFreeze(0, 1);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var timeLabel = "Time";
                var cursorPosY = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(cursorPosY + TimeChannelHeight / 2);
                ImGui.Text(timeLabel);
                ImGui.TableNextColumn();
                if (ImPlot.BeginPlot(timeLabel, new(-1, TimeChannelHeight), dataPlotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, bareAxesFlags);
                    ImPlot.PlotInfLines(string.Empty, (float*)timeRangePtr, minShape.Width);
                    ImPlot.EndPlot();
                }

                for (int i = 0; i < minShape.Height; i++)
                {
                    var labelBuffer = stackalloc byte[32];
                    var channelLabel = new StrBuilder(labelBuffer, 32);
                    channelLabel.Reset();
                    channelLabel.Append("CH");
                    channelLabel.Append(i);
                    channelLabel.End();

                    var channelColor = ImPlot.GetColormapColor(i / colorGrouping);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    cursorPosY = ImGui.GetCursorPosY();
                    ImGui.SetCursorPosY(cursorPosY + channelHeight / 2 - 5);
                    ImGui.Text(channelLabel);
                    ImGui.TableNextColumn();
                    if (ImPlot.BeginPlot(channelLabel, new(-1, channelHeight), dataPlotFlags))
                    {
                        ImPlot.PushStyleColor(ImPlotCol.Line, channelColor);
                        ImPlot.SetupAxes(string.Empty, channelLabel, bareAxesFlags, bareAxesFlags);
                        if (useFixedRange)
                            ImPlot.SetupAxisLimits(ImAxis.Y1, vMin, vMax, ImPlotCond.Always);

                        var minLinePtr = (float*)((byte*)minPtr + i * minStep);
                        var maxLinePtr = (float*)((byte*)maxPtr + i * maxStep);
                        ImPlot.PlotShaded(string.Empty, (float*)timeRangePtr, minLinePtr, maxLinePtr, minShape.Width);
                        ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, minLinePtr, minShape.Width);
                        ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, maxLinePtr, maxShape.Width);
                        ImPlot.PopStyleColor();
                        ImPlot.EndPlot();
                    }
                }
                ImGui.EndTable();
            }

            ImPlot.PopStyleVar();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
