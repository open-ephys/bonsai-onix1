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
        const int TimeDivisions = 10;

        // visual constants
        const uint ColSweepCursor = ImGuiPalette.Yellow;
        const float SweepCursorWeight = 1;
        const uint ColGraticule = ImGuiPalette.Grey0x88;
        const float GraticuleWeight = 1;

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
        int sweepHeadSnap;

        Mat rowOffsets;
        Mat displayMin;
        Mat displayMax;
        double timeSpan;
        readonly string[] divisionLabels = new string[TimeDivisions + 1];
        double labeledTimebase = double.NaN;
        bool[] channelHidden = Array.Empty<bool>();
        bool[] paintOriginal = Array.Empty<bool>();
        bool? paintHidden;
        int paintAnchor;
        bool paintLeftAnchor;

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
                if (channelHidden.Length != data.Rows)
                {
                    channelHidden = new bool[data.Rows];
                    paintOriginal = new bool[data.Rows];
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
                        sweepHeadSnap = decimatorMin.Cursor;
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
        /// centered at <c>-i</c> with +/- range / 2 mapped to +/- 0.5, so a trace that exceeds its range
        /// runs into the neighboring channels' bands instead of being clipped at a row edge.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The decimated buffers are transformed into those units each frame as
        /// <c>data / range + rowOffsets</c>, where <c>rowOffsets</c> is the constant <c>-i</c> term, one
        /// row per channel.
        /// </para>
        /// <para>
        /// The graticules are drawn into this window's draw list rather than as plot decorations so
        /// they stay put while the channels scroll; the plot background is cleared so they show
        /// through it.
        /// </para>
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
            ImPlot.PushStyleColor(ImPlotCol.Bg, Vector4.Zero);

            var plotFlags = ImPlotFlags.CanvasOnly | ImPlotFlags.NoFrame | ImPlotFlags.NoInputs;
            var axesFlags = ImPlotAxisFlags.NoHighlight | ImPlotAxisFlags.NoDecorations;
            var tableFlags = ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY;
            var tableHeight = -(ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y);
            var plotTop = ImGui.GetCursorScreenPos().Y;
            var plotBottom = plotTop + ImGui.GetContentRegionAvail().Y + tableHeight;
            var plotX = 0f;
            var plotWidth = 0f;

            // NB: channel numbers are zero-padded to the width of the largest so the label column,
            // and with it the plot, keeps one width whichever channels are scrolled into view.
            var labelDigits = 1;
            for (var n = rows - 1; n >= 10; n /= 10)
                labelDigits++;
            var labelWidth = ImGui.CalcTextSize("CH").X + labelDigits * ImGui.CalcTextSize("0").X;

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, tableHeight)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, labelWidth);
                ImGui.TableSetupColumn(string.Empty);

                // NB: the row is one plot rows * channelHeight tall, so channel i's band starts
                // i * channelHeight below the row top.
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var rowTop = ImGui.GetCursorScreenPos().Y;
                var firstVisible = Math.Max(0, (int)Math.Floor((plotTop - rowTop) / channelHeight));
                var lastVisible = Math.Min(rows, (int)Math.Ceiling((plotBottom - rowTop) / channelHeight));

                // NB: the pressed selectable holds ImGui's active id, so hover on the others has to
                // be allowed past it for a drag to reach them.
                var labelBuffer = stackalloc byte[32];
                var channelLabel = new StrBuilder(labelBuffer, 32);
                var labelTop = ImGui.GetCursorPosY();
                var labelSize = new Vector2(0, channelHeight);
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    paintHidden = null;
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 0.5f));
                for (int i = firstVisible; i < lastVisible; i++)
                {
                    channelLabel.Reset();
                    channelLabel.Append("CH");
                    var digits = 1;
                    for (var n = i; n >= 10; n /= 10)
                        digits++;
                    for (; digits < labelDigits; digits++)
                        channelLabel.Append('0');
                    channelLabel.Append(i);
                    channelLabel.End();

                    var hidden = channelHidden[i];
                    if (hidden) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
                    ImGui.SetCursorPosY(labelTop + i * channelHeight);
                    ImGui.Selectable(channelLabel, false, ImGuiSelectableFlags.None, labelSize);
                    if (hidden) ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem))
                    {
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            Array.Copy(channelHidden, paintOriginal, rows);
                            paintHidden = !hidden;
                            paintAnchor = i;
                            paintLeftAnchor = false;
                        }

                        // NB: a drag that comes back to the pressed channel undoes it too, unlike
                        // a click that never left it.
                        if (paintHidden is bool paint)
                        {
                            paintLeftAnchor |= i != paintAnchor;
                            var lo = Math.Min(paintAnchor, i);
                            var hi = paintLeftAnchor && i == paintAnchor ? lo - 1 : Math.Max(paintAnchor, i);
                            for (int c = 0; c < rows; c++)
                                channelHidden[c] = c >= lo && c <= hi ? paint : paintOriginal[c];
                        }
                    }
                }
                ImGui.PopStyleVar();

                // NB: with no padding, border or decorations the plot area is the item rect, so
                // its extent is known before the plot is drawn.
                ImGui.TableNextColumn();
                plotX = ImGui.GetCursorScreenPos().X;
                plotWidth = ImGui.GetContentRegionAvail().X;
                if (ImPlot.BeginPlot("##channels", new(plotWidth, rows * channelHeight), plotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, axesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, timeSpan, ImPlotCond.Always);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, -(rows - 1) - 0.5, 0.5, ImPlotCond.Always);
                    for (int i = firstVisible; i < lastVisible; i++)
                    {
                        if (channelHidden[i])
                            continue;

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

                    var sweepHead = minSnap is not null ? sweepHeadSnap : decimatorMin.Cursor;
                    double sweepTime = ((float*)timeRangePtr)[sweepHead];
                    ImPlot.PushStyleColor(ImPlotCol.Line, ImGui.ColorConvertU32ToFloat4(ColSweepCursor));
                    ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, SweepCursorWeight);
                    ImPlot.PlotInfLines(string.Empty, &sweepTime, 1);
                    ImPlot.PopStyleVar();
                    ImPlot.PopStyleColor();
                    ImPlot.EndPlot();

                    // NB: drawn after the plot so it lies over the traces
                    DrawFrame(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);
                }
                ImGui.EndTable();
            }

            ImPlot.PopStyleColor();
            ImPlot.PopStyleVar(3);

            if (plotWidth > 0)
                DrawGraticules(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);
        }

        // NB: the frame and graticules are filled rects on whole pixels. AddLine and AddRect
        // offset their coordinates by half a pixel and anti-alias, which puts a 1 px frame one
        // pixel inside its extent and gives thicker lines a grey halo.
        static void DrawFrame(ImDrawListPtr draw, float left, float width, float top, float bottom)
        {
            var l = MathF.Floor(left);
            var r = MathF.Floor(left + width);
            var t = MathF.Floor(top);
            var b = MathF.Floor(bottom);
            var w = GraticuleWeight;
            draw.AddRectFilled(new Vector2(l, t), new Vector2(r, t + w), ColGraticule);
            draw.AddRectFilled(new Vector2(l, b - w), new Vector2(r, b), ColGraticule);
            draw.AddRectFilled(new Vector2(l, t), new Vector2(l + w, b), ColGraticule);
            draw.AddRectFilled(new Vector2(r - w, t), new Vector2(r, b), ColGraticule);
        }

        void DrawGraticules(ImDrawListPtr draw, float left, float width, float top, float bottom)
        {
            var t = MathF.Floor(top) + GraticuleWeight;
            var b = MathF.Floor(bottom) - GraticuleWeight;
            var textColor = ImGui.GetColorU32(ImGuiCol.Text);
            var labelY = bottom + ImGui.GetStyle().ItemSpacing.Y;

            if (labeledTimebase != timebase)
            {
                for (int d = 0; d <= TimeDivisions; d++)
                    divisionLabels[d] = $"{d * timebase / TimeDivisions:g} s";
                labeledTimebase = timebase;
            }

            for (int d = 0; d <= TimeDivisions; d++)
            {
                var x = left + d * width / TimeDivisions;
                if (d > 0 && d < TimeDivisions)
                {
                    var l = MathF.Floor(x);
                    draw.AddRectFilled(new Vector2(l, t), new Vector2(l + GraticuleWeight, b), ColGraticule);
                }

                var label = divisionLabels[d];
                draw.AddText(new Vector2(x - ImGui.CalcTextSize(label).X / 2, labelY), textColor, label);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
