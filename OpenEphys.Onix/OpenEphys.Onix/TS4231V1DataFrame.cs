using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that contains information about a single synchronization pulse or light sweep from a SteamVR V1 base station.
    /// </summary>
    public class TS4231V1DataFrame : DataFrame
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TS4231V1DataFrame"/> class.
        /// </summary>
        /// <param name="frame">An <see cref="oni.Frame"/> produced by a TS4231 device</param>
        /// <param name="hubClockPeriod">The period of the TS4231 devices local clock in Hz</param>
        public unsafe TS4231V1DataFrame(oni.Frame frame, double hubClockPeriod)
            : base(frame.Clock)
        {
            var payload = (TS4231V1Payload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            SensorIndex = payload->SensorIndex;
            EnvelopeWidth = 1e6 * hubClockPeriod * payload->EnvelopeWidth;
            EnvelopeType = payload->EnvelopeType;
        }

        /// <summary>
        /// Gets the index of the TS4231 sensor that produced this data.
        /// </summary>
        public int SensorIndex { get; }

        /// <summary>
        /// Gets the width of the envelope of the modulated optical pulse or sweep in microseconds.
        /// </summary>
        public double EnvelopeWidth { get; }

        /// <summary>
        /// Gets the pulse or sweep classification.
        /// </summary>
        public TS4231V1Envelope EnvelopeType { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TS4231V1Payload
    {
        public ulong HubClock;
        public ushort SensorIndex;
        public uint EnvelopeWidth;
        public TS4231V1Envelope EnvelopeType;
    }

    /// <summary>
    /// Specifies the SteamVR V1 base station optical signal classification.
    /// </summary>
    public enum TS4231V1Envelope : short
    {
        /// <summary>
        /// Specifies and invalid optical signal.
        /// </summary>
        Bad = -1,
        /// <summary>
        /// Specifies a synchronization pulse with 50.0 μS &lt; width ≤ 62.5 μS
        /// </summary>
        J0,
        /// <summary>
        /// Specifies a synchronization pulse with 62.5 μS &lt; width ≤ 72.9 μS
        /// </summary>
        K0,
        /// <summary>
        /// Specifies a synchronization pulse with 72.9 μS &lt; width ≤ 83.3 μS
        /// </summary>
        J1,
        /// <summary>
        /// Specifies a synchronization pulse with 83.3 μS &lt; width ≤ 93.8 μS
        /// </summary>
        K1,
        /// <summary>
        /// Specifies a synchronization pulse with 93.8 μS &lt; width ≤ 104 μS
        /// </summary>
        J2,
        /// <summary>
        /// Specifies a synchronization pulse with 104 μS &lt; width ≤ 115 μS
        /// </summary>
        K2,
        /// <summary>
        /// Specifies a synchronization pulse with 115 μS &lt; width ≤ 125 μS
        /// </summary>
        J3,
        /// <summary>
        /// Specifies a synchronization pulse with 125 μS &lt; width ≤ 135 μS
        /// </summary>
        K3,
        /// <summary>
        /// Specifies a light sheet sweep (width ≤ 50  μS)
        /// </summary>
        Sweep,
    }
}
