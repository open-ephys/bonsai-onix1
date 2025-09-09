using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    internal static class ProbeGroupHelper
    {
        public const string ProbeGroupFileStringPrefix = "pi_oni";
        public const string ProbeGroupExtension = ".json";
        public const string ProbeGroupFileNameFilter = "Probe Group Files|*.json|All Files|*.*";

        public static T DeserializeString<T>(string jsonString) where T : class
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
                    return null;
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

        public static void SerializeObject(object obj, string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return;

            var serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var stringJson = JsonConvert.SerializeObject(obj, Formatting.Indented, serializerSettings);

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

        public static T LoadExternalProbeGroupFile<T>(string probeConfigurationFileName) where T : ProbeGroup
        {
            if (string.IsNullOrEmpty(probeConfigurationFileName))
            {
                throw new ArgumentNullException(nameof(probeConfigurationFileName), "Probe configuration file path cannot be null or empty.");
            }

            if (!File.Exists(probeConfigurationFileName))
            {
                throw new FileNotFoundException($"The Probe Group file '{probeConfigurationFileName}' does not exist.");
            }

            try
            {
                string jsonContent = File.ReadAllText(probeConfigurationFileName);
                var result = DeserializeString<T>(jsonContent) ?? throw new InvalidDataException($"Failed to parse Probe Group file: {probeConfigurationFileName}");
                return result;
            }
            catch (UnauthorizedAccessException e)
            {
                throw new IOException($"Access denied reading '{probeConfigurationFileName}'. Check file permissions.", e);
            }
            catch (PathTooLongException e)
            {
                throw new IOException($"File path '{probeConfigurationFileName}' exceeds system maximum length.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"Unable to read '{probeConfigurationFileName}'. The file may be in use.", e);
            }
            catch (Exception e)
            {
                throw new IOException($"Unexpected error reading '{probeConfigurationFileName}'.", e);
            }
        }

        public static void SaveExternalProbeGroupFile(ProbeGroup probeGroup, string probeConfigurationFile)
        {
            SerializeObject(probeGroup, probeConfigurationFile);
        }

        public static string GenerateProbeGroupFileName(uint deviceAddress, string deviceName)
        {
            return ProbeGroupFileStringPrefix + "-" + deviceAddress + "_" + deviceName + ProbeGroupExtension;
        }

        internal class ProbeGroupFileNameConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value?.ToString() ?? string.Empty;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    string strValue = value as string;

                    if (string.IsNullOrEmpty(strValue) && context?.Instance is SingleDeviceFactory device)
                    {
                        return "[default]";
                    }

                    return strValue ?? string.Empty;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
