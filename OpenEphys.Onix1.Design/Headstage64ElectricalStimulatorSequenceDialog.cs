using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="ConfigureHeadstage64ElectricalStimulator"/>.
    /// </summary>
    public partial class Headstage64ElectricalStimulatorSequenceDialog : GenericStimulusSequenceDialog
    {
        internal readonly ConfigureHeadstage64ElectricalStimulator ElectricalStimulator;
        readonly Headstage64ElectricalStimulatorOptions StimulusSequenceOptions;

        readonly static int NumberOfChannels = 1;

        readonly Dictionary<TextBox, TextBoxBinding<double>> currentBindings;
        readonly Dictionary<TextBox, TextBoxBinding<uint>> timeBindings;
        readonly Dictionary<TextBox, TextBoxBinding<uint>> countBindings;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="electricalStimulator">Existing stimulus sequence.</param>
        public Headstage64ElectricalStimulatorSequenceDialog(ConfigureHeadstage64ElectricalStimulator electricalStimulator)
            : base(NumberOfChannels, false)
        {
            InitializeComponent();
            HideMenuStrip();

            ElectricalStimulator = new(electricalStimulator);

            StimulusSequenceOptions = new(ElectricalStimulator);
            StimulusSequenceOptions.SetChildFormProperties(this);
            groupBoxDefineStimuli.Controls.Add(StimulusSequenceOptions);

            currentBindings = new()
            {
                { StimulusSequenceOptions.textBoxPhaseOneCurrent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxPhaseOneCurrent,
                        value => { ElectricalStimulator.PhaseOneCurrent = value; return ElectricalStimulator.PhaseOneCurrent; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxInterPhaseCurrent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxInterPhaseCurrent,
                        value => { ElectricalStimulator.InterPhaseCurrent = value; return ElectricalStimulator.InterPhaseCurrent; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxPhaseTwoCurrent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxPhaseTwoCurrent,
                        value => { ElectricalStimulator.PhaseTwoCurrent = value; return ElectricalStimulator.PhaseTwoCurrent; },
                        double.Parse) }
            };

            timeBindings = new Dictionary<TextBox, TextBoxBinding<uint>>
            {
                { StimulusSequenceOptions.textBoxPhaseOneDuration,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxPhaseOneDuration,
                        value => { ElectricalStimulator.PhaseOneDuration = value; return ElectricalStimulator.PhaseOneDuration; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxPhaseTwoDuration,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxPhaseTwoDuration,
                        value => { ElectricalStimulator.PhaseTwoDuration = value; return ElectricalStimulator.PhaseTwoDuration; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxInterPhaseDuration,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxInterPhaseDuration,
                        value => { ElectricalStimulator.InterPhaseInterval = value; return ElectricalStimulator.InterPhaseInterval; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxInterBurstInterval,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxInterBurstInterval,
                        value => { ElectricalStimulator.InterBurstInterval = value; return ElectricalStimulator.InterBurstInterval; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxPulsePeriod,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxPulsePeriod,
                        value => { ElectricalStimulator.InterPulseInterval = value; return ElectricalStimulator.InterPulseInterval; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxTrainDelay,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxTrainDelay,
                        value => { ElectricalStimulator.TriggerDelay = value; return ElectricalStimulator.TriggerDelay; },
                        uint.Parse) }
            };

            countBindings = new Dictionary<TextBox, TextBoxBinding<uint>>
            {
                { StimulusSequenceOptions.textBoxBurstPulseCount,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxBurstPulseCount,
                        value => { ElectricalStimulator.BurstPulseCount = value; return ElectricalStimulator.BurstPulseCount; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxTrainBurstCount,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxTrainBurstCount,
                        value => { ElectricalStimulator.TrainBurstCount = value; return ElectricalStimulator.TrainBurstCount; },
                        uint.Parse) }
            };

            foreach (var binding in currentBindings)
            {
                binding.Key.Leave += TextBoxChanged;
                binding.Key.KeyPress += KeyPressed;
            }

            foreach (var binding in timeBindings)
            {
                binding.Key.Leave += TextBoxChanged;
                binding.Key.KeyPress += KeyPressed;
            }

            foreach (var binding in countBindings)
            {
                binding.Key.Leave += TextBoxChanged;
                binding.Key.KeyPress += KeyPressed;
            }

            StimulusSequenceOptions.Show();

            toolStripStatusIsValid.BorderSides = ToolStripStatusLabelBorderSides.None;

            YAxisMax = 1;
            YAxisMin = -2;

            SetXAxisTitle("Time [µs]");
            SetYAxisTitle("");
            RemoveYAxisLabels();

            DisableVerticalZoom();

            DrawStimulusWaveform();
        }

        void TextBoxChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (currentBindings.ContainsKey(textBox))
                {
                    currentBindings[textBox].UpdateFromTextBox();
                }
                else if (timeBindings.ContainsKey(textBox))
                {
                    timeBindings[textBox].UpdateFromTextBox();
                }
                else if (countBindings.ContainsKey(textBox))
                {
                    countBindings[textBox].UpdateFromTextBox();
                }
                else
                {
                    throw new Exception($"No valid text box found when updating parameters in {nameof(Headstage64ElectricalStimulatorSequenceDialog)}");
                }

                SetStatusValidity();
                DrawStimulusWaveform();
            }
        }

        internal void SetTextBoxBackgroundDefault()
        {
            foreach (var binding in currentBindings)
            {
                SetTextBoxBackgroundDefault(binding.Key);
            }

            foreach (var binding in timeBindings)
            {
                SetTextBoxBackgroundDefault(binding.Key);
            }

            foreach (var binding in countBindings)
            {
                SetTextBoxBackgroundDefault(binding.Key);
            }
        }

        void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && sender is TextBox)
            {
                TextBoxChanged(sender, e);
            }
        }

        internal override bool IsSequenceValid()
        {
            return IsSequenceValid(ElectricalStimulator, out string _);
        }

        bool IsSequenceValid(ConfigureHeadstage64ElectricalStimulator electricalStimulator, out string reason)
        {
            SetTextBoxBackgroundDefault();

            reason = string.Empty;

            if (electricalStimulator == null)
            {
                reason = "Sequence is null";
                return false;
            }

            static bool AnyCurrentIsSet(ConfigureHeadstage64ElectricalStimulator sequence)
            {
                return sequence.PhaseOneCurrent != 0 || sequence.InterPhaseCurrent != 0 || sequence.PhaseTwoCurrent != 0;
            }

            if (electricalStimulator.PhaseOneCurrent != 0 && electricalStimulator.PhaseOneDuration == 0)
            {
                reason = "Phase one current is greater than zero, but duration is zero.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxPhaseOneDuration);
                return false;
            }
            else if (electricalStimulator.InterPhaseCurrent != 0 && electricalStimulator.InterPhaseInterval == 0)
            {
                reason = "Inter-pulse interval current is greater than zero, but duration is zero.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxInterPhaseDuration);
                return false;
            }
            else if (electricalStimulator.PhaseTwoCurrent != 0 && electricalStimulator.PhaseTwoDuration == 0)
            {
                reason = "Phase two current is greater than zero, but duration is zero.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxPhaseTwoDuration);
                return false;
            }
            else if (AnyCurrentIsSet(electricalStimulator) && electricalStimulator.InterPulseInterval == 0)
            {
                reason = "Pulse period has not been set.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxPulsePeriod);
                return false;
            }
            else if (AnyCurrentIsSet(electricalStimulator) && electricalStimulator.InterPulseInterval < electricalStimulator.PhaseOneDuration + electricalStimulator.InterPhaseInterval + electricalStimulator.PhaseTwoDuration)
            {
                reason = "Pulse period is too short.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxPulsePeriod);
                return false;
            }
            else if (AnyCurrentIsSet(electricalStimulator) && electricalStimulator.InterPulseInterval != 0 && electricalStimulator.BurstPulseCount == 0)
            {
                reason = "Burst pulse count has not been set.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxBurstPulseCount);
                return false;
            }
            else if (AnyCurrentIsSet(electricalStimulator) && electricalStimulator.InterPulseInterval != 0 && electricalStimulator.BurstPulseCount != 0 && electricalStimulator.TrainBurstCount == 0)
            {
                reason = "Train burst count has not been set.";
                SetTextBoxBackgroundError(StimulusSequenceOptions.textBoxTrainBurstCount);
                return false;
            }

            return true;
        }

        internal override bool CanCloseForm(out DialogResult result)
        {
            if (ElectricalStimulator != null)
            {
                if (!IsSequenceValid(ElectricalStimulator, out string reason))
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

            PeakToPeak = (Math.Max(Math.Max(ElectricalStimulator.PhaseOneCurrent, ElectricalStimulator.PhaseTwoCurrent), ElectricalStimulator.InterPhaseCurrent)
                          + Math.Abs(Math.Min(Math.Min(ElectricalStimulator.PhaseOneCurrent, ElectricalStimulator.PhaseTwoCurrent), ElectricalStimulator.InterPhaseCurrent))) * ChannelScale;

            PeakToPeak = PeakToPeak == 0 ? ZeroPeakToPeak : PeakToPeak;

            if (ElectricalStimulator != null)
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    waveforms[channel] = new PointPairList { new PointPair(0, 0), new PointPair(ElectricalStimulator.TriggerDelay, 0) };

                    for (int i = 0; i < ElectricalStimulator.TrainBurstCount; i++)
                    {
                        for (int j = 0; j < ElectricalStimulator.BurstPulseCount; j++)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.PhaseOneCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.PhaseOneDuration, ElectricalStimulator.PhaseOneCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.InterPhaseCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.InterPhaseInterval, ElectricalStimulator.InterPhaseCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.PhaseTwoCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.PhaseTwoDuration, ElectricalStimulator.PhaseTwoCurrent / PeakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, 0));

                            if (j != ElectricalStimulator.BurstPulseCount - 1)
                            {
                                waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.InterPulseInterval - (ElectricalStimulator.PhaseOneDuration + ElectricalStimulator.InterPhaseInterval + ElectricalStimulator.PhaseTwoDuration), 0));
                            }
                        }

                        if (i != ElectricalStimulator.TrainBurstCount - 1)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.InterBurstInterval, 0));
                        }
                    }
                }
            }

            return waveforms;
        }

        internal override void HighlightInvalidChannels()
        {
            if (!IsSequenceValid(ElectricalStimulator, out string _))
            {
                foreach (var waveform in GetWaveformCurves())
                {
                    waveform.Color = Color.Red;
                }
            }
        }

        internal override void SetStatusValidity()
        {
            if (IsSequenceValid(ElectricalStimulator, out string reason))
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
    }
}
