using System.Threading;

namespace OpenEphys.Onix
{
    class ConfigureUclaMiniscopeV4LinkController : ConfigureFmcLinkController
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

        override protected bool CheckLinkState(DeviceContext device)
        {
            try
            {
                var ds90ub9x = device.Context.GetPassthroughDeviceContext(DeviceAddress << 8, DS90UB9x.ID);
                ConfigureUclaMiniscopeV4Camera.ConfigureMiniscope(ds90ub9x);
            }
            catch (oni.ONIException ex)
            {
                // this can occur if power is too low, so we need to be able to try again
                const int FailureToWriteRegister = -6;
                if (ex.Number != FailureToWriteRegister)
                {
                    throw;
                }
            }

            Thread.Sleep(200);

            var linkState = device.ReadRegister(FmcLinkController.LINKSTATE);
            return (linkState & FmcLinkController.LINKSTATE_SL) != 0;
        }
    }
}
