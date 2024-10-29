using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a heartbeat device.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="HeartbeatData"/>,
    /// using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a heartbeat device.")]
    public class ConfigureHeartbeat : SingleDeviceFactory
    {
        readonly BehaviorSubject<uint> beatsPerSecond = new(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeartbeat"/> class.
        /// </summary>
        public ConfigureHeartbeat()
            : base(typeof(Heartbeat))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="HeartbeatData"/> instance that is linked to this configuration will produce data.
        /// If set to false, it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the heartbeat device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the rate at which beats are produced in Hz.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="HeartbeatData"/> instance that is linked to this configuration will produce data.
        /// If set to false, it will not produce data.
        /// </remarks>
        [Range(1, 10e6)]
        [Category(AcquisitionCategory)]
        [Description("Rate at which beats are produced (Hz).")]
        public uint BeatsPerSecond
        {
            get => beatsPerSecond.Value;
            set => beatsPerSecond.OnNext(value);
        }

        /// <summary>
        /// Configures a heartbeat device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a heartbeat device./></returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Heartbeat.ENABLE, enable ? 1u : 0u);
                var subscription = beatsPerSecond.SubscribeSafe(observer, newValue =>
                {
                    var clkHz = device.ReadRegister(Heartbeat.CLK_HZ);
                    device.WriteRegister(Heartbeat.CLK_DIV, clkHz / newValue);
                });

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType),
                    subscription
                );
            });
        }
    }

    static class Heartbeat
    {
        public const int ID = 12;

        public const uint ENABLE = 0;  // Enable the heartbeat
        public const uint CLK_DIV = 1;  // Heartbeat clock divider ratio. Default results in 10 Hz heartbeat. Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
        public const uint CLK_HZ = 2; // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Heartbeat))
            {
            }
        }
    }

    // NB: Can be used to remove Enable and BeatsPerSecond properties from MultiDeviceFactories that
    // include a Heartbeat when having those options would cause confusion
    internal class HeartbeatSingleDeviceFactoryConverter : SingleDeviceFactoryConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = (from property in base.GetProperties(context, value, attributes).Cast<PropertyDescriptor>()
                              where !property.IsReadOnly &&
                                    !(property.DisplayName == "Enable") &&
                                    !(property.DisplayName == "BeatsPerSecond") &&
                                    property.ComponentType != typeof(SingleDeviceFactory)
                              select property)
                              .ToArray();
            return new PropertyDescriptorCollection(properties).Sort(properties.Select(p => p.Name).ToArray());
        }
    }
}
