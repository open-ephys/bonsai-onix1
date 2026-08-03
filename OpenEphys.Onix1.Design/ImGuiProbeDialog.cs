using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Abstract Form base for ImGui-rendered probe-configuration dialogs. Owns the GL control, the render
    /// timer, the three-panel layout (minimap | zoomed view | properties), and an always-present,
    /// collapsible, vertically resizable bottom panel that is the dialog's console log. Subclasses supply
    /// the properties panel content via <see cref="DrawPropsPanel"/> and append log lines via
    /// <see cref="Log"/>.
    /// </summary>
    internal abstract class ImGuiProbeDialog : Form, IProbeInterfaceDialog
    {
        /// <summary>
        /// One line of the console log at the bottom of the dialog.
        /// </summary>
        internal struct LogEntry
        {
            internal DateTimeOffset When;
            internal string Msg;
            internal bool IsError;
        }

        // GL state and rendering timer
        readonly ImGuiGLControl glControl;
        readonly Timer renderTimer;

        // Console log state
        readonly List<LogEntry> log = new();
        HashSet<int> selectedLogLines = new();
        int lastClickedLogIndex = -1;

        // Visual constants
        const uint ColorBottomPanelHeaderHovered = ImGuiPalette.Grey0x30;
        const uint ColorBottomPanelHeaderActive = ImGuiPalette.Grey0x4D;
        protected static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        /// <summary>
        /// Persisted height in pixels of the expanded bottom panel. This is the authoritative, user-resized
        /// vertical quantity. Its size stays fixed across window resizes (like a typical fixed-height
        /// console/status panel). The top wrapper (minimap | zoomed view | properties) is derived each frame
        /// to fill whatever space remains.
        /// </summary>
        float bottomPanelHeight = 200f;

        /// <summary>
        /// Persisted collapsed/expanded state of the bottom panel
        /// .</summary>
        bool bottomPanelOpen = true;

        /// <summary>
        /// <see cref="ImGui.GetContentRegionAvail"/>'s Y from the previous frame, used to detect when the
        /// Form itself was resized (as opposed to the splitter being dragged).
        /// </summary>
        float lastAvailY = -1f;

        /// <summary>
        /// Persisted pixel height of the separator + collapsing header row, measured from the previous frame
        /// so the top panels' height can be reserved exactly (avoids clipping from guessed constants).
        /// </summary>
        float bottomFooterHeaderHeight = 30f;

        /// <summary>
        /// The probe selector component shared by all subclasses.
        /// </summary>
        protected readonly ImGuiProbeSelector selector = new();

        /// <summary>
        /// Pixel width of the props panel on the right side of the dialog.
        /// </summary>
        protected virtual float PropsWidth => 500f;

        /// <summary>
        /// When false, the selector suppresses drag-select interaction.
        /// </summary>
        protected virtual bool SelectionEnabled => true;

        protected ImGuiProbeDialog()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;

            glControl = new ImGuiGLControl { Dock = DockStyle.Fill };
            glControl.Render += RenderFrame;
            Controls.Add(glControl);

            renderTimer = new Timer { Interval = 16 };
            renderTimer.Tick += (_, _) => glControl.Invalidate();

            Shown += (_, _) => renderTimer.Start();
            FormClosing += OnFormClosingHandler;
        }

        /// <summary>
        /// Draw the contents of the right-hand props panel each frame.
        /// </summary>
        protected abstract void DrawPropsPanel();

        /// <summary>
        /// Appends a line to the console log.
        /// </summary>
        protected void Log(string message, bool isError = false) =>
            log.Add(new LogEntry { When = DateTimeOffset.UtcNow, Msg = message, IsError = isError });

        /// <summary>
        /// Called when the form is about to close. Set <c>e.Cancel = true</c> to abort the
        /// close (the render timer is restarted automatically in that case).
        /// </summary>
        protected virtual void OnClosing(FormClosingEventArgs e) { }

        /// <inheritdoc/>
        public virtual bool HasChanges { get => false; protected set { } }

        /// <inheritdoc/>
        public virtual event EventHandler OnStateChange { add { } remove { } }

        /// <inheritdoc/>
        public virtual bool ProcessMenuShortcut(Keys keyData) => false;

        void RenderFrame(object sender, EventArgs e)
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new Vector2(glControl.Width, glControl.Height);

            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);
            ImGui.Begin("##root",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar  | ImGuiWindowFlags.NoScrollWithMouse);

            float availY = ImGui.GetContentRegionAvail().Y;

            // bottomPanelHeight is authoritative and fixed across resizes; the wrapper fills
            // whatever's left. ResizeY ignores BeginChild's size argument after first use, so an
            // Always-priority SetNextWindowSize is needed to force a correction. But that also
            // strips ResizeY (disabling the drag grip), so only force it on frames the Form
            // itself actually resizes, not every frame.
            bool formResized = Math.Abs(availY - lastAvailY) > 0.5f;
            lastAvailY = availY;

            // Separate IDs for collapsed vs. expanded: reusing one ID would let the collapsed
            // fill-size overwrite the expanded wrapper's remembered size, so re-expanding would
            // come back collapsed instead of returning to bottomPanelHeight.
            var topFlags = bottomPanelOpen ? ImGuiChildFlags.ResizeY : ImGuiChildFlags.None;
            string topWrapperId = bottomPanelOpen ? "##topWrapperExpanded" : "##topWrapperCollapsed";
            float topTarget;
            if (bottomPanelOpen)
            {
                topTarget = Math.Max(100f, availY - bottomPanelHeight - bottomFooterHeaderHeight);
                if (formResized)
                    ImGui.SetNextWindowSize(new Vector2(-1, topTarget), ImGuiCond.Always);
            }
            else
            {
                topTarget = availY - bottomFooterHeaderHeight;
            }

            ImGui.BeginChild(topWrapperId, new Vector2(-1, topTarget), topFlags,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            float topOriginScreenY = ImGui.GetCursorScreenPos().Y;
            float topH = ImGui.GetContentRegionAvail().Y;
            float topAvailW = ImGui.GetContentRegionAvail().X;
            float minimapColW = selector.ComputeMinimapColumnWidth(topH, topAvailW);
            float sameLineGaps = 2f * ImGui.GetStyle().ItemSpacing.X; // minimap<->zoomed and zoomed<->props
            float zoomedW = topAvailW - minimapColW - PropsWidth - sameLineGaps;

            selector.UpdateLayout(topH, zoomedW);

            ImGui.BeginChild("##minimapFcol", new Vector2(minimapColW, topH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            selector.DrawMinimap(topH);
            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.BeginChild("##zoomed", new Vector2(zoomedW, topH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            selector.DrawZoomedView(topH, SelectionEnabled);
            ImGui.EndChild();
            ImGui.SameLine();

            float propsLeftScreenX = ImGui.GetCursorScreenPos().X;

            ImGui.BeginChild("##props", new Vector2(PropsWidth, topH), ImGuiChildFlags.Borders,
                ImGuiWindowFlags.AlwaysVerticalScrollbar);
            DrawPropsPanel();
            ImGui.EndChild();

            if (bottomPanelOpen)
            {
                float actualTopH = ImGui.GetWindowSize().Y;
                bottomPanelHeight = Math.Max(60f,
                    Math.Min(availY - bottomFooterHeaderHeight - 100f, availY - actualTopH - bottomFooterHeaderHeight));
            }
            ImGui.EndChild();

            float footerStartY = ImGui.GetCursorPosY();
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.Header, ImGuiPalette.Black);
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, ColorBottomPanelHeaderHovered);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, ColorBottomPanelHeaderActive);
            bool wasOpen = bottomPanelOpen;
            bottomPanelOpen = ImGui.CollapsingHeader("Log##bottomPanelHeader", ImGuiTreeNodeFlags.DefaultOpen);
            ImGui.PopStyleColor(3);
            if (wasOpen && !bottomPanelOpen)
            {
                selectedLogLines.Clear();
                lastClickedLogIndex = -1;
            }

            bottomFooterHeaderHeight = ImGui.GetCursorPosY() - footerStartY;

            if (bottomPanelOpen)
            {
                ImGui.BeginChild("##bottomPanelChild", new Vector2(-1, -1), ImGuiChildFlags.Borders);
                DrawLog();
                ImGui.EndChild();
            }

            selector.HandleScrollInput(topOriginScreenY + topH, propsLeftScreenX);
            ImGui.End();
        }

        void OnFormClosingHandler(object sender, FormClosingEventArgs e)
        {
            renderTimer.Stop();
            OnClosing(e);
            if (e.Cancel) renderTimer.Start();
        }

        void DrawLog()
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
                    if (entry.IsError) ImGui.TextColored(ColorTextError, entry.Msg);
                    else               ImGui.TextUnformatted(entry.Msg);
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
