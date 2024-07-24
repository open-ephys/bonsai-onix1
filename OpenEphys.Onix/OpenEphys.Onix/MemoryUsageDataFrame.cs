using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class MemoryUsageDataFrame 
    {
        public unsafe MemoryUsageDataFrame(oni.Frame frame, uint totalMemory) 
        {
            var payload = (MemoryUsagePayload*)frame.Data.ToPointer();

            Clock = frame.Clock;
            DeviceAddress = frame.DeviceAddress;
            HubClock = payload->HubClock;
            PercentUsed = 100.0 * payload->Usage / totalMemory;
            BytesUsed = payload->Usage * 4;
        }

        public ulong Clock { get; private set; }

        public uint DeviceAddress { get; private set; }

        public ulong HubClock { get; }

        public double PercentUsed { get; }

        public uint BytesUsed { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MemoryUsagePayload
    {
        public ulong HubClock;
        public uint Usage;
    }
}
