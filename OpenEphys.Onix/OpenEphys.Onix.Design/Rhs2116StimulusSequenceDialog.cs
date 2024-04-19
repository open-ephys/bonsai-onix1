using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        public Rhs2116StimulusSequence Sequence;

        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = ObjectExtensions.Copy(sequence);

            PropertyGridStimulusSequence.SelectedObject = Sequence;

            ZedGraphWaveform.IsAutoScrollRange = true;

            InitializeZedGraph();
            DrawStimulusWaveform();
        }

        private void LinkLabelDocumentation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://open-ephys.github.io/onix-docs/Software%20Guide/Bonsai.ONIX/Nodes/RHS2116TriggerDevice.html");
            }
            catch 
            {
                MessageBox.Show("Unable to open documentation link.");
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
                PointPairList pointPairs = CreateStimulusWaveform(stimuli[i], Sequence.CurrentStepSizeuA, Sequence.SequenceLengthSamples, -peakToPeak * i);
                var curve = ZedGraphWaveform.GraphPane.AddCurve("Test", pointPairs, Color.CornflowerBlue, SymbolType.None);

                curve.Label.IsVisible = false;
                curve.Line.Width = 3;
            }

            // Autoscale to view all data
            ZedGraphWaveform.GraphPane.XAxis.Scale.Min = 0;
            ZedGraphWaveform.GraphPane.XAxis.Scale.Max = Sequence.SequenceLengthSamples > 0 ? Sequence.SequenceLengthSamples : 1;
            ZedGraphWaveform.GraphPane.YAxis.Scale.Min = -peakToPeak * (stimuli.Length + 0.5);
            ZedGraphWaveform.GraphPane.YAxis.Scale.Max = peakToPeak * 1.5;

            // Change step size
            ZedGraphWaveform.GraphPane.XAxis.Scale.MajorStep = 1;
            ZedGraphWaveform.GraphPane.XAxis.Scale.MinorStep = 0;
            ZedGraphWaveform.GraphPane.YAxis.Scale.MajorStep = peakToPeak * 1.5;
            ZedGraphWaveform.GraphPane.YAxis.Scale.MinorStep = 0;

            ZedGraphWaveform.AxisChange();

            // Redraw waveforms
            ZedGraphWaveform.Refresh();
        }

        private PointPairList CreateStimulusWaveform(Rhs2116Stimulus stimulus, double currentStepSize, double sequenceLength, double yOffset)
        {
            PointPairList points = new PointPairList
            {
                { 0, yOffset },
                { stimulus.DelaySamples, yOffset }
            };

            for (int i = 0; i < stimulus.NumberOfStimuli; i++)
            {
                double amplitude = (stimulus.AnodicFirst ? stimulus.AnodicAmplitudeSteps : stimulus.CathodicAmplitudeSteps) * currentStepSize + yOffset;
                double width = stimulus.AnodicFirst ? stimulus.AnodicWidthSamples : stimulus.CathodicWidthSamples;

                // TODO?: Make this block (i.e., anodic/cathodic pulse) a function that returns a list of points to minimize errors
                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.DwellSamples, yOffset);

                amplitude = -(stimulus.AnodicFirst ? stimulus.CathodicAmplitudeSteps : stimulus.AnodicAmplitudeSteps) * currentStepSize + yOffset;
                width = stimulus.AnodicFirst ? stimulus.CathodicWidthSamples : stimulus.AnodicWidthSamples;

                points.Add(points[points.Count - 1].X, amplitude);
                points.Add(points[points.Count - 1].X + width, amplitude);
                points.Add(points[points.Count - 1].X, yOffset);

                points.Add(points[points.Count - 1].X + stimulus.InterStimulusIntervalSamples, yOffset);
            }

            points.Add(sequenceLength, yOffset);

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
            ZedGraphWaveform.GraphPane.XAxis.Title.Text = "Time [microseconds]";
            ZedGraphWaveform.GraphPane.YAxis.Title.Text = "Amplitude [microamps]";
        }
    }
}
