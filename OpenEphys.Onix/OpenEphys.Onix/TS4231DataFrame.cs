using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class Ts4231DataFrame
    {
        public unsafe Ts4231DataFrame(ulong clock, ulong hubClock, int sensorIndex, Vector3 position)
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Ts4231Payload
    {
        public ulong HubClock;
        public ushort SensorIndex;
        public uint EnvelopeWidth;
        public Ts4231Envelope EnvelopeType;
    }

    public enum Ts4231Envelope : short
    {
        Bad = -1,
        J0,
        K0,
        J1,
        K1,
        J2,
        K2,
        J3,
        K3,
        Sweep,
    }
}
