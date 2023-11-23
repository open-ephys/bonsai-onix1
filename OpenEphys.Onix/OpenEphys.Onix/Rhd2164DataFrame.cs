using System.Net;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Rhd2164DataFrame
    {
        public Rhd2164DataFrame(ulong clock, long hubClock, Mat amplifierData, Mat auxData)
        {
            Clock = clock;
            HubClock = unchecked((ulong)IPAddress.NetworkToHostOrder(hubClock));
            AmplifierData = amplifierData;
            AuxData = auxData;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public Mat AmplifierData { get; }

        public Mat AuxData { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct Rhd2164Payload
    {
        public const int AmplifierChannelCount = 64;
        public const int AuxChannelCount = 3;

        public long HubClock;
        public fixed ushort AmplifierData[AmplifierChannelCount];
        public fixed ushort AuxData[AuxChannelCount];
    }
}
