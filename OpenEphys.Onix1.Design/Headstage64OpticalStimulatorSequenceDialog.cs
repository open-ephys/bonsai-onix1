using System;
using System.Collections.Generic;
using System.Drawing;
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

        internal ConfigureHeadstage64OpticalStimulator OpticalStimulator
        {
            get => (ConfigureHeadstage64OpticalStimulator)Device;
        }
        readonly Headstage64OpticalStimulatorOptions StimulusSequenceOptions;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="opticalStimulator">Existing stimulus sequence.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="DeviceTablePropertyAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public Headstage64OpticalStimulatorSequenceDialog(ConfigureHeadstage64OpticalStimulator opticalStimulator, bool filterProperties = false)
            : base(opticalStimulator, NumberOfChannels, filterProperties)
        {
            InitializeComponent();
            HideMenuStrip();

            StimulusSequenceOptions = new(OpticalStimulator);
            StimulusSequenceOptions.SetChildFormProperties(this);
            tabPageDefineStimuli.Controls.Add(StimulusSequenceOptions);

            StimulusSequenceOptions.trackBarChannelOnePercent.Scroll += ChannelPercentTrackBarChanged;
            StimulusSequenceOptions.trackBarChannelTwoPercent.Scroll += ChannelPercentTrackBarChanged;

            StimulusSequenceOptions.textBoxMaxCurrent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.MaxCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxChannelOnePercent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.ChannelOneCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxChannelTwoPercent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.ChannelTwoCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxInterBurstInterval.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.InterBurstInterval),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxDelay.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.Delay),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPulseDuration.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.PulseDuration),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPulsePeriod.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.PulsesPerSecond),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPulsesPerBurst.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.PulsesPerBurst),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxBurstsPerTrain.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(OpticalStimulator.BurstsPerTrain),
                false,
                DataSourceUpdateMode.OnValidation);

            foreach (Control control in StimulusSequenceOptions.GetAllControls().OfType<TextBox>())
            {
                control.Validated += (sender, e) =>
                {
                    DrawStimulusWaveform();
                };

                control.KeyPress += (sender, e) =>
                {
                    if (e.KeyChar == '\r' && sender is TextBox tb)
                    {
                        foreach (Binding binding in tb.DataBindings)
                        {
                            binding.WriteValue();
                        }

                        bindingSource.ResetCurrentItem();

                        DrawStimulusWaveform();
                    }
                };
            }

            StimulusSequenceOptions.Show();

            toolStripStatusIsValid.BorderSides = ToolStripStatusLabelBorderSides.None;

            SetXAxisTitle("Time [µs]");
            yAxisScale = "mA";

            DisableVerticalZoom();

            DrawStimulusWaveform();

            bindingSource.ListChanged += (sender, eventArgs) => propertyGrid.Refresh();

            tabControlProperties.SelectedIndexChanged += (sender, eventArgs) =>
            {
                if (tabControlProperties.SelectedTab == tabPageProperties)
                    propertyGrid.Refresh();

                else if (tabControlProperties.SelectedTab == tabPageDefineStimuli)
                    bindingSource.ResetCurrentItem();
            };
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

                propertyGrid.Refresh();

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
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + OpticalStimulator.PulsesPerSecond - OpticalStimulator.PulseDuration, offset));
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
            else if (sequence.PulsesPerBurst > 1 && sequence.PulsesPerSecond <= sequence.PulseDuration)
            {
                reason = "Pulse period is too short compared to the pulse duration.";
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
