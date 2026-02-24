using System;
using System.IO;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    internal static class ProbeInterfaceHelper
    {
        public const string ProbeInterfaceFileExtension = ".json";
        public const string ProbeInterfaceFileNameFilter = "ProbeInterface Files|*" + ProbeInterfaceFileExtension + "|All Files|*.*";

        public static ProbeGroup LoadExternalProbeInterfaceFile(string probeInterfaceFileName, Type type)
        {
            if (string.IsNullOrEmpty(probeInterfaceFileName))
            {
                throw new ArgumentNullException(nameof(probeInterfaceFileName), "ProbeInterface file path cannot be null or empty.");
            }

            if (!typeof(ProbeGroup).IsAssignableFrom(type)) throw new InvalidDataException($"Invalid type given: {type.Name} is not a valid {nameof(ProbeGroup)} type.");

            if (!File.Exists(probeInterfaceFileName))
            {
                throw new FileNotFoundException($"The ProbeInterface file '{probeInterfaceFileName}' does not exist.");
            }

            try
            {
                string jsonContent = File.ReadAllText(probeInterfaceFileName);
                var result = JsonHelper.DeserializeString(jsonContent, type) as ProbeGroup ?? throw new InvalidDataException($"Failed to parse ProbeInterface file: {probeInterfaceFileName}");
                return result;
            }
            catch (UnauthorizedAccessException e)
            {
                throw new IOException($"Access denied reading '{probeInterfaceFileName}'. Check file permissions.", e);
            }
            catch (PathTooLongException e)
            {
                throw new IOException($"File path '{probeInterfaceFileName}' exceeds system maximum length.", e);
            }
            catch (IOException e)
            {
                throw new IOException($"Unable to read '{probeInterfaceFileName}'. The file may be in use.", e);
            }
            catch (Exception e)
            {
                throw new IOException($"Unexpected error reading '{probeInterfaceFileName}'.", e);
            }
        }

        public static void SaveExternalProbeInterfaceFile(ProbeGroup probeGroup, string probeInterfaceFile)
        {
            JsonHelper.SerializeObject(probeGroup, probeInterfaceFile);
        }
    }
}
