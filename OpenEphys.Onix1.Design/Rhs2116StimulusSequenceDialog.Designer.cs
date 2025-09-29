namespace OpenEphys.Onix1.Design
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
            System.Windows.Forms.Label labelAmplitudeCathodic;
            System.Windows.Forms.Label labelPulseWidthCathodic;
            System.Windows.Forms.Label labelAmplitudeAnodic;
            System.Windows.Forms.Label labelPulseWidthAnodic;
            System.Windows.Forms.Label labelInterPulseInterval;
            System.Windows.Forms.Label labelStepSizeAmplitude;
            System.Windows.Forms.Label labelNumberOfPulses;
            System.Windows.Forms.Label labelInterStimulusInterval;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rhs2116StimulusSequenceDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusSlotsUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.panelParameters = new System.Windows.Forms.Panel();
            this.textBoxStepSize = new System.Windows.Forms.TextBox();
            this.groupBoxCathode = new System.Windows.Forms.GroupBox();
            this.textboxAmplitudeCathodic = new System.Windows.Forms.TextBox();
            this.textboxPulseWidthCathodic = new System.Windows.Forms.TextBox();
            this.textboxAmplitudeCathodicRequested = new System.Windows.Forms.TextBox();
            this.groupBoxAnode = new System.Windows.Forms.GroupBox();
            this.textboxAmplitudeAnodic = new System.Windows.Forms.TextBox();
            this.textboxPulseWidthAnodic = new System.Windows.Forms.TextBox();
            this.textboxAmplitudeAnodicRequested = new System.Windows.Forms.TextBox();
            this.buttonClearPulses = new System.Windows.Forms.Button();
            this.buttonReadPulses = new System.Windows.Forms.Button();
            this.textboxInterPulseInterval = new System.Windows.Forms.TextBox();
            this.checkBoxAnodicFirst = new System.Windows.Forms.CheckBox();
            this.buttonAddPulses = new System.Windows.Forms.Button();
            this.textboxDelay = new System.Windows.Forms.TextBox();
            this.labelDelay = new System.Windows.Forms.Label();
            this.checkboxBiphasicSymmetrical = new System.Windows.Forms.CheckBox();
            this.textboxInterStimulusInterval = new System.Windows.Forms.TextBox();
            this.tabControlVisualization = new System.Windows.Forms.TabControl();
            this.tabPageWaveform = new System.Windows.Forms.TabPage();
            this.zedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.tabPageTable = new System.Windows.Forms.TabPage();
            this.dataGridViewStimulusTable = new System.Windows.Forms.DataGridView();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stimulusWaveformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.numericUpDownNumberOfPulses = new System.Windows.Forms.NumericUpDown();
            labelAmplitudeCathodic = new System.Windows.Forms.Label();
            labelPulseWidthCathodic = new System.Windows.Forms.Label();
            labelAmplitudeAnodic = new System.Windows.Forms.Label();
            labelPulseWidthAnodic = new System.Windows.Forms.Label();
            labelInterPulseInterval = new System.Windows.Forms.Label();
            labelStepSizeAmplitude = new System.Windows.Forms.Label();
            labelNumberOfPulses = new System.Windows.Forms.Label();
            labelInterStimulusInterval = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            this.panelParameters.SuspendLayout();
            this.groupBoxCathode.SuspendLayout();
            this.groupBoxAnode.SuspendLayout();
            this.tabControlVisualization.SuspendLayout();
            this.tabPageWaveform.SuspendLayout();
            this.tabPageTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).BeginInit();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfPulses)).BeginInit();
            this.SuspendLayout();
            // 
            // labelAmplitudeCathodic
            // 
            labelAmplitudeCathodic.AutoSize = true;
            labelAmplitudeCathodic.Location = new System.Drawing.Point(8, 15);
            labelAmplitudeCathodic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelAmplitudeCathodic.Name = "labelAmplitudeCathodic";
            labelAmplitudeCathodic.Size = new System.Drawing.Size(75, 13);
            labelAmplitudeCathodic.TabIndex = 0;
            labelAmplitudeCathodic.Text = "Amplitude [µA]";
            // 
            // labelPulseWidthCathodic
            // 
            labelPulseWidthCathodic.AutoSize = true;
            labelPulseWidthCathodic.Location = new System.Drawing.Point(8, 59);
            labelPulseWidthCathodic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPulseWidthCathodic.Name = "labelPulseWidthCathodic";
            labelPulseWidthCathodic.Size = new System.Drawing.Size(86, 13);
            labelPulseWidthCathodic.TabIndex = 3;
            labelPulseWidthCathodic.Text = "Pulse Width [ms]";
            // 
            // labelAmplitudeAnodic
            // 
            labelAmplitudeAnodic.AutoSize = true;
            labelAmplitudeAnodic.Location = new System.Drawing.Point(6, 15);
            labelAmplitudeAnodic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelAmplitudeAnodic.Name = "labelAmplitudeAnodic";
            labelAmplitudeAnodic.Size = new System.Drawing.Size(75, 13);
            labelAmplitudeAnodic.TabIndex = 0;
            labelAmplitudeAnodic.Text = "Amplitude [µA]";
            // 
            // labelPulseWidthAnodic
            // 
            labelPulseWidthAnodic.AutoSize = true;
            labelPulseWidthAnodic.Location = new System.Drawing.Point(6, 59);
            labelPulseWidthAnodic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelPulseWidthAnodic.Name = "labelPulseWidthAnodic";
            labelPulseWidthAnodic.Size = new System.Drawing.Size(86, 13);
            labelPulseWidthAnodic.TabIndex = 3;
            labelPulseWidthAnodic.Text = "Pulse Width [ms]";
            // 
            // labelInterPulseInterval
            // 
            labelInterPulseInterval.AutoSize = true;
            labelInterPulseInterval.Location = new System.Drawing.Point(188, 57);
            labelInterPulseInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelInterPulseInterval.Name = "labelInterPulseInterval";
            labelInterPulseInterval.Size = new System.Drawing.Size(79, 13);
            labelInterPulseInterval.TabIndex = 6;
            labelInterPulseInterval.Text = "Inter-Pulse [ms]";
            // 
            // labelStepSizeAmplitude
            // 
            labelStepSizeAmplitude.AutoSize = true;
            labelStepSizeAmplitude.Location = new System.Drawing.Point(188, 11);
            labelStepSizeAmplitude.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelStepSizeAmplitude.Name = "labelStepSizeAmplitude";
            labelStepSizeAmplitude.Size = new System.Drawing.Size(107, 13);
            labelStepSizeAmplitude.TabIndex = 2;
            labelStepSizeAmplitude.Text = "Step Size (Amplitude)";
            // 
            // labelNumberOfPulses
            // 
            labelNumberOfPulses.AutoSize = true;
            labelNumberOfPulses.Location = new System.Drawing.Point(178, 167);
            labelNumberOfPulses.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelNumberOfPulses.Name = "labelNumberOfPulses";
            labelNumberOfPulses.Size = new System.Drawing.Size(90, 13);
            labelNumberOfPulses.TabIndex = 12;
            labelNumberOfPulses.Text = "Number of Pulses";
            // 
            // labelInterStimulusInterval
            // 
            labelInterStimulusInterval.AutoSize = true;
            labelInterStimulusInterval.Location = new System.Drawing.Point(11, 166);
            labelInterStimulusInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            labelInterStimulusInterval.Name = "labelInterStimulusInterval";
            labelInterStimulusInterval.Size = new System.Drawing.Size(92, 13);
            labelInterStimulusInterval.TabIndex = 10;
            labelInterStimulusInterval.Text = "Inter-Stimulus [ms]";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(1046, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(108, 26);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusIsValid,
            this.toolStripStatusSlotsUsed});
            this.statusStrip.Location = new System.Drawing.Point(0, 609);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 9, 0);
            this.statusStrip.Size = new System.Drawing.Size(1160, 21);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusIsValid
            // 
            this.toolStripStatusIsValid.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusIsValid.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripStatusIsValid.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusReadyImage;
            this.toolStripStatusIsValid.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripStatusIsValid.Name = "toolStripStatusIsValid";
            this.toolStripStatusIsValid.Size = new System.Drawing.Size(153, 16);
            this.toolStripStatusIsValid.Text = "Valid stimulus sequence";
            this.toolStripStatusIsValid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusSlotsUsed
            // 
            this.toolStripStatusSlotsUsed.Name = "toolStripStatusSlotsUsed";
            this.toolStripStatusSlotsUsed.Size = new System.Drawing.Size(104, 16);
            this.toolStripStatusSlotsUsed.Text = "100% of slots used";
            this.toolStripStatusSlotsUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Location = new System.Drawing.Point(934, 2);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(108, 26);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // panelParameters
            // 
            this.panelParameters.AutoScroll = true;
            this.panelParameters.Controls.Add(this.numericUpDownNumberOfPulses);
            this.panelParameters.Controls.Add(this.textBoxStepSize);
            this.panelParameters.Controls.Add(this.groupBoxCathode);
            this.panelParameters.Controls.Add(this.groupBoxAnode);
            this.panelParameters.Controls.Add(this.buttonClearPulses);
            this.panelParameters.Controls.Add(this.buttonReadPulses);
            this.panelParameters.Controls.Add(this.textboxInterPulseInterval);
            this.panelParameters.Controls.Add(labelInterPulseInterval);
            this.panelParameters.Controls.Add(labelStepSizeAmplitude);
            this.panelParameters.Controls.Add(this.checkBoxAnodicFirst);
            this.panelParameters.Controls.Add(this.buttonAddPulses);
            this.panelParameters.Controls.Add(this.textboxDelay);
            this.panelParameters.Controls.Add(labelNumberOfPulses);
            this.panelParameters.Controls.Add(this.labelDelay);
            this.panelParameters.Controls.Add(this.checkboxBiphasicSymmetrical);
            this.panelParameters.Controls.Add(this.textboxInterStimulusInterval);
            this.panelParameters.Controls.Add(labelInterStimulusInterval);
            this.panelParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParameters.Location = new System.Drawing.Point(2, 15);
            this.panelParameters.Margin = new System.Windows.Forms.Padding(2);
            this.panelParameters.Name = "panelParameters";
            this.panelParameters.Size = new System.Drawing.Size(330, 227);
            this.panelParameters.TabIndex = 0;
            this.panelParameters.TabStop = true;
            // 
            // textBoxStepSize
            // 
            this.textBoxStepSize.Enabled = false;
            this.textBoxStepSize.Location = new System.Drawing.Point(192, 26);
            this.textBoxStepSize.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxStepSize.Name = "textBoxStepSize";
            this.textBoxStepSize.ReadOnly = true;
            this.textBoxStepSize.Size = new System.Drawing.Size(122, 20);
            this.textBoxStepSize.TabIndex = 3;
            this.textBoxStepSize.TabStop = false;
            // 
            // groupBoxCathode
            // 
            this.groupBoxCathode.Controls.Add(this.textboxAmplitudeCathodic);
            this.groupBoxCathode.Controls.Add(labelAmplitudeCathodic);
            this.groupBoxCathode.Controls.Add(labelPulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.textboxPulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.textboxAmplitudeCathodicRequested);
            this.groupBoxCathode.Location = new System.Drawing.Point(173, 76);
            this.groupBoxCathode.Name = "groupBoxCathode";
            this.groupBoxCathode.Size = new System.Drawing.Size(146, 82);
            this.groupBoxCathode.TabIndex = 9;
            this.groupBoxCathode.TabStop = false;
            this.groupBoxCathode.Text = "Cathode";
            this.groupBoxCathode.Visible = false;
            // 
            // textboxAmplitudeCathodic
            // 
            this.textboxAmplitudeCathodic.Location = new System.Drawing.Point(99, 32);
            this.textboxAmplitudeCathodic.Margin = new System.Windows.Forms.Padding(2);
            this.textboxAmplitudeCathodic.Name = "textboxAmplitudeCathodic";
            this.textboxAmplitudeCathodic.ReadOnly = true;
            this.textboxAmplitudeCathodic.Size = new System.Drawing.Size(42, 20);
            this.textboxAmplitudeCathodic.TabIndex = 2;
            this.textboxAmplitudeCathodic.TabStop = false;
            // 
            // textboxPulseWidthCathodic
            // 
            this.textboxPulseWidthCathodic.Location = new System.Drawing.Point(99, 55);
            this.textboxPulseWidthCathodic.Margin = new System.Windows.Forms.Padding(2);
            this.textboxPulseWidthCathodic.Name = "textboxPulseWidthCathodic";
            this.textboxPulseWidthCathodic.Size = new System.Drawing.Size(42, 20);
            this.textboxPulseWidthCathodic.TabIndex = 4;
            this.textboxPulseWidthCathodic.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Time);
            this.textboxPulseWidthCathodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // textboxAmplitudeCathodicRequested
            // 
            this.textboxAmplitudeCathodicRequested.Location = new System.Drawing.Point(99, 11);
            this.textboxAmplitudeCathodicRequested.Margin = new System.Windows.Forms.Padding(2);
            this.textboxAmplitudeCathodicRequested.Name = "textboxAmplitudeCathodicRequested";
            this.textboxAmplitudeCathodicRequested.Size = new System.Drawing.Size(42, 20);
            this.textboxAmplitudeCathodicRequested.TabIndex = 1;
            this.textboxAmplitudeCathodicRequested.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Amplitude);
            this.textboxAmplitudeCathodicRequested.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // groupBoxAnode
            // 
            this.groupBoxAnode.Controls.Add(this.textboxAmplitudeAnodic);
            this.groupBoxAnode.Controls.Add(labelAmplitudeAnodic);
            this.groupBoxAnode.Controls.Add(labelPulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.textboxPulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.textboxAmplitudeAnodicRequested);
            this.groupBoxAnode.Location = new System.Drawing.Point(10, 76);
            this.groupBoxAnode.Name = "groupBoxAnode";
            this.groupBoxAnode.Size = new System.Drawing.Size(146, 82);
            this.groupBoxAnode.TabIndex = 8;
            this.groupBoxAnode.TabStop = false;
            this.groupBoxAnode.Text = "Anode";
            // 
            // textboxAmplitudeAnodic
            // 
            this.textboxAmplitudeAnodic.Location = new System.Drawing.Point(97, 32);
            this.textboxAmplitudeAnodic.Margin = new System.Windows.Forms.Padding(2);
            this.textboxAmplitudeAnodic.Name = "textboxAmplitudeAnodic";
            this.textboxAmplitudeAnodic.ReadOnly = true;
            this.textboxAmplitudeAnodic.Size = new System.Drawing.Size(42, 20);
            this.textboxAmplitudeAnodic.TabIndex = 2;
            this.textboxAmplitudeAnodic.TabStop = false;
            // 
            // textboxPulseWidthAnodic
            // 
            this.textboxPulseWidthAnodic.Location = new System.Drawing.Point(97, 55);
            this.textboxPulseWidthAnodic.Margin = new System.Windows.Forms.Padding(2);
            this.textboxPulseWidthAnodic.Name = "textboxPulseWidthAnodic";
            this.textboxPulseWidthAnodic.Size = new System.Drawing.Size(42, 20);
            this.textboxPulseWidthAnodic.TabIndex = 4;
            this.textboxPulseWidthAnodic.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Time);
            this.textboxPulseWidthAnodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // textboxAmplitudeAnodicRequested
            // 
            this.textboxAmplitudeAnodicRequested.Location = new System.Drawing.Point(97, 11);
            this.textboxAmplitudeAnodicRequested.Margin = new System.Windows.Forms.Padding(2);
            this.textboxAmplitudeAnodicRequested.Name = "textboxAmplitudeAnodicRequested";
            this.textboxAmplitudeAnodicRequested.Size = new System.Drawing.Size(42, 20);
            this.textboxAmplitudeAnodicRequested.TabIndex = 1;
            this.textboxAmplitudeAnodicRequested.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Amplitude);
            this.textboxAmplitudeAnodicRequested.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // buttonClearPulses
            // 
            this.buttonClearPulses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.buttonClearPulses.Location = new System.Drawing.Point(14, 193);
            this.buttonClearPulses.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClearPulses.Name = "buttonClearPulses";
            this.buttonClearPulses.Size = new System.Drawing.Size(74, 28);
            this.buttonClearPulses.TabIndex = 14;
            this.buttonClearPulses.Text = "Clear";
            this.toolTip1.SetToolTip(this.buttonClearPulses, "Removes the settings for all selected contacts. If no contacts are selected, this" +
        " will clear the parameters for all contacts.");
            this.buttonClearPulses.UseVisualStyleBackColor = true;
            this.buttonClearPulses.Click += new System.EventHandler(this.ButtonClearPulses_Click);
            // 
            // buttonReadPulses
            // 
            this.buttonReadPulses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.buttonReadPulses.Location = new System.Drawing.Point(92, 193);
            this.buttonReadPulses.Margin = new System.Windows.Forms.Padding(2);
            this.buttonReadPulses.Name = "buttonReadPulses";
            this.buttonReadPulses.Size = new System.Drawing.Size(75, 28);
            this.buttonReadPulses.TabIndex = 15;
            this.buttonReadPulses.Text = "Read";
            this.toolTip1.SetToolTip(this.buttonReadPulses, "If a single contact is selected, this will read the current settings for that con" +
        "tact and display in the parameters. Useful for copying settings from one channel" +
        " to another.");
            this.buttonReadPulses.UseVisualStyleBackColor = true;
            this.buttonReadPulses.Click += new System.EventHandler(this.ButtonReadPulses_Click);
            // 
            // textboxInterPulseInterval
            // 
            this.textboxInterPulseInterval.Location = new System.Drawing.Point(272, 54);
            this.textboxInterPulseInterval.Margin = new System.Windows.Forms.Padding(2);
            this.textboxInterPulseInterval.Name = "textboxInterPulseInterval";
            this.textboxInterPulseInterval.Size = new System.Drawing.Size(42, 20);
            this.textboxInterPulseInterval.TabIndex = 7;
            this.textboxInterPulseInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Time);
            this.textboxInterPulseInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // checkBoxAnodicFirst
            // 
            this.checkBoxAnodicFirst.AutoSize = true;
            this.checkBoxAnodicFirst.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxAnodicFirst.Checked = true;
            this.checkBoxAnodicFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnodicFirst.Location = new System.Drawing.Point(68, 27);
            this.checkBoxAnodicFirst.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxAnodicFirst.Name = "checkBoxAnodicFirst";
            this.checkBoxAnodicFirst.Size = new System.Drawing.Size(81, 17);
            this.checkBoxAnodicFirst.TabIndex = 1;
            this.checkBoxAnodicFirst.Text = "Anodic First";
            this.checkBoxAnodicFirst.UseVisualStyleBackColor = true;
            this.checkBoxAnodicFirst.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // buttonAddPulses
            // 
            this.buttonAddPulses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.buttonAddPulses.Location = new System.Drawing.Point(238, 193);
            this.buttonAddPulses.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAddPulses.Name = "buttonAddPulses";
            this.buttonAddPulses.Size = new System.Drawing.Size(75, 28);
            this.buttonAddPulses.TabIndex = 16;
            this.buttonAddPulses.Text = "Apply";
            this.toolTip1.SetToolTip(this.buttonAddPulses, "Applies the currently chosen parameters to all selected contacts. If no contacts " +
        "are selected, this will apply the settings to all contacts.");
            this.buttonAddPulses.UseVisualStyleBackColor = true;
            this.buttonAddPulses.Click += new System.EventHandler(this.ButtonAddPulses_Click);
            // 
            // textboxDelay
            // 
            this.textboxDelay.Location = new System.Drawing.Point(107, 54);
            this.textboxDelay.Margin = new System.Windows.Forms.Padding(2);
            this.textboxDelay.Name = "textboxDelay";
            this.textboxDelay.Size = new System.Drawing.Size(42, 20);
            this.textboxDelay.TabIndex = 5;
            this.textboxDelay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Time);
            this.textboxDelay.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelDelay
            // 
            this.labelDelay.AutoSize = true;
            this.labelDelay.Location = new System.Drawing.Point(46, 57);
            this.labelDelay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDelay.Name = "labelDelay";
            this.labelDelay.Size = new System.Drawing.Size(56, 13);
            this.labelDelay.TabIndex = 4;
            this.labelDelay.Text = "Delay [ms]";
            // 
            // checkboxBiphasicSymmetrical
            // 
            this.checkboxBiphasicSymmetrical.AutoSize = true;
            this.checkboxBiphasicSymmetrical.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkboxBiphasicSymmetrical.Checked = true;
            this.checkboxBiphasicSymmetrical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxBiphasicSymmetrical.Location = new System.Drawing.Point(32, 11);
            this.checkboxBiphasicSymmetrical.Margin = new System.Windows.Forms.Padding(2);
            this.checkboxBiphasicSymmetrical.Name = "checkboxBiphasicSymmetrical";
            this.checkboxBiphasicSymmetrical.Size = new System.Drawing.Size(117, 17);
            this.checkboxBiphasicSymmetrical.TabIndex = 0;
            this.checkboxBiphasicSymmetrical.Text = "Biphasic Symmetric";
            this.checkboxBiphasicSymmetrical.UseVisualStyleBackColor = true;
            this.checkboxBiphasicSymmetrical.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // textboxInterStimulusInterval
            // 
            this.textboxInterStimulusInterval.Location = new System.Drawing.Point(107, 163);
            this.textboxInterStimulusInterval.Margin = new System.Windows.Forms.Padding(2);
            this.textboxInterStimulusInterval.Name = "textboxInterStimulusInterval";
            this.textboxInterStimulusInterval.Size = new System.Drawing.Size(42, 20);
            this.textboxInterStimulusInterval.TabIndex = 11;
            this.textboxInterStimulusInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterKeyPress_Time);
            this.textboxInterStimulusInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // tabControlVisualization
            // 
            this.tabControlVisualization.Controls.Add(this.tabPageWaveform);
            this.tabControlVisualization.Controls.Add(this.tabPageTable);
            this.tabControlVisualization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlVisualization.Location = new System.Drawing.Point(2, 2);
            this.tabControlVisualization.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlVisualization.Name = "tabControlVisualization";
            this.tableLayoutPanel1.SetRowSpan(this.tabControlVisualization, 2);
            this.tabControlVisualization.SelectedIndex = 0;
            this.tabControlVisualization.Size = new System.Drawing.Size(818, 513);
            this.tabControlVisualization.TabIndex = 6;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.zedGraphWaveform);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 22);
            this.tabPageWaveform.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageWaveform.Size = new System.Drawing.Size(810, 487);
            this.tabPageWaveform.TabIndex = 0;
            this.tabPageWaveform.Text = "Stimulus Waveform";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // zedGraphWaveform
            // 
            this.zedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphWaveform.Location = new System.Drawing.Point(2, 2);
            this.zedGraphWaveform.Margin = new System.Windows.Forms.Padding(0);
            this.zedGraphWaveform.Name = "zedGraphWaveform";
            this.zedGraphWaveform.ScrollGrace = 0D;
            this.zedGraphWaveform.ScrollMaxX = 0D;
            this.zedGraphWaveform.ScrollMaxY = 0D;
            this.zedGraphWaveform.ScrollMaxY2 = 0D;
            this.zedGraphWaveform.ScrollMinX = 0D;
            this.zedGraphWaveform.ScrollMinY = 0D;
            this.zedGraphWaveform.ScrollMinY2 = 0D;
            this.zedGraphWaveform.Size = new System.Drawing.Size(806, 483);
            this.zedGraphWaveform.TabIndex = 4;
            this.zedGraphWaveform.TabStop = false;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // tabPageTable
            // 
            this.tabPageTable.Controls.Add(this.dataGridViewStimulusTable);
            this.tabPageTable.Location = new System.Drawing.Point(4, 22);
            this.tabPageTable.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageTable.Name = "tabPageTable";
            this.tabPageTable.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageTable.Size = new System.Drawing.Size(810, 487);
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
            this.dataGridViewStimulusTable.Location = new System.Drawing.Point(2, 2);
            this.dataGridViewStimulusTable.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewStimulusTable.Name = "dataGridViewStimulusTable";
            this.dataGridViewStimulusTable.RowHeadersWidth = 62;
            this.dataGridViewStimulusTable.RowTemplate.Height = 28;
            this.dataGridViewStimulusTable.Size = new System.Drawing.Size(806, 483);
            this.dataGridViewStimulusTable.TabIndex = 0;
            this.dataGridViewStimulusTable.TabStop = false;
            this.dataGridViewStimulusTable.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewStimulusTable_CellEndEdit);
            this.dataGridViewStimulusTable.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridViewStimulusTable_DataBindingComplete);
            this.dataGridViewStimulusTable.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridViewStimulusTable_DataError);
            // 
            // panelProbe
            // 
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(824, 2);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(334, 299);
            this.panelProbe.TabIndex = 1;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1160, 24);
            this.menuStrip.TabIndex = 7;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stimulusWaveformToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // stimulusWaveformToolStripMenuItem
            // 
            this.stimulusWaveformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.saveFileToolStripMenuItem});
            this.stimulusWaveformToolStripMenuItem.Name = "stimulusWaveformToolStripMenuItem";
            this.stimulusWaveformToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.stimulusWaveformToolStripMenuItem.Text = "Stimulus Waveform";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemLoadFile_Click);
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.saveFileToolStripMenuItem.Text = "Save File";
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemSaveFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 338F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControlVisualization, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1160, 585);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panelParameters);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(824, 305);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.SetRowSpan(this.groupBox1, 2);
            this.groupBox1.Size = new System.Drawing.Size(334, 244);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Define Stimuli";
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOk);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 553);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1156, 30);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.buttonResetZoom);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 519);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(818, 30);
            this.flowLayoutPanel2.TabIndex = 8;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonResetZoom.Location = new System.Drawing.Point(2, 2);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(108, 26);
            this.buttonResetZoom.TabIndex = 5;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonResetZoomClick);
            // 
            // numericUpDownNumberOfPulses
            // 
            this.numericUpDownNumberOfPulses.Location = new System.Drawing.Point(272, 165);
            this.numericUpDownNumberOfPulses.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownNumberOfPulses.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNumberOfPulses.Name = "numericUpDownNumberOfPulses";
            this.numericUpDownNumberOfPulses.Size = new System.Drawing.Size(42, 20);
            this.numericUpDownNumberOfPulses.TabIndex = 17;
            this.numericUpDownNumberOfPulses.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNumberOfPulses.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDownNumberOfPulses_KeyDown);
            this.numericUpDownNumberOfPulses.Leave += new System.EventHandler(this.numericUpDownNumberOfPulses_Leave);
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1160, 630);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(102, 50);
            this.Name = "Rhs2116StimulusSequenceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rhs2116StimulusSequenceDialog";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelParameters.ResumeLayout(false);
            this.panelParameters.PerformLayout();
            this.groupBoxCathode.ResumeLayout(false);
            this.groupBoxCathode.PerformLayout();
            this.groupBoxAnode.ResumeLayout(false);
            this.groupBoxAnode.PerformLayout();
            this.tabControlVisualization.ResumeLayout(false);
            this.tabPageWaveform.ResumeLayout(false);
            this.tabPageTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfPulses)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusIsValid;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusSlotsUsed;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelDelay;
        private System.Windows.Forms.TextBox textboxAmplitudeAnodicRequested;
        private System.Windows.Forms.TextBox textboxDelay;
        private System.Windows.Forms.CheckBox checkboxBiphasicSymmetrical;
        private System.Windows.Forms.Button buttonAddPulses;
        private System.Windows.Forms.TextBox textboxInterStimulusInterval;
        private System.Windows.Forms.TextBox textboxPulseWidthAnodic;
        private System.Windows.Forms.TabControl tabControlVisualization;
        private System.Windows.Forms.TabPage tabPageWaveform;
        private System.Windows.Forms.TabPage tabPageTable;
        private System.Windows.Forms.DataGridView dataGridViewStimulusTable;
        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.CheckBox checkBoxAnodicFirst;
        private System.Windows.Forms.TextBox textboxPulseWidthCathodic;
        private System.Windows.Forms.TextBox textboxAmplitudeCathodicRequested;
        private System.Windows.Forms.TextBox textboxInterPulseInterval;
        private System.Windows.Forms.Button buttonClearPulses;
        private System.Windows.Forms.Button buttonReadPulses;
        private System.Windows.Forms.GroupBox groupBoxCathode;
        private System.Windows.Forms.GroupBox groupBoxAnode;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private ZedGraph.ZedGraphControl zedGraphWaveform;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.ToolStripMenuItem stimulusWaveformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxStepSize;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textboxAmplitudeCathodic;
        private System.Windows.Forms.TextBox textboxAmplitudeAnodic;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.NumericUpDown numericUpDownNumberOfPulses;
    }
}
