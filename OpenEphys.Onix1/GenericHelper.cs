namespace OpenEphys.Onix1
{
    static class GenericHelper
    {
        public static uint GetHubAddressFromDeviceAddress(uint deviceAddress)
        {
            return (deviceAddress & 0xFF00u);
        }

        /// <summary>
        /// Gets the version components from a 16-bit version register value
        /// </summary>
        /// <param name="rawVersion">Raw 16-bit version</param>
        /// <param name="major">Contains the major component extracted from <paramref name="rawVersion"/></param>
        /// <param name="minor">Contains the minor component extracted from <paramref name="rawVersion"/></param>
        public static void GetVersionComponents(uint rawVersion, out uint major, out uint minor)
        {
            major = (rawVersion >> 8) & 0xFF;
            minor = rawVersion & 0xFF;
        }
    }
}
