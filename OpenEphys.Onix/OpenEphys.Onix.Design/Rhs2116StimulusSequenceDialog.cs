using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        /// <summary>
        /// Holds a local copy of the Rhs2116StimulusSequence until the user presses Okay
        /// </summary>
        public Rhs2116StimulusSequence Sequence;

        private const double SamplePeriodMicroSeconds = 1e6 / 30.1932367151e3;

        private readonly bool[] SelectedChannels = null;

        private readonly string DefaultChannelLayoutFilePath = "../../Python/simple_rhs2116_headstage_probe_interface.json";

        private const string ContactStringFormat = "Contact_{0}";
        private const string TextStringFormat = "TextContact_{0}";

        private ProbeGroup probeGroup;

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters
        /// </summary>
        /// <param name="sequence"></param>
        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = new Rhs2116StimulusSequence(sequence);

            propertyGridStimulusSequence.SelectedObject = Sequence;

            dataGridViewStimulusTable.DataSource = Sequence.Stimuli;
            dataGridViewStimulusTable.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

            SelectedChannels = new bool[Sequence.Stimuli.Length];
            SetAllChannels(true);

            comboBoxStepSize.DataSource = Enum.GetValues(typeof(Rhs2116StepSize));
            comboBoxStepSize.SelectedIndex = (int)Sequence.CurrentStepSize;

            InitializeZedGraphChannels();
            LoadDefaultChannelLayout();
            DrawChannels();

            InitializeZedGraphWaveform();
            DrawStimulusWaveform();
        }

        private void LoadDefaultChannelLayout()
        {
            textBoxChannelLayoutFilePath.Text = Path.GetFullPath(DefaultChannelLayoutFilePath);

            LoadChannelLayout(DefaultChannelLayoutFilePath);
        }

        private void LoadChannelLayout(string channelLayoutFilePath)
        {
            string channelLayoutString = File.ReadAllText(channelLayoutFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            probeGroup = JsonSerializer.Deserialize<ProbeGroup>(channelLayoutString, options);
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
            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            double peakToPeak = (Sequence.MaximumPeakToPeakAmplitudeSteps > 0 ? 
                Sequence.CurrentStepSizeuA * Sequence.MaximumPeakToPeakAmplitudeSteps :
                Sequence.CurrentStepSizeuA * 1) * 1.1;

            var stimuli = Sequence.Stimuli;

            for (int i = 0; i < stimuli.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], -peakToPeak * i);
                    var curve = zedGraphWaveform.GraphPane.AddCurve("Test", pointPairs, Color.CornflowerBlue, SymbolType.None);

                    curve.Label.IsVisible = false;
                    curve.Line.Width = 3;
                }
            }

            zedGraphWaveform.GraphPane.XAxis.Scale.Min = 0;
            zedGraphWaveform.GraphPane.XAxis.Scale.Max = (Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1) * SamplePeriodMicroSeconds;
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = -peakToPeak * stimuli.Length;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak;

            zedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;

            zedGraphWaveform.AxisChange();

            SetStatusValidity();
            SetPercentOfSlotsUsed();

            zedGraphWaveform.Refresh();
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
            zedGraphWaveform.GraphPane.Title.IsVisible = false;
            zedGraphWaveform.GraphPane.TitleGap = 0;
            zedGraphWaveform.GraphPane.Border.IsVisible = false;
            zedGraphWaveform.GraphPane.IsFontsScaled = false;

            zedGraphWaveform.GraphPane.XAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.XAxis.MinorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MajorTic.IsAllTics = false;
            zedGraphWaveform.GraphPane.YAxis.MinorTic.IsAllTics = false;

            zedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [μs]";
            zedGraphWaveform.GraphPane.YAxis.Title.Text = "Amplitude [μA]";
            
            zedGraphWaveform.IsAutoScrollRange = true;
        }

        private void InitializeZedGraphChannels()
        {
            zedGraphChannels.GraphPane.Title.IsVisible = false;
            zedGraphChannels.GraphPane.TitleGap = 0;
            zedGraphChannels.GraphPane.Border.IsVisible = false;
            zedGraphChannels.GraphPane.Chart.Border.IsVisible = false;
            zedGraphChannels.GraphPane.IsFontsScaled = true;

            zedGraphChannels.GraphPane.XAxis.IsVisible = false;
            zedGraphChannels.GraphPane.YAxis.IsVisible = false;

            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;
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
                    toolStripStatusIsValid.Text = "Stimulus sequence is too complex";
                }
                else
                {
                    toolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                    toolStripStatusIsValid.Text = "Stimulus sequence is not valid";
                }
            }
        }

        private void SetPercentOfSlotsUsed()
        {
            toolStripStatusSlotsUsed.Text = string.Format("{0, 0:P1} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
        }

        private void DrawChannels()
        {
            double minX = 1e3, minY = 1e3, maxX = -1e3, maxY = -1e3;

            for (int i = 0; i < probeGroup.Probes.Length; i++)
            {
                PointD[] planarContours = ConvertFloatArrayToPointD(probeGroup.Probes[i].Probe_Planar_Contour);
                PolyObj contour = new(planarContours, Color.LightGreen, Color.LightGreen)
                {
                    ZOrder = ZOrder.E_BehindCurves
                };

                zedGraphChannels.GraphPane.GraphObjList.Add(contour);

                var tmp = planarContours.Min(p => p.X);
                minX = tmp < minX ? tmp : minX;

                tmp = planarContours.Min(p => p.Y);
                minY = tmp < minY ? tmp : minY;

                tmp = planarContours.Max(p => p.X);
                maxX = tmp > maxX ? tmp : maxX;

                tmp = planarContours.Max(p => p.Y);
                maxY = tmp > maxY ? tmp : maxY; 

                for (int j = 0; j < probeGroup.Probes[i].Contact_Positions.Length; j++)
                {
                    Contact contact = probeGroup.Probes[i].GetContact(j);

                    if (contact.Shape.Equals("circle"))
                    {
                        EllipseObj contactObj = new(contact.PosX - contact.ShapeParams.Radius, contact.PosY + contact.ShapeParams.Radius,
                            contact.ShapeParams.Radius * 2, contact.ShapeParams.Radius * 2, Color.DarkGray, Color.WhiteSmoke)
                        {
                            ZOrder=ZOrder.B_BehindLegend,
                            Tag = string.Format(ContactStringFormat, contact.ContactId)
                        };

                        zedGraphChannels.GraphPane.GraphObjList.Add(contactObj);

                        TextObj textObj = new(contact.ContactId, contact.PosX, contact.PosY)
                        {
                            ZOrder=ZOrder.A_InFront,
                            Tag = string.Format(TextStringFormat, contact.ContactId)
                        };
                        textObj.FontSpec.Size = 22;
                        textObj.FontSpec.Border.IsVisible = false;
                        textObj.FontSpec.Fill.IsVisible = false;

                        zedGraphChannels.GraphPane.GraphObjList.Add(textObj);
                    }
                    else
                    {
                        MessageBox.Show("Contact shapes other than 'circle' not implemented yet.");
                    }
                }
            }

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

            if (rangeY < rangeX / 2)
            {
                minY -= rangeX / 2 - rangeY;
                maxY += rangeX / 2 - rangeY;
                rangeY = maxY - minY;
            }

            zedGraphChannels.GraphPane.XAxis.Scale.Min = minX - rangeX * 0.1; 
            zedGraphChannels.GraphPane.XAxis.Scale.Max = maxX + rangeX * 0.1; 
            zedGraphChannels.GraphPane.YAxis.Scale.Min = minY - rangeY * 0.1;
            zedGraphChannels.GraphPane.YAxis.Scale.Max = maxY + rangeY * 0.1;

            zedGraphChannels.AxisChange();
            zedGraphChannels.Refresh();
        }

        private PointD[] ConvertFloatArrayToPointD(float[][] floats)
        {
            PointD[] pointD = new PointD[floats.Length];

            for (int i = 0; i < floats.Length; i++)
            {
                pointD[i] = new PointD(floats[i][0], floats[i][1]);
            }

            return pointD;
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

            PointF mouseClick = new(e.X, e.Y);

            if (zedGraphChannels.GraphPane.FindNearestObject(mouseClick, CreateGraphics(), out object nearestObject, out int _))
            {
                if (nearestObject is TextObj textObj)
                {
                    ToggleSelectedChannel(textObj.Tag.ToString());
                }
                else if (nearestObject is EllipseObj circleObj)
                {
                    ToggleSelectedChannel(circleObj.Tag.ToString());
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

            VisualizeSelectedChannels();

            DrawStimulusWaveform();

            zedGraphChannels.Refresh();
        }

        private void ToggleSelectedChannel(string tag)
        {
            if (SelectedChannels.All(x => x))
                SetAllChannels(false);

            string[] words = tag.Split('_');
            if (int.TryParse(words[1], out int num))
            {
                SelectedChannels[num] = !SelectedChannels[num];
            }
            else
            {
                MessageBox.Show("Warning: Invalid channel tag detected.");
            }
        }

        private void VisualizeSelectedChannels()
        {
            if (SelectedChannels.All(x => !x))
            {
                SetAllChannels(true);
            }

            bool plotAllChannels = SelectedChannels.All(x => x);

            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                EllipseObj circleObj = (EllipseObj)zedGraphChannels.GraphPane.GraphObjList[string.Format(ContactStringFormat, i)];

                if (circleObj != null)
                {
                    if (plotAllChannels || !SelectedChannels[i])
                    {
                        circleObj.Fill.Color = Color.White;
                    }
                    else
                    {
                        circleObj.Fill.Color = Color.SlateGray;
                    }
                }
            }
        }

        private void SetAllChannels(bool status)
        {
            for (int i = 0; i < SelectedChannels.Length; i++) 
            { 
                SelectedChannels[i] = status;
            }
        }

        private void ButtonAddPulses_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    if (!uint.TryParse(delaySamples.Text, out uint delay))
                    {
                        MessageBox.Show("Unable to parse Delay.");
                        return;
                    }

                    if (!byte.TryParse(amplitudeAnodicSteps.Text, out byte amplitudeAnodic))
                    {
                        MessageBox.Show("Unable to parse anodic Amplitude.");
                        return;
                    }

                    if (!uint.TryParse(pulseWidthAnodicSamples.Text, out uint pulseWidthAnodic))
                    {
                        MessageBox.Show("Unable to parse anodic Pulse Width.");
                        return;
                    }

                    if (!uint.TryParse(interPulseIntervalSamples.Text, out uint interPulseInterval))
                    {
                        MessageBox.Show("Unable to parse Inter-Pulse Interval.");
                        return;
                    }

                    byte amplitudeCathodic = 0;
                    uint pulseWidthCathodic = 0;

                    if (!checkboxBiphasicSymmetrical.Checked)
                    {
                        if (!byte.TryParse(this.amplitudeCathodicSteps.Text, out amplitudeCathodic))
                        {
                            MessageBox.Show("Unable to parse cathodic Amplitude.");
                            return;
                        }

                        if (!uint.TryParse(this.pulseWidthCathodicSamples.Text, out pulseWidthCathodic))
                        {
                            MessageBox.Show("Unable to parse cathodic Pulse Width.");
                            return;
                        }
                    }

                    if (!uint.TryParse(interStimulusIntervalSamples.Text, out uint isi))
                    {
                        MessageBox.Show("Unable to parse ISI.");
                        return;
                    }

                    if (!uint.TryParse(numberOfStimuliText.Text, out uint numberOfStimuli))
                    {
                        MessageBox.Show("Unable to parse Number of Stimuli.");
                        return;
                    }

                    Sequence.Stimuli[i].DelaySamples = delay;

                    Sequence.Stimuli[i].AnodicAmplitudeSteps = amplitudeAnodic;
                    Sequence.Stimuli[i].AnodicWidthSamples = pulseWidthAnodic;
                    
                    if (!checkboxBiphasicSymmetrical.Checked)
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

                    Sequence.Stimuli[i].AnodicFirst = checkBoxAnodicFirst.Checked;
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
            dataGridViewStimulusTable.BindingContext[dataGridViewStimulusTable.DataSource].EndCurrentEdit();
            DrawStimulusWaveform();
        }

        private void ComboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sequence.CurrentStepSize = (Rhs2116StepSize)comboBoxStepSize.SelectedItem;
            DrawStimulusWaveform();

            Amplitude_TextChanged(amplitudeAnodicSteps, e);
            Amplitude_TextChanged(amplitudeCathodicSteps, e);
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
                case nameof(delaySamples):
                    delaySamplesConverted.Text = text;
                    break;

                case nameof(pulseWidthAnodicSamples):
                    pulseWidthAnodicConverted.Text = text;
                    break;

                case nameof(interPulseIntervalSamples):
                    interPulseIntervalConverted.Text = text;
                    break;

                case nameof(pulseWidthCathodicSamples):
                    pulseWidthCathodicConverted.Text = text;
                    break;

                case nameof(interStimulusIntervalSamples):
                    interStimulusIntervalConverted.Text = text;
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
                case nameof(amplitudeAnodicSteps):
                    amplitudeAnodicConverted.Text = text;
                    break;

                case nameof(amplitudeCathodicSteps):
                    amplitudeCathodicConverted.Text = text;
                    break;

                default:
                    MessageBox.Show("Unknown text box.");
                    break;
            }
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
    }
}
