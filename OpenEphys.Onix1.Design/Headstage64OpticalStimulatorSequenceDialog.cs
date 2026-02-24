using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="ConfigureHeadstage64OpticalStimulator"/>.
    /// </summary>
    public partial class Headstage64OpticalStimulatorSequenceDialog : GenericStimulusSequenceDialog
    {
        readonly static int NumberOfChannels = 2;

        internal readonly ConfigureHeadstage64OpticalStimulator OpticalStimulator;
        readonly Headstage64OpticalStimulatorOptions StimulusSequenceOptions;

        readonly Dictionary<TextBox, TextBoxBinding<double>> currentBindings;
        readonly Dictionary<TextBox, TextBoxBinding<double>> timeBindings;
        readonly Dictionary<TextBox, TextBoxBinding<uint>> countBindings;

        private protected override string XAxisScaleUnits => "ms";
        private protected override string YAxisScaleUnits => "mA";

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="opticalStimulator">Existing stimulus sequence.</param>
        public Headstage64OpticalStimulatorSequenceDialog(ConfigureHeadstage64OpticalStimulator opticalStimulator)
            : base(NumberOfChannels, false)
        {
            InitializeComponent();
            HideMenuStrip();

            OpticalStimulator = new(opticalStimulator);

            StimulusSequenceOptions = new(OpticalStimulator);
            StimulusSequenceOptions.SetChildFormProperties(this);
            groupBoxDefineStimuli.Controls.Add(StimulusSequenceOptions);

            StimulusSequenceOptions.trackBarChannelOnePercent.Scroll += ChannelPercentTrackBarChanged;
            StimulusSequenceOptions.trackBarChannelTwoPercent.Scroll += ChannelPercentTrackBarChanged;

            currentBindings = new()
            {
                { StimulusSequenceOptions.textBoxMaxCurrent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxMaxCurrent,
                        value => { OpticalStimulator.MaxCurrent = value; return OpticalStimulator.MaxCurrent; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxChannelOnePercent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxChannelOnePercent,
                        value =>
                        {
                            OpticalStimulator.ChannelOneCurrent = value;
                            StimulusSequenceOptions.trackBarChannelOnePercent.Value = (int)(OpticalStimulator.ChannelOneCurrent * StimulusSequenceOptions.channelOneScalingFactor);
                            return OpticalStimulator.ChannelOneCurrent;
                        },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxChannelTwoPercent,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxChannelTwoPercent,
                        value =>
                        {
                            OpticalStimulator.ChannelTwoCurrent = value;
                            StimulusSequenceOptions.trackBarChannelTwoPercent.Value = (int)(OpticalStimulator.ChannelTwoCurrent * StimulusSequenceOptions.channelTwoScalingFactor);
                            return OpticalStimulator.ChannelTwoCurrent;
                        },
                        double.Parse) }
            };

            timeBindings = new()
            {
                { StimulusSequenceOptions.textBoxInterBurstInterval,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxInterBurstInterval,
                        value => { OpticalStimulator.InterBurstInterval = value; return OpticalStimulator.InterBurstInterval; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxDelay,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxDelay,
                        value => { OpticalStimulator.Delay = value; return OpticalStimulator.Delay; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxPulseDuration,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxPulseDuration,
                        value => { OpticalStimulator.PulseDuration = value; return OpticalStimulator.PulseDuration; },
                        double.Parse) },
                { StimulusSequenceOptions.textBoxPulseFrequencyHz,
                    new TextBoxBinding<double>(
                        StimulusSequenceOptions.textBoxPulseFrequencyHz,
                        value => { OpticalStimulator.PulsesPerSecond = value; return OpticalStimulator.PulsesPerSecond; },
                        double.Parse) },
            };

            countBindings = new()
            {
                { StimulusSequenceOptions.textBoxPulsesPerBurst,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxPulsesPerBurst,
                        value => { OpticalStimulator.PulsesPerBurst = value; return OpticalStimulator.PulsesPerBurst; },
                        uint.Parse) },
                { StimulusSequenceOptions.textBoxBurstsPerTrain,
                    new TextBoxBinding<uint>(
                        StimulusSequenceOptions.textBoxBurstsPerTrain,
                        value => { OpticalStimulator.BurstsPerTrain = value; return OpticalStimulator.BurstsPerTrain; },
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

            SetXAxisTitle($"Time [{XAxisScaleUnits}]");

            DisableVerticalZoom();

            DrawStimulusWaveform();

            stimulusWaveformToolStripMenuItem.Text = "Optical Stimulus Sequence";
        }

        void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && sender is TextBox)
            {
                TextBoxChanged(sender, e);
            }
        }

        void TextBoxChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (currentBindings.TryGetValue(textBox, out var currentBinding))
                {
                    currentBinding.UpdateFromTextBox();
                }
                else if (timeBindings.TryGetValue(textBox, out var timeBinding))
                {
                    timeBinding.UpdateFromTextBox();
                }
                else if (countBindings.TryGetValue(textBox, out var countBinding))
                {
                    countBinding.UpdateFromTextBox();
                }
                else
                {
                    throw new NotImplementedException($"No valid text box found when updating parameters in {nameof(Headstage64OpticalStimulatorSequenceDialog)}");
                }

                SetTextBoxBackgroundDefault(textBox);
                DrawStimulusWaveform();
            }
        }

        void ChannelPercentTrackBarChanged(object sender, EventArgs eventArgs)
        {
            if (sender is TrackBar tb)
            {
                if (tb.Name == StimulusSequenceOptions.trackBarChannelOnePercent.Name)
                {
                    OpticalStimulator.ChannelOneCurrent = tb.Value / StimulusSequenceOptions.channelOneScalingFactor;
                    tb.Value = (int)(OpticalStimulator.ChannelOneCurrent * StimulusSequenceOptions.channelOneScalingFactor);
                    StimulusSequenceOptions.textBoxChannelOnePercent.Text = OpticalStimulator.ChannelOneCurrent.ToString();
                }
                else if (tb.Name == StimulusSequenceOptions.trackBarChannelTwoPercent.Name)
                {
                    OpticalStimulator.ChannelTwoCurrent = tb.Value / StimulusSequenceOptions.channelTwoScalingFactor;
                    tb.Value = (int)(OpticalStimulator.ChannelTwoCurrent * StimulusSequenceOptions.channelTwoScalingFactor);
                    StimulusSequenceOptions.textBoxChannelTwoPercent.Text = OpticalStimulator.ChannelTwoCurrent.ToString();
                }
                else
                {
                    throw new NotImplementedException($"Could not find a valid track bar when updating parameters in {nameof(Headstage64OpticalStimulatorSequenceDialog)}");
                }

                DrawStimulusWaveform();
            }
        }

        double GetChannelCurrent(double maxCurrent, double channelPercent)
        {
            return maxCurrent * channelPercent;
        }

        double GetChannelCurrentScaled(double maxCurrent, double channelPercent, double scale)
        {
            return (GetChannelCurrent(maxCurrent, channelPercent) / 100.0) / scale;
        }

        internal override double GetPeakToPeakAmplitudeInMicroAmps()
        {
            return OpticalStimulator.MaxCurrent == 0 ? ZeroPeakToPeak : OpticalStimulator.MaxCurrent;
        }

        internal override PointPairList[] CreateStimulusWaveforms()
        {
            PointPairList[] waveforms = new PointPairList[NumberOfChannels];

            var peakToPeak = GetPeakToPeakAmplitudeInMicroAmps() * ChannelScale;

            for (int channel = 0; channel < NumberOfChannels; channel++)
            {
                double offset = -channel;

                waveforms[channel] = new PointPairList
                {
                    new PointPairList { new PointPair(0, offset), new PointPair(OpticalStimulator.Delay, offset) }
                };

                var stimulusCurrent = offset + GetChannelCurrentScaled(OpticalStimulator.MaxCurrent,
                                                                       channel == 0 ? OpticalStimulator.ChannelOneCurrent : OpticalStimulator.ChannelTwoCurrent,
                                                                       peakToPeak);

                for (int i = 0; i < OpticalStimulator.BurstsPerTrain; i++)
                {
                    for (int j = 0; j < OpticalStimulator.PulsesPerBurst; j++)
                    {
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, stimulusCurrent));
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + OpticalStimulator.PulseDuration, stimulusCurrent));
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, offset));

                        if (j != OpticalStimulator.PulsesPerBurst - 1)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + 1000.0 / OpticalStimulator.PulsesPerSecond - OpticalStimulator.PulseDuration, offset));
                        }
                    }

                    if (i != OpticalStimulator.BurstsPerTrain - 1)
                    {
                        waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + OpticalStimulator.InterBurstInterval, offset));
                    }
                }
            }

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
            return IsSequenceValid(OpticalStimulator, out string _);
        }

        static bool IsSequenceValid(ConfigureHeadstage64OpticalStimulator sequence, out string reason)
        {
            reason = string.Empty;

            if (sequence.MaxCurrent < 0 || sequence.MaxCurrent > 300)
            {
                reason = "Maximum current is invalid.";
                return false;
            }
            else if (sequence.PulsesPerBurst > 1 && 1000.0 / sequence.PulsesPerSecond <= sequence.PulseDuration)
            {
                reason = "Pulse frequency is too low compared to the pulse duration.";
                return false;
            }

            return true;
        }

        internal override void SetStatusValidity()
        {
            if (IsSequenceValid(OpticalStimulator, out string reason))
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
