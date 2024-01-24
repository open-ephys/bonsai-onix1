using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureDS90UB9x : SingleDeviceFactory
    {
        public ConfigureDS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the DS90UB9x raw device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DS90UB9x.ID);
                device.WriteRegister(DS90UB9x.ENABLE, enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class DS90UB9x
    {
        public const int ID = 24;

        // managed registers
        public const uint ENABLE = 0x8000;
        public const uint READSZ = 0x8001;
        public const uint TRIGGER = 0x8002;
        public const uint TRIGGEROFF = 0x8003;
        public const uint DATAGATE = 0x8004;
        public const uint SYNCBITS = 0x8005;
        public const uint MARK = 0x8006;
        public const uint MAGIC_MASK = 0x8007;
        public const uint MAGIC = 0x8008;
        public const uint MAGIC_WAIT = 0x8009;
        public const uint DATAMODE = 0x800A;
        public const uint DATALINES0 = 0x800B;
        public const uint DATALINES1 = 0x800C;

        // reserved registers
        public const uint GPIO_DIR = 0x8010;
        public const uint GPIO_VAL = 0x8011;
        public const uint LINKSTATUS = 0x8012;
        public const uint LASTI2CL = 0x8013;
        public const uint LASTI2CH = 0x8014;

        // unmanaged default serializer / deserializer I2C addresses
        public const uint DES_ADDR = 0x30;
        public const uint SER_ADDR = 0x58;
    }

    enum DS90UB9xTriggerMode : uint
    {
        Continuous = 0,
        HsyncEdgePositive = 0b0001,
        HsyncEdgeNegative = 0b1001,
        HsyncLevelPositive = 0b0101,
        HsyncLevelNegative = 0b1101,
        VsyncEdgePositive = 0b0011,
        VsyncEdgeNegative = 0b1011,
        VsyncLevelPositive = 0b0111,
        VsyncLevelNegative = 0b1111,
    }

    enum DS90UB9xDataGate : uint
    {
        Disabled = 0,
        HsyncPositive = 0b001,
        HsyncNegative = 0b101,
        VsyncPositive = 0b011,
        VsyncNegative = 0b111,
    }

    enum DS90UB9xMarkMode : uint
    {
        Disabled = 0,
        HsyncRising = 0b001,
        HsyncFalling = 0b101,
        VsyncRising = 0b011,
        VsyncFalling = 0b111,
    }

    enum DS90UB9xDeserializerI2CRegister
    {
        PortMode = 0x6D,

        SlaveID1 = 0x5E,
        SlaveID2 = 0x5F,
        SlaveID3 = 0x60,
        SlaveID4 = 0x61,
        SlaveID5 = 0x62,
        SlaveID6 = 0x63,
        SlaveID7 = 0x64,

        SlaveAlias1 = 0x66,
        SlaveAlias2 = 0x67,
        SlaveAlias3 = 0x68,
        SlaveAlias4 = 0x69,
        SlaveAlias5 = 0x6A,
        SlaveAlias6 = 0x6B,
        SlaveAlias7 = 0x6C,
    }

    enum DS90UB9xSerializerI2CRegister
    {
        GPIO10 = 0x0D,
        GPIO32 = 0x0E,
    }

    enum DS90UB9xMode
    {
        Raw12BitLowFrequency = 1,
        Raw12BitHighFrequency = 2,
        Raw10Bit = 3,
    }

    enum DS90UB9xDirection
    {
        Input = 0,
        Output = 1
    }
}
