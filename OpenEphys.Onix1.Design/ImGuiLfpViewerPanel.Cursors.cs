using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <remarks>
    /// A cursor is an axis position only, so it stays on the samples it marks through panning, zooming and
    /// pausing. Its readings are taken where it crosses the selected channel. Cursor 0 is always present
    /// and is the origin the others are measured from.
    /// </remarks>
    partial class ImGuiLfpViewerPanel
    {
        const int MaxCursors = 4;
        const uint ColCursor = ImGuiPalette.White;
        const float CursorDot = 2f;
        const float CursorGrabDistance = 4f;
        const float CursorMarkerRadius = 3f;
        const float CursorLabelGap = 4f;
        const float CursorArrow = 8f;
        static readonly uint ColSelectedRow = ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x20);
        static readonly uint ColLabelBg = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0xBB);
        const float LabelPad = 3f;

        readonly List<long> cursors = new(MaxCursors);
        int grabbedCursor = -1;
        int selectedChannel = -1;
        Vector2 cursorTableSize;

        /// <summary>
        /// Whether the cursors are shown. While they are, a cursor's line can be dragged and a plain click on
        /// the plot selects the channel they read.
        /// </summary>
        public bool ShowCursors { get; set; }

        /// <summary>
        /// Moves the cursors to the middle of the view, keeping their spacing, or spreads them evenly across it
        /// in the same order if they span more than it does.
        /// </summary>
        public void HomeCursors()
        {
            if (cursors.Count == 0)
                return;

            long first = long.MaxValue, last = long.MinValue;
            foreach (var position in cursors)
            {
                first = Math.Min(first, position);
                last = Math.Max(last, position);
            }

            if (last - first < window.Span)
            {
                var shift = window.Start + window.Span / 2 - (first + last) / 2;
                for (int i = 0; i < cursors.Count; i++)
                    cursors[i] = SnapToColumn(cursors[i] + shift);
                return;
            }

            var order = new int[cursors.Count];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
            Array.Sort(cursors.ToArray(), order);
            for (int k = 0; k < order.Length; k++)
                cursors[order[k]] = SnapToColumn(window.Start + window.Span * (k + 1) / (order.Length + 1));
        }

        /// <summary>
        /// The channel the cursors read, or -1 while they are hidden. An expanded channel is the only one
        /// there is to read.
        /// </summary>
        int CursorChannel => !ShowCursors ? -1 : expandedChannel >= 0 ? expandedChannel : selectedChannel;

        /// <summary>
        /// The middle of the column <paramref name="position"/> falls in, within the window.
        /// </summary>
        /// <remarks>
        /// The middle rather than the start, so that the column it is read from does not change with a
        /// window whose start is not a whole number of columns from it.
        /// </remarks>
        long SnapToColumn(long position)
        {
            var column = Math.Max(0, Math.Min(window.Columns - 1, window.ColumnOf(position)));
            return window.PositionOf(column) + window.Step / 2;
        }

        /// <summary>
        /// Shades the row of the channel the cursors read.
        /// </summary>
        /// <remarks>
        /// Drawn ahead of the traces, so the selection is marked without anything drawn over the trace.
        /// </remarks>
        void SelectedRowBand(ImDrawListPtr draw, in RowLayout layout, float left, float width)
        {
            var channel = CursorChannel;
            if (channel < 0 || expandedChannel >= 0)
                return;

            var top = MathF.Floor(layout.RowTop(channel));
            draw.AddRectFilled(new Vector2(left, top), new Vector2(left + width, top + layout.RowHeight), ColSelectedRow);
        }

        /// <summary>
        /// Moves a cursor by dragging its line, selects the channel clicked on, steps the selection with
        /// Q and E, and draws the cursors.
        /// </summary>
        void Cursors(ImDrawListPtr draw, in RowLayout layout, int hovered, Mat waveformMin, Mat waveformMax,
            float left, float width, float top, float bottom)
        {
            if (!ShowCursors)
                return;

            if (cursors.Count == 0)
                cursors.Add(SnapToColumn(window.PositionOf(window.Columns / 2)));

            if (selectedChannel < 0 || selectedChannel >= channelHidden.Length)
                selectedChannel = layout.FirstVisible;

            var pointer = ImGui.GetMousePos();
            var inPlot = ImGui.IsWindowHovered() &&
                pointer.X >= left && pointer.X < left + width &&
                pointer.Y >= top && pointer.Y < bottom;

            if (grabbedCursor >= 0)
            {
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    grabbedCursor = -1;
                else
                {
                    // NB: clamped to the plot, so a cursor dragged past an edge stops at it.
                    var fraction = Math.Max(0, Math.Min(1, (pointer.X - left) / width));
                    cursors[grabbedCursor] = SnapToColumn(window.Start + (long)(fraction * window.Span));
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                }
            }
            else if (inPlot)
            {
                var near = -1;
                var nearest = CursorGrabDistance;
                for (int i = 0; i < cursors.Count; i++)
                {
                    var distance = Math.Abs(pointer.X - CursorX(cursors[i], left, width));
                    if (distance <= nearest)
                    {
                        near = i;
                        nearest = distance;
                    }
                }

                var clicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Modifiers();
                if (near >= 0)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                    if (clicked)
                        grabbedCursor = near;
                }
                else if (clicked && hovered >= 0 && !channelHidden[hovered])
                {
                    selectedChannel = hovered;
                }
            }

            var step = !Modifiers() || expandedChannel >= 0 ? 0
                : HotkeyPressed(ImGuiKey.Q) ? -1
                : HotkeyPressed(ImGuiKey.E) ? 1
                : 0;
            if (step != 0)
            {
                for (var c = selectedChannel + step; c >= 0 && c < channelHidden.Length; c += step)
                {
                    if (!channelHidden[c])
                    {
                        selectedChannel = c;
                        break;
                    }
                }

                var rowTop = layout.RowTop(selectedChannel);
                if (rowTop < top)
                    ImGui.SetScrollY(ImGui.GetScrollY() - (top - rowTop));
                else if (rowTop + layout.RowHeight > bottom)
                    ImGui.SetScrollY(ImGui.GetScrollY() + (rowTop + layout.RowHeight - bottom));
            }

            for (int i = 0; i < cursors.Count; i++)
            {
                var x = CursorX(cursors[i], left, width);
                if (x < left || x > left + width)
                    continue;

                // NB: the reference solid and the rest dotted, as on a scope.
                if (i == 0)
                    draw.AddRectFilled(new Vector2(MathF.Floor(x), top), new Vector2(MathF.Floor(x) + GraticuleWeight, bottom),
                        ColCursor);
                else
                    DottedLine(draw, x, top, bottom);

                if (TryReadCursor(waveformMin, waveformMax, cursors[i], out var min, out var max))
                    CursorValues(draw, layout, min, max, x, left + width);
            }

            // NB: one row per cursor rather than packed, so the dimensions can never overlap however the
            // cursors are arranged.
            var row = ImGui.GetTextLineHeight() + 2 * LabelPad + CursorLabelGap;
            for (int i = 1; i < cursors.Count; i++)
                CursorDimension(draw, cursors[0], cursors[i], left, width, top + CursorLabelGap + (i - 1) * row);
        }

        /// <summary>
        /// A dimension from <paramref name="from"/> to <paramref name="to"/> at height <paramref name="y"/>,
        /// with arrowheads on the cursor lines and the time between them and its inverse in the middle.
        /// </summary>
        void CursorDimension(ImDrawListPtr draw, long from, long to, float left, float width, float y)
        {
            var samples = Math.Abs(to - from);
            if (samples == 0)
                return;

            var a = CursorX(Math.Min(from, to), left, width);
            var b = CursorX(Math.Max(from, to), left, width);
            var right = left + width;
            if (b < left || a > right)
                return;

            var size = ImGui.CalcTextSize("0");
            var center = y + LabelPad + size.Y / 2;
            var start = Math.Max(left, a);
            var end = Math.Min(right, b);
            draw.AddRectFilled(new Vector2(start, MathF.Floor(center)), new Vector2(end, MathF.Floor(center) + GraticuleWeight),
                ColCursor);

            // NB: an arrowhead only where the dimension reaches its cursor, not where the plot edge cuts it.
            if (a >= left)
                draw.AddTriangleFilled(new Vector2(a, center),
                    new Vector2(a + CursorArrow, center - CursorArrow / 2), new Vector2(a + CursorArrow, center + CursorArrow / 2),
                    ColCursor);
            if (b <= right)
                draw.AddTriangleFilled(new Vector2(b, center),
                    new Vector2(b - CursorArrow, center + CursorArrow / 2), new Vector2(b - CursorArrow, center - CursorArrow / 2),
                    ColCursor);

            var text = $"{Significant(samples * 1000.0 / sampleRate)} ms ({Significant(sampleRate / (double)samples)} Hz)";
            var textSize = ImGui.CalcTextSize(text);
            var corner = new Vector2(
                Math.Max(left + LabelPad, Math.Min(right - LabelPad - textSize.X, (start + end - textSize.X) / 2)),
                center - textSize.Y / 2);
            draw.AddRectFilled(corner - new Vector2(LabelPad), corner + textSize + new Vector2(LabelPad), ColLabelBg, LabelPad);
            draw.AddText(corner, ColCursor, text);
        }

        /// <summary>
        /// The min and max of the column at <paramref name="position"/> on the channel the cursors read.
        /// </summary>
        /// <remarks>
        /// Read from the column that is drawn, so the values are those of the trace on screen.
        /// </remarks>
        /// <returns>False if no channel can be read, or the column is off the window or holds no data.</returns>
        bool TryReadCursor(Mat waveformMin, Mat waveformMax, long position, out double min, out double max)
        {
            min = max = double.NaN;
            var channel = CursorChannel;
            var column = window.ColumnOf(position);
            if (channel < 0 || channelHidden[channel] && channel != expandedChannel ||
                column < 0 || column >= waveformMin.Cols)
                return false;

            min = waveformMin.GetReal(channel, column);
            max = waveformMax.GetReal(channel, column);
            return !double.IsNaN(min) && !double.IsNaN(max);
        }

        /// <summary>
        /// Marks where a cursor crosses the channel's trace and labels the value there: the column's max above
        /// and its min below, or the one sample when the column holds only one.
        /// </summary>
        void CursorValues(ImDrawListPtr draw, in RowLayout layout, double min, double max, float x, float right)
        {
            var center = layout.RowTop(CursorChannel) + layout.RowHeight / 2;
            var scale = layout.RowHeight / rangeAmplitude;
            float ScreenY(double value) => center - (float)(value * scale);

            if (min == max)
            {
                CursorValue(draw, new Vector2(x, ScreenY(max)), max, 0, right);
                return;
            }

            CursorValue(draw, new Vector2(x, ScreenY(max)), max, -1, right);
            CursorValue(draw, new Vector2(x, ScreenY(min)), min, 1, right);
        }

        /// <summary>
        /// A marker at <paramref name="at"/>, labeled beside the cursor line: above the marker for
        /// <paramref name="side"/> -1, below it for 1, level with it for 0.
        /// </summary>
        void CursorValue(ImDrawListPtr draw, Vector2 at, double value, int side, float right)
        {
            draw.AddCircleFilled(at, CursorMarkerRadius, ColCursor);

            var text = $"{Significant(value)} {Unit}";
            var size = ImGui.CalcTextSize(text);
            var x = at.X + CursorLabelGap;
            if (x + size.X + LabelPad > right)
                x = at.X - CursorLabelGap - size.X;

            var y = side < 0 ? at.Y - CursorLabelGap - size.Y
                : side > 0 ? at.Y + CursorLabelGap
                : at.Y - size.Y / 2;

            var corner = new Vector2(x, y);
            draw.AddRectFilled(corner - new Vector2(LabelPad), corner + size + new Vector2(LabelPad), ColLabelBg, LabelPad);
            draw.AddText(corner, ColCursor, text);
        }

        // NB: three significant figures, as fine as a value read off a trace by eye is worth.
        static double Significant(double value)
        {
            if (value == 0)
                return 0;

            var decimals = 2 - (int)Math.Floor(Math.Log10(Math.Abs(value)));
            return Math.Round(value, Math.Max(0, Math.Min(15, decimals)));
        }

        /// <summary>
        /// Labels each cursor with its time in the row of time axis labels.
        /// </summary>
        /// <remarks>
        /// Boxed, the reference filled, and drawn over the axis labels on the window's own background with a margin either side, so
        /// that it takes the place of any axis label close enough to collide with it.
        /// </remarks>
        void CursorTimeLabels(ImDrawListPtr draw, float left, float width, float labelY)
        {
            if (!ShowCursors)
                return;

            var pad = ImGui.GetStyle().FramePadding;
            var margin = new Vector2(pad.X + ImGui.GetStyle().ItemSpacing.X, pad.Y);
            var background = ImGui.GetColorU32(ImGuiCol.WindowBg);
            for (int i = 0; i < cursors.Count; i++)
            {
                var x = CursorX(cursors[i], left, width);
                if (x < left || x > left + width)
                    continue;

                var text = $"C{i}: {FormatSeconds(SecondsAt(cursors[i]))} s";
                var size = ImGui.CalcTextSize(text);
                var corner = new Vector2(
                    Math.Max(left + pad.X, Math.Min(left + width - pad.X - size.X, x - size.X / 2)), labelY);
                draw.AddRectFilled(corner - margin, corner + size + margin, background);
                if (i == 0)
                {
                    draw.AddRectFilled(corner - pad, corner + size + pad, ColCursor);
                    draw.AddText(corner, ImGuiPalette.Black, text);
                }
                else
                {
                    draw.AddRect(corner - pad, corner + size + pad, ColCursor);
                    draw.AddText(corner, ColCursor, text);
                }
            }
        }

        // NB: to the width of a column, which is as fine as a cursor can be placed.
        string FormatSeconds(double seconds) =>
            seconds.ToString("F" + Math.Max(0, (int)Math.Ceiling(-Math.Log10(window.Step / (double)sampleRate))));

        /// <summary>
        /// The cursors' readings on the selected channel, in a table over the plot's lower left corner, with
        /// buttons to add and remove cursors.
        /// </summary>
        /// <remarks>
        /// Times are read only. Right of the sweep cursor on a paused view, two places on the axis carry the
        /// same time, so a typed time would not name one place. A cursor is moved by dragging its line.
        /// </remarks>
        void CursorTable(Mat waveformMin, Mat waveformMax, float left, float bottom)
        {
            if (!ShowCursors || cursors.Count == 0)
                return;

            var gap = ImGui.GetStyle().ItemSpacing;
            ImGui.SetCursorScreenPos(new Vector2(left + gap.X, bottom - cursorTableSize.Y - gap.Y));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ColLabelBg);

            // NB: the plot's region is opened with no vertical padding, which this window would inherit.
            var padding = ImGui.GetStyle().WindowPadding.X;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding, padding));
            var childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY |
                ImGuiChildFlags.AlwaysAutoResize;
            if (ImGui.BeginChild("##cursorTable", Vector2.Zero, childFlags, ImGuiWindowFlags.NoScrollbar))
            {
                var channel = CursorChannel;
                ImGui.TextUnformatted(channel >= 0 ? $"Ch {channel}" : "No channel");

                var remove = -1;
                // NB: a column of one sample has min and max equal, so it has one value and one difference from C0.
                var oneSample = window.Step == 1;
                var headers = oneSample
                    ? new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"y ({Unit})", $"dy ({Unit})", "" }
                    : new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"min ({Unit})", $"max ({Unit})", $"dmin ({Unit})", $"dmax ({Unit})", "" };
                if (ImGui.BeginTable("##readings", headers.Length, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerV))
                {
                    foreach (var header in headers)
                        ImGui.TableSetupColumn(header);
                    ImGui.TableHeadersRow();

                    TryReadCursor(waveformMin, waveformMax, cursors[0], out var min0, out var max0);
                    for (int i = 0; i < cursors.Count; i++)
                    {
                        var read = TryReadCursor(waveformMin, waveformMax, cursors[i], out var min, out var max);
                        var samples = cursors[i] - cursors[0];

                        ImGui.TableNextRow();

                        // NB: only paused, since live the window does not move. Home brings a cursor back instead.
                        ImGui.TableNextColumn();
                        ImGui.BeginDisabled(!Paused);
                        if (ImGui.SmallButton($"C{i}"))
                            Pan(window.Start + window.Span / 2 - cursors[i]);
                        ImGui.EndDisabled();

                        Cell(FormatSeconds(SecondsAt(cursors[i])));
                        Cell(i > 0 ? $"{Significant(samples * 1000.0 / sampleRate)}" : "");
                        Cell(i > 0 && samples != 0 ? $"{Significant(sampleRate / (double)Math.Abs(samples))}" : "");
                        Cell(read ? $"{Significant(min)}" : "");
                        if (!oneSample)
                            Cell(read ? $"{Significant(max)}" : "");
                        Cell(i > 0 && read && !double.IsNaN(min0) ? $"{Significant(min - min0)}" : "");
                        if (!oneSample)
                            Cell(i > 0 && read && !double.IsNaN(max0) ? $"{Significant(max - max0)}" : "");

                        ImGui.TableNextColumn();
                        if (i > 0 && ImGui.SmallButton($"x##remove{i}"))
                            remove = i;
                    }

                    ImGui.EndTable();
                }

                if (remove > 0)
                    cursors.RemoveAt(remove);

                ImGui.BeginDisabled(cursors.Count >= MaxCursors);
                if (ImGui.SmallButton("+ Add cursor"))
                    cursors.Add(SnapToColumn(window.PositionOf(window.Columns / 2)));
                ImGui.EndDisabled();

                ImGui.SameLine();
                if (ImGui.SmallButton("Home (h)"))
                    HomeCursors();
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            cursorTableSize = ImGui.GetItemRectSize();

            static void Cell(string text)
            {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(text);
            }
        }

        float CursorX(long position, float left, float width) =>
            left + (float)(width * window.FractionOf(position));

        static void DottedLine(ImDrawListPtr draw, float x, float top, float bottom)
        {
            x = MathF.Floor(x);
            for (var y = MathF.Floor(top); y < bottom; y += 2 * CursorDot)
                draw.AddRectFilled(
                    new Vector2(x, y), new Vector2(x + GraticuleWeight, Math.Min(y + CursorDot, bottom)), ColCursor);
        }
    }
}
