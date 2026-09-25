using System;
using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiLfpViewerPanel
    {
        const int TimeDivisions = 10;

        // visual constants
        const uint ColSweepCursor = ImGuiPalette.Yellow;
        const float SweepCursorWeight = 1;
        const uint ColGraticule = ImGuiPalette.Grey0x88;
        const float GraticuleWeight = 1;

        /// <summary>
        /// Color and weight of the frame around the plot, so that a pane set beside it can be outlined
        /// to match rather than with ImGui's fainter default border.
        /// </summary>
        public const uint FrameColor = ColGraticule;
        public const float FrameWeight = GraticuleWeight;
        static readonly uint ColLabelHover = ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x25);

        readonly string[] divisionLabels = new string[TimeDivisions + 1];
        double labeledTimebase = double.NaN;

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
        /// <param name="waveformMin">Per-bin minima, one row per channel.</param>
        /// <param name="waveformMax">Per-bin maxima, one row per channel.</param>
        void WaveformPlot(Mat waveformMin, Mat waveformMax)
        {
            var rows = waveformMin.Rows;
            var labelDigits = DigitCount(rows - 1);

            ImPlot.PushStyleVar(ImPlotStyleVar.Padding, new Vector2(0, 0));
            ImPlot.PushStyleVar(ImPlotStyleVar.BorderSize, 0);
            ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
            ImPlot.PushStyleColor(ImPlotCol.Bg, Vector4.Zero);

            var plotFlags = ImPlotFlags.CanvasOnly | ImPlotFlags.NoFrame | ImPlotFlags.NoInputs;
            var axesFlags = ImPlotAxisFlags.NoHighlight | ImPlotAxisFlags.NoDecorations;
            var tableFlags = ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY;

            // NB: the time labels take a band above the plot, and the drag that pans a paused view
            // goes with them, leaving the bottom edge to whatever the owner puts there. They sit at
            // the bottom of the band, next to the plot they label, so a taller band leaves room above
            // them rather than pushing them away from it.
            var textHeight = ImGui.GetTextLineHeight();
            var headerHeight = Math.Max(textHeight, HeaderHeight);
            var labelY = ImGui.GetCursorScreenPos().Y + headerHeight - textHeight;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + headerHeight);

            var plotTop = ImGui.GetCursorScreenPos().Y;
            var tableHeight = ImGui.GetContentRegionAvail().Y;
            var plotBottom = plotTop + tableHeight;
            var plotX = 0f;
            var plotWidth = 0f;

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, tableHeight)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, LabelColumnWidth(labelDigits));
                ImGui.TableSetupColumn(string.Empty);
                RestoreScrollIfPending();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var layout = LayoutRows(rows, plotTop, plotBottom, ImGui.GetCursorScreenPos().Y,
                    ImGui.GetContentRegionAvail().Y);
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
                    PlotTraces(waveformMin, waveformMax, layout.FirstVisible, layout.LastVisible);
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
            {
                DrawGraticules(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom, labelY);
                HandlePanInput(plotX, plotWidth, labelY);
            }
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

        RowLayout LayoutRows(int rows, float top, float bottom, float origin, float available)
        {
            if (expandedChannel >= rows)
                Collapse();

            return expandedChannel >= 0
                ? new RowLayout(expandedChannel, expandedChannel + 1, available, top, bottom, origin)
                : new RowLayout(0, rows, channelHeight, top, bottom, origin);
        }

        // NB: channel numbers are zero-padded to the width of the largest so the label column, and
        // with it the plot, keeps one width whichever channels are scrolled into view.
        static float LabelColumnWidth(int labelDigits) =>
            ImGui.CalcTextSize("CH").X + labelDigits * ImGui.CalcTextSize("0").X;

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

        /// <summary>
        /// Transforms the decimated buffers into channel units as <c>data / range + rowOffsets</c>,
        /// where <c>rowOffsets</c> is the constant <c>-i</c> term, one row per channel, and plots
        /// the visible rows.
        /// </summary>
        unsafe void PlotTraces(Mat waveformMin, Mat waveformMax, int first, int last)
        {
            CV.AddWeighted(waveformMin, 1 / rangeAmplitude, rowOffsets, 1, 0, scaledWaveformMin);
            CV.AddWeighted(waveformMax, 1 / rangeAmplitude, rowOffsets, 1, 0, scaledWaveformMax);
            scaledWaveformMin.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            scaledWaveformMax.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
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

        /// <summary>
        /// Marks the column being written, or the column the pause instant fell at, which slides right
        /// and off the plot as a paused view is moved back through history.
        /// </summary>
        unsafe void PlotSweepCursor()
        {
            var sweepHead = Panned ? PannedCursorColumn : Paused ? pausedCursor : waveformMinDecimator.Cursor;
            if (sweepHead < 0 || sweepHead >= waveformMinDecimator.Buffer.Cols)
                return;

            timeRange.GetRawData(out IntPtr timeRangePtr, out int _, out Size _);
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

        void DrawGraticules(ImDrawListPtr draw, float left, float width, float top, float bottom, float labelY)
        {
            var t = MathF.Floor(top) + GraticuleWeight;
            var b = MathF.Floor(bottom) - GraticuleWeight;
            var textColor = ImGui.GetColorU32(ImGuiCol.Text);

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
