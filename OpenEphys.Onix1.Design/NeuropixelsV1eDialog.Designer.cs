namespace OpenEphys.Onix1.Design
{
    partial class NeuropixelsV1eDialog
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
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGainCalSN;
            System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAdcCalSN;
            System.Windows.Forms.Label labelPresets;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label labelGainCorrection;
            System.Windows.Forms.Label adcCalibrationFile;
            System.Windows.Forms.Label gainCalibrationFile;
            System.Windows.Forms.Label spikeFilter;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label lfpGain;
            System.Windows.Forms.Label apGain;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label1;
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.gainCalibrationSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.adcCalibrationSN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControlProbe = new System.Windows.Forms.TabControl();
            this.tabPageProbe = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.buttonEnableContacts = new System.Windows.Forms.Button();
            this.buttonClearSelections = new System.Windows.Forms.Button();
            this.comboBoxChannelPresets = new System.Windows.Forms.ComboBox();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.buttonClearAdcCalibrationFile = new System.Windows.Forms.Button();
            this.buttonClearGainCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxLfpGainCorrection = new System.Windows.Forms.TextBox();
            this.textBoxApGainCorrection = new System.Windows.Forms.TextBox();
            this.checkBoxSpikeFilter = new System.Windows.Forms.CheckBox();
            this.buttonChooseAdcCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxAdcCalibrationFile = new System.Windows.Forms.TextBox();
            this.buttonChooseGainCalibrationFile = new System.Windows.Forms.Button();
            this.textBoxGainCalibrationFile = new System.Windows.Forms.TextBox();
            this.comboBoxReference = new System.Windows.Forms.ComboBox();
            this.comboBoxLfpGain = new System.Windows.Forms.ComboBox();
            this.comboBoxApGain = new System.Windows.Forms.ComboBox();
            this.tabPageADCs = new System.Windows.Forms.TabPage();
            this.dataGridViewAdcs = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            toolStripStatusLabelGainCalSN = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelAdcCalSN = new System.Windows.Forms.ToolStripStatusLabel();
            labelPresets = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            labelGainCorrection = new System.Windows.Forms.Label();
            adcCalibrationFile = new System.Windows.Forms.Label();
            gainCalibrationFile = new System.Windows.Forms.Label();
            spikeFilter = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            lfpGain = new System.Windows.Forms.Label();
            apGain = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlProbe.SuspendLayout();
            this.tabPageProbe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panelProbe.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelOptions.SuspendLayout();
            this.tabPageADCs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAdcs)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripStatusLabelGainCalSN
            // 
            toolStripStatusLabelGainCalSN.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            toolStripStatusLabelGainCalSN.Name = "toolStripStatusLabelGainCalSN";
            toolStripStatusLabelGainCalSN.Size = new System.Drawing.Size(119, 15);
            toolStripStatusLabelGainCalSN.Text = "Gain Calibration SN: ";
            // 
            // toolStripStatusLabelAdcCalSN
            // 
            toolStripStatusLabelAdcCalSN.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            toolStripStatusLabelAdcCalSN.Name = "toolStripStatusLabelAdcCalSN";
            toolStripStatusLabelAdcCalSN.Size = new System.Drawing.Size(118, 15);
            toolStripStatusLabelAdcCalSN.Text = "ADC Calibration SN: ";
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(16, 313);
            labelPresets.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(84, 13);
            labelPresets.TabIndex = 25;
            labelPresets.Text = "Channel Presets";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(67, 82);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(29, 13);
            label4.TabIndex = 20;
            label4.Text = "Gain";
            // 
            // labelGainCorrection
            // 
            labelGainCorrection.AutoSize = true;
            labelGainCorrection.Location = new System.Drawing.Point(146, 82);
            labelGainCorrection.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelGainCorrection.Name = "labelGainCorrection";
            labelGainCorrection.Size = new System.Drawing.Size(55, 13);
            labelGainCorrection.TabIndex = 15;
            labelGainCorrection.Text = "Correction";
            // 
            // adcCalibrationFile
            // 
            adcCalibrationFile.AutoSize = true;
            adcCalibrationFile.Location = new System.Drawing.Point(16, 163);
            adcCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            adcCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            adcCalibrationFile.Name = "adcCalibrationFile";
            adcCalibrationFile.Size = new System.Drawing.Size(100, 13);
            adcCalibrationFile.TabIndex = 11;
            adcCalibrationFile.Text = "ADC Calibration File";
            // 
            // gainCalibrationFile
            // 
            gainCalibrationFile.AutoSize = true;
            gainCalibrationFile.Location = new System.Drawing.Point(16, 12);
            gainCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            gainCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            gainCalibrationFile.Name = "gainCalibrationFile";
            gainCalibrationFile.Size = new System.Drawing.Size(100, 13);
            gainCalibrationFile.TabIndex = 8;
            gainCalibrationFile.Text = "Gain Calibration File";
            // 
            // spikeFilter
            // 
            spikeFilter.AutoSize = true;
            spikeFilter.Location = new System.Drawing.Point(16, 272);
            spikeFilter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            spikeFilter.Name = "spikeFilter";
            spikeFilter.Size = new System.Drawing.Size(59, 13);
            spikeFilter.TabIndex = 6;
            spikeFilter.Text = "Spike Filter";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(16, 242);
            Reference.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(57, 13);
            Reference.TabIndex = 4;
            Reference.Text = "Reference";
            // 
            // lfpGain
            // 
            lfpGain.AutoSize = true;
            lfpGain.Location = new System.Drawing.Point(16, 133);
            lfpGain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lfpGain.Name = "lfpGain";
            lfpGain.Size = new System.Drawing.Size(26, 13);
            lfpGain.TabIndex = 2;
            lfpGain.Text = "LFP";
            // 
            // apGain
            // 
            apGain.AutoSize = true;
            apGain.Location = new System.Drawing.Point(16, 104);
            apGain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            apGain.Name = "apGain";
            apGain.Size = new System.Drawing.Size(21, 13);
            apGain.TabIndex = 0;
            apGain.Text = "AP";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(5, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(38, 13);
            label3.TabIndex = 32;
            label3.Text = "10 mm";
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 509);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(32, 13);
            label1.TabIndex = 31;
            label1.Text = "0 mm";
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(998, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripStatusLabelGainCalSN,
            this.gainCalibrationSN,
            toolStripStatusLabelAdcCalSN,
            this.adcCalibrationSN,
            this.toolStripStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 607);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip.Size = new System.Drawing.Size(998, 20);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // gainCalibrationSN
            // 
            this.gainCalibrationSN.AutoSize = false;
            this.gainCalibrationSN.Name = "gainCalibrationSN";
            this.gainCalibrationSN.Size = new System.Drawing.Size(150, 15);
            this.gainCalibrationSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // adcCalibrationSN
            // 
            this.adcCalibrationSN.AutoSize = false;
            this.adcCalibrationSN.Name = "adcCalibrationSN";
            this.adcCalibrationSN.Size = new System.Drawing.Size(150, 15);
            this.adcCalibrationSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatus
            // 
            this.toolStripStatus.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusReadyImage;
            this.toolStripStatus.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripStatus.Name = "toolStripStatus";
            this.toolStripStatus.Size = new System.Drawing.Size(16, 15);
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
            this.splitContainer1.Panel1.Controls.Add(this.tabControlProbe);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(998, 583);
            this.splitContainer1.SplitterDistance = 554;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControlProbe
            // 
            this.tabControlProbe.Controls.Add(this.tabPageProbe);
            this.tabControlProbe.Controls.Add(this.tabPageADCs);
            this.tabControlProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProbe.Location = new System.Drawing.Point(0, 0);
            this.tabControlProbe.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlProbe.Name = "tabControlProbe";
            this.tabControlProbe.SelectedIndex = 0;
            this.tabControlProbe.Size = new System.Drawing.Size(998, 554);
            this.tabControlProbe.TabIndex = 0;
            // 
            // tabPageProbe
            // 
            this.tabPageProbe.Controls.Add(this.splitContainer2);
            this.tabPageProbe.Location = new System.Drawing.Point(4, 22);
            this.tabPageProbe.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageProbe.Name = "tabPageProbe";
            this.tabPageProbe.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageProbe.Size = new System.Drawing.Size(990, 528);
            this.tabPageProbe.TabIndex = 0;
            this.tabPageProbe.Text = "Probe";
            this.tabPageProbe.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(2, 2);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer2.Panel1.Controls.Add(this.panelProbe);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelOptions);
            this.splitContainer2.Size = new System.Drawing.Size(986, 524);
            this.splitContainer2.SplitterDistance = 757;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // panelProbe
            // 
            this.panelProbe.BackColor = System.Drawing.SystemColors.Control;
            this.panelProbe.Controls.Add(this.panelTrackBar);
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(0, 0);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(757, 524);
            this.panelProbe.TabIndex = 0;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.panelTrackBar.Controls.Add(label1);
            this.panelTrackBar.Controls.Add(label3);
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Location = new System.Drawing.Point(706, 0);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(48, 524);
            this.panelTrackBar.TabIndex = 33;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarProbePosition.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarProbePosition.Location = new System.Drawing.Point(2, 2);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(45, 520);
            this.trackBarProbePosition.TabIndex = 30;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // panelOptions
            // 
            this.panelOptions.Controls.Add(this.buttonEnableContacts);
            this.panelOptions.Controls.Add(this.buttonClearSelections);
            this.panelOptions.Controls.Add(this.comboBoxChannelPresets);
            this.panelOptions.Controls.Add(labelPresets);
            this.panelOptions.Controls.Add(this.buttonResetZoom);
            this.panelOptions.Controls.Add(this.buttonClearAdcCalibrationFile);
            this.panelOptions.Controls.Add(label4);
            this.panelOptions.Controls.Add(this.buttonClearGainCalibrationFile);
            this.panelOptions.Controls.Add(this.textBoxLfpGainCorrection);
            this.panelOptions.Controls.Add(this.textBoxApGainCorrection);
            this.panelOptions.Controls.Add(labelGainCorrection);
            this.panelOptions.Controls.Add(this.checkBoxSpikeFilter);
            this.panelOptions.Controls.Add(this.buttonChooseAdcCalibrationFile);
            this.panelOptions.Controls.Add(this.textBoxAdcCalibrationFile);
            this.panelOptions.Controls.Add(adcCalibrationFile);
            this.panelOptions.Controls.Add(this.buttonChooseGainCalibrationFile);
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
            this.panelOptions.Location = new System.Drawing.Point(0, 0);
            this.panelOptions.Margin = new System.Windows.Forms.Padding(2);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(226, 524);
            this.panelOptions.TabIndex = 2;
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Location = new System.Drawing.Point(16, 349);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(94, 36);
            this.buttonEnableContacts.TabIndex = 28;
            this.buttonEnableContacts.Text = "Enable Selected Contacts";
            this.buttonEnableContacts.UseVisualStyleBackColor = true;
            this.buttonEnableContacts.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearSelections
            // 
            this.buttonClearSelections.Location = new System.Drawing.Point(121, 348);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(94, 36);
            this.buttonClearSelections.TabIndex = 27;
            this.buttonClearSelections.Text = "Deselect Contacts";
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(103, 309);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(112, 21);
            this.comboBoxChannelPresets.TabIndex = 26;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Location = new System.Drawing.Point(66, 389);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(94, 36);
            this.buttonResetZoom.TabIndex = 22;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearAdcCalibrationFile
            // 
            this.buttonClearAdcCalibrationFile.Location = new System.Drawing.Point(121, 202);
            this.buttonClearAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearAdcCalibrationFile.Name = "buttonClearAdcCalibrationFile";
            this.buttonClearAdcCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonClearAdcCalibrationFile.TabIndex = 21;
            this.buttonClearAdcCalibrationFile.Text = "Clear";
            this.buttonClearAdcCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonClearAdcCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonClearGainCalibrationFile
            // 
            this.buttonClearGainCalibrationFile.Location = new System.Drawing.Point(121, 51);
            this.buttonClearGainCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearGainCalibrationFile.Name = "buttonClearGainCalibrationFile";
            this.buttonClearGainCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonClearGainCalibrationFile.TabIndex = 19;
            this.buttonClearGainCalibrationFile.Text = "Clear";
            this.buttonClearGainCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonClearGainCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxLfpGainCorrection
            // 
            this.textBoxLfpGainCorrection.Location = new System.Drawing.Point(132, 129);
            this.textBoxLfpGainCorrection.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxLfpGainCorrection.Name = "textBoxLfpGainCorrection";
            this.textBoxLfpGainCorrection.ReadOnly = true;
            this.textBoxLfpGainCorrection.Size = new System.Drawing.Size(83, 20);
            this.textBoxLfpGainCorrection.TabIndex = 18;
            // 
            // textBoxApGainCorrection
            // 
            this.textBoxApGainCorrection.Location = new System.Drawing.Point(132, 100);
            this.textBoxApGainCorrection.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxApGainCorrection.Name = "textBoxApGainCorrection";
            this.textBoxApGainCorrection.ReadOnly = true;
            this.textBoxApGainCorrection.Size = new System.Drawing.Size(83, 20);
            this.textBoxApGainCorrection.TabIndex = 16;
            // 
            // checkBoxSpikeFilter
            // 
            this.checkBoxSpikeFilter.AutoSize = true;
            this.checkBoxSpikeFilter.Location = new System.Drawing.Point(103, 270);
            this.checkBoxSpikeFilter.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSpikeFilter.Name = "checkBoxSpikeFilter";
            this.checkBoxSpikeFilter.Size = new System.Drawing.Size(65, 17);
            this.checkBoxSpikeFilter.TabIndex = 14;
            this.checkBoxSpikeFilter.Text = "Enabled";
            this.checkBoxSpikeFilter.UseVisualStyleBackColor = true;
            // 
            // buttonChooseAdcCalibrationFile
            // 
            this.buttonChooseAdcCalibrationFile.Location = new System.Drawing.Point(16, 202);
            this.buttonChooseAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseAdcCalibrationFile.Name = "buttonChooseAdcCalibrationFile";
            this.buttonChooseAdcCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonChooseAdcCalibrationFile.TabIndex = 13;
            this.buttonChooseAdcCalibrationFile.Text = "Choose";
            this.buttonChooseAdcCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseAdcCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxAdcCalibrationFile
            // 
            this.textBoxAdcCalibrationFile.Location = new System.Drawing.Point(16, 178);
            this.textBoxAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAdcCalibrationFile.Name = "textBoxAdcCalibrationFile";
            this.textBoxAdcCalibrationFile.ReadOnly = true;
            this.textBoxAdcCalibrationFile.Size = new System.Drawing.Size(199, 20);
            this.textBoxAdcCalibrationFile.TabIndex = 12;
            this.textBoxAdcCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxAdcCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // buttonChooseGainCalibrationFile
            // 
            this.buttonChooseGainCalibrationFile.Location = new System.Drawing.Point(16, 51);
            this.buttonChooseGainCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseGainCalibrationFile.Name = "buttonChooseGainCalibrationFile";
            this.buttonChooseGainCalibrationFile.Size = new System.Drawing.Size(94, 21);
            this.buttonChooseGainCalibrationFile.TabIndex = 10;
            this.buttonChooseGainCalibrationFile.Text = "Choose";
            this.buttonChooseGainCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseGainCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // textBoxGainCalibrationFile
            // 
            this.textBoxGainCalibrationFile.Location = new System.Drawing.Point(16, 27);
            this.textBoxGainCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxGainCalibrationFile.Name = "textBoxGainCalibrationFile";
            this.textBoxGainCalibrationFile.ReadOnly = true;
            this.textBoxGainCalibrationFile.Size = new System.Drawing.Size(199, 20);
            this.textBoxGainCalibrationFile.TabIndex = 9;
            this.textBoxGainCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxGainCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(103, 238);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(112, 21);
            this.comboBoxReference.TabIndex = 5;
            // 
            // comboBoxLfpGain
            // 
            this.comboBoxLfpGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLfpGain.FormattingEnabled = true;
            this.comboBoxLfpGain.Location = new System.Drawing.Point(40, 129);
            this.comboBoxLfpGain.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxLfpGain.Name = "comboBoxLfpGain";
            this.comboBoxLfpGain.Size = new System.Drawing.Size(82, 21);
            this.comboBoxLfpGain.TabIndex = 3;
            // 
            // comboBoxApGain
            // 
            this.comboBoxApGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxApGain.FormattingEnabled = true;
            this.comboBoxApGain.Location = new System.Drawing.Point(40, 100);
            this.comboBoxApGain.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxApGain.Name = "comboBoxApGain";
            this.comboBoxApGain.Size = new System.Drawing.Size(82, 21);
            this.comboBoxApGain.TabIndex = 1;
            // 
            // tabPageADCs
            // 
            this.tabPageADCs.Controls.Add(this.dataGridViewAdcs);
            this.tabPageADCs.Location = new System.Drawing.Point(4, 22);
            this.tabPageADCs.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageADCs.Name = "tabPageADCs";
            this.tabPageADCs.Size = new System.Drawing.Size(990, 527);
            this.tabPageADCs.TabIndex = 2;
            this.tabPageADCs.Text = "ADCs";
            this.tabPageADCs.UseVisualStyleBackColor = true;
            // 
            // dataGridViewAdcs
            // 
            this.dataGridViewAdcs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAdcs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewAdcs.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewAdcs.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewAdcs.Name = "dataGridViewAdcs";
            this.dataGridViewAdcs.RowHeadersWidth = 62;
            this.dataGridViewAdcs.RowTemplate.Height = 28;
            this.dataGridViewAdcs.Size = new System.Drawing.Size(990, 527);
            this.dataGridViewAdcs.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOkay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(998, 26);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(908, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 22);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOkay
            // 
            this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOkay.Location = new System.Drawing.Point(819, 0);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(83, 22);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            this.buttonOkay.Click += new System.EventHandler(this.ButtonClick);
            // 
            // NeuropixelsV1eDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 627);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NeuropixelsV1eDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NeuropixelsV1eDialog";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlProbe.ResumeLayout(false);
            this.tabPageProbe.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panelProbe.ResumeLayout(false);
            this.panelTrackBar.ResumeLayout(false);
            this.panelTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelOptions.ResumeLayout(false);
            this.panelOptions.PerformLayout();
            this.tabPageADCs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAdcs)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControlProbe;
        private System.Windows.Forms.TabPage tabPageProbe;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOkay;
        private System.Windows.Forms.ToolStripStatusLabel gainCalibrationSN;
        private System.Windows.Forms.ToolStripStatusLabel adcCalibrationSN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.TabPage tabPageADCs;
        private System.Windows.Forms.DataGridView dataGridViewAdcs;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Button buttonEnableContacts;
        private System.Windows.Forms.Button buttonClearSelections;
        private System.Windows.Forms.ComboBox comboBoxChannelPresets;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.Button buttonClearAdcCalibrationFile;
        private System.Windows.Forms.Button buttonClearGainCalibrationFile;
        private System.Windows.Forms.TextBox textBoxLfpGainCorrection;
        private System.Windows.Forms.TextBox textBoxApGainCorrection;
        private System.Windows.Forms.CheckBox checkBoxSpikeFilter;
        private System.Windows.Forms.Button buttonChooseAdcCalibrationFile;
        private System.Windows.Forms.TextBox textBoxAdcCalibrationFile;
        private System.Windows.Forms.Button buttonChooseGainCalibrationFile;
        private System.Windows.Forms.TextBox textBoxGainCalibrationFile;
        private System.Windows.Forms.ComboBox comboBoxReference;
        private System.Windows.Forms.ComboBox comboBoxLfpGain;
        private System.Windows.Forms.ComboBox comboBoxApGain;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.TrackBar trackBarProbePosition;
        private System.Windows.Forms.Panel panelTrackBar;
    }
}
