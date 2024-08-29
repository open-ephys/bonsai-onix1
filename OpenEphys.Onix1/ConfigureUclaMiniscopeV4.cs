using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    public class ConfigureUclaMiniscopeV4 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureUclaMiniscopeV4PortController PortControl = new();

        public ConfigureUclaMiniscopeV4()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureUclaMiniscopeV4Camera Camera { get; set; } = new();

        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureUclaMiniscopeV4Bno055 Bno055 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                PortControl.DeviceAddress = (uint)port;
                Camera.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 5.0V to 6.0V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        public double? PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Camera;
            yield return Bno055;
        }
        class ConfigureUclaMiniscopeV4PortController : ConfigurePortController
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
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(WaitUntilVoltageSettles);
                device.WriteRegister(PortController.PORTVOLTAGE, voltage);
                Thread.Sleep(WaitUntilVoltageSettles);
            }

            override protected bool CheckLinkState(DeviceContext device)
            {
                try
                {
                    var ds90ub9x = device.Context.GetPassthroughDeviceContext(DeviceAddress << 8, typeof(DS90UB9x));
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

                var linkState = device.ReadRegister(PortController.LINKSTATE);
                return (linkState & PortController.LINKSTATE_SL) != 0;
            }
        }
    }
}
