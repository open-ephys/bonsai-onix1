using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Port status information.
    /// </summary>
    public class PortStatusFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortStatusFrame"/> class.
        /// </summary>
        /// <param name="frame">A data frame produced by a port controller device.</param>
        public unsafe PortStatusFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (PortStatusPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            StatusCode = payload->Code;
        }

        /// <summary>
        /// Gets the port status code.
        /// </summary>
        public PortStatusCode StatusCode { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PortStatusPayload
    {
        public ulong HubClock;
        public PortStatusCode Code;
    }
}
