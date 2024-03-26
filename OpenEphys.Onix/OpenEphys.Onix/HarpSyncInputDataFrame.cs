using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class HarpSyncInputDataFrame
    {
        public unsafe HarpSyncInputDataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (HarpSyncInputPayload*)frame.Data.ToPointer();
            HubSyncCounter = payload->HubSyncCounter;
            HarpTime = payload->HarpTime;
        }

        public ulong Clock { get; }

        public ulong HubSyncCounter { get; }

        public uint HarpTime { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HarpSyncInputPayload
    {
        public ulong HubSyncCounter;
        public uint HarpTime;
    }
}
