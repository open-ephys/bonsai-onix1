using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class DigitalInputDataFrame
    {
        public unsafe DigitalInputDataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (DigitalInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            Port = payload->PortInputs;
            Links = payload->Links;
            Buttons = payload->Buttons;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public PortState Port { get; }

        public LinkState Links { get; }

        public ButtonState Buttons { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DigitalInputPayload
    {
        public ulong HubClock;
        public byte Reserved0;
        public PortState PortInputs;
        public LinkState Links;
        public ButtonState Buttons;
    }
}
