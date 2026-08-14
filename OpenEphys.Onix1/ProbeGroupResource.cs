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
    }
}
