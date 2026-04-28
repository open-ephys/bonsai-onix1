using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Abstract base class for headstage configuration dialogs that host multiple child
    /// dialogs in tabs. The layout is built programmatically, so no Designer file is needed.
    /// </summary>
    /// <remarks>
    /// Subclasses call <see cref="AddProbeTab{T}"/>, <see cref="AddSequenceTab{T}"/>, and
    /// <see cref="AddDeviceTab"/> in their constructors to register tabs. The base class handles:
    /// <list type="bullet">
    ///   <item>OK / Cancel buttons</item>
    ///   <item>Routing keyboard shortcuts to the active probe tab</item>
    ///   <item>Prompting on cancel when there are unsaved probe changes</item>
    ///   <item>Closing child probe dialogs in order and recreating any that were closed prematurely</item>
    ///   <item>Validating stimulus sequence dialogs before allowing OK to close the headstage dialog</item>
    /// </list>
    /// </remarks>
    public abstract class HeadstageDialog : Form
    {
        private readonly TabControl tabControl;
        private readonly List<ProbeTabEntry> probeTabs = new();
        private readonly List<StimulusSequenceTabEntry> stimulusSequenceTabs = new();
        private readonly HashSet<TabPage> deviceTabPages = new();

        /// <summary>
        /// Initializes a new instance of <see cref="HeadstageDialog"/> with a programmatically
        /// created tab + OK / Cancel layout.
        /// </summary>
        protected HeadstageDialog()
        {
            var resources = new ComponentResourceManager(typeof(HeadstageDialog));

            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1334, 811);
            DoubleBuffered = true;
            Margin = new Padding(3, 2, 3, 2);
            StartPosition = FormStartPosition.CenterParent;
            Icon = (Icon)resources.GetObject("$this.Icon");

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 2, 3, 2),
                TabStop = false
            };

            var buttonOkay = new Button
            {
                Text = "OK",
                Size = new Size(144, 32),
                Margin = new Padding(3, 2, 3, 2),
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            buttonOkay.Click += ValidateStimulusSequences;

            var buttonCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(144, 32),
                Margin = new Padding(3, 2, 3, 2),
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            var flowLayout = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(4)
            };
            flowLayout.Controls.Add(buttonCancel);
            flowLayout.Controls.Add(buttonOkay);

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
            tableLayout.Controls.Add(tabControl, 0, 0);
            tableLayout.Controls.Add(flowLayout, 0, 1);

            Controls.Add(tableLayout);
            FormClosing += DialogClosing;
        }

        /// <summary>
        /// Gets whether any registered probe dialog has unsaved changes.
        /// </summary>
        protected bool HasChanges => probeTabs.Any(e => e.Dialog.HasChanges);

        /// <summary>
        /// Returns the <see cref="IProbeInterfaceDialog"/> registered at the given probe-tab index.
        /// </summary>
        protected IProbeInterfaceDialog GetProbeDialog(int index) => probeTabs[index].Dialog;

        /// <summary>
        /// Registers a probe dialog as a new tab. Probe tabs support unsaved-change tracking,
        /// keyboard-shortcut routing, and automatic recreation when closing is cancelled.
        /// </summary>
        /// <typeparam name="T">Concrete probe dialog type.</typeparam>
        /// <param name="tabName">Label shown on the tab.</param>
        /// <param name="dialog">Probe dialog to host.</param>
        /// <param name="recreate">
        /// Factory that produces a replacement dialog from the disposed old one, allowing any
        /// in-progress state (e.g. probe group selection) to be copied across.
        /// </param>
        protected void AddProbeTab<T>(string tabName, T dialog, Func<T, T> recreate)
            where T : Form, IProbeInterfaceDialog
        {
            var (tabPage, panel) = CreateTabWithPanel(tabName);
            dialog.SetChildFormProperties(this).AddDialogToPanel(panel);
            SubscribeStateChange(dialog, tabPage);
            probeTabs.Add(new ProbeTabEntry(tabPage, panel, dialog, old => recreate((T)old)));
        }

        /// <summary>
        /// Registers a stimulus sequence dialog as a new tab. Sequence tabs are validated via
        /// <see cref="IStimulusSequenceDialog.CanCloseForm"/> when the user clicks OK, before
        /// the headstage dialog is allowed to close. The dialog is added directly to the tab page
        /// (without an intermediate panel) to match the layout expected by sequence dialogs.
        /// </summary>
        /// <typeparam name="T">Concrete sequence dialog type.</typeparam>
        /// <param name="tabName">Label shown on the tab and used in validation prompts.</param>
        /// <param name="dialog">Sequence dialog to host.</param>
        protected void AddSequenceTab<T>(string tabName, T dialog)
            where T : Form, IStimulusSequenceDialog
        {
            var tabPage = new TabPage { Text = tabName, UseVisualStyleBackColor = true };
            tabControl.TabPages.Add(tabPage);
            dialog.SetChildFormProperties(this).AddDialogToTab(tabPage);
            stimulusSequenceTabs.Add(new StimulusSequenceTabEntry(tabName, dialog));
        }

        /// <summary>
        /// Registers a simple device dialog (e.g. <see cref="GenericDeviceDialog"/>) as a new tab.
        /// Device tabs do not participate in change tracking or keyboard-shortcut routing.
        /// </summary>
        /// <param name="tabName">Label shown on the tab.</param>
        /// <param name="dialog">Device dialog to host.</param>
        protected void AddDeviceTab(string tabName, Form dialog)
        {
            var (tabPage, panel) = CreateTabWithPanel(tabName);
            dialog.SetChildFormProperties(this).AddDialogToPanel(panel);
            deviceTabPages.Add(tabPage);
        }

        private (TabPage tabPage, Panel panel) CreateTabWithPanel(string tabName)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var tabPage = new TabPage { Text = tabName, UseVisualStyleBackColor = true };
            tabPage.Controls.Add(panel);
            tabControl.TabPages.Add(tabPage);
            return (tabPage, panel);
        }

        private static void SubscribeStateChange(IProbeInterfaceDialog dialog, TabPage tabPage)
        {
            dialog.OnStateChange += (s, e) =>
            {
                if (dialog.HasChanges)
                {
                    if (!tabPage.Text.EndsWith("*"))
                        tabPage.Text += '*';
                }
                else
                {
                    tabPage.Text = tabPage.Text.TrimEnd('*');
                }
            };
        }

        private void ValidateStimulusSequences(object sender, EventArgs e)
        {
            if (stimulusSequenceTabs.Count == 0)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            var anyOk = false;
            foreach (var entry in stimulusSequenceTabs)
            {
                if (!entry.Dialog.CanCloseForm(out var result, entry.Name))
                    return; // Short-circuit on the first failure.
                if (result == DialogResult.OK)
                    anyOk = true;
            }

            // OK if at least one sequence was valid
            // Cancel if all were discarded as invalid.
            DialogResult = anyOk ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab != null)
            {
                var entry = probeTabs.FirstOrDefault(e => e.TabPage == selectedTab);
                if (entry != null)
                    return entry.Dialog.ProcessMenuShortcut(keyData);

                if (deviceTabPages.Contains(selectedTab))
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (HasChanges && this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage))
                return;

            for (int i = 0; i < probeTabs.Count; i++)
            {
                ((Form)probeTabs[i].Dialog).CloseWithResult(this);

                if (!((Form)probeTabs[i].Dialog).IsDisposed)
                {
                    e.Cancel = true;

                    // Recreate any probe dialogs already closed during this pass so that the
                    // headstage dialog remains in a consistent state when the user keeps it open.
                    for (int j = 0; j < i; j++)
                        probeTabs[j].Recreate(this);

                    return;
                }
            }
        }

        private sealed class StimulusSequenceTabEntry
        {
            public string Name { get; }
            public IStimulusSequenceDialog Dialog { get; }

            public StimulusSequenceTabEntry(string name, IStimulusSequenceDialog dialog)
            {
                Name = name;
                Dialog = dialog;
            }
        }

        private sealed class ProbeTabEntry
        {
            public TabPage TabPage { get; }
            public Panel Panel { get; }
            public IProbeInterfaceDialog Dialog { get; private set; }

            private readonly Func<IProbeInterfaceDialog, IProbeInterfaceDialog> factory;

            public ProbeTabEntry(TabPage tabPage, Panel panel, IProbeInterfaceDialog dialog,
                Func<IProbeInterfaceDialog, IProbeInterfaceDialog> factory)
            {
                TabPage = tabPage;
                Panel = panel;
                Dialog = dialog;
                this.factory = factory;
            }

            public void Recreate(HeadstageDialog parent)
            {
                var oldDialog = Dialog;
                var newDialog = factory(oldDialog);
                var newForm = (Form)newDialog;
                Panel.Controls.Remove((Form)oldDialog);
                newForm.SetChildFormProperties(parent).AddDialogToPanel(Panel);
                SubscribeStateChange(newDialog, TabPage);
                Dialog = newDialog;
            }
        }
    }
}
