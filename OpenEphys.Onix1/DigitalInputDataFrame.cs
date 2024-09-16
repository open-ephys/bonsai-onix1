using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A digital event produced by the ONIX breakout board.
    /// </summary>
    public class DigitalInputDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalInputDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A frame produced by an ONIX breakout board's digital IO device.</param>
        public unsafe DigitalInputDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (DigitalInputPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            DigitalInputs = payload->DigitalInputs;
            Buttons = payload->Buttons;
        }

        /// <summary>
        /// Gets the state of the breakout board's 8-bit digital input port.
        /// </summary>
        public DigitalPortState DigitalInputs { get; }

        /// <summary>
        /// Gets the state of the breakout board's buttons and switches.
        /// </summary>
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
