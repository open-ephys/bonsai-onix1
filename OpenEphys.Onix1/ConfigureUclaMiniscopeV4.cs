using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using oni;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a UCLA Miniscope V4 on the specified port.
    /// </summary>
    /// <remarks>
    /// The UCLA Miniscope V4 is a miniaturized fluorescent microscope for performing single-photon calcium
    /// imaging in freely moving animals. It has the following features:
    /// <list type="bullet">
    /// <item><description>A Python-480 0.48 Megapixel CMOS image sensor.</description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// <item><description>An electrowetting lens for remote focal plane adjustment.</description></item>
    /// <item><description>An excitation LED with adjustable brightness control and optional exposure-driven
    /// interleaving to reduce photobleaching.</description></item>
    /// </list>
    /// </remarks>
    public class ConfigureUclaMiniscopeV4 : MultiDeviceFactory
    {
        const double MaxVoltage = 5.7;

        PortName port;
        readonly ConfigureUclaMiniscopeV4PortController PortControl = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureUclaMiniscopeV4"/> class.
        /// </summary>
        public ConfigureUclaMiniscopeV4()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the Miniscope camera configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureUclaMiniscopeV4Camera Camera { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(PolledBno055SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.ZYX, AxisSign = Bno055AxisSign.MirrorX | Bno055AxisSign.MirrorY | Bno055AxisSign.MirrorZ };


        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the miniscope to the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                PortControl.DeviceAddress = (uint)port;
                Camera.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;

                // Hack: we configure the camera using the port controller below. configuration is super
                // unreliable, so we do a bunch of retries in the logic PortController logic. We don't want to
                // reperform configuration in the Camera object after this. So we capture a reference to the
                // camera here in order to inform it we have already performed configuration. 
                PortControl.Camera = Camera;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage override.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If defined, it will override automated voltage discovery and apply the specified voltage to the miniscope.
        /// If left blank, an automated headstage detection algorithm will attempt to communicate with the miniscope and
        /// apply an appropriate voltage for stable operation. Because ONIX allows any coaxial tether to be used, some of
        /// which are thin enough to result in a significant voltage drop, its may be required to manually specify the
        /// port voltage.
        /// </para>
        /// <para>
        /// Warning: this device requires 4.0 to 5.0V, measured at the miniscope, for proper operation. Supplying higher
        /// voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("If defined, it will override automated voltage discovery and apply the specified voltage " +
                     "to the miniscope. Warning: this device requires 4.0 to 5.0V, measured at the scope, for proper operation. " +
                     "Supplying higher voltages may result in damage to the miniscope.")]
        [Category(ConfigurationCategory)]
        public double? PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Camera;
            yield return Bno055;
        }

        class ConfigureUclaMiniscopeV4PortController : ConfigurePortController
        {
            internal ConfigureUclaMiniscopeV4Camera Camera;

            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const double MinVoltage = 5.2;
                const double VoltageIncrement = 0.05;

                for (var voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (CheckLinkStateWithRetry(device))
                    {
                        return true;
                    }
                }

                return false;
            }

            void SetPortVoltage(DeviceContext device, double voltage)
            {
                if (voltage > MaxVoltage)
                {
                    throw new ArgumentException($"The port voltage must be set to a value less than {MaxVoltage} " +
                        $"volts to prevent damage to the miniscope.");
                }

                const int WaitUntilVoltageSettles = 400;
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(WaitUntilVoltageSettles);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(WaitUntilVoltageSettles);
            }

            override protected bool CheckLinkState(DeviceContext device)
            {
                var ds90ub9x = device.Context.GetPassthroughDeviceContext(DeviceAddress << 8, typeof(DS90UB9x));
                ConfigureUclaMiniscopeV4Camera.ConfigureDeserializer(ds90ub9x);

                const int FailureToWriteRegister = -6;
                try
                {
                    ConfigureUclaMiniscopeV4Camera.ConfigureCameraSystem(ds90ub9x, Camera.FrameRate, Camera.InterleaveLed);
                }
                catch (ONIException ex) when (ex.Number == FailureToWriteRegister)
                {
                    return false;
                }

                Thread.Sleep(150);
                var linkState = device.ReadRegister(PortController.LINKSTATE);
                return (linkState & PortController.LINKSTATE_SL) != 0;
            }

            bool CheckLinkStateWithRetry(DeviceContext device)
            {
                const int TotalTries = 5;
                for (int i = 0; i < TotalTries; i++)
                {
                    if (CheckLinkState(device))
                    {
                        Camera.Configured = true;
                        return true;
                    }
                }

                return false;
            }

            override protected bool ConfigurePortVoltageOverride(DeviceContext device, double voltage)
            {
                const int TotalTries = 3;
                for (int i = 0; i < TotalTries; i++)
                {
                    SetPortVoltage(device, voltage);
                    if (CheckLinkStateWithRetry(device))
                        return true;
                }

                return false;
            }
        }
    }
}
