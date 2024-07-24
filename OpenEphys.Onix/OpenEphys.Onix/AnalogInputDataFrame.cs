using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class AnalogInputDataFrame : BufferedDataFrame
    {
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubClock, Mat analogData)
            : base(clock, hubClock)
        {
            AnalogData = analogData;
        }

        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AnalogInputPayload
    {
        public ulong HubClock;
        public fixed short AnalogData[AnalogIO.ChannelCount];
    }
}
