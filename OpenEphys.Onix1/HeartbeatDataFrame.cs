using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// The time of a single heartbeat.
    /// </summary>
    public class HeartbeatDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A data frame produced by a heartbeat device.</param>
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
