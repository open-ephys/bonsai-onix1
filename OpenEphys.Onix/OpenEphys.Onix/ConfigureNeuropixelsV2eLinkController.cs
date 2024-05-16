using System.Threading;

namespace OpenEphys.Onix
{
    class ConfigureNeuropixelsV2eLinkController : ConfigureFmcLinkController
    {
        public double? PortVoltage { get; set; } = null;

        // TODO: Needs more testing
        protected override bool ConfigurePortVoltage(DeviceContext device)
        {
            if (PortVoltage == null)
            {
                const double MinVoltage = 3.3;
                const double MaxVoltage = 5.5;
                const double VoltageOffset = 1.0;
                const double VoltageIncrement = 0.2;

                for (double voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetVoltage(device, voltage);
                    if (CheckLinkState(device))
                    {
                        SetVoltage(device, voltage + VoltageOffset);
                        return CheckLinkState(device);
                    }
                }
                return false;
            }
            else
            {
                SetVoltage(device, (double)PortVoltage);
            }

            // NB: The headstage needs an additional reset after power on
            // to provide its device table
            device.Context.Reset();
            Thread.Sleep(200);

            if (CheckLinkState(device))
            {
                device.Context.Reset();
                Thread.Sleep(200);
                return true;
            }

            return false;
        }

        void SetVoltage(DeviceContext device, double voltage)
        {
            device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
            Thread.Sleep(300);
            device.WriteRegister(FmcLinkController.PORTVOLTAGE, (uint)(10 * voltage));
            Thread.Sleep(500);
        }

    }
}
