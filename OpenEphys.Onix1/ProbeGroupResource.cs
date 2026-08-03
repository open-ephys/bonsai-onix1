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
            var name = $"OpenEphys.Onix1.Resources.{defaultProbeInterfaceFileName}";
            using var stream = Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing embedded resource: {name}");
            using var reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd())
                ?? throw new InvalidOperationException($"Failed to deserialise {name}.");
        }
    }
}
