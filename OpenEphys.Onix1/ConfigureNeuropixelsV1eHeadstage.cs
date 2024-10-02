using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV1e headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV1e Headstage is a 0.68g serialized, multifunction headstage for small animals. This
    /// headstage is designed to function with IMEC Neuropixels V1 probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for a single IMEC Neuropixels 1.0 probe that features:
    /// <list type="bullet">
    /// <item><description>A single 1 cm long shank probe with a 70 x 24 µm shank cross-section.</description></item>
    /// <item><description>960-electrode low-impedance TiN electrodes.</description></item>
    /// <item><description>384 parallel, dual-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Description("Configures a NeuropixelsV1e headstage.")]
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureNeuropixelsV1eHeadstage : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV1ePortController PortControl = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1eHeadstage"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1eHeadstage()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV1e configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV1e device.")]
        public ConfigureNeuropixelsV1e NeuropixelsV1e { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(PolledBno055SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.ZXY, AxisSign = Bno055AxisSign.MirrorZ | Bno055AxisSign.MirrorY };

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
                NeuropixelsV1e.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies the
        /// specified voltage to the headstage. To enable automated voltage discovery, leave this field empty.
        /// Warning: This device requires 3.8V to 5.0V, measured at the headstage, for proper operation.
        /// Voltages higher than 5.0V can damage the headstage.
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.0V " +
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
            yield return NeuropixelsV1e;
            yield return Bno055;
        }

        class ConfigureNeuropixelsV1ePortController : ConfigurePortController
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
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(200);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(200);
            }
        }
    }
}
