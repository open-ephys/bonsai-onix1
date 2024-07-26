using System.Numerics;
using System.Runtime.InteropServices;
using oni;

namespace OpenEphys.Onix
{
    public class TS4231V1DataFrame : DataFrame
    {
        public unsafe TS4231V1DataFrame(oni.Frame frame, double hubClockPeriod)
            : base(frame.Clock)
        {
            var payload = (TS4231Payload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            SensorIndex = payload->SensorIndex;
            EnvelopeWidth = hubClockPeriod * payload->EnvelopeWidth;
            EnvelopeType = payload->EnvelopeType;
        }

        public int SensorIndex { get; }

        public double EnvelopeWidth { get; }

        public TS4231V1Envelope EnvelopeType { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TS4231Payload
    {
        public ulong HubClock;
        public ushort SensorIndex;
        public uint EnvelopeWidth;
        public TS4231V1Envelope EnvelopeType;
    }

    public enum TS4231V1Envelope : short
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
