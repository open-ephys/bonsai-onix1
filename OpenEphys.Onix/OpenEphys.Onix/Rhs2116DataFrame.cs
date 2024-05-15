using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Rhs2116DataFrame
    {
        public Rhs2116DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat dcData)
        {
            Clock = clock;
            HubClock = hubClock;
            AmplifierData = amplifierData;
            DCData = dcData;
        }

        public ulong[] Clock { get; }

        public ulong[] HubClock { get; }

        public Mat AmplifierData { get; }

        public Mat DCData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhs2116Payload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[Rhs2116.AmplifierChannelCount];
        public fixed ushort DCData[Rhs2116.AmplifierChannelCount];
    }
}
