using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class BreakoutAnalogInputDataFrame : BufferedDataFrame
    {
        public BreakoutAnalogInputDataFrame(ulong[] clock, ulong[] hubClock, Mat analogData)
            : base(clock, hubClock)
        {
            AnalogData = analogData;
        }

        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct BreakoutAnalogInputPayload
    {
        public ulong HubClock;
        public fixed short AnalogData[BreakoutAnalogIO.ChannelCount];
    }
}
