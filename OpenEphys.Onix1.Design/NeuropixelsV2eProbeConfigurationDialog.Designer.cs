namespace OpenEphys.Onix1.Design
{
    partial class NeuropixelsV2eProbeConfigurationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGain;
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelProbe;
            System.Windows.Forms.Label probeCalibrationFile;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label labelPresets;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV2eProbeConfigurationDialog));
            this.toolStripLabelProbeNumber = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.probeSn = new System.Windows.Forms.ToolStripStatusLabel();
            this.gain = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelChannelOptions = new System.Windows.Forms.Panel();
            this.buttonClearCalibrationFile = new System.Windows.Forms.Button();
            this.buttonChooseCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxProbeCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            toolStripStatusLabelGain = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelProbe = new System.Windows.Forms.ToolStripStatusLabel();
            probeCalibrationFile = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            labelPresets = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panelProbe.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelChannelOptions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripStatusLabelGain
            // 
            toolStripStatusLabelGain.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            toolStripStatusLabelGain.Name = "toolStripStatusLabelGain";
            toolStripStatusLabelGain.Size = new System.Drawing.Size(100, 17);
            toolStripStatusLabelGain.Text = "Gain Correction: ";
            // 
            // toolStripStatusLabelProbe
            // 
            toolStripStatusLabelProbe.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            toolStripStatusLabelProbe.Name = "toolStripStatusLabelProbe";
            toolStripStatusLabelProbe.Size = new System.Drawing.Size(29, 17);
            toolStripStatusLabelProbe.Text = "SN: ";
            // 
            // probeCalibrationFile
            // 
            probeCalibrationFile.AutoSize = true;
            probeCalibrationFile.Location = new System.Drawing.Point(8, 9);
            probeCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            probeCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            probeCalibrationFile.Name = "probeCalibrationFile";
            probeCalibrationFile.Size = new System.Drawing.Size(106, 13);
            probeCalibrationFile.TabIndex = 32;
            probeCalibrationFile.Text = "Probe Calibration File";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(8, 87);
            Reference.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(57, 13);
            Reference.TabIndex = 30;
            Reference.Text = "Reference";
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(1, 1);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(38, 13);
            label7.TabIndex = 29;
            label7.Text = "10 mm";
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(4, 443);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(32, 13);
            label6.TabIndex = 28;
            label6.Text = "0 mm";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(8, 119);
            labelPresets.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(46, 26);
            labelPresets.TabIndex = 23;
            labelPresets.Text = "Channel\r\nPresets";
            // 
            // toolStripLabelProbeNumber
            // 
            this.toolStripLabelProbeNumber.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelProbeNumber.Name = "toolStripLabelProbeNumber";
            this.toolStripLabelProbeNumber.Size = new System.Drawing.Size(40, 17);
            this.toolStripLabelProbeNumber.Text = "Probe";
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(834, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStripNeuropixelsV2e";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelProbeNumber,
            toolStripStatusLabelProbe,
            this.probeSn,
            toolStripStatusLabelGain,
            this.gain});
            this.statusStrip.Location = new System.Drawing.Point(0, 511);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip.Size = new System.Drawing.Size(834, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // probeSn
            // 
            this.probeSn.AutoSize = false;
            this.probeSn.Name = "probeSn";
            this.probeSn.Size = new System.Drawing.Size(120, 17);
            this.probeSn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gain
            // 
            this.gain.AutoSize = false;
            this.gain.Name = "gain";
            this.gain.Size = new System.Drawing.Size(120, 17);
            this.gain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(834, 487);
            this.splitContainer1.SplitterDistance = 458;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panelProbe);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelChannelOptions);
            this.splitContainer2.Size = new System.Drawing.Size(834, 458);
            this.splitContainer2.SplitterDistance = 622;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 1;
            // 
            // panelProbe
            // 
            this.panelProbe.Controls.Add(this.panelTrackBar);
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(0, 0);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(622, 458);
            this.panelProbe.TabIndex = 1;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.panelTrackBar.Controls.Add(label6);
            this.panelTrackBar.Controls.Add(label7);
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Location = new System.Drawing.Point(580, 1);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(39, 456);
            this.panelTrackBar.TabIndex = 30;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarProbePosition.AutoSize = false;
            this.trackBarProbePosition.Location = new System.Drawing.Point(2, 2);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(37, 452);
            this.trackBarProbePosition.TabIndex = 22;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // panelChannelOptions
            // 
            this.panelChannelOptions.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelChannelOptions.Controls.Add(this.buttonClearCalibrationFile);
            this.panelChannelOptions.Controls.Add(this.buttonChooseCalibrationFile);
            this.panelChannelOptions.Controls.Add(this.textBoxProbeCalibrationFile);
            this.panelChannelOptions.Controls.Add(probeCalibrationFile);
            this.panelChannelOptions.Controls.Add(this.comboBoxReference);
            this.panelChannelOptions.Controls.Add(Reference);
            this.panelChannelOptions.Controls.Add(this.comboBoxChannelPresets);
            this.panelChannelOptions.Controls.Add(labelPresets);
            this.panelChannelOptions.Controls.Add(this.buttonEnableContacts);
            this.panelChannelOptions.Controls.Add(this.buttonClearSelections);
            this.panelChannelOptions.Controls.Add(this.buttonResetZoom);
            this.panelChannelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChannelOptions.Location = new System.Drawing.Point(0, 0);
            this.panelChannelOptions.Margin = new System.Windows.Forms.Padding(2);
            this.panelChannelOptions.Name = "panelChannelOptions";
            this.panelChannelOptions.Size = new System.Drawing.Size(209, 458);
            this.panelChannelOptions.TabIndex = 1;
            // 
            // buttonClearCalibrationFile
            // 
            this.buttonClearCalibrationFile.Location = new System.Drawing.Point(103, 45);
            this.buttonClearCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearCalibrationFile.Name = "buttonClearCalibrationFile";
            this.buttonClearCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonClearCalibrationFile.TabIndex = 35;
            this.buttonClearCalibrationFile.Text = "Clear";
            this.buttonClearCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonClearCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonChooseCalibrationFile
            // 
            this.buttonChooseCalibrationFile.Location = new System.Drawing.Point(8, 45);
            this.buttonChooseCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseCalibrationFile.Name = "buttonChooseCalibrationFile";
            this.buttonChooseCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonChooseCalibrationFile.TabIndex = 34;
            this.buttonChooseCalibrationFile.Text = "Choose";
            this.buttonChooseCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxProbeCalibrationFile
            // 
            this.textBoxProbeCalibrationFile.Location = new System.Drawing.Point(8, 24);
            this.textBoxProbeCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxProbeCalibrationFile.Name = "textBoxProbeCalibrationFile";
            this.textBoxProbeCalibrationFile.ReadOnly = true;
            this.textBoxProbeCalibrationFile.Size = new System.Drawing.Size(189, 20);
            this.textBoxProbeCalibrationFile.TabIndex = 33;
            this.textBoxProbeCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxProbeCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(78, 83);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(119, 21);
            this.comboBoxReference.TabIndex = 31;
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(78, 122);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(119, 21);
            this.comboBoxChannelPresets.TabIndex = 24;
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Location = new System.Drawing.Point(8, 165);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(94, 36);
            this.buttonEnableContacts.TabIndex = 20;
            this.buttonEnableContacts.Text = "Enable Selected Contacts";
            this.toolTip.SetToolTip(this.buttonEnableContacts, "Click and drag to select contacts in the probe view. \r\nPress this button to enabl" +
        "e the selected contacts.");
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Location = new System.Drawing.Point(103, 165);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(94, 36);
            this.buttonClearSelections.TabIndex = 19;
            this.buttonClearSelections.Text = "Deselect Contacts";
            this.toolTip.SetToolTip(this.buttonClearSelections, "Remove selections from contacts in the probe view. Press this button to deselect " +
        "contacts.\r\nNote that this does not disable contacts, but simply deselects them.");
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Location = new System.Drawing.Point(57, 208);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(94, 36);
            this.buttonResetZoom.TabIndex = 4;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.toolTip.SetToolTip(this.buttonResetZoom, "Reset the zoom in the probe view so that the probe is zoomed out and centered.\r\nP" +
        "ress this button to reset the zoom.");
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOkay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(834, 26);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(744, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 22);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonOkay
            // 
            this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOkay.Location = new System.Drawing.Point(655, 0);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(83, 22);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            this.buttonOkay.Click += new System.EventHandler(this.ButtonClick);
            // 
            // NeuropixelsV2eProbeConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 533);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NeuropixelsV2eProbeConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NeuropixelsV2eProbeConfigurationDialog";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panelProbe.ResumeLayout(false);
            this.panelTrackBar.ResumeLayout(false);
            this.panelTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelChannelOptions.ResumeLayout(false);
            this.panelChannelOptions.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOkay;
        private System.Windows.Forms.ToolStripStatusLabel probeSn;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripStatusLabel gain;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.Panel panelChannelOptions;
        private System.Windows.Forms.Button buttonClearCalibrationFile;
        private System.Windows.Forms.Button buttonChooseCalibrationFile;
        internal System.Windows.Forms.TextBox textBoxProbeCalibrationFile;
        private System.Windows.Forms.ComboBox comboBoxReference;
        private System.Windows.Forms.ComboBox comboBoxChannelPresets;
        private System.Windows.Forms.TrackBar trackBarProbePosition;
        private System.Windows.Forms.Button buttonEnableContacts;
        private System.Windows.Forms.Button buttonClearSelections;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelProbeNumber;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panelTrackBar;
    }
}
