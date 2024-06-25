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
            DigitalInputs = payload->DigitalInputs;
            Buttons = payload->Buttons;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public DigitalPortState DigitalInputs { get; }

        public BreakoutButtonState Buttons { get; }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DigitalInputPayload
    {
        public ulong HubClock;
        public DigitalPortState DigitalInputs;
        public BreakoutButtonState Buttons;
    }
}
