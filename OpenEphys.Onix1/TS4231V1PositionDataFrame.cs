using System.Numerics;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// 3D position of a single photodiode within a TS4231 sensor array.
    /// </summary>
    /// <remarks>
    /// A sequence of 12 <see cref="oni.Frame"/> objects produced by a single TS4231 sensor are required to
    /// geometrically calculate the position of the sensor's photodiode in 3D space.
    /// </remarks>
    public class TS4231V1PositionDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TS4231V1PositionDataFrame"/> class.
        /// </summary>
        /// <param name="clock">The median <see cref="DataFrame.Clock"/> value of the 12 frames required to construct a single position.</param>
        /// <param name="hubClock">The median <see cref="DataFrame.HubClock"/> value of the 12 frames required to construct a single position.</param>
        /// <param name="sensorIndex">The index of the TS4231 sensor that the 3D position corresponds to.</param>
        /// <param name="position">The 3 dimensional position of the photodiode connected to the TS4231 sensor with units determined by
        /// <see cref="TS4231V1PositionData.P"/> and <see cref="TS4231V1PositionData.Q"/>.</param>
        public TS4231V1PositionDataFrame(ulong clock, ulong hubClock, int sensorIndex, Vector3 position)
        {
            Clock = clock;
            HubClock = hubClock;
            SensorIndex = sensorIndex;
            Position = position;
        }

        /// <inheritdoc cref = "DataFrame.Clock"/>
        public ulong Clock { get; }

        /// <inheritdoc cref = "DataFrame.Clock"/>
        public ulong HubClock { get; }

        /// <summary>
        /// Gets the index of the TS4231 sensor that produced this data.
        /// </summary>
        public int SensorIndex { get; }

        /// <summary>
        /// Gets the 3D position of the photodiode connected to the TS4231 receiver with index <see cref="SensorIndex"/>  
        //  in units determined by <see cref="TS4231V1PositionData.P"/> and <see cref="TS4231V1PositionData.Q"/>.
        /// </summary>
        public Vector3 Position { get; }
    }
}
