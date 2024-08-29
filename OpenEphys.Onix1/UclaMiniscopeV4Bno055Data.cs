using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    public class UclaMiniscopeV4Bno055Data : Source<Bno055DataFrame>
    {
        [TypeConverter(typeof(UclaMiniscopeV4Bno055.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<Bno055DataFrame> Generate()
        {
            // Max of 100 Hz, but limited by I2C bus
            var source = Observable.Interval(TimeSpan.FromSeconds(0.01));
            return Generate(source);
        }

        public unsafe IObservable<Bno055DataFrame> Generate<TSource>(IObservable<TSource> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                return !((NeuropixelsV2eBno055DeviceInfo)deviceInfo).Enable
                    ? Observable.Empty<Bno055DataFrame>()
                    : Observable.Create<Bno055DataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2eBno055));
                        var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                        var i2c = new I2CRegisterContext(passthrough, NeuropixelsV2eBno055.BNO055Address);

                        return source.SubscribeSafe(observer, _ =>
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
                        });
                    });
            });
        }
    }
}
