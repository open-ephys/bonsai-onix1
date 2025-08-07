using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class that holds the stimulus definition UI elements for a <see cref="Headstage64OpticalStimulatorSequenceDialog"/>.
    /// </summary>
    public partial class Headstage64OpticalStimulatorOptions : Form
    {
        /// <summary>
        /// Initialize a new <see cref="Headstage64OpticalStimulatorOptions"/> dialog.
        /// </summary>
        public Headstage64OpticalStimulatorOptions()
        {
            InitializeComponent();
        }

        private void SynchronizeTextBoxAndTrackBar(TextBox textBox, TrackBar trackBar, double value)
        {
            if (textBox == null || trackBar == null)
            {
                return;
            }

            textBox.Text = value.ToString();
            trackBar.Value = (int)value;
        }

        private double VerifyPercentLimits(double value)
        {
            double MinimumPercent = 0;
            double MaximumPercent = 100;

            if (value < MinimumPercent)
            {
                return MinimumPercent;
            }
            else if (value > MaximumPercent)
            {
                return MaximumPercent;
            }
            else
            {
                return value;
            }
        }

        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                ChannelPercentTextChanged(sender, null);
            }
        }

        private void ChannelPercentTextChanged(object sender, EventArgs eventArgs)
        {
            if (sender is TextBox tb)
            {
                if (tb.Name == textBoxChannelOnePercent.Name)
                {
                    if (double.TryParse(tb.Text, out double result))
                    {
                        result = VerifyPercentLimits(result);
                        SynchronizeTextBoxAndTrackBar(textBoxChannelOnePercent, trackBarChannelOnePercent, result);
                    }
                    else
                    {
                        SynchronizeTextBoxAndTrackBar(textBoxChannelOnePercent, trackBarChannelOnePercent, 0);
                    }
                }
                else if (tb.Name == textBoxChannelTwoPercent.Name)
                {
                    if (double.TryParse(tb.Text, out double result))
                    {
                        result = VerifyPercentLimits(result);
                        SynchronizeTextBoxAndTrackBar(textBoxChannelTwoPercent, trackBarChannelTwoPercent, result);
                    }
                    else
                    {
                        SynchronizeTextBoxAndTrackBar(textBoxChannelTwoPercent, trackBarChannelTwoPercent, 0);
                    }
                }
            }
        }

        private void ChannelPercentTrackBarChanged(object sender, EventArgs eventArgs)
        {
            if (sender is TrackBar tb)
            {
                if (tb.Name == trackBarChannelOnePercent.Name)
                {
                    SynchronizeTextBoxAndTrackBar(textBoxChannelOnePercent, trackBarChannelOnePercent, trackBarChannelOnePercent.Value);
                }
                else if (tb.Name == trackBarChannelTwoPercent.Name)
                {
                    SynchronizeTextBoxAndTrackBar(textBoxChannelTwoPercent, trackBarChannelTwoPercent, trackBarChannelTwoPercent.Value);
                }
            }
        }
    }
}
