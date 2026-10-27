using System;
using System.Text;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Marshals strings to and from the fixed-size, null-terminated UTF-8 buffers ImGui's text inputs edit.
    /// </summary>
    internal static class ImGuiStrings
    {
        public static void WriteString(byte[] buf, string value)
        {
            Array.Clear(buf, 0, buf.Length);
            if (!string.IsNullOrEmpty(value))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                Array.Copy(bytes, buf, Math.Min(bytes.Length, buf.Length - 1));
            }
        }

        public static string ReadBuffer(byte[] buf)
        {
            int len = Array.IndexOf(buf, (byte)0);
            return Encoding.UTF8.GetString(buf, 0, len < 0 ? buf.Length : len);
        }
    }
}
