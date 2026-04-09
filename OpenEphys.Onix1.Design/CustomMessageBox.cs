using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Create a custom message box form that allows for custom button names
    /// </summary>
    public partial class CustomMessageBox : Form
    {
        /// <summary>
        /// Specifies the button that was clicked in a <see cref="CustomMessageBox"/>.
        /// </summary>
        public enum CustomMessageBoxButton
        {
            /// <summary>
            /// Specifies that the first button was clicked.
            /// </summary>
            Button1,
            /// <summary>
            /// Specifies that the second button was clicked.
            /// </summary>
            Button2,
            /// <summary>
            /// Specifies that the third button was clicked.
            /// </summary>
            Button3
        }

        /// <summary>
        /// Specifies the button combinations that can be used in a <see cref="CustomMessageBox"/>.
        /// </summary>
        public enum CustomMessageBoxButtons
        {
            /// <summary>
            /// Specifies that there are two buttons, "Confirm" and "Cancel"
            /// </summary>
            ConfirmCancel,
            /// <summary>
            /// Specifies that there are three buttons, "Save", "Save As", and "Cancel"
            /// </summary>
            SaveSaveAsCancel,
            /// <summary>
            /// Specifies that there are three buttons, "Save", "Discard", and "Cancel"
            /// </summary>
            SaveDiscardCancel
        }

        /// <summary>
        /// Gets the button that was clicked in the <see cref="CustomMessageBox"/>.
        /// </summary>
        public CustomMessageBoxButton ClickedButton { get; private set; }

        CustomMessageBox(string message, string title)
        {
            InitializeComponent();

            label1.Text = message;
            Text = title;

            var size = TextRenderer.MeasureText(label1.Text, label1.Font, label1.Size, TextFormatFlags.WordBreak);

            if (size.Height > label1.Height)
            {
                Height += size.Height - label1.Height;
            }

            button1.Click += (sender, e) =>
            {
                ClickedButton = CustomMessageBoxButton.Button1;
                DialogResult = button1.Text.Contains("Cancel") ? DialogResult.Cancel : DialogResult.OK;
                Close();
            };

            button2.Click += (sender, e) =>
            {
                ClickedButton = CustomMessageBoxButton.Button2;
                DialogResult = button2.Text.Contains("Cancel") ? DialogResult.Cancel : DialogResult.OK;
                Close();
            };

            button3.Click += (sender, e) =>
            {
                ClickedButton = CustomMessageBoxButton.Button3;
                DialogResult = button3.Text.Contains("Cancel") ? DialogResult.Cancel : DialogResult.OK;
                Close();
            };
        }

        /// <summary>
        /// Initialize a new custom message box form
        /// </summary>
        public CustomMessageBox(string message, string title, string button1Text = "", string button2Text = "", string button3Text = "")
            : this(message, title)
        {
            InitializeButtons(button1Text, button2Text, button3Text);
        }

        /// <summary>
        /// Initializes a new custom message box form with a specified button combination from the <see cref="CustomMessageBoxButtons"/> enum.
        /// </summary>
        /// <param name="message">Message to display in the custom message box.</param>
        /// <param name="buttons">Button combination to use in the custom message box.</param>
        /// <param name="title">Title of the custom message box.</param>
        public CustomMessageBox(string message, CustomMessageBoxButtons buttons, string title)
            : this(message, title)
        {
            string button1Text = "", button2Text = "", button3Text = "";

            switch (buttons)
            {
                case CustomMessageBoxButtons.ConfirmCancel:
                    button1Text = "Confirm";
                    button2Text = "Cancel";
                    break;
                case CustomMessageBoxButtons.SaveSaveAsCancel:
                    button1Text = "Save";
                    button2Text = "Save As";
                    button3Text = "Cancel";
                    break;
                case CustomMessageBoxButtons.SaveDiscardCancel:
                    button1Text = "Save";
                    button2Text = "Discard";
                    button3Text = "Cancel";
                    break;
            }

            InitializeButtons(button1Text, button2Text, button3Text);
        }

        void InitializeButtons(string button1Text, string button2Text, string button3Text)
        {
            button1.Text = button1Text;
            button1.Visible = !string.IsNullOrEmpty(button1Text);

            button2.Text = button2Text;
            button2.Visible = !string.IsNullOrEmpty(button2Text);

            button3.Text = button3Text;
            button3.Visible = !string.IsNullOrEmpty(button3Text);
        }
    }
}
