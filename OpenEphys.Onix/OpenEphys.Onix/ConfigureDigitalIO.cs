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
    public enum PortState : byte
    {
        Input0 = 0x1,
        Input1 = 0x2,
        Input2 = 0x4,
        Input3 = 0x8,
        Input4 = 0x10,
        Input5 = 0x20,
        Input6 = 0x40,
        Input7 = 0x80,
    }

    [Flags]
    public enum LinkState : byte
    {
        PortA = 0x1,
        PortB = 0x2,
        PortC = 0x4,
        PortD = 0x8
    }

    [Flags]
    public enum ButtonState : byte
    {
        Button0 = 0x1,
        Button1 = 0x2,
        Button2 = 0x4,
        Button3 = 0x8,
        Button4 = 0x10,
        Button5 = 0x20
    }
}
