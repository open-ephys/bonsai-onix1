using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;
using System.IO;

namespace OpenEphys.Onix1
{
    internal static class ProbeGroupHelper
    {
        public const string ProbeInterfaceFileString = "pi";
        public const string ProbeInterfaceExtension = ".json";

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

            try
            {
                var obj = JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);

                if (errors.Count > 0)
                {
                    Console.WriteLine("There were errors encountered while parsing a JSON string.\n");
                    foreach (var e in errors)
                    {
                        Console.Error.WriteLine(e);
                    }
                    return default;
                }

                return obj;
            }
            catch (JsonReaderException e)
            {
                throw new InvalidDataException("Invalid JSON format", e);
            }
            catch (JsonSerializationException e)
            {
                throw new InvalidDataException("Failed to deserialize JSON", e);
            }
        }
#nullable disable

        public static void SerializeObject(object _object, string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return;

            var serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var stringJson = JsonConvert.SerializeObject(_object, Formatting.Indented, serializerSettings);

            try
            {
                File.WriteAllText(filepath, stringJson);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new IOException($"Access denied writing to '{filepath}'. Check file permissions.", e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new IOException($"Directory not found for '{filepath}'. Ensure the directory exists.", e);
            }
            catch (PathTooLongException e)
            {
                throw new IOException($"File path '{filepath}' exceeds system maximum length.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"Unable to write to '{filepath}'. The file may be in use.", e);
            }
            catch (Exception e)
            {
                throw new IOException($"Unexpected error writing to '{filepath}'.", e);
            }
        }

        public static T LoadExternalProbeInterfaceFile<T>(string probeConfigurationFile) where T : ProbeGroup
        {
            if (string.IsNullOrEmpty(probeConfigurationFile))
            {
                throw new ArgumentNullException(nameof(probeConfigurationFile), "Probe configuration file path cannot be null or empty.");
            }

            if (!File.Exists(probeConfigurationFile))
            {
                throw new FileNotFoundException($"The Probe Interface file '{probeConfigurationFile}' does not exist.");
            }

            try
            {
                string jsonContent = File.ReadAllText(probeConfigurationFile);
                var result = DeserializeString<T>(jsonContent) ?? throw new InvalidDataException($"Failed to parse probe interface file: {probeConfigurationFile}");
                return result;
            }
            catch (UnauthorizedAccessException e)
            {
                throw new IOException($"Access denied reading '{probeConfigurationFile}'. Check file permissions.", e);
            }
            catch (PathTooLongException e)
            {
                throw new IOException($"File path '{probeConfigurationFile}' exceeds system maximum length.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"Unable to read '{probeConfigurationFile}'. The file may be in use.", e);
            }
            catch (Exception e)
            {
                throw new IOException($"Unexpected error reading '{probeConfigurationFile}'.", e);
            }
        }

        public static void SaveExternalProbeInterfaceFile(ProbeGroup probeGroup, string probeConfigurationFile)
        {
            SerializeObject(probeGroup, probeConfigurationFile);
        }

        /// <summary>
        /// Creates a probe configuration filename located in the current working directory, using the given
        /// device address and device name.
        /// </summary>
        /// <param name="address">Unsigned integer defining the address of the device</param>
        /// <param name="name">String defining the name of the device, including the headstage name and any other relevant information, if needed.</param>
        /// <returns>Filename in the format {<see cref="ProbeInterfaceFileString"/>_{address}_{name}.json}</returns>
        public static string GenerateProbeInterfaceFilename(uint address, string name)
        {
            if (!string.IsNullOrEmpty(name))
                name = name.Replace("/", "_");

            string filename = ProbeInterfaceFileString + "_" + address + "_" + name + ProbeInterfaceExtension;

            return filename;
        }
    }
}
