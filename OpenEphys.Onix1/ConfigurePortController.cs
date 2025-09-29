using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;

namespace OpenEphys.Onix1
{
    internal abstract class ConfigurePortController : SingleDeviceFactory
    {
        public ConfigurePortController()
            : base(typeof(PortController))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the hub device should be configured in standard or passthrough mode.")]
        public HubConfiguration HubConfiguration { get; set; }

        [Description("Specifies the supplied port voltage. If this value is specified, it will override automated" +
                     "voltage discovery. Warning: Supplying excessive voltage may result in damage to devices. " +
                     "Consult the device datasheet and documentation for allowable voltage ranges.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage { get; set; } = new();

        protected virtual bool CheckLinkState(DeviceContext device)
        {
            var linkState = device.ReadRegister(PortController.LINKSTATE);
            return (linkState & PortController.LINKSTATE_SL) != 0;
        }

        protected abstract bool ConfigurePortVoltage(DeviceContext device, out double voltage);

        protected virtual bool ConfigurePortVoltageOverride(DeviceContext device, double voltage)
        {
            device.WriteRegister(PortController.PORTVOLTAGE, (uint)(voltage * 10));
            Thread.Sleep(500);
            return CheckLinkState(device);
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var hubConfiguration = HubConfiguration;
            var portVoltage = PortVoltage;
            return source.ConfigureHost(context =>
            {
                // configure passthrough mode on the port controller
                // assuming the device address is the port number
                var portShift = ((int)deviceAddress - 1) * 2;
                var passthroughState = (hubConfiguration == HubConfiguration.Passthrough ? 1 : 0) << portShift;
                context.HubState = (PassthroughState)(((int)context.HubState & ~(1 << portShift)) | passthroughState);

                // leave in standard mode when finished
                return Disposable.Create(() => context.HubState = (PassthroughState)((int)context.HubState & ~(1 << portShift)));
            })
            .ConfigureLink(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                void dispose() { device.WriteRegister(PortController.PORTVOLTAGE, 0); }
                device.WriteRegister(PortController.ENABLE, 1);
                device.WriteRegister(PortController.DESPWR, 1); 

                bool serdesLock = false;
                if (portVoltage.Requested.HasValue)
                {
                    var voltage = portVoltage.Requested.GetValueOrDefault();
                    serdesLock = ConfigurePortVoltageOverride(device, voltage);
                    PortVoltage.Applied = voltage;

                } else
                {
                    serdesLock = ConfigurePortVoltage(device, out double voltage);
                    PortVoltage.Applied = voltage;
                }

                if (!serdesLock)
                {
                    var port = (PortName)deviceAddress;
                    var portString = port.GetType()
                                         .GetField(port.ToString())?
                                         .GetCustomAttribute<DescriptionAttribute>()?
                                         .Description ?? "Address " + deviceAddress.ToString();
                    var message = portVoltage.Requested.HasValue ?
                        $"Unable to acquire communication lock on {portString}" :
                        $"Unable to acquire communication lock on {portString}. You may need to manually specify a PortVoltage greater than {PortVoltage.Applied} volts, the maximum automatic value for this device.";

                    dispose();
                    throw new InvalidOperationException(message);
                }

                return Disposable.Create(dispose);
            })
            .ConfigureDevice(context =>
            {
                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }

    internal static class PortController
    {
        public const int ID = 23;
        public const uint MinimumVersion = 2;

        public const uint ENABLE = 0; // The LSB is used to enable or disable the device data stream
        public const uint GPOSTATE = 1; // GPO output state (bits 31 downto 3: ignore. bits 2 downto 0: ‘1’ = high, ‘0’ = low)
        public const uint DESPWR = 2; // Set link deserializer PDB state, 0 = deserializer power off else on. Does not affect port voltage.
        public const uint PORTVOLTAGE = 3; // 10 * link voltage
        public const uint SAVEVOLTAGE = 4; // Save link voltage to non-volatile EEPROM if greater than 0. This voltage will be applied after POR.
        public const uint LINKSTATE = 5; // bit 1 pass; bit 0 lock

        public const uint LINKSTATE_PP = 0x2; // parity check pass bit
        public const uint LINKSTATE_SL = 0x1; // SERDES lock bit

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(PortController))
            {
            }
        }
    }

    internal enum HubConfiguration
    {
        Standard,
        Passthrough
    }

    /// <summary>
    /// Specifies the headstage port status codes.
    /// </summary>
    [Flags]
    public enum PortStatusCode : ushort
    {
        /// <summary>
        /// Specifies the SERDES forward channel lock status.
        /// </summary>
        SerdesLock = 0x0001,

        /// <summary>
        /// Specifies the SERDES on-chip parity check status.
        /// </summary>
        SerdesParityPass = 0x0002,

        /// <summary>
        /// Specifies a cyclic redundancy check failure during data transmission.
        /// </summary>
        CrcError = 0x0100,

        /// <summary>
        /// Specifies that too many devices were indicated in the hub device table.
        /// </summary>
        TooManyDevices = 0x0200,

        /// <summary>
        /// Specifies a hub initialization error.
        /// </summary>
        InitializationError = 0x0400,

        /// <summary>
        /// Specifies the receipt of a badly formatted data packet.
        /// </summary>
        BadPacketFormat = 0x0800,

        /// <summary>
        /// Specifies the a cyclic redundancy check failure during hub initialization.
        /// </summary>
        InitializationCrcError = 0x1000,
    }

    /// <summary>
    /// Specifies the physical port that a headstage is plugged into.
    /// </summary>
    /// <remarks>
    /// ONIX uses a common protocol to communicate with a variety of devices using the same physical
    /// connection. For this reason lots of different headstage types can be plugged into a headstage port.
    /// </remarks>
    public enum PortName
    {
        /// <summary>
        /// Specifies Port A.
        /// </summary>
        [Description("Port A")]
        PortA = 1,
        /// <summary>
        /// Specifies Port B.
        /// </summary>
        [Description("Port B")]
        PortB = 2
    }
}
