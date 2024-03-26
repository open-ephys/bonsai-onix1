using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Test0DataFrame
    {
        public Test0DataFrame(ulong[] clock, ulong[] hubSyncCounter, Mat message, Mat dummy)
        {
            Clock = clock;
            HubSyncCounter = hubSyncCounter;
            Message = message;
            Dummy = dummy;
        }

        public ulong[] Clock { get; }

        public ulong[] HubSyncCounter { get; }

        public Mat Message { get; }

        public Mat Dummy { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Test0PayloadHeader
    {
        public ulong HubSyncCounter;
        public short Message;
    }
}
