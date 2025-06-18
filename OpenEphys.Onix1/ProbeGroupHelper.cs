using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    internal static class ProbeGroupHelper
    {
        public const string ProbeInterfaceFileString = "probe_interface";

        /// <summary>
        /// Given a string with a valid JSON structure, deserialize the string to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
#nullable enable
        public static T? DeserializeString<T>(string jsonString)
        {
            var errors = new List<string>();

            var serializerSettings = new JsonSerializerSettings()
            {
                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var obj = JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);

            if (errors.Count > 0)
            {
                Console.WriteLine($"There were errors encountered while parsing a JSON string.\n\n");

                foreach (var e in errors)
                {
                    Console.Error.WriteLine(e);
                }

                return default;
            }

            return obj;
        }
#nullable disable

        public static void SerializeObject(object _object, string filepath)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var stringJson = JsonConvert.SerializeObject(_object, Formatting.Indented, serializerSettings);

            File.WriteAllText(filepath, stringJson);
        }

        public static T LoadExternalProbeConfigurationFile<T>(string probeConfigurationFile) where T : ProbeGroup
        {
            if (File.Exists(probeConfigurationFile))
            {
                return DeserializeString<T>(File.ReadAllText(probeConfigurationFile));
            }
            else
            {
                throw new NullReferenceException($"The Probe Interface file given for {typeof(T).Name} does not exist. Double check that the file exists and can be read. " +
                    $"\n\nFilepath: '{probeConfigurationFile}'");
            }
        }

        public static void SaveExternalProbeConfigurationFile(ProbeGroup probeGroup, string probeConfigurationFile)
        {
            SerializeObject(probeGroup, probeConfigurationFile);
        }

        public static string GenerateProbeConfigurationFilename()
        {
            const string extension = ".json";

            string basename = Path.Combine(Directory.GetCurrentDirectory(), ProbeInterfaceFileString);
            string filename = basename + extension;

            int counter = 0;

            while (File.Exists(filename))
            {
                filename = basename + $"_{++counter}" + extension;
            }

            return filename;
        }

        /// <summary>
        /// Compares two file paths to determine if they are equal.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>
        /// Returns true if the two paths are equal. Returns false if either string is null or empty, or if the
        /// two paths are different in any way.</returns>
        public static bool CompareFilePaths(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) return false;

            return Path.GetFullPath(path1) == Path.GetFullPath(path2);
        }

        #region Neuropixels 1.0

        /// <summary>
        /// Convert a <see cref="NeuropixelsV1eProbeGroup"/> object to a list of electrodes, which only includes currently enabled electrodes
        /// </summary>
        /// <param name="probeGroup">A <see cref="NeuropixelsV1eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/>'s that are enabled</returns>
        public static NeuropixelsV1Electrode[] ToChannelMap(this NeuropixelsV1eProbeGroup probeGroup)
        {
            var enabledContacts = probeGroup.GetContacts().Where(c => c.DeviceId != -1);

            if (enabledContacts.Count() != NeuropixelsV1.ChannelCount)
            {
                throw new InvalidOperationException($"Channel configuration must have {NeuropixelsV1.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            return enabledContacts.Select(c => new NeuropixelsV1Electrode(c.Index))
                                  .OrderBy(e => e.Channel)
                                  .ToArray();
        }

        public static void SelectElectrodes(this NeuropixelsV1eProbeGroup probeGroup, NeuropixelsV1Electrode[] electrodes)
        {
            var channelMap = probeGroup.ToChannelMap();

            foreach (var e in electrodes)
            {
                try
                {
                    channelMap[e.Channel] = e;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {channelMap.Length - 1} are supported.", ex);
                }
            }

            probeGroup.UpdateDeviceChannelIndices(channelMap);
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes.
        /// </summary>
        /// <param name="probeGroup">A <see cref="NeuropixelsV1eProbeGroup"/> object.</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/> electrodes.</returns>
        public static List<NeuropixelsV1Electrode> ToElectrodes(this NeuropixelsV1eProbeGroup probeGroup)
        {
            List<NeuropixelsV1Electrode> electrodes = new();

            foreach (var c in probeGroup.GetContacts())
            {
                electrodes.Add(new NeuropixelsV1Electrode(c.Index));
            }

            return electrodes;
        }

        #endregion
    }
}
