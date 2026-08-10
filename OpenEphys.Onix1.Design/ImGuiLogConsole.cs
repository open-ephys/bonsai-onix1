using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One line of an <see cref="ImGuiLogConsole"/>.
    /// </summary>
    internal struct LogEntry
    {
        internal DateTimeOffset When;
        internal string Msg;
        internal bool IsError;
    }

    /// <summary>
    /// A console log with timestamped lines, error coloring, right-click Copy Selected/Copy All,
    /// <c>Ctrl+C</c> copy, and auto-scroll-to-bottom (only while already at the bottom, so a manual scroll-up
    /// sticks instead of snapping back every frame).
    /// </summary>
    internal sealed class ImGuiLogConsole
    {
        static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        readonly List<LogEntry> log = new();
        readonly HashSet<int> selectedLogLines = new();
        int lastClickedLogIndex = -1;

        /// <summary>
        /// Appends a line to the console log.
        /// </summary>
        internal void Log(string message, bool isError = false) =>
            log.Add(new LogEntry { When = DateTimeOffset.UtcNow, Msg = message, IsError = isError });

        /// <summary>
        /// Clears the current line-selection state, e.g. when the hosting panel collapses the log view.
        /// </summary>
        internal void ClearSelection()
        {
            selectedLogLines.Clear();
            lastClickedLogIndex = -1;
        }

        /// <summary>
        /// Draws the log inside a self-contained, full-region scrollable child. Caller is responsible
        /// for sizing/bordering the outer region this fills (e.g. a `BeginChild`/`EndChild` pair).
        /// </summary>
        internal void Draw()
        {
            if (ImGui.BeginChild("##loginner", new Vector2(-1, -1), ImGuiChildFlags.None))
            {
                // Only auto-follow new lines if the view was already at the bottom before this
                // frame's content; otherwise a manual scroll (wheel or scrollbar drag) sticks
                // instead of snapping back every frame.
                bool wasAtBottom = ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 1f;

                int start = Math.Max(0, log.Count - 200);

                // Hover should look like the same muted fill used for a selected line (not the
                // theme's default bright-blue hover); a white outline distinguishes "hovering"
                // from "selected" instead.
                var mutedSelectColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Header];
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, mutedSelectColor);
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, mutedSelectColor);

                for (int i = start; i < log.Count; i++)
                {
                    var entry = log[i];
                    ImGui.PushID(i);

                    bool isSelected = selectedLogLines.Contains(i);

                    // Invisible selectable spanning full width, drawn under the  text
                    if (ImGui.Selectable("##sel", isSelected, ImGuiSelectableFlags.AllowOverlap))
                    {
                        bool ctrl = ImGui.GetIO().KeyCtrl;
                        bool shift = ImGui.GetIO().KeyShift;

                        if (shift && lastClickedLogIndex >= 0)
                        {
                            selectedLogLines.Clear();
                            int lo = Math.Min(lastClickedLogIndex, i);
                            int hi = Math.Max(lastClickedLogIndex, i);
                            for (int j = lo; j <= hi; j++) selectedLogLines.Add(j);
                        }
                        else if (ctrl)
                        {
                            if (!selectedLogLines.Add(i)) selectedLogLines.Remove(i);
                            lastClickedLogIndex = i;
                        }
                        else
                        {
                            selectedLogLines.Clear();
                            selectedLogLines.Add(i);
                            lastClickedLogIndex = i;
                        }
                    }

                    // Right-click context menu (also selects the line if not already selected)
                    if (ImGui.BeginPopupContextItem("##ctx"))
                    {
                        if (!selectedLogLines.Contains(i))
                        {
                            selectedLogLines.Clear();
                            selectedLogLines.Add(i);
                        }
                        if (ImGui.MenuItem("Copy Selected"))
                        {
                            var text = string.Join("\n", selectedLogLines.OrderBy(x => x).Select(idx => log[idx].Msg));
                            ImGui.SetClipboardText(text);
                        }
                        if (ImGui.MenuItem("Copy All"))
                        {
                            ImGui.SetClipboardText(string.Join("\n", log.Select(l => l.Msg)));
                        }
                        ImGui.EndPopup();
                    }

                    ImGui.PopID();

                    ImGui.SameLine(0, 0);
                    ImGui.TextDisabled(entry.When.ToLocalTime().ToString("HH:mm:ss"));
                    ImGui.SameLine();
                    if (entry.IsError)
                    {
                        // NB: TextColored (like Text) treats its string as a printf format -- entry.Msg is
                        // arbitrary (e.g. exception text) and may contain '%', so push the color instead of
                        // using TextColored directly.
                        ImGui.PushStyleColor(ImGuiCol.Text, ColorTextError);
                        ImGui.TextUnformatted(entry.Msg);
                        ImGui.PopStyleColor();
                    }
                    else ImGui.TextUnformatted(entry.Msg);
                }

                ImGui.PopStyleColor(2);

                if (wasAtBottom)
                    ImGui.SetScrollHereY(1.0f);

                if (ImGui.IsWindowFocused() && ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.C, false) && selectedLogLines.Count > 0)
                {
                    var text = string.Join("\n", selectedLogLines.OrderBy(x => x).Select(idx => log[idx].Msg));
                    ImGui.SetClipboardText(text);
                }

                if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered())
                    selectedLogLines.Clear();
            }

            ImGui.EndChild();
        }
    }
}
