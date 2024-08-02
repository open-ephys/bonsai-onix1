using System;

namespace OpenEphys.Onix1.Design
{
    public class ContactTag
    {
        public const string ContactStringFormat = "Probe_{0}-Contact_{1}";
        public const string TextStringFormat = "TextProbe_{0}-Contact_{1}";

        public int ProbeNumber;
        public int ContactNumber;

        public string ContactString => GetContactString(ProbeNumber, ContactNumber);

        public string TextString => GetTextString(ProbeNumber, ContactNumber);

        public ContactTag(int probeNumber, int contactNumber)
        {
            ProbeNumber = probeNumber;
            ContactNumber = contactNumber;
        }

        public ContactTag(string tag)
        {
            ProbeNumber = ParseProbeNumber(tag);
            ContactNumber = ParseContactNumber(tag);
        }

        public static int ParseProbeNumber(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new NullReferenceException(nameof(tag));

            string[] words = tag.Split('-');
            string[] probeStrings = words[0].Split('_');

            if (!int.TryParse(probeStrings[1], out int probeNumber))
            {
                throw new ArgumentException($"Invalid channel tag \"{tag}\" found");
            }

            return probeNumber;
        }

        public static int ParseContactNumber(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new NullReferenceException(nameof(tag));

            string[] words = tag.Split('-');
            string[] contactStrings = words[1].Split('_');

            if (!int.TryParse(contactStrings[1], out int contactNumber))
            {
                throw new ArgumentException($"Invalid channel tag \"{tag}\" found");
            }

            return contactNumber;
        }

        public static string GetContactString(int probeNumber, int contactNumber)
        {
            return string.Format(ContactStringFormat, probeNumber, contactNumber);
        }

        public static string GetTextString(int probeNumber, int contactNumber)
        {
            return string.Format(TextStringFormat, probeNumber, contactNumber);
        }
    }

}
