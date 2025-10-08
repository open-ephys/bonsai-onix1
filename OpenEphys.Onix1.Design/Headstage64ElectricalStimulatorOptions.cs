using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class that holds the stimulus definition UI elements for a <see cref="Headstage64ElectricalStimulatorSequenceDialog"/>.
    /// </summary>
    public partial class Headstage64ElectricalStimulatorOptions : Form
    {
        /// <summary>
        /// Initialize a new <see cref="Headstage64ElectricalStimulatorOptions"/> dialog.
        /// </summary>
        public Headstage64ElectricalStimulatorOptions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes an instance of the <see cref="Headstage64ElectricalStimulatorOptions"/> form with the given sequence.
        /// </summary>
        /// <param name="electricalStimulator"></param>
        public Headstage64ElectricalStimulatorOptions(ConfigureHeadstage64ElectricalStimulator electricalStimulator)
            : this()
        {
            textBoxPhaseOneCurrent.Text = electricalStimulator.PhaseOneCurrent.ToString();
            textBoxPhaseOneDuration.Text = electricalStimulator.PhaseOneDuration.ToString();

            textBoxInterPhaseCurrent.Text = electricalStimulator.InterPhaseCurrent.ToString();
            textBoxInterPhaseDuration.Text = electricalStimulator.InterPhaseInterval.ToString();

            textBoxPhaseTwoCurrent.Text = electricalStimulator.PhaseTwoCurrent.ToString();
            textBoxPhaseTwoDuration.Text = electricalStimulator.PhaseTwoDuration.ToString();

            textBoxPulsePeriod.Text = electricalStimulator.InterPulseInterval.ToString();
            textBoxBurstPulseCount.Text = electricalStimulator.BurstPulseCount.ToString();
            textBoxInterBurstInterval.Text = electricalStimulator.InterBurstInterval.ToString();
            textBoxTrainBurstCount.Text = electricalStimulator.TrainBurstCount.ToString();
            textBoxTrainDelay.Text = electricalStimulator.TriggerDelay.ToString();
        }

        void BurstPulseCountChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(textBoxBurstPulseCount.Text, out int result))
            {
                textBoxPulsePeriod.Enabled = result > 1;
            }
        }

        void TrainBurstCountChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(textBoxTrainBurstCount.Text, out int result))
            {
                textBoxInterBurstInterval.Enabled = result > 1;
            }
        }
    }
}
