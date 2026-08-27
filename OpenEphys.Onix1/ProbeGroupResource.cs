using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace OpenEphys.Onix1
{
    static class ProbeGroupResource
    {
        static readonly Assembly Assembly = typeof(ProbeGroupResource).Assembly;

        internal static T LoadDefault<T>(string defaultProbeInterfaceFileName) where T : class
        {
            var json = LoadDefaultJson(defaultProbeInterfaceFileName);
            return JsonConvert.DeserializeObject<T>(json)
                ?? throw new InvalidOperationException($"Failed to deserialise {defaultProbeInterfaceFileName}.");
        }

        /// <summary>
        /// Reads the raw JSON text of an embedded default probe-interface resource, without
        /// deserializing it. Exposed (via <c>InternalsVisibleTo</c>) so design-time quick-load UI can
        /// route a bundled default through the exact same deserialization path used for a
        /// user-browsed file.
        /// </summary>
        internal static string LoadDefaultJson(string defaultProbeInterfaceFileName)
        {
            var name = $"OpenEphys.Onix1.Resources.{defaultProbeInterfaceFileName}";
            using var stream = Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing embedded resource: {name}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Reads an embedded resource as a flat array of little-endian <see cref="ushort"/> values.
        /// </summary>
        internal static ushort[] LoadUInt16Array(string resourceFileName)
        {
            var name = $"OpenEphys.Onix1.Resources.{resourceFileName}";
            using var stream = Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing embedded resource: {name}");
            using var reader = new BinaryReader(stream);

            var bytes = reader.ReadBytes((int)stream.Length);
            if (bytes.Length % 2 != 0)
                throw new InvalidOperationException($"Embedded resource '{name}' has an odd byte length; expected 16-bit values.");

            var values = new ushort[bytes.Length / 2];
            for (int i = 0; i < values.Length; i++)
                values[i] = BitConverter.ToUInt16(bytes, i * 2);

            return values;
        }
    }
}
