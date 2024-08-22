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
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label probeCalibrationFile;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label labelPresets;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV2eProbeConfigurationDialog));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelChannelOptions = new System.Windows.Forms.Panel();
            this.buttonChooseCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxProbeCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            probeCalibrationFile = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            labelPresets = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.panelProbe.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelChannelOptions.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(0, 440);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(32, 13);
            label6.TabIndex = 28;
            label6.Text = "0 mm";
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(0, 0);
            label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(38, 13);
            label7.TabIndex = 29;
            label7.Text = "10 mm";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // probeCalibrationFile
            // 
            probeCalibrationFile.AutoSize = true;
            probeCalibrationFile.Location = new System.Drawing.Point(8, 9);
            probeCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            probeCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            probeCalibrationFile.Name = "probeCalibrationFile";
            probeCalibrationFile.Size = new System.Drawing.Size(109, 13);
            probeCalibrationFile.TabIndex = 32;
            probeCalibrationFile.Text = "Probe Calibration File:";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(8, 62);
            Reference.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(60, 13);
            Reference.TabIndex = 30;
            Reference.Text = "Reference:";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(8, 94);
            labelPresets.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(49, 26);
            labelPresets.TabIndex = 23;
            labelPresets.Text = "Channel \nPresets:";
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
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEnableContacts.Location = new System.Drawing.Point(11, 138);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(183, 36);
            this.buttonEnableContacts.TabIndex = 20;
            this.buttonEnableContacts.Text = "Enable Selected Electrodes";
            this.toolTip.SetToolTip(this.buttonEnableContacts, "Click and drag to select contacts in the probe view. \r\nPress this button to enabl" +
        "e the selected contacts.");
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearSelections.Location = new System.Drawing.Point(11, 178);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(183, 36);
            this.buttonClearSelections.TabIndex = 19;
            this.buttonClearSelections.Text = "Clear Electrode Selection";
            this.toolTip.SetToolTip(this.buttonClearSelections, "Remove selections from contacts in the probe view. Press this button to deselect " +
        "contacts.\r\nNote that this does not disable contacts, but simply deselects them.");
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetZoom.Location = new System.Drawing.Point(11, 218);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(183, 36);
            this.buttonResetZoom.TabIndex = 4;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.toolTip.SetToolTip(this.buttonResetZoom, "Reset the zoom in the probe view so that the probe is zoomed out and centered.\r\nP" +
        "ress this button to reset the zoom.");
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // panelProbe
            // 
            this.panelProbe.AutoSize = true;
            this.panelProbe.Controls.Add(this.panelTrackBar);
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(2, 2);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(621, 463);
            this.panelProbe.TabIndex = 1;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelTrackBar.Controls.Add(label6);
            this.panelTrackBar.Controls.Add(label7);
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Location = new System.Drawing.Point(581, 6);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(37, 454);
            this.panelTrackBar.TabIndex = 30;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarProbePosition.AutoSize = false;
            this.trackBarProbePosition.Location = new System.Drawing.Point(-6, 9);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(37, 435);
            this.trackBarProbePosition.TabIndex = 22;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // panelChannelOptions
            // 
            this.panelChannelOptions.AutoSize = true;
            this.panelChannelOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelChannelOptions.BackColor = System.Drawing.SystemColors.ControlLightLight;
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
            this.panelChannelOptions.Location = new System.Drawing.Point(627, 2);
            this.panelChannelOptions.Margin = new System.Windows.Forms.Padding(2);
            this.panelChannelOptions.Name = "panelChannelOptions";
            this.panelChannelOptions.Size = new System.Drawing.Size(205, 463);
            this.panelChannelOptions.TabIndex = 1;
            // 
            // buttonChooseCalibrationFile
            // 
            this.buttonChooseCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseCalibrationFile.Location = new System.Drawing.Point(169, 24);
            this.buttonChooseCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseCalibrationFile.Name = "buttonChooseCalibrationFile";
            this.buttonChooseCalibrationFile.Size = new System.Drawing.Size(28, 20);
            this.buttonChooseCalibrationFile.TabIndex = 34;
            this.buttonChooseCalibrationFile.Text = "...";
            this.toolTip.SetToolTip(this.buttonChooseCalibrationFile, "Browse for a gain calibration file.");
            this.buttonChooseCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxProbeCalibrationFile
            // 
            this.textBoxProbeCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProbeCalibrationFile.Location = new System.Drawing.Point(11, 24);
            this.textBoxProbeCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxProbeCalibrationFile.Name = "textBoxProbeCalibrationFile";
            this.textBoxProbeCalibrationFile.Size = new System.Drawing.Size(154, 20);
            this.textBoxProbeCalibrationFile.TabIndex = 33;
            this.textBoxProbeCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(78, 58);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(115, 21);
            this.comboBoxReference.TabIndex = 31;
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(78, 97);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(115, 21);
            this.comboBoxChannelPresets.TabIndex = 24;
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEnableContacts.Location = new System.Drawing.Point(11, 138);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(182, 36);
            this.buttonEnableContacts.TabIndex = 20;
            this.buttonEnableContacts.Text = "Enable Selected Electrodes";
            this.toolTip.SetToolTip(this.buttonEnableContacts, "Click and drag to select electrodes in the probe view. \r\nPress this button to ena" +
        "ble the selected electrodes. \r\nNot all electrode combinations are possible.");
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearSelections.Location = new System.Drawing.Point(11, 178);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(182, 36);
            this.buttonClearSelections.TabIndex = 19;
            this.buttonClearSelections.Text = "Clear Electrode Selection";
            this.toolTip.SetToolTip(this.buttonClearSelections, "Deselect all electrodes in the probe view. \r\nNote that this does not disable elec" +
        "trodes, but simply deselects them.");
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetZoom.Location = new System.Drawing.Point(11, 218);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(182, 36);
            this.buttonResetZoom.TabIndex = 4;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.toolTip.SetToolTip(this.buttonResetZoom, "Reset the zoom in the probe view so that the probe is zoomed out and centered.");
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(726, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 30);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonOkay
            // 
            this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOkay.Location = new System.Drawing.Point(622, 2);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(100, 30);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            this.buttonOkay.Click += new System.EventHandler(this.ButtonClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.panelChannelOptions, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(834, 509);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOkay);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 470);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(828, 36);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // NeuropixelsV2eProbeConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 533);
            this.Controls.Add(this.tableLayoutPanel1);
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
            this.panelProbe.ResumeLayout(false);
            this.panelTrackBar.ResumeLayout(false);
            this.panelTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelChannelOptions.ResumeLayout(false);
            this.panelChannelOptions.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.Panel panelTrackBar;
        private System.Windows.Forms.TrackBar trackBarProbePosition;
        private System.Windows.Forms.Panel panelChannelOptions;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonChooseCalibrationFile;
        private System.Windows.Forms.Button buttonOkay;
        internal System.Windows.Forms.TextBox textBoxProbeCalibrationFile;
        private System.Windows.Forms.ComboBox comboBoxReference;
        private System.Windows.Forms.ComboBox comboBoxChannelPresets;
        private System.Windows.Forms.Button buttonEnableContacts;
        private System.Windows.Forms.Button buttonClearSelections;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
