using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class MemoryMonitorDataFrame : DataFrame
    {
        public unsafe MemoryMonitorDataFrame(oni.Frame frame, uint totalMemory)
            : base(frame.Clock)
        {
            var payload = (MemoryUsagePayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            PercentUsed = 100.0 * payload->Usage / totalMemory;
            BytesUsed = payload->Usage * 4;
        }

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
