using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.Collections.Generic;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Rhs2116StimulusSequence"/>.
    /// </summary>
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        internal Rhs2116StimulusSequencePair Sequence { get; set; }

        /// <summary>
        /// Holds the step size that is displayed in the text box of the GUI. This is not the step size that is saved for the stimulus sequence object.
        /// </summary>
        private Rhs2116StepSize StepSize { get; set; }

        internal readonly Rhs2116ChannelConfigurationDialog ChannelDialog;

        private const double SamplePeriodMilliSeconds = 1e3 / 30.1932367151e3;
        const double MinAmplitudeuA = 0.01; // NB: Minimum possible amplitude is 10 nA (0.01 µA)
        const double MaxAmplitudeuA = 2550; // NB: Maximum possible amplitude is 2550000 nA (2550 µA)


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
            zedGraphWaveform.MouseMoveEvent += MouseMoveEvent;

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

        private static double GetPeakToPeakAmplitudeInMicroAmps(Rhs2116StimulusSequencePair stimulusSequence)
        {
            return stimulusSequence.MaximumPeakToPeakAmplitudeSteps > 0
                   ? stimulusSequence.CurrentStepSizeuA * stimulusSequence.MaximumPeakToPeakAmplitudeSteps
                   : stimulusSequence.CurrentStepSizeuA * 1;
        }

        private void DrawStimulusWaveform()
        {
            bool plotAllContacts = ChannelDialog.SelectedContacts.All(x => x == false);

            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            double peakToPeak = GetPeakToPeakAmplitudeInMicroAmps(Sequence) * 1.1;

            ZoomInBoundaryY = 3;

            var stimuli = Sequence.Stimuli;

            double maxLength = 0;

            for (int i = 0; i < stimuli.Length; i++)
            {
                var channelOffset = peakToPeak * i;

                if (ChannelDialog.SelectedContacts[i] || plotAllContacts)
                {
                    List<PointPairList> pulses = CreatePulses(stimuli[i], channelOffset, peakToPeak);

                    foreach (var pulse in pulses)
                    {
                        var pulseCurve = zedGraphWaveform.GraphPane.AddCurve("", pulse, Color.Red, SymbolType.None);

                        pulseCurve.Label.IsVisible = false;
                        pulseCurve.Line.Width = 3;
                    }

                    PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], channelOffset, peakToPeak);

                    Color color = stimuli[i].IsValid() ? Color.CornflowerBlue : Color.DarkRed;

                    var waveformCurve = zedGraphWaveform.GraphPane.AddCurve("", pointPairs, color, SymbolType.None);

                    waveformCurve.Label.IsVisible = false;
                    waveformCurve.Line.Width = 3;

                    maxLength = pointPairs.Last().X > maxLength ? pointPairs.Last().X : maxLength;
                }
            }

            zedGraphWaveform.GraphPane.YAxis.Scale.MajorStep = 1;
            zedGraphWaveform.GraphPane.YAxis.Scale.BaseTic = 0;

            HighlightInvalidContacts();

            SetStatusValidity();
            SetPercentOfSlotsUsed();

            zedGraphWaveform.GraphPane.XAxis.Scale.Max = maxLength;
            zedGraphWaveform.GraphPane.XAxis.Scale.Min = -(maxLength * 0.02);
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = -2;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max = stimuli.Length - 0.2;

            DrawScale();

            SetZoomOutBoundaries(zedGraphWaveform);

            ZoomInBoundaryX = (ZoomOutBoundaryRight - ZoomOutBoundaryLeft) * 0.01;

            dataGridViewStimulusTable.Refresh();

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

            var zeroOffsetX = zedGraphWaveform.GraphPane.XAxis.Scale.Min + CalculateScaleRange(zedGraphWaveform.GraphPane.XAxis.Scale) * 0.025;
            var zeroOffsetY = zedGraphWaveform.GraphPane.YAxis.Scale.Min + CalculateScaleRange(zedGraphWaveform.GraphPane.YAxis.Scale) * 0.025;

            var x = CalculateScaleRange(zedGraphWaveform.GraphPane.XAxis.Scale) * 0.05;
            var y = 1 / 2.2; // NB: Equal to 1/2 of the max peak-to-peak amplitude

            PointPairList points = new()
            {
                { zeroOffsetX, zeroOffsetY },
                { zeroOffsetX, zeroOffsetY + y },
                { zeroOffsetX, zeroOffsetY },
                { zeroOffsetX + x, zeroOffsetY }
            };

            var line = zedGraphWaveform.GraphPane.AddCurve("scale", points, Color.Black, SymbolType.None);
            line.Line.Width = 3;
            line.Label.IsVisible = false;
            zedGraphWaveform.GraphPane.CurveList.Move(zedGraphWaveform.GraphPane.CurveList.Count - 1, -99);

            TextObj timeScale = new(GetTimeScaleString(x) + " ms", zeroOffsetX + x * 1.02, zeroOffsetY, CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
            timeScale.FontSpec.Border.IsVisible = false;
            timeScale.FontSpec.Fill.IsVisible = false;
            timeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(timeScale);

            TextObj amplitudeScale = new((GetPeakToPeakAmplitudeInMicroAmps(Sequence) / 2).ToString() + " µA", zeroOffsetX, zeroOffsetY + y * 1.02, CoordType.AxisXYScale, AlignH.Center, AlignV.Bottom);
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
                double amplitude = CalculateFirstPulseAmplitude(stimulus.AnodicFirst, stimulus.AnodicAmplitudeSteps, stimulus.CathodicAmplitudeSteps, peakToPeak, yOffset);
                double width = CalculateFirstPulseWidth(stimulus.AnodicFirst, stimulus.AnodicWidthSamples, stimulus.CathodicWidthSamples);

                points.AddRange(CreatePulse(points[points.Count - 1].X, amplitude, width, yOffset));

                points.Add(points[points.Count - 1].X + stimulus.DwellSamples * SamplePeriodMilliSeconds, yOffset);

                amplitude = CalculateSecondPulseAmplitude(stimulus.AnodicFirst, stimulus.AnodicAmplitudeSteps, stimulus.CathodicAmplitudeSteps, peakToPeak, yOffset);
                width = CalculateSecondPulseWidth(stimulus.AnodicFirst, stimulus.AnodicWidthSamples, stimulus.CathodicWidthSamples);

                points.AddRange(CreatePulse(points[points.Count - 1].X, amplitude, width, yOffset));

                points.Add(points[points.Count - 1].X + stimulus.InterStimulusIntervalSamples * SamplePeriodMilliSeconds, yOffset);
            }

            points.Add(Sequence.SequenceLengthSamples * SamplePeriodMilliSeconds, yOffset);

            return points;
        }

        /// <summary>
        /// Only create the pulses, so that they can be plotted as an overlay on top of the full waveform to highlight individual pulses
        /// </summary>
        /// <param name="stimulus"></param>
        /// <param name="yOffset"></param>
        /// <param name="peakToPeak"></param>
        /// <returns></returns>
        private List<PointPairList> CreatePulses(Rhs2116Stimulus stimulus, double yOffset, double peakToPeak)
        {
            yOffset /= peakToPeak;

            var pulses = new List<PointPairList>();

            for (int i = 0; i < stimulus.NumberOfStimuli; i++)
            {
                PointPairList pulse = new();

                if (i == 0)
                    pulse.Add(stimulus.DelaySamples * SamplePeriodMilliSeconds, yOffset);
                else
                    pulse.Add(pulses[pulses.Count - 1][0].X + stimulus.InterStimulusIntervalSamples * SamplePeriodMilliSeconds, yOffset);

                double amplitude = CalculateFirstPulseAmplitude(stimulus.AnodicFirst, stimulus.AnodicAmplitudeSteps, stimulus.CathodicAmplitudeSteps, peakToPeak, yOffset);
                double width = CalculateFirstPulseWidth(stimulus.AnodicFirst, stimulus.AnodicWidthSamples, stimulus.CathodicWidthSamples);

                pulse.AddRange(CreatePulse(pulse[pulse.Count - 1].X, amplitude, width, yOffset));

                pulse.Add(pulse[pulse.Count - 1].X + stimulus.DwellSamples * SamplePeriodMilliSeconds, yOffset);

                amplitude = CalculateSecondPulseAmplitude(stimulus.AnodicFirst, stimulus.AnodicAmplitudeSteps, stimulus.CathodicAmplitudeSteps, peakToPeak, yOffset);
                width = CalculateSecondPulseWidth(stimulus.AnodicFirst, stimulus.AnodicWidthSamples, stimulus.CathodicWidthSamples);

                pulse.AddRange(CreatePulse(pulse[pulse.Count - 1].X, amplitude, width, yOffset));

                pulses.Add(pulse);
            }

            return pulses;
        }

        private double CalculateSecondPulseWidth(bool anodicFirst, uint anodicWidthSamples, uint cathodicWidthSamples)
        {
            return (anodicFirst ? cathodicWidthSamples : anodicWidthSamples) * SamplePeriodMilliSeconds;
        }

        private double CalculateSecondPulseAmplitude(bool anodicFirst, byte anodicAmplitudeSteps, byte cathodicAmplitudeSteps, double peakToPeak, double yOffset)
        {
            return (anodicFirst ? -cathodicAmplitudeSteps : anodicAmplitudeSteps) * Sequence.CurrentStepSizeuA / peakToPeak + yOffset;
        }

        private List<PointPair> CreatePulse(double x, double amplitude, double width, double yOffset)
        {
            return new List<PointPair>()
            {
                { new PointPair(x, amplitude) },
                { new PointPair(x + width, amplitude) },
                { new PointPair(x + width, yOffset) },
            };
        }

        private double CalculateFirstPulseWidth(bool anodicFirst, uint anodicWidthSamples, uint cathodicWidthSamples)
        {
            return (anodicFirst ? anodicWidthSamples : cathodicWidthSamples) * SamplePeriodMilliSeconds;
        }

        private double CalculateFirstPulseAmplitude(bool anodicFirst, byte anodicAmplitudeSteps, byte cathodicAmplitudeSteps, double peakToPeak, double yOffset)
        {
            return (anodicFirst ? anodicAmplitudeSteps : -cathodicAmplitudeSteps) * Sequence.CurrentStepSizeuA / peakToPeak + yOffset;
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
        private double? ZoomInBoundaryX = 5;
        private double? ZoomInBoundaryY = 2;

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

            if (ZoomInBoundaryX.HasValue && rangeX < ZoomInBoundaryX)
            {
                if (ZoomInBoundaryX.HasValue && rangeX / ZoomInBoundaryX == zedGraphControl.ZoomStepFraction)
                {
                    return false;
                }
                else
                {
                    if (ZoomInBoundaryX.HasValue && ZoomInBoundaryX.Value > 0)
                    {
                        var diffX = (ZoomInBoundaryX.Value - rangeX) / 2;
                        zedGraphControl.GraphPane.XAxis.Scale.Min -= diffX;
                        zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;
                    }
                }
            }

            if (ZoomInBoundaryY.HasValue && rangeY < ZoomInBoundaryY)
            {
                if (ZoomInBoundaryY.HasValue && rangeY / ZoomInBoundaryY == zedGraphControl.ZoomStepFraction)
                    return false;
                else
                {
                    if (ZoomInBoundaryY.HasValue && ZoomInBoundaryY.Value > 0)
                    {
                        var diffY = (ZoomInBoundaryY.Value - rangeY) / 2;
                        zedGraphControl.GraphPane.YAxis.Scale.Min -= diffY;
                        zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
                    }
                }
            }

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

                                  var validAnodicAmplitude = GetSampleFromAmplitude(currentAnodicAmplitude, out var newAnodicSteps);
                                  var validCathodicAmplitude = GetSampleFromAmplitude(currentAnodicAmplitude, out var newCathodicSteps);

                                  return (ValidAmplitudes: validAnodicAmplitude && newAnodicSteps != 0 && validCathodicAmplitude && newCathodicSteps != 0,
                                          s.Index,
                                          NewAnodicSteps: newAnodicSteps,
                                          NewCathodicSteps: newCathodicSteps);
                              });

                foreach (var (ValidAmplitudes, Index, NewAnodicSteps, NewCathodicSteps) in stimuli)
                {
                    if (ValidAmplitudes)
                    {
                        Sequence.Stimuli[Index].AnodicAmplitudeSteps = NewAnodicSteps;
                        Sequence.Stimuli[Index].CathodicAmplitudeSteps = NewCathodicSteps;
                    }
                    else
                    {
                        var result = MessageBox.Show($"To produce this new sequence, the step size needs to be {GetStepSizeStringuA(StepSize)}," +
                            $" but the stimulus on channel {Index} cannot be defined with this step size. " +
                            $"Press Ok to clear the stimulus from channel {Index}, or Cancel to stop adding this sequence.",
                            "Amplitude Out of Range", MessageBoxButtons.OKCancel);

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
            var ratio = value / SamplePeriodMilliSeconds;
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
            return value * SamplePeriodMilliSeconds;
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
                    textBox.Text = result > MaxAmplitudeuA ? MaxAmplitudeuA.ToString() : "0";
                    textBox.Tag = result > MaxAmplitudeuA ? 255 : 0;
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

        /// <summary>
        /// Updates the current step size based on the given amplitude
        /// </summary>
        /// <param name="amplitude">New amplitude value.</param>
        /// <returns>True if the amplitude is a valid value and the step size has been updated. False if something went wrong, the step size has not been changed.</returns>
        private bool UpdateStepSizeFromAmplitude(double amplitude)
        {
            const string InvalidAmplitudeString = "Invalid Amplitude";

            if (amplitude > MaxAmplitudeuA)
            {
                MessageBox.Show($"Warning: Amplitude is too high. Amplitude must be less than or equal to {MaxAmplitudeuA} µA.", InvalidAmplitudeString);
                return false;
            }
            else if (amplitude < 0)
            {
                MessageBox.Show("Warning: Amplitude cannot be a negative value.", InvalidAmplitudeString);
                return false;
            }
            else if (amplitude < MinAmplitudeuA && amplitude >= 0)
            {
                MessageBox.Show($"Amplitude is too small to be resolved. Amplitude must be greater than or equal to {MinAmplitudeuA} µA.", InvalidAmplitudeString);
                return false;
            }

            // NB: Update step size to a value that supports the requested amplitude.
            var possibleStepSizes = Enum.GetValues(typeof(Rhs2116StepSize))
                                        .Cast<Rhs2116StepSize>()
                                        .Where(s =>
                                        {
                                            var numSteps = (int)(amplitude / GetStepSizeuA(s));
                                            return numSteps > 0 && numSteps <= 255;
                                        });

            if (possibleStepSizes.Count() == 1)
            {
                StepSize = possibleStepSizes.First();
            }
            else
            {
                if (possibleStepSizes.Contains(Sequence.CurrentStepSize))
                {
                    StepSize = Sequence.CurrentStepSize;
                }
                else
                {
                    StepSize = possibleStepSizes.First();
                }
            }

            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            return true;
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

            StepSize = Sequence.CurrentStepSize;
            textBoxStepSize.Text = GetStepSizeStringuA(StepSize);

            checkBoxAnodicFirst.Checked = Sequence.Stimuli[index].AnodicFirst;

            Checkbox_CheckedChanged(checkboxBiphasicSymmetrical, e);

            textboxDelay.Text = GetTimeString(Sequence.Stimuli[index].DelaySamples);
            textboxDelay.Tag = Sequence.Stimuli[index].DelaySamples;

            textboxAmplitudeAnodic.Text = GetAmplitudeString(Sequence.Stimuli[index].AnodicAmplitudeSteps);
            textboxAmplitudeAnodic.Tag = Sequence.Stimuli[index].AnodicAmplitudeSteps;

            textboxPulseWidthAnodic.Text = GetTimeString(Sequence.Stimuli[index].AnodicWidthSamples);
            textboxPulseWidthAnodic.Tag = Sequence.Stimuli[index].AnodicWidthSamples;

            textboxAmplitudeCathodic.Text = GetAmplitudeString(Sequence.Stimuli[index].CathodicAmplitudeSteps);
            textboxAmplitudeCathodic.Tag = Sequence.Stimuli[index].CathodicAmplitudeSteps;

            textboxPulseWidthCathodic.Text = GetTimeString(Sequence.Stimuli[index].CathodicWidthSamples);
            textboxPulseWidthCathodic.Tag = Sequence.Stimuli[index].CathodicWidthSamples;

            textboxInterPulseInterval.Text = GetTimeString(Sequence.Stimuli[index].DwellSamples);
            textboxInterPulseInterval.Tag = Sequence.Stimuli[index].DwellSamples;

            textboxInterStimulusInterval.Text = GetTimeString(Sequence.Stimuli[index].InterStimulusIntervalSamples);
            textboxInterStimulusInterval.Tag = Sequence.Stimuli[index].InterStimulusIntervalSamples;

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
