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

        static readonly uint ColFrozenTail = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0x80);

        /// <summary>
        /// Color and weight of the plot's frame, for the control strip's divider to match.
        /// </summary>
        public const uint FrameColor = ColGraticule;
        public const float FrameWeight = GraticuleWeight;
        const float HoverFillAlpha = 0.65f;

        float plotLeft;
        float plotSpan;

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
        /// <param name="waveformMin">Per-bin minima, one row per channel.</param>
        /// <param name="waveformMax">Per-bin maxima, one row per channel.</param>
        void WaveformPlot(Mat waveformMin, Mat waveformMax)
        {
            var rows = waveformMin.Rows;
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
                plotLeft = plotX;
                plotSpan = plotWidth;
                if (ImPlot.BeginPlot("##channels", new(plotWidth, layout.Height), plotFlags))
                {
                    ImPlot.SetupAxes(string.Empty, string.Empty, axesFlags, axesFlags);
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, waveformMinDecimator.Buffer.Cols, ImPlotCond.Always);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, -(layout.LastRow - 1) - 0.5, -layout.FirstRow + 0.5, ImPlotCond.Always);
                    SelectedRowBand(ImGui.GetWindowDrawList(), layout, plotX, plotWidth);
                    PlotTraces(waveformMin, waveformMax, layout.FirstVisible, layout.LastVisible, hovered,
                        layout.RowHeight);
                    PlotSweepCursor();
                    ImPlot.EndPlot();

                    // NB: both drawn after the plot, and from inside the table,
                    // so they lie over the traces
                    ShadeFrozenTail(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);
                    Cursors(ImGui.GetWindowDrawList(), layout, hovered, waveformMin, waveformMax,
                        plotX, plotWidth, plotTop, plotBottom);
                    DrawFrame(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom);
                    DrawReadout(ImGui.GetWindowDrawList(), layout, hovered, plotX, plotWidth, plotBottom);
                    AmplitudeLabels(ImGui.GetWindowDrawList(), plotX, plotTop, plotBottom);
                }
                ImGui.EndTable();
            }

            ImPlot.PopStyleColor();
            ImPlot.PopStyleVar(2);

            if (plotWidth > 0)
            {
                DrawGraticules(ImGui.GetWindowDrawList(), plotX, plotWidth, plotTop, plotBottom, labelY);
                CursorTimeLabels(ImGui.GetWindowDrawList(), plotX, plotWidth, labelY);
                HandlePanInput(plotX, plotWidth, labelY);
                TimeScrollBar(plotX, plotWidth, plotBottom + scrollGap, scrollHeight);

                // NB: last, so that its window is drawn over the table's and takes the pointer from it.
                CursorTable(waveformMin, waveformMax, plotX, plotBottom);
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
        /// The time the axis labels give <paramref name="position"/>.
        /// </summary>
        double SecondsAt(double position)
        {
            var seconds = (position - (Paused ? CursorPosition : 0)) / window.Span * timebase;
            return Paused && position > CursorPosition ? seconds - pausedTimebase : seconds;
        }

        /// <summary>
        /// Shows the channel, time and amplitude under the pointer in the plot's lower right corner.
        /// </summary>
        /// <remarks>
        /// The time reads as the time axis labels do, and the amplitude from the center of the channel's row.
        /// </remarks>
        void DrawReadout(ImDrawListPtr draw, in RowLayout layout, int hovered, float left, float width, float bottom)
        {
            var mouse = ImGui.GetMousePos();
            if (hovered < 0 || mouse.X < left || mouse.X >= left + width)
                return;

            var seconds = SecondsAt(window.Start + PlotFraction(mouse.X) * window.Span);
            var center = layout.RowTop(hovered) + layout.RowHeight / 2;
            var amplitude = (center - mouse.Y) / layout.RowHeight * rangeAmplitude;

            // NB: an expanded channel is named in the plot's top left already.
            var position = $"{seconds:0.0000} s   {amplitude:0.0} {Unit}";
            var text = expandedChannel >= 0 ? position : $"Ch {hovered}   {position}";
            var corner = new Vector2(left + width, bottom) - ImGui.CalcTextSize(text) - 2 * ImGui.GetStyle().FramePadding;
            FramedText(draw, corner, text);
        }

        /// <summary>
        /// Writes <paramref name="text"/> at <paramref name="corner"/> in a box framed like the cursor table.
        /// </summary>
        /// <remarks>
        /// The cursor table is a bordered child window, so the box takes its rounding and border color from the
        /// style, and the overlays on the plot read as one set.
        /// </remarks>
        static void FramedText(ImDrawListPtr draw, Vector2 corner, string text)
        {
            var pad = ImGui.GetStyle().FramePadding;
            var rounding = ImGui.GetStyle().ChildRounding;
            var size = ImGui.CalcTextSize(text);
            draw.AddRectFilled(corner - pad, corner + size + pad, ColLabelBg, rounding);
            draw.AddRect(corner - pad, corner + size + pad, ImGui.GetColorU32(ImGuiCol.Border), rounding);
            draw.AddText(corner, ImGui.GetColorU32(ImGuiCol.Text), text);
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

        static float LabelColumnWidth(int labelDigits) => labelDigits * ImGui.CalcTextSize("0").X;

        unsafe void ChannelLabels(in RowLayout layout, int labelDigits, int hovered)
        {
            var labelBuffer = stackalloc byte[32];
            var label = new StrBuilder(labelBuffer, 32);
            var labelTop = ImGui.GetCursorPosY();
            var textOffset = (layout.RowHeight - ImGui.GetTextLineHeight()) / 2;
            var left = ImGui.GetCursorScreenPos().X;
            var right = left + ImGui.GetContentRegionAvail().X;
            var draw = ImGui.GetWindowDrawList();

            // NB: an expanded channel is named inside the plot instead, by AmplitudeLabels.
            for (int i = layout.FirstVisible; i < layout.LastVisible && expandedChannel < 0; i++)
            {
                label.Reset();
                for (var n = DigitCount(i); n < labelDigits; n++)
                    label.Append('0');
                label.Append(i);
                label.End();

                // Highlight hovered channel and the one the cursors read
                if ((i == hovered || i == CursorChannel) && expandedChannel < 0)
                {
                    var rowTop = layout.RowTop(i);
                    var fill = ChannelColor(i);
                    fill.W = HoverFillAlpha;
                    draw.AddRectFilled(new Vector2(left, rowTop), new Vector2(right, rowTop + layout.RowHeight),
                        ImGui.ColorConvertFloat4ToU32(fill));
                }

                if (i == CursorChannel && expandedChannel < 0)
                {
                    var rowTop = MathF.Floor(layout.RowTop(i));
                    draw.AddRect(new Vector2(left, rowTop), new Vector2(right, rowTop + layout.RowHeight), ColCursor);
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
        /// Scales the buffers into channel units, <c>data / range + rowOffsets</c> with <c>-i</c> in row
        /// <c>i</c>, and draws the visible rows.
        /// </summary>
        unsafe void PlotTraces(Mat waveformMin, Mat waveformMax, int first, int last, int hovered, float rowHeight)
        {
            CV.AddWeighted(waveformMin, 1 / rangeAmplitude, rowOffsets, 1, 0, scaledWaveformMin);
            CV.AddWeighted(waveformMax, 1 / rangeAmplitude, rowOffsets, 1, 0, scaledWaveformMax);
            scaledWaveformMin.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            scaledWaveformMax.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            var columns = shape.Width;

            // NB: no more bins than the plot has pixels, each holding the min and max of the columns that
            // fall in it. A pixel cannot show more than that, and every column drawn costs the same however
            // many share a pixel. Each bin sits at the first column it covers, on the plot's column axis.
            var pixels = (int)plotSpan;
            var bins = pixels > 0 && pixels < columns ? pixels : columns;
            float* binX = stackalloc float[bins];
            float* binMin = stackalloc float[bins];
            float* binMax = stackalloc float[bins];
            float* fillMin = stackalloc float[bins];
            float* fillMax = stackalloc float[bins];
            for (int p = 0; p < bins; p++)
                binX[p] = p * columns / bins;

            // NB: with no outline to fall back on, the fill alone has to cover the trace. Each bin is stretched
            // to reach the one before it, so the band covers every step between neighbors as a line would,
            // rather than a band one pixel tall that a steep step thins below a pixel. What is left flat is
            // padded to a pixel, so a trace does not vanish where it is flattest.
            var pixel = 1f / rowHeight;

            // NB: an expanded channel is drawn alone, so it keeps the translucent fill and outlines that show
            // its shape best. Otherwise outlines would double what each channel costs to draw, so the fill
            // carries the trace on its own and outlines mark only the channel under the pointer.
            var expanded = expandedChannel >= 0;
            ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, expanded ? 0.25f : 1f);

            // NB: when every bin is one sample there is no envelope, only the samples, so they are joined by a
            // line. A fill there could only span neighboring samples, and would shadow the line.
            var samplesOnly = window.Step == 1 && bins == columns;

            for (int i = first; i < last; i++)
            {
                if (channelHidden[i] && i != expandedChannel)
                    continue;

                var minLine = (float*)((byte*)minPtr + i * minStep);
                var maxLine = (float*)((byte*)maxPtr + i * maxStep);
                for (int p = 0; p < bins; p++)
                {
                    int start = p * columns / bins, end = (p + 1) * columns / bins;
                    float low = minLine[start], high = maxLine[start];
                    for (int c = start + 1; c < end; c++)
                    {
                        low = Math.Min(low, minLine[c]);
                        high = Math.Max(high, maxLine[c]);
                    }

                    binMin[p] = low;
                    binMax[p] = high;

                    if (p > 0 && !float.IsNaN(binMin[p - 1]))
                    {
                        low = Math.Min(low, binMax[p - 1]);
                        high = Math.Max(high, binMin[p - 1]);
                    }

                    var shortfall = pixel - (high - low);
                    if (shortfall > 0)
                    {
                        low -= shortfall / 2;
                        high += shortfall / 2;
                    }

                    fillMin[p] = low;
                    fillMax[p] = high;
                }

                var channelColor = ChannelColor(i);
                ImPlot.PushStyleColor(ImPlotCol.Fill, channelColor);
                ImPlot.PushStyleColor(ImPlotCol.Line, channelColor);
                var hover = i == hovered && !expanded;
                if (hover) ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, HoverLineWeight);

                if (samplesOnly)
                {
                    ImPlot.PlotLine(string.Empty, binX, binMin, bins);
                }
                else
                {
                    ImPlot.PlotShaded(string.Empty, binX, fillMin, fillMax, bins);
                    if (expanded || hover)
                    {
                        ImPlot.PlotLine(string.Empty, binX, binMin, bins);
                        ImPlot.PlotLine(string.Empty, binX, binMax, bins);
                    }
                }

                if (hover) ImPlot.PopStyleVar();

                ImPlot.PopStyleColor(2);
            }

            ImPlot.PopStyleVar();
        }

        /// <summary>
        /// Marks the column being written, or where the display was paused.
        /// </summary>
        unsafe void PlotSweepCursor()
        {
            // NB: not rounded to a column while paused, since the pause instant can fall partway through one.
            var columns = waveformMinDecimator.Buffer.Cols;
            double sweepHead = Paused ? window.FractionOf(CursorPosition) * columns : waveformMinDecimator.Cursor;
            if (sweepHead < 0 || sweepHead >= columns)
                return;

            ImPlot.PushStyleColor(ImPlotCol.Line, ImGui.ColorConvertU32ToFloat4(ColSweepCursor));
            ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, SweepCursorWeight);
            ImPlot.PlotInfLines(string.Empty, &sweepHead, 1);
            ImPlot.PopStyleVar();
            ImPlot.PopStyleColor();
        }

        // NB: filled rects on whole pixels, here and in DrawGraticules. AddRect and AddLine draw half a
        // pixel off and anti-alias, which blurs a 1 px line.
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
                var text = $"{Significant(rangeAmplitude * (AmplitudeDivisions / 2 - k) / AmplitudeDivisions)}";
                var y = AmplitudeDivisionY(k, top, bottom) - ImGui.GetTextLineHeight() / 2;
                draw.AddText(new Vector2(left + pad.X, y), color, text);
            }

            var timeDivision = timebase / TimeDivisions;
            var time = timeDivision < 1 ? $"{Significant(timeDivision * 1000)} ms/div" : $"{Significant(timeDivision)} s/div";
            var name = $"Ch {expandedChannel}   {Significant(rangeAmplitude / AmplitudeDivisions)} {Unit}/div   {time}";
            FramedText(draw, new Vector2(left, top) + 2 * pad, name);
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
