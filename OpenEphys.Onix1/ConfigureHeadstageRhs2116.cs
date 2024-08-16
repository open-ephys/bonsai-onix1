using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    public class ConfigureHeadstageRhs2116 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageRhs2116LinkController LinkController = new();

        public ConfigureHeadstageRhs2116()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureRhs2116Dual Rhs2116Dual { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureRhs2116Trigger StimulusTrigger { get; set; } = new();

        internal override void UpdateDeviceNames()
        {
            LinkController.DeviceName = GetFullDeviceName(nameof(LinkController));
            Rhs2116Dual.DeviceName = GetFullDeviceName(nameof(Rhs2116Dual));
            StimulusTrigger.DeviceName = GetFullDeviceName(nameof(StimulusTrigger));
        }

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhs2116Dual.DeviceAddress = offset + 0;
                StimulusTrigger.DeviceAddress = offset + 2;
            }
        }


        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 3.4V to 4.4V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhs2116Dual;
            yield return StimulusTrigger;
        }

        class ConfigureHeadstageRhs2116LinkController : ConfigureFmcLinkController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const double MinVoltage = 3.3;
                const double MaxVoltage = 4.4;
                const double VoltageOffset = 2.0;
                const double VoltageIncrement = 0.2;

                for (var voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (base.CheckLinkState(device))
                    {
                        SetPortVoltage(device, voltage + VoltageOffset);
                        return CheckLinkState(device);
                    }
                }

                return false;
            }

            private void SetPortVoltage(DeviceContext device, double voltage)
            {
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                Thread.Sleep(500);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(500);
            }

            protected override bool CheckLinkState(DeviceContext device)
            {
                // NB: The RHS2116 headstage needs an additional reset after power on to provide its device table.
                device.Context.Reset();
                var linkState = device.ReadRegister(FmcLinkController.LINKSTATE);
                return (linkState & FmcLinkController.LINKSTATE_SL) != 0;
            }
        }
    }

}
