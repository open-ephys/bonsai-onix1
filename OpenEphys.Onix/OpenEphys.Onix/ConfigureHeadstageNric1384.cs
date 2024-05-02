using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix
{
    public class ConfigureHeadstageNric1384 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageNric1384LinkController LinkController = new();

        public ConfigureHeadstageNric1384()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNric1384 Nric1384 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Nric1384.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.5V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }


        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Nric1384;
            yield return Bno055;
        }

        class ConfigureHeadstageNric1384LinkController : ConfigureFmcLinkController
        {

            public double? PortVoltage { get; set; } = null;

            // TODO: Needs more testing
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {

                if (PortVoltage == null)
                {
                    const double MinVoltage = 3.3;
                    const double MaxVoltage = 5.5;
                    const double VoltageOffset = 0.3;
                    const double VoltageIncrement = 0.2;

                    for (var voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                    {

                        SetVoltage(device, voltage);

                        if (CheckLinkState(device))
                        {
                            SetVoltage(device, voltage + VoltageOffset);
                            break;
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
                //device.WriteRegister(FmcLinkController.PORTVOLTAGE, 33);
                //device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                //Thread.Sleep(500);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(500);
            }
        }
    }
}
