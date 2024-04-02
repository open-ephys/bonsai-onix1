using System.Threading;

namespace OpenEphys.Onix
{
    class ConfigureNeuropixelsV2eLinkController : ConfigureFmcLinkController
    {
        protected override bool ConfigurePortVoltage(DeviceContext device)
        {
            const uint MinVoltage = 50;
            const uint MaxVoltage = 70;
            const uint VoltageOffset = 02;
            const uint VoltageIncrement = 02;

            for (uint voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
            {
                SetPortVoltage(device, voltage);
                if (CheckLinkState(device))
                {
                    SetPortVoltage(device, voltage + VoltageOffset);
                    return CheckLinkState(device);
                }
            }

            return false;
        }

        private void SetPortVoltage(DeviceContext device, uint voltage)
        {
            const int WaitUntilVoltageSettles = 200;
            device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
            Thread.Sleep(WaitUntilVoltageSettles);
            device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage);
            Thread.Sleep(WaitUntilVoltageSettles);
        }
    }
}
