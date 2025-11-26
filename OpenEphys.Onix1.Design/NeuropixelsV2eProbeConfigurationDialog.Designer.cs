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
            System.Windows.Forms.Label probeCalibrationFile;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label labelPresets;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label invertPolarity;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV2eProbeConfigurationDialog));
            this.toolStripLabelGainCalibrationSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.buttonChooseCalibrationFile = new System.Windows.Forms.Button();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelChannelOptions = new System.Windows.Forms.Panel();
            this.checkBoxInvertPolarity = new System.Windows.Forms.CheckBox();
            this.textBoxGainCorrection = new System.Windows.Forms.TextBox();
            this.textBoxProbeCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlProbe = new System.Windows.Forms.TabControl();
            this.tabPageConfiguration = new System.Windows.Forms.TabPage();
            this.tabPageProperties = new System.Windows.Forms.TabPage();
            this.propertyGrid = new Bonsai.Design.PropertyGrid();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripGainCalSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.bindingSource = new System.Windows.Forms.BindingSource(this.components);
            probeCalibrationFile = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            labelPresets = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            invertPolarity = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelChannelOptions.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControlProbe.SuspendLayout();
            this.tabPageConfiguration.SuspendLayout();
            this.tabPageProperties.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // probeCalibrationFile
            // 
            probeCalibrationFile.AutoSize = true;
            probeCalibrationFile.Location = new System.Drawing.Point(15, 11);
            probeCalibrationFile.MaximumSize = new System.Drawing.Size(177, 36);
            probeCalibrationFile.Name = "probeCalibrationFile";
            probeCalibrationFile.Size = new System.Drawing.Size(139, 16);
            probeCalibrationFile.TabIndex = 0;
            probeCalibrationFile.Text = "Probe Calibration File:";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(15, 114);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(73, 16);
            Reference.TabIndex = 5;
            Reference.Text = "Reference:";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(15, 146);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(59, 32);
            labelPresets.TabIndex = 7;
            labelPresets.Text = "Channel \nPresets:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(15, 63);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(71, 32);
            label1.TabIndex = 3;
            label1.Text = "Gain\r\nCorrection:";
            // 
            // invertPolarity
            // 
            invertPolarity.AutoSize = true;
            invertPolarity.Location = new System.Drawing.Point(15, 187);
            invertPolarity.Name = "invertPolarity";
            invertPolarity.Size = new System.Drawing.Size(55, 32);
            invertPolarity.TabIndex = 9;
            invertPolarity.Text = "Invert\r\nPolarity:";
            // 
            // toolStripLabelGainCalibrationSN
            // 
            this.toolStripLabelGainCalibrationSN.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelGainCalibrationSN.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusWarningImage;
            this.toolStripLabelGainCalibrationSN.Name = "toolStripLabelGainCalibrationSN";
            this.toolStripLabelGainCalibrationSN.Size = new System.Drawing.Size(139, 20);
            this.toolStripLabelGainCalibrationSN.Text = "Gain Calibration SN: ";
            this.toolStripLabelGainCalibrationSN.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(1234, 24);
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
            this.buttonEnableContacts.Location = new System.Drawing.Point(15, 232);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(266, 44);
            this.buttonEnableContacts.TabIndex = 11;
            this.buttonEnableContacts.Text = "Enable Selected Electrodes";
            this.toolTip.SetToolTip(this.buttonEnableContacts, "Click and drag to select electrodes in the probe view. \r\nPress this button to ena" +
        "ble the selected electrodes. \r\nNot all electrode combinations are possible.");
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.EnableContacts_Click);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearSelections.Location = new System.Drawing.Point(15, 282);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(266, 44);
            this.buttonClearSelections.TabIndex = 12;
            this.buttonClearSelections.Text = "Clear Electrode Selection";
            this.toolTip.SetToolTip(this.buttonClearSelections, "Deselect all electrodes in the probe view. \r\nNote that this does not disable elec" +
        "trodes, but simply deselects them.");
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ClearSelection_Click);
            // 
            // buttonChooseCalibrationFile
            // 
            this.buttonChooseCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseCalibrationFile.Location = new System.Drawing.Point(244, 29);
            this.buttonChooseCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonChooseCalibrationFile.Name = "buttonChooseCalibrationFile";
            this.buttonChooseCalibrationFile.Size = new System.Drawing.Size(37, 25);
            this.buttonChooseCalibrationFile.TabIndex = 2;
            this.buttonChooseCalibrationFile.Text = "...";
            this.toolTip.SetToolTip(this.buttonChooseCalibrationFile, "Browse for a gain calibration file.");
            this.buttonChooseCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseCalibrationFile.Click += new System.EventHandler(this.ChooseCalibrationFile_Click);
            // 
            // panelProbe
            // 
            this.panelProbe.AutoSize = true;
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(3, 2);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(858, 662);
            this.panelProbe.TabIndex = 1;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTrackBar.Location = new System.Drawing.Point(868, 4);
            this.panelTrackBar.Margin = new System.Windows.Forms.Padding(4);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(47, 658);
            this.panelTrackBar.TabIndex = 30;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.AutoSize = false;
            this.trackBarProbePosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBarProbePosition.Location = new System.Drawing.Point(0, 0);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(47, 658);
            this.trackBarProbePosition.TabIndex = 22;
            this.trackBarProbePosition.TabStop = false;
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
            this.panelChannelOptions.Controls.Add(this.checkBoxInvertPolarity);
            this.panelChannelOptions.Controls.Add(invertPolarity);
            this.panelChannelOptions.Controls.Add(this.textBoxGainCorrection);
            this.panelChannelOptions.Controls.Add(label1);
            this.panelChannelOptions.Controls.Add(this.buttonChooseCalibrationFile);
            this.panelChannelOptions.Controls.Add(this.textBoxProbeCalibrationFile);
            this.panelChannelOptions.Controls.Add(probeCalibrationFile);
            this.panelChannelOptions.Controls.Add(this.comboBoxReference);
            this.panelChannelOptions.Controls.Add(Reference);
            this.panelChannelOptions.Controls.Add(this.comboBoxChannelPresets);
            this.panelChannelOptions.Controls.Add(labelPresets);
            this.panelChannelOptions.Controls.Add(this.buttonEnableContacts);
            this.panelChannelOptions.Controls.Add(this.buttonClearSelections);
            this.panelChannelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChannelOptions.Location = new System.Drawing.Point(3, 3);
            this.panelChannelOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelChannelOptions.Name = "panelChannelOptions";
            this.panelChannelOptions.Size = new System.Drawing.Size(295, 599);
            this.panelChannelOptions.TabIndex = 1;
            // 
            // checkBoxInvertPolarity
            // 
            this.checkBoxInvertPolarity.AutoSize = true;
            this.checkBoxInvertPolarity.Location = new System.Drawing.Point(107, 193);
            this.checkBoxInvertPolarity.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxInvertPolarity.Name = "checkBoxInvertPolarity";
            this.checkBoxInvertPolarity.Size = new System.Drawing.Size(77, 20);
            this.checkBoxInvertPolarity.TabIndex = 10;
            this.checkBoxInvertPolarity.Text = "Enabled";
            this.checkBoxInvertPolarity.UseVisualStyleBackColor = true;
            // 
            // textBoxGainCorrection
            // 
            this.textBoxGainCorrection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGainCorrection.Location = new System.Drawing.Point(107, 68);
            this.textBoxGainCorrection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxGainCorrection.Name = "textBoxGainCorrection";
            this.textBoxGainCorrection.ReadOnly = true;
            this.textBoxGainCorrection.Size = new System.Drawing.Size(174, 22);
            this.textBoxGainCorrection.TabIndex = 4;
            this.textBoxGainCorrection.TabStop = false;
            // 
            // textBoxProbeCalibrationFile
            // 
            this.textBoxProbeCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProbeCalibrationFile.Location = new System.Drawing.Point(15, 30);
            this.textBoxProbeCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxProbeCalibrationFile.Name = "textBoxProbeCalibrationFile";
            this.textBoxProbeCalibrationFile.Size = new System.Drawing.Size(221, 22);
            this.textBoxProbeCalibrationFile.TabIndex = 1;
            this.textBoxProbeCalibrationFile.TabStop = false;
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(107, 110);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(174, 24);
            this.comboBoxReference.TabIndex = 6;
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(107, 150);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(174, 24);
            this.comboBoxChannelPresets.TabIndex = 8;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1112, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(111, 34);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOkay
            // 
            this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOkay.Location = new System.Drawing.Point(995, 2);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(111, 34);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 315F));
            this.tableLayoutPanel1.Controls.Add(this.panelTrackBar, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControlProbe, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1234, 712);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tabControlProbe
            // 
            this.tabControlProbe.Controls.Add(this.tabPageConfiguration);
            this.tabControlProbe.Controls.Add(this.tabPageProperties);
            this.tabControlProbe.Location = new System.Drawing.Point(922, 2);
            this.tabControlProbe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlProbe.Name = "tabControlProbe";
            this.tabControlProbe.SelectedIndex = 0;
            this.tabControlProbe.Size = new System.Drawing.Size(309, 634);
            this.tabControlProbe.TabIndex = 31;
            this.tabControlProbe.TabStop = false;
            // 
            // tabPageConfiguration
            // 
            this.tabPageConfiguration.Controls.Add(this.panelChannelOptions);
            this.tabPageConfiguration.Location = new System.Drawing.Point(4, 25);
            this.tabPageConfiguration.Name = "tabPageConfiguration";
            this.tabPageConfiguration.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConfiguration.Size = new System.Drawing.Size(301, 605);
            this.tabPageConfiguration.TabIndex = 1;
            this.tabPageConfiguration.Text = "Configuration";
            this.tabPageConfiguration.UseVisualStyleBackColor = true;
            // 
            // tabPageProperties
            // 
            this.tabPageProperties.Controls.Add(this.propertyGrid);
            this.tabPageProperties.Location = new System.Drawing.Point(4, 25);
            this.tabPageProperties.Name = "tabPageProperties";
            this.tabPageProperties.Size = new System.Drawing.Size(301, 605);
            this.tabPageProperties.TabIndex = 0;
            this.tabPageProperties.Text = "Properties";
            this.tabPageProperties.UseVisualStyleBackColor = true;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid.Location = new System.Drawing.Point(8, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(293, 605);
            this.propertyGrid.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 3);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOkay);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 670);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1226, 38);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelGainCalibrationSN,
            this.toolStripGainCalSN});
            this.statusStrip1.Location = new System.Drawing.Point(0, 736);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1234, 25);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripGainCalSN
            // 
            this.toolStripGainCalSN.Name = "toolStripGainCalSN";
            this.toolStripGainCalSN.Size = new System.Drawing.Size(91, 20);
            this.toolStripGainCalSN.Text = "No file selected.";
            this.toolStripGainCalSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NeuropixelsV2eProbeConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1234, 761);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "NeuropixelsV2eProbeConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NeuropixelsV2e Probe Configuration";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panelTrackBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelChannelOptions.ResumeLayout(false);
            this.panelChannelOptions.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControlProbe.ResumeLayout(false);
            this.tabPageConfiguration.ResumeLayout(false);
            this.tabPageConfiguration.PerformLayout();
            this.tabPageProperties.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxGainCorrection;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripGainCalSN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelGainCalibrationSN;
        private System.Windows.Forms.CheckBox checkBoxInvertPolarity;
        private System.Windows.Forms.TabControl tabControlProbe;
        private System.Windows.Forms.TabPage tabPageProperties;
        private Bonsai.Design.PropertyGrid propertyGrid;
        private System.Windows.Forms.TabPage tabPageConfiguration;
        private System.Windows.Forms.BindingSource bindingSource;
    }
}
