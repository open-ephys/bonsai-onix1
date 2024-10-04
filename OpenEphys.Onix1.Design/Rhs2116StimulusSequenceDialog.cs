using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.IO;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Rhs2116StimulusSequence"/>.
    /// </summary>
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        internal Rhs2116StimulusSequencePair Sequence { get; set; }

        private Rhs2116StepSize StepSize { get; set; }

        internal readonly Rhs2116ChannelConfigurationDialog ChannelDialog;

        private const double SamplePeriodMicroSeconds = 1e6 / 30.1932367151e3;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="probeGroup"></param>
        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequencePair sequence, Rhs2116ProbeGroup probeGroup)
        {
            InitializeComponent();
            Shown += FormShown;

            Sequence = new Rhs2116StimulusSequencePair(sequence);
            StepSize = Sequence.CurrentStepSize;

            ChannelDialog = new(probeGroup)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            panelProbe.Controls.Add(ChannelDialog);
            this.AddMenuItemsFromDialogToFileOption(ChannelDialog, "Channel Configuration");

            ChannelDialog.OnSelect += OnSelect;
            ChannelDialog.OnZoom += OnZoom;

            ChannelDialog.Show();

            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            if (probeGroup.NumberOfContacts != 32)
            {
                throw new ArgumentException($"Probe group is not valid: 32 channels were expected, there are {probeGroup.NumberOfContacts} instead.");
            }

            InitializeZedGraphWaveform();
            DrawStimulusWaveform();

            zedGraphWaveform.ZoomEvent += OnZoom_Waveform;

            dataGridViewStimulusTable.DataSource = Sequence.Stimuli;
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 2;

                menuStrip.Visible = false;
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (TopLevel)
            {
                if (CanCloseForm(Sequence, out DialogResult result))
                {
                    DialogResult = result;
                    Close();
                }
            }
        }

        /// <summary>
        /// Checks the given stimulus sequence for validity, and confirms if the user wants to close the form
        /// </summary>
        /// <param name="sequence">Rhs2116 Stimulus Sequence</param>
        /// <param name="result">DialogResult, used to set the DialogResult of the form before closing</param>
        /// <returns></returns>
        public static bool CanCloseForm(Rhs2116StimulusSequencePair sequence, out DialogResult result)
        {
            if (sequence != null)
            {
                if (!sequence.Valid)
                {
                    DialogResult resultContinue = MessageBox.Show("Warning: Stimulus sequence is not valid. " +
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

        private void OnSelect(object sender, EventArgs e)
        {
            DrawStimulusWaveform();
        }

        private void OnZoom(object sender, EventArgs e)
        {
            ChannelDialog.UpdateFontSize();
        }

        private void OnZoom_Waveform(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            if (newState.Type == ZoomState.StateType.WheelZoom)
            {
                if (CheckZoomBoundaries(sender))
                {
                    CenterAxesOnCursor(sender);
                }
                else
                {
                    sender.ZoomOut(sender.GraphPane);
                }
            }
            else if (newState.Type == ZoomState.StateType.Zoom)
            {
                CheckZoomBoundaries(sender);
            }
        }

        private void HighlightInvalidContacts()
        {
            var contactObjects = ChannelDialog.zedGraphChannels.GraphPane.GraphObjList
                                 .OfType<BoxObj>()
                                 .Where(c => c is not PolyObj);

            foreach (var contact in contactObjects)
            {
                if (contact.Tag is ContactTag contactTag)
                {
                    var index = contactTag.ProbeIndex == 0
                                ? contactTag.ContactIndex
                                : contactTag.ContactIndex + ChannelDialog.ProbeGroup.Probes
                                                            .Take(contactTag.ProbeIndex)
                                                            .Aggregate(0, (total, next) => total + next.NumberOfContacts);

                    if (!Sequence.Stimuli[index].IsValid())
                    {
                        contact.Fill.Color = Color.Red;

                        if (contact.Border.Color != ChannelDialog.SelectedContactBorder)
                            contact.Border.Color = Color.Red;
                    }
                }
            }

            ChannelDialog.RefreshZedGraph();
        }

        private void DrawStimulusWaveform()
        {
            bool plotAllContacts = ChannelDialog.SelectedContacts.All(x => x == false);

            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            double peakToPeak = (Sequence.MaximumPeakToPeakAmplitudeSteps > 0
                                ? Sequence.CurrentStepSizeuA * Sequence.MaximumPeakToPeakAmplitudeSteps
                                : Sequence.CurrentStepSizeuA * 1) * 1.1;

            ZoomInBoundaryY = 2 * peakToPeak;

            var stimuli = Sequence.Stimuli;

            zedGraphWaveform.GraphPane.XAxis.Scale.Max = (Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1) * SamplePeriodMicroSeconds;
            zedGraphWaveform.GraphPane.XAxis.Scale.Min = -zedGraphWaveform.GraphPane.XAxis.Scale.Max * 0.03;
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = 0;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak * (stimuli.Length + 1);

            var contactTextLocation = zedGraphWaveform.GraphPane.XAxis.Scale.Min / 2;

            for (int i = 0; i < stimuli.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i] || plotAllContacts)
                {
                    PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], peakToPeak * (i + 0.5));

                    Color color;
                    if (stimuli[i].IsValid())
                    {
                        color = Color.CornflowerBlue;
                    }
                    else
                    {
                        color = Color.Red;
                    }

                    var curve = zedGraphWaveform.GraphPane.AddCurve("", pointPairs, color, SymbolType.None);

                    curve.Label.IsVisible = false;
                    curve.Line.Width = 3;

                    if (pointPairs.Count == 3 && pointPairs[0].X == pointPairs[2].X) continue; // NB: Empty line, skip drawing the contact number for cleanliness

                    TextObj contactNumber = new(i.ToString(), contactTextLocation, curve.Points[0].Y);
                    contactNumber.FontSpec.Size = 16;
                    contactNumber.FontSpec.Border.IsVisible = false;
                    contactNumber.FontSpec.Fill.IsVisible = false;
                    contactNumber.IsClippedToChartRect = true;

                    zedGraphWaveform.GraphPane.GraphObjList.Add(contactNumber);
                }
            }

            HighlightInvalidContacts();

            SetStatusValidity();
            SetPercentOfSlotsUsed();

            dataGridViewStimulusTable.Refresh();

            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();

            SetZoomOutBoundaries(zedGraphWaveform);
        }

        private PointPairList CreateStimulusWaveform(Rhs2116Stimulus stimulus, double yOffset)
        {
            PointPairList points = new()
            {
                { 0, yOffset },
                { stimulus.DelaySamples * SamplePeriodMicroSeconds, yOffset }
            };

            for (int i = 0; i < stimulus.NumberOfStimuli; i++)
            {
                double amplitude = (stimulus.AnodicFirst ? stimulus.AnodicAmplitudeSteps : -stimulus.CathodicAmplitudeSteps) * Sequence.CurrentStepSizeuA + yOffset;
                double width = (stimulus.AnodicFirst ? stimulus.AnodicWidthSamples : stimulus.CathodicWidthSamples) * SamplePeriodMicroSeconds;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.DwellSamples * SamplePeriodMicroSeconds, yOffset);

                amplitude = (stimulus.AnodicFirst ? -stimulus.CathodicAmplitudeSteps : stimulus.AnodicAmplitudeSteps) * Sequence.CurrentStepSizeuA + yOffset;
                width = (stimulus.AnodicFirst ? stimulus.CathodicWidthSamples : stimulus.AnodicWidthSamples) * SamplePeriodMicroSeconds;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.InterStimulusIntervalSamples * SamplePeriodMicroSeconds, yOffset);
            }

            points.Add(Sequence.SequenceLengthSamples * SamplePeriodMicroSeconds, yOffset);

            return points;
        }

        private void InitializeZedGraphWaveform()
        {
            zedGraphWaveform.IsZoomOnMouseCenter = true;

            zedGraphWaveform.GraphPane.Title.IsVisible = false;
            zedGraphWaveform.GraphPane.TitleGap = 0;
            zedGraphWaveform.GraphPane.Border.IsVisible = false;
            zedGraphWaveform.GraphPane.IsFontsScaled = false;

            zedGraphWaveform.GraphPane.YAxis.MajorGrid.IsZeroLine = false;

            zedGraphWaveform.GraphPane.XAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.XAxis.MinorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MinorTic.IsAllTics = false;

            zedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;

            zedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [μs]";
            zedGraphWaveform.GraphPane.YAxis.Title.Text = "Amplitude [μA]";

            zedGraphWaveform.IsAutoScrollRange = true;

            zedGraphWaveform.ZoomStepFraction = 0.5;
        }

        private double ZoomOutBoundaryLeft = default;
        private double ZoomOutBoundaryRight = default;
        private double ZoomOutBoundaryBottom = default;
        private double ZoomOutBoundaryTop = default;
        private readonly double? ZoomInBoundaryX = null;
        private double? ZoomInBoundaryY = 20;

        private void SetZoomOutBoundaries(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale);

            var margin = Math.Min(rangeX, rangeY) * 0.1;

            ZoomOutBoundaryLeft = zedGraphControl.GraphPane.XAxis.Scale.Min - margin;
            ZoomOutBoundaryBottom = zedGraphControl.GraphPane.YAxis.Scale.Min - margin;
            ZoomOutBoundaryRight = zedGraphControl.GraphPane.XAxis.Scale.Max + margin;
            ZoomOutBoundaryTop = zedGraphControl.GraphPane.YAxis.Scale.Max + margin;
        }

        internal static double CalculateScaleRange(Scale scale)
        {
            return scale.Max - scale.Min;
        }

        private static PointD TransformPixelsToCoordinates(Point pixels, GraphPane graphPane)
        {
            graphPane.ReverseTransform(pixels, out double x, out double y);
            y += CalculateScaleRange(graphPane.YAxis.Scale) * 0.1;

            return new PointD(x, y);
        }

        private void CenterAxesOnCursor(ZedGraphControl zedGraphControl)
        {
            if ((zedGraphControl.GraphPane.XAxis.Scale.Min == ZoomOutBoundaryLeft &&
                zedGraphControl.GraphPane.XAxis.Scale.Max == ZoomOutBoundaryRight &&
                zedGraphControl.GraphPane.YAxis.Scale.Min == ZoomOutBoundaryBottom &&
                zedGraphControl.GraphPane.YAxis.Scale.Max == ZoomOutBoundaryTop) ||
                ZoomInBoundaryX.HasValue && CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) == ZoomInBoundaryX.Value ||
                ZoomInBoundaryY.HasValue && CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) == ZoomInBoundaryY.Value)
            {
                return;
            }

            var mouseClientPosition = PointToClient(Cursor.Position);
            mouseClientPosition.X -= (zedGraphControl.Parent.Width - zedGraphControl.Width) / 2;
            mouseClientPosition.Y += (zedGraphControl.Parent.Height - zedGraphControl.Height) / 2;

            var currentMousePosition = TransformPixelsToCoordinates(mouseClientPosition, zedGraphControl.GraphPane);
            //currentMousePosition.Y += ZoomInBoundaryY.Value;

            var centerX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) / 2 + zedGraphControl.GraphPane.XAxis.Scale.Min;

            var centerY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) / 2 + zedGraphControl.GraphPane.YAxis.Scale.Min;

            var diffX = centerX - currentMousePosition.X;
            var diffY = centerY - currentMousePosition.Y;

            zedGraphControl.GraphPane.XAxis.Scale.Min += diffX;
            zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;

            zedGraphControl.GraphPane.YAxis.Scale.Min += diffY;
            zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
        }

        /// <summary>
        /// Checks if the <see cref="ZedGraphControl"/> is too zoomed in or out. If the graph is too zoomed in,
        /// reset the boundaries to match <see cref="ZoomInBoundaryX"/> and <see cref="ZoomInBoundaryY"/>. If the graph is too zoomed out,
        /// reset the boundaries to match the automatically generated boundaries based on the size of the waveforms.
        /// </summary>
        /// <param name="zedGraphControl">A <see cref="ZedGraphControl"/> object.</param>
        /// <returns>True if the zoom boundary has been correctly handled, False if the previous zoom state should be reinstated.</returns>
        private bool CheckZoomBoundaries(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale);

            if ((ZoomInBoundaryX.HasValue && rangeX < ZoomInBoundaryX) || (ZoomInBoundaryY.HasValue && rangeY < ZoomInBoundaryY))
            {
                if ((ZoomInBoundaryX.HasValue && rangeX / ZoomInBoundaryX == zedGraphControl.ZoomStepFraction) ||
                    (ZoomInBoundaryY.HasValue && rangeY / ZoomInBoundaryY == zedGraphControl.ZoomStepFraction))
                    return false;
                else
                {
                    if (ZoomInBoundaryX.HasValue && ZoomInBoundaryX.Value > 0)
                    {
                        var diffX = (ZoomInBoundaryX.Value - rangeX) / 2;
                        zedGraphControl.GraphPane.XAxis.Scale.Min -= diffX;
                        zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;
                    }

                    if (ZoomInBoundaryY.HasValue && ZoomInBoundaryY.Value > 0)
                    {
                        var diffY = (ZoomInBoundaryY.Value - rangeY) / 2;
                        zedGraphControl.GraphPane.YAxis.Scale.Min -= diffY;
                        zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
                    }

                    return true;
                }
            }
            else
            {
                if (zedGraphControl.GraphPane.XAxis.Scale.Min < ZoomOutBoundaryLeft && zedGraphControl.GraphPane.XAxis.Scale.Max > ZoomOutBoundaryRight ||
                    CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) >= ZoomOutBoundaryRight - ZoomOutBoundaryLeft)
                {
                    zedGraphControl.GraphPane.XAxis.Scale.Min = ZoomOutBoundaryLeft;
                    zedGraphControl.GraphPane.XAxis.Scale.Max = ZoomOutBoundaryRight;
                }

                if (zedGraphControl.GraphPane.YAxis.Scale.Min < ZoomOutBoundaryBottom && zedGraphControl.GraphPane.YAxis.Scale.Max > ZoomOutBoundaryTop ||
                    CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) >= ZoomOutBoundaryTop - ZoomOutBoundaryBottom)
                {
                    zedGraphControl.GraphPane.YAxis.Scale.Min = ZoomOutBoundaryBottom;
                    zedGraphControl.GraphPane.YAxis.Scale.Max = ZoomOutBoundaryTop;
                }

                return true;
            }
        }

        private void SetStatusValidity()
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
                                                 .First();

                    toolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                    toolStripStatusIsValid.Text = string.Format("Invalid sequence - Contact {0}, Reason: {1}", reason.ind, reason.reason);
                }
            }
        }

        private void SetPercentOfSlotsUsed()
        {
            toolStripStatusSlotsUsed.Text = string.Format("{0, 0:P1} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
        }

        private void ButtonAddPulses_Click(object sender, EventArgs e)
        {
            if (ChannelDialog.SelectedContacts.All(x => x))
            {
                DialogResult result = MessageBox.Show("Caution: All channels are currently selected, and all " +
                    "settings will be applied to all channels if you continue. Press Okay to add pulse settings to all channels, or Cancel to keep them as is",
                    "Set all channel settings?", MessageBoxButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (ChannelDialog.SelectedContacts.All(x => x == false))
            {
                MessageBox.Show("No contacts selected. Please select contact(s) before trying to add pulses.");
                return;
            }

            if (StepSize != Sequence.CurrentStepSize)
            {
                var stimuli = Sequence.Stimuli
                              .Select((s, ind) => { return (Index: ind, Stimulus: s); })
                              .Where(s => s.Stimulus.IsValid() && (s.Stimulus.AnodicAmplitudeSteps != 0 || s.Stimulus.CathodicAmplitudeSteps != 0) && !ChannelDialog.SelectedContacts[s.Index])
                              .Select(s =>
                              {
                                  var currentAnodicAmplitude = GetAmplitudeFromSample(s.Stimulus.AnodicAmplitudeSteps, Sequence.CurrentStepSize);
                                  var currentCathodicAmplitude = GetAmplitudeFromSample(s.Stimulus.CathodicAmplitudeSteps, Sequence.CurrentStepSize);

                                  var validAnodicAmplitude = GetSampleFromAmplitude(currentAnodicAmplitude, out var newAnodicSamples);
                                  var validCathodicAmplitude = GetSampleFromAmplitude(currentAnodicAmplitude, out var newCathodicSamples);

                                  return (ValidAmplitudes: validAnodicAmplitude && newAnodicSamples != 0 && validCathodicAmplitude && newCathodicSamples != 0,
                                          s.Index,
                                          NewAnodicSamples: newAnodicSamples,
                                          NewCathodicSamples: newCathodicSamples);
                              });

                foreach (var (ValidAmplitudes, Index, NewAnodicSamples, NewCathodicSamples) in stimuli)
                {
                    if (ValidAmplitudes)
                    {
                        Sequence.Stimuli[Index].AnodicAmplitudeSteps = NewAnodicSamples;
                        Sequence.Stimuli[Index].CathodicAmplitudeSteps = NewCathodicSamples;
                    }
                    else
                    {
                        var result = MessageBox.Show($"The stimulus for channel {Index} is incompatible with the newly selected step size. " +
                        "Moving forward will remove the stimulus for this channel. " +
                        "Press Ok to remove the stimulus, or Cancel to keep the current parameters.",
                        "Invalid Stimulus", MessageBoxButtons.OKCancel);

                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }

                        Sequence.Stimuli[Index].Clear();
                    }
                }
            }

            for (int i = 0; i < ChannelDialog.SelectedContacts.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i])
                {
                    if (textboxDelay.Tag == null)
                    {
                        MessageBox.Show("Unable to parse delay.");
                        return;
                    }

                    if (textboxAmplitudeAnodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse anodic amplitude.");
                        return;
                    }

                    if (textboxPulseWidthAnodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse anodic pulse width.");
                        return;
                    }

                    if (textboxInterPulseInterval.Tag == null)
                    {
                        MessageBox.Show("Unable to parse inter-pulse interval.");
                        return;
                    }

                    if (textboxAmplitudeCathodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse cathodic amplitude.");
                        return;
                    }

                    if (textboxPulseWidthCathodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse cathodic pulse width.");
                        return;
                    }

                    if (textboxInterStimulusInterval.Tag == null)
                    {
                        MessageBox.Show("Unable to parse inter-stimulus interval.");
                        return;
                    }

                    if (!uint.TryParse(textboxNumberOfStimuli.Text, out uint numberOfStimuliValue))
                    {
                        MessageBox.Show("Unable to parse number of stimuli.");
                        return;
                    }

                    Sequence.Stimuli[i].DelaySamples = (uint)textboxDelay.Tag;

                    Sequence.Stimuli[i].AnodicAmplitudeSteps = (byte)textboxAmplitudeAnodic.Tag;
                    Sequence.Stimuli[i].AnodicWidthSamples = (uint)textboxPulseWidthAnodic.Tag;

                    Sequence.Stimuli[i].CathodicAmplitudeSteps = (byte)textboxAmplitudeCathodic.Tag;
                    Sequence.Stimuli[i].CathodicWidthSamples = (uint)textboxPulseWidthCathodic.Tag;

                    Sequence.Stimuli[i].DwellSamples = (uint)textboxInterPulseInterval.Tag;

                    Sequence.Stimuli[i].InterStimulusIntervalSamples = (uint)textboxInterStimulusInterval.Tag;

                    Sequence.Stimuli[i].NumberOfStimuli = numberOfStimuliValue;

                    Sequence.Stimuli[i].AnodicFirst = checkBoxAnodicFirst.Checked;
                }
            }

            Sequence.CurrentStepSize = StepSize;

            ChannelDialog.HighlightEnabledContacts();
            DrawStimulusWaveform();
        }

        private void ParameterKeyPress_Time(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                Samples_TextChanged(sender, e);
            }
        }

        private void ParameterKeyPress_Amplitude(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                Amplitude_TextChanged(sender, e);
            }
        }

        private void DataGridViewStimulusTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewStimulusTable.BindingContext[dataGridViewStimulusTable.DataSource].EndCurrentEdit();
            AddDeviceChannelIndexToGridRow();
            ChannelDialog.HighlightEnabledContacts();
            DrawStimulusWaveform();
        }

        private void DataGridViewStimulusTable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
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

        private string GetStepSizeStringuA(Rhs2116StepSize stepSize)
        {
            return Rhs2116StimulusSequence.GetStepSizeuA(stepSize).ToString() + " µA";
        }

        private double GetStepSizeuA(Rhs2116StepSize stepSize)
        {
            return stepSize switch
            {
                Rhs2116StepSize.Step10nA => 0.01,
                Rhs2116StepSize.Step20nA => 0.02,
                Rhs2116StepSize.Step50nA => 0.05,
                Rhs2116StepSize.Step100nA => 0.1,
                Rhs2116StepSize.Step200nA => 0.2,
                Rhs2116StepSize.Step500nA => 0.5,
                Rhs2116StepSize.Step1000nA => 1.0,
                Rhs2116StepSize.Step2000nA => 2.0,
                Rhs2116StepSize.Step5000nA => 5.0,
                Rhs2116StepSize.Step10000nA => 10.0,
                _ => throw new ArgumentException("Invalid stimulus step size selection."),
            };
        }

        private string GetAmplitudeString(byte amplitude)
        {
            string format = StepSize switch
            {
                Rhs2116StepSize.Step10nA or Rhs2116StepSize.Step20nA or Rhs2116StepSize.Step50nA => "{0:F2}",
                Rhs2116StepSize.Step100nA or Rhs2116StepSize.Step200nA or Rhs2116StepSize.Step500nA => "{0:F1}",
                Rhs2116StepSize.Step1000nA or Rhs2116StepSize.Step2000nA or Rhs2116StepSize.Step5000nA or Rhs2116StepSize.Step10000nA => "{0:F0}",
                _ => "{0:F3}",
            };
            return string.Format(format, GetAmplitudeFromSample(amplitude));
        }

        private string GetTimeString(uint time)
        {
            return string.Format("{0:F2}", GetTimeFromSample(time));
        }

        private void Samples_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox == null)
                return;

            else if (textBox.Text == "")
            {
                textBox.Tag = null;
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
                        if (textBox.Name == nameof(textboxPulseWidthAnodic) ||
                            textBox.Name == nameof(textboxPulseWidthCathodic))
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

            if (textBox.Name == nameof(textboxPulseWidthAnodic) && checkboxBiphasicSymmetrical.Checked)
            {
                textboxPulseWidthCathodic.Text = textBox.Text;
                textboxPulseWidthCathodic.Tag = textBox.Tag;
            }
            else if (textBox.Name == nameof(textboxPulseWidthCathodic) && checkboxBiphasicSymmetrical.Checked)
            {
                textboxPulseWidthAnodic.Text = textBox.Text;
                textboxPulseWidthAnodic.Tag = textBox.Tag;
            }
        }

        private bool GetSampleFromTime(double value, out uint samples)
        {
            var ratio = value * 1e3 / SamplePeriodMicroSeconds;
            samples = (uint)Math.Round(ratio);

            return !(ratio > uint.MaxValue || ratio < uint.MinValue);
        }

        /// <summary>
        /// Get the number of samples needed at the current step size to represent a given amplitude.
        /// </summary>
        /// <param name="value">Double value defining the amplitude in microamps.</param>
        /// <param name="samples">Output returning the number of samples as a byte.</param>
        /// <returns>Returns true if the number of samples is a valid byte value (between 0 and 255). Returns false if the number of samples cannot be represented in byte format.</returns>
        private bool GetSampleFromAmplitude(double value, out byte samples)
        {
            var ratio = value / Rhs2116StimulusSequence.GetStepSizeuA(StepSize);
            samples = (byte)Math.Round(ratio);

            return !(ratio > byte.MaxValue || ratio < 0);
        }

        private double GetTimeFromSample(uint value)
        {
            return value * SamplePeriodMicroSeconds / 1e3;
        }

        private double GetAmplitudeFromSample(byte value)
        {
            return GetAmplitudeFromSample(value, StepSize);
        }

        private double GetAmplitudeFromSample(byte value, Rhs2116StepSize stepSize)
        {
            return value * Rhs2116StimulusSequence.GetStepSizeuA(stepSize);
        }

        private void Amplitude_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
            {
                textBox.Tag = null;
                return;
            }

            if (double.TryParse(textBox.Text, out double result))
            {
                if (!UpdateStepSizeFromAmplitude(result))
                {
                    textBox.Text = "";
                    textBox.Tag = null;
                    return;
                }

                GetSampleFromAmplitude(result, out byte sampleAmplitude);

                textBox.Text = GetAmplitudeString(sampleAmplitude);
                textBox.Tag = sampleAmplitude;
            }
            else
            {
                MessageBox.Show("Unable to parse text. Please enter a valid value in milliamps");
                textBox.Text = "";
                textBox.Tag = null;
            }

            if (checkboxBiphasicSymmetrical.Checked)
            {
                if (textBox.Name == nameof(textboxAmplitudeAnodic))
                {
                    textboxAmplitudeCathodic.Text = textBox.Text;
                    textboxAmplitudeCathodic.Tag = textBox.Tag;
                }
                else if (textBox.Name == nameof(textboxAmplitudeCathodic))
                {
                    textboxAmplitudeAnodic.Text = textBox.Text;
                    textboxAmplitudeAnodic.Tag = textBox.Tag;
                }
            }
            else
            {
                if (textBox.Name == nameof(textboxAmplitudeAnodic) && double.TryParse(textboxAmplitudeCathodic.Text, out var cathodicAmplitude))
                {
                    if (!GetSampleFromAmplitude(cathodicAmplitude, out var samples) || samples == 0)
                    {
                        MessageBox.Show("Invalid amplitude chosen for the anodic pulse. The step-size required " +
                            "for this amplitude is incompatible with the step-size required for the cathodic pulse.", "Invalid Anodic Amplitude");
                        textBox.Text = "";
                        textBox.Tag = null;

                        textboxAmplitudeCathodic.Text = "";
                        textboxAmplitudeCathodic.Tag = null;
                        return;
                    }
                    else
                    {
                        textboxAmplitudeCathodic.Text = GetAmplitudeString(samples);
                        textboxAmplitudeCathodic.Tag = samples;
                    }
                }
                else if (textBox.Name == nameof(textboxAmplitudeCathodic) && double.TryParse(textboxAmplitudeAnodic.Text, out var anodicAmplitude))
                {
                    if (!GetSampleFromAmplitude(anodicAmplitude, out var samples) || samples == 0)
                    {
                        MessageBox.Show("Invalid amplitude chosen for the cathodic pulse. The step-size required " +
                            "for this amplitude is incompatible with the step-size required for the anodic pulse.", "Invalid Cathodic Amplitude");
                        textBox.Text = "";
                        textBox.Tag = null;

                        textboxAmplitudeAnodic.Text = "";
                        textboxAmplitudeAnodic.Tag = null;
                        return;
                    }
                    else
                    {
                        textboxAmplitudeAnodic.Text = GetAmplitudeString(samples);
                        textboxAmplitudeAnodic.Tag = samples;
                    }
                }
            }
        }

        private bool UpdateStepSizeFromAmplitude(double amplitude)
        {
            const double minAmplitudeuA = 0.01; // NB: Minimum possible amplitude is 10 nA (0.01 µA)
            const double maxAmplitudeuA = 2550; // NB: Maximum possible amplitude is 2550000 nA (2550 µA)

            const string InvalidAmplitudeString = "Invalid Amplitude";

            bool result = true;

            if (amplitude > maxAmplitudeuA)
            {
                MessageBox.Show($"Warning: Amplitude is too high. Amplitude must be less than {maxAmplitudeuA} µA.", InvalidAmplitudeString);
                result = false;
            }
            else if (amplitude < 0)
            {
                MessageBox.Show("Warning: Amplitude cannot be a negative value.", InvalidAmplitudeString);
                result = false;
            }
            else if (amplitude < minAmplitudeuA && amplitude > 0)
            {
                MessageBox.Show($"Amplitude is too small to be resolved. Amplitude must be at least {minAmplitudeuA} µA.", InvalidAmplitudeString);
            }

            // NB: Update step size to a value that supports the requested amplitude.
            StepSize = Enum.GetValues(typeof(Rhs2116StepSize))
                                       .Cast<Rhs2116StepSize>()
                                       .Where(s =>
                                       {
                                           var numSteps = (int)(amplitude / GetStepSizeuA(s));
                                           return numSteps > 0 && numSteps <= 255;
                                       })
                                       .FirstOrDefault();

            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            return result;
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxBiphasicSymmetrical.Checked)
            {
                if (checkBoxAnodicFirst.Checked)
                {
                    groupBoxCathode.Visible = false;
                    groupBoxAnode.Visible = true;
                }
                else
                {
                    groupBoxCathode.Visible = true;
                    groupBoxAnode.Visible = false;
                }
            }
            else
            {
                groupBoxCathode.Visible = true;
                groupBoxAnode.Visible = true;
            }
        }

        private void ButtonClearPulses_Click(object sender, EventArgs e)
        {
            if (ChannelDialog.SelectedContacts.All(x => x == false) || ChannelDialog.SelectedContacts.All(x => x == true))
            {
                DialogResult result = MessageBox.Show("Caution: All channels are currently selected, and all " +
                    "settings will be cleared if you continue. Press Okay to clear all pulse settings, or Cancel to keep them",
                    "Remove all channel settings?", MessageBoxButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            var clearAllContacts = ChannelDialog.SelectedContacts.All(x => x == false);

            for (int i = 0; i < ChannelDialog.SelectedContacts.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i] || clearAllContacts)
                {
                    Sequence.Stimuli[i].Clear();
                }
            }

            ChannelDialog.HighlightEnabledContacts();
            DrawStimulusWaveform();
            ChannelDialog.RefreshZedGraph();
        }

        private void ButtonReadPulses_Click(object sender, EventArgs e)
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

            if (Sequence.Stimuli[index].NumberOfStimuli == 0 || !Sequence.Stimuli[index].IsValid())
                return;

            if (Sequence.Stimuli[index].AnodicAmplitudeSteps == Sequence.Stimuli[index].CathodicAmplitudeSteps &&
                Sequence.Stimuli[index].AnodicWidthSamples == Sequence.Stimuli[index].CathodicWidthSamples)
            {
                checkboxBiphasicSymmetrical.Checked = true;
            }
            else
            {
                checkboxBiphasicSymmetrical.Checked = false;
            }

            checkBoxAnodicFirst.Checked = Sequence.Stimuli[index].AnodicFirst;

            Checkbox_CheckedChanged(checkboxBiphasicSymmetrical, e);

            textboxDelay.Text = GetTimeString(Sequence.Stimuli[index].DelaySamples); Samples_TextChanged(textboxDelay, e);
            textboxAmplitudeAnodic.Text = GetAmplitudeString(Sequence.Stimuli[index].AnodicAmplitudeSteps); Amplitude_TextChanged(textboxAmplitudeAnodic, e);
            textboxPulseWidthAnodic.Text = GetTimeString(Sequence.Stimuli[index].AnodicWidthSamples); Samples_TextChanged(textboxPulseWidthAnodic, e);
            textboxAmplitudeCathodic.Text = GetAmplitudeString(Sequence.Stimuli[index].CathodicAmplitudeSteps); Amplitude_TextChanged(textboxAmplitudeCathodic, e);
            textboxPulseWidthCathodic.Text = GetTimeString(Sequence.Stimuli[index].CathodicWidthSamples); Samples_TextChanged(textboxPulseWidthCathodic, e);
            textboxInterPulseInterval.Text = GetTimeString(Sequence.Stimuli[index].DwellSamples); Samples_TextChanged(textboxInterPulseInterval, e);
            textboxInterStimulusInterval.Text = GetTimeString(Sequence.Stimuli[index].InterStimulusIntervalSamples); Samples_TextChanged(textboxInterStimulusInterval, e);
            textboxNumberOfStimuli.Text = Sequence.Stimuli[index].NumberOfStimuli.ToString();
        }

        private void MenuItemSaveFile_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "Stimulus Sequence Files (*.json)|*.json";
            sfd.FilterIndex = 1;
            sfd.Title = "Choose where to save the stimulus sequence file";
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                DesignHelper.SerializeObject(Sequence, sfd.FileName);
            }
        }

        private void MenuItemLoadFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new();

            ofd.Filter = "Stimulus Sequence Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose saved stimulus sequence file";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(ofd.FileName))
                {
                    MessageBox.Show("File does not exist.");
                    return;
                }

                var sequence = DesignHelper.DeserializeString<Rhs2116StimulusSequencePair>(File.ReadAllText(ofd.FileName));

                if (sequence != null && sequence.Stimuli.Length == 32)
                {
                    Sequence = sequence;
                    dataGridViewStimulusTable.DataSource = Sequence.Stimuli;
                }
                else
                {
                    MessageBox.Show("Incoming sequence is not valid. Check file for validity.");
                }

                DrawStimulusWaveform();
            }
        }

        private void DataGridViewStimulusTable_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) == DataGridViewDataErrorContexts.Parsing)
            {
                DataGridView view = (DataGridView)sender;

                var cell = view.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.Value is byte)
                {
                    if (int.TryParse((string)cell.GetEditedFormattedValue(e.RowIndex, e.Context), out int result))
                    {
                        if (result > byte.MaxValue || result < byte.MinValue)
                        {
                            MessageBox.Show("Warning: Entered value must be between 0 and 255.", "Invalid Value");
                        }
                    }
                }
            }
        }
    }
}
