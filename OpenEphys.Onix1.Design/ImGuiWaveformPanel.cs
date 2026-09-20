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
    internal sealed class ImGuiWaveformPanel : IDisposable
    {
        const float TextBoxWidth = 80;
        const int MinChannelHeight = 10;
        const int TimeDivisions = 10;
        const int PaletteSteps = 16;

        enum ColorPalette
        {
            OpenEphysGui,
            Custom,
        }

        // visual constants
        const uint ColSweepCursor = ImGuiPalette.Yellow;
        const float SweepCursorWeight = 1;
        const uint ColGraticule = ImGuiPalette.Grey0x88;
        const float GraticuleWeight = 1;
        static readonly uint ColLabelHover = ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x25);

        static readonly double[] StandardTimeBases =
        {
            0.05, 0.1, 0.25, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0, 10.0, 20.0
        };

        static readonly double[] StandardRanges =
        {
            50, 100, 250, 500, 1000, 2500, 5000, 10000
        };

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
        bool? dragHidden;
        int dragAnchor;
        bool dragLeftAnchor;

        int expandedChannel = -1;
        float collapsedScroll;
        bool restoreScroll;

        int heightDragStart = -1;
        float heightDragMouseY;
        float heightDragPivot;
        float heightDragScroll;

        readonly string[] divisionLabels = new string[TimeDivisions + 1];
        double labeledTimebase = double.NaN;

        int channelHeight = 20;
        int maxSamplesPerChannel = 1920;
        double timebase = 2.0;
        double rangeAmplitude = 500;
        bool colorGroupingEnabled;
        int colorGrouping = 1;
        ColorPalette palette = ColorPalette.OpenEphysGui;
        Vector3 customColor = new(140 / 255f, 219 / 255f, 142 / 255f); // suggested default: a soft green

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

        void MenuWidgets()
        {
            if (!ImGui.BeginTable("##menu", columns: 7, ImGuiTableFlags.NoSavedSettings))
                return;

            ImGui.TableNextRow();
            ImGui.PushItemWidth(TextBoxWidth);

            var paused = minSnap is not null;

            if (BeginMenuColumn("Band"))
            {
                ImGui.BeginDisabled(paused); // NB: changing would cause buffer refresh and unpause
                BandCombo();
                ImGui.EndDisabled();
                ImGui.SameLine();
                var cmr = UseCommonMedianReference;
                if (ImGui.Checkbox("Apply CMR", ref cmr))
                    UseCommonMedianReference = cmr;
                EndMenuColumn();
            }

            if (BeginMenuColumn("Timebase (s)"))
            {
                ImGui.BeginDisabled(paused); // NB: changing would cause buffer refresh and unpause
                InputDoubleCombo("##timebase", ref timebase, StandardTimeBases);
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            if (BeginMenuColumn("Chan. Height"))
            {
                ImGui.DragInt(
                    "##channelHeight",
                    ref channelHeight,
                    vSpeed: 1,
                    MinChannelHeight,
                    int.MaxValue,
                    ImGuiSliderFlags.AlwaysClamp);
                EndMenuColumn();
            }

            var rangeLabel = string.IsNullOrEmpty(RangeLabel) ? "Range" : $"Range ({RangeLabel})";
            if (BeginMenuColumn(rangeLabel))
            {
                if (InputDoubleCombo("##range", ref rangeAmplitude, StandardRanges))
                    rangeAmplitude = Math.Max(1, rangeAmplitude);
                EndMenuColumn();
            }

            ImGui.TableNextColumn();
            PauseButton();

            if (BeginMenuColumn("Palette"))
            {
                PaletteCombo();
                if (palette == ColorPalette.Custom)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit3("##customColor", ref customColor, ImGuiColorEditFlags.NoInputs);
                }
                EndMenuColumn();
            }

            if (BeginMenuColumn("Color Groups"))
            {
                ImGui.Checkbox("##colorGroupingEnabled", ref colorGroupingEnabled);
                ImGui.SameLine();
                ImGui.BeginDisabled(!colorGroupingEnabled);
                if (ImGui.InputInt("##colorGrouping", ref colorGrouping))
                    colorGrouping = Math.Max(1, colorGrouping);
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            ImGui.PopItemWidth();
            ImGui.EndTable();
        }

        static bool BeginMenuColumn(string label)
        {
            ImGui.TableNextColumn();
            if (!ImGui.BeginTable(label, 1, ImGuiTableFlags.NoSavedSettings))
                return false;

            ImGui.TableNextColumn();
            ImGui.Text(label);
            return true;
        }

        static void EndMenuColumn() => ImGui.EndTable();

        static bool InputDoubleCombo(string label, ref double value, double[] comboItems)
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

        void PaletteCombo()
        {
            var preview = palette == ColorPalette.OpenEphysGui ? "Open Ephys GUI" : "Custom";
            if (ImGui.BeginCombo("##palette", preview))
            {
                if (ImGui.Selectable("Open Ephys GUI", palette == ColorPalette.OpenEphysGui))
                    palette = ColorPalette.OpenEphysGui;
                if (palette == ColorPalette.OpenEphysGui)
                    ImGui.SetItemDefaultFocus();

                if (ImGui.Selectable("Custom", palette == ColorPalette.Custom))
                    palette = ColorPalette.Custom;
                if (palette == ColorPalette.Custom)
                    ImGui.SetItemDefaultFocus();

                ImGui.EndCombo();
            }
        }

        /// <summary>
        /// The color for channel group <paramref name="group"/> under the selected <see
        /// cref="palette"/>.
        /// </summary>
        /// <remarks>
        /// In <see cref="ColorPalette.Custom"/>, groups are variants of the one picked hue, spread
        /// across <see cref="PaletteSteps"/> saturation/value combinations rather than assigned in
        /// order, so that consecutive groups land far apart in the ramp instead of a barely
        /// different neighboring shade — group 0 is always the picked color unmodified, which is
        /// also what a single-group plot gets.
        /// </remarks>
        Vector4 GroupColor(int group)
        {
            if (palette == ColorPalette.OpenEphysGui)
                return ImGui.ColorConvertU32ToFloat4(ImGuiPalette.OpenEphysGuiLfp[group % ImGuiPalette.OpenEphysGuiLfp.Length]);

            float hue = 0, saturation = 0, value = 0;
            ImGui.ColorConvertRGBtoHSV(customColor.X, customColor.Y, customColor.Z, ref hue, ref saturation, ref value);

            var step = group * 7 % PaletteSteps;
            var fraction = step / (float)(PaletteSteps - 1);
            saturation = Math.Min(1f, saturation + 0.4f * fraction);
            value *= 1f - 0.6f * fraction;

            float r = 0, g = 0, b = 0;
            ImGui.ColorConvertHSVtoRGB(hue, saturation, value, ref r, ref g, ref b);
            return new Vector4(r, g, b, 1f);
        }

        void PauseButton()
        {
            var paused = minSnap is not null;
            if (paused)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

            var buttonSize = new Vector2(TextBoxWidth, ImGui.GetFrameHeight() * 2);
            if (ImGui.Button("Pause", buttonSize) || ImGui.IsKeyPressed(ImGuiKey.Space))
                TogglePause();

            if (paused)
                ImGui.PopStyleColor();
        }

        void TogglePause()
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

        /// <summary>
        /// Lays out the label column and the plot, with every channel in one plot whose y axis is in
        /// channel units: channel <c>i</c> is centered at <c>-i</c> with +/- range / 2 mapped to
        /// +/- 0.5, so a trace that exceeds its range runs into the neighboring channels' rows
        /// instead of being clipped at a row edge.
        /// </summary>
        /// <remarks>
        /// The graticules are drawn into this window's draw list rather than as plot decorations so
        /// they stay put while the channels scroll; the plot background is cleared so they show
        /// through it.
        /// </remarks>
        /// <param name="minBuffer">Per-bin minima, one row per channel.</param>
        /// <param name="maxBuffer">Per-bin maxima, one row per channel.</param>
        void WaveformPlot(Mat minBuffer, Mat maxBuffer)
        {
            var rows = minBuffer.Rows;
            var labelDigits = DigitCount(rows - 1);

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

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, tableHeight)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, LabelColumnWidth(labelDigits));
                ImGui.TableSetupColumn(string.Empty);
                RestoreScrollIfPending();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var layout = LayoutRows(rows, plotTop, plotBottom, ImGui.GetCursorScreenPos().Y);
                var hovered = HoveredChannel(layout);
                ChannelLabels(layout, labelDigits, hovered);
                HandleChannelInput(hovered);
                HandleZoomInput(layout);

                // NB: with no padding, border or decorations the plot area is the item rect, so
                // its extent is known before the plot is drawn.
                ImGui.TableNextColumn();
                plotX = ImGui.GetCursorScreenPos().X;
                plotWidth = ImGui.GetContentRegionAvail().X;
                if (ImPlot.BeginPlot("##channels", new(plotWidth, layout.Height), plotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, axesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, timeSpan, ImPlotCond.Always);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, -(layout.LastRow - 1) - 0.5, -layout.FirstRow + 0.5, ImPlotCond.Always);
                    PlotTraces(minBuffer, maxBuffer, layout.FirstVisible, layout.LastVisible);
                    PlotSweepCursor();
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

        /// <summary>
        /// The rows the plot spans this frame and where they fall on screen: every channel at
        /// <c>channelHeight</c>, or the expanded channel alone filling the visible height.
        /// </summary>
        readonly struct RowLayout
        {
            public readonly int FirstRow;
            public readonly int LastRow;
            public readonly float RowHeight;
            public readonly float Top;
            public readonly float Bottom;
            public readonly float Origin;

            public RowLayout(int firstRow, int lastRow, float rowHeight, float top, float bottom, float origin)
            {
                FirstRow = firstRow;
                LastRow = lastRow;
                RowHeight = rowHeight;
                Top = top;
                Bottom = bottom;
                Origin = origin;
            }

            public float Height => (LastRow - FirstRow) * RowHeight;
            public int FirstVisible => Math.Max(FirstRow, ChannelAt(Top));
            public int LastVisible => Math.Min(LastRow, FirstRow + (int)Math.Ceiling((Bottom - Origin) / RowHeight));
            public float RowTop(int channel) => Origin + (channel - FirstRow) * RowHeight;
            public int ChannelAt(float y) => FirstRow + (int)Math.Floor((y - Origin) / RowHeight);
        }

        RowLayout LayoutRows(int rows, float top, float bottom, float origin)
        {
            if (expandedChannel >= rows)
                Collapse();

            return expandedChannel >= 0
                ? new RowLayout(expandedChannel, expandedChannel + 1, bottom - top, top, bottom, origin)
                : new RowLayout(0, rows, channelHeight, top, bottom, origin);
        }

        // NB: the channel under the mouse is found from its y anywhere across the label column and
        // the plot, so a trace can be acted on where it is looked at.
        static int HoveredChannel(in RowLayout layout)
        {
            var mouse = ImGui.GetMousePos();
            var left = ImGui.GetCursorScreenPos().X;
            var right = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X;
            if (ImGui.GetScrollMaxY() > 0)
                right -= ImGui.GetStyle().ScrollbarSize;

            if (!ImGui.IsWindowHovered() ||
                mouse.X < left || mouse.X >= right ||
                mouse.Y < layout.Top || mouse.Y >= layout.Bottom)
            {
                return -1;
            }

            var channel = layout.ChannelAt(mouse.Y);
            return channel >= layout.FirstRow && channel < layout.LastRow ? channel : -1;
        }

        // NB: channel numbers are zero-padded to the width of the largest so the label column, and
        // with it the plot, keeps one width whichever channels are scrolled into view.
        static float LabelColumnWidth(int labelDigits) =>
            ImGui.CalcTextSize("CH").X + labelDigits * ImGui.CalcTextSize("0").X;

        // NB: the scroll clamps to zero while one band fills the table, so the position from
        // before the expand is put back on collapse.
        void RestoreScrollIfPending()
        {
            if (restoreScroll)
            {
                ImGui.SetScrollY(collapsedScroll);
                restoreScroll = false;
            }
        }

        unsafe void ChannelLabels(in RowLayout layout, int labelDigits, int hovered)
        {
            var labelBuffer = stackalloc byte[32];
            var label = new StrBuilder(labelBuffer, 32);
            var labelTop = ImGui.GetCursorPosY();
            var textOffset = (layout.RowHeight - ImGui.GetTextLineHeight()) / 2;
            var left = ImGui.GetCursorScreenPos().X;
            var right = left + ImGui.GetContentRegionAvail().X;
            var draw = ImGui.GetWindowDrawList();

            for (int i = layout.FirstVisible; i < layout.LastVisible; i++)
            {
                label.Reset();
                label.Append("CH");
                for (var n = DigitCount(i); n < labelDigits; n++)
                    label.Append('0');
                label.Append(i);
                label.End();

                if (i == hovered)
                {
                    var rowTop = layout.RowTop(i);
                    draw.AddRectFilled(new Vector2(left, rowTop), new Vector2(right, rowTop + layout.RowHeight), ColLabelHover);
                }

                var hidden = channelHidden[i];
                if (hidden) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
                ImGui.SetCursorPosY(labelTop + (i - layout.FirstRow) * layout.RowHeight + textOffset);
                ImGui.Text(label);
                if (hidden) ImGui.PopStyleColor();
            }
        }

        static int DigitCount(int value)
        {
            var digits = 1;
            for (; value >= 10; value /= 10)
                digits++;
            return digits;
        }

        void HandleChannelInput(int channel)
        {
            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                dragHidden = null;
            if (channel < 0)
                return;

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (expandedChannel >= 0) Collapse();
                else Expand(channel);
                return;
            }

            if (expandedChannel >= 0)
                return;

            var rows = channelHidden.Length;
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.GetIO().KeyShift)
            {
                Array.Copy(channelHidden, dragOriginal, rows);
                dragHidden = !channelHidden[channel];
                dragAnchor = channel;
                dragLeftAnchor = false;
            }

            // NB: a drag that comes back to the pressed channel undoes it too, unlike a click that
            // never left it.
            if (dragHidden is bool paint)
            {
                dragLeftAnchor |= channel != dragAnchor;
                var lo = Math.Min(dragAnchor, channel);
                var hi = dragLeftAnchor && channel == dragAnchor ? lo - 1 : Math.Max(dragAnchor, channel);
                for (int c = 0; c < rows; c++)
                    channelHidden[c] = c >= lo && c <= hi ? paint : dragOriginal[c];
            }
        }

        // NB: ImGui keeps Ctrl+wheel for its own font zoom, which is off, and turns Shift+wheel into
        // horizontal scroll, which the table cannot do, so neither moves anything and both are free
        // to use here. Alt+wheel is plain scroll to ImGui and would scroll the channels.
        void HandleZoomInput(in RowLayout layout)
        {
            var io = ImGui.GetIO();
            var mouse = ImGui.GetMousePos();

            if (heightDragStart >= 0 && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
                heightDragStart = -1;

            if (expandedChannel >= 0 || !ImGui.IsWindowHovered() && heightDragStart < 0)
                return;

            if (io.MouseWheel != 0 && io.KeyCtrl && heightDragStart < 0)
            {
                var step = io.MouseWheel > 0 ? 2 : -2;
                if (channelHeight > 100)
                    step *= 3;
                var pivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                SetChannelHeight(channelHeight + step, channelHeight, pivot, ImGui.GetScrollY(), layout);
            }
            else if (io.MouseWheel != 0 && io.KeyShift)
            {
                StepRange(io.MouseWheel > 0 ? 1 : -1);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && io.KeyCtrl && ImGui.IsWindowHovered())
            {
                heightDragStart = channelHeight;
                heightDragMouseY = mouse.Y;
                heightDragPivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                heightDragScroll = ImGui.GetScrollY();
            }

            if (heightDragStart >= 0)
            {
                var delta = (int)Math.Round(-0.2f * (mouse.Y - heightDragMouseY));
                if (heightDragStart > 100)
                    delta *= 3;
                SetChannelHeight(heightDragStart + delta, heightDragStart, heightDragPivot, heightDragScroll, layout);
            }
        }

        // NB: the channel that was under the pointer when the gesture began is kept at the same
        // screen position by moving the scroll with the height change.
        void SetChannelHeight(int height, int baseHeight, float pivot, float baseScroll, in RowLayout layout)
        {
            height = Math.Max(MinChannelHeight, Math.Min((int)(layout.Bottom - layout.Top), height));
            if (height == channelHeight)
                return;

            channelHeight = height;
            ImGui.SetScrollY(baseScroll + pivot * (height - baseHeight));
        }

        void StepRange(int direction)
        {
            var i = Array.BinarySearch(StandardRanges, rangeAmplitude);
            if (i < 0)
            {
                i = ~i;
                if (direction < 0)
                    i--;
            }
            else
            {
                i += direction;
            }

            rangeAmplitude = StandardRanges[Math.Max(0, Math.Min(StandardRanges.Length - 1, i))];
        }

        void Expand(int channel)
        {
            collapsedScroll = ImGui.GetScrollY();
            expandedChannel = channel;
        }

        void Collapse()
        {
            expandedChannel = -1;
            restoreScroll = true;
        }

        /// <summary>
        /// Transforms the decimated buffers into channel units as <c>data / range + rowOffsets</c>,
        /// where <c>rowOffsets</c> is the constant <c>-i</c> term, one row per channel, and plots
        /// the visible rows.
        /// </summary>
        unsafe void PlotTraces(Mat minBuffer, Mat maxBuffer, int first, int last)
        {
            CV.AddWeighted(minBuffer, 1 / rangeAmplitude, rowOffsets, 1, 0, displayMin);
            CV.AddWeighted(maxBuffer, 1 / rangeAmplitude, rowOffsets, 1, 0, displayMax);
            displayMin.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            displayMax.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            timeRange.GetRawData(out IntPtr timeRangePtr, out int _, out Size _);
            var columns = shape.Width;

            for (int i = first; i < last; i++)
            {
                if (channelHidden[i] && i != expandedChannel)
                    continue;

                var minLinePtr = (float*)((byte*)minPtr + i * minStep);
                var maxLinePtr = (float*)((byte*)maxPtr + i * maxStep);
                var group = colorGroupingEnabled
                    ? i / colorGrouping
                    : palette == ColorPalette.OpenEphysGui ? i : 0;
                var channelColor = GroupColor(group);
                ImPlot.PushStyleColor(ImPlotCol.Line, channelColor);
                ImPlot.PushStyleColor(ImPlotCol.Fill, channelColor);
                ImPlot.PlotShaded(string.Empty, (float*)timeRangePtr, minLinePtr, maxLinePtr, columns);
                ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, minLinePtr, columns);
                ImPlot.PlotLine(string.Empty, (float*)timeRangePtr, maxLinePtr, columns);
                ImPlot.PopStyleColor(2);
            }
        }

        unsafe void PlotSweepCursor()
        {
            timeRange.GetRawData(out IntPtr timeRangePtr, out int _, out Size _);
            var sweepHead = minSnap is not null ? sweepHeadSnap : decimatorMin.Cursor;
            double sweepTime = ((float*)timeRangePtr)[sweepHead];
            ImPlot.PushStyleColor(ImPlotCol.Line, ImGui.ColorConvertU32ToFloat4(ColSweepCursor));
            ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, SweepCursorWeight);
            ImPlot.PlotInfLines(string.Empty, &sweepTime, 1);
            ImPlot.PopStyleVar();
            ImPlot.PopStyleColor();
        }

        // NB: AddRectFilled on floored coordinates, here and in DrawGraticules, because AddRect and
        // AddLine offset by half a pixel and anti-alias: a 1 px AddRect frame lands a pixel inside
        // its extent, and thicker lines get a grey halo.
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
    }
}
