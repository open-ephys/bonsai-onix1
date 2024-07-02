using System;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix
{
    [TypeConverter(typeof(SortedPropertyConverter))]
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
        public AnalogIOVoltageRange InputRange0 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 1.")]
        public AnalogIOVoltageRange InputRange1 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 2.")]
        public AnalogIOVoltageRange InputRange2 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 3.")]
        public AnalogIOVoltageRange InputRange3 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 4.")]
        public AnalogIOVoltageRange InputRange4 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 5.")]
        public AnalogIOVoltageRange InputRange5 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 6.")]
        public AnalogIOVoltageRange InputRange6 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 7.")]
        public AnalogIOVoltageRange InputRange7 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 8.")]
        public AnalogIOVoltageRange InputRange8 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 9.")]
        public AnalogIOVoltageRange InputRange9 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 10.")]
        public AnalogIOVoltageRange InputRange10 { get; set; }

        [Category(ConfigurationCategory)]
        [Description("The input voltage range of channel 11.")]
        public AnalogIOVoltageRange InputRange11 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 0.")]
        public AnalogIODirection Direction0 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 1.")]
        public AnalogIODirection Direction1 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 2.")]
        public AnalogIODirection Direction2 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 3.")]
        public AnalogIODirection Direction3 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 4.")]
        public AnalogIODirection Direction4 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 5.")]
        public AnalogIODirection Direction5 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 6.")]
        public AnalogIODirection Direction6 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 7.")]
        public AnalogIODirection Direction7 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 8.")]
        public AnalogIODirection Direction8 { get; set; }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 9.")]
        public AnalogIODirection Direction9 { get; set; }

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
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(AnalogIO.ENABLE, Enable ? 1u : 0u);
                device.WriteRegister(AnalogIO.CH00INRANGE, (uint)InputRange0);
                device.WriteRegister(AnalogIO.CH01INRANGE, (uint)InputRange1);
                device.WriteRegister(AnalogIO.CH02INRANGE, (uint)InputRange2);
                device.WriteRegister(AnalogIO.CH03INRANGE, (uint)InputRange3);
                device.WriteRegister(AnalogIO.CH04INRANGE, (uint)InputRange4);
                device.WriteRegister(AnalogIO.CH05INRANGE, (uint)InputRange5);
                device.WriteRegister(AnalogIO.CH06INRANGE, (uint)InputRange6);
                device.WriteRegister(AnalogIO.CH07INRANGE, (uint)InputRange7);
                device.WriteRegister(AnalogIO.CH08INRANGE, (uint)InputRange8);
                device.WriteRegister(AnalogIO.CH09INRANGE, (uint)InputRange9);
                device.WriteRegister(AnalogIO.CH10INRANGE, (uint)InputRange10);
                device.WriteRegister(AnalogIO.CH11INRANGE, (uint)InputRange11);

                // Build the whole value for CHDIR and write it once
                static uint SetIO(uint io_reg, int channel, AnalogIODirection direction) =>
                    (io_reg & ~((uint)1 << channel)) | ((uint)(direction) << channel);

                var io_reg = 0u;
                io_reg = SetIO(io_reg, 0, Direction0);
                io_reg = SetIO(io_reg, 1, Direction1);
                io_reg = SetIO(io_reg, 2, Direction2);
                io_reg = SetIO(io_reg, 3, Direction3);
                io_reg = SetIO(io_reg, 4, Direction4);
                io_reg = SetIO(io_reg, 5, Direction5);
                io_reg = SetIO(io_reg, 6, Direction6);
                io_reg = SetIO(io_reg, 7, Direction7);
                io_reg = SetIO(io_reg, 8, Direction8);
                io_reg = SetIO(io_reg, 9, Direction9);
                io_reg = SetIO(io_reg, 10, Direction10);
                io_reg = SetIO(io_reg, 11, Direction11);
                device.WriteRegister(AnalogIO.CHDIR, io_reg);

                var deviceInfo = new AnalogIODeviceInfo(device, this);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }

        class SortedPropertyConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var properties = base.GetProperties(context, value, attributes);
                var sortedOrder = properties.Cast<PropertyDescriptor>()
                                            .Where(p => p.PropertyType == typeof(AnalogIOVoltageRange)
                                                     || p.PropertyType == typeof(AnalogIODirection))
                                            .OrderBy(p => p.PropertyType.MetadataToken)
                                            .Select(p => p.Name)
                                            .Prepend(nameof(Enable))
                                            .ToArray();
                return properties.Sort(sortedOrder);
            }
        }
    }

    static class AnalogIO
    {
        public const int ID = 22;

        // constants
        public const int ChannelCount = 12;
        public const int NumberOfDivisions = 1 << 16;

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

    class AnalogIODeviceInfo : DeviceInfo
    {
        public AnalogIODeviceInfo(DeviceContext device, ConfigureAnalogIO deviceFactory)
            : base(device, deviceFactory.DeviceType)
        {
            VoltsPerDivision = new[]
            {
                GetVoltsPerDivision(deviceFactory.InputRange0),
                GetVoltsPerDivision(deviceFactory.InputRange1),
                GetVoltsPerDivision(deviceFactory.InputRange2),
                GetVoltsPerDivision(deviceFactory.InputRange3),
                GetVoltsPerDivision(deviceFactory.InputRange4),
                GetVoltsPerDivision(deviceFactory.InputRange5),
                GetVoltsPerDivision(deviceFactory.InputRange6),
                GetVoltsPerDivision(deviceFactory.InputRange7),
                GetVoltsPerDivision(deviceFactory.InputRange8),
                GetVoltsPerDivision(deviceFactory.InputRange9),
                GetVoltsPerDivision(deviceFactory.InputRange10),
                GetVoltsPerDivision(deviceFactory.InputRange11)
            };
        }

        public static float GetVoltsPerDivision(AnalogIOVoltageRange voltageRange)
        {
            return voltageRange switch
            {
                AnalogIOVoltageRange.TenVolts => 20.0f / AnalogIO.NumberOfDivisions,
                AnalogIOVoltageRange.TwoPointFiveVolts => 5.0f / AnalogIO.NumberOfDivisions,
                AnalogIOVoltageRange.FiveVolts => 10.0f / AnalogIO.NumberOfDivisions,
                _ => throw new ArgumentOutOfRangeException(nameof(voltageRange)),
            };
        }

        public float[] VoltsPerDivision { get; }
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

    public enum AnalogIODataType
    {
        S16,
        Volts
    }
}
