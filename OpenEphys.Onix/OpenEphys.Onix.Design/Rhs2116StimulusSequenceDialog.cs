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
        private const string SelectionAreaTag = "Selection";

        private ProbeGroup probeGroup = null;

        private PointD mouseLocation = new(0.0, 0.0);
        private PointD clickStart = new(0.0, 0.0);

        /// <summary>
        /// Opens a dialog allowing for easy changing of stimulus sequence parameters
        /// </summary>
        /// <param name="sequence"></param>
        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = new Rhs2116StimulusSequence(sequence);

            propertyGridStimulusSequence.SelectedObject = Sequence;

            SelectedChannels = new bool[Sequence.Stimuli.Length];
            SetAllChannels(true);

            comboBoxStepSize.DataSource = Enum.GetValues(typeof(Rhs2116StepSize));
            comboBoxStepSize.SelectedIndex = (int)Sequence.CurrentStepSize;

            InitializeZedGraphChannels();
            LoadDefaultChannelLayout();
            DrawChannels();

            InitializeZedGraphWaveform();
            DrawStimulusWaveform();

            dataGridViewStimulusTable.DataSource = Sequence.Stimuli;
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

            if (probeGroup == null)
            {
                MessageBox.Show("Error opening the JSON file.");
                return;
            }

            if (probeGroup.NumContacts != 32)
            {
                MessageBox.Show("Warning: Wrong number of contacts found in the file. " +
                    "Please confirm that there are 32 contacts across all probes.");
                probeGroup = null;
            }
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
            VisualizeSelectedChannels();
        }

        private void DrawStimulusWaveform()
        {
            zedGraphWaveform.GraphPane.CurveList.Clear();
            zedGraphWaveform.GraphPane.GraphObjList.Clear();
            zedGraphWaveform.ZoomOutAll(zedGraphWaveform.GraphPane);

            double peakToPeak = (Sequence.MaximumPeakToPeakAmplitudeSteps > 0 ?
                Sequence.CurrentStepSizeuA * Sequence.MaximumPeakToPeakAmplitudeSteps :
                Sequence.CurrentStepSizeuA * 1) * 1.1;

            var stimuli = Sequence.Stimuli;

            zedGraphWaveform.GraphPane.XAxis.Scale.Max = (Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1) * SamplePeriodMicroSeconds;
            zedGraphWaveform.GraphPane.XAxis.Scale.Min = -zedGraphWaveform.GraphPane.XAxis.Scale.Max * 0.03;
            zedGraphWaveform.GraphPane.YAxis.Scale.Min = -peakToPeak * stimuli.Length;
            zedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak;

            var contactTextLocation = zedGraphWaveform.GraphPane.XAxis.Scale.Min / 2;

            for (int i = 0; i < stimuli.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], -peakToPeak * i);

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

                    TextObj contactNumber = new(i.ToString(), contactTextLocation, curve.Points[0].Y)
                    {
                        Tag = string.Format(TextStringFormat, i)
                    };
                    contactNumber.FontSpec.Size = 12;
                    contactNumber.FontSpec.Border.IsVisible = false;
                    contactNumber.FontSpec.Fill.IsVisible = false;

                    zedGraphWaveform.GraphPane.GraphObjList.Add(contactNumber);
                }
            }

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

            zedGraphWaveform.GraphPane.YAxis.MajorGrid.IsZeroLine = false;

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
                    toolStripStatusIsValid.Text = "Stimulus sequence too complex";
                }
                else
                {
                    toolStripStatusIsValid.Image = Properties.Resources.StatusCriticalImage;
                    toolStripStatusIsValid.Text = "Stimulus sequence not valid";
                }
            }
        }

        private void SetPercentOfSlotsUsed()
        {
            toolStripStatusSlotsUsed.Text = string.Format("{0, 0:P1} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
        }

        private void DrawChannels()
        {
            if (probeGroup == null)
                return;

            zedGraphChannels.GraphPane.GraphObjList.Clear();

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

            zedGraphChannels.GraphPane.XAxis.Scale.Min = minX - rangeX * 0.02;
            zedGraphChannels.GraphPane.XAxis.Scale.Max = maxX + rangeX * 0.02;
            zedGraphChannels.GraphPane.YAxis.Scale.Min = minY - rangeY * 0.02;
            zedGraphChannels.GraphPane.YAxis.Scale.Max = maxY + rangeY * 0.02;

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
            if (e.Button != MouseButtons.Left || zedGraphChannels.GraphPane.GraphObjList[SelectionAreaTag] is BoxObj)
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
        private void SetSelectedChannel(string tag, bool status)
        {
            if (SelectedChannels.All(x => x))
                SetAllChannels(false);

            string[] words = tag.Split('_');
            if (int.TryParse(words[1], out int num))
            {
                SelectedChannels[num] = status;
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

            bool showAllChannels = SelectedChannels.All(x => x);

            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                EllipseObj circleObj = (EllipseObj)zedGraphChannels.GraphPane.GraphObjList[string.Format(ContactStringFormat, i)];

                if (circleObj != null)
                {
                    if (!Sequence.Stimuli[i].IsValid())
                    {
                        circleObj.Fill.Color = Color.Red;
                    }
                    else if (showAllChannels || !SelectedChannels[i])
                    {
                        circleObj.Fill.Color = Color.White;
                    }
                    else
                    {
                        circleObj.Fill.Color = Color.SlateGray;
                    }
                }
            }

            zedGraphChannels.Refresh();
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
            if (SelectedChannels.All(x => x))
            {
                DialogResult result = MessageBox.Show("Caution: All channels are currently selected, and all " +
                    "settings will be applied to all channels if you continue. Press Okay to add pulse settings to all channels, or Cancel to keep them as is",
                    "Set all channel settings?", MessageBoxButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    if (delay.Tag == null)
                    {
                        MessageBox.Show("Unable to parse delay.");
                        return;
                    }

                    if (amplitudeAnodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse anodic amplitude.");
                        return;
                    }

                    if (pulseWidthAnodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse anodic pulse width.");
                        return;
                    }

                    if (interPulseInterval.Tag == null)
                    {
                        MessageBox.Show("Unable to parse inter-pulse interval.");
                        return;
                    }

                    if (amplitudeCathodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse cathodic amplitude.");
                        return;
                    }

                    if (pulseWidthCathodic.Tag == null)
                    {
                        MessageBox.Show("Unable to parse cathodic pulse width.");
                        return;
                    }

                    if (interStimulusInterval.Tag == null)
                    {
                        MessageBox.Show("Unable to parse inter-stimulus interval.");
                        return;
                    }

                    if (!uint.TryParse(numberOfStimuli.Text, out uint numberOfStimuliValue))
                    {
                        MessageBox.Show("Unable to parse number of stimuli.");
                        return;
                    }

                    Sequence.Stimuli[i].DelaySamples = (uint)delay.Tag;

                    Sequence.Stimuli[i].AnodicAmplitudeSteps = (byte)amplitudeAnodic.Tag;
                    Sequence.Stimuli[i].AnodicWidthSamples = (uint)pulseWidthAnodic.Tag;

                    Sequence.Stimuli[i].CathodicAmplitudeSteps = (byte)amplitudeCathodic.Tag;
                    Sequence.Stimuli[i].CathodicWidthSamples = (uint)pulseWidthCathodic.Tag;

                    Sequence.Stimuli[i].DwellSamples = (uint)interPulseInterval.Tag;

                    Sequence.Stimuli[i].InterStimulusIntervalSamples = (uint)interStimulusInterval.Tag;

                    Sequence.Stimuli[i].NumberOfStimuli = numberOfStimuliValue;

                    Sequence.Stimuli[i].AnodicFirst = checkBoxAnodicFirst.Checked;
                }
            }

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
            AddContactIdToGridRow();
            DrawStimulusWaveform();
            VisualizeSelectedChannels();
        }

        private void DataGridViewStimulusTable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            AddContactIdToGridRow();
        }

        private void AddContactIdToGridRow()
        {
            if (probeGroup == null)
                return;

            var contactIds = probeGroup.GetContactIds();

            for (int i = 0; i < contactIds.Length; i++)
            {
                dataGridViewStimulusTable.Rows[i].HeaderCell.Value = contactIds[i];
            }
        }

        private void ComboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sequence.CurrentStepSize = (Rhs2116StepSize)comboBoxStepSize.SelectedItem;
            DrawStimulusWaveform();
            UpdateAmplitudeLabelUnits();

            if (amplitudeAnodic.Tag != null)
            {
                amplitudeAnodic.Text = GetAmplitudeString((byte)amplitudeAnodic.Tag);
            }

            if (amplitudeCathodic.Tag != null)
            {
                amplitudeCathodic.Text = GetAmplitudeString((byte)amplitudeCathodic.Tag);
            }
        }

        private string GetAmplitudeString(byte amplitude)
        {
            string format = Sequence.CurrentStepSize switch
            {
                Rhs2116StepSize.Step10nA or Rhs2116StepSize.Step20nA or Rhs2116StepSize.Step50nA => "{0:F2}",
                Rhs2116StepSize.Step100nA or Rhs2116StepSize.Step200nA or Rhs2116StepSize.Step500nA => "{0:F1}",
                Rhs2116StepSize.Step1000nA or Rhs2116StepSize.Step2000nA => "{0:F0}",
                Rhs2116StepSize.Step5000nA => "{0:F3}",
                Rhs2116StepSize.Step10000nA => "{0:F2}",
                _ => "{0:F3}",
            };
            return string.Format(format, GetAmplitudeFromSample(amplitude));
        }

        private string GetTimeString(uint time)
        {
            return string.Format("{0:F2}", GetTimeFromSample(time));
        }

        private double GetUnitConversion()
        {
            return Sequence.CurrentStepSize switch
            {
                Rhs2116StepSize.Step10nA or Rhs2116StepSize.Step20nA or Rhs2116StepSize.Step50nA or
                Rhs2116StepSize.Step100nA or Rhs2116StepSize.Step200nA or Rhs2116StepSize.Step500nA or
                Rhs2116StepSize.Step1000nA or Rhs2116StepSize.Step2000nA => 1,
                Rhs2116StepSize.Step5000nA or Rhs2116StepSize.Step10000nA => 1e3,
                _ => 1e6,
            };
        }

        private void UpdateAmplitudeLabelUnits()
        {
            switch (Sequence.CurrentStepSize)
            {
                case Rhs2116StepSize.Step10nA:
                case Rhs2116StepSize.Step20nA:
                case Rhs2116StepSize.Step50nA:
                case Rhs2116StepSize.Step100nA:
                case Rhs2116StepSize.Step200nA:
                case Rhs2116StepSize.Step500nA:
                case Rhs2116StepSize.Step1000nA:
                case Rhs2116StepSize.Step2000nA:
                    labelAmplitudeAnodic.Text = "Amplitude [μA]";
                    labelAmplitudeCathodic.Text = "Amplitude [μA]";
                    break;

                case Rhs2116StepSize.Step5000nA:
                case Rhs2116StepSize.Step10000nA:
                    labelAmplitudeAnodic.Text = "Amplitude [mA]";
                    labelAmplitudeCathodic.Text = "Amplitude [mA]";
                    break;

                default:
                    labelAmplitudeAnodic.Text = "Amplitude [μA]";
                    labelAmplitudeCathodic.Text = "Amplitude [μA]";
                    break;
            }
        }

        private void Samples_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "" || textBox.Text == "0" || textBox.Text == "0.0" || textBox.Text == "0.00" || textBox.Text == "0.000")
                return;

            if (double.TryParse(textBox.Text, out double result))
            {
                if (!GetSampleFromTime(result, out uint sampleTime))
                {
                    MessageBox.Show("Warning: Value was too small. Time is now set to zero seconds. Please increase the value.");
                }
                textBox.Text = GetTimeString(sampleTime);
                textBox.Tag = sampleTime;
            }
            else
            {
                MessageBox.Show("Unable to parse text. Please enter a valid value in milliseconds");
                textBox.Text = "";
                textBox.Tag = null;
            }

            if (groupBoxAnode.Visible && !groupBoxCathode.Visible)
            {
                pulseWidthCathodic.Text = textBox.Text;
                pulseWidthCathodic.Tag = textBox.Tag;
            }
            else if (groupBoxCathode.Visible && !groupBoxAnode.Visible)
            {
                pulseWidthAnodic.Text = textBox.Text;
                pulseWidthAnodic.Tag = textBox.Tag;
            }
        }

        private bool GetSampleFromTime(double value, out uint samples)
        {
            var ratio = value * 1e3 / SamplePeriodMicroSeconds;
            samples = (uint)Math.Round(ratio);

            return !(ratio > uint.MaxValue || ratio < uint.MinValue || samples == 0);
        }

        private bool GetSampleFromAmplitude(double value, out byte samples)
        {
            var ratio = value * GetUnitConversion() / Sequence.CurrentStepSizeuA;
            samples = (byte)Math.Round(ratio);

            return !(ratio > byte.MaxValue || ratio < 0 || samples == 0);
        }

        private double GetTimeFromSample(uint value)
        {
            return value * SamplePeriodMicroSeconds / 1e3;
        }

        private double GetAmplitudeFromSample(byte value)
        {
            return value * Sequence.CurrentStepSizeuA / GetUnitConversion();
        }

        private void Amplitude_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "" || textBox.Text == "0" || textBox.Text == "0.0" || textBox.Text == "0.00" || textBox.Text == "0.000")
                return;

            if (double.TryParse(textBox.Text, out double result))
            {
                if (!GetSampleFromAmplitude(result, out byte sampleAmplitude))
                {
                    if (sampleAmplitude == 0)
                    {
                        MessageBox.Show("Warning: amplitude is set to zero. Please increase the amplitude value and try again.");
                        textBox.Text = "";
                        textBox.Tag = null;
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Warning: Amplitude is too high for the given step-size. " +
                            "Please increase the amplitude step-size and try again.");
                        sampleAmplitude = byte.MaxValue;
                    }
                }

                textBox.Text = GetAmplitudeString(sampleAmplitude);
                textBox.Tag = sampleAmplitude;
            }
            else
            {
                MessageBox.Show("Unable to parse text. Please enter a valid value in milliamps");
                textBox.Text = "";
                textBox.Tag = null;
            }

            if (groupBoxAnode.Visible && !groupBoxCathode.Visible)
            {
                amplitudeCathodic.Text = textBox.Text;
                amplitudeCathodic.Tag = textBox.Tag;
            }
            else if (groupBoxCathode.Visible && !groupBoxAnode.Visible)
            {
                amplitudeAnodic.Text = textBox.Text;
                amplitudeAnodic.Tag = textBox.Tag;
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

        private void ButtonClearPulses_Click(object sender, EventArgs e)
        {
            if (SelectedChannels.All(x => x))
            {
                DialogResult result = MessageBox.Show("Caution: All channels are currently selected, and all " +
                    "settings will be cleared if you continue. Press Okay to clear all pulse settings, or Cancel to keep them",
                    "Remove all channel settings?", MessageBoxButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    Sequence.Stimuli[i].Clear();
                }
            }

            DrawStimulusWaveform();
        }

        private void ButtonDefaultChannelLayout_Click(object sender, EventArgs e)
        {
            SetAllChannels(true);
            LoadDefaultChannelLayout();
            DrawChannels();

            DrawStimulusWaveform();
        }

        private void ButtonCustomChannelLayout_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new();

            ofd.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(DefaultChannelLayoutFilePath));
            ofd.Filter = "Probe Interface Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose custom Probe Interface file";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofd.FileName))
                {
                    SetAllChannels(true);
                    LoadChannelLayout(ofd.FileName);
                    DrawChannels();

                    DrawStimulusWaveform();

                    textBoxChannelLayoutFilePath.Text = Path.GetFullPath(ofd.FileName);
                }
            }
        }

        private bool ZedGraphChannels_MouseDownEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                clickStart = TransformPixelsToCoordinates(e.Location);
            }

            return false; // Return true if I do not want ZedGraph to perform any additional work on the mouse down event
        }

        private bool ZedGraphChannels_MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (zedGraphChannels.Cursor != Cursors.Cross)
                {
                    zedGraphChannels.Cursor = Cursors.Cross;
                }

                mouseLocation = TransformPixelsToCoordinates(e.Location);

                BoxObj selectionArea = new(
                    mouseLocation.X < clickStart.X ? mouseLocation.X : clickStart.X,
                    mouseLocation.Y > clickStart.Y ? mouseLocation.Y : clickStart.Y,
                    Math.Abs(mouseLocation.X - clickStart.X),
                    Math.Abs(mouseLocation.Y - clickStart.Y));
                selectionArea.Border.Color = Color.DarkSlateGray;
                selectionArea.Fill.IsVisible = false;
                selectionArea.ZOrder = ZOrder.A_InFront;
                selectionArea.Tag = SelectionAreaTag;

                BoxObj oldArea = (BoxObj)zedGraphChannels.GraphPane.GraphObjList[SelectionAreaTag];
                if (oldArea != null)
                {
                    zedGraphChannels.GraphPane.GraphObjList.Remove(oldArea);
                }

                zedGraphChannels.GraphPane.GraphObjList.Add(selectionArea);
                zedGraphChannels.Refresh();

                return true;
            }
            else if (e.Button == MouseButtons.None)
            {
                zedGraphChannels.Cursor = Cursors.Arrow;

                return true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                return false;
            }

            return false;
        }

        private PointD TransformPixelsToCoordinates(Point pixels)
        {
            zedGraphChannels.GraphPane.ReverseTransform(pixels, out double x, out double y);

            return new PointD(x, y);
        }

        private bool ZedGraphChannels_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (zedGraphChannels.GraphPane.GraphObjList[SelectionAreaTag] is BoxObj selectionArea && selectionArea != null && probeGroup != null)
            {
                RectangleF rect = selectionArea.Location.Rect;

                if (!rect.IsEmpty)
                {
                    string[] ids = probeGroup.GetContactIds();

                    foreach (string id in ids)
                    {
                        if (zedGraphChannels.GraphPane.GraphObjList[string.Format(ContactStringFormat, id)] is EllipseObj contact && contact != null)
                        {
                            if (Contains(rect, contact.Location))
                            {
                                SetSelectedChannel(contact.Tag as string, true);
                            }
                        }
                    }
                }

                zedGraphChannels.GraphPane.GraphObjList.Remove(selectionArea);

                VisualizeSelectedChannels();
                DrawStimulusWaveform();
            }

            return true;
        }

        private bool Contains(RectangleF rect, Location location)
        {
            if (!rect.IsEmpty)
            {
                if (location != null)
                {
                    var x = location.X + location.Width / 2;
                    var y = location.Y - location.Height / 2;

                    if (x >= rect.X && x <= rect.X + rect.Width && y <= rect.Y && y >= rect.Y - rect.Height)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ButtonReadPulses_Click(object sender, EventArgs e)
        {
            if (SelectedChannels.Count(x => x) > 1)
            {
                MessageBox.Show("Too many contacts selected. Please choose a single channel to read from.");
                return;
            }

            int index = -1;

            for (int i = 0; i < SelectedChannels.Length; i++)
            {
                if (SelectedChannels[i])
                {
                    index = i; break;
                }
            }

            if (index < 0)
            {
                MessageBox.Show("Warning: No contact selected. Please choose a contact before continuing.");
                return;
            }

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

            delay.Text = GetTimeString(Sequence.Stimuli[index].DelaySamples); Samples_TextChanged(delay, e);
            amplitudeAnodic.Text = GetAmplitudeString(Sequence.Stimuli[index].AnodicAmplitudeSteps); Amplitude_TextChanged(amplitudeAnodic, e);
            pulseWidthAnodic.Text = GetTimeString(Sequence.Stimuli[index].AnodicWidthSamples); Samples_TextChanged(pulseWidthAnodic, e);
            amplitudeCathodic.Text = GetAmplitudeString(Sequence.Stimuli[index].CathodicAmplitudeSteps); Amplitude_TextChanged(amplitudeCathodic, e);
            pulseWidthCathodic.Text = GetTimeString(Sequence.Stimuli[index].CathodicWidthSamples); Samples_TextChanged(pulseWidthCathodic, e);
            interPulseInterval.Text = GetTimeString(Sequence.Stimuli[index].DwellSamples); Samples_TextChanged(interPulseInterval, e);
            interStimulusInterval.Text = GetTimeString(Sequence.Stimuli[index].InterStimulusIntervalSamples); Samples_TextChanged(interStimulusInterval, e);
            numberOfStimuli.Text = Sequence.Stimuli[index].NumberOfStimuli.ToString();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(DefaultChannelLayoutFilePath));
            sfd.Filter = "Stimulus Sequence Files (*.json)|*.json";
            sfd.FilterIndex = 1;
            sfd.Title = "Choose where to save the stimulus sequence file";
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var fileJson = JsonSerializer.Serialize(Sequence);

                File.WriteAllText(sfd.FileName, fileJson);
            }
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new();

            ofd.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(DefaultChannelLayoutFilePath));
            ofd.Filter = "Stimulus Sequence Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose saved stimulus sequence file";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true,
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                if (!File.Exists(ofd.FileName))
                {
                    MessageBox.Show("File does not exist.");
                    return;
                }

                var json = File.ReadAllText(ofd.FileName);

                Sequence = JsonSerializer.Deserialize<Rhs2116StimulusSequence>(json, options);

                DrawStimulusWaveform();
            }
        }
    }
}
