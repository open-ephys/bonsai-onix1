using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Cursors on a stack of traces: lines across the plot that read the selected channel where they cross it.
    /// </summary>
    /// <remarks>
    /// A cursor is an axis position only, so it stays on the samples it marks through panning, zooming and
    /// pausing. Cursor 0 is always present and is the origin the others are measured from. The cursors keep only
    /// their own state; everything about the plot comes in with each frame.
    /// </remarks>
    internal sealed class WaveformCursors
    {
        /// <summary>
        /// Color of the cursors and of everything marking what they read.
        /// </summary>
        public const uint Color = ImGuiPalette.White;

        const int MaxCursors = 4;
        const float LineWeight = 1;
        const float Dot = 2f;
        const float GrabDistance = 4f;
        const float MarkerRadius = 3f;
        const float LabelGap = 4f;
        const float Arrow = 8f;

        static readonly uint ColSelectedRow = ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x20);

        readonly List<long> positions = new(MaxCursors);
        int grabbed = -1;
        int selectedChannel = -1;
        Vector2 tableSize;

        /// <summary>
        /// Whether the cursors are shown. While they are, a cursor's line can be dragged and a plain click on
        /// the plot selects the channel they read.
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// The channel the cursors read, or -1 while they are hidden. An expanded channel is the only one
        /// there is to read.
        /// </summary>
        public int Channel(int expanded) => !Show ? -1 : expanded >= 0 ? expanded : selectedChannel;

        /// <summary>
        /// Moves the cursors to the middle of <paramref name="window"/>, keeping their spacing, or spreads them
        /// evenly across it in the same order if they span more than it does.
        /// </summary>
        public void Home(DisplayWindow window)
        {
            if (positions.Count == 0)
                return;

            long first = long.MaxValue, last = long.MinValue;
            foreach (var position in positions)
            {
                first = Math.Min(first, position);
                last = Math.Max(last, position);
            }

            if (last - first < window.Span)
            {
                var shift = window.Start + window.Span / 2 - (first + last) / 2;
                for (int i = 0; i < positions.Count; i++)
                    positions[i] = SnapToColumn(window, positions[i] + shift);
                return;
            }

            var order = new int[positions.Count];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
            Array.Sort(positions.ToArray(), order);
            for (int k = 0; k < order.Length; k++)
                positions[order[k]] = SnapToColumn(window, window.Start + window.Span * (k + 1) / (order.Length + 1));
        }

        /// <summary>
        /// The middle of the column <paramref name="position"/> falls in, within the window.
        /// </summary>
        /// <remarks>
        /// The middle rather than the start, so that the column it is read from does not change with a
        /// window whose start is not a whole number of columns from it.
        /// </remarks>
        static long SnapToColumn(DisplayWindow window, long position)
        {
            var column = Math.Max(0, Math.Min(window.Columns - 1, window.ColumnOf(position)));
            return window.PositionOf(column) + window.Step / 2;
        }

        /// <summary>
        /// Shades the row of the channel the cursors read.
        /// </summary>
        /// <remarks>
        /// Drawn ahead of the traces, so the selection is marked without anything drawn over the trace. Not in the
        /// heatmap, which covers it, and where the values written across the row mark it instead.
        /// </remarks>
        public void DrawRowBand(ImDrawListPtr draw, in PlotFrame frame)
        {
            var channel = Channel(frame.Expanded);
            if (channel < 0 || frame.Expanded >= 0 || frame.Heatmap)
                return;

            var top = MathF.Floor(frame.Layout.RowTop(channel));
            draw.AddRectFilled(new Vector2(frame.Left, top),
                new Vector2(frame.Left + frame.Width, top + frame.Layout.RowHeight), ColSelectedRow);
        }

        /// <summary>
        /// Moves a cursor by dragging its line, selects the channel clicked on, steps the selection with
        /// Q and E, and draws the cursors.
        /// </summary>
        public void Draw(ImDrawListPtr draw, in PlotFrame frame, int hovered)
        {
            if (!Show)
                return;

            var window = frame.Window;
            var hidden = frame.Hidden;
            var layout = frame.Layout;
            var (left, width, top, bottom) = (frame.Left, frame.Width, frame.Top, frame.Bottom);
            if (positions.Count == 0)
                positions.Add(SnapToColumn(window, window.PositionOf(window.Columns / 2)));

            if (selectedChannel < 0 || selectedChannel >= hidden.Length)
                selectedChannel = layout.FirstVisible;

            var pointer = ImGui.GetMousePos();
            var inPlot = ImGui.IsWindowHovered() &&
                pointer.X >= left && pointer.X < left + width &&
                pointer.Y >= top && pointer.Y < bottom;

            if (grabbed >= 0)
            {
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    grabbed = -1;
                else
                {
                    // NB: clamped to the plot, so a cursor dragged past an edge stops at it.
                    var fraction = Math.Max(0, Math.Min(1, (pointer.X - left) / width));
                    positions[grabbed] = SnapToColumn(window, window.Start + (long)(fraction * window.Span));
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                }
            }
            else if (inPlot)
            {
                var near = -1;
                var nearest = GrabDistance;
                for (int i = 0; i < positions.Count; i++)
                {
                    var distance = Math.Abs(pointer.X - X(frame, positions[i]));
                    if (distance <= nearest)
                    {
                        near = i;
                        nearest = distance;
                    }
                }

                var clicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGuiLfpViewerPanel.Modifiers();
                if (near >= 0)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                    if (clicked)
                        grabbed = near;
                }
                else if (clicked && hovered >= 0 && !hidden[hovered])
                {
                    selectedChannel = hovered;
                }
            }

            var step = !ImGuiLfpViewerPanel.Modifiers() || frame.Expanded >= 0 ? 0
                : ImGuiLfpViewerPanel.HotkeyPressed(ImGuiKey.Q) ? -1
                : ImGuiLfpViewerPanel.HotkeyPressed(ImGuiKey.E) ? 1
                : 0;
            if (step != 0)
            {
                for (var c = selectedChannel + step; c >= 0 && c < hidden.Length; c += step)
                {
                    if (!hidden[c])
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

            for (int i = 0; i < positions.Count; i++)
            {
                var x = X(frame, positions[i]);
                if (x < left || x > left + width)
                    continue;

                // NB: the reference solid and the rest dotted, as on a scope.
                if (i == 0)
                    draw.AddRectFilled(new Vector2(MathF.Floor(x), top), new Vector2(MathF.Floor(x) + LineWeight, bottom), Color);
                else
                    DottedLine(draw, x, top, bottom);

                if (TryRead(frame, positions[i], out var min, out var max))
                    Values(draw, frame, min, max, x);
            }

            // NB: one row per cursor rather than packed, so the dimensions can never overlap however the
            // cursors are arranged. An expanded channel's name takes the first row.
            var row = ImGui.GetTextLineHeight() + 2 * ImGui.GetStyle().FramePadding.Y + LabelGap;
            var first = top + LabelGap + (frame.Expanded >= 0 ? row : 0);
            for (int i = 1; i < positions.Count; i++)
                Dimension(draw, frame, positions[0], positions[i], first + (i - 1) * row);
        }

        /// <summary>
        /// A dimension from <paramref name="from"/> to <paramref name="to"/> at height <paramref name="y"/>,
        /// with arrowheads on the cursor lines and the time between them and its inverse in the middle.
        /// </summary>
        static void Dimension(ImDrawListPtr draw, in PlotFrame frame, long from, long to, float y)
        {
            var samples = Math.Abs(to - from);
            if (samples == 0)
                return;

            var left = frame.Left;
            var right = left + frame.Width;
            var a = X(frame, Math.Min(from, to));
            var b = X(frame, Math.Max(from, to));
            if (b < left || a > right)
                return;

            var pad = ImGui.GetStyle().FramePadding;
            var center = y + pad.Y + ImGui.GetTextLineHeight() / 2;
            var start = Math.Max(left, a);
            var end = Math.Min(right, b);

            // NB: the line breaks around the text, as a drawing's dimension does, rather than the text sitting on it
            // in a box.
            var text = $"{PlotText.Significant(samples * 1000.0 / frame.SampleRate)} ms " +
                $"({PlotText.Significant(frame.SampleRate / (double)samples)} Hz)";
            var textSize = ImGui.CalcTextSize(text);
            var textLeft = Math.Max(left + pad.X, Math.Min(right - pad.X - textSize.X, (start + end - textSize.X) / 2));
            var line = MathF.Floor(center);
            if (textLeft - pad.X > start)
                draw.AddRectFilled(new Vector2(start, line), new Vector2(textLeft - pad.X, line + LineWeight), Color);
            if (textLeft + textSize.X + pad.X < end)
                draw.AddRectFilled(new Vector2(textLeft + textSize.X + pad.X, line), new Vector2(end, line + LineWeight), Color);
            draw.AddText(new Vector2(textLeft, center - textSize.Y / 2), Color, text);

            // NB: an arrowhead only where the dimension reaches its cursor, not where the plot edge cuts it.
            if (a >= left)
                draw.AddTriangleFilled(new Vector2(a, center),
                    new Vector2(a + Arrow, center - Arrow / 2), new Vector2(a + Arrow, center + Arrow / 2), Color);
            if (b <= right)
                draw.AddTriangleFilled(new Vector2(b, center),
                    new Vector2(b - Arrow, center + Arrow / 2), new Vector2(b - Arrow, center - Arrow / 2), Color);
        }

        /// <summary>
        /// The min and max of the column at <paramref name="position"/> on the channel the cursors read.
        /// </summary>
        /// <remarks>
        /// Read from the column that is drawn, so the values are those of the trace on screen. In the heatmap both
        /// are the one value its color shows, whichever of them lies further from zero.
        /// </remarks>
        /// <returns>False if no channel can be read, or the column is off the window or holds no data.</returns>
        bool TryRead(in PlotFrame frame, long position, out double min, out double max)
        {
            min = max = double.NaN;
            var channel = Channel(frame.Expanded);
            var column = frame.Window.ColumnOf(position);
            if (channel < 0 || frame.Hidden[channel] && channel != frame.Expanded ||
                column < 0 || column >= frame.WaveformMin.Cols)
                return false;

            min = frame.WaveformMin.GetReal(channel, column);
            max = frame.WaveformMax.GetReal(channel, column);
            if (Heatmap(frame))
                min = max = Math.Abs(max) >= Math.Abs(min) ? max : min;
            return !double.IsNaN(min) && !double.IsNaN(max);
        }

        /// <summary>
        /// Marks where a cursor crosses the channel's trace and labels the value there: the column's max above
        /// and its min below, or the one sample when the column holds only one. In the heatmap, which shows no
        /// trace to mark, the one value is labeled level with the middle of the row.
        /// </summary>
        void Values(ImDrawListPtr draw, in PlotFrame frame, double min, double max, float x)
        {
            var layout = frame.Layout;
            var center = layout.RowTop(Channel(frame.Expanded)) + layout.RowHeight / 2;
            var scale = layout.RowHeight / frame.Range;
            var right = frame.Left + frame.Width;
            float ScreenY(double value) => center - (float)(value * scale);

            if (Heatmap(frame))
            {
                Value(draw, new Vector2(x, center), max, frame.Unit, 0, right, marker: false);
                return;
            }

            if (min == max)
            {
                Value(draw, new Vector2(x, ScreenY(max)), max, frame.Unit, 0, right);
                return;
            }

            Value(draw, new Vector2(x, ScreenY(max)), max, frame.Unit, -1, right);
            Value(draw, new Vector2(x, ScreenY(min)), min, frame.Unit, 1, right);
        }

        /// <summary>
        /// A marker at <paramref name="at"/>, labeled beside the cursor line: above the marker for
        /// <paramref name="side"/> -1, below it for 1, level with it for 0.
        /// </summary>
        static void Value(
            ImDrawListPtr draw, Vector2 at, double value, string unit, int side, float right, bool marker = true)
        {
            if (marker)
                draw.AddCircleFilled(at, MarkerRadius, Color);

            var text = $"{PlotText.Significant(value)} {unit}";
            var size = ImGui.CalcTextSize(text);
            var x = at.X + LabelGap;
            if (x + size.X + ImGui.GetStyle().FramePadding.X > right)
                x = at.X - LabelGap - size.X;

            var y = side < 0 ? at.Y - LabelGap - size.Y
                : side > 0 ? at.Y + LabelGap
                : at.Y - size.Y / 2;

            PlotText.Framed(draw, new Vector2(x, y), text);
        }

        /// <summary>
        /// Labels each cursor with its time in the row of time axis labels.
        /// </summary>
        /// <remarks>
        /// Boxed, the reference filled, and drawn over the axis labels on the window's own background with a margin
        /// either side, so that it takes the place of any axis label close enough to collide with it.
        /// </remarks>
        public void DrawTimeLabels(ImDrawListPtr draw, in PlotFrame frame, float labelY)
        {
            if (!Show)
                return;

            var left = frame.Left;
            var width = frame.Width;
            var pad = ImGui.GetStyle().FramePadding;
            var margin = new Vector2(pad.X + ImGui.GetStyle().ItemSpacing.X, pad.Y);
            var background = ImGui.GetColorU32(ImGuiCol.WindowBg);
            for (int i = 0; i < positions.Count; i++)
            {
                var x = X(frame, positions[i]);
                if (x < left || x > left + width)
                    continue;

                var text = $"C{i}: {FormatSeconds(frame, ImGuiLfpViewerPanel.SecondsAt(frame, positions[i]))} s";
                var size = ImGui.CalcTextSize(text);
                var corner = new Vector2(
                    Math.Max(left + pad.X, Math.Min(left + width - pad.X - size.X, x - size.X / 2)), labelY);
                draw.AddRectFilled(corner - margin, corner + size + margin, background);
                if (i == 0)
                {
                    draw.AddRectFilled(corner - pad, corner + size + pad, Color);
                    draw.AddText(corner, ImGuiPalette.Black, text);
                }
                else
                {
                    draw.AddRect(corner - pad, corner + size + pad, Color);
                    draw.AddText(corner, Color, text);
                }
            }
        }

        // NB: to the width of a column, which is as fine as a cursor can be placed.
        static string FormatSeconds(in PlotFrame frame, double seconds) =>
            seconds.ToString("F" + Math.Max(0, (int)Math.Ceiling(-Math.Log10(frame.Window.Step / (double)frame.SampleRate))));

        /// <summary>
        /// The cursors' readings on the selected channel, in a table over the plot's lower left corner, with
        /// buttons to add and remove cursors.
        /// </summary>
        /// <remarks>
        /// Times are read only. Right of the sweep cursor on a paused view, two places on the axis carry the
        /// same time, so a typed time would not name one place. A cursor is moved by dragging its line.
        /// </remarks>
        /// <returns>How far to pan back through history to center a cursor whose button was pressed, or null.</returns>
        public long? DrawTable(in PlotFrame frame)
        {
            if (!Show || positions.Count == 0)
                return null;

            long? pan = null;
            var window = frame.Window;
            var sampleRate = frame.SampleRate;
            var unit = frame.Unit;
            var gap = ImGui.GetStyle().ItemSpacing;
            ImGui.SetCursorScreenPos(new Vector2(frame.Left + gap.X, frame.Bottom - tableSize.Y - gap.Y));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, PlotText.LabelBackground);

            // NB: the plot's region is opened with no vertical padding, which this window would inherit.
            var padding = ImGui.GetStyle().WindowPadding.X;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding, padding));
            var childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY |
                ImGuiChildFlags.AlwaysAutoResize;
            if (ImGui.BeginChild("##cursorTable", Vector2.Zero, childFlags, ImGuiWindowFlags.NoScrollbar))
            {
                var channel = Channel(frame.Expanded);
                ImGui.TextUnformatted(channel >= 0 ? $"Ch {channel}" : "No channel");

                var remove = -1;
                // NB: a column of one sample has min and max equal, so it has one value and one difference from C0,
                // as does the heatmap, which reduces a column to one value.
                var oneSample = window.Step == 1 || Heatmap(frame);
                var headers = oneSample
                    ? new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"y ({unit})", $"dy ({unit})", "" }
                    : new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"min ({unit})", $"max ({unit})", $"dy min ({unit})", $"dy max ({unit})", "" };
                if (ImGui.BeginTable("##readings", headers.Length, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerV))
                {
                    foreach (var header in headers)
                        ImGui.TableSetupColumn(header);
                    ImGui.TableHeadersRow();

                    TryRead(frame, positions[0], out var min0, out var max0);
                    for (int i = 0; i < positions.Count; i++)
                    {
                        var read = TryRead(frame, positions[i], out var min, out var max);
                        var samples = positions[i] - positions[0];

                        ImGui.TableNextRow();

                        // NB: only paused, since live the window does not move. Home brings a cursor back instead.
                        ImGui.TableNextColumn();
                        ImGui.BeginDisabled(!frame.Paused);
                        if (ImGui.SmallButton($"C{i}"))
                            pan = window.Start + window.Span / 2 - positions[i];
                        ImGui.EndDisabled();

                        Cell(FormatSeconds(frame, ImGuiLfpViewerPanel.SecondsAt(frame, positions[i])));
                        Cell(i > 0 ? $"{PlotText.Significant(samples * 1000.0 / sampleRate)}" : "");
                        Cell(i > 0 && samples != 0 ? $"{PlotText.Significant(sampleRate / (double)Math.Abs(samples))}" : "");
                        Cell(read ? $"{PlotText.Significant(min)}" : "");
                        if (!oneSample)
                            Cell(read ? $"{PlotText.Significant(max)}" : "");

                        // NB: the signal at each cursor lies somewhere in its column's range, so its difference
                        // from C0's lies between the two extremes, which meet at one sample per column.
                        var differs = i > 0 && read && !double.IsNaN(min0);
                        Cell(differs ? $"{PlotText.Significant(min - max0)}" : "");
                        if (!oneSample)
                            Cell(differs ? $"{PlotText.Significant(max - min0)}" : "");

                        ImGui.TableNextColumn();
                        if (i > 0 && ImGui.SmallButton($"x##remove{i}"))
                            remove = i;
                    }

                    ImGui.EndTable();
                }

                if (remove > 0)
                    positions.RemoveAt(remove);

                ImGui.BeginDisabled(positions.Count >= MaxCursors);
                if (ImGui.SmallButton("+ Add cursor"))
                    positions.Add(SnapToColumn(window, window.PositionOf(window.Columns / 2)));
                ImGui.EndDisabled();

                ImGui.SameLine();
                if (ImGui.SmallButton("Home"))
                    Home(window);
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            tableSize = ImGui.GetItemRectSize();
            return pan;

            static void Cell(string text)
            {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(text);
            }
        }

        // NB: an expanded channel is drawn as a trace in either view.
        static bool Heatmap(in PlotFrame frame) => frame.Heatmap && frame.Expanded < 0;

        static float X(in PlotFrame frame, long position) =>
            frame.Left + (float)(frame.Width * frame.Window.FractionOf(position));

        static void DottedLine(ImDrawListPtr draw, float x, float top, float bottom)
        {
            x = MathF.Floor(x);
            for (var y = MathF.Floor(top); y < bottom; y += 2 * Dot)
                draw.AddRectFilled(new Vector2(x, y), new Vector2(x + LineWeight, Math.Min(y + Dot, bottom)), Color);
        }
    }
}
