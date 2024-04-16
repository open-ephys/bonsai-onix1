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
        public VoltageRange InputRange00 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 1.")]
        public VoltageRange InputRange01 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 2.")]
        public VoltageRange InputRange02 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 3.")]
        public VoltageRange InputRange03 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 4.")]
        public VoltageRange InputRange04 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 5.")]
        public VoltageRange InputRange05 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 6.")]
        public VoltageRange InputRange06 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 7.")]
        public VoltageRange InputRange07 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 8.")]
        public VoltageRange InputRange08 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 9.")]
        public VoltageRange InputRange09 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 10.")]
        public VoltageRange InputRange10 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 11.")]
        public VoltageRange InputRange11 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 0.")]
        public ChannelDirection Direction00 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 1.")]
        public ChannelDirection Direction01 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 2.")]
        public ChannelDirection Direction02 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 3.")]
        public ChannelDirection Direction03 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 4.")]
        public ChannelDirection Direction04 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 5.")]
        public ChannelDirection Direction05 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 6.")]
        public ChannelDirection Direction06 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 7.")]
        public ChannelDirection Direction07 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 8.")]
        public ChannelDirection Direction08 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 9.")]
        public ChannelDirection Direction09 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 10.")]
        public ChannelDirection Direction10 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 11.")]
        public ChannelDirection Direction11 { get; set; }

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
                void SetIO(int channel, ChannelDirection direction)
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

    public enum VoltageRange
    {
        [Description("+/-10.0 V")]
        TenVolts = 0,
        [Description("+/-2.5 V")]
        TwoPointFiveVolts = 1,
        [Description("+/-5.0 V")]
        FiveVolts,
    }

    public enum ChannelDirection
    {
        Input = 0,
        Output = 1
    }
}
