using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// A standalone window shell that hosts a single <see cref="ImGuiProbeDialog"/> with a title bar, resize
    /// handles, and OK / Cancel buttons. Use this when showing an <see cref="ImGuiProbeDialog"/> subclass
    /// directly from a <see cref="Bonsai.Design.WorkflowComponentEditor"/> rather than embedding it inside a
    /// <see cref="HeadstageDialog"/> tab.
    /// </summary>
    internal sealed class ProbeShellDialog : Form
    {
        readonly ImGuiProbeDialog innerDialog;

        /// <summary>
        /// Initializes a new instance of <see cref="ProbeShellDialog"/> wrapping
        /// <paramref name="dialog"/>.
        /// </summary>
        internal ProbeShellDialog(ImGuiProbeDialog dialog)
        {
            innerDialog = dialog;

            var resources = new ComponentResourceManager(typeof(HeadstageDialog));
            Icon = (Icon)resources.GetObject("$this.Icon");

            Text = dialog.Text;
            ClientSize = new Size(1334, 811);
            StartPosition = FormStartPosition.CenterScreen;
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

            var contentPanel = new Panel { Dock = DockStyle.Fill };
            dialog.SetChildFormProperties(this).AddDialogToPanel(contentPanel);

            tableLayout.Controls.Add(contentPanel, 0, 0);
            tableLayout.Controls.Add(flowLayout, 0, 1);
            Controls.Add(tableLayout);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            FormClosing += ShellClosing;
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
            => innerDialog.ProcessMenuShortcut(keyData) || base.ProcessCmdKey(ref msg, keyData);

        void ShellClosing(object sender, FormClosingEventArgs e)
        {
            if (innerDialog.HasChanges && this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage))
                return;

            ((Form)innerDialog).CloseWithResult(this);
            if (!((Form)innerDialog).IsDisposed)
                e.Cancel = true;
        }
    }
}
