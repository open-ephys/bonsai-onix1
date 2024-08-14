using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that contains hardware memory use information.
    /// </summary>
    public class PortStatusFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMonitorDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A data frame produced by a memory monitor device.</param>
        /// <param name="totalMemory">
        /// The total amount of memory, in 32-bit words, on the hardware that is available for data buffering.
        /// </param>
        public unsafe PortStatusFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (PortStatusPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            StatusCode = payload->Code;
            StatusCodeValid = (payload->DeserializerStatus & 0x0004) == 4;
            SerdesLocked = (payload->DeserializerStatus & 0x0001) == 1;
            SerdesPass = (payload->DeserializerStatus & 0x0002) == 2;
        }

        /// <summary>
        /// Gets the 
        /// </summary>
        public PortStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the 
        /// </summary>
        public bool StatusCodeValid { get; }

        /// <summary>
        /// Gets the 
        /// </summary>
        public bool SerdesLocked { get; }

        /// <summary>
        /// Gets the 
        /// </summary>
        public bool SerdesPass { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PortStatusPayload
    {
        public ulong HubClock;
        public PortStatusCode Code;
        public byte DeserializerStatus;
    }
}
