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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rhs2116StimulusSequenceDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusSlotsUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.panelParameters = new System.Windows.Forms.Panel();
            this.textBoxStepSize = new System.Windows.Forms.TextBox();
            this.groupBoxCathode = new System.Windows.Forms.GroupBox();
            this.labelAmplitudeCathodic = new System.Windows.Forms.Label();
            this.labelPulseWidthCathodic = new System.Windows.Forms.Label();
            this.textboxPulseWidthCathodic = new System.Windows.Forms.TextBox();
            this.textboxAmplitudeCathodic = new System.Windows.Forms.TextBox();
            this.groupBoxAnode = new System.Windows.Forms.GroupBox();
            this.labelAmplitudeAnodic = new System.Windows.Forms.Label();
            this.labelPulseWidthAnodic = new System.Windows.Forms.Label();
            this.textboxPulseWidthAnodic = new System.Windows.Forms.TextBox();
            this.textboxAmplitudeAnodic = new System.Windows.Forms.TextBox();
            this.buttonClearPulses = new System.Windows.Forms.Button();
            this.buttonReadPulses = new System.Windows.Forms.Button();
            this.textboxInterPulseInterval = new System.Windows.Forms.TextBox();
            this.labelInterPulseInterval = new System.Windows.Forms.Label();
            this.labelStepSizeAmplitude = new System.Windows.Forms.Label();
            this.checkBoxAnodicFirst = new System.Windows.Forms.CheckBox();
            this.buttonAddPulses = new System.Windows.Forms.Button();
            this.textboxDelay = new System.Windows.Forms.TextBox();
            this.labelNumberOfPulses = new System.Windows.Forms.Label();
            this.labelDelay = new System.Windows.Forms.Label();
            this.textboxNumberOfStimuli = new System.Windows.Forms.TextBox();
            this.checkboxBiphasicSymmetrical = new System.Windows.Forms.CheckBox();
            this.textboxInterStimulusInterval = new System.Windows.Forms.TextBox();
            this.labelInterStimulusInterval = new System.Windows.Forms.Label();
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
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
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(1359, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(144, 32);
            this.buttonCancel.TabIndex = 0;
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
            this.statusStrip.Location = new System.Drawing.Point(0, 721);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 12, 0);
            this.statusStrip.Size = new System.Drawing.Size(1512, 26);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusIsValid
            // 
            this.toolStripStatusIsValid.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusIsValid.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripStatusIsValid.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusReadyImage;
            this.toolStripStatusIsValid.Name = "toolStripStatusIsValid";
            this.toolStripStatusIsValid.Size = new System.Drawing.Size(194, 20);
            this.toolStripStatusIsValid.Text = "Valid stimulus sequence";
            this.toolStripStatusIsValid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusSlotsUsed
            // 
            this.toolStripStatusSlotsUsed.Name = "toolStripStatusSlotsUsed";
            this.toolStripStatusSlotsUsed.Size = new System.Drawing.Size(132, 20);
            this.toolStripStatusSlotsUsed.Text = "100% of slots used";
            this.toolStripStatusSlotsUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Location = new System.Drawing.Point(1209, 2);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(144, 32);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // panelParameters
            // 
            this.panelParameters.AutoScroll = true;
            this.panelParameters.Controls.Add(this.textBoxStepSize);
            this.panelParameters.Controls.Add(this.groupBoxCathode);
            this.panelParameters.Controls.Add(this.groupBoxAnode);
            this.panelParameters.Controls.Add(this.buttonClearPulses);
            this.panelParameters.Controls.Add(this.buttonReadPulses);
            this.panelParameters.Controls.Add(this.textboxInterPulseInterval);
            this.panelParameters.Controls.Add(this.labelInterPulseInterval);
            this.panelParameters.Controls.Add(this.labelStepSizeAmplitude);
            this.panelParameters.Controls.Add(this.checkBoxAnodicFirst);
            this.panelParameters.Controls.Add(this.buttonAddPulses);
            this.panelParameters.Controls.Add(this.textboxDelay);
            this.panelParameters.Controls.Add(this.labelNumberOfPulses);
            this.panelParameters.Controls.Add(this.labelDelay);
            this.panelParameters.Controls.Add(this.textboxNumberOfStimuli);
            this.panelParameters.Controls.Add(this.checkboxBiphasicSymmetrical);
            this.panelParameters.Controls.Add(this.textboxInterStimulusInterval);
            this.panelParameters.Controls.Add(this.labelInterStimulusInterval);
            this.panelParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParameters.Location = new System.Drawing.Point(1065, 388);
            this.panelParameters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelParameters.Name = "panelParameters";
            this.panelParameters.Size = new System.Drawing.Size(444, 261);
            this.panelParameters.TabIndex = 0;
            // 
            // textBoxStepSize
            // 
            this.textBoxStepSize.Enabled = false;
            this.textBoxStepSize.Location = new System.Drawing.Point(259, 36);
            this.textBoxStepSize.Name = "textBoxStepSize";
            this.textBoxStepSize.ReadOnly = true;
            this.textBoxStepSize.Size = new System.Drawing.Size(132, 22);
            this.textBoxStepSize.TabIndex = 34;
            this.textBoxStepSize.TabStop = false;
            // 
            // groupBoxCathode
            // 
            this.groupBoxCathode.Controls.Add(this.labelAmplitudeCathodic);
            this.groupBoxCathode.Controls.Add(this.labelPulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.textboxPulseWidthCathodic);
            this.groupBoxCathode.Controls.Add(this.textboxAmplitudeCathodic);
            this.groupBoxCathode.Location = new System.Drawing.Point(231, 98);
            this.groupBoxCathode.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxCathode.Name = "groupBoxCathode";
            this.groupBoxCathode.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxCathode.Size = new System.Drawing.Size(195, 69);
            this.groupBoxCathode.TabIndex = 3;
            this.groupBoxCathode.TabStop = false;
            this.groupBoxCathode.Text = "Cathode";
            this.groupBoxCathode.Visible = false;
            // 
            // labelAmplitudeCathodic
            // 
            this.labelAmplitudeCathodic.AutoSize = true;
            this.labelAmplitudeCathodic.Location = new System.Drawing.Point(11, 19);
            this.labelAmplitudeCathodic.Name = "labelAmplitudeCathodic";
            this.labelAmplitudeCathodic.Size = new System.Drawing.Size(94, 16);
            this.labelAmplitudeCathodic.TabIndex = 23;
            this.labelAmplitudeCathodic.Text = "Amplitude [µA]";
            // 
            // labelPulseWidthCathodic
            // 
            this.labelPulseWidthCathodic.AutoSize = true;
            this.labelPulseWidthCathodic.Location = new System.Drawing.Point(11, 44);
            this.labelPulseWidthCathodic.Name = "labelPulseWidthCathodic";
            this.labelPulseWidthCathodic.Size = new System.Drawing.Size(107, 16);
            this.labelPulseWidthCathodic.TabIndex = 24;
            this.labelPulseWidthCathodic.Text = "Pulse Width [ms]";
            // 
            // textboxPulseWidthCathodic
            // 
            this.textboxPulseWidthCathodic.Location = new System.Drawing.Point(132, 39);
            this.textboxPulseWidthCathodic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxPulseWidthCathodic.Name = "textboxPulseWidthCathodic";
            this.textboxPulseWidthCathodic.Size = new System.Drawing.Size(54, 22);
            this.textboxPulseWidthCathodic.TabIndex = 6;
            this.textboxPulseWidthCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.textboxPulseWidthCathodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // textboxAmplitudeCathodic
            // 
            this.textboxAmplitudeCathodic.Location = new System.Drawing.Point(132, 14);
            this.textboxAmplitudeCathodic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxAmplitudeCathodic.Name = "textboxAmplitudeCathodic";
            this.textboxAmplitudeCathodic.Size = new System.Drawing.Size(54, 22);
            this.textboxAmplitudeCathodic.TabIndex = 5;
            this.textboxAmplitudeCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Amplitude);
            this.textboxAmplitudeCathodic.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // groupBoxAnode
            // 
            this.groupBoxAnode.Controls.Add(this.labelAmplitudeAnodic);
            this.groupBoxAnode.Controls.Add(this.labelPulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.textboxPulseWidthAnodic);
            this.groupBoxAnode.Controls.Add(this.textboxAmplitudeAnodic);
            this.groupBoxAnode.Location = new System.Drawing.Point(13, 98);
            this.groupBoxAnode.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxAnode.Name = "groupBoxAnode";
            this.groupBoxAnode.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxAnode.Size = new System.Drawing.Size(195, 69);
            this.groupBoxAnode.TabIndex = 2;
            this.groupBoxAnode.TabStop = false;
            this.groupBoxAnode.Text = "Anode";
            // 
            // labelAmplitudeAnodic
            // 
            this.labelAmplitudeAnodic.AutoSize = true;
            this.labelAmplitudeAnodic.Location = new System.Drawing.Point(8, 19);
            this.labelAmplitudeAnodic.Name = "labelAmplitudeAnodic";
            this.labelAmplitudeAnodic.Size = new System.Drawing.Size(94, 16);
            this.labelAmplitudeAnodic.TabIndex = 4;
            this.labelAmplitudeAnodic.Text = "Amplitude [µA]";
            // 
            // labelPulseWidthAnodic
            // 
            this.labelPulseWidthAnodic.AutoSize = true;
            this.labelPulseWidthAnodic.Location = new System.Drawing.Point(8, 44);
            this.labelPulseWidthAnodic.Name = "labelPulseWidthAnodic";
            this.labelPulseWidthAnodic.Size = new System.Drawing.Size(107, 16);
            this.labelPulseWidthAnodic.TabIndex = 7;
            this.labelPulseWidthAnodic.Text = "Pulse Width [ms]";
            // 
            // textboxPulseWidthAnodic
            // 
            this.textboxPulseWidthAnodic.Location = new System.Drawing.Point(129, 39);
            this.textboxPulseWidthAnodic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxPulseWidthAnodic.Name = "textboxPulseWidthAnodic";
            this.textboxPulseWidthAnodic.Size = new System.Drawing.Size(54, 22);
            this.textboxPulseWidthAnodic.TabIndex = 4;
            this.textboxPulseWidthAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.textboxPulseWidthAnodic.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // textboxAmplitudeAnodic
            // 
            this.textboxAmplitudeAnodic.Location = new System.Drawing.Point(129, 14);
            this.textboxAmplitudeAnodic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxAmplitudeAnodic.Name = "textboxAmplitudeAnodic";
            this.textboxAmplitudeAnodic.Size = new System.Drawing.Size(54, 22);
            this.textboxAmplitudeAnodic.TabIndex = 3;
            this.textboxAmplitudeAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Amplitude);
            this.textboxAmplitudeAnodic.Leave += new System.EventHandler(this.Amplitude_TextChanged);
            // 
            // buttonClearPulses
            // 
            this.buttonClearPulses.Location = new System.Drawing.Point(297, 206);
            this.buttonClearPulses.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonClearPulses.Name = "buttonClearPulses";
            this.buttonClearPulses.Size = new System.Drawing.Size(98, 34);
            this.buttonClearPulses.TabIndex = 33;
            this.buttonClearPulses.TabStop = false;
            this.buttonClearPulses.Text = "Clear Pulses";
            this.toolTip1.SetToolTip(this.buttonClearPulses, "Removes the settings for all selected contacts. If no contacts are selected, this" +
        " will clear the parameters for all contacts.");
            this.buttonClearPulses.UseVisualStyleBackColor = true;
            this.buttonClearPulses.Click += new System.EventHandler(this.ButtonClearPulses_Click);
            // 
            // buttonReadPulses
            // 
            this.buttonReadPulses.Location = new System.Drawing.Point(177, 206);
            this.buttonReadPulses.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonReadPulses.Name = "buttonReadPulses";
            this.buttonReadPulses.Size = new System.Drawing.Size(100, 34);
            this.buttonReadPulses.TabIndex = 32;
            this.buttonReadPulses.TabStop = false;
            this.buttonReadPulses.Text = "Read Pulses";
            this.toolTip1.SetToolTip(this.buttonReadPulses, "If a single contact is selected, this will read the current settings for that con" +
        "tact and display in the parameters. Useful for copying settings from one channel" +
        " to another.");
            this.buttonReadPulses.UseVisualStyleBackColor = true;
            this.buttonReadPulses.Click += new System.EventHandler(this.ButtonReadPulses_Click);
            // 
            // textboxInterPulseInterval
            // 
            this.textboxInterPulseInterval.Location = new System.Drawing.Point(362, 71);
            this.textboxInterPulseInterval.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxInterPulseInterval.Name = "textboxInterPulseInterval";
            this.textboxInterPulseInterval.Size = new System.Drawing.Size(54, 22);
            this.textboxInterPulseInterval.TabIndex = 1;
            this.textboxInterPulseInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.textboxInterPulseInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelInterPulseInterval
            // 
            this.labelInterPulseInterval.AutoSize = true;
            this.labelInterPulseInterval.Location = new System.Drawing.Point(251, 71);
            this.labelInterPulseInterval.Name = "labelInterPulseInterval";
            this.labelInterPulseInterval.Size = new System.Drawing.Size(99, 16);
            this.labelInterPulseInterval.TabIndex = 29;
            this.labelInterPulseInterval.Text = "Inter-Pulse [ms]";
            // 
            // labelStepSizeAmplitude
            // 
            this.labelStepSizeAmplitude.AutoSize = true;
            this.labelStepSizeAmplitude.Location = new System.Drawing.Point(256, 16);
            this.labelStepSizeAmplitude.Name = "labelStepSizeAmplitude";
            this.labelStepSizeAmplitude.Size = new System.Drawing.Size(135, 16);
            this.labelStepSizeAmplitude.TabIndex = 17;
            this.labelStepSizeAmplitude.Text = "Step Size (Amplitude)";
            // 
            // checkBoxAnodicFirst
            // 
            this.checkBoxAnodicFirst.AutoSize = true;
            this.checkBoxAnodicFirst.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxAnodicFirst.Checked = true;
            this.checkBoxAnodicFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnodicFirst.Location = new System.Drawing.Point(97, 36);
            this.checkBoxAnodicFirst.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxAnodicFirst.Name = "checkBoxAnodicFirst";
            this.checkBoxAnodicFirst.Size = new System.Drawing.Size(99, 20);
            this.checkBoxAnodicFirst.TabIndex = 16;
            this.checkBoxAnodicFirst.TabStop = false;
            this.checkBoxAnodicFirst.Text = "Anodic First";
            this.checkBoxAnodicFirst.UseVisualStyleBackColor = true;
            this.checkBoxAnodicFirst.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // buttonAddPulses
            // 
            this.buttonAddPulses.Location = new System.Drawing.Point(57, 206);
            this.buttonAddPulses.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAddPulses.Name = "buttonAddPulses";
            this.buttonAddPulses.Size = new System.Drawing.Size(100, 34);
            this.buttonAddPulses.TabIndex = 6;
            this.buttonAddPulses.Text = "Add Pulse(s)";
            this.toolTip1.SetToolTip(this.buttonAddPulses, "Applies the currently chosen parameters to all selected contacts. If no contacts " +
        "are selected, this will apply the settings to all contacts.");
            this.buttonAddPulses.UseVisualStyleBackColor = true;
            this.buttonAddPulses.Click += new System.EventHandler(this.ButtonAddPulses_Click);
            // 
            // textboxDelay
            // 
            this.textboxDelay.Location = new System.Drawing.Point(142, 69);
            this.textboxDelay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxDelay.Name = "textboxDelay";
            this.textboxDelay.Size = new System.Drawing.Size(54, 22);
            this.textboxDelay.TabIndex = 0;
            this.textboxDelay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.textboxDelay.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelNumberOfPulses
            // 
            this.labelNumberOfPulses.AutoSize = true;
            this.labelNumberOfPulses.Location = new System.Drawing.Point(237, 178);
            this.labelNumberOfPulses.Name = "labelNumberOfPulses";
            this.labelNumberOfPulses.Size = new System.Drawing.Size(113, 16);
            this.labelNumberOfPulses.TabIndex = 13;
            this.labelNumberOfPulses.Text = "Number of Pulses";
            // 
            // labelDelay
            // 
            this.labelDelay.AutoSize = true;
            this.labelDelay.Location = new System.Drawing.Point(68, 74);
            this.labelDelay.Name = "labelDelay";
            this.labelDelay.Size = new System.Drawing.Size(72, 16);
            this.labelDelay.TabIndex = 3;
            this.labelDelay.Text = "Delay [ms]";
            // 
            // textboxNumberOfStimuli
            // 
            this.textboxNumberOfStimuli.Location = new System.Drawing.Point(362, 174);
            this.textboxNumberOfStimuli.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxNumberOfStimuli.Name = "textboxNumberOfStimuli";
            this.textboxNumberOfStimuli.Size = new System.Drawing.Size(54, 22);
            this.textboxNumberOfStimuli.TabIndex = 5;
            // 
            // checkboxBiphasicSymmetrical
            // 
            this.checkboxBiphasicSymmetrical.AutoSize = true;
            this.checkboxBiphasicSymmetrical.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkboxBiphasicSymmetrical.Checked = true;
            this.checkboxBiphasicSymmetrical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxBiphasicSymmetrical.Location = new System.Drawing.Point(49, 16);
            this.checkboxBiphasicSymmetrical.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkboxBiphasicSymmetrical.Name = "checkboxBiphasicSymmetrical";
            this.checkboxBiphasicSymmetrical.Size = new System.Drawing.Size(147, 20);
            this.checkboxBiphasicSymmetrical.TabIndex = 5;
            this.checkboxBiphasicSymmetrical.TabStop = false;
            this.checkboxBiphasicSymmetrical.Text = "Biphasic Symmetric";
            this.checkboxBiphasicSymmetrical.UseVisualStyleBackColor = true;
            this.checkboxBiphasicSymmetrical.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // textboxInterStimulusInterval
            // 
            this.textboxInterStimulusInterval.Location = new System.Drawing.Point(142, 174);
            this.textboxInterStimulusInterval.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textboxInterStimulusInterval.Name = "textboxInterStimulusInterval";
            this.textboxInterStimulusInterval.Size = new System.Drawing.Size(54, 22);
            this.textboxInterStimulusInterval.TabIndex = 4;
            this.textboxInterStimulusInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress_Time);
            this.textboxInterStimulusInterval.Leave += new System.EventHandler(this.Samples_TextChanged);
            // 
            // labelInterStimulusInterval
            // 
            this.labelInterStimulusInterval.AutoSize = true;
            this.labelInterStimulusInterval.Location = new System.Drawing.Point(15, 176);
            this.labelInterStimulusInterval.Name = "labelInterStimulusInterval";
            this.labelInterStimulusInterval.Size = new System.Drawing.Size(115, 16);
            this.labelInterStimulusInterval.TabIndex = 9;
            this.labelInterStimulusInterval.Text = "Inter-Stimulus [ms]";
            // 
            // tabControlVisualization
            // 
            this.tabControlVisualization.Controls.Add(this.tabPageWaveform);
            this.tabControlVisualization.Controls.Add(this.tabPageTable);
            this.tabControlVisualization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlVisualization.Location = new System.Drawing.Point(3, 2);
            this.tabControlVisualization.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlVisualization.Name = "tabControlVisualization";
            this.tableLayoutPanel1.SetRowSpan(this.tabControlVisualization, 2);
            this.tabControlVisualization.SelectedIndex = 0;
            this.tabControlVisualization.Size = new System.Drawing.Size(1056, 647);
            this.tabControlVisualization.TabIndex = 6;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.zedGraphWaveform);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 25);
            this.tabPageWaveform.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageWaveform.Size = new System.Drawing.Size(1048, 618);
            this.tabPageWaveform.TabIndex = 0;
            this.tabPageWaveform.Text = "Stimulus Waveform";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // zedGraphWaveform
            // 
            this.zedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphWaveform.Location = new System.Drawing.Point(3, 2);
            this.zedGraphWaveform.Margin = new System.Windows.Forms.Padding(0);
            this.zedGraphWaveform.Name = "zedGraphWaveform";
            this.zedGraphWaveform.ScrollGrace = 0D;
            this.zedGraphWaveform.ScrollMaxX = 0D;
            this.zedGraphWaveform.ScrollMaxY = 0D;
            this.zedGraphWaveform.ScrollMaxY2 = 0D;
            this.zedGraphWaveform.ScrollMinX = 0D;
            this.zedGraphWaveform.ScrollMinY = 0D;
            this.zedGraphWaveform.ScrollMinY2 = 0D;
            this.zedGraphWaveform.Size = new System.Drawing.Size(1042, 614);
            this.zedGraphWaveform.TabIndex = 4;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // tabPageTable
            // 
            this.tabPageTable.Controls.Add(this.dataGridViewStimulusTable);
            this.tabPageTable.Location = new System.Drawing.Point(4, 25);
            this.tabPageTable.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTable.Name = "tabPageTable";
            this.tabPageTable.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTable.Size = new System.Drawing.Size(1048, 618);
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
            this.dataGridViewStimulusTable.Location = new System.Drawing.Point(3, 2);
            this.dataGridViewStimulusTable.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridViewStimulusTable.Name = "dataGridViewStimulusTable";
            this.dataGridViewStimulusTable.RowHeadersWidth = 62;
            this.dataGridViewStimulusTable.RowTemplate.Height = 28;
            this.dataGridViewStimulusTable.Size = new System.Drawing.Size(1042, 614);
            this.dataGridViewStimulusTable.TabIndex = 0;
            this.dataGridViewStimulusTable.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewStimulusTable_CellEndEdit);
            this.dataGridViewStimulusTable.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridViewStimulusTable_DataBindingComplete);
            this.dataGridViewStimulusTable.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridViewStimulusTable_DataError);
            // 
            // panelProbe
            // 
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(1065, 2);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(444, 382);
            this.panelProbe.TabIndex = 0;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1512, 28);
            this.menuStrip.TabIndex = 7;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stimulusWaveformToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // stimulusWaveformToolStripMenuItem
            // 
            this.stimulusWaveformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.saveFileToolStripMenuItem});
            this.stimulusWaveformToolStripMenuItem.Name = "stimulusWaveformToolStripMenuItem";
            this.stimulusWaveformToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.stimulusWaveformToolStripMenuItem.Text = "Stimulus Waveform";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemLoadFile_Click);
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.saveFileToolStripMenuItem.Text = "Save File";
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemSaveFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 450F));
            this.tableLayoutPanel1.Controls.Add(this.panelParameters, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControlVisualization, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 265F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1512, 693);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOk);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 654);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1506, 36);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1512, 747);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(132, 54);
            this.Name = "Rhs2116StimulusSequenceDialog";
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
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusIsValid;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusSlotsUsed;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelAmplitudeAnodic;
        private System.Windows.Forms.Label labelDelay;
        private System.Windows.Forms.TextBox textboxAmplitudeAnodic;
        private System.Windows.Forms.TextBox textboxDelay;
        private System.Windows.Forms.CheckBox checkboxBiphasicSymmetrical;
        private System.Windows.Forms.Button buttonAddPulses;
        private System.Windows.Forms.Label labelNumberOfPulses;
        private System.Windows.Forms.TextBox textboxNumberOfStimuli;
        private System.Windows.Forms.Label labelInterStimulusInterval;
        private System.Windows.Forms.TextBox textboxInterStimulusInterval;
        private System.Windows.Forms.Label labelPulseWidthAnodic;
        private System.Windows.Forms.TextBox textboxPulseWidthAnodic;
        private System.Windows.Forms.TabControl tabControlVisualization;
        private System.Windows.Forms.TabPage tabPageWaveform;
        private System.Windows.Forms.TabPage tabPageTable;
        private System.Windows.Forms.DataGridView dataGridViewStimulusTable;
        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.Label labelStepSizeAmplitude;
        private System.Windows.Forms.CheckBox checkBoxAnodicFirst;
        private System.Windows.Forms.Label labelAmplitudeCathodic;
        private System.Windows.Forms.Label labelPulseWidthCathodic;
        private System.Windows.Forms.TextBox textboxPulseWidthCathodic;
        private System.Windows.Forms.TextBox textboxAmplitudeCathodic;
        private System.Windows.Forms.TextBox textboxInterPulseInterval;
        private System.Windows.Forms.Label labelInterPulseInterval;
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
    }
}
