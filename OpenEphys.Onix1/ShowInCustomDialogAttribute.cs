using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies whether a property should be displayed in a custom dialog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class ShowInCustomDialogAttribute : Attribute
    {
        /// <summary>
        /// Specifies that the default value for this attribute is that a property should be displayed in a custom dialog.
        /// </summary>
        public static readonly ShowInCustomDialogAttribute Default = new(true);

        /// <summary>
        /// Gets a value indicating whether a property should be shown in a custom dialog.
        /// </summary>
        public bool ShowInCustomDialog { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInCustomDialogAttribute"/> class.
        /// </summary>
        /// <param name="showInCustomDialog">
        /// <see langword="true"/> if the property can be displayed in a custom dialog;
        /// otherwise, <see langword="false"/>. The default is <see langword="true"/>.
        /// </param>
        public ShowInCustomDialogAttribute(bool showInCustomDialog)
        {
            ShowInCustomDialog = showInCustomDialog;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="ShowInCustomDialogAttribute"/>
        /// and the state equals the state of this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            return obj is ShowInCustomDialogAttribute other && other.ShowInCustomDialog == ShowInCustomDialog;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return ShowInCustomDialog.GetHashCode();
        }
    }
}
