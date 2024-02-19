using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class HarpSyncInputDataFrame
    {
        public unsafe HarpSyncInputDataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (HarpSyncInputPayload*)frame.Data.ToPointer();
            HubClock = BitHelper.SwapEndian(payload->HubClock);
            HarpTime = ((uint)payload->HarpTimeHigh << 16) | ((uint)payload->HarpTimeLow << 0);
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public uint HarpTime { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HarpSyncInputPayload
    {
        public ulong HubClock;
        public ushort HarpTimeHigh;
        public ushort HarpTimeLow;
    }
}
