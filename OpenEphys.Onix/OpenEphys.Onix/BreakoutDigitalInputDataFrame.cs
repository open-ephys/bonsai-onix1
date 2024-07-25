using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class BreakoutDigitalInputDataFrame : DataFrame
    {
        public unsafe BreakoutDigitalInputDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (BreakoutDigitalInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            DigitalInputs = payload->DigitalInputs;
            Buttons = payload->Buttons;
        }

        public BreakoutDigitalPortState DigitalInputs { get; }

        public BreakoutButtonState Buttons { get; }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BreakoutDigitalInputPayload
    {
        public ulong HubClock;
        public BreakoutDigitalPortState DigitalInputs;
        public BreakoutButtonState Buttons;
    }
}
