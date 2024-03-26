using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class TS4231DataFrame
    {
        public unsafe TS4231DataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (TS4231Payload*)frame.Data.ToPointer();
            HubSyncCounter = payload->HubSyncCounter;
            SensorIndex = payload->SensorIndex;
            EnvelopeWidth = payload->EnvelopeWidth;
            EnvelopeType = payload->EnvelopeType;
        }

        public ulong Clock { get; }

        public ulong HubSyncCounter { get; }

        public int SensorIndex { get; }

        public uint EnvelopeWidth { get; }

        public TS4231Envelope EnvelopeType { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TS4231Payload
    {
        public ulong HubSyncCounter;
        public ushort SensorIndex;
        public uint EnvelopeWidth;
        public TS4231Envelope EnvelopeType;
    }

    public enum TS4231Envelope : short
    {
        Sweep,
        J0,
        K0,
        J1,
        K1,
        J2,
        K2
    }
}
