using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;
using oni;

namespace OpenEphys.Onix
{
    public class ConfigureAnalogIO : SingleDeviceFactory
    {
        const string ConfigurationCategory = "Configuration";
        const string AcquisitionCategory = "Acquisition";

        readonly BehaviorSubject<ChannelDirection> direction00 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction01 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction02 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction03 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction04 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction05 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction06 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction07 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction08 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction09 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction10 = new(ChannelDirection.Input);
        readonly BehaviorSubject<ChannelDirection> direction11 = new(ChannelDirection.Input);

        public ConfigureAnalogIO()
            : base(typeof(AnalogIO))
        {
            DeviceIndex = 6;
        }

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
        public ChannelDirection Direction00
        {
            get => direction00.Value;
            set => direction00.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 1.")]
        public ChannelDirection Direction01
        {
            get => direction01.Value;
            set => direction01.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 2.")]
        public ChannelDirection Direction02
        {
            get => direction02.Value;
            set => direction02.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 3.")]
        public ChannelDirection Direction03
        {
            get => direction03.Value;
            set => direction03.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 4.")]
        public ChannelDirection Direction04
        {
            get => direction04.Value;
            set => direction04.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 5.")]
        public ChannelDirection Direction05
        {
            get => direction05.Value;
            set => direction05.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 6.")]
        public ChannelDirection Direction06
        {
            get => direction06.Value;
            set => direction06.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 7.")]
        public ChannelDirection Direction07
        {
            get => direction07.Value;
            set => direction07.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 8.")]
        public ChannelDirection Direction08
        {
            get => direction08.Value;
            set => direction08.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 9.")]
        public ChannelDirection Direction09
        {
            get => direction09.Value;
            set => direction09.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 10.")]
        public ChannelDirection Direction10
        {
            get => direction10.Value;
            set => direction10.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("The direction of channel 11.")]
        public ChannelDirection Direction11
        {
            get => direction11.Value;
            set => direction11.OnNext(value);
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceIndex = DeviceIndex;
            return source.ConfigureDevice(context =>
            {
                if (!context.DeviceTable.TryGetValue(deviceIndex, out Device device))
                {
                    throw new InvalidOperationException("Selected device index is invalid.");
                }

                // Enable only takes effect after context reset
                context.WriteRegister(deviceIndex, AnalogIO.ENABLE, 1);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange00);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange01);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange02);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange03);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange04);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange05);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange06);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange07);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange08);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange09);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange10);
                context.WriteRegister(deviceIndex, AnalogIO.CH00INRANGE, (uint)InputRange11);

                var io_reg = 0u;
                void SetIO(int channel, ChannelDirection direction)
                {
                    io_reg = (io_reg & ~((uint)1 << channel)) | ((uint)(direction) << channel);
                    context.WriteRegister(deviceIndex, AnalogIO.CHDIR, io_reg);
                }

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceIndex);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return new CompositeDisposable(
                    disposable,
                    direction00.Subscribe(newValue => SetIO(0, newValue)),
                    direction01.Subscribe(newValue => SetIO(1, newValue)),
                    direction02.Subscribe(newValue => SetIO(2, newValue)),
                    direction03.Subscribe(newValue => SetIO(3, newValue)),
                    direction04.Subscribe(newValue => SetIO(4, newValue)),
                    direction05.Subscribe(newValue => SetIO(5, newValue)),
                    direction06.Subscribe(newValue => SetIO(6, newValue)),
                    direction07.Subscribe(newValue => SetIO(7, newValue)),
                    direction08.Subscribe(newValue => SetIO(8, newValue)),
                    direction09.Subscribe(newValue => SetIO(9, newValue)),
                    direction10.Subscribe(newValue => SetIO(10, newValue)),
                    direction11.Subscribe(newValue => SetIO(11, newValue))
                );
            });
        }
    }

    static class AnalogIO
    {
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
