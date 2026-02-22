using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a persistent heartbeat device whose data stream cannot be disabled.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="HeartbeatData"/>,
    /// using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a heartbeat device.")]
    public class ConfigurePersistentHeartbeat : SingleDeviceFactory
    {
        readonly BehaviorSubject<uint> beatsPerSecond = new(100);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurePersistentHeartbeat"/> class.
        /// </summary>
        public ConfigurePersistentHeartbeat()
            : base(typeof(PersistentHeartbeat))
        {
        }

        /// <summary>
        /// Gets or sets the rate at which beats are produced in Hz.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Rate at which beats are produced (Hz).")]
        public uint BeatsPerSecond
        {
            get => beatsPerSecond.Value;
            set => beatsPerSecond.OnNext(value);
        }

        /// <summary>
        /// Configures a persistent heartbeat device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a persistent heartbeat device./></returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            //var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureAndLatchDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                var subscription = beatsPerSecond.SubscribeSafe(observer, newValue =>
                {
                    var clkHz = device.ReadRegister(PersistentHeartbeat.CLK_HZ);
                    var minHeartbeatHz = device.ReadRegister(PersistentHeartbeat.MIN_HB_HZ);

                    if (newValue < minHeartbeatHz || newValue > 10e6)
                    {
                        throw new InvalidOperationException($"Value must be between {minHeartbeatHz} Hz and 10 MHz.");
                    }

                    device.WriteRegister(PersistentHeartbeat.CLK_DIV, clkHz / newValue);
                });

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    [EquivalentDataSource(typeof(Heartbeat))]
    static class PersistentHeartbeat
    {
        public const int ID = 35;
        public const uint MinimumVersion = 1;

        public const uint ENABLE = 0; // Heartbeat enable state (read only; always enabled for this device).
        public const uint CLK_DIV = 1; // Heartbeat clock divider ratio. Minimum value is CLK_HZ / 10e6.
                                       // Maximum value is CLK_HZ / MIN_HB_HZ.Attempting to set to a value outside
                                       // this range will result in error.
        public const uint CLK_HZ = 2; // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV
        public const uint MIN_HB_HZ = 3; // The minimum allowed beat frequency, in Hz, for this device

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(PersistentHeartbeat))
            {
            }
        }
    }
}
