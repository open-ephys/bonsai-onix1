using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a UCLA Miniscope V4 on the specified port.
    /// </summary>
    /// <remarks>
    /// The UCLA Miniscope V4 is a e miniaturized fluorescent microscope for performing single-photon calcium
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
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureUclaMiniscopeV4Bno055 Bno055 { get; set; } = new();

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
        /// Warning: this device requires 5.0V to 6.0V, measured at the miniscope, for proper operation. Supplying higher
        /// voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the miniscope. Warning: this device requires 5.0V to 6.0V for proper operation." +
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
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const uint MinVoltage = 50;
                const uint MaxVoltage = 70;
                const uint VoltageOffset = 02;
                const uint VoltageIncrement = 02;

                for (uint voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (CheckLinkState(device))
                    {
                        SetPortVoltage(device, voltage + VoltageOffset);
                        return CheckLinkState(device);
                    }
                }

                return false;
            }

            private void SetPortVoltage(DeviceContext device, uint voltage)
            {
                const int WaitUntilVoltageSettles = 200;
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(WaitUntilVoltageSettles);
                device.WriteRegister(PortController.PORTVOLTAGE, voltage);
                Thread.Sleep(WaitUntilVoltageSettles);
            }

            override protected bool CheckLinkState(DeviceContext device)
            {
                try
                {
                    var ds90ub9x = device.Context.GetPassthroughDeviceContext(DeviceAddress << 8, typeof(DS90UB9x));
                    ConfigureUclaMiniscopeV4Camera.ConfigureMiniscope(ds90ub9x);
                }
                catch (oni.ONIException ex)
                {
                    // this can occur if power is too low, so we need to be able to try again
                    const int FailureToWriteRegister = -6;
                    if (ex.Number != FailureToWriteRegister)
                    {
                        throw;
                    }
                }

                Thread.Sleep(200);

                var linkState = device.ReadRegister(PortController.LINKSTATE);
                return (linkState & PortController.LINKSTATE_SL) != 0;
            }
        }
    }
}
