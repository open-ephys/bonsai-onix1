using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <inheritdoc/>
    public class OnixContextTask : ContextTask
    {
        /// <inheritdoc/>
        internal OnixContextTask(string driver, int index) : base(driver, index) { }

        /// <inheritdoc/>
        protected override void ContextCreationChecks()
        {
            var (major, _) = GenericHelper.GetFirmwareVersionComponents(GetHub(0).FirmwareVersion);
            if (major != 2)
            {
                throw new NotSupportedException("This library requires version 2.x of the ONIX firmware. "
                    + "Please perform a firmware update to use this library. Instructions can be found at "
                    + "https://open-ephys.github.io/onix-docs/Hardware%20Guide/PCIe%20Controller/updating-gateware.html");
            }
        }

        /// <inheritdoc/>
        protected override bool IndependentFrameClockReset()
        {
            int address = ctx.HardwareAddress;
            int mode = (address & 0x00FF0000) >> 16;
            return mode != 0; // synchronized mode, reset counter independently
        }

        #region oni.Context Properties
        // Port A and Port B each have a bit in PORTFUNC
        internal PassthroughState HubState
        {
            get => (PassthroughState)ctx.GetCustomOption((int)oni.ONIXOption.PORTFUNC);
            set => ctx.SetCustomOption((int)oni.ONIXOption.PORTFUNC, (int)value);
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

        #endregion


    }
}
