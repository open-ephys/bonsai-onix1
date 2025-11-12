using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="GenericStimulusSequenceDialog"/>.
    /// </summary>
    public partial class GenericStimulusSequenceDialog : Form
    {
        readonly int NumberOfChannels;
        readonly bool UseProbeGroup;
        readonly bool UseTable;

        internal const double ZeroPeakToPeak = 1e-12;
        internal readonly double ChannelScale = 1.1;

        [Obsolete("Designer only", true)]
        GenericStimulusSequenceDialog()
        {
            InitializeComponent();

            NumberOfChannels = 0;
            UseProbeGroup = true;
        }

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters, with visual feedback on what the resulting stimulus sequence looks like.
        /// </summary>
        public GenericStimulusSequenceDialog(int numberOfChannels, bool useProbeGroup, bool useTable = false)
        {
            InitializeComponent();
            Shown += FormShown;

            NumberOfChannels = numberOfChannels;
            UseProbeGroup = useProbeGroup;
            UseTable = useTable;

            if (!UseProbeGroup)
            {
                tableLayoutPanel1.Controls.Remove(panelProbe);
                GroupBox gb = tableLayoutPanel1.Controls[nameof(groupBoxDefineStimuli)] as GroupBox;
                tableLayoutPanel1.SetRow(gb, 0);
                tableLayoutPanel1.SetRowSpan(gb, 2);
            }

            if (!UseTable)
            {
                panelWaveform.Controls.Remove(tabControlVisualization);
                panelWaveform.Controls.Add(zedGraphWaveform);
            }

            InitializeZedGraphWaveform();
            SetTableDataSource();

            zedGraphWaveform.ZoomEvent += OnZoom_Waveform;
            zedGraphWaveform.MouseMoveEvent += MouseMoveEvent;
        }

        void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 2;

                menuStrip.Visible = false;
            }
        }

        void ButtonOk_Click(object sender, EventArgs e)
        {
            if (TopLevel)
            {
                if (CanCloseForm(out DialogResult result))
                {
                    DialogResult = result;
                    Close();
                }
            }
        }

        internal bool CanCloseForm(out DialogResult result, string stimulusName = "Stimulus")
        {
            bool canClose = true;

            if (!IsSequenceValid())
            {
                DialogResult resultContinue = MessageBox.Show($"Warning: {stimulusName} sequence is not valid. " +
                    $"If you continue, the current settings for {stimulusName} will be discarded. " +
                    "Press OK to discard all changes for this device, or press Cancel to continue editing the sequence.",
                    $"Invalid {stimulusName} Sequence",
                    MessageBoxButtons.OKCancel);

                if (resultContinue == DialogResult.OK)
                {
                    result = DialogResult.Cancel;
                }
                else
                {
                    result = DialogResult.OK;
                    canClose = false;
                }
            }
            else
            {
                result = DialogResult.OK;
            }

            DialogResult = result;
            return canClose;
        }

        internal void OnSelect(object sender, EventArgs e)
        {
            DrawStimulusWaveform();
        }

        void OnZoom_Waveform(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            if (newState.Type == ZoomState.StateType.WheelZoom)
            {
                CenterAxesOnCursor(sender, sender.IsEnableHZoom, sender.IsEnableVZoom);
            }

            DrawScale();
        }

        bool MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                DrawScale();
            }

            return false;
        }

        internal virtual void HighlightInvalidChannels()
        {
            throw new NotImplementedException();
        }

        internal virtual PointPairList[] CreateStimulusWaveforms()
        {
            throw new NotImplementedException();
        }

        internal CurveItem[] GetWaveformCurves()
        {
            return zedGraphWaveform.GraphPane.CurveList.Where(item => item.Label.Text.Contains("Ch")).ToArray();
        }

        internal void DrawStimulusWaveform(bool setZoomState = true)
        {
            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();

            var (XMin, XMax, YMin, YMax)= (
                zedGraphWaveform.GraphPane.XAxis.Scale.Min,
                zedGraphWaveform.GraphPane.XAxis.Scale.Max,
                zedGraphWaveform.GraphPane.YAxis.Scale.Min,
                zedGraphWaveform.GraphPane.YAxis.Scale.Max
            );

            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            PointPairList[] waveforms = CreateStimulusWaveforms();

            if (waveforms.Length > NumberOfChannels)
            {
                throw new InvalidOperationException("Attempting to plot more channels than should exist.");
            }

            double maxLength = 0;

            for (int i = 0; i < waveforms.Length; i++)
            {
                if (waveforms[i] == null || waveforms[i].Count == 0)
                    continue;

                Color color = Color.CornflowerBlue;

                var curve = zedGraphWaveform.GraphPane.AddCurve("Ch" + i, waveforms[i], color, SymbolType.None);

                curve.Label.IsVisible = false;
                curve.Line.Width = 3;

                maxLength = waveforms[i].Last().X > maxLength ? waveforms[i].Last().X : maxLength;
            }

            zedGraphWaveform.GraphPane.YAxis.Scale.MajorStep = 1;
            zedGraphWaveform.GraphPane.YAxis.Scale.BaseTic = -waveforms.Length + 1;

            HighlightInvalidChannels();

            SetStatusValidity();

            zedGraphWaveform.GraphPane.XAxis.Scale.Max = maxLength;
            zedGraphWaveform.GraphPane.XAxis.Scale.Min = -(maxLength * 0.02);
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = -waveforms.Length - 1;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max =  1;

            zedGraphWaveform.GraphPane.YAxis.ScaleFormatEvent += (gp, axis, val, index) =>
            {
                return val <= 0 ? Math.Abs(val).ToString("0") : "";
            };

            dataGridViewStimulusTable.Refresh();

            if (setZoomState && XMin != 0 && XMax != 0)
            {
                zedGraphWaveform.GraphPane.XAxis.Scale.Min = XMin;
                zedGraphWaveform.GraphPane.XAxis.Scale.Max = XMax;
                zedGraphWaveform.GraphPane.YAxis.Scale.Min = YMin;
                zedGraphWaveform.GraphPane.YAxis.Scale.Max = YMax;
            }

            DrawScale();

            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();
        }

        internal string yAxisScaleUnits = "µA";
        internal string xAxisScaleUnits = "ms";
        internal virtual double GetPeakToPeakAmplitudeInMicroAmps() => throw new NotImplementedException();

        void DrawScale()
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
            var y = 1 / (ChannelScale * 2); // NB: Equal to 1/2 of the max peak-to-peak amplitude

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

            TextObj timeScale = new(GetTimeScaleString(x) + " " + xAxisScaleUnits, zeroOffsetX + x * TextObjScaleFactor, zeroOffsetY, CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
            timeScale.FontSpec.Border.IsVisible = false;
            timeScale.FontSpec.Fill.IsVisible = false;
            timeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(timeScale);

            TextObj amplitudeScale = new(yScaleValue.ToString("0.##") + " " + yAxisScaleUnits, zeroOffsetX, zeroOffsetY + y * TextObjScaleFactor, CoordType.AxisXYScale, AlignH.Center, AlignV.Bottom);
            amplitudeScale.FontSpec.Border.IsVisible = false;
            amplitudeScale.FontSpec.Fill.IsVisible = false;
            amplitudeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(amplitudeScale);
        }

        double GetTimeScaleString(double time)
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
                _ => Math.Round(time / 1000, 1) * 1000
            };
        }

        internal void DisableHorizontalZoom()
        {
            zedGraphWaveform.IsEnableHZoom = false;
        }

        internal void DisableVerticalZoom()
        {
            zedGraphWaveform.IsEnableVZoom = false;
        }

        void InitializeZedGraphWaveform()
        {
            zedGraphWaveform.IsZoomOnMouseCenter = true;

            zedGraphWaveform.GraphPane.Title.IsVisible = false;
            zedGraphWaveform.GraphPane.TitleGap = 0;
            zedGraphWaveform.GraphPane.Border.IsVisible = false;
            zedGraphWaveform.GraphPane.IsFontsScaled = false;

            zedGraphWaveform.GraphPane.YAxis.MajorGrid.IsZeroLine = false;

            zedGraphWaveform.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphWaveform.GraphPane.YAxis.MajorGrid.IsVisible = true;

            zedGraphWaveform.IsEnableVZoom = true;
            zedGraphWaveform.IsEnableHZoom = true;

            zedGraphWaveform.GraphPane.XAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.XAxis.MinorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MinorTic.IsAllTics = false;

            zedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;
            zedGraphWaveform.GraphPane.YAxis.Scale.IsSkipLastLabel = true;
            zedGraphWaveform.GraphPane.YAxis.Scale.IsSkipFirstLabel = true;

            zedGraphWaveform.GraphPane.YAxis.Title.Text = "Channel Number";

            zedGraphWaveform.IsAutoScrollRange = true;

            zedGraphWaveform.ZoomStepFraction = 0.5;
        }

        internal void SetXAxisTitle(string title)
        {
            zedGraphWaveform.GraphPane.XAxis.Title.Text = title;
        }

        internal void SetYAxisTitle(string title)
        {
            zedGraphWaveform.GraphPane.YAxis.Title.Text = title;
        }

        internal void RemoveYAxisLabels()
        {
            zedGraphWaveform.GraphPane.YAxis.IsVisible = false;
        }

        internal static double CalculateScaleRange(Scale scale)
        {
            return scale.Max - scale.Min;
        }

        static PointD TransformPixelsToCoordinates(Point pixels, GraphPane graphPane)
        {
            graphPane.ReverseTransform(pixels, out double x, out double y);
            y += CalculateScaleRange(graphPane.YAxis.Scale) * 0.1;

            return new PointD(x, y);
        }

        void CenterAxesOnCursor(ZedGraphControl zedGraphControl, bool hZoomEnabled, bool vZoomEnabled)
        {
            var mouseClientPosition = PointToClient(Cursor.Position);
            var currentMousePosition = TransformPixelsToCoordinates(mouseClientPosition, zedGraphControl.GraphPane);

            if (hZoomEnabled)
            {
                mouseClientPosition.X -= (zedGraphControl.Parent.Width - zedGraphControl.Width) / 2;
                var centerX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) / 2 + zedGraphControl.GraphPane.XAxis.Scale.Min;
                var diffX = centerX - currentMousePosition.X;

                zedGraphControl.GraphPane.XAxis.Scale.Min += diffX;
                zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;
            }

            if (vZoomEnabled)
            {
                mouseClientPosition.Y += (zedGraphControl.Parent.Height - zedGraphControl.Height) / 2;
                var centerY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) / 2 + zedGraphControl.GraphPane.YAxis.Scale.Min;
                var diffY = centerY - currentMousePosition.Y;

                zedGraphControl.GraphPane.YAxis.Scale.Min += diffY;
                zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
            }
        }

        internal virtual bool IsSequenceValid()
        {
            throw new NotImplementedException();
        }

        internal virtual void SetStatusValidity()
        {
            if (IsSequenceValid())
            {
                toolStripStatusIsValid.Image = Properties.Resources.StatusReadyImage;
                toolStripStatusIsValid.Text = "Generic stimulus sequence is valid.";
            }
            else
            {
                toolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                toolStripStatusIsValid.Text = "Generic stimulus sequence is invalid.";
            }
        }

        internal double GetRatio(double value1, double value2)
        {
            return value1 / value2;
        }

        internal virtual void SerializeStimulusSequence(string fileName)
        {
            throw new NotImplementedException();
        }

        void MenuItemSaveFile_Click(object sender, EventArgs e)
        {
            if (!IsSequenceValid())
            {
                var result = MessageBox.Show("Warning: The stimulus sequence is invalid; are you sure you want to save this file?",
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
                SerializeStimulusSequence(sfd.FileName);
            }
        }

        internal virtual void DeserializeStimulusSequence(string fileName)
        {
            throw new NotImplementedException();
        }

        void MenuItemLoadFile_Click(object sender, EventArgs e)
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

                DeserializeStimulusSequence(ofd.FileName);

                DrawStimulusWaveform();
            }
        }

        internal static void SetTextBoxBackgroundDefault(TextBox textBox)
        {
            SetTextBoxBackgroundColor(textBox, Color.White);
        }

        internal static void SetTextBoxBackgroundError(TextBox textBox)
        {
            SetTextBoxBackgroundColor(textBox, Color.LightGoldenrodYellow);
        }

        static void SetTextBoxBackgroundColor(TextBox textBox, Color color)
        {
            textBox.BackColor = color;
        }

        internal void HideMenuStrip()
        {
            menuStrip.Visible = false;
            menuStrip.Enabled = false;
        }

        internal virtual void SetTableDataSource()
        {
            if (UseTable)
                throw new NotImplementedException();
        }

        void ResetZoom_Click(object sender, EventArgs e)
        {
            ResetZoom();
        }

        void ResetZoom()
        {
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);
            DrawStimulusWaveform(false);
            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();
        }

    }
}
