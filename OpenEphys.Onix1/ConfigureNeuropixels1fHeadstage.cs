using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    public class ConfigureNeuropixels1fHeadstage : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixels1fHeadstageLinkController PortControl = new();

        public ConfigureNeuropixels1fHeadstage()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the voltage required for full
            // functionality. Locking occurs at around 2V on the headstage (enough to turn 1.8V on). Full functionality is at 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV1A configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV1 in connector A.")]
        public ConfigureNeuropixelsV1f NeuropixelsV1A { get; set; } = new();

        /// <summary>
        /// Gets or sets the NeuropixelsV1B configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV1 in connector B.")]
        public ConfigureNeuropixelsV1f NeuropixelsV1B { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(PolledBno055SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigureBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Gets or sets the SteamVR V1 basestation 3D tracking array configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the TS4231 device in the headstage-64.")]
        public ConfigureTS4231V1 TS4231 { get; set; } = new() { Enable = false };

        internal override void UpdateDeviceNames()
        {
            PortControl.DeviceName = GetFullDeviceName(nameof(PortControl));
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
                PortControl.DeviceAddress = (uint)port;
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
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return NeuropixelsV1A;
            yield return NeuropixelsV1B;
            yield return Bno055;
            yield return TS4231;
        }

        class ConfigureNeuropixels1fHeadstageLinkController : ConfigurePortController
        {
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
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(300);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(500);
            }
        }
    }
}
