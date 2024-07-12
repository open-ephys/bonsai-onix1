using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public abstract partial class GenericDeviceDialog : Form
    {
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
