using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        public Rhs2116StimulusSequence Sequence;

        private const double SamplePeriodMicroSeconds = 1e6 / 30.1932367151e3;

        private readonly bool[] SelectedChannels = null;

        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = ObjectExtensions.Copy(sequence);

            PropertyGridStimulusSequence.SelectedObject = Sequence;

            DataGridViewStimulusTable.DataSource = Sequence.Stimuli;
            DataGridViewStimulusTable.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

            ComboBoxStepSize.DataSource = Enum.GetValues(typeof(Rhs2116StepSize));
            ComboBoxStepSize.SelectedIndex = (int)Sequence.CurrentStepSize;

            ZedGraphWaveform.IsAutoScrollRange = true;

            SelectedChannels = new bool[16];
            SetAllChannels(true);

            InitializeZedGraphWaveform();
            DrawStimulusWaveform();

            InitializeZedGraphChannels();
            DrawChannels();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (Sequence != null)
            {
                if (!Sequence.Valid)
                {
                    DialogResult result = MessageBox.Show("Warning: Stimulus sequence is not valid. " +
                        "If you continue, the current settings will be discarded. " +
                        "Press OK to discard changes, or press Cancel to continue editing the sequence.", "Invalid Sequence",
                        MessageBoxButtons.OKCancel);

                    if (result == DialogResult.OK)
                    {
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void PropertyGridStimulusSequence_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            DrawStimulusWaveform();
        }

        private void DrawStimulusWaveform()
        {
            ZedGraphWaveform.GraphPane.CurveList.Clear();
            ZedGraphWaveform.ZoomOutAll(ZedGraphWaveform.GraphPane);

            double peakToPeak = Sequence.MaximumPeakToPeakAmplitudeSteps > 0 ? 
                Sequence.CurrentStepSizeuA * Sequence.MaximumPeakToPeakAmplitudeSteps :
                Sequence.CurrentStepSizeuA * 1;

            var stimuli = Sequence.Stimuli;

            for (int i = 0; i < stimuli.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], -(peakToPeak * 1.1) * i);
                    var curve = ZedGraphWaveform.GraphPane.AddCurve("Test", pointPairs, Color.CornflowerBlue, SymbolType.None);

                    curve.Label.IsVisible = false;
                    curve.Line.Width = 3;
                }
            }

            ZedGraphWaveform.GraphPane.XAxis.Scale.Min = 0;
            ZedGraphWaveform.GraphPane.XAxis.Scale.Max = (Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1) * SamplePeriodMicroSeconds;
            ZedGraphWaveform.GraphPane.YAxis.Scale.Min = -peakToPeak * (stimuli.Length + 0.5);
            ZedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak * 1.5;

            ZedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;

            ZedGraphWaveform.AxisChange();

            SetStatusValidity();
            SetPercentOfSlotsUsed();

            ZedGraphWaveform.Refresh();
        }

        private PointPairList CreateStimulusWaveform(Rhs2116Stimulus stimulus, double yOffset)
        {
            PointPairList points = new PointPairList
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
            ZedGraphWaveform.GraphPane.Title.IsVisible = false;
            ZedGraphWaveform.GraphPane.TitleGap = 0;
            ZedGraphWaveform.GraphPane.Border.IsVisible = false;
            ZedGraphWaveform.GraphPane.IsFontsScaled = false;

            ZedGraphWaveform.GraphPane.XAxis.MajorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.XAxis.MinorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.YAxis.MajorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.YAxis.MinorTic.IsAllTics = false;

            ZedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [μs]";
            ZedGraphWaveform.GraphPane.YAxis.Title.Text = "Amplitude [μA]";
        }

        private void InitializeZedGraphChannels()
        {
            ZedGraphChannels.GraphPane.Title.IsVisible = false;
            ZedGraphChannels.GraphPane.TitleGap = 0;
            ZedGraphChannels.GraphPane.Border.IsVisible = false;
            ZedGraphChannels.GraphPane.IsFontsScaled = false;

            ZedGraphChannels.GraphPane.XAxis.IsVisible = false;
            ZedGraphChannels.GraphPane.YAxis.IsVisible = false;

            ZedGraphChannels.GraphPane.XAxis.Scale.Min = 0;
            ZedGraphChannels.GraphPane.XAxis.Scale.Max = 1;
            ZedGraphChannels.GraphPane.YAxis.Scale.Min = 0;
            ZedGraphChannels.GraphPane.YAxis.Scale.Max = 1;

            ZedGraphChannels.IsEnableZoom = false;
            ZedGraphChannels.IsEnableVPan = false;
            ZedGraphChannels.IsEnableHPan = false;
        }

        private void SetStatusValidity()
        {
            if (Sequence.Valid && Sequence.FitsInHardware)
            {
                ToolStripStatusIsValid.Image = Properties.Resources.StatusReadyImage;
                ToolStripStatusIsValid.Text = "Valid stimulus sequence";
            }
            else
            {
                if (!Sequence.FitsInHardware)
                {
                    ToolStripStatusIsValid.Image = Properties.Resources.StatusBlockedImage;
                    ToolStripStatusIsValid.Text = "Stimulus sequence is too complex";
                }
                else
                {
                    ToolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                    ToolStripStatusIsValid.Text = "Stimulus sequence is not valid";
                }
            }
        }

        private void SetPercentOfSlotsUsed()
        {
            ToolStripStatusSlotsUsed.Text = string.Format("{0, 0:P1} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
        }

        private void DrawChannels()
        {
            EllipseObj circleObj = new EllipseObj(0.2, 0.4, 0.2, 0.2, Color.Black, Color.White)
            {
                ZOrder = ZOrder.D_BehindAxis,
                Tag = "Circle_0"
            };

            ZedGraphChannels.GraphPane.GraphObjList.Add(circleObj);

            TextObj textObj = new TextObj("0", 0.3, 0.3);
            textObj.FontSpec.Size = 30;
            textObj.FontSpec.Border.IsVisible = false;
            textObj.FontSpec.Fill.IsVisible = false;
            textObj.Tag = "Text_0";
            textObj.ZOrder = ZOrder.A_InFront;

            ZedGraphChannels.GraphPane.GraphObjList.Add(textObj);

            circleObj = new EllipseObj(0.6, 0.4, 0.2, 0.2, Color.Black, Color.White)
            {
                ZOrder = ZOrder.D_BehindAxis,
                Tag = "Circle_1"
            };

            ZedGraphChannels.GraphPane.GraphObjList.Add(circleObj);

            textObj = new TextObj("1", 0.7, 0.3);
            textObj.FontSpec.Size = 30;
            textObj.FontSpec.Border.IsVisible = false;
            textObj.FontSpec.Fill.IsVisible = false;
            textObj.Tag = "Text_1";
            textObj.ZOrder = ZOrder.A_InFront;

            ZedGraphChannels.GraphPane.GraphObjList.Add(textObj);

            ZedGraphChannels.Refresh();
        }

        private void LinkLabelDocumentation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://open-ephys.github.io/onix-docs/Software%20Guide/Bonsai.ONIX/Nodes/RHS2116TriggerDevice.html");
            }
            catch
            {
                MessageBox.Show("Unable to open documentation link. Please copy and paste the following link " +
                    "manually to find the documentation: " +
                    "https://open-ephys.github.io/onix-docs/Software%20Guide/Bonsai.ONIX/Nodes/RHS2116TriggerDevice.html");
            }
        }

        private void ZedGraphChannels_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            PointF mouseClick = new PointF(e.X, e.Y);

            if (ZedGraphChannels.GraphPane.FindNearestObject(mouseClick, CreateGraphics(), out object nearestObject, out int _))
            {
                if (nearestObject is TextObj textObj)
                {
                    HighlightSelectedChannels(textObj.Tag.ToString());
                }
                else if (nearestObject is EllipseObj circleObj)
                {
                    HighlightSelectedChannels(circleObj.Tag.ToString());
                }
                else
                {
                    SetAllChannels(true);
                }
            }
            else
            {
                SetAllChannels(true);
            }

            DrawStimulusWaveform();

            ZedGraphChannels.Refresh();
        }

        private void HighlightSelectedChannels(string tag)
        {
            if (SelectedChannels.All(x => x))
                SetAllChannels(false);

            string[] words = tag.Split('_');
            int.TryParse(words[1], out int num);

            EllipseObj circleObj = (EllipseObj)ZedGraphChannels.GraphPane.GraphObjList[string.Format("Circle_{0}", num)];
            circleObj.Fill.Color = Color.Gray;

            SelectedChannels[num] = true;
        }

        private void SetAllChannels(bool status)
        {
            for (int i = 0; i < SelectedChannels.Length; i++) 
            { 
                SelectedChannels[i] = status; 

                EllipseObj circleObj = (EllipseObj)ZedGraphChannels.GraphPane.GraphObjList[string.Format("Circle_{0}", i)];

                if (circleObj != null)
                {
                    circleObj.Fill.Color = Color.White;
                }
            }
        }

        private void ButtonAddPulses_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    if (!uint.TryParse(DelaySamples.Text, out uint delay))
                    {
                        MessageBox.Show("Unable to parse Delay.");
                        return;
                    }

                    if (!byte.TryParse(AmplitudeAnodic.Text, out byte amplitudeAnodic))
                    {
                        MessageBox.Show("Unable to parse anodic Amplitude.");
                        return;
                    }

                    if (!uint.TryParse(PulseWidthAnodic.Text, out uint pulseWidthAnodic))
                    {
                        MessageBox.Show("Unable to parse anodic Pulse Width.");
                        return;
                    }

                    if (!uint.TryParse(InterPulseInterval.Text, out uint interPulseInterval))
                    {
                        MessageBox.Show("Unable to parse Inter-Pulse Interval.");
                        return;
                    }

                    byte amplitudeCathodic = 0;
                    uint pulseWidthCathodic = 0;

                    if (!CheckboxBiphasicSymmetrical.Checked)
                    {
                        if (!byte.TryParse(AmplitudeCathodic.Text, out amplitudeCathodic))
                        {
                            MessageBox.Show("Unable to parse cathodic Amplitude.");
                            return;
                        }

                        if (!uint.TryParse(PulseWidthCathodic.Text, out pulseWidthCathodic))
                        {
                            MessageBox.Show("Unable to parse cathodic Pulse Width.");
                            return;
                        }
                    }

                    if (!uint.TryParse(InterStimulusInterval.Text, out uint isi))
                    {
                        MessageBox.Show("Unable to parse ISI.");
                        return;
                    }

                    if (!uint.TryParse(NumberOfStimuli.Text, out uint numberOfStimuli))
                    {
                        MessageBox.Show("Unable to parse Number of Stimuli.");
                        return;
                    }

                    Sequence.Stimuli[i].DelaySamples = delay;

                    Sequence.Stimuli[i].AnodicAmplitudeSteps = amplitudeAnodic;
                    Sequence.Stimuli[i].AnodicWidthSamples = pulseWidthAnodic;
                    
                    if (!CheckboxBiphasicSymmetrical.Checked)
                    {
                        Sequence.Stimuli[i].CathodicAmplitudeSteps = amplitudeCathodic;
                        Sequence.Stimuli[i].CathodicWidthSamples = pulseWidthCathodic;
                    }
                    else
                    {
                        Sequence.Stimuli[i].CathodicAmplitudeSteps = amplitudeAnodic;
                        Sequence.Stimuli[i].CathodicWidthSamples = pulseWidthAnodic;
                    }

                    Sequence.Stimuli[i].DwellSamples = interPulseInterval;
                    
                    Sequence.Stimuli[i].InterStimulusIntervalSamples = isi;

                    Sequence.Stimuli[i].NumberOfStimuli = numberOfStimuli;

                    Sequence.Stimuli[i].AnodicFirst = CheckBoxAnodicFirst.Checked;
                }
            }

            DrawStimulusWaveform();
        }

        private void ParameterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                ButtonAddPulses_Click(sender, e);
            }
        }

        private void DataGridViewStimulusTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewStimulusTable.BindingContext[DataGridViewStimulusTable.DataSource].EndCurrentEdit();
            DrawStimulusWaveform();
        }

        private void ComboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sequence.CurrentStepSize = (Rhs2116StepSize)ComboBoxStepSize.SelectedItem;
            DrawStimulusWaveform();

            Amplitude_TextChanged(AmplitudeAnodic, e);
            Amplitude_TextChanged(AmplitudeCathodic, e);
        }

        private void Samples_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = "";

            if (uint.TryParse(textBox.Text, out uint result))
            {
                double timeInMicroSeconds = result * SamplePeriodMicroSeconds;
                bool isMicro = timeInMicroSeconds < 1e3;
                
                text = string.Format("{0:F2} {1}s",
                            isMicro ? timeInMicroSeconds : timeInMicroSeconds / 1e3,
                            isMicro ? "μ" : "m");
            }

            switch (textBox.Name)
            {
                case nameof(DelaySamples):
                    DelaySamplesConverted.Text = text;
                    break;

                case nameof(PulseWidthAnodic):
                    PulseWidthAnodicConverted.Text = text;
                    break;

                case nameof(InterPulseInterval):
                    InterPulseIntervalConverted.Text = text;
                    break;

                case nameof(PulseWidthCathodic):
                    PulseWidthCathodicConverted.Text = text;
                    break;

                case nameof(InterStimulusInterval):
                    InterStimulusIntervalConverted.Text = text;
                    break;

                default:
                    MessageBox.Show("Unknown text box.");
                    break;
            }
        }

        private void Amplitude_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = "";

            if (byte.TryParse(textBox.Text, out byte result))
            {
                double amplitudeInMicroAmps = result * Sequence.CurrentStepSizeuA;
                bool isMicro = amplitudeInMicroAmps < 1e3;

                text = string.Format("{0:F2} {1}A",
                            isMicro ? amplitudeInMicroAmps : amplitudeInMicroAmps / 1e3,
                            isMicro ? "μ" : "m");
            }

            switch(textBox.Name)
            {
                case nameof(AmplitudeAnodic):
                    AmplitudeAnodicConverted.Text = text;
                    break;

                case nameof(AmplitudeCathodic):
                    AmplitudeCathodicConverted.Text = text;
                    break;

                default:
                    MessageBox.Show("Unknown text box.");
                    break;
            }
        }

        private void CheckboxBiphasicSymmetrical_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
            {
                GroupBoxCathode.Visible = false;
            }
            else
            {
                GroupBoxCathode.Visible = true;
            }
        }
    }
}
