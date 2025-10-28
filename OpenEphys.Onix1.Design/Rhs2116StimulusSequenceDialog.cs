using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Rhs2116StimulusSequencePair"/>.
    /// </summary>
    public partial class Rhs2116StimulusSequenceDialog : GenericStimulusSequenceDialog
    {
        readonly TabControl tabControlVisualization = new();
        readonly TabPage tabPageWaveform = new();
        readonly TabPage tabPageTable = new();
        readonly DataGridView dataGridViewStimulusTable = new();

        const double SamplePeriodMilliSeconds = 1e3 / Rhs2116.SampleFrequencyHz;
        const int NumberOfChannels = 32;

        internal Rhs2116StimulusSequencePair Sequence { get => Trigger.StimulusSequence; }

        internal ConfigureRhs2116Trigger Trigger
        {
            get => (ConfigureRhs2116Trigger)Device;
            private set => Device = value;
        }

        readonly Rhs2116StimulusSequencePair SequenceCopy = new();

        readonly double[] RequestedAnodicAmplitudeuA;
        readonly double[] RequestedCathodicAmplitudeuA;

        /// <summary>
        /// Holds the step size that is displayed in the text box of the GUI. 
        /// This is not the step size that is saved for the stimulus sequence object.
        /// </summary>
        Rhs2116StepSize StepSize { get; set; }

        internal readonly Rhs2116ChannelConfigurationDialog ChannelDialog;
        readonly Rhs2116StimulusSequenceOptions StimulusSequenceOptions;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with 
        /// visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="rhs2116Trigger">Existing <see cref="ConfigureRhs2116Trigger"/> object.</param>
        public Rhs2116StimulusSequenceDialog(ConfigureRhs2116Trigger rhs2116Trigger)
            : base(rhs2116Trigger, NumberOfChannels)
        {
            if (rhs2116Trigger.ProbeGroup.NumberOfContacts != NumberOfChannels)
            {
                throw new ArgumentException($"Probe group is not valid: {NumberOfChannels} channels were expected, there are {rhs2116Trigger.ProbeGroup.NumberOfContacts} instead.");
            }

            InitializeComponent();

            // NB: Add a tabbed control to show the table of stimuli values
            tableLayoutPanel1.Controls.Remove(panelWaveform);

            tabControlVisualization.SuspendLayout();
            tabPageWaveform.SuspendLayout();
            tabPageTable.SuspendLayout();

            tabControlVisualization.Controls.Add(tabPageWaveform);
            tabControlVisualization.Controls.Add(tabPageTable);
            tabControlVisualization.Dock = DockStyle.Fill;
            tabControlVisualization.Name = nameof(tabControlVisualization);

            tabPageWaveform.Controls.Add(panelWaveform);
            tabPageWaveform.Name = nameof(tabPageWaveform);
            tabPageWaveform.Text = "Stimulus Waveform";
            tabPageWaveform.UseVisualStyleBackColor = true;

            tabPageTable.Controls.Add(dataGridViewStimulusTable);
            tabPageTable.Name = nameof(tabPageTable);
            tabPageTable.Text = "Table";
            tabPageTable.UseVisualStyleBackColor = true;

            dataGridViewStimulusTable.AllowUserToAddRows = false;
            dataGridViewStimulusTable.AllowUserToDeleteRows = false;
            dataGridViewStimulusTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewStimulusTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewStimulusTable.Dock = DockStyle.Fill;
            dataGridViewStimulusTable.Name = nameof(dataGridViewStimulusTable);
            dataGridViewStimulusTable.ReadOnly = true;
            dataGridViewStimulusTable.RowHeadersWidth = 62;
            dataGridViewStimulusTable.RowTemplate.Height = 24;
            dataGridViewStimulusTable.TabStop = false;

            tableLayoutPanel1.Controls.Add(tabControlVisualization);

            tabControlVisualization.ResumeLayout(false);
            tabPageWaveform.ResumeLayout(false);
            tabPageTable.ResumeLayout(false);

            Trigger = rhs2116Trigger;

            dataGridViewStimulusTable.DataBindingComplete += DataBindingComplete;
            SetTableDataSource();

            RequestedAnodicAmplitudeuA = new double[Sequence.Stimuli.Length];
            RequestedCathodicAmplitudeuA = new double[Sequence.Stimuli.Length];

            for (int i = 0; i < Sequence.Stimuli.Length; i++)
            {
                RequestedAnodicAmplitudeuA[i] = Sequence.Stimuli[i].AnodicAmplitudeSteps * Sequence.CurrentStepSizeuA;
                RequestedCathodicAmplitudeuA[i] = Sequence.Stimuli[i].CathodicAmplitudeSteps * Sequence.CurrentStepSizeuA;
            }

            StepSize = Sequence.CurrentStepSize;

            StimulusSequenceOptions = new();
            tabPageDefineStimuli.Controls.Add(StimulusSequenceOptions.SetChildFormProperties(this));

            StimulusSequenceOptions.buttonAddPulses.Click += ButtonAddPulses_Click;
            StimulusSequenceOptions.buttonReadPulses.Click += ButtonReadPulses_Click;
            StimulusSequenceOptions.buttonClearPulses.Click += ButtonClearPulses_Click;

            StimulusSequenceOptions.checkBoxAnodicFirst.CheckedChanged += Checkbox_CheckedChanged;
            StimulusSequenceOptions.checkboxBiphasicSymmetrical.CheckedChanged += Checkbox_CheckedChanged;

            StimulusSequenceOptions.textboxPulseWidthCathodic.KeyDown += ParameterKeyDown_Time;
            StimulusSequenceOptions.textboxPulseWidthCathodic.Leave += Samples_TextChanged;

            StimulusSequenceOptions.textboxPulseWidthAnodic.KeyDown += ParameterKeyDown_Time;
            StimulusSequenceOptions.textboxPulseWidthAnodic.Leave += Samples_TextChanged;

            StimulusSequenceOptions.textboxInterPulseInterval.KeyDown += ParameterKeyDown_Time;
            StimulusSequenceOptions.textboxInterPulseInterval.Leave += Samples_TextChanged;

            StimulusSequenceOptions.textboxDelay.KeyDown += ParameterKeyDown_Time;
            StimulusSequenceOptions.textboxDelay.Leave += Samples_TextChanged;

            StimulusSequenceOptions.textboxInterStimulusInterval.KeyDown += ParameterKeyDown_Time;
            StimulusSequenceOptions.textboxInterStimulusInterval.Leave += Samples_TextChanged;

            StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Leave += Amplitude_TextChanged;
            StimulusSequenceOptions.textboxAmplitudeAnodicRequested.KeyDown += ParameterKeyDown_Amplitude;

            StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Leave += Amplitude_TextChanged;
            StimulusSequenceOptions.textboxAmplitudeCathodicRequested.KeyDown += ParameterKeyDown_Amplitude;

            StimulusSequenceOptions.numericUpDownNumberOfPulses.KeyDown += NumericUpDownNumberOfPulses_KeyDown;
            StimulusSequenceOptions.numericUpDownNumberOfPulses.Leave += NumericUpDownNumberOfPulses_Leave;

            ChannelDialog = new(rhs2116Trigger.ProbeGroup);

            ChannelDialog.SetChildFormProperties(this).AddDialogToPanel(StimulusSequenceOptions.panelProbe);
            this.AddMenuItemsFromDialogToFileOption(ChannelDialog, "Channel Configuration");

            ChannelDialog.OnSelect += OnSelect;
            ChannelDialog.OnZoom += OnZoom;

            StimulusSequenceOptions.Show();

            StimulusSequenceOptions.textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            DrawStimulusWaveform();
        }

        internal void OnZoom(object sender, EventArgs e)
        {
            ChannelDialog.UpdateFontSize();
        }

        string GetStepSizeStringuA(Rhs2116StepSize stepSize)
        {
            return Rhs2116StimulusSequence.GetStepSizeuA(stepSize).ToString() + " µA";
        }

        internal override void HighlightInvalidChannels()
        {
            if (ChannelDialog == null)
            {
                return;
            }

            var contactObjects = ChannelDialog.zedGraphChannels.GraphPane.GraphObjList
                                 .OfType<BoxObj>()
                                 .Where(c => c is not PolyObj);

            foreach (var contact in contactObjects)
            {
                if (contact.Tag is ContactTag contactTag)
                {
                    var contactIndex = contactTag.ProbeIndex == 0
                                ? contactTag.ContactIndex
                                : contactTag.ContactIndex + ChannelDialog.ProbeGroup.Probes
                                                            .Take(contactTag.ProbeIndex)
                                                            .Aggregate(0, (total, next) => total + next.NumberOfContacts);

                    if (!Sequence.Stimuli[contactIndex].IsValid())
                    {
                        contact.Fill.Color = Color.Red;

                        if (contact.Border.Color != ChannelDialog.SelectedContactBorder)
                            contact.Border.Color = Color.Red;
                    }
                }
            }

            foreach (var waveform in GetWaveformCurves())
            {
                int waveformIndex = int.Parse(waveform.Label.Text.ToLowerInvariant().TrimStart('c', 'h'));

                if (!Sequence.Stimuli[waveformIndex].IsValid())
                {
                    waveform.Color = Color.Red;
                }
            }

            ChannelDialog.RefreshZedGraph();
        }

        internal override double GetPeakToPeakAmplitudeInMicroAmps()
        {
            return Sequence.MaximumPeakToPeakAmplitudeSteps > 0
                ? Sequence.GetMaxPeakToPeakAmplitudeuA()
                : Sequence.CurrentStepSizeuA;
        }

        PointPairList CreateStimulusWaveform(Rhs2116Stimulus stimulus, double yOffset, double peakToPeak)
        {
            yOffset /= peakToPeak;

            PointPairList points = new()
            {
                { 0, yOffset },
                { stimulus.DelaySamples * SamplePeriodMilliSeconds, yOffset }
            };

            for (int i = 0; i < stimulus.NumberOfStimuli; i++)
            {
                double amplitude = (stimulus.AnodicFirst ? stimulus.AnodicAmplitudeSteps : -stimulus.CathodicAmplitudeSteps) * Sequence.CurrentStepSizeuA / peakToPeak + yOffset;
                double width = (stimulus.AnodicFirst ? stimulus.AnodicWidthSamples : stimulus.CathodicWidthSamples) * SamplePeriodMilliSeconds;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.DwellSamples * SamplePeriodMilliSeconds, yOffset);

                amplitude = (stimulus.AnodicFirst ? -stimulus.CathodicAmplitudeSteps : stimulus.AnodicAmplitudeSteps) * Sequence.CurrentStepSizeuA / peakToPeak + yOffset;
                width = (stimulus.AnodicFirst ? stimulus.CathodicWidthSamples : stimulus.AnodicWidthSamples) * SamplePeriodMilliSeconds;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.InterStimulusIntervalSamples * SamplePeriodMilliSeconds, yOffset);
            }

            points.Add(Sequence.SequenceLengthSamples * SamplePeriodMilliSeconds, yOffset);

            return points;
        }

        internal override PointPairList[] CreateStimulusWaveforms()
        {
            PointPairList[] waveforms = new PointPairList[NumberOfChannels];

            if (ChannelDialog == null)
            {
                return waveforms;
            }

            bool plotAllContacts = ChannelDialog.SelectedContacts.All(x => x == false);

            var peakToPeak = GetPeakToPeakAmplitudeInMicroAmps() * ChannelScale;

            for (int i = 0; i < Sequence.Stimuli.Length; i++)
            {
                var channelOffset = -peakToPeak * i;

                if (ChannelDialog.SelectedContacts[i] || plotAllContacts)
                {
                    waveforms[i] = CreateStimulusWaveform(Sequence.Stimuli[i], channelOffset, peakToPeak);
                }
                else
                {
                    waveforms[i] = new PointPairList();
                }
            }

            return waveforms;
        }

        internal override void SetStatusValidity()
        {
            if (Sequence.Valid && Sequence.FitsInHardware)
            {
                toolStripStatusIsValid.Image = Properties.Resources.StatusReadyImage;
                toolStripStatusIsValid.Text = "Valid stimulus sequence";
            }
            else
            {
                if (!Sequence.FitsInHardware)
                {
                    toolStripStatusIsValid.Image = Properties.Resources.StatusBlockedImage;
                    toolStripStatusIsValid.Text = "Invalid sequence - Too many pulses defined";
                }
                else
                {
                    var reason = Sequence.Stimuli.Select((s, ind) =>
                    {
                        s.IsValid(out string reason);
                        return (reason, ind);
                    })
                    .Where(reason => reason.reason != "")
                    .FirstOrDefault();

                    toolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                    toolStripStatusIsValid.Text = string.Format("Invalid sequence - Contact {0}, Reason: {1}", reason.ind, reason.reason);
                }
            }

            SetPercentOfSlotsUsed();
        }

        void SetPercentOfSlotsUsed()
        {
            toolStripStatusText.Text = string.Format("{0, 0:P1} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
        }

        /// <inheritdoc cref="GetSampleFromAmplitude(double, Rhs2116StepSize, out byte)"/>
        bool GetSampleFromAmplitude(double value, out byte samples)
        {
            return GetSampleFromAmplitude(value, StepSize, out samples);
        }

        double GetTimeFromSample(uint value)
        {
            return value * SamplePeriodMilliSeconds;
        }

        string GetAmplitudeString(byte amplitude)
        {
            return GetAmplitudeString(amplitude, StepSize);
        }

        string GetTimeString(uint time)
        {
            return string.Format("{0:F2}", GetTimeFromSample(time));
        }

        string GetAmplitudeString(byte amplitude, Rhs2116StepSize stepSize)
        {
            string format = stepSize switch
            {
                Rhs2116StepSize.Step10nA or Rhs2116StepSize.Step20nA or Rhs2116StepSize.Step50nA => "{0:F2}",
                Rhs2116StepSize.Step100nA or Rhs2116StepSize.Step200nA or Rhs2116StepSize.Step500nA => "{0:F1}",
                Rhs2116StepSize.Step1000nA or Rhs2116StepSize.Step2000nA or Rhs2116StepSize.Step5000nA or Rhs2116StepSize.Step10000nA => "{0:F0}",
                _ => "{0:F3}",
            };
            return string.Format(format, GetAmplitudeFromSample(amplitude, stepSize));
        }

        double GetAmplitudeFromSample(byte value, Rhs2116StepSize stepSize)
        {
            return value * Rhs2116StimulusSequence.GetStepSizeuA(stepSize);
        }

        /// <summary>
        /// Get the number of samples needed at the current step size to represent a given amplitude.
        /// </summary>
        /// <param name="value">Double value defining the amplitude in microamps.</param>
        /// <param name="stepSize"><see cref="Rhs2116StepSize"/></param>
        /// <param name="samples">Output returning the number of samples as a byte.</param>
        /// <returns>Returns true if the number of samples is a valid byte value (between 0 and 255). Returns false if the number of samples cannot be represented in byte format.</returns>
        bool GetSampleFromAmplitude(double value, Rhs2116StepSize stepSize, out byte samples)
        {
            var ratio = GetRatio(value, Rhs2116StimulusSequence.GetStepSizeuA(stepSize));

            if (ratio >= 255) samples = 255;
            else if (ratio <= 0) samples = 0;
            else samples = (byte)Math.Round(ratio);

            return !(ratio > byte.MaxValue || ratio < 0);
        }

        void ButtonAddPulses_Click(object sender, EventArgs e)
        {
            var stimuli = Sequence.Stimuli
                            .Select((s, ind) => { return (Index: ind, Stimulus: s); })
                            .Where(s => s.Stimulus.Valid
                                        && (s.Stimulus.AnodicAmplitudeSteps != 0
                                            || s.Stimulus.CathodicAmplitudeSteps != 0
                                            || (s.Stimulus.AnodicAmplitudeSteps == 0 && RequestedAnodicAmplitudeuA[s.Index] != 0.0)
                                            || (s.Stimulus.CathodicAmplitudeSteps == 0 && RequestedCathodicAmplitudeuA[s.Index] != 0.0))
                                        && !ChannelDialog.SelectedContacts[s.Index])
                            .Select(s =>
                            {
                                GetSampleFromAmplitude(RequestedAnodicAmplitudeuA[s.Index], out var requestedAnodicSteps);
                                var requestedAnodicError = s.Stimulus.AnodicAmplitudeSteps == 0
                                                           ? GetAmplitudeFromSample(requestedAnodicSteps, StepSize)
                                                           : CalculateAmplitudePercentError(RequestedAnodicAmplitudeuA[s.Index], StepSize);

                                GetSampleFromAmplitude(RequestedCathodicAmplitudeuA[s.Index], out var requestedCathodicSteps);
                                var requestedCathodicError = s.Stimulus.CathodicAmplitudeSteps == 0
                                                             ? GetAmplitudeFromSample(requestedCathodicSteps, StepSize)
                                                             : CalculateAmplitudePercentError(RequestedCathodicAmplitudeuA[s.Index], StepSize);

                                return (s.Index,
                                        ErrorAnodic: requestedAnodicError,
                                        ErrorCathodic: requestedCathodicError,
                                        StepsAnodic: requestedAnodicSteps,
                                        StepsCathodic: requestedCathodicSteps);
                            });

            if (Sequence.CurrentStepSize != StepSize && stimuli.Any(e => e.ErrorCathodic != 0 || e.ErrorAnodic != 0 &&
                                                                         ((Sequence.Stimuli[e.Index].AnodicAmplitudeSteps == 0 && e.StepsAnodic != 0) ||
                                                                          (Sequence.Stimuli[e.Index].CathodicAmplitudeSteps == 0 && e.StepsCathodic != 0))))
            {
                var message = $"The step size is changing from {GetStepSizeStringuA(Sequence.CurrentStepSize)} to {GetStepSizeStringuA(StepSize)}, " +
                    $"which will adjust some amplitudes. If applied, the following values will be modified:\n";

                foreach (var (Index, ErrorAnodic, ErrorCathodic, StepsAnodic, StepsCathodic) in stimuli)
                {
                    if (ErrorAnodic != 0 || ErrorCathodic != 0 && ((Sequence.Stimuli[Index].AnodicAmplitudeSteps == 0 && StepsAnodic != 0) || (Sequence.Stimuli[Index].CathodicAmplitudeSteps == 0 && StepsCathodic != 0)))
                    {
                        var oldAnodicAmplitude = GetAmplitudeFromSample(Sequence.Stimuli[Index].AnodicAmplitudeSteps, Sequence.CurrentStepSize);
                        var newAnodicAmplitude = GetAmplitudeFromSample(StepsAnodic, StepSize);

                        var oldCathodicAmplitude = GetAmplitudeFromSample(Sequence.Stimuli[Index].CathodicAmplitudeSteps, Sequence.CurrentStepSize);
                        var newCathodicAmplitude = GetAmplitudeFromSample(StepsCathodic, StepSize);

                        if (oldAnodicAmplitude == newAnodicAmplitude && oldCathodicAmplitude == newCathodicAmplitude) continue;

                        message += $"\nChannel {Index}: Anode = {oldAnodicAmplitude} µA → {newAnodicAmplitude} µA," +
                            $" Cathode = {oldCathodicAmplitude} µA → {newCathodicAmplitude} µA";
                    }
                }

                message += "\n\nClick Update to update these channels, or Cancel to stop.";

                CustomMessageBox messageBox = new(message, "New Amplitude Values", "Update", "Cancel");
                var result = messageBox.ShowDialog();

                if (result == DialogResult.Cancel) return;
            }

            foreach (var (Index, ErrorAnodic, ErrorCathodic, StepsAnodic, StepsCathodic) in stimuli)
            {
                if (StepsAnodic == 0 && StepsCathodic == 0)
                {
                    if (Sequence.Stimuli[Index].AnodicAmplitudeSteps != 0 && Sequence.Stimuli[Index].CathodicAmplitudeSteps != 0)
                    {
                        SequenceCopy.UpdateStimulus(Sequence.Stimuli[Index], Index); // NB: Store the current pulse pattern before clearing
                        Sequence.Stimuli[Index].Clear();
                    }
                }
                else
                {
                    if (Sequence.Stimuli[Index].NumberOfStimuli == 0 && SequenceCopy.Stimuli[Index].IsValid() && SequenceCopy.Stimuli[Index].NumberOfStimuli != 0)
                    {
                        Sequence.UpdateStimulus(SequenceCopy.Stimuli[Index], Index); // NB: Restore pulse timings before adding amplitude steps
                    }
                    else if (Sequence.Stimuli[Index].NumberOfStimuli == 0 && SequenceCopy.Stimuli[Index].NumberOfStimuli != 0) continue;

                    Sequence.Stimuli[Index].AnodicAmplitudeSteps = StepsAnodic;
                    Sequence.Stimuli[Index].CathodicAmplitudeSteps = StepsCathodic;
                }
            }

            SetTableDataSource();

            for (int i = 0; i < ChannelDialog.SelectedContacts.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i])
                {
                    if (StimulusSequenceOptions.textboxDelay.Tag != null)
                    {
                        Sequence.Stimuli[i].DelaySamples = (uint)StimulusSequenceOptions.textboxDelay.Tag;
                    }

                    if (StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag != null)
                    {
                        RequestedAnodicAmplitudeuA[i] = (double)StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag;
                    }

                    if (StimulusSequenceOptions.textboxAmplitudeAnodic.Tag != null)
                    {
                        Sequence.Stimuli[i].AnodicAmplitudeSteps = (byte)StimulusSequenceOptions.textboxAmplitudeAnodic.Tag;
                    }

                    if (StimulusSequenceOptions.textboxPulseWidthAnodic.Tag != null)
                    {
                        Sequence.Stimuli[i].AnodicWidthSamples = (uint)StimulusSequenceOptions.textboxPulseWidthAnodic.Tag;
                    }

                    if (StimulusSequenceOptions.textboxInterPulseInterval.Tag != null)
                    {
                        Sequence.Stimuli[i].DwellSamples = (uint)StimulusSequenceOptions.textboxInterPulseInterval.Tag;
                    }

                    if (StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag != null)
                    {
                        RequestedCathodicAmplitudeuA[i] = (double)StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag;
                    }

                    if (StimulusSequenceOptions.textboxAmplitudeCathodic.Tag != null)
                    {
                        Sequence.Stimuli[i].CathodicAmplitudeSteps = (byte)StimulusSequenceOptions.textboxAmplitudeCathodic.Tag;
                    }

                    if (StimulusSequenceOptions.textboxPulseWidthCathodic.Tag != null)
                    {
                        Sequence.Stimuli[i].CathodicWidthSamples = (uint)StimulusSequenceOptions.textboxPulseWidthCathodic.Tag;
                    }

                    if (StimulusSequenceOptions.textboxInterStimulusInterval.Tag != null)
                    {
                        Sequence.Stimuli[i].InterStimulusIntervalSamples = (uint)StimulusSequenceOptions.textboxInterStimulusInterval.Tag;
                    }

                    Sequence.Stimuli[i].NumberOfStimuli = (uint)StimulusSequenceOptions.numericUpDownNumberOfPulses.Value;
                    Sequence.Stimuli[i].AnodicFirst = StimulusSequenceOptions.checkBoxAnodicFirst.Checked;
                }
            }

            Sequence.CurrentStepSize = StepSize;

            ChannelDialog.HighlightEnabledContacts();

            DrawStimulusWaveform();
        }

        double CalculateAmplitudePercentError(double amplitude, Rhs2116StepSize stepSize)
        {
            if (amplitude == 0) return 0;

            var stepSizeuA = Rhs2116StimulusSequence.GetStepSizeuA(stepSize);

            GetSampleFromAmplitude(amplitude, stepSize, out var steps);

            return 100 * ((amplitude - steps * stepSizeuA) / amplitude);
        }

        void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
            {
                if (StimulusSequenceOptions.checkBoxAnodicFirst.Checked)
                {
                    StimulusSequenceOptions.groupBoxCathode.Visible = false;
                    StimulusSequenceOptions.groupBoxAnode.Visible = true;

                    StimulusSequenceOptions.textboxPulseWidthCathodic.Text = StimulusSequenceOptions.textboxPulseWidthAnodic.Text;
                    StimulusSequenceOptions.textboxPulseWidthCathodic.Tag = StimulusSequenceOptions.textboxPulseWidthAnodic.Tag;

                    StimulusSequenceOptions.textboxAmplitudeCathodic.Text = StimulusSequenceOptions.textboxAmplitudeAnodic.Text;
                    StimulusSequenceOptions.textboxAmplitudeCathodic.Tag = StimulusSequenceOptions.textboxAmplitudeAnodic.Tag;

                    StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text = StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text;
                    StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag = StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag;
                }
                else
                {
                    StimulusSequenceOptions.groupBoxCathode.Visible = true;
                    StimulusSequenceOptions.groupBoxAnode.Visible = false;

                    StimulusSequenceOptions.textboxPulseWidthAnodic.Text = StimulusSequenceOptions.textboxPulseWidthCathodic.Text;
                    StimulusSequenceOptions.textboxPulseWidthAnodic.Tag = StimulusSequenceOptions.textboxPulseWidthCathodic.Tag;

                    StimulusSequenceOptions.textboxAmplitudeAnodic.Text = StimulusSequenceOptions.textboxAmplitudeCathodic.Text;
                    StimulusSequenceOptions.textboxAmplitudeAnodic.Tag = StimulusSequenceOptions.textboxAmplitudeCathodic.Tag;

                    StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text = StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text;
                    StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag = StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag;
                }
            }
            else
            {
                StimulusSequenceOptions.groupBoxCathode.Visible = true;
                StimulusSequenceOptions.groupBoxAnode.Visible = true;
            }
        }

        void ButtonClearPulses_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ChannelDialog.SelectedContacts.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i])
                {
                    Sequence.Stimuli[i].Clear();
                    RequestedAnodicAmplitudeuA[i] = 0.0;
                    RequestedCathodicAmplitudeuA[i] = 0.0;
                }
            }

            ChannelDialog.HighlightEnabledContacts();
            DrawStimulusWaveform();
            ChannelDialog.RefreshZedGraph();
        }

        void ButtonReadPulses_Click(object sender, EventArgs e)
        {
            if (ChannelDialog.SelectedContacts.Count(x => x) != 1)
            {
                MessageBox.Show("Please choose a single contact to read from.");
                return;
            }

            var index = ChannelDialog.SelectedContacts
                        .Select((s, ind) => { return (Selected: s, Ind: ind); })
                        .Where(c => c.Selected)
                        .Select(c => c.Ind)
                        .First();

            if (Sequence.Stimuli[index].AnodicAmplitudeSteps == Sequence.Stimuli[index].CathodicAmplitudeSteps &&
                Sequence.Stimuli[index].AnodicWidthSamples == Sequence.Stimuli[index].CathodicWidthSamples)
            {
                StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked = true;
            }
            else
            {
                StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked = false;
            }

            StepSize = Sequence.CurrentStepSize;
            StimulusSequenceOptions.textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            StimulusSequenceOptions.checkBoxAnodicFirst.Checked = Sequence.Stimuli[index].AnodicFirst;

            Checkbox_CheckedChanged(StimulusSequenceOptions.checkboxBiphasicSymmetrical, e);

            StimulusSequenceOptions.textboxDelay.Text = GetTimeString(Sequence.Stimuli[index].DelaySamples);
            StimulusSequenceOptions.textboxDelay.Tag = Sequence.Stimuli[index].DelaySamples;

            StimulusSequenceOptions.textboxAmplitudeAnodic.Text = GetAmplitudeString(Sequence.Stimuli[index].AnodicAmplitudeSteps);
            StimulusSequenceOptions.textboxAmplitudeAnodic.Tag = Sequence.Stimuli[index].AnodicAmplitudeSteps;

            if (RequestedAnodicAmplitudeuA[index] != 0.0)
            {
                StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text = RequestedAnodicAmplitudeuA[index].ToString();
                StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag = RequestedAnodicAmplitudeuA[index];
            }
            else
            {
                StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text = "";
                StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag = null;
            }

            StimulusSequenceOptions.textboxPulseWidthAnodic.Text = GetTimeString(Sequence.Stimuli[index].AnodicWidthSamples);
            StimulusSequenceOptions.textboxPulseWidthAnodic.Tag = Sequence.Stimuli[index].AnodicWidthSamples;

            StimulusSequenceOptions.textboxAmplitudeCathodic.Text = GetAmplitudeString(Sequence.Stimuli[index].CathodicAmplitudeSteps);
            StimulusSequenceOptions.textboxAmplitudeCathodic.Tag = Sequence.Stimuli[index].CathodicAmplitudeSteps;

            if (RequestedCathodicAmplitudeuA[index] != 0.0)
            {
                StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text = RequestedCathodicAmplitudeuA[index].ToString();
                StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag = RequestedCathodicAmplitudeuA[index];
            }
            else
            {
                StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text = "";
                StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag = null;
            }

            StimulusSequenceOptions.textboxPulseWidthCathodic.Text = GetTimeString(Sequence.Stimuli[index].CathodicWidthSamples);
            StimulusSequenceOptions.textboxPulseWidthCathodic.Tag = Sequence.Stimuli[index].CathodicWidthSamples;

            StimulusSequenceOptions.textboxInterPulseInterval.Text = GetTimeString(Sequence.Stimuli[index].DwellSamples);
            StimulusSequenceOptions.textboxInterPulseInterval.Tag = Sequence.Stimuli[index].DwellSamples;

            StimulusSequenceOptions.textboxInterStimulusInterval.Text = GetTimeString(Sequence.Stimuli[index].InterStimulusIntervalSamples);
            StimulusSequenceOptions.textboxInterStimulusInterval.Tag = Sequence.Stimuli[index].InterStimulusIntervalSamples;

            StimulusSequenceOptions.numericUpDownNumberOfPulses.Value = Sequence.Stimuli[index].NumberOfStimuli;
        }

        void ParameterKeyDown_Time(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Samples_TextChanged(sender, e);
                ButtonAddPulses_Click(sender, e);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void ParameterKeyDown_Amplitude(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Amplitude_TextChanged(sender, e);
                ButtonAddPulses_Click(sender, e);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void NumericUpDownNumberOfPulses_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonAddPulses_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void NumericUpDownNumberOfPulses_Leave(object sender, EventArgs e)
        {
            ButtonAddPulses_Click(sender, e);
        }

        void Samples_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox == null)
                return;

            else if (textBox.Text == "")
            {
                textBox.Tag = null;

                if (textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthAnodic) && StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
                {
                    StimulusSequenceOptions.textboxPulseWidthCathodic.Text = "";
                    StimulusSequenceOptions.textboxPulseWidthCathodic.Tag = null;
                }
                else if (textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthCathodic) && StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
                {
                    StimulusSequenceOptions.textboxPulseWidthAnodic.Text = "";
                    StimulusSequenceOptions.textboxPulseWidthAnodic.Tag = null;
                }

                return;
            }

            if (double.TryParse(textBox.Text, out double result))
            {
                if (!GetSampleFromTime(result, out uint sampleTime))
                {
                    MessageBox.Show($"Warning: Value \"{result}\" is not valid.");
                    textBox.Text = "";
                    textBox.Tag = null;
                }
                else
                {
                    textBox.Text = GetTimeString(sampleTime);
                    textBox.Tag = sampleTime;

                    if (sampleTime == 0)
                    {
                        if (textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthAnodic) ||
                            textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthCathodic))
                        {
                            MessageBox.Show($"Warning: Value entered must be greater than {result}.");
                            textBox.Text = "";
                            textBox.Tag = null;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Unable to parse text. Please enter a valid value in milliseconds");
                textBox.Text = "";
                textBox.Tag = null;
            }

            if (textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthAnodic) && StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
            {
                StimulusSequenceOptions.textboxPulseWidthCathodic.Text = textBox.Text;
                StimulusSequenceOptions.textboxPulseWidthCathodic.Tag = textBox.Tag;
            }
            else if (textBox.Name == nameof(StimulusSequenceOptions.textboxPulseWidthCathodic) && StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
            {
                StimulusSequenceOptions.textboxPulseWidthAnodic.Text = textBox.Text;
                StimulusSequenceOptions.textboxPulseWidthAnodic.Tag = textBox.Tag;
            }

            ButtonAddPulses_Click(sender, e);
        }

        bool GetSampleFromTime(double value, out uint samples)
        {
            var ratio = value / SamplePeriodMilliSeconds;
            samples = (uint)Math.Round(ratio);

            return !(ratio > uint.MaxValue || ratio < uint.MinValue);
        }

        void UpdateAmplitudeTextBoxes(TextBox textBox, string text = "", byte? tag = null)
        {
            if (StimulusSequenceOptions.checkboxBiphasicSymmetrical.Checked)
            {
                StimulusSequenceOptions.textboxAmplitudeCathodic.Text = text;
                StimulusSequenceOptions.textboxAmplitudeCathodic.Tag = tag ?? null;

                StimulusSequenceOptions.textboxAmplitudeAnodic.Text = text;
                StimulusSequenceOptions.textboxAmplitudeAnodic.Tag = tag ?? null;

                if (textBox.Name == nameof(StimulusSequenceOptions.textboxAmplitudeAnodicRequested))
                {
                    StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text = StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text;
                    StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag = StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag;
                }
                else if (textBox.Name == nameof(StimulusSequenceOptions.textboxAmplitudeCathodicRequested))
                {
                    StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Text = StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Text;
                    StimulusSequenceOptions.textboxAmplitudeAnodicRequested.Tag = StimulusSequenceOptions.textboxAmplitudeCathodicRequested.Tag;
                }
            }
            else
            {
                if (textBox.Name == nameof(StimulusSequenceOptions.textboxAmplitudeAnodicRequested))
                {
                    StimulusSequenceOptions.textboxAmplitudeAnodic.Text = text;
                    StimulusSequenceOptions.textboxAmplitudeAnodic.Tag = tag ?? null;
                }
                else if (textBox.Name == nameof(StimulusSequenceOptions.textboxAmplitudeCathodicRequested))
                {
                    StimulusSequenceOptions.textboxAmplitudeCathodic.Text = text;
                    StimulusSequenceOptions.textboxAmplitudeCathodic.Tag = tag ?? null;
                }
            }

        }

        void Amplitude_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
            {
                UpdateAmplitudeTextBoxes(textBox);
                textBox.Tag = null;

                return;
            }

            if (double.TryParse(textBox.Text, out double result))
            {
                if (!UpdateStepSizeFromAmplitude(result))
                {
                    string text = "0";
                    byte tag = 0;
                    textBox.Text = "";

                    UpdateAmplitudeTextBoxes(textBox, text, tag);

                    return;
                }

                textBox.Tag = result;

                GetSampleFromAmplitude(result, out byte sampleAmplitude);

                UpdateAmplitudeTextBoxes(textBox, GetAmplitudeString(sampleAmplitude), sampleAmplitude);
            }
            else
            {
                MessageBox.Show("Unable to parse text. Please enter a valid value in milliamps.", "Invalid Requested Amplitude");
                textBox.Text = "";
                textBox.Tag = null;
            }
        }

        /// <summary>
        /// Updates the current step size based on the given amplitude
        /// </summary>
        /// <param name="amplitude">New amplitude value.</param>
        /// <returns>True if the amplitude is a valid value and the step size has been updated. False if something went wrong, the step size has not been changed.</returns>
        bool UpdateStepSizeFromAmplitude(double amplitude)
        {
            const string InvalidAmplitudeString = "Invalid Amplitude";

            if (amplitude < 0)
            {
                MessageBox.Show("Warning: Amplitude cannot be a negative value.", InvalidAmplitudeString);
                return false;
            }

            var stepSizes = Enum.GetValues(typeof(Rhs2116StepSize)).Cast<Rhs2116StepSize>();
            var validStepSizes = stepSizes.Where(stepSize => IsValidNumberOfSteps(GetNumberOfSteps(amplitude, stepSize)));

            if (validStepSizes.Count() == 1)
            {
                StepSize = validStepSizes.First();
                StimulusSequenceOptions.textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

                return true;
            }
            else if (validStepSizes.Count() == 0)
            {
                if (amplitude > Rhs2116StimulusSequence.GetStepSizeuA(Rhs2116StepSize.Step10000nA) * 255)
                {
                    MessageBox.Show($"Warning: Requested amplitude of {amplitude} µA is too large. The maximum value available is " +
                        $"{Rhs2116StimulusSequence.GetStepSizeuA(Rhs2116StepSize.Step10000nA) * 255}.");
                }

                return false;
            }

            StepSize = Rhs2116StimulusSequence.GetStepSizeWithMinError(validStepSizes, Sequence.Stimuli, amplitude, Sequence.CurrentStepSize);
            StimulusSequenceOptions.textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            return true;
        }

        bool IsValidNumberOfSteps(int numberOfSteps)
        {
            return numberOfSteps > 0 && numberOfSteps <= 255;
        }

        int GetNumberOfSteps(double amplitude, Rhs2116StepSize stepSize)
        {
            return (int)(amplitude / Rhs2116StimulusSequence.GetStepSizeuA(stepSize));
        }

        internal override void SerializeStimulusSequence(string fileName)
        {
            JsonHelper.SerializeObject(Sequence, fileName);
        }

        internal override bool IsSequenceValid()
        {
            return Sequence.Valid && Sequence.FitsInHardware;
        }

        internal override void DeserializeStimulusSequence(string fileName)
        {
            if (JsonHelper.DeserializeString(File.ReadAllText(fileName), typeof(Rhs2116StimulusSequencePair)) is Rhs2116StimulusSequencePair newSequence && newSequence.Stimuli.Length == 32)
            {
                if (newSequence == new Rhs2116StimulusSequencePair())
                {
                    var result = MessageBox.Show("The stimulus sequence loaded does not have any configuration settings applied. " +
                        "This could be because the file did not have the correct format. If this sequence is loaded, it will clear out " +
                        "all current settings. Continue?", "No Settings Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                Trigger.StimulusSequence = newSequence;

                for (int i = 0; i < newSequence.Stimuli.Length; i++)
                {
                    RequestedAnodicAmplitudeuA[i] = newSequence.Stimuli[i].AnodicAmplitudeSteps * newSequence.CurrentStepSizeuA;
                    RequestedCathodicAmplitudeuA[i] = newSequence.Stimuli[i].CathodicAmplitudeSteps * newSequence.CurrentStepSizeuA;
                }

                if (!newSequence.Valid)
                {
                    MessageBox.Show("Warning: Invalid stimuli found in the recently opened file. Check all values to ensure they are what is expected.",
                        "Invalid Stimuli", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Incoming file is not valid. Check file for validity.");
            }
        }

        internal void SetTableDataSource()
        {
            dataGridViewStimulusTable.DataSource = Trigger?.StimulusSequence.Stimuli;
        }

        private void DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            AddDeviceChannelIndexToGridRow();
        }

        private void AddDeviceChannelIndexToGridRow()
        {
            if (ChannelDialog == null || ChannelDialog.ProbeGroup.NumberOfContacts != 32)
                return;

            var deviceChannelIndices = ChannelDialog.ProbeGroup.GetDeviceChannelIndices();

            for (int i = 0; i < deviceChannelIndices.Count(); i++)
            {
                var index = deviceChannelIndices.ElementAt(i);

                if (index != -1)
                {
                    dataGridViewStimulusTable.Rows[index].HeaderCell.Value = index.ToString();
                }
            }
        }
    }
}
