using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    public class ConfigureTest0 : SingleDeviceFactory
    {
        readonly BehaviorSubject<short> message = new(0);

        public ConfigureTest0()
            : base(typeof(Test0))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Test0 device is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(AcquisitionCategory)]
        [Description("Specifies the first 16-bit word that appears in the device to host frame.")]
        public short Message
        {
            get => message.Value;
            set => message.OnNext(value);
        }

        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("Indicates the number of 16-bit numbers, 0 to PayloadWords - 1, that follow Message in each frame.")]
        public uint DummyCount { get; private set; }

        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("Indicates the rate at which frames are produced in Hz. 0 indicates that the frame rate is unspecified (variable or upstream controlled).")]
        public uint FramesPerSecond { get; private set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Test0.ID);
                device.WriteRegister(Test0.ENABLE, Enable ? 1u : 0);
                FramesPerSecond = device.ReadRegister(Test0.FRAMERATE);
                DummyCount = device.ReadRegister(Test0.NUMTESTWORDS);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Test0
    {
        public const int ID = 10;

        public const uint ENABLE = 0x0;
        public const uint MESSAGE = 0x1;
        public const uint NUMTESTWORDS = 0x2;
        public const uint FRAMERATE = 0x3;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Test0))
            {
            }
        }
    }
}
