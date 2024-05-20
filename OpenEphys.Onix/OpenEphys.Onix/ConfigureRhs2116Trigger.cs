using System;
using System.ComponentModel;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix
{
    public class ConfigureRhs2116Trigger : SingleDeviceFactory
    {
        public ConfigureRhs2116Trigger()
            : base(typeof(Rhs2116Trigger))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHS2116 device is enabled.")]
        public Rhs2116TriggerSource TriggerSource { get; set; } = Rhs2116TriggerSource.Local;

        [Category(ConfigurationCategory)]
        [Description("Probe Interface group to define channel mapping")]
        public ProbeGroup ChannelConfiguration { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var triggerSource = TriggerSource;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;

            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Rhs2116Trigger.ID);

                device.WriteRegister(Rhs2116Trigger.TRIGGERSOURCE, (uint)triggerSource);

                var deviceInfo = new Rhs2116TriggerDeviceInfo(context, DeviceType, deviceAddress, ChannelConfiguration);

                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }

    class Rhs2116TriggerDeviceInfo : DeviceInfo
    {
        public ProbeGroup ChannelConfiguration { get; }

        public Rhs2116TriggerDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, ProbeGroup channelConfiguration)
            : base(context, deviceType, deviceAddress)
        {
            ChannelConfiguration = channelConfiguration;
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
