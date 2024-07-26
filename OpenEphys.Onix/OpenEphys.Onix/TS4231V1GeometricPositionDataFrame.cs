using System.Numerics;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that contains the 3D position of a photodiode in a TS4231 sensor array relative
    /// to a given SteamVR V1 base station origin.
    /// </summary>
    /// <remarks>
    /// A sequence of 12 <see cref="oni.Frame"/> objects produced by a single TS4231 sensor are required to
    /// geometrically calculate the position of the sensor's photodiode in 3D space.
    /// </remarks>
    public class TS4231V1GeometricPositionDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TS4231V1GeometricPositionDataFrame"/> class.
        /// </summary>
        /// <param name="clock">The median <see cref="DataFrame.Clock"/> value of the 12 frames required to construct a single position.</param>
        /// <param name="hubClock">The median <see cref="DataFrame.HubClock"/> value of the 12 frames required to construct a single position.</param>
        /// <param name="sensorIndex">The index of the TS4231 sensor that the 3D position corresponds to.</param>
        /// <param name="position">The 3 dimensional position of the photodiode connected to the TS4231 sensor with units determined by
        /// <see cref="TS4231V1GeometricPositionData.P"/> and <see cref="TS4231V1GeometricPositionData.Q"/>.</param>
        public TS4231V1GeometricPositionDataFrame(ulong clock, ulong hubClock, int sensorIndex, Vector3 position)
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
        /// Gets rhe 3D position of the photodiode connected to the TS4231[<see cref="SensorIndex"/>] sensor with units determined by
        /// <see cref="TS4231V1GeometricPositionData.P"/> and <see cref="TS4231V1GeometricPositionData.Q"/>.
        /// </summary>
        public Vector3 Position { get; }
    }
}
