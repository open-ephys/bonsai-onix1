namespace OpenEphys.Onix1
{
    static class OniHelper
    {
        public static uint GetHubAddressFromDeviceAddress(uint deviceAddress)
        {
            return deviceAddress & 0xFF00u;
        }
    }
}
