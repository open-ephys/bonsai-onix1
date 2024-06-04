using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class AnalogInputDataFrame
    {
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubSyncCounter, Mat analogData)
        {
            Clock = clock;
            HubSyncCounter = hubSyncCounter;
            AnalogData = analogData;
        }

        public ulong[] Clock { get; }

        public ulong[] HubSyncCounter { get; }

        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AnalogInputPayload
    {
        public ulong HubSyncCounter;
        public fixed short AnalogData[AnalogIO.ChannelCount];
    }
}
