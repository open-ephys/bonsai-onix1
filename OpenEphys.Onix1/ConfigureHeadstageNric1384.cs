using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a Nric1384 headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The Nric1384 Headstage is a 2.5g serialized, multifunction headstage built around the IMEC
    /// Nric1384 bioacquisition chip and designed to function with passive probes of
    /// the user's choosing (e.g. silicon probe arrays, high-density tetrode drives, etc). It
    /// provides the following features:
    /// <list type="bullet">
    /// <item><description>384 analog ephys channels sampled at 30 kHz per channel and exposed via an array of
    /// 12x ultra-high density Molex 203390-0323 quad-row connectors. </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking at 100
    /// Hz.</description></item>
    /// <item><description>Two TS4231 light to digital converters for real-time, 3D position tracking with HTC
    /// Vive base stations.</description></item>
    /// <item><description>A single electrical stimulator (current controlled, +/-15V compliance, automatic
    /// electrode discharge).</description></item>
    /// </list>
    /// </remarks>
    [Description("Configures a Nric1384 headstage.")]
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
        /// Gets or sets the Nric1384 bioacquisition chip configuration.
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
        /// <para>
        /// If defined, it will override automated voltage discovery and apply the specified voltage to the headstage.
        /// If left blank, an automated headstage detection algorithm will attempt to communicate with the headstage and
        /// apply an appropriate voltage for stable operation. Because ONIX allows any coaxial tether to be used, some of
        /// which are thin enough to result in a significant voltage drop, its may be required to manually specify the
        /// port voltage.
        /// </para>
        /// <para>
        /// Warning: This device requires 3.8V to 5.5V for proper operation. Voltages higher than 5.5V can
        /// damage the headstage.
        /// </para>
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.5V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage
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

            public ConfigureHeadstageNric1384PortController()
                : base(typeof(PortController))
            {
            }

            protected override bool ConfigurePortVoltage(DeviceContext device, out double voltage)
            {
                const double MinVoltage = 3.8;
                const double MaxVoltage = 5.5;
                const double VoltageOffset = 0.7;
                const double VoltageIncrement = 0.2;

                voltage = MinVoltage;
                for (; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (base.CheckLinkState(device))
                    {
                        voltage += VoltageOffset;
                        SetPortVoltage(device, voltage);
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
