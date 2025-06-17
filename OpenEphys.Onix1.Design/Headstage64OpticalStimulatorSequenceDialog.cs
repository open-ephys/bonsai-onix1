using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Headstage64OpticalStimulatorSequence"/>.
    /// </summary>
    public partial class Headstage64OpticalStimulatorSequenceDialog : GenericStimulusSequenceDialog
    {
        private readonly static int NumberOfChannels = 2;

        internal Headstage64OpticalStimulatorSequence Sequence;
        private readonly Headstage64OpticalStimulatorOptions StimulusSequenceOptions;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="stimulusSequence">Existing stimulus sequence.</param>
        public Headstage64OpticalStimulatorSequenceDialog(Headstage64OpticalStimulatorSequence stimulusSequence)
            : base(NumberOfChannels, false)
        {
            InitializeComponent();

            Sequence = new(stimulusSequence);

            StimulusSequenceOptions = new()
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };
            groupBoxDefineStimuli.Controls.Add(StimulusSequenceOptions);

            // NB: Initialize current parameters
            StimulusSequenceOptions.textBoxMaxCurrent.Text = Sequence.MaxCurrent.ToString();
            StimulusSequenceOptions.textBoxPulseDuration.Text = Sequence.PulseDuration.ToString();
            StimulusSequenceOptions.textBoxPulsePeriod.Text = Sequence.PulsePeriod.ToString();

            StimulusSequenceOptions.textBoxChannelOnePercent.Text = Sequence.ChannelOnePercent.ToString();
            StimulusSequenceOptions.trackBarChannelOnePercent.Value = (int)Sequence.ChannelOnePercent;
            StimulusSequenceOptions.textBoxChannelTwoPercent.Text = Sequence.ChannelTwoPercent.ToString();
            StimulusSequenceOptions.trackBarChannelTwoPercent.Value = (int)Sequence.ChannelTwoPercent;

            StimulusSequenceOptions.textBoxPulsesPerBurst.Text = Sequence.PulsesPerBurst.ToString();
            StimulusSequenceOptions.textBoxInterBurstInterval.Text = Sequence.InterBurstInterval.ToString();
            StimulusSequenceOptions.textBoxBurstsPerTrain.Text = Sequence.BurstsPerTrain.ToString();
            StimulusSequenceOptions.textBoxDelay.Text = Sequence.Delay.ToString();

            // NB: Add event handlers
            StimulusSequenceOptions.textBoxMaxCurrent.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxMaxCurrent.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxPulseDuration.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxPulseDuration.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxPulsePeriod.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxPulsePeriod.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxChannelOnePercent.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxChannelOnePercent.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxChannelOnePercent.TextChanged += TextBoxChanged;
            StimulusSequenceOptions.textBoxChannelTwoPercent.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxChannelTwoPercent.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxChannelTwoPercent.TextChanged += TextBoxChanged;

            StimulusSequenceOptions.textBoxPulsesPerBurst.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxPulsesPerBurst.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxInterBurstInterval.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxInterBurstInterval.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxBurstsPerTrain.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxBurstsPerTrain.KeyPress += KeyPressed;
            StimulusSequenceOptions.textBoxDelay.Leave += TextBoxChanged;
            StimulusSequenceOptions.textBoxDelay.KeyPress += KeyPressed;

            StimulusSequenceOptions.Show();

            toolStripStatusIsValid.BorderSides = ToolStripStatusLabelBorderSides.None;

            DisableVerticalZoom();

            DrawStimulusWaveform();
        }

        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (sender is TextBox tb)
                {
                    if (tb.Name == StimulusSequenceOptions.textBoxMaxCurrent.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxPulseDuration.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxPulsePeriod.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxChannelOnePercent.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxChannelTwoPercent.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxPulsesPerBurst.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxInterBurstInterval.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxBurstsPerTrain.Name ||
                        tb.Name == StimulusSequenceOptions.textBoxDelay.Name)
                        TextBoxChanged(sender, null);
                }
            }
        }

        private void TextBoxChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == StimulusSequenceOptions.textBoxMaxCurrent.Name)
                {
                    Sequence.MaxCurrent = double.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.MaxCurrent.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxPulseDuration.Name)
                {
                    Sequence.PulseDuration = double.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.PulseDuration.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxPulsePeriod.Name)
                {
                    Sequence.PulsePeriod= double.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.PulsePeriod.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxChannelOnePercent.Name)
                {
                    Sequence.ChannelOnePercent = double.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.ChannelOnePercent.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxChannelTwoPercent.Name)
                {
                    Sequence.ChannelTwoPercent = double.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.ChannelTwoPercent.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxPulsesPerBurst.Name)
                {
                    Sequence.PulsesPerBurst = uint.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.PulsesPerBurst.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxInterBurstInterval.Name)
                {
                    Sequence.InterBurstInterval = uint.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.InterBurstInterval.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxBurstsPerTrain.Name)
                {
                    Sequence.BurstsPerTrain = uint.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.BurstsPerTrain.ToString();
                }
                else if (textBox.Name == StimulusSequenceOptions.textBoxDelay.Name)
                {
                    Sequence.Delay = uint.TryParse(textBox.Text, out var result) ? result : 0;
                    textBox.Text = Sequence.Delay.ToString();
                }
            }

            DrawStimulusWaveform();
        }

        private double GetChannelCurrent(double maxCurrent, double channelPercent)
        {
            return maxCurrent * channelPercent;
        }

        private double GetChannelCurrentScaled(double maxCurrent, double channelPercent, double scale)
        {
            return (GetChannelCurrent(maxCurrent, channelPercent) / 100.0) / scale;
        }

        internal override PointPairList[] CreateStimulusWaveforms()
        {
            PointPairList[] waveforms = new PointPairList[NumberOfChannels];

            PeakToPeak = Sequence.MaxCurrent * ChannelScale;

            for (int channel = 0; channel < NumberOfChannels; channel++)
            {
                double offset = channel;

                waveforms[channel] = new PointPairList
                {
                    new PointPairList { new PointPair(0, offset), new PointPair(Sequence.Delay, offset) }
                };

                var stimulusCurrent = offset + GetChannelCurrentScaled(Sequence.MaxCurrent, 
                                                                       channel == 0 ? Sequence.ChannelOnePercent : Sequence.ChannelTwoPercent,
                                                                       PeakToPeak);

                for (int i = 0; i < Sequence.BurstsPerTrain; i++)
                {
                    for (int j = 0; j < Sequence.PulsesPerBurst; j++)
                    {
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, stimulusCurrent));
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.PulseDuration, stimulusCurrent));
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, offset));

                        if (j != Sequence.PulsesPerBurst - 1)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.PulsePeriod - Sequence.PulseDuration, offset));
                        }
                    }

                    if (i != Sequence.BurstsPerTrain - 1)
                    {
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.InterBurstInterval, offset));
                    }
                }
            }

            YAxisMax = NumberOfChannels;

            return waveforms;
        }

        internal override void HighlightInvalidChannels()
        {
            if (!IsSequenceValid())
            {
                foreach (var waveform in GetWaveformCurves())
                {
                    waveform.Color = Color.Red;
                }
            }
        }

        internal override bool IsSequenceValid()
        {
            return IsSequenceValid(Sequence, out string _);
        }

        private static bool IsSequenceValid(Headstage64OpticalStimulatorSequence sequence, out string reason)
        {
            reason = string.Empty;

            if (sequence.MaxCurrent < 0 || sequence.MaxCurrent > 300)
            {
                reason = "Maximum current is invalid.";
                return false;
            }
            else if (sequence.PulsePeriod <= sequence.PulseDuration)
            {
                reason = "Pulse duration is too short compared to the pulse period.";
                return false;
            }

            return true;
        }

        internal override void SetStatusValidity()
        {
            if (IsSequenceValid(Sequence, out string reason))
            {
                toolStripStatusIsValid.Image = Properties.Resources.StatusReadyImage;
                toolStripStatusIsValid.Text = "Valid stimulus sequence";
            }
            else
            {
                toolStripStatusIsValid.Image = Properties.Resources.StatusBlockedImage;
                toolStripStatusIsValid.Text = "Warning: " + reason;
            }
        }

        internal override void SerializeStimulusSequence(string fileName)
        {
            DesignHelper.SerializeObject(Sequence, fileName);
        }

        internal override void DeserializeStimulusSequence(string fileName)
        {
            var sequence = DesignHelper.DeserializeString<Headstage64OpticalStimulatorSequence>(File.ReadAllText(fileName));

            if (sequence != null)
            {
                if (sequence == new Headstage64OpticalStimulatorSequence())
                {
                    var result = MessageBox.Show("The stimulus sequence loaded does not have any configuration settings applied. " +
                        "This could be because the file did not have the correct format. If this sequence is loaded, it will clear out " +
                        "all current settings. Continue?", "No Settings Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                Sequence = sequence;

                if (!IsSequenceValid(Sequence, out string reason))
                {
                    MessageBox.Show($"Warning: The stimulus sequence loaded is invalid ({reason}). Check all values to ensure they are correct.",
                        "Invalid Stimuli", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Incoming file is not valid. Check file for validity.");
            }
        }

        internal override bool CanCloseForm(out DialogResult result)
        {
            if (Sequence != null)
            {
                if (!IsSequenceValid(Sequence, out string reason))
                {
                    DialogResult resultContinue = MessageBox.Show($"Warning: Stimulus sequence is not valid ({reason}). " +
                        "If you continue, the current settings will be discarded. " +
                        "Press OK to discard changes, or press Cancel to continue editing the sequence.", "Invalid Sequence",
                        MessageBoxButtons.OKCancel);

                    if (resultContinue == DialogResult.OK)
                    {
                        result = DialogResult.Cancel;
                        return true;
                    }
                    else
                    {
                        result = DialogResult.OK;
                        return false;
                    }
                }
                else
                {
                    result = DialogResult.OK;
                    return true;
                }
            }
            else
            {
                result = DialogResult.Cancel;
                return true;
            }
        }
    }
}
