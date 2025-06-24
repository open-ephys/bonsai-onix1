namespace OpenEphys.Onix1
{
    static class GenericHelper
    {
        public static uint GetHubAddressFromDeviceAddress(uint deviceAddress)
        {
            return (deviceAddress & 0xFF00u);
        }

        /// <summary>
        /// Gets 8-bit version components from a 16-bit version register value
        /// </summary>
        /// <param name="version">Register value containing a 16-bit firmware version number.</param>
        /// <returns> A tuple containing the 8-bit major and minor version components.</returns>
        internal static (byte major, byte minor) GetFirmwareVersionComponents(uint version)
        {
            return ((byte)((version >> 8) & 0xFF), (byte)(version & 0xFF));
        }
    }
}
