using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class MemoryUsageDataFrame 
    {
        public unsafe MemoryUsageDataFrame(oni.Frame frame, uint totalMemory) 
        {
            var payload = (MemoryUsagePayload*)frame.Data.ToPointer();

            FrameClock = frame.Clock;
            DeviceAddress = frame.DeviceAddress;
            HubSyncCounter = payload->HubSyncCounter;
            PercentUsed = 100.0 * payload->Usage / totalMemory;
            BytesUsed = payload->Usage * 4;

        }

        public ulong FrameClock { get; private set; }

        public uint DeviceAddress { get; private set; }

        public ulong HubSyncCounter { get; }

        public double PercentUsed { get; }

        public uint BytesUsed { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MemoryUsagePayload
    {
        public ulong HubSyncCounter;
        public uint Usage;
    }
}
