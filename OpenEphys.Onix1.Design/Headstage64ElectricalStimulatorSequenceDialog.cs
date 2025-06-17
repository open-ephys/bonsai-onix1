using System;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.Drawing;
using System.IO;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Headstage64ElectricalStimulatorSequence"/>.
    /// </summary>
    public partial class Headstage64ElectricalStimulatorSequenceDialog : GenericStimulusSequenceDialog
    {
        internal Headstage64ElectricalStimulatorSequence Sequence;
        private readonly Headstage64ElectricalStimulatorOptions StimulusSequenceOptions;

        private readonly static int NumberOfChannels = 1;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="stimulusSequence">Existing stimulus sequence.</param>
        public Headstage64ElectricalStimulatorSequenceDialog(Headstage64ElectricalStimulatorSequence stimulusSequence)
            : base(NumberOfChannels, false)
        {
            InitializeComponent();

            Sequence = new Headstage64ElectricalStimulatorSequence(stimulusSequence);

            StimulusSequenceOptions = new()
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };
            groupBoxDefineStimuli.Controls.Add(StimulusSequenceOptions);

            // NB: Initialize current parameters
            StimulusSequenceOptions.textBoxPhaseOneCurrent.Text = Sequence.PhaseOneCurrent.ToString();
            StimulusSequenceOptions.textBoxPhaseOneDuration.Text = Sequence.PhaseOneDuration.ToString();

            StimulusSequenceOptions.textBoxInterPhaseCurrent.Text = Sequence.InterPhaseCurrent.ToString();
            StimulusSequenceOptions.textBoxInterPhaseDuration.Text = Sequence.InterPhaseInterval.ToString();

            StimulusSequenceOptions.textBoxPhaseTwoCurrent.Text = Sequence.PhaseTwoCurrent.ToString();
            StimulusSequenceOptions.textBoxPhaseTwoDuration.Text = Sequence.PhaseTwoDuration.ToString();

            StimulusSequenceOptions.textBoxPulsePeriod.Text = Sequence.InterPulseInterval.ToString();
            StimulusSequenceOptions.textBoxBurstPulseCount.Text = Sequence.BurstPulseCount.ToString();
            StimulusSequenceOptions.textBoxInterBurstInterval.Text = Sequence.InterBurstInterval.ToString();
            StimulusSequenceOptions.textBoxTrainBurstCount.Text = Sequence.TrainBurstCount.ToString();
            StimulusSequenceOptions.textBoxTrainDelay.Text = Sequence.TriggerDelay.ToString();

            // NB: Add event handlers
            StimulusSequenceOptions.textBoxPhaseOneCurrent.Leave += CurrentValueChanged;
            StimulusSequenceOptions.textBoxPhaseOneCurrent.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxPhaseOneDuration.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxPhaseOneDuration.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxInterPhaseCurrent.Leave += CurrentValueChanged;
            StimulusSequenceOptions.textBoxInterPhaseCurrent.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxInterPhaseDuration.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxInterPhaseDuration.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxPhaseTwoCurrent.Leave += CurrentValueChanged;
            StimulusSequenceOptions.textBoxPhaseTwoCurrent.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxPhaseTwoDuration.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxPhaseTwoDuration.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxPulsePeriod.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxPulsePeriod.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxBurstPulseCount.Leave += CountValueChanged;
            StimulusSequenceOptions.textBoxBurstPulseCount.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxInterBurstInterval.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxInterBurstInterval.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxTrainBurstCount.Leave += CountValueChanged;
            StimulusSequenceOptions.textBoxTrainBurstCount.KeyPress += KeyPressed;

            StimulusSequenceOptions.textBoxTrainDelay.Leave += TimeValueChanged;
            StimulusSequenceOptions.textBoxTrainDelay.KeyPress += KeyPressed;

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
                    if (tb.Name == StimulusSequenceOptions.textBoxPhaseOneCurrent.Name)
                        CurrentValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxPhaseOneDuration.Name)
                        TimeValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxInterPhaseCurrent.Name)
                        CurrentValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxInterPhaseDuration.Name)
                        TimeValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxPhaseTwoCurrent.Name)
                        CurrentValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxPhaseTwoDuration.Name)
                        TimeValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxPulsePeriod.Name)
                        TimeValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxBurstPulseCount.Name)
                        CountValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxInterBurstInterval.Name)
                        TimeValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxTrainBurstCount.Name)
                        CountValueChanged(sender, null);
                    else if (tb.Name == StimulusSequenceOptions.textBoxTrainDelay.Name)
                        TimeValueChanged(sender, null);
                }
            }
        }

        private void CountValueChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Name == StimulusSequenceOptions.textBoxBurstPulseCount.Name)
                {
                    Sequence.BurstPulseCount = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.BurstPulseCount.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxTrainBurstCount.Name)
                {
                    Sequence.TrainBurstCount = uint.TryParse(tb.Text,out uint result) ? result : 0;
                    tb.Text = Sequence.TrainBurstCount.ToString();
                }
            }

            DrawStimulusWaveform();
        }

        private void TimeValueChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Name == StimulusSequenceOptions.textBoxPhaseOneDuration.Name)
                {
                    Sequence.PhaseOneDuration = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.PhaseOneDuration.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxInterPhaseDuration.Name)
                {
                    Sequence.InterPhaseInterval = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.InterPhaseInterval.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxPhaseTwoDuration.Name)
                {
                    Sequence.PhaseTwoDuration = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.PhaseTwoDuration.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxPulsePeriod.Name)
                {
                    Sequence.InterPulseInterval = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.InterPulseInterval.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxInterBurstInterval.Name)
                {
                    Sequence.InterBurstInterval = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.InterBurstInterval.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxTrainDelay.Name)
                {
                    Sequence.TriggerDelay = uint.TryParse(tb.Text, out uint result) ? result : 0;
                    tb.Text = Sequence.TriggerDelay.ToString();
                }
            }

            DrawStimulusWaveform();
        }

        private void CurrentValueChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Name == StimulusSequenceOptions.textBoxPhaseOneCurrent.Name)
                {
                    Sequence.PhaseOneCurrent = double.TryParse(tb.Text, out double result) ? result : 0;
                    tb.Text = Sequence.PhaseOneCurrent.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxInterPhaseCurrent.Name)
                {
                    Sequence.InterPhaseCurrent = double.TryParse(tb.Text, out double result) ? result : 0;
                    tb.Text = Sequence.InterPhaseCurrent.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.textBoxPhaseTwoCurrent.Name)
                {
                    Sequence.PhaseTwoCurrent = double.TryParse(tb.Text, out double result) ? result : 0;
                    tb.Text = Sequence.PhaseTwoCurrent.ToString();
                }
            }

            DrawStimulusWaveform();
        }

        internal override bool IsSequenceValid()
        {
            return IsSequenceValid(Sequence, out string _);
        }

        private static bool IsSequenceValid(Headstage64ElectricalStimulatorSequence sequence, out string reason)
        {
            reason = string.Empty;

            if (sequence == null)
            {
                reason = "Sequence is null";
                return false;
            }

            static bool AnyCurrentIsSet(Headstage64ElectricalStimulatorSequence sequence)
            {
                return sequence.PhaseOneCurrent != 0 || sequence.InterPhaseCurrent != 0 || sequence.PhaseTwoCurrent != 0;
            }

            if (sequence.PhaseOneCurrent != 0 && sequence.PhaseOneDuration == 0)
            {
                reason = "Phase one current is greater than zero, but duration is zero.";
                return false;
            }
            else if (sequence.InterPhaseCurrent != 0 && sequence.InterPhaseInterval == 0)
            {
                reason = "Inter-pulse interval current is greater than zero, but duration is zero.";
                return false;
            }
            else if (sequence.PhaseTwoCurrent != 0 && sequence.PhaseTwoDuration == 0)
            {
                reason = "Phase two current is greater than zero, but duration is zero.";
                return false;
            }
            else if (AnyCurrentIsSet(sequence) && sequence.InterPulseInterval == 0)
            {
                reason = "Pulse period has not been set.";
                return false;
            }
            else if (AnyCurrentIsSet(sequence) && sequence.InterPulseInterval < sequence.PhaseOneDuration + sequence.InterPhaseInterval + sequence.PhaseTwoDuration)
            {
                reason = "Pulse period is too short.";
                return false;
            }
            else if (AnyCurrentIsSet(sequence) && sequence.InterPulseInterval != 0 && sequence.BurstPulseCount == 0)
            {
                reason = "Burst pulse count has not been set.";
                return false;
            }
            else if (AnyCurrentIsSet(sequence) && sequence.InterPulseInterval != 0 && sequence.BurstPulseCount != 0 && sequence.TrainBurstCount == 0)
            {
                reason = "Train burst count has not been set.";
                return false;
            }

            return true;
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

        internal override PointPairList[] CreateStimulusWaveforms()
        {
            PointPairList[] waveforms = new PointPairList[NumberOfChannels];

            PeakToPeak = (Math.Abs(Sequence.PhaseOneCurrent) + Math.Abs(Sequence.PhaseTwoCurrent)) * ChannelScale;

            if (Sequence != null)
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    waveforms[channel] = new PointPairList { new PointPair(0, 0), new PointPair(Sequence.TriggerDelay, 0) };

                    for (int i = 0; i < Sequence.TrainBurstCount; i++)
                    {
                        for (int j = 0; j < Sequence.BurstPulseCount; j++)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, Sequence.PhaseOneCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.PhaseOneDuration, Sequence.PhaseOneCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, Sequence.InterPhaseCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.InterPhaseInterval, Sequence.InterPhaseCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, Sequence.PhaseTwoCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.PhaseTwoDuration, Sequence.PhaseTwoCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, 0));

                            if (j != Sequence.BurstPulseCount - 1)
                            {
                                waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.InterPulseInterval - (Sequence.PhaseOneDuration + Sequence.InterPhaseInterval + Sequence.PhaseTwoDuration), 0));
                            }
                        }

                        if (i != Sequence.TrainBurstCount - 1)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + Sequence.InterBurstInterval, 0));
                        }
                    }
                }
            }

            return waveforms;
        }

        internal override void HighlightInvalidChannels()
        {
            if (!IsSequenceValid(Sequence, out string _))
            {
                foreach (var waveform in GetWaveformCurves())
                {
                    waveform.Color = Color.Red;
                }
            }
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
            var sequence = DesignHelper.DeserializeString<Headstage64ElectricalStimulatorSequence>(File.ReadAllText(fileName));

            if (sequence != null)
            {
                if (sequence == new Headstage64ElectricalStimulatorSequence())
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
    }
}
