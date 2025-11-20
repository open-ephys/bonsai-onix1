namespace OpenEphys.Onix1.Design
{
    partial class Headstage64OpticalStimulatorOptions
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
            this.textBoxBurstsPerTrain = new System.Windows.Forms.TextBox();
            this.labelBurstsPerTrain = new System.Windows.Forms.Label();
            this.textBoxInterBurstInterval = new System.Windows.Forms.TextBox();
            this.labelInterBurstInterval = new System.Windows.Forms.Label();
            this.textBoxPulsesPerBurst = new System.Windows.Forms.TextBox();
            this.labelPulsesPerBurst = new System.Windows.Forms.Label();
            this.labelMaxCurrent = new System.Windows.Forms.Label();
            this.textBoxMaxCurrent = new System.Windows.Forms.TextBox();
            this.textBoxChannelOnePercent = new System.Windows.Forms.TextBox();
            this.labelChannelOnePercent = new System.Windows.Forms.Label();
            this.trackBarChannelOnePercent = new System.Windows.Forms.TrackBar();
            this.textBoxPulseDuration = new System.Windows.Forms.TextBox();
            this.labelPulseDuration = new System.Windows.Forms.Label();
            this.trackBarChannelTwoPercent = new System.Windows.Forms.TrackBar();
            this.textBoxChannelTwoPercent = new System.Windows.Forms.TextBox();
            this.labelChannelTwoPercent = new System.Windows.Forms.Label();
            this.textBoxPulseFrequencyHz = new System.Windows.Forms.TextBox();
            this.labelPulseFrequencyHz = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarChannelOnePercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarChannelTwoPercent)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxBurstsPerTrain
            // 
            this.textBoxBurstsPerTrain.Location = new System.Drawing.Point(100, 198);
            this.textBoxBurstsPerTrain.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxBurstsPerTrain.Name = "textBoxBurstsPerTrain";
            this.textBoxBurstsPerTrain.Size = new System.Drawing.Size(57, 20);
            this.textBoxBurstsPerTrain.TabIndex = 17;
            this.textBoxBurstsPerTrain.TextChanged += new System.EventHandler(this.BurstsPerTrainChanged);
            // 
            // labelBurstsPerTrain
            // 
            this.labelBurstsPerTrain.AutoSize = true;
            this.labelBurstsPerTrain.Location = new System.Drawing.Point(4, 201);
            this.labelBurstsPerTrain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelBurstsPerTrain.Name = "labelBurstsPerTrain";
            this.labelBurstsPerTrain.Size = new System.Drawing.Size(82, 13);
            this.labelBurstsPerTrain.TabIndex = 16;
            this.labelBurstsPerTrain.Text = "Bursts Per Train";
            // 
            // textBoxInterBurstInterval
            // 
            this.textBoxInterBurstInterval.Location = new System.Drawing.Point(267, 158);
            this.textBoxInterBurstInterval.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxInterBurstInterval.Name = "textBoxInterBurstInterval";
            this.textBoxInterBurstInterval.Size = new System.Drawing.Size(57, 20);
            this.textBoxInterBurstInterval.TabIndex = 15;
            // 
            // labelInterBurstInterval
            // 
            this.labelInterBurstInterval.Location = new System.Drawing.Point(162, 161);
            this.labelInterBurstInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelInterBurstInterval.Name = "labelInterBurstInterval";
            this.labelInterBurstInterval.Size = new System.Drawing.Size(110, 15);
            this.labelInterBurstInterval.TabIndex = 14;
            this.labelInterBurstInterval.Text = "Inter-Burst Interval [ms]";
            // 
            // textBoxPulsesPerBurst
            // 
            this.textBoxPulsesPerBurst.Location = new System.Drawing.Point(100, 158);
            this.textBoxPulsesPerBurst.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPulsesPerBurst.Name = "textBoxPulsesPerBurst";
            this.textBoxPulsesPerBurst.Size = new System.Drawing.Size(57, 20);
            this.textBoxPulsesPerBurst.TabIndex = 13;
            this.textBoxPulsesPerBurst.TextChanged += new System.EventHandler(this.PulsesPerBurstChanged);
            // 
            // labelPulsesPerBurst
            // 
            this.labelPulsesPerBurst.AutoSize = true;
            this.labelPulsesPerBurst.Location = new System.Drawing.Point(4, 161);
            this.labelPulsesPerBurst.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPulsesPerBurst.Name = "labelPulsesPerBurst";
            this.labelPulsesPerBurst.Size = new System.Drawing.Size(84, 13);
            this.labelPulsesPerBurst.TabIndex = 12;
            this.labelPulsesPerBurst.Text = "Pulses Per Burst";
            // 
            // labelMaxCurrent
            // 
            this.labelMaxCurrent.AutoSize = true;
            this.labelMaxCurrent.Location = new System.Drawing.Point(4, 16);
            this.labelMaxCurrent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMaxCurrent.Name = "labelMaxCurrent";
            this.labelMaxCurrent.Size = new System.Drawing.Size(88, 13);
            this.labelMaxCurrent.TabIndex = 0;
            this.labelMaxCurrent.Text = "Max Current [mA]";
            // 
            // textBoxMaxCurrent
            // 
            this.textBoxMaxCurrent.Location = new System.Drawing.Point(100, 14);
            this.textBoxMaxCurrent.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxMaxCurrent.Name = "textBoxMaxCurrent";
            this.textBoxMaxCurrent.Size = new System.Drawing.Size(57, 20);
            this.textBoxMaxCurrent.TabIndex = 1;
            // 
            // textBoxChannelOnePercent
            // 
            this.textBoxChannelOnePercent.Location = new System.Drawing.Point(100, 47);
            this.textBoxChannelOnePercent.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxChannelOnePercent.Name = "textBoxChannelOnePercent";
            this.textBoxChannelOnePercent.Size = new System.Drawing.Size(57, 20);
            this.textBoxChannelOnePercent.TabIndex = 3;
            // 
            // labelChannelOnePercent
            // 
            this.labelChannelOnePercent.AutoSize = true;
            this.labelChannelOnePercent.Location = new System.Drawing.Point(4, 50);
            this.labelChannelOnePercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelChannelOnePercent.Name = "labelChannelOnePercent";
            this.labelChannelOnePercent.Size = new System.Drawing.Size(86, 13);
            this.labelChannelOnePercent.TabIndex = 2;
            this.labelChannelOnePercent.Text = "Channel One [%]";
            // 
            // trackBarChannelOnePercent
            // 
            this.trackBarChannelOnePercent.LargeChange = 125;
            this.trackBarChannelOnePercent.Location = new System.Drawing.Point(8, 67);
            this.trackBarChannelOnePercent.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarChannelOnePercent.Maximum = 1000;
            this.trackBarChannelOnePercent.Name = "trackBarChannelOnePercent";
            this.trackBarChannelOnePercent.Size = new System.Drawing.Size(148, 45);
            this.trackBarChannelOnePercent.SmallChange = 125;
            this.trackBarChannelOnePercent.TabIndex = 6;
            this.trackBarChannelOnePercent.TickFrequency = 125;
            this.trackBarChannelOnePercent.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarChannelOnePercent.Value = 1000;
            // 
            // textBoxPulseDuration
            // 
            this.textBoxPulseDuration.Location = new System.Drawing.Point(100, 119);
            this.textBoxPulseDuration.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPulseDuration.Name = "textBoxPulseDuration";
            this.textBoxPulseDuration.Size = new System.Drawing.Size(57, 20);
            this.textBoxPulseDuration.TabIndex = 9;
            // 
            // labelPulseDuration
            // 
            this.labelPulseDuration.AutoSize = true;
            this.labelPulseDuration.Location = new System.Drawing.Point(4, 121);
            this.labelPulseDuration.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPulseDuration.Name = "labelPulseDuration";
            this.labelPulseDuration.Size = new System.Drawing.Size(98, 13);
            this.labelPulseDuration.TabIndex = 8;
            this.labelPulseDuration.Text = "Pulse Duration [ms]";
            // 
            // trackBarChannelTwoPercent
            // 
            this.trackBarChannelTwoPercent.LargeChange = 125;
            this.trackBarChannelTwoPercent.Location = new System.Drawing.Point(164, 67);
            this.trackBarChannelTwoPercent.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarChannelTwoPercent.Maximum = 1000;
            this.trackBarChannelTwoPercent.Name = "trackBarChannelTwoPercent";
            this.trackBarChannelTwoPercent.Size = new System.Drawing.Size(159, 45);
            this.trackBarChannelTwoPercent.SmallChange = 125;
            this.trackBarChannelTwoPercent.TabIndex = 7;
            this.trackBarChannelTwoPercent.TickFrequency = 125;
            this.trackBarChannelTwoPercent.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // textBoxChannelTwoPercent
            // 
            this.textBoxChannelTwoPercent.Location = new System.Drawing.Point(267, 47);
            this.textBoxChannelTwoPercent.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxChannelTwoPercent.Name = "textBoxChannelTwoPercent";
            this.textBoxChannelTwoPercent.Size = new System.Drawing.Size(57, 20);
            this.textBoxChannelTwoPercent.TabIndex = 5;
            // 
            // labelChannelTwoPercent
            // 
            this.labelChannelTwoPercent.AutoSize = true;
            this.labelChannelTwoPercent.Location = new System.Drawing.Point(162, 50);
            this.labelChannelTwoPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelChannelTwoPercent.Name = "labelChannelTwoPercent";
            this.labelChannelTwoPercent.Size = new System.Drawing.Size(87, 13);
            this.labelChannelTwoPercent.TabIndex = 4;
            this.labelChannelTwoPercent.Text = "Channel Two [%]";
            // 
            // textBoxPulseFrequencyHz
            // 
            this.textBoxPulseFrequencyHz.Location = new System.Drawing.Point(267, 119);
            this.textBoxPulseFrequencyHz.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPulseFrequencyHz.Name = "textBoxPulseFrequencyHz";
            this.textBoxPulseFrequencyHz.Size = new System.Drawing.Size(57, 20);
            this.textBoxPulseFrequencyHz.TabIndex = 11;
            // 
            // labelPulseFrequencyHz
            // 
            this.labelPulseFrequencyHz.AutoSize = true;
            this.labelPulseFrequencyHz.Location = new System.Drawing.Point(162, 121);
            this.labelPulseFrequencyHz.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPulseFrequencyHz.Name = "labelPulseFrequencyHz";
            this.labelPulseFrequencyHz.Size = new System.Drawing.Size(82, 13);
            this.labelPulseFrequencyHz.TabIndex = 10;
            this.labelPulseFrequencyHz.Text = "Pulse Freq. [Hz]";
            // 
            // Headstage64OpticalStimulatorOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(328, 235);
            this.Controls.Add(this.textBoxPulseFrequencyHz);
            this.Controls.Add(this.labelPulseFrequencyHz);
            this.Controls.Add(this.trackBarChannelTwoPercent);
            this.Controls.Add(this.textBoxChannelTwoPercent);
            this.Controls.Add(this.labelChannelTwoPercent);
            this.Controls.Add(this.textBoxPulseDuration);
            this.Controls.Add(this.labelPulseDuration);
            this.Controls.Add(this.trackBarChannelOnePercent);
            this.Controls.Add(this.textBoxChannelOnePercent);
            this.Controls.Add(this.labelChannelOnePercent);
            this.Controls.Add(this.textBoxMaxCurrent);
            this.Controls.Add(this.labelMaxCurrent);
            this.Controls.Add(this.textBoxBurstsPerTrain);
            this.Controls.Add(this.labelBurstsPerTrain);
            this.Controls.Add(this.textBoxInterBurstInterval);
            this.Controls.Add(this.labelInterBurstInterval);
            this.Controls.Add(this.textBoxPulsesPerBurst);
            this.Controls.Add(this.labelPulsesPerBurst);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Headstage64OpticalStimulatorOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Headstage64OpticalStimulatorOptions";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarChannelOnePercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarChannelTwoPercent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.TextBox textBoxBurstsPerTrain;
        private System.Windows.Forms.Label labelBurstsPerTrain;
        internal System.Windows.Forms.TextBox textBoxInterBurstInterval;
        private System.Windows.Forms.Label labelInterBurstInterval;
        internal System.Windows.Forms.TextBox textBoxPulsesPerBurst;
        private System.Windows.Forms.Label labelPulsesPerBurst;
        private System.Windows.Forms.Label labelMaxCurrent;
        internal System.Windows.Forms.TextBox textBoxMaxCurrent;
        internal System.Windows.Forms.TextBox textBoxChannelOnePercent;
        private System.Windows.Forms.Label labelChannelOnePercent;
        internal System.Windows.Forms.TextBox textBoxPulseDuration;
        private System.Windows.Forms.Label labelPulseDuration;
        internal System.Windows.Forms.TextBox textBoxChannelTwoPercent;
        private System.Windows.Forms.Label labelChannelTwoPercent;
        internal System.Windows.Forms.TrackBar trackBarChannelOnePercent;
        internal System.Windows.Forms.TrackBar trackBarChannelTwoPercent;
        internal System.Windows.Forms.TextBox textBoxPulseFrequencyHz;
        private System.Windows.Forms.Label labelPulseFrequencyHz;
    }
}
