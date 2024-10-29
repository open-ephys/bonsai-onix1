using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Create a custom message box form that allows for custom button names
    /// </summary>
    public partial class CustomMessageBox : Form
    {
        /// <summary>
        /// Initialize a new custom message box form
        /// </summary>
        public CustomMessageBox(string message, string title = "Message", string confirmText = "Confirm", string cancelText = "Cancel")
        {
            InitializeComponent();

            label1.Text = message;

            var size = TextRenderer.MeasureText(label1.Text, label1.Font, label1.Size, TextFormatFlags.WordBreak);

            if (size.Height > label1.Height)
            {
                Height += size.Height - label1.Height;
            }

            Text = title;
            buttonConfirm.Text = confirmText;
            buttonCancel.Text = cancelText;
        }
    }
}
