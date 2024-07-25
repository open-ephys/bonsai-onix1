using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Test0DataFrame : BufferedDataFrame
    {
        public Test0DataFrame(ulong[] clock, ulong[] hubClock, Mat message, Mat dummy)
            : base(clock, hubClock)
        {
            Message = message;
            Dummy = dummy;
        }

        public Mat Message { get; }

        public Mat Dummy { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Test0PayloadHeader
    {
        public ulong HubClock;
        public short Message;
    }
}
