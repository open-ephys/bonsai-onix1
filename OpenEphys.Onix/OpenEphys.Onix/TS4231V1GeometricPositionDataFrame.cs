using System.Numerics;

namespace OpenEphys.Onix
{
    public class TS4231V1GeometricPositionDataFrame
    {
        public TS4231V1GeometricPositionDataFrame(ulong clock, ulong hubClock, int sensorIndex, Vector3 position)
        {
            Clock = clock;
            HubClock = hubClock;
            SensorIndex = sensorIndex;
            Position = position;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public int SensorIndex { get; }

        public Vector3 Position { get; }

    }
}
