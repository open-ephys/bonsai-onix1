using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures an ONIX headstage-64 in the specified port.
    /// </summary>
    [Description("Configures an ONIX headstage-64 in the specified port.")]
    public class ConfigureHeadstage64 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstage64LinkController LinkController = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64"/> class.
        /// </summary>
        /// <remarks>
        /// Headstage-64 is a 1.5g serialized, multifunction headstage for small animals. This headstage is designed to function
        /// with tetrode microdrives. Alternatively it can be used with other passive probes (e.g. silicon arrays, EEG/ECOG arrays,
        /// etc.). It provides the following features on the headstage:
        /// <list type="bullet">
        /// <item><description>64 analog ephys channels and 3 auxiliary channels sampled at 30 kHz per channel.</description></item>
        /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
        /// <item><description>Three TS4231 light to digital converters for real-time, 3D position tracking with HTC Vive base stations.</description></item>
        /// <item><description>A single electrical stimulator (current controlled, +/-15V compliance, automatic electrode discharge).</description></item>
        /// <item><description>Two optical stimulators (800 mA peak current per channel).</description></item>
        /// </list>
        /// </remarks>
        public ConfigureHeadstage64()
        {
            // WONTFIX: The issue with this headstage is that its locking voltage is far, far lower than the voltage required for full
            // functionality. Locking occurs at around 2V on the headstage (enough to turn 1.8V on). Full functionality is at 5.0 volts.
            // The FMC port voltage can only go down to 3.3V, which means that its very hard to find the true lowest voltage
            // for a lock and then add a large offset to that. Fixing this requires a hardware change.
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Gets or sets the Rhd2164 configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Rhd2164 device in the headstage-64.")]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device in the headstage-64.")]
        public ConfigureBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Gets or sets the SteamVR V1 basestation 3D tracking array configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the TS4231 device in the headstage-64.")]
        public ConfigureTS4231V1 TS4231 { get; set; } = new() { Enable = false };

        /// <summary>
        /// Gets or sets onboard electrical stimulator configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the ElectricalStimulator device in the headstage-64.")]
        public ConfigureHeadstage64ElectricalStimulator ElectricalStimulator { get; set; } = new();

        /// <summary>
        /// Gets or sets onboard optical stimulator configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the OpticalStimulator device in the headstage-64.")]
        public ConfigureHeadstage64OpticalStimulator OpticalStimulator { get; set; } = new();

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the headstage to the ONIX breakout board.")]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
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
        /// If defined, it will override automated voltage discovery and apply the specified voltage to the headstage.
        /// If left blank, an automated headstage detection algorithm will attempt to communicate with the headstage and
        /// apply an appropriate voltage for stable operation. Because ONIX allows any coaxial tether to be used, some of
        /// which are thin enough to result in a significant voltage drop, its may be required to manually specify the
        /// port voltage.
        /// </para>
        /// <para>
        /// Warning: this device requires 5.5V to 6.0V, measured at the headstage, for proper operation. Supplying higher
        /// voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 5.5V to 6.0V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhd2164;
            yield return Bno055;
            yield return TS4231;
            yield return ElectricalStimulator;
            yield return OpticalStimulator;
        }

        class ConfigureHeadstage64LinkController : ConfigureFmcLinkController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                // WONTFIX: It takes a huge amount of time to get to 0, almost 10 seconds.
                // The best we can do at the moment is drive port voltage to minimum which
                // is an active process and then settle from there to zero volts. This requires
                // a hardware revision that discharges the headstage between cycles to fix.
                const uint MinVoltage = 33;
                const uint MaxVoltage = 60;
                const uint VoltageOffset = 34;
                const uint VoltageIncrement = 02;

                // Start with highest voltage and ramp it down to find lowest lock voltage
                var voltage = MaxVoltage;
                for (; voltage >= MinVoltage; voltage -= VoltageIncrement)
                {
                    device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage);
                    Thread.Sleep(200);
                    if (!CheckLinkState(device))
                    {
                        if (voltage == MaxVoltage) return false;
                        else break;
                    }
                }

                device.WriteRegister(FmcLinkController.PORTVOLTAGE, MinVoltage);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                Thread.Sleep(1000);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage + VoltageOffset);
                Thread.Sleep(200);
                return CheckLinkState(device);
            }
        }
    }

    /// <summary>
    /// Specifies the physical port that a headstage is plugged into.
    /// </summary>
    /// <remarks>
    /// ONIX uses a common protocol to communicate with a variety of devices using the same physical connection. For this reason
    /// lots of different headstage types can be plugged into a headstage port.
    /// </remarks>
    public enum PortName
    {
        /// <summary>
        /// Specifies Port A.
        /// </summary>
        PortA = 1,
        /// <summary>
        /// Specifies Port B.
        /// </summary>
        PortB = 2
    }
}
