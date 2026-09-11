using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Content hosted as one tab (or the sole panel) inside an <see cref="ImGuiShellDialog"/>.
    /// </summary>
    internal interface IImGuiTabPanel
    {
        /// <summary>
        /// Gets whether the panel has unsaved changes.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Called when the shell is about to close with the given pending result. Return
        /// <see langword="false"/> to veto the close (e.g. the user cancelled a save prompt).
        /// </summary>
        bool CanClose(DialogResult pendingResult);

        /// <summary>
        /// Draws the panel's content. Called once per frame while its tab is active.
        /// </summary>
        void Draw();
    }
}
