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
            this.ZedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.LinkLabelDocumentation = new System.Windows.Forms.LinkLabel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.ToolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.ToolStripStatusSlotsUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.TabControlParameters = new System.Windows.Forms.TabControl();
            this.TabPageAddPulse = new System.Windows.Forms.TabPage();
            this.PanelParameters = new System.Windows.Forms.Panel();
            this.GroupBoxCathode = new System.Windows.Forms.GroupBox();
            this.PulseWidthCathodicConverted = new System.Windows.Forms.TextBox();
            this.AmplitudeCathodicConverted = new System.Windows.Forms.TextBox();
            this.LabelAmplitudeCathodic = new System.Windows.Forms.Label();
            this.LabelPulseWidthCathodicText = new System.Windows.Forms.Label();
            this.PulseWidthCathodic = new System.Windows.Forms.TextBox();
            this.AmplitudeCathodic = new System.Windows.Forms.TextBox();
            this.GroupBoxAnode = new System.Windows.Forms.GroupBox();
            this.PulseWidthAnodicConverted = new System.Windows.Forms.TextBox();
            this.AmplitudeAnodicConverted = new System.Windows.Forms.TextBox();
            this.LabelAnplitudeAnodicText = new System.Windows.Forms.Label();
            this.LabelPulseWidthAnodicText = new System.Windows.Forms.Label();
            this.PulseWidthAnodic = new System.Windows.Forms.TextBox();
            this.AmplitudeAnodic = new System.Windows.Forms.TextBox();
            this.ComboBoxStepSize = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.InterStimulusIntervalConverted = new System.Windows.Forms.TextBox();
            this.InterPulseIntervalConverted = new System.Windows.Forms.TextBox();
            this.InterPulseInterval = new System.Windows.Forms.TextBox();
            this.InterPulseIntervalText = new System.Windows.Forms.Label();
            this.LabelStepSize = new System.Windows.Forms.Label();
            this.CheckBoxAnodicFirst = new System.Windows.Forms.CheckBox();
            this.DelaySamplesConverted = new System.Windows.Forms.TextBox();
            this.ButtonAddPulses = new System.Windows.Forms.Button();
            this.DelaySamples = new System.Windows.Forms.TextBox();
            this.NumberOfPulsesText = new System.Windows.Forms.Label();
            this.DelayText = new System.Windows.Forms.Label();
            this.NumberOfStimuli = new System.Windows.Forms.TextBox();
            this.CheckboxBiphasicSymmetrical = new System.Windows.Forms.CheckBox();
            this.InterStimulusInterval = new System.Windows.Forms.TextBox();
            this.InterStimulusIntervalText = new System.Windows.Forms.Label();
            this.TabPageChannelLayout = new System.Windows.Forms.TabPage();
            this.PanelChannelLayout = new System.Windows.Forms.Panel();
            this.CustomChannelLayout = new System.Windows.Forms.Button();
            this.DefaultChannelLayout = new System.Windows.Forms.Button();
            this.TextBoxChannelLayoutFilePath = new System.Windows.Forms.TextBox();
            this.TabPageEditorDialog = new System.Windows.Forms.TabPage();
            this.PropertyGridStimulusSequence = new System.Windows.Forms.PropertyGrid();
            this.TabControlVisualization = new System.Windows.Forms.TabControl();
            this.TabPageWaveform = new System.Windows.Forms.TabPage();
            this.TabPageTable = new System.Windows.Forms.TabPage();
            this.DataGridViewStimulusTable = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.ZedGraphChannels = new ZedGraph.ZedGraphControl();
            this.StatusStrip.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.TabControlParameters.SuspendLayout();
            this.TabPageAddPulse.SuspendLayout();
            this.PanelParameters.SuspendLayout();
            this.GroupBoxCathode.SuspendLayout();
            this.GroupBoxAnode.SuspendLayout();
            this.TabPageChannelLayout.SuspendLayout();
            this.PanelChannelLayout.SuspendLayout();
            this.TabPageEditorDialog.SuspendLayout();
            this.TabControlVisualization.SuspendLayout();
            this.TabPageWaveform.SuspendLayout();
            this.TabPageTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewStimulusTable)).BeginInit();
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
            // ZedGraphWaveform
            // 
            this.ZedGraphWaveform.AutoScroll = true;
            this.ZedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphWaveform.Location = new System.Drawing.Point(2, 2);
            this.ZedGraphWaveform.Name = "ZedGraphWaveform";
            this.ZedGraphWaveform.ScrollGrace = 0D;
            this.ZedGraphWaveform.ScrollMaxX = 0D;
            this.ZedGraphWaveform.ScrollMaxY = 0D;
            this.ZedGraphWaveform.ScrollMaxY2 = 0D;
            this.ZedGraphWaveform.ScrollMinX = 0D;
            this.ZedGraphWaveform.ScrollMinY = 0D;
            this.ZedGraphWaveform.ScrollMinY2 = 0D;
            this.ZedGraphWaveform.Size = new System.Drawing.Size(934, 521);
            this.ZedGraphWaveform.TabIndex = 4;
            this.ZedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // LinkLabelDocumentation
            // 
            this.LinkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkLabelDocumentation.AutoSize = true;
            this.LinkLabelDocumentation.Location = new System.Drawing.Point(1114, 556);
            this.LinkLabelDocumentation.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LinkLabelDocumentation.Name = "LinkLabelDocumentation";
            this.LinkLabelDocumentation.Size = new System.Drawing.Size(79, 13);
            this.LinkLabelDocumentation.TabIndex = 4;
            this.LinkLabelDocumentation.TabStop = true;
            this.LinkLabelDocumentation.Text = "Documentation";
            this.LinkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDocumentation_LinkClicked);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(177, 0);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(67, 26);
            this.ButtonCancel.TabIndex = 0;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // StatusStrip
            // 
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripStatusIsValid,
            this.ToolStripStatusSlotsUsed});
            this.StatusStrip.Location = new System.Drawing.Point(0, 551);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.StatusStrip.Size = new System.Drawing.Size(1198, 22);
            this.StatusStrip.SizingGrip = false;
            this.StatusStrip.TabIndex = 1;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // ToolStripStatusIsValid
            // 
            this.ToolStripStatusIsValid.Name = "ToolStripStatusIsValid";
            this.ToolStripStatusIsValid.Size = new System.Drawing.Size(133, 17);
            this.ToolStripStatusIsValid.Text = "Valid stimulus sequence";
            // 
            // ToolStripStatusSlotsUsed
            // 
            this.ToolStripStatusSlotsUsed.Name = "ToolStripStatusSlotsUsed";
            this.ToolStripStatusSlotsUsed.Size = new System.Drawing.Size(104, 17);
            this.ToolStripStatusSlotsUsed.Text = "100% of slots used";
            this.ToolStripStatusSlotsUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.Controls.Add(this.ButtonOk);
            this.PanelButtons.Controls.Add(this.ButtonCancel);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(0, 0);
            this.PanelButtons.Margin = new System.Windows.Forms.Padding(2);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(249, 28);
            this.PanelButtons.TabIndex = 0;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(106, 0);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(67, 26);
            this.ButtonOk.TabIndex = 4;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // TabControlParameters
            // 
            this.TabControlParameters.Controls.Add(this.TabPageAddPulse);
            this.TabControlParameters.Controls.Add(this.TabPageChannelLayout);
            this.TabControlParameters.Controls.Add(this.TabPageEditorDialog);
            this.TabControlParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControlParameters.Location = new System.Drawing.Point(0, 0);
            this.TabControlParameters.Margin = new System.Windows.Forms.Padding(2);
            this.TabControlParameters.MinimumSize = new System.Drawing.Size(248, 315);
            this.TabControlParameters.Name = "TabControlParameters";
            this.TabControlParameters.SelectedIndex = 0;
            this.TabControlParameters.Size = new System.Drawing.Size(249, 315);
            this.TabControlParameters.TabIndex = 1;
            // 
            // TabPageAddPulse
            // 
            this.TabPageAddPulse.Controls.Add(this.PanelParameters);
            this.TabPageAddPulse.Location = new System.Drawing.Point(4, 22);
            this.TabPageAddPulse.Margin = new System.Windows.Forms.Padding(2);
            this.TabPageAddPulse.Name = "TabPageAddPulse";
            this.TabPageAddPulse.Padding = new System.Windows.Forms.Padding(2);
            this.TabPageAddPulse.Size = new System.Drawing.Size(241, 289);
            this.TabPageAddPulse.TabIndex = 0;
            this.TabPageAddPulse.Text = "Add Pulses";
            this.TabPageAddPulse.UseVisualStyleBackColor = true;
            // 
            // PanelParameters
            // 
            this.PanelParameters.AutoScroll = true;
            this.PanelParameters.AutoSize = true;
            this.PanelParameters.Controls.Add(this.GroupBoxCathode);
            this.PanelParameters.Controls.Add(this.GroupBoxAnode);
            this.PanelParameters.Controls.Add(this.ComboBoxStepSize);
            this.PanelParameters.Controls.Add(this.button2);
            this.PanelParameters.Controls.Add(this.button1);
            this.PanelParameters.Controls.Add(this.InterStimulusIntervalConverted);
            this.PanelParameters.Controls.Add(this.InterPulseIntervalConverted);
            this.PanelParameters.Controls.Add(this.InterPulseInterval);
            this.PanelParameters.Controls.Add(this.InterPulseIntervalText);
            this.PanelParameters.Controls.Add(this.LabelStepSize);
            this.PanelParameters.Controls.Add(this.CheckBoxAnodicFirst);
            this.PanelParameters.Controls.Add(this.DelaySamplesConverted);
            this.PanelParameters.Controls.Add(this.ButtonAddPulses);
            this.PanelParameters.Controls.Add(this.DelaySamples);
            this.PanelParameters.Controls.Add(this.NumberOfPulsesText);
            this.PanelParameters.Controls.Add(this.DelayText);
            this.PanelParameters.Controls.Add(this.NumberOfStimuli);
            this.PanelParameters.Controls.Add(this.CheckboxBiphasicSymmetrical);
            this.PanelParameters.Controls.Add(this.InterStimulusInterval);
            this.PanelParameters.Controls.Add(this.InterStimulusIntervalText);
            this.PanelParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelParameters.Location = new System.Drawing.Point(2, 2);
            this.PanelParameters.Margin = new System.Windows.Forms.Padding(2);
            this.PanelParameters.MinimumSize = new System.Drawing.Size(239, 284);
            this.PanelParameters.Name = "PanelParameters";
            this.PanelParameters.Size = new System.Drawing.Size(239, 285);
            this.PanelParameters.TabIndex = 0;
            // 
            // GroupBoxCathode
            // 
            this.GroupBoxCathode.Controls.Add(this.PulseWidthCathodicConverted);
            this.GroupBoxCathode.Controls.Add(this.AmplitudeCathodicConverted);
            this.GroupBoxCathode.Controls.Add(this.LabelAmplitudeCathodic);
            this.GroupBoxCathode.Controls.Add(this.LabelPulseWidthCathodicText);
            this.GroupBoxCathode.Controls.Add(this.PulseWidthCathodic);
            this.GroupBoxCathode.Controls.Add(this.AmplitudeCathodic);
            this.GroupBoxCathode.Location = new System.Drawing.Point(0, 149);
            this.GroupBoxCathode.Name = "GroupBoxCathode";
            this.GroupBoxCathode.Size = new System.Drawing.Size(239, 56);
            this.GroupBoxCathode.TabIndex = 4;
            this.GroupBoxCathode.TabStop = false;
            this.GroupBoxCathode.Text = "Cathode";
            this.GroupBoxCathode.Visible = false;
            // 
            // PulseWidthCathodicConverted
            // 
            this.PulseWidthCathodicConverted.Enabled = false;
            this.PulseWidthCathodicConverted.Location = new System.Drawing.Point(166, 32);
            this.PulseWidthCathodicConverted.Margin = new System.Windows.Forms.Padding(2);
            this.PulseWidthCathodicConverted.Name = "PulseWidthCathodicConverted";
            this.PulseWidthCathodicConverted.ReadOnly = true;
            this.PulseWidthCathodicConverted.Size = new System.Drawing.Size(65, 20);
            this.PulseWidthCathodicConverted.TabIndex = 27;
            this.PulseWidthCathodicConverted.TabStop = false;
            // 
            // AmplitudeCathodicConverted
            // 
            this.AmplitudeCathodicConverted.Enabled = false;
            this.AmplitudeCathodicConverted.Location = new System.Drawing.Point(166, 12);
            this.AmplitudeCathodicConverted.Margin = new System.Windows.Forms.Padding(2);
            this.AmplitudeCathodicConverted.Name = "AmplitudeCathodicConverted";
            this.AmplitudeCathodicConverted.ReadOnly = true;
            this.AmplitudeCathodicConverted.Size = new System.Drawing.Size(65, 20);
            this.AmplitudeCathodicConverted.TabIndex = 26;
            this.AmplitudeCathodicConverted.TabStop = false;
            // 
            // LabelAmplitudeCathodic
            // 
            this.LabelAmplitudeCathodic.AutoSize = true;
            this.LabelAmplitudeCathodic.Location = new System.Drawing.Point(39, 16);
            this.LabelAmplitudeCathodic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelAmplitudeCathodic.Name = "LabelAmplitudeCathodic";
            this.LabelAmplitudeCathodic.Size = new System.Drawing.Size(83, 13);
            this.LabelAmplitudeCathodic.TabIndex = 23;
            this.LabelAmplitudeCathodic.Text = "Amplitude Steps";
            // 
            // LabelPulseWidthCathodicText
            // 
            this.LabelPulseWidthCathodicText.AutoSize = true;
            this.LabelPulseWidthCathodicText.Location = new System.Drawing.Point(15, 36);
            this.LabelPulseWidthCathodicText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelPulseWidthCathodicText.Name = "LabelPulseWidthCathodicText";
            this.LabelPulseWidthCathodicText.Size = new System.Drawing.Size(107, 13);
            this.LabelPulseWidthCathodicText.TabIndex = 24;
            this.LabelPulseWidthCathodicText.Text = "Pulse Width Samples";
            // 
            // PulseWidthCathodic
            // 
            this.PulseWidthCathodic.Location = new System.Drawing.Point(126, 32);
            this.PulseWidthCathodic.Margin = new System.Windows.Forms.Padding(2);
            this.PulseWidthCathodic.Name = "PulseWidthCathodic";
            this.PulseWidthCathodic.Size = new System.Drawing.Size(30, 20);
            this.PulseWidthCathodic.TabIndex = 5;
            this.PulseWidthCathodic.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.PulseWidthCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // AmplitudeCathodic
            // 
            this.AmplitudeCathodic.Location = new System.Drawing.Point(126, 12);
            this.AmplitudeCathodic.Margin = new System.Windows.Forms.Padding(2);
            this.AmplitudeCathodic.Name = "AmplitudeCathodic";
            this.AmplitudeCathodic.Size = new System.Drawing.Size(30, 20);
            this.AmplitudeCathodic.TabIndex = 4;
            this.AmplitudeCathodic.TextChanged += new System.EventHandler(this.Amplitude_TextChanged);
            this.AmplitudeCathodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // GroupBoxAnode
            // 
            this.GroupBoxAnode.Controls.Add(this.PulseWidthAnodicConverted);
            this.GroupBoxAnode.Controls.Add(this.AmplitudeAnodicConverted);
            this.GroupBoxAnode.Controls.Add(this.LabelAnplitudeAnodicText);
            this.GroupBoxAnode.Controls.Add(this.LabelPulseWidthAnodicText);
            this.GroupBoxAnode.Controls.Add(this.PulseWidthAnodic);
            this.GroupBoxAnode.Controls.Add(this.AmplitudeAnodic);
            this.GroupBoxAnode.Location = new System.Drawing.Point(0, 70);
            this.GroupBoxAnode.Name = "GroupBoxAnode";
            this.GroupBoxAnode.Size = new System.Drawing.Size(239, 56);
            this.GroupBoxAnode.TabIndex = 1;
            this.GroupBoxAnode.TabStop = false;
            this.GroupBoxAnode.Text = "Anode";
            // 
            // PulseWidthAnodicConverted
            // 
            this.PulseWidthAnodicConverted.Enabled = false;
            this.PulseWidthAnodicConverted.Location = new System.Drawing.Point(166, 32);
            this.PulseWidthAnodicConverted.Margin = new System.Windows.Forms.Padding(2);
            this.PulseWidthAnodicConverted.Name = "PulseWidthAnodicConverted";
            this.PulseWidthAnodicConverted.ReadOnly = true;
            this.PulseWidthAnodicConverted.Size = new System.Drawing.Size(65, 20);
            this.PulseWidthAnodicConverted.TabIndex = 20;
            this.PulseWidthAnodicConverted.TabStop = false;
            // 
            // AmplitudeAnodicConverted
            // 
            this.AmplitudeAnodicConverted.Enabled = false;
            this.AmplitudeAnodicConverted.Location = new System.Drawing.Point(166, 12);
            this.AmplitudeAnodicConverted.Margin = new System.Windows.Forms.Padding(2);
            this.AmplitudeAnodicConverted.Name = "AmplitudeAnodicConverted";
            this.AmplitudeAnodicConverted.ReadOnly = true;
            this.AmplitudeAnodicConverted.Size = new System.Drawing.Size(65, 20);
            this.AmplitudeAnodicConverted.TabIndex = 19;
            this.AmplitudeAnodicConverted.TabStop = false;
            // 
            // LabelAnplitudeAnodicText
            // 
            this.LabelAnplitudeAnodicText.AutoSize = true;
            this.LabelAnplitudeAnodicText.Location = new System.Drawing.Point(39, 16);
            this.LabelAnplitudeAnodicText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelAnplitudeAnodicText.Name = "LabelAnplitudeAnodicText";
            this.LabelAnplitudeAnodicText.Size = new System.Drawing.Size(83, 13);
            this.LabelAnplitudeAnodicText.TabIndex = 4;
            this.LabelAnplitudeAnodicText.Text = "Amplitude Steps";
            // 
            // LabelPulseWidthAnodicText
            // 
            this.LabelPulseWidthAnodicText.AutoSize = true;
            this.LabelPulseWidthAnodicText.Location = new System.Drawing.Point(15, 36);
            this.LabelPulseWidthAnodicText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelPulseWidthAnodicText.Name = "LabelPulseWidthAnodicText";
            this.LabelPulseWidthAnodicText.Size = new System.Drawing.Size(107, 13);
            this.LabelPulseWidthAnodicText.TabIndex = 7;
            this.LabelPulseWidthAnodicText.Text = "Pulse Width Samples";
            // 
            // PulseWidthAnodic
            // 
            this.PulseWidthAnodic.Location = new System.Drawing.Point(126, 32);
            this.PulseWidthAnodic.Margin = new System.Windows.Forms.Padding(2);
            this.PulseWidthAnodic.Name = "PulseWidthAnodic";
            this.PulseWidthAnodic.Size = new System.Drawing.Size(30, 20);
            this.PulseWidthAnodic.TabIndex = 2;
            this.PulseWidthAnodic.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.PulseWidthAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // AmplitudeAnodic
            // 
            this.AmplitudeAnodic.Location = new System.Drawing.Point(126, 12);
            this.AmplitudeAnodic.Margin = new System.Windows.Forms.Padding(2);
            this.AmplitudeAnodic.Name = "AmplitudeAnodic";
            this.AmplitudeAnodic.Size = new System.Drawing.Size(30, 20);
            this.AmplitudeAnodic.TabIndex = 1;
            this.AmplitudeAnodic.TextChanged += new System.EventHandler(this.Amplitude_TextChanged);
            this.AmplitudeAnodic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // ComboBoxStepSize
            // 
            this.ComboBoxStepSize.FormattingEnabled = true;
            this.ComboBoxStepSize.Location = new System.Drawing.Point(132, 46);
            this.ComboBoxStepSize.Margin = new System.Windows.Forms.Padding(2);
            this.ComboBoxStepSize.Name = "ComboBoxStepSize";
            this.ComboBoxStepSize.Size = new System.Drawing.Size(90, 21);
            this.ComboBoxStepSize.TabIndex = 34;
            this.ComboBoxStepSize.TabStop = false;
            this.ComboBoxStepSize.SelectedIndexChanged += new System.EventHandler(this.ComboBoxStepSize_SelectedIndexChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(161, 255);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(73, 27);
            this.button2.TabIndex = 33;
            this.button2.TabStop = false;
            this.button2.Text = "Clear Pulses";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(83, 255);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 27);
            this.button1.TabIndex = 32;
            this.button1.TabStop = false;
            this.button1.Text = "Read Pulses";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // InterStimulusIntervalConverted
            // 
            this.InterStimulusIntervalConverted.Enabled = false;
            this.InterStimulusIntervalConverted.Location = new System.Drawing.Point(166, 207);
            this.InterStimulusIntervalConverted.Margin = new System.Windows.Forms.Padding(2);
            this.InterStimulusIntervalConverted.Name = "InterStimulusIntervalConverted";
            this.InterStimulusIntervalConverted.ReadOnly = true;
            this.InterStimulusIntervalConverted.Size = new System.Drawing.Size(65, 20);
            this.InterStimulusIntervalConverted.TabIndex = 31;
            this.InterStimulusIntervalConverted.TabStop = false;
            // 
            // InterPulseIntervalConverted
            // 
            this.InterPulseIntervalConverted.Enabled = false;
            this.InterPulseIntervalConverted.Location = new System.Drawing.Point(166, 129);
            this.InterPulseIntervalConverted.Margin = new System.Windows.Forms.Padding(2);
            this.InterPulseIntervalConverted.Name = "InterPulseIntervalConverted";
            this.InterPulseIntervalConverted.ReadOnly = true;
            this.InterPulseIntervalConverted.Size = new System.Drawing.Size(65, 20);
            this.InterPulseIntervalConverted.TabIndex = 30;
            this.InterPulseIntervalConverted.TabStop = false;
            // 
            // InterPulseInterval
            // 
            this.InterPulseInterval.Location = new System.Drawing.Point(126, 129);
            this.InterPulseInterval.Margin = new System.Windows.Forms.Padding(2);
            this.InterPulseInterval.Name = "InterPulseInterval";
            this.InterPulseInterval.Size = new System.Drawing.Size(30, 20);
            this.InterPulseInterval.TabIndex = 3;
            this.InterPulseInterval.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.InterPulseInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // InterPulseIntervalText
            // 
            this.InterPulseIntervalText.AutoSize = true;
            this.InterPulseIntervalText.Location = new System.Drawing.Point(22, 133);
            this.InterPulseIntervalText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InterPulseIntervalText.Name = "InterPulseIntervalText";
            this.InterPulseIntervalText.Size = new System.Drawing.Size(100, 13);
            this.InterPulseIntervalText.TabIndex = 29;
            this.InterPulseIntervalText.Text = "Inter-Pulse Samples";
            // 
            // LabelStepSize
            // 
            this.LabelStepSize.AutoSize = true;
            this.LabelStepSize.Location = new System.Drawing.Point(132, 31);
            this.LabelStepSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelStepSize.Name = "LabelStepSize";
            this.LabelStepSize.Size = new System.Drawing.Size(52, 13);
            this.LabelStepSize.TabIndex = 17;
            this.LabelStepSize.Text = "Step Size";
            // 
            // CheckBoxAnodicFirst
            // 
            this.CheckBoxAnodicFirst.AutoSize = true;
            this.CheckBoxAnodicFirst.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckBoxAnodicFirst.Checked = true;
            this.CheckBoxAnodicFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckBoxAnodicFirst.Location = new System.Drawing.Point(42, 49);
            this.CheckBoxAnodicFirst.Margin = new System.Windows.Forms.Padding(2);
            this.CheckBoxAnodicFirst.Name = "CheckBoxAnodicFirst";
            this.CheckBoxAnodicFirst.Size = new System.Drawing.Size(81, 17);
            this.CheckBoxAnodicFirst.TabIndex = 16;
            this.CheckBoxAnodicFirst.TabStop = false;
            this.CheckBoxAnodicFirst.Text = "Anodic First";
            this.CheckBoxAnodicFirst.UseVisualStyleBackColor = true;
            // 
            // DelaySamplesConverted
            // 
            this.DelaySamplesConverted.Enabled = false;
            this.DelaySamplesConverted.Location = new System.Drawing.Point(166, 7);
            this.DelaySamplesConverted.Margin = new System.Windows.Forms.Padding(2);
            this.DelaySamplesConverted.Name = "DelaySamplesConverted";
            this.DelaySamplesConverted.ReadOnly = true;
            this.DelaySamplesConverted.Size = new System.Drawing.Size(65, 20);
            this.DelaySamplesConverted.TabIndex = 15;
            this.DelaySamplesConverted.TabStop = false;
            // 
            // ButtonAddPulses
            // 
            this.ButtonAddPulses.Location = new System.Drawing.Point(5, 255);
            this.ButtonAddPulses.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonAddPulses.Name = "ButtonAddPulses";
            this.ButtonAddPulses.Size = new System.Drawing.Size(75, 27);
            this.ButtonAddPulses.TabIndex = 14;
            this.ButtonAddPulses.TabStop = false;
            this.ButtonAddPulses.Text = "Add Pulse(s)";
            this.ButtonAddPulses.UseVisualStyleBackColor = true;
            this.ButtonAddPulses.Click += new System.EventHandler(this.ButtonAddPulses_Click);
            // 
            // DelaySamples
            // 
            this.DelaySamples.Location = new System.Drawing.Point(126, 7);
            this.DelaySamples.Margin = new System.Windows.Forms.Padding(2);
            this.DelaySamples.Name = "DelaySamples";
            this.DelaySamples.Size = new System.Drawing.Size(30, 20);
            this.DelaySamples.TabIndex = 0;
            this.DelaySamples.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.DelaySamples.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // NumberOfPulsesText
            // 
            this.NumberOfPulsesText.AutoSize = true;
            this.NumberOfPulsesText.Location = new System.Drawing.Point(32, 232);
            this.NumberOfPulsesText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.NumberOfPulsesText.Name = "NumberOfPulsesText";
            this.NumberOfPulsesText.Size = new System.Drawing.Size(90, 13);
            this.NumberOfPulsesText.TabIndex = 13;
            this.NumberOfPulsesText.Text = "Number of Pulses";
            // 
            // DelayText
            // 
            this.DelayText.AutoSize = true;
            this.DelayText.Location = new System.Drawing.Point(45, 11);
            this.DelayText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DelayText.Name = "DelayText";
            this.DelayText.Size = new System.Drawing.Size(77, 13);
            this.DelayText.TabIndex = 3;
            this.DelayText.Text = "Delay Samples";
            // 
            // NumberOfStimuli
            // 
            this.NumberOfStimuli.Location = new System.Drawing.Point(126, 228);
            this.NumberOfStimuli.Margin = new System.Windows.Forms.Padding(2);
            this.NumberOfStimuli.Name = "NumberOfStimuli";
            this.NumberOfStimuli.Size = new System.Drawing.Size(30, 20);
            this.NumberOfStimuli.TabIndex = 7;
            this.NumberOfStimuli.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // CheckboxBiphasicSymmetrical
            // 
            this.CheckboxBiphasicSymmetrical.AutoSize = true;
            this.CheckboxBiphasicSymmetrical.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckboxBiphasicSymmetrical.Checked = true;
            this.CheckboxBiphasicSymmetrical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckboxBiphasicSymmetrical.Location = new System.Drawing.Point(6, 34);
            this.CheckboxBiphasicSymmetrical.Margin = new System.Windows.Forms.Padding(2);
            this.CheckboxBiphasicSymmetrical.Name = "CheckboxBiphasicSymmetrical";
            this.CheckboxBiphasicSymmetrical.Size = new System.Drawing.Size(117, 17);
            this.CheckboxBiphasicSymmetrical.TabIndex = 5;
            this.CheckboxBiphasicSymmetrical.TabStop = false;
            this.CheckboxBiphasicSymmetrical.Text = "Biphasic Symmetric";
            this.CheckboxBiphasicSymmetrical.UseVisualStyleBackColor = true;
            this.CheckboxBiphasicSymmetrical.CheckedChanged += new System.EventHandler(this.CheckboxBiphasicSymmetrical_CheckedChanged);
            // 
            // InterStimulusInterval
            // 
            this.InterStimulusInterval.Location = new System.Drawing.Point(126, 207);
            this.InterStimulusInterval.Margin = new System.Windows.Forms.Padding(2);
            this.InterStimulusInterval.Name = "InterStimulusInterval";
            this.InterStimulusInterval.Size = new System.Drawing.Size(30, 20);
            this.InterStimulusInterval.TabIndex = 6;
            this.InterStimulusInterval.TextChanged += new System.EventHandler(this.Samples_TextChanged);
            this.InterStimulusInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ParameterKeyPress);
            // 
            // InterStimulusIntervalText
            // 
            this.InterStimulusIntervalText.AutoSize = true;
            this.InterStimulusIntervalText.Location = new System.Drawing.Point(9, 211);
            this.InterStimulusIntervalText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InterStimulusIntervalText.Name = "InterStimulusIntervalText";
            this.InterStimulusIntervalText.Size = new System.Drawing.Size(113, 13);
            this.InterStimulusIntervalText.TabIndex = 9;
            this.InterStimulusIntervalText.Text = "Inter-Stimulus Samples";
            // 
            // TabPageChannelLayout
            // 
            this.TabPageChannelLayout.Controls.Add(this.PanelChannelLayout);
            this.TabPageChannelLayout.Location = new System.Drawing.Point(4, 22);
            this.TabPageChannelLayout.Name = "TabPageChannelLayout";
            this.TabPageChannelLayout.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageChannelLayout.Size = new System.Drawing.Size(241, 289);
            this.TabPageChannelLayout.TabIndex = 3;
            this.TabPageChannelLayout.Text = "Channel Layout";
            this.TabPageChannelLayout.UseVisualStyleBackColor = true;
            // 
            // PanelChannelLayout
            // 
            this.PanelChannelLayout.Controls.Add(this.CustomChannelLayout);
            this.PanelChannelLayout.Controls.Add(this.DefaultChannelLayout);
            this.PanelChannelLayout.Controls.Add(this.TextBoxChannelLayoutFilePath);
            this.PanelChannelLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelChannelLayout.Location = new System.Drawing.Point(3, 3);
            this.PanelChannelLayout.Name = "PanelChannelLayout";
            this.PanelChannelLayout.Size = new System.Drawing.Size(235, 283);
            this.PanelChannelLayout.TabIndex = 0;
            // 
            // CustomChannelLayout
            // 
            this.CustomChannelLayout.Location = new System.Drawing.Point(122, 85);
            this.CustomChannelLayout.Name = "CustomChannelLayout";
            this.CustomChannelLayout.Size = new System.Drawing.Size(86, 26);
            this.CustomChannelLayout.TabIndex = 2;
            this.CustomChannelLayout.Text = "Custom Layout";
            this.CustomChannelLayout.UseVisualStyleBackColor = true;
            // 
            // DefaultChannelLayout
            // 
            this.DefaultChannelLayout.Location = new System.Drawing.Point(19, 85);
            this.DefaultChannelLayout.Name = "DefaultChannelLayout";
            this.DefaultChannelLayout.Size = new System.Drawing.Size(86, 26);
            this.DefaultChannelLayout.TabIndex = 1;
            this.DefaultChannelLayout.Text = "Default Layout";
            this.DefaultChannelLayout.UseVisualStyleBackColor = true;
            // 
            // TextBoxChannelLayoutFilePath
            // 
            this.TextBoxChannelLayoutFilePath.Location = new System.Drawing.Point(16, 9);
            this.TextBoxChannelLayoutFilePath.Multiline = true;
            this.TextBoxChannelLayoutFilePath.Name = "TextBoxChannelLayoutFilePath";
            this.TextBoxChannelLayoutFilePath.ReadOnly = true;
            this.TextBoxChannelLayoutFilePath.Size = new System.Drawing.Size(203, 67);
            this.TextBoxChannelLayoutFilePath.TabIndex = 0;
            // 
            // TabPageEditorDialog
            // 
            this.TabPageEditorDialog.Controls.Add(this.PropertyGridStimulusSequence);
            this.TabPageEditorDialog.Location = new System.Drawing.Point(4, 22);
            this.TabPageEditorDialog.Margin = new System.Windows.Forms.Padding(2);
            this.TabPageEditorDialog.Name = "TabPageEditorDialog";
            this.TabPageEditorDialog.Padding = new System.Windows.Forms.Padding(2);
            this.TabPageEditorDialog.Size = new System.Drawing.Size(241, 289);
            this.TabPageEditorDialog.TabIndex = 2;
            this.TabPageEditorDialog.Text = "EditorDialog";
            this.TabPageEditorDialog.UseVisualStyleBackColor = true;
            // 
            // PropertyGridStimulusSequence
            // 
            this.PropertyGridStimulusSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGridStimulusSequence.Location = new System.Drawing.Point(2, 2);
            this.PropertyGridStimulusSequence.Margin = new System.Windows.Forms.Padding(2);
            this.PropertyGridStimulusSequence.Name = "PropertyGridStimulusSequence";
            this.PropertyGridStimulusSequence.Size = new System.Drawing.Size(237, 285);
            this.PropertyGridStimulusSequence.TabIndex = 4;
            this.PropertyGridStimulusSequence.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropertyGridStimulusSequence_PropertyValueChanged);
            // 
            // TabControlVisualization
            // 
            this.TabControlVisualization.Controls.Add(this.TabPageWaveform);
            this.TabControlVisualization.Controls.Add(this.TabPageTable);
            this.TabControlVisualization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControlVisualization.Location = new System.Drawing.Point(0, 0);
            this.TabControlVisualization.Margin = new System.Windows.Forms.Padding(2);
            this.TabControlVisualization.Name = "TabControlVisualization";
            this.TabControlVisualization.SelectedIndex = 0;
            this.TabControlVisualization.Size = new System.Drawing.Size(946, 551);
            this.TabControlVisualization.TabIndex = 6;
            // 
            // TabPageWaveform
            // 
            this.TabPageWaveform.Controls.Add(this.ZedGraphWaveform);
            this.TabPageWaveform.Location = new System.Drawing.Point(4, 22);
            this.TabPageWaveform.Margin = new System.Windows.Forms.Padding(2);
            this.TabPageWaveform.Name = "TabPageWaveform";
            this.TabPageWaveform.Padding = new System.Windows.Forms.Padding(2);
            this.TabPageWaveform.Size = new System.Drawing.Size(938, 525);
            this.TabPageWaveform.TabIndex = 0;
            this.TabPageWaveform.Text = "Stimulus Waveform";
            this.TabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // TabPageTable
            // 
            this.TabPageTable.Controls.Add(this.DataGridViewStimulusTable);
            this.TabPageTable.Location = new System.Drawing.Point(4, 22);
            this.TabPageTable.Margin = new System.Windows.Forms.Padding(2);
            this.TabPageTable.Name = "TabPageTable";
            this.TabPageTable.Padding = new System.Windows.Forms.Padding(2);
            this.TabPageTable.Size = new System.Drawing.Size(938, 525);
            this.TabPageTable.TabIndex = 1;
            this.TabPageTable.Text = "Table";
            this.TabPageTable.UseVisualStyleBackColor = true;
            // 
            // DataGridViewStimulusTable
            // 
            this.DataGridViewStimulusTable.AllowUserToAddRows = false;
            this.DataGridViewStimulusTable.AllowUserToDeleteRows = false;
            this.DataGridViewStimulusTable.AllowUserToOrderColumns = true;
            this.DataGridViewStimulusTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.DataGridViewStimulusTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewStimulusTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridViewStimulusTable.Location = new System.Drawing.Point(2, 2);
            this.DataGridViewStimulusTable.Margin = new System.Windows.Forms.Padding(2);
            this.DataGridViewStimulusTable.Name = "DataGridViewStimulusTable";
            this.DataGridViewStimulusTable.RowHeadersWidth = 62;
            this.DataGridViewStimulusTable.RowTemplate.Height = 28;
            this.DataGridViewStimulusTable.Size = new System.Drawing.Size(934, 521);
            this.DataGridViewStimulusTable.TabIndex = 0;
            this.DataGridViewStimulusTable.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewStimulusTable_CellEndEdit);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TabControlVisualization);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1198, 551);
            this.splitContainer1.SplitterDistance = 946;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.PanelButtons);
            this.splitContainer2.Size = new System.Drawing.Size(249, 551);
            this.splitContainer2.SplitterDistance = 520;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.ZedGraphChannels);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.TabControlParameters);
            this.splitContainer3.Panel2MinSize = 315;
            this.splitContainer3.Size = new System.Drawing.Size(249, 520);
            this.splitContainer3.SplitterDistance = 200;
            this.splitContainer3.SplitterWidth = 5;
            this.splitContainer3.TabIndex = 0;
            // 
            // ZedGraphChannels
            // 
            this.ZedGraphChannels.AutoSize = true;
            this.ZedGraphChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphChannels.Location = new System.Drawing.Point(0, 0);
            this.ZedGraphChannels.Name = "ZedGraphChannels";
            this.ZedGraphChannels.ScrollGrace = 0D;
            this.ZedGraphChannels.ScrollMaxX = 0D;
            this.ZedGraphChannels.ScrollMaxY = 0D;
            this.ZedGraphChannels.ScrollMaxY2 = 0D;
            this.ZedGraphChannels.ScrollMinX = 0D;
            this.ZedGraphChannels.ScrollMinY = 0D;
            this.ZedGraphChannels.ScrollMinY2 = 0D;
            this.ZedGraphChannels.Size = new System.Drawing.Size(249, 200);
            this.ZedGraphChannels.TabIndex = 3;
            this.ZedGraphChannels.UseExtendedPrintDialog = true;
            this.ZedGraphChannels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ZedGraphChannels_MouseClick);
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(1198, 573);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.LinkLabelDocumentation);
            this.Controls.Add(this.StatusStrip);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(105, 50);
            this.Name = "Rhs2116StimulusSequenceDialog";
            this.Text = "Rhs2116StimulusSequenceDialog";
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.PanelButtons.ResumeLayout(false);
            this.TabControlParameters.ResumeLayout(false);
            this.TabPageAddPulse.ResumeLayout(false);
            this.TabPageAddPulse.PerformLayout();
            this.PanelParameters.ResumeLayout(false);
            this.PanelParameters.PerformLayout();
            this.GroupBoxCathode.ResumeLayout(false);
            this.GroupBoxCathode.PerformLayout();
            this.GroupBoxAnode.ResumeLayout(false);
            this.GroupBoxAnode.PerformLayout();
            this.TabPageChannelLayout.ResumeLayout(false);
            this.PanelChannelLayout.ResumeLayout(false);
            this.PanelChannelLayout.PerformLayout();
            this.TabPageEditorDialog.ResumeLayout(false);
            this.TabControlVisualization.ResumeLayout(false);
            this.TabPageWaveform.ResumeLayout(false);
            this.TabPageTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewStimulusTable)).EndInit();
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
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusIsValid;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.LinkLabel LinkLabelDocumentation;
        private ZedGraph.ZedGraphControl ZedGraphWaveform;
        private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusSlotsUsed;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TabControl TabControlParameters;
        private System.Windows.Forms.TabPage TabPageAddPulse;
        private System.Windows.Forms.TabPage TabPageEditorDialog;
        private System.Windows.Forms.PropertyGrid PropertyGridStimulusSequence;
        private System.Windows.Forms.Label LabelAnplitudeAnodicText;
        private System.Windows.Forms.Label DelayText;
        private System.Windows.Forms.TextBox AmplitudeAnodic;
        private System.Windows.Forms.TextBox DelaySamples;
        private System.Windows.Forms.CheckBox CheckboxBiphasicSymmetrical;
        private System.Windows.Forms.Button ButtonAddPulses;
        private System.Windows.Forms.Label NumberOfPulsesText;
        private System.Windows.Forms.TextBox NumberOfStimuli;
        private System.Windows.Forms.Label InterStimulusIntervalText;
        private System.Windows.Forms.TextBox InterStimulusInterval;
        private System.Windows.Forms.Label LabelPulseWidthAnodicText;
        private System.Windows.Forms.TextBox PulseWidthAnodic;
        private System.Windows.Forms.TabControl TabControlVisualization;
        private System.Windows.Forms.TabPage TabPageWaveform;
        private System.Windows.Forms.TabPage TabPageTable;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private ZedGraph.ZedGraphControl ZedGraphChannels;
        private System.Windows.Forms.DataGridView DataGridViewStimulusTable;
        private System.Windows.Forms.Panel PanelParameters;
        private System.Windows.Forms.TextBox DelaySamplesConverted;
        private System.Windows.Forms.Label LabelStepSize;
        private System.Windows.Forms.CheckBox CheckBoxAnodicFirst;
        private System.Windows.Forms.TextBox AmplitudeAnodicConverted;
        private System.Windows.Forms.TextBox PulseWidthAnodicConverted;
        private System.Windows.Forms.TextBox PulseWidthCathodicConverted;
        private System.Windows.Forms.TextBox AmplitudeCathodicConverted;
        private System.Windows.Forms.Label LabelAmplitudeCathodic;
        private System.Windows.Forms.Label LabelPulseWidthCathodicText;
        private System.Windows.Forms.TextBox PulseWidthCathodic;
        private System.Windows.Forms.TextBox AmplitudeCathodic;
        private System.Windows.Forms.TextBox InterPulseIntervalConverted;
        private System.Windows.Forms.TextBox InterPulseInterval;
        private System.Windows.Forms.Label InterPulseIntervalText;
        private System.Windows.Forms.TextBox InterStimulusIntervalConverted;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox ComboBoxStepSize;
        private System.Windows.Forms.GroupBox GroupBoxCathode;
        private System.Windows.Forms.GroupBox GroupBoxAnode;
        private System.Windows.Forms.TabPage TabPageChannelLayout;
        private System.Windows.Forms.Panel PanelChannelLayout;
        private System.Windows.Forms.TextBox TextBoxChannelLayoutFilePath;
        private System.Windows.Forms.Button CustomChannelLayout;
        private System.Windows.Forms.Button DefaultChannelLayout;
    }
}
