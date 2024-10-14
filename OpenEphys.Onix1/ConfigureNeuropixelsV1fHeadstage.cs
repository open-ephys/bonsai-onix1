using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV1f headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsVf Headstage is a 1g serialized, multifunction headstage for small animals. This
    /// headstage is designed to function with IMEC Neuropixels V1 probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for a 2x IMEC Neuropixels 1.0 probes, each of which features:
    /// <list type="bullet">
    /// <item><description>A single 1 cm long shank probe with a 70 x 24 µm shank cross-section.</description></item>
    /// <item><description>960-electrode low-impedance TiN electrodes.</description></item>
    /// <item><description>384 parallel, dual-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking, updated at 100 Hz.</description></item>
    /// <item><description>Three TS4231 light to digital converters for real-time, 3D position tracking with
    /// HTC Vive base stations.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1fHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV1f headstage.")]
    public class ConfigureNeuropixelsV1fHeadstage : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixels1fHeadstageLinkController PortControl = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1eHeadstage"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1fHeadstage()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV1 probe A configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV1 in connector A.")]
        public ConfigureNeuropixelsV1f NeuropixelsV1A { get; set; } = new(NeuropixelsV1Probe.ProbeA);

        /// <summary>
        /// Gets or sets the NeuropixelsV1 probe B configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV1 in connector B.")]
        public ConfigureNeuropixelsV1f NeuropixelsV1B { get; set; } = new(NeuropixelsV1Probe.ProbeB);

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

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies the
        /// specified voltage to the headstage. To enable automated voltage discovery, leave this field empty.
        /// Warning: This device requires 4.5V to 5.5V, measured at the headstage, for proper operation.
        /// Voltages higher than 6.0V can damage the headstage.
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
        "the specified voltage to the headstage. Warning: this device requires 4.5V to 5.5V " +
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
                    const double MaxVoltage = 7.0;
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
