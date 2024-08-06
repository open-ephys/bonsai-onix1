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
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGain;
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelProbe;
            System.Windows.Forms.Label probeCalibrationFile;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label labelPresets;
            System.Windows.Forms.Label labelSelection;
            System.Windows.Forms.Label label1;
            this.toolStripLabelProbeNumber = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.probeSn = new System.Windows.Forms.ToolStripStatusLabel();
            this.gain = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelChannelOptions = new System.Windows.Forms.Panel();
            this.buttonClearCalibrationFile = new System.Windows.Forms.Button();
            this.buttonChooseCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxProbeCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            this.linkLabelDocumentation = new System.Windows.Forms.LinkLabel();
            toolStripStatusLabelGain = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelProbe = new System.Windows.Forms.ToolStripStatusLabel();
            probeCalibrationFile = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            labelPresets = new System.Windows.Forms.Label();
            labelSelection = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
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
            this.panelChannelOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripStatusLabelGain
            // 
            toolStripStatusLabelGain.Name = "toolStripStatusLabelGain";
            toolStripStatusLabelGain.Size = new System.Drawing.Size(47, 25);
            toolStripStatusLabelGain.Text = "Gain";
            // 
            // toolStripStatusLabelProbe
            // 
            toolStripStatusLabelProbe.Name = "toolStripStatusLabelProbe";
            toolStripStatusLabelProbe.Size = new System.Drawing.Size(44, 25);
            toolStripStatusLabelProbe.Text = "SN: ";
            // 
            // probeCalibrationFile
            // 
            probeCalibrationFile.AutoSize = true;
            probeCalibrationFile.Location = new System.Drawing.Point(8, 14);
            probeCalibrationFile.MaximumSize = new System.Drawing.Size(200, 45);
            probeCalibrationFile.Name = "probeCalibrationFile";
            probeCalibrationFile.Size = new System.Drawing.Size(159, 20);
            probeCalibrationFile.TabIndex = 32;
            probeCalibrationFile.Text = "Probe Calibration File";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(18, 134);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(84, 20);
            Reference.TabIndex = 30;
            Reference.Text = "Reference";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(63, 218);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(57, 20);
            label7.TabIndex = 29;
            label7.Text = "10 mm";
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(63, 649);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(48, 20);
            label6.TabIndex = 28;
            label6.Text = "0 mm";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(150, 354);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(126, 20);
            labelPresets.TabIndex = 23;
            labelPresets.Text = "Channel Presets";
            // 
            // labelSelection
            // 
            labelSelection.AutoSize = true;
            labelSelection.Location = new System.Drawing.Point(187, 186);
            labelSelection.Name = "labelSelection";
            labelSelection.Size = new System.Drawing.Size(75, 20);
            labelSelection.TabIndex = 18;
            labelSelection.Text = "Selection";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 186);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(66, 20);
            label1.TabIndex = 5;
            label1.Text = "Jump to";
            // 
            // toolStripLabelProbeNumber
            // 
            this.toolStripLabelProbeNumber.Name = "toolStripLabelProbeNumber";
            this.toolStripLabelProbeNumber.Size = new System.Drawing.Size(59, 25);
            this.toolStripLabelProbeNumber.Text = "Probe";
            // 
            // menuStrip
            // 
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1265, 33);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStripNeuropixelsV2e";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
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
            this.statusStrip.Location = new System.Drawing.Point(0, 784);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1265, 32);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // probeSn
            // 
            this.probeSn.AutoSize = false;
            this.probeSn.Name = "probeSn";
            this.probeSn.Size = new System.Drawing.Size(135, 25);
            this.probeSn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gain
            // 
            this.gain.AutoSize = false;
            this.gain.Name = "gain";
            this.gain.Size = new System.Drawing.Size(120, 25);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
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
            this.splitContainer1.Size = new System.Drawing.Size(1265, 751);
            this.splitContainer1.SplitterDistance = 699;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panelProbe);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelChannelOptions);
            this.splitContainer2.Size = new System.Drawing.Size(1265, 699);
            this.splitContainer2.SplitterDistance = 940;
            this.splitContainer2.TabIndex = 1;
            // 
            // panelProbe
            // 
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(0, 0);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(940, 699);
            this.panelProbe.TabIndex = 1;
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
            this.panelChannelOptions.Controls.Add(label7);
            this.panelChannelOptions.Controls.Add(label6);
            this.panelChannelOptions.Controls.Add(this.comboBoxChannelPresets);
            this.panelChannelOptions.Controls.Add(labelPresets);
            this.panelChannelOptions.Controls.Add(this.trackBarProbePosition);
            this.panelChannelOptions.Controls.Add(this.buttonEnableContacts);
            this.panelChannelOptions.Controls.Add(this.buttonClearSelections);
            this.panelChannelOptions.Controls.Add(labelSelection);
            this.panelChannelOptions.Controls.Add(label1);
            this.panelChannelOptions.Controls.Add(this.buttonResetZoom);
            this.panelChannelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChannelOptions.Location = new System.Drawing.Point(0, 0);
            this.panelChannelOptions.Name = "panelChannelOptions";
            this.panelChannelOptions.Size = new System.Drawing.Size(321, 699);
            this.panelChannelOptions.TabIndex = 1;
            // 
            // buttonClearCalibrationFile
            // 
            this.buttonClearCalibrationFile.Location = new System.Drawing.Point(154, 69);
            this.buttonClearCalibrationFile.Name = "buttonClearCalibrationFile";
            this.buttonClearCalibrationFile.Size = new System.Drawing.Size(141, 32);
            this.buttonClearCalibrationFile.TabIndex = 35;
            this.buttonClearCalibrationFile.Text = "Clear";
            this.buttonClearCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonClearCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonChooseCalibrationFile
            // 
            this.buttonChooseCalibrationFile.Location = new System.Drawing.Point(12, 69);
            this.buttonChooseCalibrationFile.Name = "buttonChooseCalibrationFile";
            this.buttonChooseCalibrationFile.Size = new System.Drawing.Size(141, 32);
            this.buttonChooseCalibrationFile.TabIndex = 34;
            this.buttonChooseCalibrationFile.Text = "Choose";
            this.buttonChooseCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxProbeCalibrationFile
            // 
            this.textBoxProbeCalibrationFile.Location = new System.Drawing.Point(12, 37);
            this.textBoxProbeCalibrationFile.Name = "textBoxProbeCalibrationFile";
            this.textBoxProbeCalibrationFile.ReadOnly = true;
            this.textBoxProbeCalibrationFile.Size = new System.Drawing.Size(283, 26);
            this.textBoxProbeCalibrationFile.TabIndex = 33;
            this.textBoxProbeCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxProbeCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(119, 131);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(176, 28);
            this.comboBoxReference.TabIndex = 31;
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(136, 377);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(162, 28);
            this.comboBoxChannelPresets.TabIndex = 24;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.trackBarProbePosition.AutoSize = false;
            this.trackBarProbePosition.Location = new System.Drawing.Point(17, 209);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(56, 469);
            this.trackBarProbePosition.TabIndex = 22;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Location = new System.Drawing.Point(136, 209);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(169, 56);
            this.buttonEnableContacts.TabIndex = 20;
            this.buttonEnableContacts.Text = "Enable Selected Contacts";
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Location = new System.Drawing.Point(136, 271);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(169, 59);
            this.buttonClearSelections.TabIndex = 19;
            this.buttonClearSelections.Text = "Deselect Contacts";
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetZoom.Location = new System.Drawing.Point(159, 635);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(117, 34);
            this.buttonResetZoom.TabIndex = 4;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOkay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1265, 48);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(1129, 7);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(124, 34);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonOkay
            // 
            this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOkay.Location = new System.Drawing.Point(996, 7);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(124, 34);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            this.buttonOkay.Click += new System.EventHandler(this.ButtonClick);
            // 
            // linkLabelDocumentation
            // 
            this.linkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDocumentation.AutoSize = true;
            this.linkLabelDocumentation.BackColor = System.Drawing.Color.GhostWhite;
            this.linkLabelDocumentation.Location = new System.Drawing.Point(1125, 790);
            this.linkLabelDocumentation.Name = "linkLabelDocumentation";
            this.linkLabelDocumentation.Size = new System.Drawing.Size(118, 20);
            this.linkLabelDocumentation.TabIndex = 3;
            this.linkLabelDocumentation.TabStop = true;
            this.linkLabelDocumentation.Text = "Documentation";
            this.linkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClicked);
            // 
            // NeuropixelsV2eProbeConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1265, 816);
            this.Controls.Add(this.linkLabelDocumentation);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip;
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
            this.panelChannelOptions.ResumeLayout(false);
            this.panelChannelOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
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
        private System.Windows.Forms.LinkLabel linkLabelDocumentation;
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
    }
}
