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
            var statusCodeValid = (payload->SerdesStatus & 0x0004) == 4;
            StatusCode = statusCodeValid ? payload->Code : PortStatusCode.Invalid;
            SerdesLocked = (payload->SerdesStatus & 0x0001) == 1;
            SerdesPass = (payload->SerdesStatus & 0x0002) == 2;
        }

        /// <summary>
        /// Gets the port status code.
        /// </summary>
        public PortStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the SERDES forward channel lock status.
        /// </summary>
        public bool SerdesLocked { get; }

        /// <summary>
        /// Gets the SERDES on-chip parity check status.
        /// </summary>
        public bool SerdesPass { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PortStatusPayload
    {
        public ulong HubClock;
        public PortStatusCode Code;
        public byte SerdesStatus;
    }
}
