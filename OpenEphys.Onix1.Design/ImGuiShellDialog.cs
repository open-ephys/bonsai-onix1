using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using OpenEphys.EphysVisualization.Design.WinForms;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// A Form hosting a single shared ImGui GL context. Hosts one or more <see
    /// cref="IImGuiTabPanel"/>s registered via <see cref="AddTab"/>: with exactly one, its content fills the
    /// window directly (no visible tab bar); with two or more, they're drawn inside an ImGui tab bar, with
    /// only the active tab's content updated each frame. Used directly for standalone single-panel editing
    /// and as a base for multi-tab headstage dialogs.
    /// </summary>
    internal class ImGuiShellDialog : Form
    {
        readonly ImGuiGLControl glControl;
        readonly List<(string Name, IImGuiTabPanel Panel)> tabs = new();

        IImGuiTabPanel sidePanel;
        float sidePanelWidth;
        string sidePanelTitle;
        bool sidePanelCollapsed;
        const float SidePanelCollapsedWidth = 28f;

        /// <summary>
        /// Console log. Pass this to relevant sources to feed the log. 
        /// </summary>
        internal ImGuiLogConsole Log { get; } = new();

        bool logPanelOpen = true;
        float logPanelHeight = 200f;
        float logFooterHeaderHeight = 30f;
        float lastAvailY = -1f;

        /// <summary>
        /// Initializes a new instance of <see cref="ImGuiShellDialog"/>.
        /// </summary>
        internal ImGuiShellDialog(string title)
        {
            Icon = Properties.Resources.OpenEphysIcon;

            Text = title;
            ClientSize = new Size(1334, 811);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(144, 32),
                UseVisualStyleBackColor = true
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(144, 32),
                UseVisualStyleBackColor = true
            };

            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(4)
            };
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(okButton);

            var layout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            glControl = new ImGuiGLControl { Dock = DockStyle.Fill };
            glControl.Render += RenderFrame;

            layout.Controls.Add(glControl, 0, 0);
            layout.Controls.Add(buttons, 0, 1);
            Controls.Add(layout);

            // NB: no AcceptButton, because Enter must commit an ImGui text field without closing the dialog. The GL
            // control takes Esc for ImGui, so CancelButton only covers Esc while a button has focus. DrawContent
            // handles it otherwise.
            CancelButton = cancelButton;

            FormClosing += ShellClosing;
            FormClosed += ShellClosed;
        }

        /// <summary>
        /// Registers <paramref name="panel"/> as a new tab labeled <paramref name="tabName"/>.
        /// </summary>
        internal void AddTab(string tabName, IImGuiTabPanel panel) => tabs.Add((tabName, panel));

        IImGuiTabPanel tabToSelect;

        /// <summary>
        /// Makes the tab of <paramref name="panel"/> the active tab on the next frame.
        /// </summary>
        internal void SelectTab(IImGuiTabPanel panel) => tabToSelect = panel;

        /// <summary>
        /// Registers <paramref name="panel"/> as an always-visible, collapsible column to the left of the
        /// tab group, instead of another tab. At most one side panel is supported.
        /// </summary>
        /// <param name="panel">The panel to draw as the side column.</param>
        /// <param name="width">Pixel width of the side column when expanded.</param>
        /// <param name="title">Shown inline next to the collapse toggle; omit for no label.</param>
        internal void SetSidePanel(IImGuiTabPanel panel, float width, string title = null)
        {
            sidePanel = panel;
            sidePanelWidth = width;
            sidePanelTitle = title;
        }

        void RenderFrame(object sender, EventArgs e)
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new Vector2(glControl.Width, glControl.Height);

            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);
            ImGui.Begin("##root",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            DrawContent();

            ImGui.End();
            UpdateMinimumWidth();
        }

        int minimumWidth;

        // NB: applied after the frame, since resizing the window from inside a paint would paint again within it.
        void UpdateMinimumWidth()
        {
            float needed = tabs.Select(t => t.Panel).OfType<ImGuiProbePanel>()
                .Select(p => p.MinimumDisplayWidth)
                .DefaultIfEmpty(0f)
                .Max();
            if (needed <= 0f) return;

            int width = (int)Math.Ceiling(needed) + Width - glControl.Width;
            if (width == minimumWidth) return;

            minimumWidth = width;
            BeginInvoke((Action)(() =>
            {
                if (!IsDisposed) MinimumSize = new Size(width, MinimumSize.Height);
            }));
        }

        // Same resizable/collapsible top-wrapper-plus-bottom-child split ImGuiProbePanel.Draw() uses for
        // its own per-probe log, just promoted one level up so the bottom log spans under DrawContent()
        // (tab area + side panel together) instead of a single tab's own content.
        void DrawContent()
        {
            // NB: this runs before anything is drawn, so an active text field or open popup still counts. Esc
            // then goes to them instead of closing the dialog.
            if (ImGui.IsKeyPressed(ImGuiKey.Escape, false) && !ImGui.IsAnyItemActive() &&
                !ImGui.IsPopupOpen("", ImGuiPopupFlags.AnyPopupId | ImGuiPopupFlags.AnyPopupLevel))
                DialogResult = DialogResult.Cancel;

            float availY = ImGui.GetContentRegionAvail().Y;
            bool formResized = Math.Abs(availY - lastAvailY) > 0.5f;
            lastAvailY = availY;

            var topFlags = logPanelOpen ? ImGuiChildFlags.ResizeY : ImGuiChildFlags.None;
            string topWrapperId = logPanelOpen ? "##shellTopWrapperExpanded" : "##shellTopWrapperCollapsed";
            float topTarget;

            
            if (logPanelOpen)
            {
                topTarget = Math.Max(100f, availY - logPanelHeight - logFooterHeaderHeight);
                if (formResized)
                    ImGui.SetNextWindowSize(new Vector2(-1, topTarget), ImGuiCond.Always);
            }
            else
            {
                topTarget = availY - logFooterHeaderHeight;
            }


            ImGui.BeginChild(topWrapperId, new Vector2(-1, topTarget), topFlags,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            // Draw main tab area + optional side panel
            if (sidePanel == null)
            {
                DrawTabAsPanelOrTabs();
            }
            else
            {
                float avail = ImGui.GetContentRegionAvail().X;
                float sideWidth = sidePanelCollapsed ? SidePanelCollapsedWidth : sidePanelWidth;
                float tabsWidth = Math.Max(200f, avail - sideWidth - ImGui.GetStyle().ItemSpacing.X);

                ImGui.BeginChild("##sidePanel", new Vector2(sideWidth, -1), ImGuiChildFlags.Borders);
                if (ImGui.Button(sidePanelCollapsed ? "»" : "«"))
                    sidePanelCollapsed = !sidePanelCollapsed;
                if (sidePanelTitle != null)
                {
                    if (sidePanelCollapsed)
                        ImGuiWidgets.StackedLabel(sidePanelTitle.ToUpper());
                    else
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted(sidePanelTitle);
                    }
                }
                if (!sidePanelCollapsed)
                {
                    ImGui.Spacing();
                    sidePanel.Draw();
                }
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("##tabArea", new Vector2(tabsWidth, -1));
                DrawTabAsPanelOrTabs();
                ImGui.EndChild();
            }

            if (logPanelOpen)
            {
                float actualTopH = ImGui.GetWindowSize().Y;
                logPanelHeight = Math.Max(60f,
                    Math.Min(availY - logFooterHeaderHeight - 100f, availY - actualTopH - logFooterHeaderHeight));
            }
            ImGui.EndChild();

            float footerStartY = ImGui.GetCursorPosY();
            ImGui.Separator();
            bool wasOpen = logPanelOpen;
            if (ImGui.Button(logPanelOpen ? "»" : "«"))
                logPanelOpen = !logPanelOpen;
            ImGui.SameLine();
            ImGui.TextUnformatted("Log");
            if (wasOpen && !logPanelOpen)
                Log.ClearSelection();

            logFooterHeaderHeight = ImGui.GetCursorPosY() - footerStartY;

            if (logPanelOpen)
            {
                ImGui.BeginChild("##shellLogChild", new Vector2(-1, -1), ImGuiChildFlags.Borders);
                Log.Draw();
                ImGui.EndChild();
            }
        }

        void DrawTabAsPanelOrTabs()
        {
            if (tabs.Count == 1) // single device, no need to show tabs
            {
                tabs[0].Panel.Draw();
            }
            else if (tabs.Count > 1 && ImGui.BeginTabBar("##shellTabs"))
            {
                foreach (var (name, panel) in tabs)
                {
                    var flags = panel.HasChanges ? ImGuiTabItemFlags.UnsavedDocument : ImGuiTabItemFlags.None;
                    if (panel == tabToSelect) flags |= ImGuiTabItemFlags.SetSelected;
                    if (ImGui.BeginTabItem(name, flags))
                    {
                        panel.Draw();
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
                tabToSelect = null;
            }
        }

        // NB: the window's X, Alt+F4 and the system menu's Close all arrive as SC_CLOSE. They act as OK instead of
        // the Cancel that WinForms would give them, so closing the window keeps what was set in it. Unsaved
        // probe interface changes still prompt, through CanClose.
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && ((int)m.WParam & 0xFFF0) == SC_CLOSE)
            {
                DialogResult = DialogResult.OK;
                return;
            }
            base.WndProc(ref m);
        }

        void ShellClosing(object sender, FormClosingEventArgs e)
        {
            glControl.Paced = false;

            // NB: every close path runs the same CanClose gate.
            foreach (var (_, panel) in tabs)
            {
                if (!panel.CanClose(DialogResult))
                {
                    e.Cancel = true;
                    break;
                }
            }
            if (!e.Cancel && sidePanel != null && !sidePanel.CanClose(DialogResult))
                e.Cancel = true;

            if (e.Cancel) glControl.Paced = true;
        }

        // NB: panels act on their close decisions only here, because a later panel can still veto the close after
        // an earlier one has agreed to it.
        void ShellClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var (_, panel) in tabs)
                panel.Closed(DialogResult);
            sidePanel?.Closed(DialogResult);
        }
    }
}
