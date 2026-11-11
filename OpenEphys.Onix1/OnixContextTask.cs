using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Encapsulates a single ONI context and orchestrates interaction with an ONIX system.
    /// </summary>
    /// <remarks>
    /// <see cref="OnixContextTask"/> is the <see cref="ContextTask"/> for ONIX hardware and is created by
    /// the <see cref="CreateContext"/> operator. Configuration operators that require ONIX-specific
    /// functionality, such as headstages, only accept an <see cref="OnixContextTask"/>.
    /// </remarks>
    public sealed class OnixContextTask : ContextTask
    {
        /// <inheritdoc/>
        internal OnixContextTask(string driver, int index, ValidationLevel validationLevel)
            : base(driver, index, validationLevel)
        {
            try
            {
                var (major, _) = GenericHelper.GetFirmwareVersionComponents(GetHub(0).FirmwareVersion);
                if (major != 2)
                {
                    throw new NotSupportedException("This library requires version 2.x of the ONIX firmware. "
                        + "Please perform a firmware update to use this library. Instructions can be found at "
                        + "https://open-ephys.github.io/onix-docs/Hardware%20Guide/PCIe%20Controller/updating-gateware.html");
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal const string DefaultDriver = "riffa";
        internal const int DefaultIndex = 0;

        // In ONIX, HardwareAddress is a 32-bit integer where bits 16-17 represent
        // the synchronization mode of the controller:
        // If all zero the controller is in standalone mode.

        // TODO : This is actually a naive approach, kept for keeping the original code behavior.
        // However, nothing guarantees that all controllers are synchronized before acquisition starts 
        // in all of them.
        // the 0x00FF0000 is kept to maintain the original code, but it should be changed to 0x00030000

        /// <summary>
        /// Gets a value indicating whether the frame clock is reset when acquisition starts.
        /// </summary>
        /// <remarks>The result depends on the synchronization configuration of the ONIX controller.</remarks>
        /// <returns><see langword="true"/> if the frame clock is reset on acquisition start; otherwise, <see langword="false"/>.</returns>
        protected override bool GetFrameClockResetOnStart()
        {
            int address = HardwareAddress;
            int mode = (address & 0x00FF0000) >> 16;
            return mode == 0;
        }

        /// <summary>
        /// Performs synchronization between multiple ONI controllers, if applicable and configured in the hardware.
        /// This method is called after the context is configured and before acquisition starts.
        /// </summary>
        /// <remarks>Resets the frame clock if required by the synchronization configuration of the ONIX controller.</remarks>
        protected override void PerformControllerSynchronization()
        {
            int address = HardwareAddress;
            int mode = (address & 0x00FF0000) >> 16;
            if (mode != 0)
            {
                ResetFrameClock();
            }
        }

        // Port A and Port B each have a bit in PORTFUNC
        internal PassthroughState HubState
        {
            get => (PassthroughState)GetCustomOption((int)oni.ONIXOption.PORTFUNC);
            set => SetCustomOption((int)oni.ONIXOption.PORTFUNC, (int)value);
        }

        internal uint GetPassthroughDeviceAddress(uint deviceAddress)
        {
            var hubAddress = (deviceAddress & 0xFF00u) >> 8;
            if (hubAddress == 0)
            {
                throw new ArgumentException(
                    "Device addresses on hub zero cannot be used to create passthrough devices.",
                    nameof(deviceAddress));
            }

            return hubAddress + 7;
        }


    }
}
