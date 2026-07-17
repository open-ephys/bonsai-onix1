using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Nric1384Payload
    {
        public ulong HubClock;
        public fixed ushort LfpData[NeuropixelsV1.AdcCount];
        public fixed ushort ApData[NeuropixelsV1.ChannelCount];
        public int FrameCount;
    }
}
