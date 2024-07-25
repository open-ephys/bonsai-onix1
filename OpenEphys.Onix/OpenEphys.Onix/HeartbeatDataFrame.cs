using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class HeartbeatDataFrame : DataFrame
    {
        public unsafe HeartbeatDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (HeartbeatPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HeartbeatPayload
    {
        public ulong HubClock;
    }
}
