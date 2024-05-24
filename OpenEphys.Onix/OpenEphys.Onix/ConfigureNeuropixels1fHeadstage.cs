using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixels1fHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixels1fHeadstageLinkController LinkController = new();

        public ConfigureNeuropixels1fHeadstage()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the voltage required for full
            // functionality. Locking occurs at around 2V on the headstage (enough to turn 1.8V on). Full functionality is at 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV1f NeuropixelsV1A { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV1f NeuropixelsV1B { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureTS4231 TS4231 { get; set; } = new() { Enable = false };

        internal override void UpdateDeviceNames()
        {
            LinkController.DeviceName = GetFullDeviceName(nameof(LinkController));
            NeuropixelsV1A.DeviceName = GetFullDeviceName(nameof(NeuropixelsV1A));
            NeuropixelsV1B.DeviceName = GetFullDeviceName(nameof(NeuropixelsV1B));
            Bno055.DeviceName = GetFullDeviceName(nameof(Bno055));
            TS4231.DeviceName = GetFullDeviceName(nameof(TS4231));
        }

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                NeuropixelsV1A.DeviceAddress = offset + 0;
                NeuropixelsV1B.DeviceAddress = offset + 1;
                Bno055.DeviceAddress = offset + 2;
                TS4231.DeviceAddress = offset + 3;
            }
        }

         [Description("If defined, overrides automated voltage discovery and applies " +
        "the specified voltage to the headstage. Warning: this device requires 5.0V to 5.5V " +
        "for proper operation. Higher voltages can damage the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV1A;
            yield return NeuropixelsV1B;
            yield return Bno055;
            yield return TS4231;
        }

        class ConfigureNeuropixels1fHeadstageLinkController : ConfigureFmcLinkController
        {

            public double? PortVoltage { get; set; } = null;

            // TODO: Needs more testing
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {

                if (PortVoltage == null)
                {
                    const double MinVoltage = 5.0;
                    const double MaxVoltage = 7.5;
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
}
