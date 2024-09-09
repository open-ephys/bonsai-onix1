using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Hardware memory use information.
    /// </summary>
    public class MemoryMonitorDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMonitorDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A data frame produced by a memory monitor device.</param>
        /// <param name="totalMemory">
        /// The total amount of memory, in 32-bit words, on the hardware that is available for data buffering.
        /// </param>
        public unsafe MemoryMonitorDataFrame(oni.Frame frame, uint totalMemory)
            : base(frame.Clock)
        {
            var payload = (MemoryUsagePayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            PercentUsed = 100.0 * payload->Usage / totalMemory;
            BytesUsed = payload->Usage * 4;
        }

        /// <summary>
        /// Gets the percent of available memory that is currently used.
        /// </summary>
        public double PercentUsed { get; }

        /// <summary>
        /// Gets the number of bytes that are currently used.
        /// </summary>
        public uint BytesUsed { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MemoryUsagePayload
    {
        public ulong HubClock;
        public uint Usage;
    }
}
