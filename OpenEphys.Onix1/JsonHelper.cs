using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    internal static class JsonHelper
    {
        public static object DeserializeString(string jsonString, Type type)
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
                var obj = JsonConvert.DeserializeObject(jsonString, type, serializerSettings);

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
    }
}
