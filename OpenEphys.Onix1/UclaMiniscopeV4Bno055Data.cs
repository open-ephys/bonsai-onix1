using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Bno055DataFrame"/>s from the Bno055 9-axis inertial measurement unit
    /// on a UCLA Miniscope V4.
    /// </summary>
    public class UclaMiniscopeV4Bno055Data : Source<Bno055DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(UclaMiniscopeV4Bno055.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame"/>s at approximately 100 Hz.
        /// </summary>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/>s.</returns>
        /// <remarks>
        /// This will generate a sequence of <see cref="Bno055DataFrame"/>s at approximately 100 Hz. This rate
        /// may be limited by the hardware I2C bus.
        /// </remarks>
        public override IObservable<Bno055DataFrame> Generate()
        {
            // Max of 100 Hz, but limited by I2C bus
            var source = Observable.Interval(TimeSpan.FromSeconds(0.01));
            return Generate(source);
        }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame"/>s.
        /// </summary>
        /// <param name="source">An input sequence that drives the production of <see
        /// cref="Bno055DataFrame"/>s</param>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/>s.</returns>
        /// <remarks>
        /// A <see cref="Bno055DataFrame"/> will be produced each time an element is received from the
        /// <paramref name="source"/> sequence. This rate is limited by the hardware I2C bus and has a
        /// maximum of 100 Hz.
        /// </remarks>
        public unsafe IObservable<Bno055DataFrame> Generate<TSource>(IObservable<TSource> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                return !((UclaMiniscopeV4Bno055DeviceInfo)deviceInfo).Enable
                    ? Observable.Empty<Bno055DataFrame>()
                    : Observable.Create<Bno055DataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(UclaMiniscopeV4Bno055));
                        var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                        var i2c = new I2CRegisterContext(passthrough, UclaMiniscopeV4Bno055.BNO055Address);

                        return source.SubscribeSafe(observer, _ =>
                        {
                            Bno055DataFrame frame = default;
                            device.Context.EnsureContext(() =>
                            {
                                var data = i2c.ReadBytes(UclaMiniscopeV4Bno055.DataAddress, sizeof(Bno055DataPayload));
                                ulong clock = passthrough.ReadRegister(DS90UB9x.LASTI2CL);
                                clock += (ulong)passthrough.ReadRegister(DS90UB9x.LASTI2CH) << 32;
                                fixed (byte* dataPtr = data)
                                {
                                    frame = new Bno055DataFrame(clock, (Bno055DataPayload*)dataPtr);
                                }
                            });

                            if (frame != null)
                            {
                                observer.OnNext(frame);
                            }
                        });
                    });
            });
        }
    }
}
