using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that configures a NeuropixelsV1e headstage.
    /// </summary>
    public class ConfigureNeuropixelsV1eHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV1LinkController LinkController = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNeuropixelsV1eHeadstage"/> object.
        /// </summary>
        public ConfigureNeuropixelsV1eHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV1e"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        [Description("Configure a NeuropixelsV1e device.")]
        public ConfigureNeuropixelsV1e NeuropixelsV1 { get; set; } = new();

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV1eBno055"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        [Description("Configure a NeuropixelsV1eBno055 device.")]
        public ConfigureNeuropixelsV1eBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Get or set the <see cref="PortName"/>.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection on the breakout board.
        /// </remarks>
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                NeuropixelsV1.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Get or set the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies
        /// the specified voltage to the headstage. To enable automated voltage discovery, leave this field 
        /// empty. Warning: This device requires 3.8V to 5.0V for proper operation. Voltages higher than 5.0V can 
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.0V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV1;
            yield return Bno055;
        }

        class ConfigureNeuropixelsV1LinkController : ConfigureFmcLinkController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
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

            void SetVoltage(DeviceContext device, double voltage)
            {
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                Thread.Sleep(200);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(200);
            }
        }
    }
}
