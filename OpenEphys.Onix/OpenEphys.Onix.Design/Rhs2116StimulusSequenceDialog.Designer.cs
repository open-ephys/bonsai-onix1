namespace OpenEphys.Onix.Design
{
    partial class Rhs2116StimulusSequenceDialog
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
            this.zedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.linkLabelDocumentation = new System.Windows.Forms.LinkLabel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusSlotsUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.tabControlParameters = new System.Windows.Forms.TabControl();
            this.tabPageAddPulse = new System.Windows.Forms.TabPage();
            this.panelParameters = new System.Windows.Forms.Panel();
            this.groupBoxCathode = new System.Windows.Forms.GroupBox();
            this.labelAmplitudeCathodic = new System.Windows.Forms.Label();
            this.labelPulseWidthCathodic = new System.Windows.Forms.Label();
            this.pulseWidthCathodic = new System.Windows.Forms.TextBox();
            this.amplitudeCathodic = new System.Windows.Forms.TextBox();
            this.groupBoxAnode = new System.Windows.Forms.GroupBox();
            this.labelAmplitudeAnodic = new System.Windows.Forms.Label();
            this.labelPulseWidthAnodic = new System.Windows.Forms.Label();
            this.pulseWidthAnodic = new System.Windows.Forms.TextBox();
            this.amplitudeAnodic = new System.Windows.Forms.TextBox();
            this.comboBoxStepSize = new System.Windows.Forms.ComboBox();
            this.buttonClearPulses = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.interPulseInterval = new System.Windows.Forms.TextBox();
            this.labelInterPulseInterval = new System.Windows.Forms.Label();
            this.labelStepSizeAmplitude = new System.Windows.Forms.Label();
            this.checkBoxAnodicFirst = new System.Windows.Forms.CheckBox();
            this.buttonAddPulses = new System.Windows.Forms.Button();
            this.delay = new System.Windows.Forms.TextBox();
            this.labelNumberOfPulses = new System.Windows.Forms.Label();
            this.labelDelay = new System.Windows.Forms.Label();
            this.numberOfStimuli = new System.Windows.Forms.TextBox();
            this.checkboxBiphasicSymmetrical = new System.Windows.Forms.CheckBox();
            this.interStimulusInterval = new System.Windows.Forms.TextBox();
            this.labelInterStimulusInterval = new System.Windows.Forms.Label();
            this.tabPageChannelLayout = new System.Windows.Forms.TabPage();
            this.panelChannelLayout = new System.Windows.Forms.Panel();
            this.buttonCustomChannelLayout = new System.Windows.Forms.Button();
            this.buttonDefaultChannelLayout = new System.Windows.Forms.Button();
            this.textBoxChannelLayoutFilePath = new System.Windows.Forms.TextBox();
            this.tabPageEditorDialog = new System.Windows.Forms.TabPage();
            this.propertyGridStimulusSequence = new System.Windows.Forms.PropertyGrid();
            this.tabControlVisualization = new System.Windows.Forms.TabControl();
            this.tabPageWaveform = new System.Windows.Forms.TabPage();
            this.tabPageTable = new System.Windows.Forms.TabPage();
            this.dataGridViewStimulusTable = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.zedGraphChannels = new ZedGraph.ZedGraphControl();
            this.statusStrip.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.tabControlParameters.SuspendLayout();
            this.tabPageAddPulse.SuspendLayout();
            this.panelParameters.SuspendLayout();
            this.groupBoxCathode.SuspendLayout();
            this.groupBoxAnode.SuspendLayout();
            this.tabPageChannelLayout.SuspendLayout();
            this.panelChannelLayout.SuspendLayout();
            this.tabPageEditorDialog.SuspendLayout();
            this.tabControlVisualization.SuspendLayout();
            this.tabPageWaveform.SuspendLayout();
            this.tabPageTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // zedGraphWaveform
            // 
            this.zedGraphWaveform.AutoScroll = true;
            this.zedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphWaveform.Location = new System.Drawing.Point(3, 3);
            this.zedGraphWaveform.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphWaveform.Name = "zedGraphWaveform";
            this.zedGraphWaveform.ScrollGrace = 0D;
            this.zedGraphWaveform.ScrollMaxX = 0D;
            this.zedGraphWaveform.ScrollMaxY = 0D;
            this.zedGraphWaveform.ScrollMaxY2 = 0D;
            this.zedGraphWaveform.ScrollMinX = 0D;
            this.zedGraphWaveform.ScrollMinY = 0D;
            this.zedGraphWaveform.ScrollMinY2 = 0D;
            this.zedGraphWaveform.Size = new System.Drawing.Size(1316, 821);
            this.zedGraphWaveform.TabIndex = 4;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // linkLabelDocumentation
            // 
            this.linkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDocumentation.AutoSize = true;
            this.linkLabelDocumentation.Location = new System.Drawing.Point(1677, 865);
            this.linkLabelDocumentation.Name = "linkLabelDocumentation";
            this.linkLabelDocumentation.Size = new System.Drawing.Size(118, 20);
            this.linkLabelDocumentation.TabIndex = 4;
            this.linkLabelDocumentation.TabStop = true;
            this.linkLabelDocumentation.Text = "Documentation";
            this.linkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDocumentation_LinkClicked);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(354, 12);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 40);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusIsValid,
            this.toolStripStatusSlotsUsed});
            this.statusStrip.Location = new System.Drawing.Point(0, 860);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.statusStrip.Size = new System.Drawing.Size(1803, 32);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusIsValid
            // 
            this.toolStripStatusIsValid.AutoSize = false;
            this.toolStripStatusIsValid.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusIsValid.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripStatusIsValid.Image = global::OpenEphys.Onix.Design.Properties.Resources.StatusReadyImage;
            this.toolStripStatusIsValid.Name = "toolStripStatusIsValid";
            this.toolStripStatusIsValid.Size = new System.Drawing.Size(290, 25);
            this.toolStripStatusIsValid.Text = "Valid stimulus sequence";
            this.toolStripStatusIsValid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusSlotsUsed
            // 
            this.toolStripStatusSlotsUsed.Name = "toolStripStatusSlotsUsed";
            this.toolStripStatusSlotsUsed.Size = new System.Drawing.Size(164, 25);
            this.toolStripStatusSlotsUsed.Text = "100% of slots used";
            this.toolStripStatusSlotsUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.buttonOk);
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(0, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(469, 60);
            this.panelButtons.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Location = new System.Drawing.Point(249, 12);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 40);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // tabControlParameters
            // 
            this.tabControlParameters.Controls.Add(this.tabPageAddPulse);
            this.tabControlParameters.Controls.Add(this.tabPageChannelLayout);
            this.tabControlParameters.Controls.Add(this.tabPageEditorDialog);
            this.tabControlParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlParameters.Location = new System.Drawing.Point(0, 0);
            this.tabControlParameters.Name = "tabControlParameters";
            this.tabControlParameters.SelectedIndex = 0;
            this.tabControlParameters.Size = new System.Drawing.Size(469, 336);
            this.tabControlParameters.TabIndex = 1;
            // 
            // tabPageAddPulse
            // 
            this.tabPageAddPulse.Controls.Add(this.panelParameters);
            this.tabPageAddPulse.Location = new System.Drawing.Point(4, 29);
            this.tabPageAddPulse.Name = "tabPageAddPulse";
            this.tabPageAddPulse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAddPulse.Size = new System.Drawing.Size(461, 303);
            this.tabPageAddPulse.TabIndex = 0;
            this.tabPageAddPulse.Text = "Add Pulses";
            this.tabPageAddPulse.UseVisualStyleBackColor = true;
            // 
            // panelParameters
            // 
            this.panelParameters.AutoScroll = true;
            this.panelParameters.Controls.Add(this.groupBoxCathode);
            this.panelParameters.Controls.Add(this.groupBoxAnode);
            this.panelParameters.Controls.Add(this.comboBoxStepSize);
            this.panelParameters.Controls.Add(this.buttonClearPulses);
            this.panelParameters.Controls.Add(this.button1);
            this.panelParameters.Controls.Add(this.interPulseInterval);
            this.panelParameters.Controls.Add(this.labelInterPulseInterval);
            this.panelParameters.Controls.Add(this.labelStepSizeAmplitude);
            this.panelParameters.Controls.Add(this.checkBoxAnodicFirst);
            this.panelParameters.Controls.Add(this.buttonAddPulses);
            this.panelParameters.Controls.Add(this.delay);
            this.panelParameters.Controls.Add(this.labelNumberOfPulses);
            this.panelParameters.Controls.Add(this.labelDelay);
            this.panelParameters.Controls.Add(this.numberOfStimuli);
            this.panelParameters.Controls.Add(this.checkboxBiphasicSymmetrical);
            this.panelParameters.Controls.Add(this.interStimulusInterval);
            this.panelParameters.Controls.Add(this.labelInterStimulusInterval);
            this.panelParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParameters.Location = new System.Drawing.Point(3, 3);
            this.panelParameters.Name = "panelParameters";
            this.panelParameters.Size = new System.Drawing.Size(455, 297);
            this.panelParameters.TabIndex = 0;
            // 
            // groupBoxCathode
            // 
            this.groupBoxCathode.Controls.Add(this.labelAmplitudeCathodic);
            this.groupBoxCathode.Controls.Add(this.labelPulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.pulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.amplitudeCathodic);
            this.groupBoxCathode.Location = new System.Drawing.Point(228, 118);
            this.groupBoxCathode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxCathode.Name = "groupBoxCathode";
            this.groupBoxCathode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxCathode.Size = new System.Drawing.Size(219, 86);
            this.groupBoxCathode.TabIndex = 3;
            this.groupBoxCathode.TabStop = false;
            this.groupBoxCathode.Text = "Cathode";
            this.groupBoxCathode.Visible = false;
            // 
            // labelAmplitudeCathodic
            // 
            this.labelAmplitudeCathodic.AutoSize = true;
            this.labelAmplitudeCathodic.Location = new System.Drawing.Point(12, 24);
            this.labelAmplitudeCathodic.Name = "labelAmplitudeCathodic";
            this.labelAmplitudeCathodic.Size = new System.Drawing.Size(116, 20);
            this.labelAmplitudeCathodic.TabIndex = 23;
            this.labelAmplitudeCathodic.Text = "Amplitude [mA]";
            // 
            // labelPulseWidthCathodic
            // 
            this.labelPulseWidthCathodic.AutoSize = true;
            this.labelPulseWidthCathodic.Location = new System.Drawing.Point(12, 55);
            this.labelPulseWidthCathodic.Name = "labelPulseWidthCathodic";
            this.labelPulseWidthCathodic.Size = new System.Drawing.Size(126, 20);
            this.labelPulseWidthCathodic.TabIndex = 24;
            this.labelPulseWidthCathodic.Text = "Pulse Width [ms]";
            // 
            // pulseWidthCathodic
            // 
            this.pulseWidthCathodic.Location = new System.Drawing.Point(148, 49);
            this.pulseWidthCathodic.Name = "pulseWidthCathodic";
            this.pulseWidthCathodic.Size = new System.Drawing.Size(60, 26);
            this.pulseWidthCathodic.TabIndex = 6;
            this.pulseWidthCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.pulseWidthCathodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // amplitudeCathodic
            // 
            this.amplitudeCathodic.Location = new System.Drawing.Point(148, 18);
            this.amplitudeCathodic.Name = "amplitudeCathodic";
            this.amplitudeCathodic.Size = new System.Drawing.Size(60, 26);
            this.amplitudeCathodic.TabIndex = 5;
            this.amplitudeCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Amplitude);
            this.amplitudeCathodic.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // groupBoxAnode
            // 
            this.groupBoxAnode.Controls.Add(this.labelAmplitudeAnodic);
            this.groupBoxAnode.Controls.Add(this.labelPulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.pulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.amplitudeAnodic);
            this.groupBoxAnode.Location = new System.Drawing.Point(5, 118);
            this.groupBoxAnode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxAnode.Name = "groupBoxAnode";
            this.groupBoxAnode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxAnode.Size = new System.Drawing.Size(219, 86);
            this.groupBoxAnode.TabIndex = 2;
            this.groupBoxAnode.TabStop = false;
            this.groupBoxAnode.Text = "Anode";
            // 
            // labelAmplitudeAnodic
            // 
            this.labelAmplitudeAnodic.AutoSize = true;
            this.labelAmplitudeAnodic.Location = new System.Drawing.Point(9, 24);
            this.labelAmplitudeAnodic.Name = "labelAmplitudeAnodic";
            this.labelAmplitudeAnodic.Size = new System.Drawing.Size(116, 20);
            this.labelAmplitudeAnodic.TabIndex = 4;
            this.labelAmplitudeAnodic.Text = "Amplitude [mA]";
            // 
            // labelPulseWidthAnodic
            // 
            this.labelPulseWidthAnodic.AutoSize = true;
            this.labelPulseWidthAnodic.Location = new System.Drawing.Point(9, 55);
            this.labelPulseWidthAnodic.Name = "labelPulseWidthAnodic";
            this.labelPulseWidthAnodic.Size = new System.Drawing.Size(126, 20);
            this.labelPulseWidthAnodic.TabIndex = 7;
            this.labelPulseWidthAnodic.Text = "Pulse Width [ms]";
            // 
            // pulseWidthAnodic
            // 
            this.pulseWidthAnodic.Location = new System.Drawing.Point(145, 49);
            this.pulseWidthAnodic.Name = "pulseWidthAnodic";
            this.pulseWidthAnodic.Size = new System.Drawing.Size(60, 26);
            this.pulseWidthAnodic.TabIndex = 4;
            this.pulseWidthAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.pulseWidthAnodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // amplitudeAnodic
            // 
            this.amplitudeAnodic.Location = new System.Drawing.Point(145, 18);
            this.amplitudeAnodic.Name = "amplitudeAnodic";
            this.amplitudeAnodic.Size = new System.Drawing.Size(60, 26);
            this.amplitudeAnodic.TabIndex = 3;
            this.amplitudeAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Amplitude);
            this.amplitudeAnodic.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // comboBoxStepSize
            // 
            this.comboBoxStepSize.FormattingEnabled = true;
            this.comboBoxStepSize.Location = new System.Drawing.Point(257, 38);
            this.comboBoxStepSize.Name = "comboBoxStepSize";
            this.comboBoxStepSize.Size = new System.Drawing.Size(163, 28);
            this.comboBoxStepSize.TabIndex = 34;
            this.comboBoxStepSize.TabStop = false;
            this.comboBoxStepSize.SelectedIndexChanged += new System.EventHandler(this.ComboBoxStepSize_SelectedIndexChanged);
            // 
            // buttonClearPulses
            // 
            this.buttonClearPulses.Location = new System.Drawing.Point(310, 252);
            this.buttonClearPulses.Name = "buttonClearPulses";
            this.buttonClearPulses.Size = new System.Drawing.Size(110, 42);
            this.buttonClearPulses.TabIndex = 33;
            this.buttonClearPulses.TabStop = false;
            this.buttonClearPulses.Text = "Clear Pulses";
            this.buttonClearPulses.UseVisualStyleBackColor = true;
            this.buttonClearPulses.Click += new System.EventHandler(this.ButtonClearPulses_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(175, 252);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 42);
            this.button1.TabIndex = 32;
            this.button1.TabStop = false;
            this.button1.Text = "Read Pulses";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // interPulseInterval
            // 
            this.interPulseInterval.Location = new System.Drawing.Point(376, 84);
            this.interPulseInterval.Name = "interPulseInterval";
            this.interPulseInterval.Size = new System.Drawing.Size(60, 26);
            this.interPulseInterval.TabIndex = 1;
            this.interPulseInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.interPulseInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelInterPulseInterval
            // 
            this.labelInterPulseInterval.AutoSize = true;
            this.labelInterPulseInterval.Location = new System.Drawing.Point(251, 84);
            this.labelInterPulseInterval.Name = "labelInterPulseInterval";
            this.labelInterPulseInterval.Size = new System.Drawing.Size(119, 20);
            this.labelInterPulseInterval.TabIndex = 29;
            this.labelInterPulseInterval.Text = "Inter-Pulse [ms]";
            // 
            // labelStepSizeAmplitude
            // 
            this.labelStepSizeAmplitude.AutoSize = true;
            this.labelStepSizeAmplitude.Location = new System.Drawing.Point(257, 15);
            this.labelStepSizeAmplitude.Name = "labelStepSizeAmplitude";
            this.labelStepSizeAmplitude.Size = new System.Drawing.Size(163, 20);
            this.labelStepSizeAmplitude.TabIndex = 17;
            this.labelStepSizeAmplitude.Text = "Step Size (Amplitude)";
            // 
            // checkBoxAnodicFirst
            // 
            this.checkBoxAnodicFirst.AutoSize = true;
            this.checkBoxAnodicFirst.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxAnodicFirst.Checked = true;
            this.checkBoxAnodicFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnodicFirst.Location = new System.Drawing.Point(91, 38);
            this.checkBoxAnodicFirst.Name = "checkBoxAnodicFirst";
            this.checkBoxAnodicFirst.Size = new System.Drawing.Size(119, 24);
            this.checkBoxAnodicFirst.TabIndex = 16;
            this.checkBoxAnodicFirst.TabStop = false;
            this.checkBoxAnodicFirst.Text = "Anodic First";
            this.checkBoxAnodicFirst.UseVisualStyleBackColor = true;
            this.checkBoxAnodicFirst.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // buttonAddPulses
            // 
            this.buttonAddPulses.Location = new System.Drawing.Point(40, 252);
            this.buttonAddPulses.Name = "buttonAddPulses";
            this.buttonAddPulses.Size = new System.Drawing.Size(112, 42);
            this.buttonAddPulses.TabIndex = 6;
            this.buttonAddPulses.TabStop = false;
            this.buttonAddPulses.Text = "Add Pulse(s)";
            this.buttonAddPulses.UseVisualStyleBackColor = true;
            this.buttonAddPulses.Click += new System.EventHandler(this.ButtonAddPulses_Click);
            // 
            // delay
            // 
            this.delay.Location = new System.Drawing.Point(150, 81);
            this.delay.Name = "delay";
            this.delay.Size = new System.Drawing.Size(60, 26);
            this.delay.TabIndex = 0;
            this.delay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.delay.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelNumberOfPulses
            // 
            this.labelNumberOfPulses.AutoSize = true;
            this.labelNumberOfPulses.Location = new System.Drawing.Point(235, 218);
            this.labelNumberOfPulses.Name = "labelNumberOfPulses";
            this.labelNumberOfPulses.Size = new System.Drawing.Size(134, 20);
            this.labelNumberOfPulses.TabIndex = 13;
            this.labelNumberOfPulses.Text = "Number of Pulses";
            // 
            // labelDelay
            // 
            this.labelDelay.AutoSize = true;
            this.labelDelay.Location = new System.Drawing.Point(66, 87);
            this.labelDelay.Name = "labelDelay";
            this.labelDelay.Size = new System.Drawing.Size(82, 20);
            this.labelDelay.TabIndex = 3;
            this.labelDelay.Text = "Delay [ms]";
            // 
            // numberOfStimuli
            // 
            this.numberOfStimuli.Location = new System.Drawing.Point(376, 212);
            this.numberOfStimuli.Name = "numberOfStimuli";
            this.numberOfStimuli.Size = new System.Drawing.Size(60, 26);
            this.numberOfStimuli.TabIndex = 5;
            // 
            // checkboxBiphasicSymmetrical
            // 
            this.checkboxBiphasicSymmetrical.AutoSize = true;
            this.checkboxBiphasicSymmetrical.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkboxBiphasicSymmetrical.Checked = true;
            this.checkboxBiphasicSymmetrical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxBiphasicSymmetrical.Location = new System.Drawing.Point(37, 15);
            this.checkboxBiphasicSymmetrical.Name = "checkboxBiphasicSymmetrical";
            this.checkboxBiphasicSymmetrical.Size = new System.Drawing.Size(173, 24);
            this.checkboxBiphasicSymmetrical.TabIndex = 5;
            this.checkboxBiphasicSymmetrical.TabStop = false;
            this.checkboxBiphasicSymmetrical.Text = "Biphasic Symmetric";
            this.checkboxBiphasicSymmetrical.UseVisualStyleBackColor = true;
            this.checkboxBiphasicSymmetrical.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // interStimulusInterval
            // 
            this.interStimulusInterval.Location = new System.Drawing.Point(150, 212);
            this.interStimulusInterval.Name = "interStimulusInterval";
            this.interStimulusInterval.Size = new System.Drawing.Size(60, 26);
            this.interStimulusInterval.TabIndex = 4;
            this.interStimulusInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.interStimulusInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelInterStimulusInterval
            // 
            this.labelInterStimulusInterval.AutoSize = true;
            this.labelInterStimulusInterval.Location = new System.Drawing.Point(3, 215);
            this.labelInterStimulusInterval.Name = "labelInterStimulusInterval";
            this.labelInterStimulusInterval.Size = new System.Drawing.Size(141, 20);
            this.labelInterStimulusInterval.TabIndex = 9;
            this.labelInterStimulusInterval.Text = "Inter-Stimulus [ms]";
            // 
            // tabPageChannelLayout
            // 
            this.tabPageChannelLayout.Controls.Add(this.panelChannelLayout);
            this.tabPageChannelLayout.Location = new System.Drawing.Point(4, 29);
            this.tabPageChannelLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageChannelLayout.Name = "tabPageChannelLayout";
            this.tabPageChannelLayout.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageChannelLayout.Size = new System.Drawing.Size(461, 303);
            this.tabPageChannelLayout.TabIndex = 3;
            this.tabPageChannelLayout.Text = "Channel Layout";
            this.tabPageChannelLayout.UseVisualStyleBackColor = true;
            // 
            // panelChannelLayout
            // 
            this.panelChannelLayout.Controls.Add(this.buttonCustomChannelLayout);
            this.panelChannelLayout.Controls.Add(this.buttonDefaultChannelLayout);
            this.panelChannelLayout.Controls.Add(this.textBoxChannelLayoutFilePath);
            this.panelChannelLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChannelLayout.Location = new System.Drawing.Point(4, 5);
            this.panelChannelLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelChannelLayout.Name = "panelChannelLayout";
            this.panelChannelLayout.Size = new System.Drawing.Size(453, 293);
            this.panelChannelLayout.TabIndex = 0;
            // 
            // buttonCustomChannelLayout
            // 
            this.buttonCustomChannelLayout.Location = new System.Drawing.Point(261, 131);
            this.buttonCustomChannelLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCustomChannelLayout.Name = "buttonCustomChannelLayout";
            this.buttonCustomChannelLayout.Size = new System.Drawing.Size(129, 40);
            this.buttonCustomChannelLayout.TabIndex = 2;
            this.buttonCustomChannelLayout.Text = "Custom Layout";
            this.buttonCustomChannelLayout.UseVisualStyleBackColor = true;
            this.buttonCustomChannelLayout.Click += new System.EventHandler(this.ButtonCustomChannelLayout_Click);
            // 
            // buttonDefaultChannelLayout
            // 
            this.buttonDefaultChannelLayout.Location = new System.Drawing.Point(80, 131);
            this.buttonDefaultChannelLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonDefaultChannelLayout.Name = "buttonDefaultChannelLayout";
            this.buttonDefaultChannelLayout.Size = new System.Drawing.Size(129, 40);
            this.buttonDefaultChannelLayout.TabIndex = 1;
            this.buttonDefaultChannelLayout.Text = "Default Layout";
            this.buttonDefaultChannelLayout.UseVisualStyleBackColor = true;
            this.buttonDefaultChannelLayout.Click += new System.EventHandler(this.ButtonDefaultChannelLayout_Click);
            // 
            // textBoxChannelLayoutFilePath
            // 
            this.textBoxChannelLayoutFilePath.Location = new System.Drawing.Point(24, 14);
            this.textBoxChannelLayoutFilePath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxChannelLayoutFilePath.Multiline = true;
            this.textBoxChannelLayoutFilePath.Name = "textBoxChannelLayoutFilePath";
            this.textBoxChannelLayoutFilePath.ReadOnly = true;
            this.textBoxChannelLayoutFilePath.Size = new System.Drawing.Size(406, 101);
            this.textBoxChannelLayoutFilePath.TabIndex = 0;
            // 
            // tabPageEditorDialog
            // 
            this.tabPageEditorDialog.Controls.Add(this.propertyGridStimulusSequence);
            this.tabPageEditorDialog.Location = new System.Drawing.Point(4, 29);
            this.tabPageEditorDialog.Name = "tabPageEditorDialog";
            this.tabPageEditorDialog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageEditorDialog.Size = new System.Drawing.Size(461, 303);
            this.tabPageEditorDialog.TabIndex = 2;
            this.tabPageEditorDialog.Text = "EditorDialog";
            this.tabPageEditorDialog.UseVisualStyleBackColor = true;
            // 
            // propertyGridStimulusSequence
            // 
            this.propertyGridStimulusSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridStimulusSequence.Location = new System.Drawing.Point(3, 3);
            this.propertyGridStimulusSequence.Name = "propertyGridStimulusSequence";
            this.propertyGridStimulusSequence.Size = new System.Drawing.Size(455, 297);
            this.propertyGridStimulusSequence.TabIndex = 4;
            this.propertyGridStimulusSequence.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropertyGridStimulusSequence_PropertyValueChanged);
            // 
            // tabControlVisualization
            // 
            this.tabControlVisualization.Controls.Add(this.tabPageWaveform);
            this.tabControlVisualization.Controls.Add(this.tabPageTable);
            this.tabControlVisualization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlVisualization.Location = new System.Drawing.Point(0, 0);
            this.tabControlVisualization.Name = "tabControlVisualization";
            this.tabControlVisualization.SelectedIndex = 0;
            this.tabControlVisualization.Size = new System.Drawing.Size(1330, 860);
            this.tabControlVisualization.TabIndex = 6;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.zedGraphWaveform);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 29);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWaveform.Size = new System.Drawing.Size(1322, 827);
            this.tabPageWaveform.TabIndex = 0;
            this.tabPageWaveform.Text = "Stimulus Waveform";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // tabPageTable
            // 
            this.tabPageTable.Controls.Add(this.dataGridViewStimulusTable);
            this.tabPageTable.Location = new System.Drawing.Point(4, 29);
            this.tabPageTable.Name = "tabPageTable";
            this.tabPageTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTable.Size = new System.Drawing.Size(1322, 827);
            this.tabPageTable.TabIndex = 1;
            this.tabPageTable.Text = "Table";
            this.tabPageTable.UseVisualStyleBackColor = true;
            // 
            // dataGridViewStimulusTable
            // 
            this.dataGridViewStimulusTable.AllowUserToAddRows = false;
            this.dataGridViewStimulusTable.AllowUserToDeleteRows = false;
            this.dataGridViewStimulusTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewStimulusTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewStimulusTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStimulusTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewStimulusTable.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewStimulusTable.Name = "dataGridViewStimulusTable";
            this.dataGridViewStimulusTable.RowHeadersWidth = 62;
            this.dataGridViewStimulusTable.RowTemplate.Height = 28;
            this.dataGridViewStimulusTable.Size = new System.Drawing.Size(1316, 821);
            this.dataGridViewStimulusTable.TabIndex = 0;
            this.dataGridViewStimulusTable.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewStimulusTable_CellEndEdit);
            this.dataGridViewStimulusTable.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridViewStimulusTable_DataBindingComplete);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControlVisualization);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1803, 860);
            this.splitContainer1.SplitterDistance = 1330;
            this.splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelButtons);
            this.splitContainer2.Size = new System.Drawing.Size(469, 860);
            this.splitContainer2.SplitterDistance = 795;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.zedGraphChannels);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tabControlParameters);
            this.splitContainer3.Panel2MinSize = 315;
            this.splitContainer3.Size = new System.Drawing.Size(469, 795);
            this.splitContainer3.SplitterDistance = 451;
            this.splitContainer3.SplitterWidth = 8;
            this.splitContainer3.TabIndex = 0;
            // 
            // zedGraphChannels
            // 
            this.zedGraphChannels.AutoSize = true;
            this.zedGraphChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphChannels.Location = new System.Drawing.Point(0, 0);
            this.zedGraphChannels.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphChannels.Name = "zedGraphChannels";
            this.zedGraphChannels.ScrollGrace = 0D;
            this.zedGraphChannels.ScrollMaxX = 0D;
            this.zedGraphChannels.ScrollMaxY = 0D;
            this.zedGraphChannels.ScrollMaxY2 = 0D;
            this.zedGraphChannels.ScrollMinX = 0D;
            this.zedGraphChannels.ScrollMinY = 0D;
            this.zedGraphChannels.ScrollMinY2 = 0D;
            this.zedGraphChannels.Size = new System.Drawing.Size(469, 451);
            this.zedGraphChannels.TabIndex = 3;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            this.zedGraphChannels.MouseDownEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.ZedGraphChannels_MouseDownEvent);
            this.zedGraphChannels.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.ZedGraphChannels_MouseUpEvent);
            this.zedGraphChannels.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.ZedGraphChannels_MouseMoveEvent);
            this.zedGraphChannels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ZedGraphChannels_MouseClick);
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1803, 892);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.linkLabelDocumentation);
            this.Controls.Add(this.statusStrip);
            this.MinimumSize = new System.Drawing.Size(146, 56);
            this.Name = "Rhs2116StimulusSequenceDialog";
            this.Text = "Rhs2116StimulusSequenceDialog";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.tabControlParameters.ResumeLayout(false);
            this.tabPageAddPulse.ResumeLayout(false);
            this.panelParameters.ResumeLayout(false);
            this.panelParameters.PerformLayout();
            this.groupBoxCathode.ResumeLayout(false);
            this.groupBoxCathode.PerformLayout();
            this.groupBoxAnode.ResumeLayout(false);
            this.groupBoxAnode.PerformLayout();
            this.tabPageChannelLayout.ResumeLayout(false);
            this.panelChannelLayout.ResumeLayout(false);
            this.panelChannelLayout.PerformLayout();
            this.tabPageEditorDialog.ResumeLayout(false);
            this.tabControlVisualization.ResumeLayout(false);
            this.tabPageWaveform.ResumeLayout(false);
            this.tabPageTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusIsValid;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.LinkLabel linkLabelDocumentation;
        private ZedGraph.ZedGraphControl zedGraphWaveform;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusSlotsUsed;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TabControl tabControlParameters;
        private System.Windows.Forms.TabPage tabPageAddPulse;
        private System.Windows.Forms.TabPage tabPageEditorDialog;
        private System.Windows.Forms.PropertyGrid propertyGridStimulusSequence;
        private System.Windows.Forms.Label labelAmplitudeAnodic;
        private System.Windows.Forms.Label labelDelay;
        private System.Windows.Forms.TextBox amplitudeAnodic;
        private System.Windows.Forms.TextBox delay;
        private System.Windows.Forms.CheckBox checkboxBiphasicSymmetrical;
        private System.Windows.Forms.Button buttonAddPulses;
        private System.Windows.Forms.Label labelNumberOfPulses;
        private System.Windows.Forms.TextBox numberOfStimuli;
        private System.Windows.Forms.Label labelInterStimulusInterval;
        private System.Windows.Forms.TextBox interStimulusInterval;
        private System.Windows.Forms.Label labelPulseWidthAnodic;
        private System.Windows.Forms.TextBox pulseWidthAnodic;
        private System.Windows.Forms.TabControl tabControlVisualization;
        private System.Windows.Forms.TabPage tabPageWaveform;
        private System.Windows.Forms.TabPage tabPageTable;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.DataGridView dataGridViewStimulusTable;
        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.Label labelStepSizeAmplitude;
        private System.Windows.Forms.CheckBox checkBoxAnodicFirst;
        private System.Windows.Forms.Label labelAmplitudeCathodic;
        private System.Windows.Forms.Label labelPulseWidthCathodic;
        private System.Windows.Forms.TextBox pulseWidthCathodic;
        private System.Windows.Forms.TextBox amplitudeCathodic;
        private System.Windows.Forms.TextBox interPulseInterval;
        private System.Windows.Forms.Label labelInterPulseInterval;
        private System.Windows.Forms.Button buttonClearPulses;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBoxStepSize;
        private System.Windows.Forms.GroupBox groupBoxCathode;
        private System.Windows.Forms.GroupBox groupBoxAnode;
        private System.Windows.Forms.TabPage tabPageChannelLayout;
        private System.Windows.Forms.Panel panelChannelLayout;
        private System.Windows.Forms.TextBox textBoxChannelLayoutFilePath;
        private System.Windows.Forms.Button buttonCustomChannelLayout;
        private System.Windows.Forms.Button buttonDefaultChannelLayout;
    }
}
