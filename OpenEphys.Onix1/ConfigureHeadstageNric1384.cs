using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures a Nric1384-203390 headstage on the specified port.
    /// </summary>
    [Description("Configures a NeuropixelsV1e headstage.")]
    public class ConfigureHeadstageNric1384 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageNric1384PortController PortControl = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureHeadstageNric1384"/> class.
        /// </summary>
        public ConfigureHeadstageNric1384()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Gets or sets the Nric1384, 384-channel bioacquisition chip  configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Nric1384 bioacquisition device.")]
        public ConfigureNric1384 Nric1384 { get; set; } = new();

        /// <summary>
        /// Gets or sets the BNO055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigureBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the headstage to the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                PortControl.DeviceAddress = (uint)port;
                Nric1384.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies
        /// the specified voltage to the headstage. To enable automated voltage discovery, leave this field 
        /// empty. Warning: This device requires 3.8V to 5.5V for proper operation. Voltages higher than 5.5V can 
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.5V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        [Category(ConfigurationCategory)]
        public double? PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Nric1384;
            yield return Bno055;
        }

        class ConfigureHeadstageNric1384PortController : ConfigurePortController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const double MinVoltage = 3.8;
                const double MaxVoltage = 5.5;
                const double VoltageOffset = 0.7;
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
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(500);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(500);
            }

            protected override bool CheckLinkState(DeviceContext device)
            {
                // NB: needs an additional reset after power on to provide its device table.
                device.Context.Reset();
                var linkState = device.ReadRegister(PortController.LINKSTATE);
                return (linkState & PortController.LINKSTATE_SL) != 0;
            }

        }
    }
}
