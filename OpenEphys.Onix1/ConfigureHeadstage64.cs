using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an ONIX multifunction 64-channel headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// Headstage-64 is a 1.5g serialized, multifunction headstage designed to function with passive
    /// probes such as tetrode microdrives, silicon arrays, EEG/ECOG arrays, etc. It provides the
    /// following features:
    /// <list type="bullet">
    /// <item><description>64 electrophysiology channels and 3 auxiliary channels sampled at 30 kHz per
    /// channel.</description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// <item><description>Three TS4231 light to digital converters for real-time, 3D position tracking with
    /// HTC Vive base stations.</description></item>
    /// <item><description>A single electrical stimulator (current controlled, +/-15V compliance, automatic
    /// electrode discharge).</description></item>
    /// <item><description>Two optical stimulators (800 mA peak current per channel).</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.Headstage64Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures an ONIX multifunction 64-channel headstage.")]
    public class ConfigureHeadstage64 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstage64PortController PortControl = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64"/> class.
        /// </summary>
        public ConfigureHeadstage64()
        {
            // WONTFIX: The issue with this headstage is that its locking voltage is far, far lower than the
            // voltage required for full functionality. Locking occurs at around 2V on the headstage (enough
            // to turn 1.8V on). Full functionality is at 5.0 volts. The FMC port voltage can only go down to
            // 3.3V, which means that its very hard to find the true lowest voltage for a lock and then add a
            // large offset to that. Fixing this requires a hardware change.
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Initializes a copied instance of the <see cref="ConfigureHeadstage64"/> class.
        /// </summary>
        /// <param name="configureNode">Existing <see cref="ConfigureHeadstage64"/> instance.</param>
        public ConfigureHeadstage64(ConfigureHeadstage64 configureNode)
        {
            Name = configureNode.Name;
            Port = configureNode.Port;
            PortControl = configureNode.PortControl;
            Rhd2164 = new(configureNode.Rhd2164);
            Bno055 = new(configureNode.Bno055);
            TS4231 = new(configureNode.TS4231);
            ElectricalStimulator = new(configureNode.ElectricalStimulator);
            OpticalStimulator = new(configureNode.OpticalStimulator);
        }

        /// <summary>
        /// Gets or sets the Rhd2164 configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Rhd2164 device in the headstage-64.")]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device in the headstage-64.")]
        public ConfigureBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Gets or sets the SteamVR V1 basestation 3D tracking array configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the TS4231 device in the headstage-64.")]
        public ConfigureTS4231V1 TS4231 { get; set; } = new();

        /// <summary>
        /// Gets or sets onboard electrical stimulator configuration.
        /// </summary>
        /// <inheritdoc cref="ConfigureHeadstage64ElectricalStimulator"/>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the ElectricalStimulator device in the headstage-64.")]
        [Editor("OpenEphys.Onix1.Design.Headstage64ElectricalStimulatorUITypeEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        public ConfigureHeadstage64ElectricalStimulator ElectricalStimulator { get; set; } = new();

        /// <summary>
        /// Gets or sets onboard optical stimulator configuration.
        /// </summary>
        /// <inheritdoc cref="ConfigureHeadstage64OpticalStimulator"/>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the OpticalStimulator device in the headstage-64.")]
        [Editor("OpenEphys.Onix1.Design.Headstage64OpticalStimulatorUITypeEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        public ConfigureHeadstage64OpticalStimulator OpticalStimulator { get; set; } = new();

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to
        /// operation.
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
                Rhd2164.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
                TS4231.DeviceAddress = offset + 2;
                ElectricalStimulator.DeviceAddress = offset + 3;
                OpticalStimulator.DeviceAddress = offset + 4;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage override.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If defined, it will override automated voltage discovery and apply the specified voltage to the
        /// headstage. If left blank, an automated headstage detection algorithm will attempt to communicate
        /// with the headstage and apply an appropriate voltage for stable operation. Because ONIX allows any
        /// coaxial tether to be used, some of which are thin enough to result in a significant voltage drop,
        /// its may be required to manually specify the port voltage.
        /// </para>
        /// <para>
        /// Warning: this device requires 5.5V to 6.0V, measured at the headstage, for proper operation.
        /// Supplying higher voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("If defined, it will override automated voltage discovery and apply the specified voltage " +
                     "to the headstage. Warning: this device requires 5.5V to 6.0V, measured at the headstage, " +
                     "for proper operation. Supplying higher voltages may result in damage to the headstage.")]
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
            yield return Rhd2164;
            yield return Bno055;
            yield return TS4231;
            yield return ElectricalStimulator;
            yield return OpticalStimulator;
        }

        class ConfigureHeadstage64PortController : ConfigurePortController
        {
            protected override bool ConfigurePortVoltageOverride(DeviceContext device, double voltage)
            {
                // NB: Wait for 1 second to discharge the headstage in the case that they have e.g. just
                // restarted the workflow automatically with nearly no delay from the last run. 
                Thread.Sleep(1000);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(voltage * 10));
                Thread.Sleep(500);
                return CheckLinkState(device);
            }

            protected override bool ConfigurePortVoltage(DeviceContext device, out double voltage)
            {
                // WONTFIX: It takes a huge amount of time to get to 0, almost 10 seconds. The best we can do
                // at the moment is drive port voltage to minimum which is an active process and then settle
                // from there to zero volts. This requires a hardware revision that discharges the headstage
                // between cycles to fix.
                const double MinVoltage = 3.3;
                const double MaxVoltage = 6.0;
                const double VoltageOffset = 4.0;
                const double VoltageIncrement = 0.2;

                // NB: Wait for 1 second to discharge the headstage in the case that they have e.g. just
                // restarted the workflow automatically with nearly no delay from the last run. 
                Thread.Sleep(1000);

                // Start with highest voltage and ramp it down to find lowest lock voltage
                voltage = MaxVoltage;
                for (; voltage >= MinVoltage; voltage -= VoltageIncrement)
                {
                    device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                    Thread.Sleep(200);
                    if (!CheckLinkState(device))
                    {
                        if (voltage == MaxVoltage)
                        {
                            return false;
                        }
                        else break;
                    }
                }

                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * MinVoltage));
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(1000);
                voltage += VoltageOffset;
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(200);
                return CheckLinkState(device);
            }
        }
    }
}
