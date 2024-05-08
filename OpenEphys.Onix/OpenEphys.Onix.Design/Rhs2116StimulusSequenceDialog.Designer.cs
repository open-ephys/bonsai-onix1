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
            this.pulseWidthCathodicConverted = new System.Windows.Forms.TextBox();
            this.amplitudeCathodicConverted = new System.Windows.Forms.TextBox();
            this.labelAmplitudeCathodic = new System.Windows.Forms.Label();
            this.labelPulseWidthCathodicText = new System.Windows.Forms.Label();
            this.pulseWidthCathodicSamples = new System.Windows.Forms.TextBox();
            this.amplitudeCathodicSteps = new System.Windows.Forms.TextBox();
            this.groupBoxAnode = new System.Windows.Forms.GroupBox();
            this.pulseWidthAnodicConverted = new System.Windows.Forms.TextBox();
            this.amplitudeAnodicConverted = new System.Windows.Forms.TextBox();
            this.labelAnplitudeAnodicText = new System.Windows.Forms.Label();
            this.labelPulseWidthAnodicText = new System.Windows.Forms.Label();
            this.pulseWidthAnodicSamples = new System.Windows.Forms.TextBox();
            this.amplitudeAnodicSteps = new System.Windows.Forms.TextBox();
            this.comboBoxStepSize = new System.Windows.Forms.ComboBox();
            this.buttonClearPulses = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.interStimulusIntervalConverted = new System.Windows.Forms.TextBox();
            this.interPulseIntervalConverted = new System.Windows.Forms.TextBox();
            this.interPulseIntervalSamples = new System.Windows.Forms.TextBox();
            this.interPulseIntervalText = new System.Windows.Forms.Label();
            this.labelStepSize = new System.Windows.Forms.Label();
            this.checkBoxAnodicFirst = new System.Windows.Forms.CheckBox();
            this.delaySamplesConverted = new System.Windows.Forms.TextBox();
            this.buttonAddPulses = new System.Windows.Forms.Button();
            this.delaySamples = new System.Windows.Forms.TextBox();
            this.numberOfPulsesText = new System.Windows.Forms.Label();
            this.delayText = new System.Windows.Forms.Label();
            this.numberOfStimuliText = new System.Windows.Forms.TextBox();
            this.checkboxBiphasicSymmetrical = new System.Windows.Forms.CheckBox();
            this.interStimulusIntervalSamples = new System.Windows.Forms.TextBox();
            this.interStimulusIntervalText = new System.Windows.Forms.Label();
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
            this.zedGraphWaveform.Size = new System.Drawing.Size(1405, 811);
            this.zedGraphWaveform.TabIndex = 4;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // linkLabelDocumentation
            // 
            this.linkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDocumentation.AutoSize = true;
            this.linkLabelDocumentation.Location = new System.Drawing.Point(1671, 855);
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
            this.buttonCancel.Location = new System.Drawing.Point(266, 15);
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
            this.statusStrip.Location = new System.Drawing.Point(0, 850);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.statusStrip.Size = new System.Drawing.Size(1797, 32);
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
            this.panelButtons.Size = new System.Drawing.Size(374, 58);
            this.panelButtons.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Location = new System.Drawing.Point(159, 15);
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
            this.tabControlParameters.MinimumSize = new System.Drawing.Size(372, 485);
            this.tabControlParameters.Name = "tabControlParameters";
            this.tabControlParameters.SelectedIndex = 0;
            this.tabControlParameters.Size = new System.Drawing.Size(374, 485);
            this.tabControlParameters.TabIndex = 1;
            // 
            // tabPageAddPulse
            // 
            this.tabPageAddPulse.Controls.Add(this.panelParameters);
            this.tabPageAddPulse.Location = new System.Drawing.Point(4, 29);
            this.tabPageAddPulse.Name = "tabPageAddPulse";
            this.tabPageAddPulse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAddPulse.Size = new System.Drawing.Size(366, 452);
            this.tabPageAddPulse.TabIndex = 0;
            this.tabPageAddPulse.Text = "Add Pulses";
            this.tabPageAddPulse.UseVisualStyleBackColor = true;
            // 
            // panelParameters
            // 
            this.panelParameters.AutoScroll = true;
            this.panelParameters.AutoSize = true;
            this.panelParameters.Controls.Add(this.groupBoxCathode);
            this.panelParameters.Controls.Add(this.groupBoxAnode);
            this.panelParameters.Controls.Add(this.comboBoxStepSize);
            this.panelParameters.Controls.Add(this.buttonClearPulses);
            this.panelParameters.Controls.Add(this.button1);
            this.panelParameters.Controls.Add(this.interStimulusIntervalConverted);
            this.panelParameters.Controls.Add(this.interPulseIntervalConverted);
            this.panelParameters.Controls.Add(this.interPulseIntervalSamples);
            this.panelParameters.Controls.Add(this.interPulseIntervalText);
            this.panelParameters.Controls.Add(this.labelStepSize);
            this.panelParameters.Controls.Add(this.checkBoxAnodicFirst);
            this.panelParameters.Controls.Add(this.delaySamplesConverted);
            this.panelParameters.Controls.Add(this.buttonAddPulses);
            this.panelParameters.Controls.Add(this.delaySamples);
            this.panelParameters.Controls.Add(this.numberOfPulsesText);
            this.panelParameters.Controls.Add(this.delayText);
            this.panelParameters.Controls.Add(this.numberOfStimuliText);
            this.panelParameters.Controls.Add(this.checkboxBiphasicSymmetrical);
            this.panelParameters.Controls.Add(this.interStimulusIntervalSamples);
            this.panelParameters.Controls.Add(this.interStimulusIntervalText);
            this.panelParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParameters.Location = new System.Drawing.Point(3, 3);
            this.panelParameters.MinimumSize = new System.Drawing.Size(358, 437);
            this.panelParameters.Name = "panelParameters";
            this.panelParameters.Size = new System.Drawing.Size(360, 446);
            this.panelParameters.TabIndex = 0;
            // 
            // groupBoxCathode
            // 
            this.groupBoxCathode.Controls.Add(this.pulseWidthCathodicConverted);
            this.groupBoxCathode.Controls.Add(this.amplitudeCathodicConverted);
            this.groupBoxCathode.Controls.Add(this.labelAmplitudeCathodic);
            this.groupBoxCathode.Controls.Add(this.labelPulseWidthCathodicText);
            this.groupBoxCathode.Controls.Add(this.pulseWidthCathodicSamples);
            this.groupBoxCathode.Controls.Add(this.amplitudeCathodicSteps);
            this.groupBoxCathode.Location = new System.Drawing.Point(0, 229);
            this.groupBoxCathode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxCathode.Name = "groupBoxCathode";
            this.groupBoxCathode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxCathode.Size = new System.Drawing.Size(358, 86);
            this.groupBoxCathode.TabIndex = 4;
            this.groupBoxCathode.TabStop = false;
            this.groupBoxCathode.Text = "Cathode";
            this.groupBoxCathode.Visible = false;
            // 
            // pulseWidthCathodicConverted
            // 
            this.pulseWidthCathodicConverted.Enabled = false;
            this.pulseWidthCathodicConverted.Location = new System.Drawing.Point(249, 49);
            this.pulseWidthCathodicConverted.Name = "pulseWidthCathodicConverted";
            this.pulseWidthCathodicConverted.ReadOnly = true;
            this.pulseWidthCathodicConverted.Size = new System.Drawing.Size(96, 26);
            this.pulseWidthCathodicConverted.TabIndex = 27;
            this.pulseWidthCathodicConverted.TabStop = false;
            // 
            // amplitudeCathodicConverted
            // 
            this.amplitudeCathodicConverted.Enabled = false;
            this.amplitudeCathodicConverted.Location = new System.Drawing.Point(249, 18);
            this.amplitudeCathodicConverted.Name = "amplitudeCathodicConverted";
            this.amplitudeCathodicConverted.ReadOnly = true;
            this.amplitudeCathodicConverted.Size = new System.Drawing.Size(96, 26);
            this.amplitudeCathodicConverted.TabIndex = 26;
            this.amplitudeCathodicConverted.TabStop = false;
            // 
            // labelAmplitudeCathodic
            // 
            this.labelAmplitudeCathodic.AutoSize = true;
            this.labelAmplitudeCathodic.Location = new System.Drawing.Point(58, 25);
            this.labelAmplitudeCathodic.Name = "labelAmplitudeCathodic";
            this.labelAmplitudeCathodic.Size = new System.Drawing.Size(126, 20);
            this.labelAmplitudeCathodic.TabIndex = 23;
            this.labelAmplitudeCathodic.Text = "Amplitude Steps";
            // 
            // labelPulseWidthCathodicText
            // 
            this.labelPulseWidthCathodicText.AutoSize = true;
            this.labelPulseWidthCathodicText.Location = new System.Drawing.Point(22, 55);
            this.labelPulseWidthCathodicText.Name = "labelPulseWidthCathodicText";
            this.labelPulseWidthCathodicText.Size = new System.Drawing.Size(159, 20);
            this.labelPulseWidthCathodicText.TabIndex = 24;
            this.labelPulseWidthCathodicText.Text = "Pulse Width Samples";
            // 
            // pulseWidthCathodicSamples
            // 
            this.pulseWidthCathodicSamples.Location = new System.Drawing.Point(189, 49);
            this.pulseWidthCathodicSamples.Name = "pulseWidthCathodicSamples";
            this.pulseWidthCathodicSamples.Size = new System.Drawing.Size(43, 26);
            this.pulseWidthCathodicSamples.TabIndex = 5;
            this.pulseWidthCathodicSamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.pulseWidthCathodicSamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // amplitudeCathodicSteps
            // 
            this.amplitudeCathodicSteps.Location = new System.Drawing.Point(189, 18);
            this.amplitudeCathodicSteps.Name = "amplitudeCathodicSteps";
            this.amplitudeCathodicSteps.Size = new System.Drawing.Size(43, 26);
            this.amplitudeCathodicSteps.TabIndex = 4;
            this.amplitudeCathodicSteps.TextChanged += new System.EventHandler(this.Amplitude_TextChanged);
            this.amplitudeCathodicSteps.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // groupBoxAnode
            // 
            this.groupBoxAnode.Controls.Add(this.pulseWidthAnodicConverted);
            this.groupBoxAnode.Controls.Add(this.amplitudeAnodicConverted);
            this.groupBoxAnode.Controls.Add(this.labelAnplitudeAnodicText);
            this.groupBoxAnode.Controls.Add(this.labelPulseWidthAnodicText);
            this.groupBoxAnode.Controls.Add(this.pulseWidthAnodicSamples);
            this.groupBoxAnode.Controls.Add(this.amplitudeAnodicSteps);
            this.groupBoxAnode.Location = new System.Drawing.Point(0, 108);
            this.groupBoxAnode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxAnode.Name = "groupBoxAnode";
            this.groupBoxAnode.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxAnode.Size = new System.Drawing.Size(358, 86);
            this.groupBoxAnode.TabIndex = 1;
            this.groupBoxAnode.TabStop = false;
            this.groupBoxAnode.Text = "Anode";
            // 
            // pulseWidthAnodicConverted
            // 
            this.pulseWidthAnodicConverted.Enabled = false;
            this.pulseWidthAnodicConverted.Location = new System.Drawing.Point(249, 49);
            this.pulseWidthAnodicConverted.Name = "pulseWidthAnodicConverted";
            this.pulseWidthAnodicConverted.ReadOnly = true;
            this.pulseWidthAnodicConverted.Size = new System.Drawing.Size(96, 26);
            this.pulseWidthAnodicConverted.TabIndex = 20;
            this.pulseWidthAnodicConverted.TabStop = false;
            // 
            // amplitudeAnodicConverted
            // 
            this.amplitudeAnodicConverted.Enabled = false;
            this.amplitudeAnodicConverted.Location = new System.Drawing.Point(249, 18);
            this.amplitudeAnodicConverted.Name = "amplitudeAnodicConverted";
            this.amplitudeAnodicConverted.ReadOnly = true;
            this.amplitudeAnodicConverted.Size = new System.Drawing.Size(96, 26);
            this.amplitudeAnodicConverted.TabIndex = 19;
            this.amplitudeAnodicConverted.TabStop = false;
            // 
            // labelAnplitudeAnodicText
            // 
            this.labelAnplitudeAnodicText.AutoSize = true;
            this.labelAnplitudeAnodicText.Location = new System.Drawing.Point(58, 25);
            this.labelAnplitudeAnodicText.Name = "labelAnplitudeAnodicText";
            this.labelAnplitudeAnodicText.Size = new System.Drawing.Size(126, 20);
            this.labelAnplitudeAnodicText.TabIndex = 4;
            this.labelAnplitudeAnodicText.Text = "Amplitude Steps";
            // 
            // labelPulseWidthAnodicText
            // 
            this.labelPulseWidthAnodicText.AutoSize = true;
            this.labelPulseWidthAnodicText.Location = new System.Drawing.Point(22, 55);
            this.labelPulseWidthAnodicText.Name = "labelPulseWidthAnodicText";
            this.labelPulseWidthAnodicText.Size = new System.Drawing.Size(159, 20);
            this.labelPulseWidthAnodicText.TabIndex = 7;
            this.labelPulseWidthAnodicText.Text = "Pulse Width Samples";
            // 
            // pulseWidthAnodicSamples
            // 
            this.pulseWidthAnodicSamples.Location = new System.Drawing.Point(189, 49);
            this.pulseWidthAnodicSamples.Name = "pulseWidthAnodicSamples";
            this.pulseWidthAnodicSamples.Size = new System.Drawing.Size(43, 26);
            this.pulseWidthAnodicSamples.TabIndex = 2;
            this.pulseWidthAnodicSamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.pulseWidthAnodicSamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // amplitudeAnodicSteps
            // 
            this.amplitudeAnodicSteps.Location = new System.Drawing.Point(189, 18);
            this.amplitudeAnodicSteps.Name = "amplitudeAnodicSteps";
            this.amplitudeAnodicSteps.Size = new System.Drawing.Size(43, 26);
            this.amplitudeAnodicSteps.TabIndex = 1;
            this.amplitudeAnodicSteps.TextChanged += new System.EventHandler(this.Amplitude_TextChanged);
            this.amplitudeAnodicSteps.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // comboBoxStepSize
            // 
            this.comboBoxStepSize.FormattingEnabled = true;
            this.comboBoxStepSize.Location = new System.Drawing.Point(198, 71);
            this.comboBoxStepSize.Name = "comboBoxStepSize";
            this.comboBoxStepSize.Size = new System.Drawing.Size(133, 28);
            this.comboBoxStepSize.TabIndex = 34;
            this.comboBoxStepSize.TabStop = false;
            this.comboBoxStepSize.SelectedIndexChanged += new System.EventHandler(this.ComboBoxStepSize_SelectedIndexChanged);
            // 
            // buttonClearPulses
            // 
            this.buttonClearPulses.Location = new System.Drawing.Point(242, 392);
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
            this.button1.Location = new System.Drawing.Point(124, 392);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 42);
            this.button1.TabIndex = 32;
            this.button1.TabStop = false;
            this.button1.Text = "Read Pulses";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // interStimulusIntervalConverted
            // 
            this.interStimulusIntervalConverted.Enabled = false;
            this.interStimulusIntervalConverted.Location = new System.Drawing.Point(249, 318);
            this.interStimulusIntervalConverted.Name = "interStimulusIntervalConverted";
            this.interStimulusIntervalConverted.ReadOnly = true;
            this.interStimulusIntervalConverted.Size = new System.Drawing.Size(96, 26);
            this.interStimulusIntervalConverted.TabIndex = 31;
            this.interStimulusIntervalConverted.TabStop = false;
            // 
            // interPulseIntervalConverted
            // 
            this.interPulseIntervalConverted.Enabled = false;
            this.interPulseIntervalConverted.Location = new System.Drawing.Point(249, 198);
            this.interPulseIntervalConverted.Name = "interPulseIntervalConverted";
            this.interPulseIntervalConverted.ReadOnly = true;
            this.interPulseIntervalConverted.Size = new System.Drawing.Size(96, 26);
            this.interPulseIntervalConverted.TabIndex = 30;
            this.interPulseIntervalConverted.TabStop = false;
            // 
            // interPulseIntervalSamples
            // 
            this.interPulseIntervalSamples.Location = new System.Drawing.Point(189, 198);
            this.interPulseIntervalSamples.Name = "interPulseIntervalSamples";
            this.interPulseIntervalSamples.Size = new System.Drawing.Size(43, 26);
            this.interPulseIntervalSamples.TabIndex = 3;
            this.interPulseIntervalSamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.interPulseIntervalSamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // interPulseIntervalText
            // 
            this.interPulseIntervalText.AutoSize = true;
            this.interPulseIntervalText.Location = new System.Drawing.Point(33, 205);
            this.interPulseIntervalText.Name = "interPulseIntervalText";
            this.interPulseIntervalText.Size = new System.Drawing.Size(152, 20);
            this.interPulseIntervalText.TabIndex = 29;
            this.interPulseIntervalText.Text = "Inter-Pulse Samples";
            // 
            // labelStepSize
            // 
            this.labelStepSize.AutoSize = true;
            this.labelStepSize.Location = new System.Drawing.Point(198, 48);
            this.labelStepSize.Name = "labelStepSize";
            this.labelStepSize.Size = new System.Drawing.Size(78, 20);
            this.labelStepSize.TabIndex = 17;
            this.labelStepSize.Text = "Step Size";
            // 
            // checkBoxAnodicFirst
            // 
            this.checkBoxAnodicFirst.AutoSize = true;
            this.checkBoxAnodicFirst.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxAnodicFirst.Checked = true;
            this.checkBoxAnodicFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnodicFirst.Location = new System.Drawing.Point(63, 75);
            this.checkBoxAnodicFirst.Name = "checkBoxAnodicFirst";
            this.checkBoxAnodicFirst.Size = new System.Drawing.Size(119, 24);
            this.checkBoxAnodicFirst.TabIndex = 16;
            this.checkBoxAnodicFirst.TabStop = false;
            this.checkBoxAnodicFirst.Text = "Anodic First";
            this.checkBoxAnodicFirst.UseVisualStyleBackColor = true;
            this.checkBoxAnodicFirst.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // delaySamplesConverted
            // 
            this.delaySamplesConverted.Enabled = false;
            this.delaySamplesConverted.Location = new System.Drawing.Point(249, 11);
            this.delaySamplesConverted.Name = "delaySamplesConverted";
            this.delaySamplesConverted.ReadOnly = true;
            this.delaySamplesConverted.Size = new System.Drawing.Size(96, 26);
            this.delaySamplesConverted.TabIndex = 15;
            this.delaySamplesConverted.TabStop = false;
            // 
            // buttonAddPulses
            // 
            this.buttonAddPulses.Location = new System.Drawing.Point(8, 392);
            this.buttonAddPulses.Name = "buttonAddPulses";
            this.buttonAddPulses.Size = new System.Drawing.Size(112, 42);
            this.buttonAddPulses.TabIndex = 14;
            this.buttonAddPulses.TabStop = false;
            this.buttonAddPulses.Text = "Add Pulse(s)";
            this.buttonAddPulses.UseVisualStyleBackColor = true;
            this.buttonAddPulses.Click += new System.EventHandler(this.ButtonAddPulses_Click);
            // 
            // delaySamples
            // 
            this.delaySamples.Location = new System.Drawing.Point(189, 11);
            this.delaySamples.Name = "delaySamples";
            this.delaySamples.Size = new System.Drawing.Size(43, 26);
            this.delaySamples.TabIndex = 0;
            this.delaySamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.delaySamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // numberOfPulsesText
            // 
            this.numberOfPulsesText.AutoSize = true;
            this.numberOfPulsesText.Location = new System.Drawing.Point(48, 357);
            this.numberOfPulsesText.Name = "numberOfPulsesText";
            this.numberOfPulsesText.Size = new System.Drawing.Size(134, 20);
            this.numberOfPulsesText.TabIndex = 13;
            this.numberOfPulsesText.Text = "Number of Pulses";
            // 
            // delayText
            // 
            this.delayText.AutoSize = true;
            this.delayText.Location = new System.Drawing.Point(68, 17);
            this.delayText.Name = "delayText";
            this.delayText.Size = new System.Drawing.Size(115, 20);
            this.delayText.TabIndex = 3;
            this.delayText.Text = "Delay Samples";
            // 
            // numberOfStimuliText
            // 
            this.numberOfStimuliText.Location = new System.Drawing.Point(189, 351);
            this.numberOfStimuliText.Name = "numberOfStimuliText";
            this.numberOfStimuliText.Size = new System.Drawing.Size(43, 26);
            this.numberOfStimuliText.TabIndex = 7;
            this.numberOfStimuliText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // checkboxBiphasicSymmetrical
            // 
            this.checkboxBiphasicSymmetrical.AutoSize = true;
            this.checkboxBiphasicSymmetrical.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkboxBiphasicSymmetrical.Checked = true;
            this.checkboxBiphasicSymmetrical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxBiphasicSymmetrical.Location = new System.Drawing.Point(9, 52);
            this.checkboxBiphasicSymmetrical.Name = "checkboxBiphasicSymmetrical";
            this.checkboxBiphasicSymmetrical.Size = new System.Drawing.Size(173, 24);
            this.checkboxBiphasicSymmetrical.TabIndex = 5;
            this.checkboxBiphasicSymmetrical.TabStop = false;
            this.checkboxBiphasicSymmetrical.Text = "Biphasic Symmetric";
            this.checkboxBiphasicSymmetrical.UseVisualStyleBackColor = true;
            this.checkboxBiphasicSymmetrical.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // interStimulusIntervalSamples
            // 
            this.interStimulusIntervalSamples.Location = new System.Drawing.Point(189, 318);
            this.interStimulusIntervalSamples.Name = "interStimulusIntervalSamples";
            this.interStimulusIntervalSamples.Size = new System.Drawing.Size(43, 26);
            this.interStimulusIntervalSamples.TabIndex = 6;
            this.interStimulusIntervalSamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.interStimulusIntervalSamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // interStimulusIntervalText
            // 
            this.interStimulusIntervalText.AutoSize = true;
            this.interStimulusIntervalText.Location = new System.Drawing.Point(14, 325);
            this.interStimulusIntervalText.Name = "interStimulusIntervalText";
            this.interStimulusIntervalText.Size = new System.Drawing.Size(174, 20);
            this.interStimulusIntervalText.TabIndex = 9;
            this.interStimulusIntervalText.Text = "Inter-Stimulus Samples";
            // 
            // tabPageChannelLayout
            // 
            this.tabPageChannelLayout.Controls.Add(this.panelChannelLayout);
            this.tabPageChannelLayout.Location = new System.Drawing.Point(4, 29);
            this.tabPageChannelLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageChannelLayout.Name = "tabPageChannelLayout";
            this.tabPageChannelLayout.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageChannelLayout.Size = new System.Drawing.Size(366, 452);
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
            this.panelChannelLayout.Size = new System.Drawing.Size(358, 442);
            this.panelChannelLayout.TabIndex = 0;
            // 
            // buttonCustomChannelLayout
            // 
            this.buttonCustomChannelLayout.Location = new System.Drawing.Point(183, 131);
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
            this.buttonDefaultChannelLayout.Location = new System.Drawing.Point(28, 131);
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
            this.textBoxChannelLayoutFilePath.Size = new System.Drawing.Size(302, 101);
            this.textBoxChannelLayoutFilePath.TabIndex = 0;
            // 
            // tabPageEditorDialog
            // 
            this.tabPageEditorDialog.Controls.Add(this.propertyGridStimulusSequence);
            this.tabPageEditorDialog.Location = new System.Drawing.Point(4, 29);
            this.tabPageEditorDialog.Name = "tabPageEditorDialog";
            this.tabPageEditorDialog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageEditorDialog.Size = new System.Drawing.Size(366, 452);
            this.tabPageEditorDialog.TabIndex = 2;
            this.tabPageEditorDialog.Text = "EditorDialog";
            this.tabPageEditorDialog.UseVisualStyleBackColor = true;
            // 
            // propertyGridStimulusSequence
            // 
            this.propertyGridStimulusSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridStimulusSequence.Location = new System.Drawing.Point(3, 3);
            this.propertyGridStimulusSequence.Name = "propertyGridStimulusSequence";
            this.propertyGridStimulusSequence.Size = new System.Drawing.Size(360, 446);
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
            this.tabControlVisualization.Size = new System.Drawing.Size(1419, 850);
            this.tabControlVisualization.TabIndex = 6;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.zedGraphWaveform);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 29);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWaveform.Size = new System.Drawing.Size(1411, 817);
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
            this.tabPageTable.Size = new System.Drawing.Size(1411, 817);
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
            this.dataGridViewStimulusTable.Size = new System.Drawing.Size(1405, 811);
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
            this.splitContainer1.Size = new System.Drawing.Size(1797, 850);
            this.splitContainer1.SplitterDistance = 1419;
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
            this.splitContainer2.Size = new System.Drawing.Size(374, 850);
            this.splitContainer2.SplitterDistance = 787;
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
            this.splitContainer3.Size = new System.Drawing.Size(374, 787);
            this.splitContainer3.SplitterDistance = 299;
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
            this.zedGraphChannels.Size = new System.Drawing.Size(374, 299);
            this.zedGraphChannels.TabIndex = 3;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            this.zedGraphChannels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ZedGraphChannels_MouseClick);
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1797, 882);
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
            this.tabPageAddPulse.PerformLayout();
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
        private System.Windows.Forms.Label labelAnplitudeAnodicText;
        private System.Windows.Forms.Label delayText;
        private System.Windows.Forms.TextBox amplitudeAnodicSteps;
        private System.Windows.Forms.TextBox delaySamples;
        private System.Windows.Forms.CheckBox checkboxBiphasicSymmetrical;
        private System.Windows.Forms.Button buttonAddPulses;
        private System.Windows.Forms.Label numberOfPulsesText;
        private System.Windows.Forms.TextBox numberOfStimuliText;
        private System.Windows.Forms.Label interStimulusIntervalText;
        private System.Windows.Forms.TextBox interStimulusIntervalSamples;
        private System.Windows.Forms.Label labelPulseWidthAnodicText;
        private System.Windows.Forms.TextBox pulseWidthAnodicSamples;
        private System.Windows.Forms.TabControl tabControlVisualization;
        private System.Windows.Forms.TabPage tabPageWaveform;
        private System.Windows.Forms.TabPage tabPageTable;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.DataGridView dataGridViewStimulusTable;
        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.TextBox delaySamplesConverted;
        private System.Windows.Forms.Label labelStepSize;
        private System.Windows.Forms.CheckBox checkBoxAnodicFirst;
        private System.Windows.Forms.TextBox amplitudeAnodicConverted;
        private System.Windows.Forms.TextBox pulseWidthAnodicConverted;
        private System.Windows.Forms.TextBox pulseWidthCathodicConverted;
        private System.Windows.Forms.TextBox amplitudeCathodicConverted;
        private System.Windows.Forms.Label labelAmplitudeCathodic;
        private System.Windows.Forms.Label labelPulseWidthCathodicText;
        private System.Windows.Forms.TextBox pulseWidthCathodicSamples;
        private System.Windows.Forms.TextBox amplitudeCathodicSteps;
        private System.Windows.Forms.TextBox interPulseIntervalConverted;
        private System.Windows.Forms.TextBox interPulseIntervalSamples;
        private System.Windows.Forms.Label interPulseIntervalText;
        private System.Windows.Forms.TextBox interStimulusIntervalConverted;
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
