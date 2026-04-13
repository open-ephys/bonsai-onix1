namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageRhs2116"/>. Hosts a
    /// <see cref="Rhs2116StimulusSequenceDialog"/> and a <see cref="GenericDeviceDialog"/> for
    /// the Rhs2116 pair, each in its own tab.
    /// </summary>
    public class HeadstageRhs2116Dialog : HeadstageDialog
    {
        internal readonly Rhs2116StimulusSequenceDialog StimulusSequenceDialog;
        internal readonly GenericDeviceDialog Rhs2116Dialog;

        /// <summary>
        /// Initializes a new instance of a <see cref="HeadstageRhs2116Dialog"/>.
        /// </summary>
        /// <param name="rhs2116Trigger">Current configuration settings for <see cref="ConfigureRhs2116Trigger"/>.</param>
        /// <param name="rhs2116">Current configuration settings for a single <see cref="ConfigureRhs2116"/>.</param>
        public HeadstageRhs2116Dialog(ConfigureRhs2116Trigger rhs2116Trigger, ConfigureRhs2116Pair rhs2116)
        {
            Text = "HeadstageRhs2116 Configuration";

            StimulusSequenceDialog = new Rhs2116StimulusSequenceDialog(rhs2116Trigger, true);
            AddSequenceTab("Stimulus Sequence", StimulusSequenceDialog);

            Rhs2116Dialog = new(rhs2116, true);
            AddDeviceTab("Rhs2116", Rhs2116Dialog);
        }
    }
}
