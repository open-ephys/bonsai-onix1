using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Contract for probe dialogs that can be hosted as a tab inside a <see cref="HeadstageDialog"/>.
    /// </summary>
    internal interface IProbeInterfaceDialog
    {
        /// <summary>Gets whether the dialog has unsaved changes.</summary>
        bool HasChanges { get; }

        /// <summary>Raised whenever the unsaved-changes state transitions.</summary>
        event EventHandler OnStateChange;

        /// <summary>Routes a keyboard shortcut to the dialog's inner controls.</summary>
        bool ProcessMenuShortcut(Keys keyData);
    }
}
