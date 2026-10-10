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
    /// pausing. One cursor is the reference the others are measured from, C0 to begin with; clicking a cursor's
    /// time label makes it the reference, and the reference cannot be removed. The cursors keep only their own state; everything about the
    /// plot comes in with each frame.
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

        readonly List<long> positions = new(MaxCursors);
        int reference;
        int grabbed = -1;
        Vector2 tableSize;

        /// <summary>
        /// Whether the cursors are shown. While they are, a cursor's line can be dragged.
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// Whether a cursor's line is being dragged.
        /// </summary>
        public bool Dragging => grabbed >= 0;

        /// <summary>
        /// The channel the cursors read, or -1 while they are hidden: the selected channel, or an expanded
        /// channel, which is the only one there is to read.
        /// </summary>
        public int Channel(in PlotFrame frame) => !Show ? -1 : frame.Expanded >= 0 ? frame.Expanded : frame.Selected;

        /// <summary>
        /// Moves the cursors to the middle of the window <paramref name="axis"/> shows, keeping their spacing, or
        /// spreads them evenly across it in the same order if they span more than it does.
        /// </summary>
        public void Home(in PixelAxis axis)
        {
            var window = axis.Window;
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
                    positions[i] = axis.Snap(positions[i] + shift);
                return;
            }

            var order = new int[positions.Count];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;
            Array.Sort(positions.ToArray(), order);
            for (int k = 0; k < order.Length; k++)
                positions[order[k]] = axis.Snap(window.Start + window.Span * (k + 1) / (order.Length + 1));
        }

        /// <summary>
        /// Moves a cursor by dragging its line, and draws the cursors.
        /// </summary>
        public void Draw(ImDrawListPtr draw, in PlotFrame frame)
        {
            if (!Show)
                return;

            var axis = frame.Axis;
            var (left, width, top, bottom) = (frame.Left, frame.Width, frame.Top, frame.Bottom);
            if (positions.Count == 0)
                positions.Add(axis.Snap(axis.Window.Start + axis.Window.Span / 2));

            var pointer = ImGui.GetMousePos();
            var inPlot = ImGui.IsWindowHovered() && !ImGui.IsAnyItemHovered() &&
                pointer.X >= left && pointer.X < left + width &&
                pointer.Y >= top && pointer.Y < bottom;

            if (grabbed >= 0)
            {
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    grabbed = -1;
                else
                {
                    // NB: clamped to the plot, so a cursor dragged past an edge stops at it.
                    positions[grabbed] = axis.PositionAt(pointer.X);
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                }
            }
            else if (inPlot)
            {
                var near = -1;
                var nearest = GrabDistance;
                for (int i = 0; i < positions.Count; i++)
                {
                    var distance = Math.Abs(pointer.X - axis.X(positions[i]));
                    if (distance <= nearest)
                    {
                        near = i;
                        nearest = distance;
                    }
                }

                if (near >= 0)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGuiLfpViewerPanel.Modifiers())
                        grabbed = near;
                }
            }

            for (int i = 0; i < positions.Count; i++)
            {
                var x = axis.X(positions[i]);
                if (x < left || x > left + width)
                    continue;

                // NB: the reference solid and the rest dotted, as on a scope.
                if (i == reference)
                    draw.AddRectFilled(new Vector2(MathF.Floor(x), top), new Vector2(MathF.Floor(x) + LineWeight, bottom), Color);
                else
                    DottedLine(draw, x, top, bottom);

                if (TryRead(frame, positions[i], out var min, out var max))
                    Values(draw, frame, min, max, x);
            }

            // NB: one row per cursor rather than packed, so the dimensions can never overlap however the
            // cursors are arranged. An expanded channel's name takes the first row.
            var row = ImGui.GetTextLineHeight() + 2 * ImGui.GetStyle().FramePadding.Y + LabelGap;
            var y = top + LabelGap + (frame.Expanded >= 0 ? row : 0);
            for (int i = 0; i < positions.Count; i++)
            {
                if (i == reference)
                    continue;

                Dimension(draw, frame, positions[reference], positions[i], y);
                y += row;
            }
        }

        /// <summary>
        /// A dimension from the reference at <paramref name="from"/> to the cursor at <paramref name="to"/> at height
        /// <paramref name="y"/>, with an arrowhead on that cursor's line and the time between them and its inverse in
        /// the middle.
        /// </summary>
        static void Dimension(ImDrawListPtr draw, in PlotFrame frame, long from, long to, float y)
        {
            var samples = Math.Abs(to - from);
            if (samples == 0)
                return;

            var left = frame.Left;
            var right = left + frame.Width;
            var a = frame.Axis.X(Math.Min(from, to));
            var b = frame.Axis.X(Math.Max(from, to));
            if (b < left || a > right)
                return;

            var pad = ImGui.GetStyle().FramePadding;
            var center = y + pad.Y + ImGui.GetTextLineHeight() / 2;
            var start = Math.Max(left, a);
            var end = Math.Min(right, b);

            // NB: the line breaks around the text, as a drawing's dimension does, and the text sits on a dark fill so
            // that it reads over the cursor lines and graticules it crosses. No outline, which the break makes busy.
            var text = $"{PlotText.Significant(samples * 1000.0 / frame.SampleRate)} ms " +
                $"({PlotText.Significant(frame.SampleRate / (double)samples)} Hz)";
            var textSize = ImGui.CalcTextSize(text);
            var textLeft = Math.Max(left + pad.X, Math.Min(right - pad.X - textSize.X, (start + end - textSize.X) / 2));
            var line = MathF.Floor(center);
            if (textLeft - pad.X > start)
                draw.AddRectFilled(new Vector2(start, line), new Vector2(textLeft - pad.X, line + LineWeight), Color);
            if (textLeft + textSize.X + pad.X < end)
                draw.AddRectFilled(new Vector2(textLeft + textSize.X + pad.X, line), new Vector2(end, line + LineWeight), Color);
            PlotText.Framed(draw, new Vector2(textLeft, center - textSize.Y / 2), text, outline: false);

            // NB: one arrowhead, at the measured cursor rather than the reference, so the dimension reads from the
            // reference out to it. None where the plot edge cuts the dimension short of that cursor.
            if (to < from && a >= left)
                draw.AddTriangleFilled(new Vector2(a, center),
                    new Vector2(a + Arrow, center - Arrow / 2), new Vector2(a + Arrow, center + Arrow / 2), Color);
            if (to > from && b <= right)
                draw.AddTriangleFilled(new Vector2(b, center),
                    new Vector2(b - Arrow, center + Arrow / 2), new Vector2(b - Arrow, center - Arrow / 2), Color);
        }

        /// <summary>
        /// The min and max drawn at <paramref name="position"/> on the channel the cursors read.
        /// </summary>
        /// <remarks>
        /// Read from the cell the screen shows there, every column a pixel combines or the one column a pixel
        /// shows, as the traces are drawn, so the values are those on screen. In the heatmap both are the one value
        /// its color shows, whichever of them lies further from zero.
        /// </remarks>
        /// <returns>False if no channel can be read, or the cell is off the window or holds no data.</returns>
        bool TryRead(in PlotFrame frame, long position, out double min, out double max)
        {
            min = max = double.NaN;
            var channel = Channel(frame);
            var (start, end) = frame.Axis.CellColumns(position);
            if (channel < 0 || frame.Hidden[channel] && channel != frame.Expanded || end <= start)
                return false;

            min = double.PositiveInfinity;
            max = double.NegativeInfinity;
            for (int c = start; c < end; c++)
            {
                min = Math.Min(min, frame.Envelope.Min.GetReal(channel, c));
                max = Math.Max(max, frame.Envelope.Max.GetReal(channel, c));
            }

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
            var center = layout.RowTop(Channel(frame)) + layout.RowHeight / 2;
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
        /// Labels each cursor with its time in the row of time axis labels, and makes a cursor whose label is
        /// clicked the reference.
        /// </summary>
        /// <remarks>
        /// Boxed, the reference filled, and drawn over the axis labels on the window's own background with a margin
        /// either side, so that it takes the place of any axis label close enough to collide with it.
        /// </remarks>
        public void DrawTimeLabels(ImDrawListPtr draw, in PlotFrame frame, float labelY)
        {
            if (!Show)
                return;

            // NB: hit-tested here rather than with a button per label, so that where labels overlap only the one
            // drawn on top, the last, answers the pointer.
            var pad = ImGui.GetStyle().FramePadding;
            var pointer = ImGui.GetMousePos();
            var hovered = -1;
            for (int i = 0; ImGui.IsWindowHovered() && i < positions.Count; i++)
            {
                if (TryPlaceLabel(frame, i, labelY, out _, out var corner, out var size) &&
                    pointer.X >= corner.X - pad.X && pointer.X < corner.X + size.X + pad.X &&
                    pointer.Y >= corner.Y - pad.Y && pointer.Y < corner.Y + size.Y + pad.Y)
                {
                    hovered = i;
                }
            }

            if (hovered >= 0 && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                reference = hovered;

            var margin = new Vector2(pad.X + ImGui.GetStyle().ItemSpacing.X, pad.Y);
            var background = ImGui.GetColorU32(ImGuiCol.WindowBg);
            for (int i = 0; i < positions.Count; i++)
            {
                if (!TryPlaceLabel(frame, i, labelY, out var text, out var corner, out var size))
                    continue;

                // NB: lit while hovered, as a button is, so that it shows it can be clicked.
                draw.AddRectFilled(corner - margin, corner + size + margin, background);
                if (i == hovered)
                    draw.AddRectFilled(corner - pad, corner + size + pad, ImGui.GetColorU32(ImGuiCol.ButtonHovered));
                else if (i == reference)
                    draw.AddRectFilled(corner - pad, corner + size + pad, Color);
                draw.AddRect(corner - pad, corner + size + pad, Color);
                draw.AddText(corner, i == reference && i != hovered ? ImGuiPalette.Black : Color, text);
            }
        }

        // NB: centered on the cursor's line, kept within the plot, or false where the cursor is off it.
        bool TryPlaceLabel(in PlotFrame frame, int i, float labelY, out string text, out Vector2 corner, out Vector2 size)
        {
            text = null;
            corner = size = default;
            var x = frame.Axis.X(positions[i]);
            if (x < frame.Left || x > frame.Left + frame.Width)
                return false;

            var pad = ImGui.GetStyle().FramePadding;
            text = $"C{i}: {FormatSeconds(frame, ImGuiLfpViewerPanel.SecondsAt(frame, positions[i]))} s";
            size = ImGui.CalcTextSize(text);
            corner = new Vector2(
                Math.Max(frame.Left + pad.X, Math.Min(frame.Left + frame.Width - pad.X - size.X, x - size.X / 2)), labelY);
            return true;
        }

        // NB: to the width of a cell, which is as fine as a cursor can be placed.
        static string FormatSeconds(in PlotFrame frame, double seconds) =>
            seconds.ToString("F" + Math.Max(0, (int)Math.Ceiling(-Math.Log10(frame.Axis.CellSpan / (double)frame.SampleRate))));

        /// <summary>
        /// The cursors' readings on the selected channel, in a table over the plot's lower left corner, with a
        /// button per cursor to center a paused view on it, which reaches a cursor whose line and label are off
        /// the plot, and buttons to add and remove cursors.
        /// </summary>
        /// <remarks>
        /// Times are read only. Right of the sweep cursor on a paused view, two places on the axis carry the
        /// same time, so a typed time would not name one place. A cursor is moved by dragging its line.
        /// </remarks>
        /// <returns>The position of a cursor whose button was pressed, to center the view on, or null.</returns>
        public long? DrawTable(in PlotFrame frame)
        {
            if (!Show || positions.Count == 0)
                return null;

            long? center = null;
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
                var channel = Channel(frame);
                ImGui.TextUnformatted(channel >= 0 ? $"Ch {channel}" : "No channel");

                var remove = -1;
                // NB: a cell of one sample has min and max equal, so it has one value and one difference from C0, as
                // does the heatmap, which reduces a cell to one value.
                var oneSample = window.Step == 1 && frame.Axis.SingleColumnCells || Heatmap(frame);
                var headers = oneSample
                    ? new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"y ({unit})", $"dy ({unit})", "" }
                    : new[] { "", "t (s)", "dt (ms)", "1/dt (Hz)", $"min ({unit})", $"max ({unit})", $"dy min ({unit})", $"dy max ({unit})", "" };
                if (ImGui.BeginTable("##readings", headers.Length, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerV))
                {
                    foreach (var header in headers)
                        ImGui.TableSetupColumn(header);
                    ImGui.TableHeadersRow();

                    TryRead(frame, positions[reference], out var min0, out var max0);
                    for (int i = 0; i < positions.Count; i++)
                    {
                        var read = TryRead(frame, positions[i], out var min, out var max);
                        var samples = positions[i] - positions[reference];
                        var measured = i != reference;

                        ImGui.TableNextRow();

                        // NB: only paused, since live the window does not move. The reference's button filled, as
                        // its time label is, so that both mark it alike.
                        ImGui.TableNextColumn();
                        ImGui.BeginDisabled(!frame.Paused);
                        if (!measured)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Button, Color);
                            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiPalette.Black);
                        }
                        if (ImGui.SmallButton($"C{i}"))
                            center = positions[i];
                        if (!measured)
                            ImGui.PopStyleColor(2);
                        ImGui.EndDisabled();

                        Cell(FormatSeconds(frame, ImGuiLfpViewerPanel.SecondsAt(frame, positions[i])));
                        Cell(measured ? $"{PlotText.Significant(samples * 1000.0 / sampleRate)}" : "");
                        Cell(measured && samples != 0 ? $"{PlotText.Significant(sampleRate / (double)Math.Abs(samples))}" : "");
                        Cell(read ? $"{PlotText.Significant(min)}" : "");
                        if (!oneSample)
                            Cell(read ? $"{PlotText.Significant(max)}" : "");

                        // NB: the signal at each cursor lies somewhere in its cell's range, so its difference from
                        // the reference's lies between the two extremes, which meet at one sample per cell.
                        var differs = measured && read && !double.IsNaN(min0);
                        Cell(differs ? $"{PlotText.Significant(min - max0)}" : "");
                        if (!oneSample)
                            Cell(differs ? $"{PlotText.Significant(max - min0)}" : "");

                        ImGui.TableNextColumn();
                        if (measured && ImGui.SmallButton($"x##remove{i}"))
                            remove = i;
                    }

                    ImGui.EndTable();
                }

                if (remove >= 0)
                {
                    positions.RemoveAt(remove);
                    if (remove < reference)
                        reference--;
                }

                ImGui.BeginDisabled(positions.Count >= MaxCursors);
                if (ImGui.SmallButton("+ Add cursor"))
                    positions.Add(frame.Axis.Snap(window.Start + window.Span / 2));
                ImGui.EndDisabled();

                ImGui.SameLine();
                if (ImGui.SmallButton("Home"))
                    Home(frame.Axis);
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            tableSize = ImGui.GetItemRectSize();
            return center;

            static void Cell(string text)
            {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(text);
            }
        }

        // NB: an expanded channel is drawn as a trace in either view.
        static bool Heatmap(in PlotFrame frame) => frame.Heatmap && frame.Expanded < 0;

        static void DottedLine(ImDrawListPtr draw, float x, float top, float bottom)
        {
            x = MathF.Floor(x);
            for (var y = MathF.Floor(top); y < bottom; y += 2 * Dot)
                draw.AddRectFilled(new Vector2(x, y), new Vector2(x + LineWeight, Math.Min(y + Dot, bottom)), Color);
        }
    }
}
