using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Newtonsoft.Json;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Loads embedded default probe-interface resources bundled with the GUI
    /// (used to seed a dialog with a starting geometry).
    /// </summary>
    static class DesignResource
    {
        static readonly Assembly Assembly = typeof(DesignResource).Assembly;

        internal static T LoadDefault<T>(string defaultProbeInterfaceFileName) where T : class
        {
            var json = LoadDefaultJson(defaultProbeInterfaceFileName);
            return JsonConvert.DeserializeObject<T>(json)
                ?? throw new InvalidOperationException($"Failed to deserialise {defaultProbeInterfaceFileName}.");
        }

        /// <summary>
        /// Reads and decompresses the raw JSON text of a gzip-compressed embedded default
        /// probe-interface resource, without deserializing it.
        /// </summary>
        internal static string LoadDefaultJson(string defaultProbeInterfaceFileName)
        {
            var name = $"OpenEphys.Onix1.Design.Resources.{defaultProbeInterfaceFileName}";
            using var stream = Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing embedded resource: {name}");
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            return reader.ReadToEnd();
        }
    }
}
