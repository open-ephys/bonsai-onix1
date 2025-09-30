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

        /// <summary>
        /// Initializes a copy instance of the <see cref="Headstage64OpticalStimulatorOptions"/> form with the given opticalStimulator.
        /// </summary>
        /// <param name="opticalStimulator"></param>
        public Headstage64OpticalStimulatorOptions(ConfigureHeadstage64OpticalStimulator opticalStimulator)
            : this()
        {
            channelOneScalingFactor = trackBarChannelOnePercent.Maximum / 100;
            channelTwoScalingFactor = trackBarChannelTwoPercent.Maximum / 100;

            UpdateSequenceParameters(opticalStimulator);
        }

        internal void UpdateSequenceParameters(ConfigureHeadstage64OpticalStimulator opticalStimulator)
        {
            textBoxMaxCurrent.Text = opticalStimulator.MaxCurrent.ToString();
            textBoxPulseDuration.Text = opticalStimulator.PulseDuration.ToString();
            textBoxPulsePeriod.Text = opticalStimulator.PulsesPerSecond.ToString();
            textBoxPulsePeriod.Enabled = opticalStimulator.PulsesPerBurst > 1;

            textBoxChannelOnePercent.Text = opticalStimulator.ChannelOneCurrent.ToString();
            trackBarChannelOnePercent.Value = (int)(opticalStimulator.ChannelOneCurrent * channelOneScalingFactor);
            textBoxChannelTwoPercent.Text = opticalStimulator.ChannelTwoCurrent.ToString();
            trackBarChannelTwoPercent.Value = (int)(opticalStimulator.ChannelTwoCurrent * channelTwoScalingFactor);

            textBoxPulsesPerBurst.Text = opticalStimulator.PulsesPerBurst.ToString();
            textBoxInterBurstInterval.Text = opticalStimulator.InterBurstInterval.ToString();
            textBoxInterBurstInterval.Enabled = opticalStimulator.BurstsPerTrain > 1;
            textBoxBurstsPerTrain.Text = opticalStimulator.BurstsPerTrain.ToString();
            textBoxDelay.Text = opticalStimulator.Delay.ToString();
        }

        internal readonly double channelOneScalingFactor;
        internal readonly double channelTwoScalingFactor;
    }
}
