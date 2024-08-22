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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label labelPresets;
            System.Windows.Forms.Label adcCalibrationFile;
            System.Windows.Forms.Label gainCalibrationFile;
            System.Windows.Forms.Label spikeFilter;
            System.Windows.Forms.Label Reference;
            System.Windows.Forms.Label lfpGain;
            System.Windows.Forms.Label apGain;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV1eDialog));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelTrackBar = new System.Windows.Forms.Panel();
            this.trackBarProbePosition = new System.Windows.Forms.TrackBar();
            this.panelOptions = new System.Windows.Forms.Panel();
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOkay = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            labelPresets = new System.Windows.Forms.Label();
            adcCalibrationFile = new System.Windows.Forms.Label();
            gainCalibrationFile = new System.Windows.Forms.Label();
            spikeFilter = new System.Windows.Forms.Label();
            Reference = new System.Windows.Forms.Label();
            lfpGain = new System.Windows.Forms.Label();
            apGain = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.panelProbe.SuspendLayout();
            this.panelTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).BeginInit();
            this.panelOptions.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelPresets
            // 
            labelPresets.AutoSize = true;
            labelPresets.Location = new System.Drawing.Point(10, 217);
            labelPresets.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPresets.Name = "labelPresets";
            labelPresets.Size = new System.Drawing.Size(46, 26);
            labelPresets.TabIndex = 25;
            labelPresets.Text = "Channel\r\nPresets:";
            // 
            // adcCalibrationFile
            // 
            adcCalibrationFile.AutoSize = true;
            adcCalibrationFile.Location = new System.Drawing.Point(10, 9);
            adcCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            adcCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            adcCalibrationFile.Name = "adcCalibrationFile";
            adcCalibrationFile.Size = new System.Drawing.Size(103, 13);
            adcCalibrationFile.TabIndex = 11;
            adcCalibrationFile.Text = "ADC Calibration File:";
            // 
            // gainCalibrationFile
            // 
            gainCalibrationFile.AutoSize = true;
            gainCalibrationFile.Location = new System.Drawing.Point(10, 57);
            gainCalibrationFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            gainCalibrationFile.MaximumSize = new System.Drawing.Size(133, 29);
            gainCalibrationFile.Name = "gainCalibrationFile";
            gainCalibrationFile.Size = new System.Drawing.Size(103, 13);
            gainCalibrationFile.TabIndex = 8;
            gainCalibrationFile.Text = "Gain Calibration File:";
            // 
            // spikeFilter
            // 
            spikeFilter.AutoSize = true;
            spikeFilter.Location = new System.Drawing.Point(10, 168);
            spikeFilter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            spikeFilter.Name = "spikeFilter";
            spikeFilter.Size = new System.Drawing.Size(62, 13);
            spikeFilter.TabIndex = 6;
            spikeFilter.Text = "Spike Filter:";
            // 
            // Reference
            // 
            Reference.AutoSize = true;
            Reference.Location = new System.Drawing.Point(10, 195);
            Reference.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            Reference.Name = "Reference";
            Reference.Size = new System.Drawing.Size(60, 13);
            Reference.TabIndex = 4;
            Reference.Text = "Reference:";
            // 
            // lfpGain
            // 
            lfpGain.AutoSize = true;
            lfpGain.Location = new System.Drawing.Point(10, 139);
            lfpGain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lfpGain.Name = "lfpGain";
            lfpGain.Size = new System.Drawing.Size(54, 13);
            lfpGain.TabIndex = 2;
            lfpGain.Text = "LFP Gain:";
            // 
            // apGain
            // 
            apGain.AutoSize = true;
            apGain.Location = new System.Drawing.Point(10, 110);
            apGain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            apGain.Name = "apGain";
            apGain.Size = new System.Drawing.Size(49, 13);
            apGain.TabIndex = 0;
            apGain.Text = "AP Gain:";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(38, 13);
            label3.TabIndex = 32;
            label3.Text = "10 mm";
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 552);
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
            this.menuStrip.Size = new System.Drawing.Size(984, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // panelProbe
            // 
            this.panelProbe.BackColor = System.Drawing.SystemColors.Control;
            this.panelProbe.Controls.Add(this.panelTrackBar);
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(3, 3);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(732, 566);
            this.panelProbe.TabIndex = 0;
            // 
            // panelTrackBar
            // 
            this.panelTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.panelTrackBar.Controls.Add(label1);
            this.panelTrackBar.Controls.Add(label3);
            this.panelTrackBar.Controls.Add(this.trackBarProbePosition);
            this.panelTrackBar.Location = new System.Drawing.Point(691, 1);
            this.panelTrackBar.Name = "panelTrackBar";
            this.panelTrackBar.Size = new System.Drawing.Size(38, 564);
            this.panelTrackBar.TabIndex = 33;
            // 
            // trackBarProbePosition
            // 
            this.trackBarProbePosition.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarProbePosition.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarProbePosition.Location = new System.Drawing.Point(3, 7);
            this.trackBarProbePosition.Margin = new System.Windows.Forms.Padding(2);
            this.trackBarProbePosition.Maximum = 100;
            this.trackBarProbePosition.Name = "trackBarProbePosition";
            this.trackBarProbePosition.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarProbePosition.Size = new System.Drawing.Size(45, 550);
            this.trackBarProbePosition.TabIndex = 30;
            this.trackBarProbePosition.TickFrequency = 2;
            this.trackBarProbePosition.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarProbePosition.Value = 50;
            this.trackBarProbePosition.Scroll += new System.EventHandler(this.TrackBarScroll);
            // 
            // panelOptions
            // 
            this.panelOptions.BackColor = System.Drawing.SystemColors.ControlLightLight;
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
            this.panelOptions.Location = new System.Drawing.Point(740, 2);
            this.panelOptions.Margin = new System.Windows.Forms.Padding(2);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(242, 568);
            this.panelOptions.TabIndex = 2;
            // 
            // buttonChooseAdcCalibrationFile
            // 
            this.buttonChooseAdcCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseAdcCalibrationFile.Location = new System.Drawing.Point(205, 24);
            this.buttonChooseAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseAdcCalibrationFile.Name = "buttonChooseAdcCalibrationFile";
            this.buttonChooseAdcCalibrationFile.Size = new System.Drawing.Size(28, 20);
            this.buttonChooseAdcCalibrationFile.TabIndex = 36;
            this.buttonChooseAdcCalibrationFile.Text = "...";
            this.toolTip.SetToolTip(this.buttonChooseAdcCalibrationFile, "Browse for an ADC calibration file.");
            this.buttonChooseAdcCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseAdcCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonChooseGainCalibrationFile
            // 
            this.buttonChooseGainCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseGainCalibrationFile.Location = new System.Drawing.Point(205, 72);
            this.buttonChooseGainCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonChooseGainCalibrationFile.Name = "buttonChooseGainCalibrationFile";
            this.buttonChooseGainCalibrationFile.Size = new System.Drawing.Size(28, 20);
            this.buttonChooseGainCalibrationFile.TabIndex = 35;
            this.buttonChooseGainCalibrationFile.Text = "...";
            this.toolTip.SetToolTip(this.buttonChooseGainCalibrationFile, "Browse for a gain calibration file.");
            this.buttonChooseGainCalibrationFile.UseVisualStyleBackColor = true;
            this.buttonChooseGainCalibrationFile.Click += new System.EventHandler(this.ButtonClick);
            // 
            // buttonEnableContacts
            // 
            this.buttonEnableContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEnableContacts.Location = new System.Drawing.Point(10, 265);
            this.buttonEnableContacts.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnableContacts.Name = "buttonEnableContacts";
            this.buttonEnableContacts.Size = new System.Drawing.Size(223, 36);
            this.buttonEnableContacts.TabIndex = 28;
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
            this.buttonClearSelections.Location = new System.Drawing.Point(10, 305);
            this.buttonClearSelections.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearSelections.Name = "buttonClearSelections";
            this.buttonClearSelections.Size = new System.Drawing.Size(223, 36);
            this.buttonClearSelections.TabIndex = 27;
            this.buttonClearSelections.Text = "Clear Electrode Selection";
            this.toolTip.SetToolTip(this.buttonClearSelections, "Deselect all electrodes in the probe view. \r\nNote that this does not disable elec" +
        "trodes, but simply deselects them.");
            this.buttonClearSelections.UseVisualStyleBackColor = true;
            this.buttonClearSelections.Click += new System.EventHandler(this.ButtonClick);
            // 
            // comboBoxChannelPresets
            // 
            this.comboBoxChannelPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxChannelPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannelPresets.FormattingEnabled = true;
            this.comboBoxChannelPresets.Location = new System.Drawing.Point(76, 222);
            this.comboBoxChannelPresets.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxChannelPresets.Name = "comboBoxChannelPresets";
            this.comboBoxChannelPresets.Size = new System.Drawing.Size(157, 21);
            this.comboBoxChannelPresets.TabIndex = 26;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetZoom.Location = new System.Drawing.Point(10, 345);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(223, 36);
            this.buttonResetZoom.TabIndex = 22;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.toolTip.SetToolTip(this.buttonResetZoom, "Reset the zoom in the probe view so that the probe is zoomed out and centered.");
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonClick);
            // 
            // checkBoxSpikeFilter
            // 
            this.checkBoxSpikeFilter.AutoSize = true;
            this.checkBoxSpikeFilter.Location = new System.Drawing.Point(76, 167);
            this.checkBoxSpikeFilter.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSpikeFilter.Name = "checkBoxSpikeFilter";
            this.checkBoxSpikeFilter.Size = new System.Drawing.Size(65, 17);
            this.checkBoxSpikeFilter.TabIndex = 14;
            this.checkBoxSpikeFilter.Text = "Enabled";
            this.checkBoxSpikeFilter.UseVisualStyleBackColor = true;
            // 
            // textBoxAdcCalibrationFile
            // 
            this.textBoxAdcCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAdcCalibrationFile.Location = new System.Drawing.Point(10, 24);
            this.textBoxAdcCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAdcCalibrationFile.Name = "textBoxAdcCalibrationFile";
            this.textBoxAdcCalibrationFile.Size = new System.Drawing.Size(191, 20);
            this.textBoxAdcCalibrationFile.TabIndex = 12;
            this.textBoxAdcCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxAdcCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // textBoxGainCalibrationFile
            // 
            this.textBoxGainCalibrationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGainCalibrationFile.Location = new System.Drawing.Point(10, 72);
            this.textBoxGainCalibrationFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxGainCalibrationFile.Name = "textBoxGainCalibrationFile";
            this.textBoxGainCalibrationFile.Size = new System.Drawing.Size(191, 20);
            this.textBoxGainCalibrationFile.TabIndex = 9;
            this.textBoxGainCalibrationFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxGainCalibrationFile.TextChanged += new System.EventHandler(this.FileTextChanged);
            // 
            // comboBoxReference
            // 
            this.comboBoxReference.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxReference.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReference.FormattingEnabled = true;
            this.comboBoxReference.Location = new System.Drawing.Point(76, 192);
            this.comboBoxReference.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxReference.Name = "comboBoxReference";
            this.comboBoxReference.Size = new System.Drawing.Size(157, 21);
            this.comboBoxReference.TabIndex = 5;
            // 
            // comboBoxLfpGain
            // 
            this.comboBoxLfpGain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLfpGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLfpGain.FormattingEnabled = true;
            this.comboBoxLfpGain.Location = new System.Drawing.Point(76, 136);
            this.comboBoxLfpGain.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxLfpGain.Name = "comboBoxLfpGain";
            this.comboBoxLfpGain.Size = new System.Drawing.Size(157, 21);
            this.comboBoxLfpGain.TabIndex = 3;
            // 
            // comboBoxApGain
            // 
            this.comboBoxApGain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxApGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxApGain.FormattingEnabled = true;
            this.comboBoxApGain.Location = new System.Drawing.Point(76, 106);
            this.comboBoxApGain.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxApGain.Name = "comboBoxApGain";
            this.comboBoxApGain.Size = new System.Drawing.Size(157, 21);
            this.comboBoxApGain.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(893, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 28);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOkay
            // 
            this.buttonOkay.Location = new System.Drawing.Point(806, 2);
            this.buttonOkay.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOkay.Name = "buttonOkay";
            this.buttonOkay.Size = new System.Drawing.Size(83, 28);
            this.buttonOkay.TabIndex = 0;
            this.buttonOkay.Text = "OK";
            this.buttonOkay.UseVisualStyleBackColor = true;
            this.buttonOkay.Click += new System.EventHandler(this.ButtonClick);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(984, 612);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOkay);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 575);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(978, 34);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // NeuropixelsV1eDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 636);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NeuropixelsV1eDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NeuropixelsV1e Configuration";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panelProbe.ResumeLayout(false);
            this.panelTrackBar.ResumeLayout(false);
            this.panelTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarProbePosition)).EndInit();
            this.panelOptions.ResumeLayout(false);
            this.panelOptions.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOkay;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Button buttonEnableContacts;
        private System.Windows.Forms.Button buttonClearSelections;
        private System.Windows.Forms.ComboBox comboBoxChannelPresets;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.CheckBox checkBoxSpikeFilter;
        private System.Windows.Forms.TextBox textBoxAdcCalibrationFile;
        private System.Windows.Forms.TextBox textBoxGainCalibrationFile;
        private System.Windows.Forms.ComboBox comboBoxReference;
        private System.Windows.Forms.ComboBox comboBoxLfpGain;
        private System.Windows.Forms.ComboBox comboBoxApGain;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.TrackBar trackBarProbePosition;
        private System.Windows.Forms.Panel panelTrackBar;
        private System.Windows.Forms.Button buttonChooseGainCalibrationFile;
        private System.Windows.Forms.Button buttonChooseAdcCalibrationFile;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
