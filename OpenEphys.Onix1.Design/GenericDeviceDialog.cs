using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Abstract form that implements a very basic GUI consisting of a single property grid and
    /// two buttons (OK / Cancel).
    /// </summary>
    public abstract partial class GenericDeviceDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GenericDeviceDialog"/>.
        /// </summary>
        public GenericDeviceDialog()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, System.EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == nameof(buttonOK))
                {
                    DialogResult = DialogResult.OK;
                }
                else if (button.Name == nameof(buttonCancel))
                {
                    DialogResult = DialogResult.Cancel;
                }
            }
        }
    }
}
