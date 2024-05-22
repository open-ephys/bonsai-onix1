using System.IO;
using Newtonsoft.Json;

namespace OpenEphys.Onix.Design
{
    internal class DesignHelper
    {
        public static T DeserializeString<T>(string channelLayout)
        {
            return JsonConvert.DeserializeObject<T>(channelLayout);
        }

        public static void SerializeObject(object _object, string filepath)
        {
            var stringJson = JsonConvert.SerializeObject(_object);

            File.WriteAllText(filepath, stringJson);
        }
    }
}
