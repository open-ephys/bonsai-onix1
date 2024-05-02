using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Nric1384DataFrame
    {
        public Nric1384DataFrame(ulong[] clock, ulong[] hubClock, uint[] frameCounter, Mat spikeData, Mat lfpData)
        {
            Clock = clock;
            HubClock = hubClock;
            FrameCounter = frameCounter;
            SpikeData = spikeData;
            LfpData = lfpData;
        }

        public ulong[] Clock { get; }

        public ulong[] HubClock { get; }

        public uint[] FrameCounter { get; }

        public Mat SpikeData { get; }

        public Mat LfpData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Nric1384Payload
    {
        public ulong HubClock;
        public fixed ushort LfpData[Nric1384.AdcCount];
        public fixed ushort ApData[Nric1384.ChannelCount];
        public uint FrameCount;
    }
}
