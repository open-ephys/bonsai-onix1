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
        const int AmplitudeDivisions = 10;
        const uint ColDivision = ImGuiPalette.Grey0x55;
        const float DivisionDotGap = 3;
        const float HoverLineWeight = 3;
        const uint ColHoveredRow = ImGuiPalette.Grey0x99;
        static readonly uint ColSelectedRow = ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x20);

        static readonly uint ColFrozenTail = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0x80);

        /// <summary>
        /// Color and weight of the plot's frame, for the control strip's divider to match.
        /// </summary>
        public const uint FrameColor = ColGraticule;
        public const float FrameWeight = GraticuleWeight;
        const float HoverFillAlpha = 0.65f;

        float plotLeft;
        float plotSpan;
        float visibleHeight;

        readonly TraceRaster traces = new();

        /// <summary>
        /// Where <paramref name="x"/> falls across the plot, from zero at its left edge to one at its
        /// right.
        /// </summary>
        float PlotFraction(float x) =>
            plotSpan > 0 ? Math.Max(0f, Math.Min(1f, (x - plotLeft) / plotSpan)) : 0.5f;

        /// <summary>
        /// Lays out the label column and the plot, with every channel in one plot whose y axis is in
        /// channel units: channel <c>i</c> is centered at <c>-i</c> with +/- range / 2 mapped to
        /// +/- 0.5, so a trace that exceeds its range runs into the neighboring channels' rows
        /// instead of being clipped at a row edge.
        /// </summary>
        /// <remarks>
        /// The graticules go in this window's draw list rather than the plot's, so they stay put while the
        /// channels scroll. They sit under the traces, showing through the plot's cleared background.
        /// </remarks>
        /// <param name="envelope">Per-column minima and maxima, one row per channel.</param>
        void WaveformPlot(Envelope envelope)
        {
            var rows = envelope.Rows;
            var labelDigits = DigitCount(rows - 1);

            ImPlot.PushStyleVar(ImPlotStyleVar.Padding, new Vector2(0, 0));
            ImPlot.PushStyleVar(ImPlotStyleVar.BorderSize, 0);
            ImPlot.PushStyleColor(ImPlotCol.Bg, Vector4.Zero);

            var plotFlags = ImPlotFlags.CanvasOnly | ImPlotFlags.NoFrame | ImPlotFlags.NoInputs;
            var axesFlags = ImPlotAxisFlags.NoHighlight | ImPlotAxisFlags.NoDecorations;
            var tableFlags = ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY;

            // NB: the axis labels get a button-height row, so the plot's frame starts one button-height down.
            var headerHeight = ImGui.GetFrameHeight();
            var labelY = ImGui.GetCursorScreenPos().Y + ImGui.GetStyle().FramePadding.Y;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + headerHeight);

            var scrollHeight = ImGui.GetStyle().ScrollbarSize;
            var scrollGap = ImGui.GetStyle().ItemSpacing.Y / 2;
            var plotTop = ImGui.GetCursorScreenPos().Y;
            var tableHeight = ImGui.GetContentRegionAvail().Y - scrollHeight - scrollGap;
            var plotBottom = plotTop + tableHeight;
            var plotX = 0f;
            var plotWidth = 0f;
            var frame = default(PlotFrame);

            if (ImGui.BeginTable("##table", 2, tableFlags, new Vector2(-1, tableHeight)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, LabelColumnWidth(labelDigits));
                ImGui.TableSetupColumn(string.Empty);
                RestoreScrollIfPending();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                // NB: measured inside the cell, which leaves out the row's padding, and less the scroll, by which
                // the cell's top has moved up, so that rows fitted to it fill the table without scrolling.
                visibleHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetScrollY();
                var layout = LayoutRows(rows, plotTop, plotBottom, ImGui.GetCursorScreenPos().Y,
                    ImGui.GetContentRegionAvail().Y);
                var hovered = HoveredChannel(layout);
                if (selectedChannel >= rows)
                    selectedChannel = -1;
                if (selectOnShow && selectedChannel < 0)
                    selectedChannel = FirstShownInView(layout);
                selectOnShow = false;
                ChannelLabels(layout, labelDigits, hovered);
                HandleChannelInput(hovered);
                HandleZoomInput(layout);

                // NB: with no padding, border or decorations the plot area is the item rect, so
                // its extent is known before the plot is drawn.
                ImGui.TableNextColumn();
                plotX = ImGui.GetCursorScreenPos().X;
                plotWidth = ImGui.GetContentRegionAvail().X;
                plotLeft = plotX;
                plotSpan = plotWidth;
                frame = new PlotFrame(
                    envelope, layout, plotX, plotWidth, plotTop, plotBottom,
                    window, sampleRate, Paused, CursorPosition, timebase, pausedTimebase,
                    rangeAmplitude, Unit, channelHidden, expandedChannel, selectedChannel, ShowHeatmap, ColorThreshold);
                if (ImPlot.BeginPlot("##channels", new(plotWidth, layout.Height), plotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, axesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, decimator.Sweep.Cols, ImPlotCond.Always);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, -(layout.LastRow - 1) - 0.5, -layout.FirstRow + 0.5, ImPlotCond.Always);
                    DrawRowBand(ImGui.GetWindowDrawList(), frame);
                    traces.Draw(frame, Colors.Packed(channelHidden.Length),
                        Paused ? -1 : decimator.Cursor, history?.Count ?? 0);
                    if (expandedChannel >= 0)
                        PlotChannelLines(envelope, expandedChannel, 1);
                    else if (hovered >= 0 && !channelHidden[hovered] && !ShowHeatmap)
                        PlotChannelLines(envelope, hovered, HoverLineWeight);
                    PlotSweepCursor();
                    ImPlot.EndPlot();

                    // NB: both drawn after the plot, and from inside the table,
                    // so they lie over the traces
                    ShadeFrozenTail(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);

                    // NB: a heatmap row has no line to thicken and covers the band, so the hovered and selected rows
                    // are marked from outside, which also ties a label to its row when rows are far shorter than it.
                    if (ShowHeatmap && expandedChannel < 0 && hovered >= 0 && !channelHidden[hovered])
                        BracketRow(ImGui.GetWindowDrawList(), frame, hovered, ColHoveredRow);
                    if (ShowHeatmap && expandedChannel < 0 && selectedChannel >= 0)
                        BracketRow(ImGui.GetWindowDrawList(), frame, selectedChannel, WaveformCursors.Color);
                    cursors.Draw(ImGui.GetWindowDrawList(), frame);
                    SelectClicked(frame, hovered);
                    DrawFrame(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);
                    DrawReadout(ImGui.GetWindowDrawList(), frame, hovered);
                    AmplitudeLabels(ImGui.GetWindowDrawList(), plotX, plotTop, plotBottom);
                }
                ImGui.EndTable();
            }

            ImPlot.PopStyleColor();
            ImPlot.PopStyleVar(2);

            if (plotWidth > 0)
            {
                DrawGraticules(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom, labelY);
                cursors.DrawTimeLabels(ImGui.GetWindowDrawList(), frame, labelY);
                HandlePanInput(plotX, plotWidth, labelY);
                TimeScrollBar(plotX, plotWidth, plotBottom + scrollGap, scrollHeight);

                // NB: last, so that its window is drawn over the table's and takes the pointer from it.
                if (cursors.DrawTable(frame) is long distance)
                    Pan(distance);
            }
        }

        /// <summary>
        /// Veils the part of a paused plot that is still showing the tail of the previous sweep.
        /// </summary>
        /// <remarks>
        /// Everything right of the cursor is older than everything left of it, and appears again elsewhere in
        /// the history. The shading marks it as stale.
        /// </remarks>
        void ShadeFrozenTail(ImDrawListPtr draw, float left, float width, float top, float bottom)
        {
            if (!Paused)
                return;

            var split = Math.Max(window.Start, Math.Min(window.End, CursorPosition));
            if (split >= window.End)
                return;

            var x = MathF.Floor(left + (float)(width * window.FractionOf(split)));
            draw.AddRectFilled(
                new Vector2(x, MathF.Floor(top) + GraticuleWeight),
                new Vector2(MathF.Floor(left + width) - GraticuleWeight, MathF.Floor(bottom) - GraticuleWeight),
                ColFrozenTail);
        }

        /// <summary>
        /// The time the axis labels give <paramref name="position"/>: live, from the start of the sweep; paused,
        /// before the moment of pausing, with the frozen tail a whole paused timebase older.
        /// </summary>
        internal static double SecondsAt(in PlotFrame frame, double position)
        {
            var origin = frame.Paused ? frame.PauseOrigin : 0;
            var seconds = (position - origin) / frame.Window.Span * frame.Timebase;
            return frame.Paused && position > origin ? seconds - frame.PausedTimebase : seconds;
        }

        /// <summary>
        /// Shows the channel, time and amplitude under the pointer in the plot's lower right corner.
        /// </summary>
        /// <remarks>
        /// The time reads as the time axis labels do, and the amplitude from the center of the channel's row.
        /// </remarks>
        void DrawReadout(ImDrawListPtr draw, in PlotFrame frame, int hovered)
        {
            var mouse = ImGui.GetMousePos();
            if (hovered < 0 || mouse.X < frame.Left || mouse.X >= frame.Left + frame.Width)
                return;

            var seconds = SecondsAt(frame, frame.Window.Start + PlotFraction(mouse.X) * frame.Window.Span);
            var center = frame.Layout.RowTop(hovered) + frame.Layout.RowHeight / 2;
            var amplitude = (center - mouse.Y) / frame.Layout.RowHeight * frame.Range;

            // NB: an expanded channel is named in the plot's top left already.
            var position = $"{seconds:0.0000} s   {amplitude:0.0} {frame.Unit}";
            var text = frame.Expanded >= 0 ? position : $"Ch {hovered}   {position}";
            var corner = new Vector2(frame.Left + frame.Width, frame.Bottom) - ImGui.CalcTextSize(text) -
                2 * ImGui.GetStyle().FramePadding;
            PlotText.Framed(draw, corner, text);
        }

        /// <summary>
        /// A horizontal bar below the plot showing where the window sits on the axis, which can be dragged
        /// to move it.
        /// </summary>
        /// <remarks>
        /// Drawn rather than taken from ImGui, which scrolls its own containers and has no notion of this
        /// axis. It is styled from the scrollbar colors so that it reads as one alongside the channel
        /// scrollbar it sits under.
        /// </remarks>
        void TimeScrollBar(float left, float width, float top, float height)
        {
            var axisFirst = Paused && history is not null ? AxisFirst : window.Start;
            var axisLast = Paused && history is not null ? AxisLast : window.End;
            var travel = Math.Max(0, axisLast - axisFirst - window.Span);

            // NB: the thumb sits inside the track by the same margin ImGui gives its own grabs, which is
            // most of what makes a scrollbar read as one rather than as a filled bar.
            var inset = Math.Max(0f, Math.Min(3f, MathF.Floor((height - 2f) / 2f)));
            var trackLeft = left + inset;
            var trackWidth = width - 2 * inset;

            // NB: the thumb keeps a minimum width so that a window which is a thousandth of the axis is
            // still something to aim at, which costs a little of the travel it stands for.
            var reach = (double)(travel + window.Span);
            var thumbWidth = Math.Max(ImGui.GetStyle().GrabMinSize, (float)(trackWidth * window.Span / reach));
            var slack = trackWidth - thumbWidth;
            var thumbX = travel > 0
                ? trackLeft + slack * (float)((window.Start - axisFirst) / (double)travel)
                : trackLeft;

            ImGui.SetCursorScreenPos(new Vector2(left, top));
            ImGui.InvisibleButton("##timescroll", new Vector2(width, height));

            if (ImGui.IsItemActive() && travel > 0 && slack > 0)
                Pan(-(long)(ImGui.GetIO().MouseDelta.X * (travel / slack)));

            var rounding = ImGui.GetStyle().ScrollbarRounding;
            var grab = ImGui.IsItemActive() ? ImGuiCol.ScrollbarGrabActive
                : ImGui.IsItemHovered() ? ImGuiCol.ScrollbarGrabHovered
                : ImGuiCol.ScrollbarGrab;

            var draw = ImGui.GetWindowDrawList();
            draw.AddRectFilled(new Vector2(left, top), new Vector2(left + width, top + height),
                ImGui.GetColorU32(ImGuiCol.ScrollbarBg), rounding);
            draw.AddRectFilled(
                new Vector2(thumbX, top + inset), new Vector2(thumbX + thumbWidth, top + height - inset),
                ImGui.GetColorU32(grab), rounding);
        }

        /// <summary>
        /// The rows the plot spans this frame and where they fall on screen: every channel at
        /// <see cref="ChannelHeight"/>, or the expanded channel alone filling the visible height.
        /// </summary>
        internal readonly struct RowLayout
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
                : new RowLayout(0, rows, ChannelHeight, top, bottom, origin);
        }

        static float LabelColumnWidth(int labelDigits) => labelDigits * ImGui.CalcTextSize("0").X;

        static readonly int[] LabelSteps = { 1, 2, 5, 10 };

        unsafe void ChannelLabels(in RowLayout layout, int labelDigits, int hovered)
        {
            var labelBuffer = stackalloc byte[32];
            var label = new StrBuilder(labelBuffer, 32);
            var labelTop = ImGui.GetCursorPosY();
            var textOffset = (layout.RowHeight - ImGui.GetTextLineHeight()) / 2;
            var left = ImGui.GetCursorScreenPos().X;
            var right = left + ImGui.GetContentRegionAvail().X;
            var draw = ImGui.GetWindowDrawList();

            // NB: rows shorter than the text get every 2nd, 5th or 10th label, the smallest step whose labels
            // clear each other. The hovered channel and the one the cursors read are always labeled, and the
            // stepped labels they would overlap give way to them.
            var lineHeight = ImGui.GetTextLineHeight();
            var rowHeight = layout.RowHeight;
            var step = LabelSteps[LabelSteps.Length - 1];
            foreach (var candidate in LabelSteps)
            {
                if (candidate * rowHeight >= lineHeight)
                {
                    step = candidate;
                    break;
                }
            }

            var selected = selectedChannel;
            bool Crowds(int channel, int priority) =>
                priority >= 0 && priority != channel && Math.Abs(channel - priority) * rowHeight < lineHeight;

            // NB: an expanded channel is named inside the plot instead, by AmplitudeLabels.
            for (int i = layout.FirstVisible; i < layout.LastVisible && expandedChannel < 0; i++)
            {
                label.Reset();
                for (var n = DigitCount(i); n < labelDigits; n++)
                    label.Append('0');
                label.Append(i);
                label.End();

                // NB: at least as tall as the label, centered on the row as the label is, so that on a row shorter
                // than the text the highlight frames it rather than striking it through.
                var markHeight = Math.Max(layout.RowHeight, lineHeight);
                var markTop = layout.RowTop(i) + (layout.RowHeight - markHeight) / 2;

                // Highlight hovered channel and the selected one
                if ((i == hovered || i == selected) && expandedChannel < 0)
                {
                    var fill = Colors.Of(i);
                    fill.W = HoverFillAlpha;
                    draw.AddRectFilled(new Vector2(left, markTop), new Vector2(right, markTop + markHeight),
                        ImGui.ColorConvertFloat4ToU32(fill));
                }

                if (i == selected && expandedChannel < 0)
                {
                    var boxTop = MathF.Floor(markTop);
                    draw.AddRect(new Vector2(left, boxTop), new Vector2(right, boxTop + markHeight), WaveformCursors.Color);
                }

                if (i != hovered && i != selected && (i % step != 0 || Crowds(i, hovered) || Crowds(i, selected)))
                    continue;

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
        /// Outlines a channel's trace, its max and min, or the line through its samples where each column holds
        /// one. Drawn every frame over the traces' texture, for the one channel hovered or expanded.
        /// </summary>
        unsafe void PlotChannelLines(Envelope envelope, int channel, float weight)
        {
            envelope.Min.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            envelope.Max.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            var minLine = (float*)((byte*)minPtr + channel * minStep);
            var maxLine = (float*)((byte*)maxPtr + channel * maxStep);
            var columns = shape.Width;

            // NB: no more bins than the plot has pixels, each at the first column it covers on the plot's column
            // axis, as the texture beneath has them.
            var pixels = (int)plotSpan;
            var bins = pixels > 0 && pixels < columns ? pixels : columns;
            float* binX = stackalloc float[bins];
            float* binMin = stackalloc float[bins];
            float* binMax = stackalloc float[bins];
            var scale = (float)(1 / rangeAmplitude);
            for (int p = 0; p < bins; p++)
            {
                int start = p * columns / bins, end = (p + 1) * columns / bins;
                float low = minLine[start], high = maxLine[start];
                for (int c = start + 1; c < end; c++)
                {
                    low = Math.Min(low, minLine[c]);
                    high = Math.Max(high, maxLine[c]);
                }

                binX[p] = start;
                binMin[p] = low * scale - channel;
                binMax[p] = high * scale - channel;
            }

            ImPlot.PushStyleColor(ImPlotCol.Line, Colors.Of(channel));
            ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, weight);
            ImPlot.PlotLine(string.Empty, binX, binMin, bins);
            if (window.Step > 1 || bins < columns)
                ImPlot.PlotLine(string.Empty, binX, binMax, bins);
            ImPlot.PopStyleVar();
            ImPlot.PopStyleColor();
        }

        /// <summary>
        /// Marks the column being written, or where the display was paused.
        /// </summary>
        unsafe void PlotSweepCursor()
        {
            if (FastSweep)
                return;

            // NB: not rounded to a column while paused, since the pause instant can fall partway through one.
            var columns = decimator.Sweep.Cols;
            double sweepHead = Paused ? window.FractionOf(CursorPosition) * columns : decimator.Cursor;
            if (sweepHead < 0 || sweepHead >= columns)
                return;

            ImPlot.PushStyleColor(ImPlotCol.Line, ImGui.ColorConvertU32ToFloat4(ColSweepCursor));
            ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, SweepCursorWeight);
            ImPlot.PlotInfLines(string.Empty, &sweepHead, 1);
            ImPlot.PopStyleVar();
            ImPlot.PopStyleColor();
        }

        /// <summary>
        /// Shades the row of the selected channel.
        /// </summary>
        /// <remarks>
        /// Drawn ahead of the traces, so the selection is marked without anything drawn over the trace. Not in the
        /// heatmap, which covers it, and where lines above and below the row mark it instead.
        /// </remarks>
        static void DrawRowBand(ImDrawListPtr draw, in PlotFrame frame)
        {
            if (frame.Selected < 0 || frame.Expanded >= 0 || frame.Heatmap)
                return;

            var top = MathF.Floor(frame.Layout.RowTop(frame.Selected));
            draw.AddRectFilled(new Vector2(frame.Left, top),
                new Vector2(frame.Left + frame.Width, top + frame.Layout.RowHeight), ColSelectedRow);
        }

        // NB: after the cursors, so that a click that grabs a cursor's line does not also select.
        void SelectClicked(in PlotFrame frame, int hovered)
        {
            var pointer = ImGui.GetMousePos();
            var inPlot = ImGui.IsWindowHovered() &&
                pointer.X >= frame.Left && pointer.X < frame.Left + frame.Width &&
                pointer.Y >= frame.Top && pointer.Y < frame.Bottom;
            if (inPlot && !cursors.Dragging && hovered >= 0 && !channelHidden[hovered] && expandedChannel < 0 &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Modifiers())
            {
                selectedChannel = hovered;
            }
        }

        /// <summary>
        /// Marks a channel's row with a line just above it and another just below, leaving the row as drawn.
        /// </summary>
        internal static void BracketRow(ImDrawListPtr draw, in PlotFrame frame, int channel, uint color)
        {
            var top = MathF.Floor(frame.Layout.RowTop(channel));
            var bottom = MathF.Floor(frame.Layout.RowTop(channel) + frame.Layout.RowHeight);
            var right = frame.Left + frame.Width;
            draw.AddRectFilled(new Vector2(frame.Left, top - 1), new Vector2(right, top), color);
            draw.AddRectFilled(new Vector2(frame.Left, bottom), new Vector2(right, bottom + 1), color);
        }

        // NB: filled rects on whole pixels, here and in DrawGraticules and BracketRow. AddRect and AddLine draw
        // half a pixel off and anti-alias, which blurs a 1 px line.
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

        /// <summary>
        /// Draws the time divisions and their labels.
        /// </summary>
        /// <remarks>
        /// Live, zero is the start of the sweep. Paused, zero is the cursor and every label is how long before
        /// the pause that sample was taken, so the frozen tail right of the cursor reads oldest. Divisions are
        /// fixed to positions on the axis rather than to the plot, so they pan with the data.
        /// </remarks>
        void DrawGraticules(ImDrawListPtr draw, float left, float width, float top, float bottom, float labelY)
        {
            var t = MathF.Floor(top) + GraticuleWeight;
            var b = MathF.Floor(bottom) - GraticuleWeight;
            var textColor = ImGui.GetColorU32(ImGuiCol.Text);

            // NB: a tenth of the window's span, not of the timebase. The span is a whole number of columns
            // and slightly shorter, so a tenth of the timebase would not fit ten times and the last
            // division would flicker. Labels still show the round timebase.
            var interval = window.Span / (double)TimeDivisions;
            var origin = Paused ? CursorPosition : 0;

            for (var d = (long)Math.Ceiling((window.Start - origin) / interval);
                 d <= (long)Math.Floor((window.End - origin) / interval);
                 d++)
            {
                var position = origin + d * interval;
                var x = left + (float)(width * window.FractionOf(position));

                // NB: skip divisions on the edges, where the frame already draws a line.
                var l = MathF.Floor(x);
                if (l > left + 1 && l < left + width - 1)
                    DivisionDots(draw, new Vector2(l, t), b - t, vertical: true);

                // NB: one spacing across the whole plot, so labels on either side of the cursor never
                // overlap. Right of the cursor is the frozen tail, a whole frozen window older.
                var seconds = d * timebase / TimeDivisions;
                if (Paused && position > CursorPosition)
                    seconds -= pausedTimebase;

                var label = $"{seconds:g} s";
                draw.AddText(new Vector2(x - ImGui.CalcTextSize(label).X / 2, labelY), textColor, label);
            }

            if (expandedChannel < 0)
                return;

            // NB: an expanded channel fills the plot, so its row is the plot, and a tenth of the range is a
            // tenth of the height. The outermost divisions lie on the frame.
            var l0 = MathF.Floor(left) + GraticuleWeight;
            for (int k = 1; k < AmplitudeDivisions; k++)
                DivisionDots(draw, new Vector2(l0, MathF.Floor(AmplitudeDivisionY(k, top, bottom))),
                    MathF.Floor(left + width) - GraticuleWeight - l0, vertical: false);
        }

        /// <summary>
        /// Height on screen of amplitude division <paramref name="k"/> of an expanded channel, counted down from
        /// the top of the plot.
        /// </summary>
        static float AmplitudeDivisionY(int k, float top, float bottom) =>
            top + (bottom - top) * k / AmplitudeDivisions;

        /// <summary>
        /// Labels an expanded channel's amplitude divisions down the left of the plot, and names the channel
        /// and both scales in its top left corner.
        /// </summary>
        /// <remarks>
        /// Drawn over the traces, where the divisions themselves are drawn under them.
        /// </remarks>
        void AmplitudeLabels(ImDrawListPtr draw, float left, float top, float bottom)
        {
            if (expandedChannel < 0)
                return;

            var pad = ImGui.GetStyle().FramePadding;
            var color = ImGui.GetColorU32(ImGuiCol.TextDisabled);
            for (int k = 1; k < AmplitudeDivisions; k++)
            {
                var text = $"{PlotText.Significant(rangeAmplitude * (AmplitudeDivisions / 2 - k) / AmplitudeDivisions)}";
                var y = AmplitudeDivisionY(k, top, bottom) - ImGui.GetTextLineHeight() / 2;
                draw.AddText(new Vector2(left + pad.X, y), color, text);
            }

            var timeDivision = timebase / TimeDivisions;
            var time = timeDivision < 1 ? $"{PlotText.Significant(timeDivision * 1000)} ms/div" : $"{PlotText.Significant(timeDivision)} s/div";
            var name = $"Ch {expandedChannel}   {PlotText.Significant(rangeAmplitude / AmplitudeDivisions)} {Unit}/div   {time}";
            PlotText.Framed(draw, new Vector2(left, top) + 2 * pad, name);
        }

        static void DivisionDots(ImDrawListPtr draw, Vector2 start, float length, bool vertical)
        {
            // NB: whole-pixel rects, as for the frame, since a 1 px line drawn any other way blurs.
            for (var d = 0f; d < length; d += GraticuleWeight + DivisionDotGap)
            {
                var at = vertical ? start + new Vector2(0, d) : start + new Vector2(d, 0);
                draw.AddRectFilled(at, at + new Vector2(GraticuleWeight), ColDivision);
            }
        }
    }
}
