using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using OpenEphys.Onix;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        public Rhs2116StimulusSequence Sequence;

        private const double SamplePeriodMicroSeconds = 1e6 / 30.1932367151e3;

        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = ObjectExtensions.Copy(sequence);

            PropertyGridStimulusSequence.SelectedObject = Sequence;

            ZedGraphWaveform.IsAutoScrollRange = true;

            InitializeZedGraph();
            DrawStimulusWaveform();

            SetStatusValidity();
            SetNumberOfSlotsUsed();
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
            SetStatusValidity();
            SetNumberOfSlotsUsed();
        }

        private void DrawStimulusWaveform()
        {
            // Clear curves and reset zoom
            ZedGraphWaveform.GraphPane.CurveList.Clear();
            ZedGraphWaveform.ZoomOutAll(ZedGraphWaveform.GraphPane);

            // TODO: Is it better to have a different pane for each stimulus? Might make more sense from a user perspective to have the units that way

            double peakToPeak = Sequence.MaximumPeakToPeakAmplitudeSteps > 0 ? 
                Sequence.CurrentStepSizeuA * Sequence.MaximumPeakToPeakAmplitudeSteps :
                Sequence.CurrentStepSizeuA * 1;

            // Add stimulus waveforms to graph
            var stimuli = Sequence.Stimuli;

            for (int i = 0; i < stimuli.Length; i++)
            {
                PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], -peakToPeak * i);
                var curve = ZedGraphWaveform.GraphPane.AddCurve("Test", pointPairs, Color.CornflowerBlue, SymbolType.None);

                curve.Label.IsVisible = false;
                curve.Line.Width = 3;
            }

            // Autoscale to view all data
            ZedGraphWaveform.GraphPane.XAxis.Scale.Min = 0;
            ZedGraphWaveform.GraphPane.XAxis.Scale.Max = (Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1) * SamplePeriodMicroSeconds;
            ZedGraphWaveform.GraphPane.YAxis.Scale.Min = -peakToPeak * (stimuli.Length + 0.5);
            ZedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak * 1.5;

            // Change step size
            ZedGraphWaveform.GraphPane.YAxis.Scale.MajorStep = peakToPeak * 1.5;
            ZedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;

            ZedGraphWaveform.AxisChange();

            // Redraw waveforms
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
                double amplitude = (stimulus.AnodicFirst ? stimulus.AnodicAmplitudeSteps : stimulus.CathodicAmplitudeSteps) * Sequence.CurrentStepSizeuA + yOffset;
                double width = (stimulus.AnodicFirst ? stimulus.AnodicWidthSamples : stimulus.CathodicWidthSamples) * SamplePeriodMicroSeconds;

                // TODO?: Make this block (i.e., anodic/cathodic pulse) a function that returns a list of points to minimize errors
                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.DwellSamples * SamplePeriodMicroSeconds, yOffset);

                amplitude = -(stimulus.AnodicFirst ? stimulus.CathodicAmplitudeSteps : stimulus.AnodicAmplitudeSteps) * Sequence.CurrentStepSizeuA + yOffset;
                width = (stimulus.AnodicFirst ? stimulus.CathodicWidthSamples : stimulus.AnodicWidthSamples) * SamplePeriodMicroSeconds;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.InterStimulusIntervalSamples * SamplePeriodMicroSeconds, yOffset);
            }

            points.Add(Sequence.SequenceLengthSamples * SamplePeriodMicroSeconds, yOffset);

            return points;
        }

        private void InitializeZedGraph()
        {
            // Change settings for visibility
            ZedGraphWaveform.GraphPane.Title.IsVisible = false;
            ZedGraphWaveform.GraphPane.TitleGap = 0;
            ZedGraphWaveform.GraphPane.Border.IsVisible = false;
            ZedGraphWaveform.GraphPane.IsFontsScaled = false;

            // Remove Ticks from axis
            ZedGraphWaveform.GraphPane.XAxis.MajorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.XAxis.MinorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.YAxis.MajorTic.IsAllTics = false;
            ZedGraphWaveform.GraphPane.YAxis.MinorTic.IsAllTics = false;

            // Add Axis labels and units
            ZedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [μs]";
            ZedGraphWaveform.GraphPane.YAxis.Title.Text = "Amplitude [μA]";
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

        private void SetNumberOfSlotsUsed()
        {
            ToolStripStatusSlotsUsed.Text = string.Format("{0, 0:0%} of slots used", (double)Sequence.StimulusSlotsRequired / Sequence.MaxMemorySlotsAvailable);
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
    }
}
