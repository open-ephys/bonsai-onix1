using System;
using System.Threading;

namespace OpenEphys.Onix1
{
    class ConfigureUclaMiniscopeV4PortController : ConfigurePortController
    {
        const double MaxVoltage = 6.0;

        public ConfigureUclaMiniscopeV4PortController()
            : base(typeof(PortController))
        {
            PortVoltage.Requested = 5.5;
        }

        void SetPortVoltage(DeviceContext device, double voltage)
        {
            if (voltage > MaxVoltage)
            {
                throw new ArgumentException($"The port voltage must be set to a value less than {MaxVoltage} " +
                    $"volts to prevent damage to the miniscope.");
            }

            const int WaitUntilVoltageSettles = 400;
            device.WriteRegister(PortController.PORTVOLTAGE, 0);
            Thread.Sleep(WaitUntilVoltageSettles);
            device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
            Thread.Sleep(WaitUntilVoltageSettles);
        }

        override protected bool ConfigurePortVoltageOverride(DeviceContext device, double voltage)
        {
            SetPortVoltage(device, voltage);
            return true; // NB: camera needs to be configured in order get lock on serdes
        }
    }

}
