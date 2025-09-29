using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Rhs2116StimulusSequence"/>.
    /// </summary>
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        const double SamplePeriodMilliSeconds = 1e3 / Rhs2116.SampleFrequencyHz;

        internal Rhs2116StimulusSequencePair Sequence { get; set; }

        private readonly Rhs2116StimulusSequencePair SequenceCopy = new();

        private readonly double[] RequestedAnodicAmplitudeuA;
        private readonly double[] RequestedCathodicAmplitudeuA;

        /// <summary>
        /// Holds the step size that is displayed in the text box of the GUI. This is not the step size that is saved for the stimulus sequence object.
        /// </summary>
        private Rhs2116StepSize StepSize { get; set; }

        internal readonly Rhs2116ChannelConfigurationDialog ChannelDialog;

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
            RequestedAnodicAmplitudeuA = new double[Sequence.Stimuli.Length];
            RequestedCathodicAmplitudeuA = new double[Sequence.Stimuli.Length];

            for (int i = 0; i < Sequence.Stimuli.Length; i++)
            {
                RequestedAnodicAmplitudeuA[i] = Sequence.Stimuli[i].AnodicAmplitudeSteps * Sequence.CurrentStepSizeuA;
                RequestedCathodicAmplitudeuA[i] = Sequence.Stimuli[i].CathodicAmplitudeSteps * Sequence.CurrentStepSizeuA;
            }

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
            DrawStimulusWaveform(false);

            zedGraphWaveform.ZoomEvent += OnZoom_Waveform;
            zedGraphWaveform.MouseMoveEvent += MouseMoveEvent;

            dataGridViewStimulusTable.DataSource = Sequence.Stimuli;
        }

        /// <inheritdoc/>
        protected override bool ProcessTabKey(bool forward)
        {
            Control active = ActiveControl;

            if (active != null && panelParameters.GetNextControl(active, true) == null)
            {
                // NB: If this is the last control, loop back to the beginning
                panelParameters.GetNextControl(null, true).Focus();
                return true;
            }

            return base.ProcessTabKey(forward);
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
                CenterAxesOnCursor(sender);

                if (!CheckZoomBoundaries(sender))
                {
                    sender.ZoomOut(sender.GraphPane);
                }
            }
            else if (newState.Type == ZoomState.StateType.Zoom)
            {
                CheckZoomBoundaries(sender);
            }

            DrawScale();
        }

        private bool MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (sender.GraphPane.XAxis.Scale.Max > ZoomOutBoundaryRight)
                {
                    var diff = sender.GraphPane.XAxis.Scale.Max - ZoomOutBoundaryRight;
                    sender.GraphPane.XAxis.Scale.Max -= diff;
                    sender.GraphPane.XAxis.Scale.Min -= diff;
                }
                else if (sender.GraphPane.XAxis.Scale.Min < ZoomOutBoundaryLeft)
                {
                    var diff = ZoomOutBoundaryLeft - sender.GraphPane.XAxis.Scale.Min;
                    sender.GraphPane.XAxis.Scale.Max += diff;
                    sender.GraphPane.XAxis.Scale.Min += diff;
                }

                if (sender.GraphPane.YAxis.Scale.Max > ZoomOutBoundaryTop)
                {
                    var diff = sender.GraphPane.YAxis.Scale.Max - ZoomOutBoundaryTop;
                    sender.GraphPane.YAxis.Scale.Max -= diff;
                    sender.GraphPane.YAxis.Scale.Min -= diff;
                }
                else if (sender.GraphPane.YAxis.Scale.Min < ZoomOutBoundaryBottom)
                {
                    var diff = ZoomOutBoundaryBottom - sender.GraphPane.YAxis.Scale.Min;
                    sender.GraphPane.YAxis.Scale.Max += diff;
                    sender.GraphPane.YAxis.Scale.Min += diff;
                }

                DrawScale();
            }

            return false;
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

        private double GetPeakToPeakAmplitudeInMicroAmps()
        {
            return Sequence.MaximumPeakToPeakAmplitudeSteps > 0
                ? Sequence.GetMaxPeakToPeakAmplitudeuA()
                : Sequence.CurrentStepSizeuA * 1; // NB: Used to give a buffer when plotting the stimulus waveform
        }

        private void DrawStimulusWaveform(bool setZoomState = true)
        {
            bool plotAllContacts = ChannelDialog.SelectedContacts.All(x => x == false);

            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();

            var (XMin, XMax, YMin, YMax)= (
                zedGraphWaveform.GraphPane.XAxis.Scale.Min,
                zedGraphWaveform.GraphPane.XAxis.Scale.Max,
                zedGraphWaveform.GraphPane.YAxis.Scale.Min,
                zedGraphWaveform.GraphPane.YAxis.Scale.Max
            );

            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            double peakToPeak = GetPeakToPeakAmplitudeInMicroAmps() * 1.1;

            double maxLength = 0;

            for (int i = 0; i < Sequence.Stimuli.Length; i++)
            {
                var channelOffset = -peakToPeak * i;

                if (ChannelDialog.SelectedContacts[i] || plotAllContacts)
                {
                    PointPairList pointPairs = CreateStimulusWaveform(Sequence.Stimuli[i], channelOffset, peakToPeak);

                    Color color;
                    if (Sequence.Stimuli[i].IsValid())
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

                    maxLength = pointPairs.Last().X > maxLength ? pointPairs.Last().X : maxLength;
                }
            }

            zedGraphWaveform.GraphPane.YAxis.Scale.MajorStep = 1;
            zedGraphWaveform.GraphPane.YAxis.Scale.BaseTic = -Sequence.Stimuli.Length + 1;

            HighlightInvalidContacts();

            SetStatusValidity();
            SetPercentOfSlotsUsed();

            zedGraphWaveform.GraphPane.XAxis.Scale.Max = maxLength;
            zedGraphWaveform.GraphPane.XAxis.Scale.Min = -(maxLength * 0.02);
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = -Sequence.Stimuli.Length - 2;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max =  1;

            zedGraphWaveform.GraphPane.YAxis.ScaleFormatEvent += (gp, axis, val, index) =>
            {
                return Math.Abs(val).ToString("0");
            };

            SetZoomOutBoundaries(zedGraphWaveform);

            dataGridViewStimulusTable.Refresh();

            if (setZoomState)
            {
                zedGraphWaveform.GraphPane.XAxis.Scale.Min = XMin;
                zedGraphWaveform.GraphPane.XAxis.Scale.Max = XMax;
                zedGraphWaveform.GraphPane.YAxis.Scale.Min = YMin;
                zedGraphWaveform.GraphPane.YAxis.Scale.Max = YMax;
            }

            if (!CheckZoomBoundaries(zedGraphWaveform))
                zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            DrawScale();

            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();
        }

        private void DrawScale()
        {
            const string scaleString = "scale";

            var oldScale = zedGraphWaveform.GraphPane.CurveList[scaleString];
            if (oldScale != null)
            {
                zedGraphWaveform.GraphPane.CurveList.Remove(oldScale);
                zedGraphWaveform.GraphPane.GraphObjList.RemoveAll(x => x is TextObj);
            }

            var xScaleRange = CalculateScaleRange(zedGraphWaveform.GraphPane.XAxis.Scale);
            var yScaleRange = CalculateScaleRange(zedGraphWaveform.GraphPane.YAxis.Scale);

            const double ScaleFactor = 0.025;

            var zeroOffsetX = zedGraphWaveform.GraphPane.XAxis.Scale.Min + xScaleRange * ScaleFactor;
            var zeroOffsetY = zedGraphWaveform.GraphPane.YAxis.Scale.Min + yScaleRange * ScaleFactor;

            var x = xScaleRange * ScaleFactor * 2;
            var y = 1 / 2.2; // NB: Equal to 1/2 of the max peak-to-peak amplitude

            double maxValueY = yScaleRange * 0.25;

            double yScaleValue = GetPeakToPeakAmplitudeInMicroAmps() / 2;

            if (y > maxValueY)
            {
                double ratio = y / maxValueY;
                yScaleValue /= ratio;
                y /= ratio;
            }

            PointPairList points = new()
            {
                { zeroOffsetX, zeroOffsetY + y },
                { zeroOffsetX, zeroOffsetY },
                { zeroOffsetX + x, zeroOffsetY }
            };

            float lineWidth = 3;

            var line = zedGraphWaveform.GraphPane.AddCurve("scale", points, Color.Black, SymbolType.Square);
            line.Line.Width = lineWidth;
            line.Label.IsVisible = false;
            line.Symbol.Size = lineWidth;
            line.Symbol.Border.IsVisible = false;
            line.Symbol.Fill = new Fill(Color.Black);
            zedGraphWaveform.GraphPane.CurveList.Move(zedGraphWaveform.GraphPane.CurveList.Count - 1, -99);

            const double TextObjScaleFactor = 1.02;

            TextObj timeScale = new(GetTimeScaleString(x) + " ms", zeroOffsetX + x * TextObjScaleFactor, zeroOffsetY, CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
            timeScale.FontSpec.Border.IsVisible = false;
            timeScale.FontSpec.Fill.IsVisible = false;
            timeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(timeScale);

            TextObj amplitudeScale = new(yScaleValue.ToString("0.##") + " µA", zeroOffsetX, zeroOffsetY + y * TextObjScaleFactor, CoordType.AxisXYScale, AlignH.Center, AlignV.Bottom);
            amplitudeScale.FontSpec.Border.IsVisible = false;
            amplitudeScale.FontSpec.Fill.IsVisible = false;
            amplitudeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(amplitudeScale);
        }

        private double GetTimeScaleString(double time)
        {
            return time switch
            {
                <= 0 => 0,
                < 0.01 => Math.Round(time, 4),
                < 0.1 => Math.Round(time, 3),
                < 1 => Math.Round(time, 2),
                < 10 => Math.Round(time, 1),
                < 100 => Math.Round(time / 10, 1) * 10,
                < 1000 => Math.Round(time / 100, 1) * 100,
                < 10000 => Math.Round(time / 1000, 1) * 1000,
                _ => time
            };
        }

        private PointPairList CreateStimulusWaveform(Rhs2116Stimulus stimulus, double yOffset, double peakToPeak)
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
            zedGraphWaveform.GraphPane.YAxis.Scale.IsSkipLastLabel = true;
            zedGraphWaveform.GraphPane.YAxis.Scale.IsSkipFirstLabel = true;

            zedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [ms]";
            zedGraphWaveform.GraphPane.YAxis.Title.Text = "Channel Number";

            zedGraphWaveform.IsAutoScrollRange = true;

            zedGraphWaveform.ZoomStepFraction = 0.5;
        }

        private double ZoomOutBoundaryLeft = default;
        private double ZoomOutBoundaryRight = default;
        private double ZoomOutBoundaryBottom = default;
        private double ZoomOutBoundaryTop = default;

        private void SetZoomOutBoundaries(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var marginX = rangeX * 0.03;

            ZoomOutBoundaryLeft = zedGraphControl.GraphPane.XAxis.Scale.Min;
            ZoomOutBoundaryBottom = zedGraphControl.GraphPane.YAxis.Scale.Min;
            ZoomOutBoundaryRight = zedGraphControl.GraphPane.XAxis.Scale.Max + marginX;
            ZoomOutBoundaryTop = zedGraphControl.GraphPane.YAxis.Scale.Max;
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
                zedGraphControl.GraphPane.YAxis.Scale.Max == ZoomOutBoundaryTop))
            {
                return;
            }

            var mouseClientPosition = PointToClient(Cursor.Position);
            mouseClientPosition.X -= (zedGraphControl.Parent.Width - zedGraphControl.Width) / 2;
            mouseClientPosition.Y += (zedGraphControl.Parent.Height - zedGraphControl.Height) / 2;

            var currentMousePosition = TransformPixelsToCoordinates(mouseClientPosition, zedGraphControl.GraphPane);

            var centerX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) / 2 + zedGraphControl.GraphPane.XAxis.Scale.Min;
            var centerY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) / 2 + zedGraphControl.GraphPane.YAxis.Scale.Min;

            var diffX = centerX - currentMousePosition.X;
            var diffY = centerY - currentMousePosition.Y;

            zedGraphControl.GraphPane.XAxis.Scale.Min += diffX;
            zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;

            zedGraphControl.GraphPane.YAxis.Scale.Min += diffY;
            zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
        }

        private bool CheckZoomBoundaries(ZedGraphControl zedGraphControl)
        {
            if (CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) >= ZoomOutBoundaryRight - ZoomOutBoundaryLeft)
            {
                zedGraphControl.GraphPane.XAxis.Scale.Min = ZoomOutBoundaryLeft;
                zedGraphControl.GraphPane.XAxis.Scale.Max = ZoomOutBoundaryRight;
            }
            else
            {
                if (zedGraphControl.GraphPane.XAxis.Scale.Min < ZoomOutBoundaryLeft)
                {
                    var diffX = ZoomOutBoundaryLeft - zedGraphControl.GraphPane.XAxis.Scale.Min;
                    zedGraphControl.GraphPane.XAxis.Scale.Min += diffX;
                    zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;
                }

                if (zedGraphControl.GraphPane.XAxis.Scale.Max > ZoomOutBoundaryRight)
                {
                    var diffX = zedGraphControl.GraphPane.XAxis.Scale.Max - ZoomOutBoundaryRight;
                    zedGraphControl.GraphPane.XAxis.Scale.Min -= diffX;
                    zedGraphControl.GraphPane.XAxis.Scale.Max -= diffX;
                }

            }

            if (CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) >= ZoomOutBoundaryTop - ZoomOutBoundaryBottom)
            {
                zedGraphControl.GraphPane.YAxis.Scale.Min = ZoomOutBoundaryBottom;
                zedGraphControl.GraphPane.YAxis.Scale.Max = ZoomOutBoundaryTop;
            }
            else
            {
                if (zedGraphControl.GraphPane.YAxis.Scale.Min < ZoomOutBoundaryBottom)
                {
                    var diffY = ZoomOutBoundaryBottom - zedGraphControl.GraphPane.YAxis.Scale.Min;
                    zedGraphControl.GraphPane.YAxis.Scale.Min += diffY;
                    zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
                }

                if (zedGraphControl.GraphPane.YAxis.Scale.Max > ZoomOutBoundaryTop)
                {
                    var diffY = zedGraphControl.GraphPane.YAxis.Scale.Max - ZoomOutBoundaryTop;
                    zedGraphControl.GraphPane.YAxis.Scale.Min -= diffY;
                    zedGraphControl.GraphPane.YAxis.Scale.Max -= diffY;
                }
            }

            return true;
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
                                                 .FirstOrDefault();

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
            if (ChannelDialog.SelectedContacts.All(x => x == false))
            {
                MessageBox.Show("No contacts selected. Please select contact(s) before trying to add pulses.");
                return;
            }

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

            dataGridViewStimulusTable.DataSource = Sequence.Stimuli; // NB: Force an update in case pulse timings were restored

            for (int i = 0; i < ChannelDialog.SelectedContacts.Length; i++)
            {
                if (ChannelDialog.SelectedContacts[i])
                {
                    if (textboxDelay.Tag != null)
                    {
                        Sequence.Stimuli[i].DelaySamples = (uint)textboxDelay.Tag;
                    }

                    if (textboxAmplitudeAnodicRequested.Tag != null)
                    {
                        RequestedAnodicAmplitudeuA[i] = (double)textboxAmplitudeAnodicRequested.Tag;
                    }

                    if (textboxAmplitudeAnodic.Tag != null)
                    {
                        Sequence.Stimuli[i].AnodicAmplitudeSteps = (byte)textboxAmplitudeAnodic.Tag;
                    }

                    if (textboxPulseWidthAnodic.Tag != null)
                    {
                        Sequence.Stimuli[i].AnodicWidthSamples = (uint)textboxPulseWidthAnodic.Tag;
                    }

                    if (textboxInterPulseInterval.Tag != null)
                    {
                        Sequence.Stimuli[i].DwellSamples = (uint)textboxInterPulseInterval.Tag;
                    }

                    if (textboxAmplitudeCathodicRequested.Tag != null)
                    {
                        RequestedCathodicAmplitudeuA[i] = (double)textboxAmplitudeCathodicRequested.Tag;
                    }

                    if (textboxAmplitudeCathodic.Tag != null)
                    {
                        Sequence.Stimuli[i].CathodicAmplitudeSteps = (byte)textboxAmplitudeCathodic.Tag;
                    }

                    if (textboxPulseWidthCathodic.Tag != null)
                    {
                        Sequence.Stimuli[i].CathodicWidthSamples = (uint)textboxPulseWidthCathodic.Tag;
                    }

                    if (textboxInterStimulusInterval.Tag != null)
                    {
                        Sequence.Stimuli[i].InterStimulusIntervalSamples = (uint)textboxInterStimulusInterval.Tag;
                    }

                    Sequence.Stimuli[i].NumberOfStimuli = (uint)numericUpDownNumberOfPulses.Value;
                    Sequence.Stimuli[i].AnodicFirst = checkBoxAnodicFirst.Checked;
                }
            }

            Sequence.CurrentStepSize = StepSize;

            ChannelDialog.HighlightEnabledContacts();
            
            DrawStimulusWaveform();
        }

        private void ParameterKeyPress_Time(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Samples_TextChanged(sender, e);
                ButtonAddPulses_Click(sender, e);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ParameterKeyPress_Amplitude(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Amplitude_TextChanged(sender, e);
                ButtonAddPulses_Click(sender, e);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void numericUpDownNumberOfPulses_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonAddPulses_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
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

        private string GetAmplitudeString(byte amplitude)
        {
            return GetAmplitudeString(amplitude, StepSize);
        }

        private string GetAmplitudeString(byte amplitude, Rhs2116StepSize stepSize)
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

                if (textBox.Name == nameof(textboxPulseWidthAnodic) && checkboxBiphasicSymmetrical.Checked)
                {
                    textboxPulseWidthCathodic.Text = "";
                    textboxPulseWidthCathodic.Tag = null;
                }
                else if (textBox.Name == nameof(textboxPulseWidthCathodic) && checkboxBiphasicSymmetrical.Checked)
                {
                    textboxPulseWidthAnodic.Text = "";
                    textboxPulseWidthAnodic.Tag = null;
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

            ButtonAddPulses_Click(sender, e);
        }

        private void numericUpDownNumberOfPulses_Leave(object sender, EventArgs e)
        {
            ButtonAddPulses_Click(sender, e);
        }

        private bool GetSampleFromTime(double value, out uint samples)
        {
            var ratio = value / SamplePeriodMilliSeconds;
            samples = (uint)Math.Round(ratio);

            return !(ratio > uint.MaxValue || ratio < uint.MinValue);
        }

        /// <summary>
        /// Get the number of samples needed at the current step size to represent a given amplitude.
        /// </summary>
        /// <param name="value">Double value defining the amplitude in microamps.</param>
        /// <param name="stepSize"><see cref="Rhs2116StepSize"/></param>
        /// <param name="samples">Output returning the number of samples as a byte.</param>
        /// <returns>Returns true if the number of samples is a valid byte value (between 0 and 255). Returns false if the number of samples cannot be represented in byte format.</returns>
        private bool GetSampleFromAmplitude(double value, Rhs2116StepSize stepSize, out byte samples)
        {
            var ratio = GetRatio(value, Rhs2116StimulusSequence.GetStepSizeuA(stepSize));

            if (ratio >= 255) samples = 255;
            else if (ratio <= 0) samples = 0;
            else samples = (byte)Math.Round(ratio);

            return !(ratio > byte.MaxValue || ratio < 0);
        }

        private double GetRatio(double value1, double value2)
        {
            return value1 / value2;
        }

        /// <inheritdoc cref="GetSampleFromAmplitude(double, Rhs2116StepSize, out byte)"/>
        private bool GetSampleFromAmplitude(double value, out byte samples)
        {
            return GetSampleFromAmplitude(value, StepSize, out samples);
        }

        private double GetTimeFromSample(uint value)
        {
            return value * SamplePeriodMilliSeconds;
        }

        private double GetAmplitudeFromSample(byte value, Rhs2116StepSize stepSize)
        {
            return value * Rhs2116StimulusSequence.GetStepSizeuA(stepSize);
        }

        private void UpdateAmplitudeTextBoxes(TextBox textBox, string text = "", byte? tag = null)
        {
            if (checkboxBiphasicSymmetrical.Checked)
            {
                textboxAmplitudeCathodic.Text = text;
                textboxAmplitudeCathodic.Tag = tag.HasValue ? tag.Value : null;

                textboxAmplitudeAnodic.Text = text;
                textboxAmplitudeAnodic.Tag = tag.HasValue ? tag.Value : null;

                if (textBox.Name == nameof(textboxAmplitudeAnodicRequested))
                {
                    textboxAmplitudeCathodicRequested.Text = textboxAmplitudeAnodicRequested.Text;
                    textboxAmplitudeCathodicRequested.Tag = textboxAmplitudeAnodicRequested.Tag;
                }
                else if (textBox.Name == nameof(textboxAmplitudeCathodicRequested))
                {
                    textboxAmplitudeAnodicRequested.Text = textboxAmplitudeCathodicRequested.Text;
                    textboxAmplitudeAnodicRequested.Tag = textboxAmplitudeCathodicRequested.Tag;
                }
            }
            else
            {
                if (textBox.Name == nameof(textboxAmplitudeAnodicRequested))
                {
                    textboxAmplitudeAnodic.Text = text;
                    textboxAmplitudeAnodic.Tag = tag.HasValue ? tag.Value : null;
                }
                else if (textBox.Name == nameof(textboxAmplitudeCathodicRequested))
                {
                    textboxAmplitudeCathodic.Text = text;
                    textboxAmplitudeCathodic.Tag = tag.HasValue ? tag.Value : null;
                }
            }

        }

        private void Amplitude_TextChanged(object sender, EventArgs e)
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
        private bool UpdateStepSizeFromAmplitude(double amplitude)
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
                textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

                return true;
            }

            StepSize = Rhs2116StimulusSequence.GetStepSizeWithMinError(validStepSizes, Sequence.Stimuli, amplitude, Sequence.CurrentStepSize);
            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            return true;
        }

        private bool IsValidNumberOfSteps(int numberOfSteps)
        {
            return numberOfSteps > 0 && numberOfSteps <= 255;
        }

        private int GetNumberOfSteps(double amplitude, Rhs2116StepSize stepSize)
        {
            return (int)(amplitude / Rhs2116StimulusSequence.GetStepSizeuA(stepSize));
        }

        private double CalculateAmplitudePercentError(double amplitude, Rhs2116StepSize stepSize)
        {
            if (amplitude == 0) return 0;

            var stepSizeuA = Rhs2116StimulusSequence.GetStepSizeuA(stepSize);

            GetSampleFromAmplitude(amplitude, stepSize, out var steps);

            return 100 * ((amplitude - steps * stepSizeuA) / amplitude);
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxBiphasicSymmetrical.Checked)
            {
                if (checkBoxAnodicFirst.Checked)
                {
                    groupBoxCathode.Visible = false;
                    groupBoxAnode.Visible = true;

                    textboxPulseWidthCathodic.Text = textboxPulseWidthAnodic.Text;
                    textboxPulseWidthCathodic.Tag = textboxPulseWidthAnodic.Tag;

                    textboxAmplitudeCathodic.Text = textboxAmplitudeAnodic.Text;
                    textboxAmplitudeCathodic.Tag = textboxAmplitudeAnodic.Tag;

                    textboxAmplitudeCathodicRequested.Text = textboxAmplitudeAnodicRequested.Text;
                    textboxAmplitudeCathodicRequested.Tag = textboxAmplitudeAnodicRequested.Tag;
                }
                else
                {
                    groupBoxCathode.Visible = true;
                    groupBoxAnode.Visible = false;

                    textboxPulseWidthAnodic.Text = textboxPulseWidthCathodic.Text;
                    textboxPulseWidthAnodic.Tag = textboxPulseWidthCathodic.Tag;

                    textboxAmplitudeAnodic.Text = textboxAmplitudeCathodic.Text;
                    textboxAmplitudeAnodic.Tag = textboxAmplitudeCathodic.Tag;

                    textboxAmplitudeAnodicRequested.Text = textboxAmplitudeCathodicRequested.Text;
                    textboxAmplitudeAnodicRequested.Tag = textboxAmplitudeCathodicRequested.Tag;
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

            if (Sequence.Stimuli[index].AnodicAmplitudeSteps == Sequence.Stimuli[index].CathodicAmplitudeSteps &&
                Sequence.Stimuli[index].AnodicWidthSamples == Sequence.Stimuli[index].CathodicWidthSamples)
            {
                checkboxBiphasicSymmetrical.Checked = true;
            }
            else
            {
                checkboxBiphasicSymmetrical.Checked = false;
            }

            StepSize = Sequence.CurrentStepSize;
            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            checkBoxAnodicFirst.Checked = Sequence.Stimuli[index].AnodicFirst;

            Checkbox_CheckedChanged(checkboxBiphasicSymmetrical, e);

            textboxDelay.Text = GetTimeString(Sequence.Stimuli[index].DelaySamples);
            textboxDelay.Tag = Sequence.Stimuli[index].DelaySamples;

            textboxAmplitudeAnodic.Text = GetAmplitudeString(Sequence.Stimuli[index].AnodicAmplitudeSteps);
            textboxAmplitudeAnodic.Tag = Sequence.Stimuli[index].AnodicAmplitudeSteps;

            if (RequestedAnodicAmplitudeuA[index] != 0.0)
            {
                textboxAmplitudeAnodicRequested.Text = RequestedAnodicAmplitudeuA[index].ToString();
                textboxAmplitudeAnodicRequested.Tag = RequestedAnodicAmplitudeuA[index];
            }
            else
            {
                textboxAmplitudeAnodicRequested.Text = "";
                textboxAmplitudeAnodicRequested.Tag = null;
            }

            textboxPulseWidthAnodic.Text = GetTimeString(Sequence.Stimuli[index].AnodicWidthSamples);
            textboxPulseWidthAnodic.Tag = Sequence.Stimuli[index].AnodicWidthSamples;

            textboxAmplitudeCathodic.Text = GetAmplitudeString(Sequence.Stimuli[index].CathodicAmplitudeSteps);
            textboxAmplitudeCathodic.Tag = Sequence.Stimuli[index].CathodicAmplitudeSteps;

            if (RequestedCathodicAmplitudeuA[index] != 0.0)
            {
                textboxAmplitudeCathodicRequested.Text = RequestedCathodicAmplitudeuA[index].ToString();
                textboxAmplitudeCathodicRequested.Tag = RequestedCathodicAmplitudeuA[index];
            }
            else
            {
                textboxAmplitudeCathodicRequested.Text = "";
                textboxAmplitudeCathodicRequested.Tag = null;
            }

            textboxPulseWidthCathodic.Text = GetTimeString(Sequence.Stimuli[index].CathodicWidthSamples);
            textboxPulseWidthCathodic.Tag = Sequence.Stimuli[index].CathodicWidthSamples;

            textboxInterPulseInterval.Text = GetTimeString(Sequence.Stimuli[index].DwellSamples);
            textboxInterPulseInterval.Tag = Sequence.Stimuli[index].DwellSamples;

            textboxInterStimulusInterval.Text = GetTimeString(Sequence.Stimuli[index].InterStimulusIntervalSamples);
            textboxInterStimulusInterval.Tag = Sequence.Stimuli[index].InterStimulusIntervalSamples;

            numericUpDownNumberOfPulses.Value = Sequence.Stimuli[index].NumberOfStimuli;
        }

        private void MenuItemSaveFile_Click(object sender, EventArgs e)
        {
            if (!Sequence.Valid)
            {
                var result = MessageBox.Show("Warning: Not all stimuli are valid; are you sure you want to save this file?",
                    "Invalid Stimuli", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.No) return;
            }

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

                    if (!Sequence.Valid)
                    {
                        MessageBox.Show("Warning: Invalid stimuli found in the recently opened file. Check all values to ensure they are what is expected.",
                            "Invalid Stimuli", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Incoming file is not valid. Check file for validity.");
                }

                DrawStimulusWaveform();
            }
        }

        private void ButtonResetZoomClick(object sender, EventArgs e)
        {
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);
            DrawStimulusWaveform(false);
            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();
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
