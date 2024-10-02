using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A Harp clock synchronization signal.
    /// </summary>
    public class HarpSyncInputDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HarpSyncInputDataFrame"/> class.
        /// </summary>
        /// <param name="frame">
        /// A frame produced by the Harp sync input device of an ONIX breakout board.
        /// </param>
        public unsafe HarpSyncInputDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (HarpSyncInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            HarpTime = payload->HarpTime + 1;
        }

        /// <summary>
        /// Gets the Harp clock time corresponding to the local acquisition ONIX clock count.
        /// </summary>
        public uint HarpTime { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HarpSyncInputPayload
    {
        public ulong HubClock;
        public uint HarpTime;
    }
}
