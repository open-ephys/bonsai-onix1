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

            switch (textBox.Name)
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
    }
}
