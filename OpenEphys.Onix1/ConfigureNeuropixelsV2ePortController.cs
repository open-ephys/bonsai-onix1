using System.Threading;

namespace OpenEphys.Onix1
{
    class ConfigureNeuropixelsV2ePortController : ConfigurePortController
    {
        protected override bool ConfigurePortVoltage(DeviceContext device, out double voltage)
        {
            const double MinVoltage = 3.3;
            const double MaxVoltage = 5.5;
            const double VoltageOffset = 1.0;
            const double VoltageIncrement = 0.2;

            voltage = MinVoltage;
            for (; voltage <= MaxVoltage; voltage += VoltageIncrement)
            {
                SetVoltage(device, voltage);

                if (CheckLinkState(device))
                {
                    voltage += VoltageOffset;
                    SetVoltage(device, voltage);
                    return CheckLinkState(device);
                }
            }

            return false;
        }


        void SetVoltage(DeviceContext device, double voltage)
        {
            device.WriteRegister(PortController.PORTVOLTAGE, 0);
            Thread.Sleep(200);
            device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
            Thread.Sleep(200);
        }
    }
}
