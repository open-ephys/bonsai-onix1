using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class AnalogInputDataFrame
    {
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubClock, Mat analogData)
        {
            Clock = clock;
            HubClock = hubClock;
            AnalogData = analogData;
        }

        public ulong[] Clock { get; }

        public ulong[] HubClock { get; }

        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AnalogInputPayload
    {
        public ulong HubClock;
        public fixed short AnalogData[AnalogIO.ChannelCount];
    }
}
