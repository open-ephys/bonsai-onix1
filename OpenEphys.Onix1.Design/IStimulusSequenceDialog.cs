using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Contract for stimulus sequence dialogs that can be hosted as a tab inside a
    /// <see cref="HeadstageDialog"/>. The dialog is responsible for validating its own
    /// sequence and reporting whether the headstage dialog may proceed to close.
    /// </summary>
    internal interface IStimulusSequenceDialog
    {
        /// <summary>
        /// Validates the sequence. If invalid, prompts the user to discard or keep editing.
        /// Always sets <see cref="Form.DialogResult"/> on the implementing
        /// form before returning.
        /// </summary>
        /// <param name="result">
        /// <see cref="DialogResult.OK"/> when the sequence is valid or the user chose to
        /// discard invalid changes; <see cref="DialogResult.Cancel"/> when the user chose to
        /// keep editing.
        /// </param>
        /// <param name="stimulusName">Human-readable name used in the warning prompt.</param>
        /// <returns>
        /// <see langword="true"/> if the headstage dialog may proceed to close;
        /// <see langword="false"/> if the user wants to stay in the dialog.
        /// </returns>
        bool CanCloseForm(out DialogResult result, string stimulusName);
    }
}
