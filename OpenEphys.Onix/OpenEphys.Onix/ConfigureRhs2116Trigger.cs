using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Subjects;

namespace OpenEphys.Onix
{
    [DefaultProperty(nameof(StimulusSequence))]
    public class ConfigureRhs2116Trigger : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhs2116StimulusSequenceDual> stimulusSequence = new(new Rhs2116StimulusSequenceDual());

        public ConfigureRhs2116Trigger()
            : base(typeof(Rhs2116Trigger))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHS2116 device is enabled.")]
        public Rhs2116TriggerSource TriggerSource { get; set; } = Rhs2116TriggerSource.Local;

        [Category("Acquisition")]
        [Description("Stimulus sequence.")]
        [Editor("OpenEphys.Onix.Design.Rhs2116StimulusSequenceEditor, OpenEphys.Onix.Design", typeof(UITypeEditor))]
        public Rhs2116StimulusSequenceDual StimulusSequence
        {
            get => stimulusSequence.Value;
            set => stimulusSequence.OnNext(value);
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var triggerSource = TriggerSource;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var channelConfiguration = new Rhs2116ProbeGroup();

            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Rhs2116Trigger.ID);

                device.WriteRegister(Rhs2116Trigger.TRIGGERSOURCE, (uint)triggerSource);

                var deviceInfo = new Rhs2116TriggerDeviceInfo(context, DeviceType, deviceAddress, channelConfiguration, stimulusSequence.Value);

                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }

    class Rhs2116TriggerDeviceInfo : DeviceInfo
    {
        public Rhs2116ProbeGroup ChannelConfiguration { get; }
        public Rhs2116StimulusSequenceDual StimulusSequence { get; }

        public Rhs2116TriggerDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress,
                                        Rhs2116ProbeGroup channelConfiguration, Rhs2116StimulusSequenceDual stimulusSequence)
            : base(context, deviceType, deviceAddress)
        {
            ChannelConfiguration = channelConfiguration;
            StimulusSequence = stimulusSequence;
        }
    }

    static class Rhs2116Trigger
    {
        public const int ID = 32;

        // managed registers
        public const uint ENABLE = 0; // Writes and reads to ENABLE are ignored without error
        public const uint TRIGGERSOURCE = 1; // The LSB is used to determine the trigger source
        public const uint TRIGGER = 2; // Writing 0x1 to this register will trigger a stimulation sequence if the TRIGGERSOURCE is set to 0.

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116Trigger))
            {
            }
        }
    }

    public enum Rhs2116TriggerSource
    {
        [Description("Respect local triggers (e.g. via GPIO or TRIGGER register) and broadcast via sync pin. ")]
        Local = 0,
        [Description("Receiver. Only respect triggers received from sync pin")]
        External = 1,
    }
}
