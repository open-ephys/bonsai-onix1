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
        internal ConfigureHeadstage64ElectricalStimulator ElectricalStimulator
        {
            get => (ConfigureHeadstage64ElectricalStimulator)Device;
        }
        readonly Headstage64ElectricalStimulatorOptions StimulusSequenceOptions;

        readonly static int NumberOfChannels = 1;

        private protected override string XAxisScaleUnits => "µs";
        private protected override string YAxisScaleUnits => "µA";

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="electricalStimulator">Existing stimulus sequence.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="DeviceTablePropertyAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public Headstage64ElectricalStimulatorSequenceDialog(ConfigureHeadstage64ElectricalStimulator electricalStimulator, bool filterProperties = false)
            : base(electricalStimulator, NumberOfChannels, filterProperties)
        {
            InitializeComponent();
            HideMenuStrip();

            StimulusSequenceOptions = new(ElectricalStimulator);
            StimulusSequenceOptions.SetChildFormProperties(this);
            tabPageDefineStimuli.Controls.Add(StimulusSequenceOptions);

            StimulusSequenceOptions.textBoxPhaseOneCurrent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.PhaseOneCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxInterPhaseCurrent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.InterPhaseCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPhaseTwoCurrent.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.PhaseTwoCurrent),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPhaseOneDuration.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.PhaseOneDuration),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPhaseTwoDuration.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.PhaseTwoDuration),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxInterPhaseDuration.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.InterPhaseInterval),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxInterBurstInterval.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.InterBurstInterval),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxPulsePeriod.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.InterPulseInterval),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxTrainDelay.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.TriggerDelay),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxBurstPulseCount.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.BurstPulseCount),
                false,
                DataSourceUpdateMode.OnValidation);

            StimulusSequenceOptions.textBoxTrainBurstCount.DataBindings.Add(
                "Text",
                bindingSource,
                nameof(ElectricalStimulator.TrainBurstCount),
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

            SetXAxisTitle($"Time [{XAxisScaleUnits}]");
            SetYAxisTitle("");
            RemoveYAxisLabels();

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

        internal void SetTextBoxBackgroundDefault()
        {
            var textBoxes = StimulusSequenceOptions.GetAllControls().OfType<TextBox>();

            foreach (var textBox in textBoxes)
            {
                SetTextBoxBackgroundDefault(textBox);
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
            else if (AnyCurrentIsSet(electricalStimulator) && electricalStimulator.BurstPulseCount > 1
                && (electricalStimulator.InterPulseInterval == 0
                   || electricalStimulator.InterPulseInterval < electricalStimulator.PhaseOneDuration + electricalStimulator.InterPhaseInterval + electricalStimulator.PhaseTwoDuration))
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

        internal override double GetPeakToPeakAmplitudeInMicroAmps()
        {
            var peakToPeak = Math.Max(Math.Max(ElectricalStimulator.PhaseOneCurrent, ElectricalStimulator.PhaseTwoCurrent), ElectricalStimulator.InterPhaseCurrent)
                          + Math.Abs(Math.Min(Math.Min(ElectricalStimulator.PhaseOneCurrent, ElectricalStimulator.PhaseTwoCurrent), ElectricalStimulator.InterPhaseCurrent));

            return peakToPeak == 0 ? ZeroPeakToPeak : peakToPeak;
        }

        internal override PointPairList[] CreateStimulusWaveforms()
        {
            PointPairList[] waveforms = new PointPairList[NumberOfChannels];

            var peakToPeak = GetPeakToPeakAmplitudeInMicroAmps() * ChannelScale;

            if (ElectricalStimulator != null)
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    waveforms[channel] = new PointPairList { new PointPair(0, 0), new PointPair(ElectricalStimulator.TriggerDelay, 0) };

                    for (int i = 0; i < ElectricalStimulator.TrainBurstCount; i++)
                    {
                        for (int j = 0; j < ElectricalStimulator.BurstPulseCount; j++)
                        {
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.PhaseOneCurrent / peakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.PhaseOneDuration, ElectricalStimulator.PhaseOneCurrent / peakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.InterPhaseCurrent / peakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.InterPhaseInterval, ElectricalStimulator.InterPhaseCurrent / peakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X, ElectricalStimulator.PhaseTwoCurrent / peakToPeak));
                            waveforms[channel].Add(new PointPair(waveforms[channel].Last().X + ElectricalStimulator.PhaseTwoDuration, ElectricalStimulator.PhaseTwoCurrent / peakToPeak));
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
