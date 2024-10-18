namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Public class used to create tags for contacts in their respective GUIs.
    /// </summary>
    public class ContactTag
    {
        const string ContactStringFormat = "Probe_{0}-Contact_{1}";
        const string TextStringFormat = "TextProbe_{0}-Contact_{1}";

        /// <summary>
        /// Gets the probe index of this contact.
        /// </summary>
        public int ProbeIndex { get; }

        /// <summary>
        /// Gets the contact index of this contact.
        /// </summary>
        public int ContactIndex { get; }

        /// <summary>
        /// Gets the string defining the probe and contact index for this contact.
        /// </summary>
        public string ContactString => GetContactString(ProbeIndex, ContactIndex);

        /// <summary>
        /// Gets the string defining the probe and contact index of a text object for this contact.
        /// </summary>
        public string TextString => GetTextString(ProbeIndex, ContactIndex);

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactTag"/> class with the given indices.
        /// </summary>
        /// <param name="probeIndex">Index of the probe for this contact.</param>
        /// <param name="contactIndex">Index of the contact for this contact.</param>
        public ContactTag(int probeIndex, int contactIndex)
        {
            ProbeIndex = probeIndex;
            ContactIndex = contactIndex;
        }

        static string GetContactString(int probeNumber, int contactNumber)
        {
            return string.Format(ContactStringFormat, probeNumber, contactNumber);
        }

        static string GetTextString(int probeNumber, int contactNumber)
        {
            return string.Format(TextStringFormat, probeNumber, contactNumber);
        }
    }
}
