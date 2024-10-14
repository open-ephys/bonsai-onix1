namespace OpenEphys.Onix1.Design
{
    partial class NeuropixelsV1ProbeConfigurationDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label labelPresets;
            System.Windows.Forms.Label adcCalibrationFile;
            System.Windows.Forms.Label gainCalibrationFile;
            System.Windows.Forms.Label spikeFilter;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label lfpGain;
            System.Windows.Forms.Label apGain;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV1ProbeConfigurationDialog));
            this.toolStripLabelAdcCalibrationSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripLabelGainCalibrationSn = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripAdcCalSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripGainCalSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.textBoxLfpCorrection = new System.Windows.Forms.TextBox();
            this.textBoxApCorrection = new System.Windows.Forms.TextBox();
            this.buttonViewAdcs = new System.Windows.Forms.Button();
            this.buttonChooseAdcCalibrationFile = new System.Windows.Forms.Button();
            this.buttonChooseGainCalibrationFile = new System.Windows.Forms.Button();
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.checkBoxSpikeFilter = new System.Windows.Forms.CheckBox();
            this.textBoxAdcCalibrationFile = new System.Windows.Forms.TextBox();
            this.textBoxGainCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxLfpGain = new System.Windows.Forms.ComboBox();
            this.comboBoxApGain = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            labelPresets = new System.Windows.Forms.Label();
            adcCalibrationFile = new System.Windows.Forms.Label();
            gainCalibrationFile = new System.Windows.Forms.Label();
            spikeFilter = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            lfpGain = new System.Windows.Forms.Label();
            apGain = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelProbe.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelOptions.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripLabelAdcCalibrationSN
            // 
            this.toolStripLabelAdcCalibrationSN.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelAdcCalibrationSN.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusWarningImage;
            this.toolStripLabelAdcCalibrationSN.Name = "toolStripLabelAdcCalibrationSN";
            this.toolStripLabelAdcCalibrationSN.Size = new System.Drawing.Size(172, 20);
            this.toolStripLabelAdcCalibrationSN.Text = "ADC Calibration SN: ";
            this.toolStripLabelAdcCalibrationSN.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // toolStripLabelGainCalibrationSn
            // 
            this.toolStripLabelGainCalibrationSn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabelGainCalibrationSn.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusWarningImage;
            this.toolStripLabelGainCalibrationSn.Name = "toolStripLabelGainCalibrationSn";
            this.toolStripLabelGainCalibrationSn.Size = new System.Drawing.Size(173, 20);
            this.toolStripLabelGainCalibrationSn.Text = "Gain Calibration SN: ";
            this.toolStripLabelGainCalibrationSn.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(18, 671);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(39, 16);
            label1.TabIndex = 31;
            label1.Text = "0 mm";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(15, 0);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(46, 16);
            label3.TabIndex = 32;
            label3.Text = "10 mm";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 277);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(71, 16);
            label4.TabIndex = 40;
            label4.Text = "Correction:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 210);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(71, 16);
            label2.TabIndex = 38;
            label2.Text = "Correction:";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(13, 370);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(56, 32);
            labelPresets.TabIndex = 25;
            labelPresets.Text = "Channel\r\nPresets:";
            // 
            // adcCalibrationFile
            // 
            adcCalibrationFile.AutoSize = true;
            adcCalibrationFile.Location = new System.Drawing.Point(13, 11);
            adcCalibrationFile.MaximumSize = new System.Drawing.Size(177, 36);
            adcCalibrationFile.Name = "adcCalibrationFile";
            adcCalibrationFile.Size = new System.Drawing.Size(130, 16);
            adcCalibrationFile.TabIndex = 11;
            adcCalibrationFile.Text = "ADC Calibration File:";
            // 
            // gainCalibrationFile
            // 
            gainCalibrationFile.AutoSize = true;
            gainCalibrationFile.Location = new System.Drawing.Point(13, 114);
            gainCalibrationFile.MaximumSize = new System.Drawing.Size(177, 36);
            gainCalibrationFile.Name = "gainCalibrationFile";
            gainCalibrationFile.Size = new System.Drawing.Size(130, 16);
            gainCalibrationFile.TabIndex = 8;
            gainCalibrationFile.Text = "Gain Calibration File:";
            // 
            // spikeFilter
            // 
            spikeFilter.AutoSize = true;
            spikeFilter.Location = new System.Drawing.Point(13, 310);
            spikeFilter.Name = "spikeFilter";
            spikeFilter.Size = new System.Drawing.Size(77, 16);
            spikeFilter.TabIndex = 6;
            spikeFilter.Text = "Spike Filter:";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(13, 343);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(73, 16);
            Reference.TabIndex = 4;
            Reference.Text = "Reference:";
            // 
            // lfpGain
            // 
            lfpGain.AutoSize = true;
            lfpGain.Location = new System.Drawing.Point(13, 244);
            lfpGain.Name = "lfpGain";
            lfpGain.Size = new System.Drawing.Size(65, 16);
            lfpGain.TabIndex = 2;
            lfpGain.Text = "LFP Gain:";
            // 
            // apGain
            // 
            apGain.AutoSize = true;
            apGain.Location = new System.Drawing.Point(13, 179);
            apGain.Name = "apGain";
            apGain.Size = new System.Drawing.Size(59, 16);
            apGain.TabIndex = 0;
            apGain.Text = "AP Gain:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelAdcCalibrationSN,
            this.toolStripAdcCalSN,
            this.toolStripLabelGainCalibrationSn,
            this.toolStripGainCalSN});
            this.statusStrip1.Location = new System.Drawing.Point(0, 770);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1320, 26);
            this.statusStrip1.TabIndex = 35;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripAdcCalSN
            // 
            this.toolStripAdcCalSN.Name = "toolStripAdcCalSN";
            this.toolStripAdcCalSN.Size = new System.Drawing.Size(113, 20);
            this.toolStripAdcCalSN.Text = "No file selected";
            this.toolStripAdcCalSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripGainCalSN
            // 
            this.toolStripGainCalSN.Name = "toolStripGainCalSN";
            this.toolStripGainCalSN.Size = new System.Drawing.Size(113, 20);
            this.toolStripGainCalSN.Text = "No file selected";
            this.toolStripGainCalSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(1320, 26);
            this.menuStrip.TabIndex = 36;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelOptions, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 26);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1320, 744);
            this.tableLayoutPanel1.TabIndex = 37;
            // 
            // panelProbe
            // 
            this.panelProbe.BackColor = System.Drawing.SystemColors.Control;
            this.panelProbe.Controls.Add(this.panelTrackBar);
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(4, 4);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(4);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(982, 694);
            this.panelProbe.TabIndex = 0;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.panelTrackBar.Controls.Add(label1);
            this.panelTrackBar.Controls.Add(label3);
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Location = new System.Drawing.Point(916, 4);
            this.panelTrackBar.Margin = new System.Windows.Forms.Padding(4);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(61, 686);
            this.panelTrackBar.TabIndex = 33;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarProbePosition.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarProbePosition.Location = new System.Drawing.Point(4, 9);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(56, 669);
            this.trackBarProbePosition.TabIndex = 30;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // panelOptions
            // 
            this.panelOptions.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelOptions.Controls.Add(this.textBoxLfpCorrection);
            this.panelOptions.Controls.Add(label4);
            this.panelOptions.Controls.Add(this.textBoxApCorrection);
            this.panelOptions.Controls.Add(label2);
            this.panelOptions.Controls.Add(this.buttonViewAdcs);
            this.panelOptions.Controls.Add(this.buttonChooseAdcCalibrationFile);
            this.panelOptions.Controls.Add(this.buttonChooseGainCalibrationFile);
            this.panelOptions.Controls.Add(this.buttonEnableContacts);
            this.panelOptions.Controls.Add(this.buttonClearSelections);
            this.panelOptions.Controls.Add(this.comboBoxChannelPresets);
            this.panelOptions.Controls.Add(labelPresets);
            this.panelOptions.Controls.Add(this.buttonResetZoom);
            this.panelOptions.Controls.Add(this.checkBoxSpikeFilter);
            this.panelOptions.Controls.Add(this.textBoxAdcCalibrationFile);
            this.panelOptions.Controls.Add(adcCalibrationFile);
            this.panelOptions.Controls.Add(this.textBoxGainCalibrationFile);
            this.panelOptions.Controls.Add(gainCalibrationFile);
            this.panelOptions.Controls.Add(spikeFilter);
            this.panelOptions.Controls.Add(this.comboBoxReference);
            this.panelOptions.Controls.Add(Reference);
            this.panelOptions.Controls.Add(this.comboBoxLfpGain);
            this.panelOptions.Controls.Add(lfpGain);
            this.panelOptions.Controls.Add(this.comboBoxApGain);
            this.panelOptions.Controls.Add(apGain);
            this.panelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOptions.Location = new System.Drawing.Point(993, 2);
            this.panelOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(324, 698);
            this.panelOptions.TabIndex = 2;
            // 
            // textBoxLfpCorrection
            // 
            this.textBoxLfpCorrection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLfpCorrection.Location = new System.Drawing.Point(101, 274);
            this.textBoxLfpCorrection.Name = "textBoxLfpCorrection";
            this.textBoxLfpCorrection.ReadOnly = true;
            this.textBoxLfpCorrection.Size = new System.Drawing.Size(209, 22);
            this.textBoxLfpCorrection.TabIndex = 41;
            // 
            // textBoxApCorrection
            // 
            this.textBoxApCorrection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxApCorrection.Location = new System.Drawing.Point(101, 207);
            this.textBoxApCorrection.Name = "textBoxApCorrection";
            this.textBoxApCorrection.ReadOnly = true;
            this.textBoxApCorrection.Size = new System.Drawing.Size(209, 22);
            this.textBoxApCorrection.TabIndex = 39;
            // 
            // buttonViewAdcs
            // 
            this.buttonViewAdcs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewAdcs.Enabled = false;
            this.buttonViewAdcs.Location = new System.Drawing.Point(13, 66);
            this.buttonViewAdcs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonViewAdcs.Name = "buttonViewAdcs";
            this.buttonViewAdcs.Size = new System.Drawing.Size(298, 38);
            this.buttonViewAdcs.TabIndex = 37;
            this.buttonViewAdcs.Text = "View ADC Correction Values";
            this.buttonViewAdcs.UseVisualStyleBackColor = true;
            this.buttonViewAdcs.Click += new System.EventHandler(this.ViewAdcs_Click);
            // 
            // buttonChooseAdcCalibrationFile
            // 
            this.buttonChooseAdcCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseAdcCalibrationFile.Location = new System.Drawing.Point(274, 30);
            this.buttonChooseAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonChooseAdcCalibrationFile.Name = "buttonChooseAdcCalibrationFile";
            this.buttonChooseAdcCalibrationFile.Size = new System.Drawing.Size(37, 25);
            this.buttonChooseAdcCalibrationFile.TabIndex = 36;
            this.buttonChooseAdcCalibrationFile.Text = "...";
            this.buttonChooseAdcCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseAdcCalibrationFile.Click += new System.EventHandler(this.ChooseAdcCalibrationFile_Click);
            // 
            // buttonChooseGainCalibrationFile
            // 
            this.buttonChooseGainCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseGainCalibrationFile.Location = new System.Drawing.Point(274, 133);
            this.buttonChooseGainCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonChooseGainCalibrationFile.Name = "buttonChooseGainCalibrationFile";
            this.buttonChooseGainCalibrationFile.Size = new System.Drawing.Size(37, 25);
            this.buttonChooseGainCalibrationFile.TabIndex = 35;
            this.buttonChooseGainCalibrationFile.Text = "...";
            this.buttonChooseGainCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseGainCalibrationFile.Click += new System.EventHandler(this.ChooseGainCalibrationFile_Click);
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEnableContacts.Location = new System.Drawing.Point(13, 429);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(298, 44);
            this.buttonEnableContacts.TabIndex = 28;
            this.buttonEnableContacts.Text = "Enable Selected Electrodes";
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.EnableContacts_Click);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearSelections.Location = new System.Drawing.Point(13, 478);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(298, 44);
            this.buttonClearSelections.TabIndex = 27;
            this.buttonClearSelections.Text = "Clear Electrode Selection";
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ClearSelection_Click);
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(101, 376);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(209, 24);
            this.comboBoxChannelPresets.TabIndex = 26;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetZoom.Location = new System.Drawing.Point(13, 528);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(298, 44);
            this.buttonResetZoom.TabIndex = 22;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ResetZoom_Click);
            // 
            // checkBoxSpikeFilter
            // 
            this.checkBoxSpikeFilter.AutoSize = true;
            this.checkBoxSpikeFilter.Location = new System.Drawing.Point(101, 309);
            this.checkBoxSpikeFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxSpikeFilter.Name = "checkBoxSpikeFilter";
            this.checkBoxSpikeFilter.Size = new System.Drawing.Size(80, 20);
            this.checkBoxSpikeFilter.TabIndex = 14;
            this.checkBoxSpikeFilter.Text = "Enabled";
            this.checkBoxSpikeFilter.UseVisualStyleBackColor = true;
            // 
            // textBoxAdcCalibrationFile
            // 
            this.textBoxAdcCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAdcCalibrationFile.Location = new System.Drawing.Point(13, 30);
            this.textBoxAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxAdcCalibrationFile.Name = "textBoxAdcCalibrationFile";
            this.textBoxAdcCalibrationFile.Size = new System.Drawing.Size(254, 22);
            this.textBoxAdcCalibrationFile.TabIndex = 12;
            this.textBoxAdcCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxAdcCalibrationFile.TextChanged += new System.EventHandler(this.AdcCalibrationFileTextChanged);
            // 
            // textBoxGainCalibrationFile
            // 
            this.textBoxGainCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGainCalibrationFile.Location = new System.Drawing.Point(13, 133);
            this.textBoxGainCalibrationFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxGainCalibrationFile.Name = "textBoxGainCalibrationFile";
            this.textBoxGainCalibrationFile.Size = new System.Drawing.Size(254, 22);
            this.textBoxGainCalibrationFile.TabIndex = 9;
            this.textBoxGainCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxGainCalibrationFile.TextChanged += new System.EventHandler(this.GainCalibrationFileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(101, 339);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(209, 24);
            this.comboBoxReference.TabIndex = 5;
            // 
            // comboBoxLfpGain
            // 
            this.comboBoxLfpGain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLfpGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLfpGain.FormattingEnabled = true;
            this.comboBoxLfpGain.Location = new System.Drawing.Point(101, 240);
            this.comboBoxLfpGain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxLfpGain.Name = "comboBoxLfpGain";
            this.comboBoxLfpGain.Size = new System.Drawing.Size(209, 24);
            this.comboBoxLfpGain.TabIndex = 3;
            // 
            // comboBoxApGain
            // 
            this.comboBoxApGain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxApGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxApGain.FormattingEnabled = true;
            this.comboBoxApGain.Location = new System.Drawing.Point(101, 174);
            this.comboBoxApGain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxApGain.Name = "comboBoxApGain";
            this.comboBoxApGain.Size = new System.Drawing.Size(209, 24);
            this.comboBoxApGain.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOkay);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 704);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1316, 38);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1202, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(111, 34);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOkay
            // 
            this.buttonOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOkay.Location = new System.Drawing.Point(1085, 2);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(111, 34);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            // 
            // NeuropixelsV1ProbeConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1320, 796);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NeuropixelsV1ProbeConfigurationDialog";
            this.Text = "NeuropixelsV1 Probe Configuration";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelProbe.ResumeLayout(false);
            this.panelTrackBar.ResumeLayout(false);
            this.panelTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelOptions.ResumeLayout(false);
            this.panelOptions.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripAdcCalSN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripGainCalSN;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.Panel panelTrackBar;
        private System.Windows.Forms.TrackBar trackBarProbePosition;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.TextBox textBoxLfpCorrection;
        private System.Windows.Forms.TextBox textBoxApCorrection;
        private System.Windows.Forms.Button buttonViewAdcs;
        private System.Windows.Forms.Button buttonChooseAdcCalibrationFile;
        private System.Windows.Forms.Button buttonChooseGainCalibrationFile;
        private System.Windows.Forms.Button buttonEnableContacts;
        private System.Windows.Forms.Button buttonClearSelections;
        private System.Windows.Forms.ComboBox comboBoxChannelPresets;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.CheckBox checkBoxSpikeFilter;
        internal System.Windows.Forms.TextBox textBoxAdcCalibrationFile;
        internal System.Windows.Forms.TextBox textBoxGainCalibrationFile;
        private System.Windows.Forms.ComboBox comboBoxReference;
        private System.Windows.Forms.ComboBox comboBoxLfpGain;
        private System.Windows.Forms.ComboBox comboBoxApGain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOkay;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelAdcCalibrationSN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelGainCalibrationSn;
    }
}
