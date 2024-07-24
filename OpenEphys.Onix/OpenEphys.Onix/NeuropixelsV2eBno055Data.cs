using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Produces a sequence of <see cref="Bno055DataFrame"/> from a NeuropixelsV2e headstage.
    /// </summary>
    public class NeuropixelsV2eBno055Data : Source<Bno055DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV2eBno055.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame"/> objects at approximately 100 Hz.
        /// </summary>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/> objects.</returns>
        /// <remarks>
        /// If no input is given to the <see cref="NeuropixelsV1eBno055Data"/> object, then it will
        /// generate a sequence of <see cref="Bno055DataFrame"/> objects at approximately 100 Hz. This rate
        /// may be limited by the I2C bus.
        /// </remarks>
        public override IObservable<Bno055DataFrame> Generate()
        {
            // Max of 100 Hz, but limited by I2C bus
            var source = Observable.Interval(TimeSpan.FromSeconds(0.01));
            return Generate(source);
        }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/> objects.</returns>
        public unsafe IObservable<Bno055DataFrame> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(
                    deviceInfo => Observable.Create<Bno055DataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2eBno055));
                        var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                        var i2c = new I2CRegisterContext(passthrough, NeuropixelsV2eBno055.BNO055Address);

                        var pollingObserver = Observer.Create<TSource>(
                            _ =>
                            {
                                Bno055DataFrame frame = default;
                                device.Context.EnsureContext(() =>
                                {
                                    var data = i2c.ReadBytes(NeuropixelsV2eBno055.DataAddress, sizeof(Bno055DataPayload));
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
                            },
                            observer.OnError,
                            observer.OnCompleted);
                        return source.SubscribeSafe(pollingObserver);
                    })));
        }
    }
}
