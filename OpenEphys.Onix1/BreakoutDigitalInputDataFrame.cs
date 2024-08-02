using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that contains information about a digital event on the ONIX breakout board.
    /// </summary>
    public class BreakoutDigitalInputDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BreakoutDigitalInputDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A frame produced by an ONIX breakout board's digital IO device.</param>
        public unsafe BreakoutDigitalInputDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (BreakoutDigitalInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            DigitalInputs = payload->DigitalInputs;
            Buttons = payload->Buttons;
        }

        /// <summary>
        /// Gets the state of the breakout board's 8-bit digital input port.
        /// </summary>
        public BreakoutDigitalPortState DigitalInputs { get; }

        /// <summary>
        /// Gets the state of the breakout board's buttons and switches.
        /// </summary>
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
