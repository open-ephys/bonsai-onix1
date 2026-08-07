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
    /// A Form hosting a single shared ImGui GL context and render timer, plus WinForms OK/Cancel buttons.
    /// Hosts one or more <see cref="IImGuiTabPanel"/>s registered via <see cref="AddTab"/>: with exactly one,
    /// its content fills the window directly (no visible tab bar); with two or more, they're drawn inside an
    /// ImGui tab bar, with only the active tab's content updated each frame. Used directly for standalone
    /// single-panel editing and as a base for multi-tab headstage dialogs.
    /// </summary>
    internal class ImGuiShellDialog : Form
    {
        readonly ImGuiGLControl glControl;
        readonly Timer renderTimer;
        readonly List<(string Name, IImGuiTabPanel Panel)> tabs = new();

        /// <summary>
        /// Initializes a new instance of <see cref="ImGuiShellDialog"/>.
        /// </summary>
        internal ImGuiShellDialog(string title)
        {
            var resources = new ComponentResourceManager(typeof(HeadstageDialog));
            Icon = (Icon)resources.GetObject("$this.Icon");

            Text = title;
            ClientSize = new Size(1334, 811);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            var okButton = new Button
            {
                Text = "OK",
                Size = new Size(144, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };
            okButton.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(144, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };

            var flowLayout = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(4)
            };
            flowLayout.Controls.Add(cancelButton);
            flowLayout.Controls.Add(okButton);

            var tableLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            glControl = new ImGuiGLControl { Dock = DockStyle.Fill };
            glControl.Render += RenderFrame;

            tableLayout.Controls.Add(glControl, 0, 0);
            tableLayout.Controls.Add(flowLayout, 0, 1);
            Controls.Add(tableLayout);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            renderTimer = new Timer { Interval = 16 };
            renderTimer.Tick += (_, _) => glControl.Invalidate();

            Shown += (_, _) => renderTimer.Start();
            FormClosing += ShellClosing;
        }

        /// <summary>
        /// Registers <paramref name="panel"/> as a new tab labeled <paramref name="tabName"/>.
        /// </summary>
        internal void AddTab(string tabName, IImGuiTabPanel panel) => tabs.Add((tabName, panel));

        void RenderFrame(object sender, EventArgs e)
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new Vector2(glControl.Width, glControl.Height);

            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);
            ImGui.Begin("##root",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

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

            ImGui.End();
        }

        void ShellClosing(object sender, FormClosingEventArgs e)
        {
            renderTimer.Stop();

            if (DialogResult == DialogResult.Cancel)
            {
                if (tabs.Any(t => t.Panel.HasChanges))
                    this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage);
            }
            else
            {
                foreach (var (_, panel) in tabs)
                {
                    if (!panel.CanClose(DialogResult))
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }

            if (e.Cancel) renderTimer.Start();
        }
    }
}
