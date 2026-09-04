using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// A Form hosting a single shared ImGui GL context and render timer. Hosts one or more <see
    /// cref="IImGuiTabPanel"/>s registered via <see cref="AddTab"/>: with exactly one, its content fills the
    /// window directly (no visible tab bar); with two or more, they're drawn inside an ImGui tab bar, with
    /// only the active tab's content updated each frame. Used directly for standalone single-panel editing
    /// and as a base for multi-tab headstage dialogs.
    /// </summary>
    internal class ImGuiShellDialog : Form
    {
        readonly ImGuiGLControl glControl;
        readonly Timer renderTimer;
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

            glControl = new ImGuiGLControl { Dock = DockStyle.Fill };
            glControl.Render += RenderFrame;
            Controls.Add(glControl);

            renderTimer = new Timer { Interval = 16 };
            renderTimer.Tick += (_, _) => glControl.Invalidate();

            Shown += (_, _) => renderTimer.Start();
            FormClosing += ShellClosing;
        }

        /// <summary>
        /// Registers <paramref name="panel"/> as a new tab labeled <paramref name="tabName"/>.
        /// </summary>
        internal void AddTab(string tabName, IImGuiTabPanel panel) => tabs.Add((tabName, panel));

        /// <summary>
        /// Registers <paramref name="panel"/> as an always-visible, collapsible column to the right of the
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
        }

        // "Vertical" text. One character per line, centered, e.g. for narrow collapsed label.
        static void DrawStackedVerticalLabel(string text)
        {
            float avail = ImGui.GetContentRegionAvail().X;
            foreach (char c in text)
            {
                float charWidth = ImGui.CalcTextSize(c.ToString()).X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, (avail - charWidth) * 0.5f));
                ImGui.TextUnformatted(c.ToString());
            }
        }

        // Same resizable/collapsible top-wrapper-plus-bottom-child split ImGuiProbePanel.Draw() uses for
        // its own per-probe log, just promoted one level up so the bottom log spans under DrawContent()
        // (tab area + side panel together) instead of a single tab's own content.
        void DrawContent()
        {
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

                ImGui.BeginChild("##tabArea", new Vector2(tabsWidth, -1));
                DrawTabAsPanelOrTabs();
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("##sidePanel", new Vector2(sideWidth, -1), ImGuiChildFlags.Borders);
                if (ImGui.Button(sidePanelCollapsed ? "«" : "»"))
                    sidePanelCollapsed = !sidePanelCollapsed;
                if (sidePanelTitle != null)
                {
                    if (sidePanelCollapsed)
                        DrawStackedVerticalLabel(sidePanelTitle);
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
                    if (ImGui.BeginTabItem(name, flags))
                    {
                        panel.Draw();
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }
        }

        void ShellClosing(object sender, FormClosingEventArgs e)
        {
            renderTimer.Stop();

            // NB: every close path (Close button, window X, Alt+F4/system menu) runs the same CanClose
            // gate. WinForms auto-sets DialogResult to Cancel for a modal dialog closed via X/Alt+F4 even
            // with no Cancel button on the form, so DialogResult can't be used to special-case those paths.
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

            if (e.Cancel) renderTimer.Start();
        }
    }
}
