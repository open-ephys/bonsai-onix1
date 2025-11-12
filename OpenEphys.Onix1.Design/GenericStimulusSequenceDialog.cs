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
    /// <remarks>
    /// To use this base class, create a new Form and inherit this class.
    /// Then, implement the methods in this class that are marked as virtual and throw a <see cref="NotImplementedException"/>.
    /// As an example, the <see cref="CreateStimulusWaveforms()"/> method is automatically called whenever
    /// <see cref="DrawStimulusWaveform()"/> is called, which is up to the inheriting class to implement correctly to pass along
    /// the correct waveforms for plotting.
    /// </remarks>
    public partial class GenericStimulusSequenceDialog : Form
    {
        readonly int NumberOfChannels;
        readonly bool UseProbeGroup;
        readonly bool UseTable;

        internal const double ZeroPeakToPeak = 1e-12;
        internal double PeakToPeak = 1;
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

        internal virtual bool CanCloseForm(out DialogResult result)
        {
            result = DialogResult.OK;
            return true;
        }

        internal void OnSelect(object sender, EventArgs e)
        {
            DrawStimulusWaveform();
        }

        void OnZoom_Waveform(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
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

        bool MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
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

        internal double YAxisMin = -1;
        internal double YAxisMax = 1;

        /// <summary>
        /// Calls <see cref="CreateStimulusWaveforms()"/> and draws the waveforms that are returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To modify the axis limits, ensure that the fields <see cref="YAxisMin"/> and <see cref="YAxisMax"/>
        /// are properly set in the inheriting class so that all waveforms are easily visualized.
        /// </para>
        /// <para>
        /// For the scale, ensure that the <see cref="PeakToPeak"/> field is set to the maximum peak-to-peak
        /// value of the waveforms, multiplied by <see cref="ChannelScale"/> to give a buffer between adjacent waveforms. 
        /// This will correctly modulate the scale to display the amplitude of the maximum waveform.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException"></exception>
        internal void DrawStimulusWaveform()
        {
            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();
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
                return Math.Abs(val).ToString("0");
            };

            DrawScale();

            SetZoomOutBoundaries(zedGraphWaveform);

            ZoomInBoundaryX = (ZoomOutBoundaryRight - ZoomOutBoundaryLeft) * 0.05;

            zedGraphWaveform.AxisChange();
            zedGraphWaveform.Refresh();
        }

        internal string yAxisScaleUnits = "µA";
        internal string xAxisScaleUnits = "ms";

        void DrawScale()
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
            var y = 1 / (ChannelScale * 2); // NB: Equal to 1/2 of the max peak-to-peak amplitude

            PointPairList points = new()
            {
                { zeroOffsetX, zeroOffsetY + y },
                { zeroOffsetX, zeroOffsetY },
                { zeroOffsetX + x, zeroOffsetY }
            };

            var line = zedGraphWaveform.GraphPane.AddCurve("scale", points, Color.Black, SymbolType.None);
            line.Line.Width = 3;
            line.Label.IsVisible = false;
            zedGraphWaveform.GraphPane.CurveList.Move(zedGraphWaveform.GraphPane.CurveList.Count - 1, -99);

            TextObj timeScale = new(GetTimeScaleString(x) + " " + xAxisScaleUnits, zeroOffsetX + x * 1.02, zeroOffsetY, CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
            timeScale.FontSpec.Border.IsVisible = false;
            timeScale.FontSpec.Fill.IsVisible = false;
            timeScale.ZOrder = ZOrder.A_InFront;
            zedGraphWaveform.GraphPane.GraphObjList.Add(timeScale);

            TextObj amplitudeScale = new(((PeakToPeak == ZeroPeakToPeak ? 0 : PeakToPeak) / (ChannelScale * 2)).ToString("0.##") + " " + yAxisScaleUnits, zeroOffsetX, zeroOffsetY + y * 1.02, CoordType.AxisXYScale, AlignH.Left, AlignV.Bottom);
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
                < 10000 => Math.Round(time / 1000, 1) * 1000,
                _ => time
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

        double ZoomOutBoundaryLeft = default;
        double ZoomOutBoundaryRight = default;
        double ZoomOutBoundaryBottom = default;
        double ZoomOutBoundaryTop = default;
        double? ZoomInBoundaryX = 5;
        readonly double? ZoomInBoundaryY = 2;

        void SetZoomOutBoundaries(ZedGraphControl zedGraphControl)
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

        static PointD TransformPixelsToCoordinates(Point pixels, GraphPane graphPane)
        {
            graphPane.ReverseTransform(pixels, out double x, out double y);
            y += CalculateScaleRange(graphPane.YAxis.Scale) * 0.1;

            return new PointD(x, y);
        }

        void CenterAxesOnCursor(ZedGraphControl zedGraphControl)
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
        bool CheckZoomBoundaries(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale);

            if (ZoomInBoundaryX.HasValue && rangeX < ZoomInBoundaryX)
            {
                if (ZoomInBoundaryX.HasValue && Math.Round(rangeX / ZoomInBoundaryX.Value, 2) == zedGraphControl.ZoomStepFraction)
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
                if (ZoomInBoundaryY.HasValue && Math.Round(rangeY / ZoomInBoundaryY.Value, 2) == zedGraphControl.ZoomStepFraction)
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

        internal virtual bool IsSequenceValid()
        {
            return true;
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
    }
}
