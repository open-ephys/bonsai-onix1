using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureDigitalIO : SingleDeviceFactory
    {
        public ConfigureDigitalIO()
            : base(typeof(DigitalIO))
        {
            DeviceAddress = 7;
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the digital IO device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DigitalIO.ID);
                device.WriteRegister(DigitalIO.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class DigitalIO
    {
        public const int ID = 18;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(DigitalIO))
            {
            }
        }
    }

    [Flags]
    public enum DigitalPortState : byte
    {
        Pin0 = 0x1,
        Pin1 = 0x2,
        Pin2 = 0x4,
        Pin3 = 0x8,
        Pin4 = 0x10,
        Pin5 = 0x20,
        Pin6 = 0x40,
        Pin7 = 0x80,
    }

    [Flags]
    public enum BreakoutLinkPowerState : byte
    {
        PortA = 0x1,
        PortB = 0x2,
        PortC = 0x4,
        PortD = 0x8
    }

    [Flags]
    public enum BreakoutButtonState : byte
    {
        Moon = 0x1,
        Triangle = 0x2,
        X = 0x4,
        Check = 0x8,
        Circle = 0x10,
        Square = 0x20
    }
}
