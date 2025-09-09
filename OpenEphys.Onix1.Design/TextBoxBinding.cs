using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    internal class TextBoxBinding<T>
    {
        readonly TextBox textBox;
        readonly Func<T, T> setter;
        readonly Func<string, T> parser;
        readonly T defaultValue;

        public TextBoxBinding(
            TextBox textBox,
            Func<T, T> setter,
            Func<string, T> parser,
            T defaultValue = default)
        {
            this.textBox = textBox;
            this.setter = setter;
            this.parser = parser;
            this.defaultValue = defaultValue;
        }

        public void UpdateFromTextBox()
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                setter(defaultValue);
                return;
            }

            try
            {
                var value = parser(textBox.Text);
                value = setter(value);
                textBox.Text = value.ToString();
            }
            catch
            {
                setter(defaultValue);
                textBox.Text = defaultValue.ToString();
            }
        }
    }
}
