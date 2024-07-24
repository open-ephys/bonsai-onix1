using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class HarpSyncInputDataFrame : DataFrame
    {
        public unsafe HarpSyncInputDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (HarpSyncInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            HarpTime = payload->HarpTime;
        }

        public uint HarpTime { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HarpSyncInputPayload
    {
        public ulong HubClock;
        public uint HarpTime;
    }
}
