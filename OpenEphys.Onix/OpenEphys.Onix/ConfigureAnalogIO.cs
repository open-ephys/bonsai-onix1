using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureAnalogIO : SingleDeviceFactory
    {
        public ConfigureAnalogIO()
            : base(typeof(AnalogIO))
        {
            DeviceAddress = 6;
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the analog IO device is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 0.")]
        public AnalogIOVoltageRange InputRange00 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 1.")]
        public AnalogIOVoltageRange InputRange01 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 2.")]
        public AnalogIOVoltageRange InputRange02 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 3.")]
        public AnalogIOVoltageRange InputRange03 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 4.")]
        public AnalogIOVoltageRange InputRange04 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 5.")]
        public AnalogIOVoltageRange InputRange05 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 6.")]
        public AnalogIOVoltageRange InputRange06 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 7.")]
        public AnalogIOVoltageRange InputRange07 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 8.")]
        public AnalogIOVoltageRange InputRange08 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 9.")]
        public AnalogIOVoltageRange InputRange09 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 10.")]
        public AnalogIOVoltageRange InputRange10 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 11.")]
        public AnalogIOVoltageRange InputRange11 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 0.")]
        public AnalogIODirection Direction00 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 1.")]
        public AnalogIODirection Direction01 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 2.")]
        public AnalogIODirection Direction02 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 3.")]
        public AnalogIODirection Direction03 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 4.")]
        public AnalogIODirection Direction04 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 5.")]
        public AnalogIODirection Direction05 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 6.")]
        public AnalogIODirection Direction06 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 7.")]
        public AnalogIODirection Direction07 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 8.")]
        public AnalogIODirection Direction08 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 9.")]
        public AnalogIODirection Direction09 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 10.")]
        public AnalogIODirection Direction10 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 11.")]
        public AnalogIODirection Direction11 { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, AnalogIO.ID);
                device.WriteRegister(AnalogIO.ENABLE, Enable ? 1u : 0u);
                device.WriteRegister(AnalogIO.CH00INRANGE, (uint)InputRange00);
                device.WriteRegister(AnalogIO.CH01INRANGE, (uint)InputRange01);
                device.WriteRegister(AnalogIO.CH02INRANGE, (uint)InputRange02);
                device.WriteRegister(AnalogIO.CH03INRANGE, (uint)InputRange03);
                device.WriteRegister(AnalogIO.CH04INRANGE, (uint)InputRange04);
                device.WriteRegister(AnalogIO.CH05INRANGE, (uint)InputRange05);
                device.WriteRegister(AnalogIO.CH06INRANGE, (uint)InputRange06);
                device.WriteRegister(AnalogIO.CH07INRANGE, (uint)InputRange07);
                device.WriteRegister(AnalogIO.CH08INRANGE, (uint)InputRange08);
                device.WriteRegister(AnalogIO.CH09INRANGE, (uint)InputRange09);
                device.WriteRegister(AnalogIO.CH10INRANGE, (uint)InputRange10);
                device.WriteRegister(AnalogIO.CH11INRANGE, (uint)InputRange11);

                var io_reg = 0u;
                void SetIO(int channel, AnalogIODirection direction)
                {
                    io_reg = (io_reg & ~((uint)1 << channel)) | ((uint)(direction) << channel);
                    device.WriteRegister(AnalogIO.CHDIR, io_reg);
                }

                SetIO(0, Direction00);
                SetIO(1, Direction01);
                SetIO(2, Direction02);
                SetIO(3, Direction03);
                SetIO(4, Direction04);
                SetIO(5, Direction05);
                SetIO(6, Direction06);
                SetIO(7, Direction07);
                SetIO(8, Direction08);
                SetIO(9, Direction09);
                SetIO(10, Direction10);
                SetIO(11, Direction11);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class AnalogIO
    {
        public const int ID = 22;

        // constants
        public const int ChannelCount = 12;

        // managed registers
        public const uint ENABLE = 0;
        public const uint CHDIR = 1;
        public const uint CH00INRANGE = 2;
        public const uint CH01INRANGE = 3;
        public const uint CH02INRANGE = 4;
        public const uint CH03INRANGE = 5;
        public const uint CH04INRANGE = 6;
        public const uint CH05INRANGE = 7;
        public const uint CH06INRANGE = 8;
        public const uint CH07INRANGE = 9;
        public const uint CH08INRANGE = 10;
        public const uint CH09INRANGE = 11;
        public const uint CH10INRANGE = 12;
        public const uint CH11INRANGE = 13;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(AnalogIO))
            {
            }
        }
    }

    public enum AnalogIOVoltageRange
    {
        [Description("+/-10.0 V")]
        TenVolts = 0,
        [Description("+/-2.5 V")]
        TwoPointFiveVolts = 1,
        [Description("+/-5.0 V")]
        FiveVolts,
    }

    public enum AnalogIODirection
    {
        Input = 0,
        Output = 1
    }
}
