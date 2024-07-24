using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Ts4231Data : Source<Ts4231DataFrame>
    {
        [TypeConverter(typeof(TS4231.NameConverter))]
        public string DeviceName { get; set; }

        public Point3d P { get; set; } = new(0, 0, 0);

        public Point3d Q { get; set; } = new(1, 0, 0);

        public unsafe override IObservable<Ts4231DataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<Ts4231DataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(TS4231));
                        var pulseConverter = new Ts4231PulseConverter(device.Hub.ClockHz, P, Q);
                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var position = pulseConverter.Convert(frame);
                                if (position != null)
                                {
                                    observer.OnNext(position);
                                }
                            },
                            observer.OnError,
                            observer.OnCompleted);
                        return deviceInfo.Context.FrameReceived
                            .Where(frame => frame.DeviceAddress == device.Address)
                            .SubscribeSafe(frameObserver);
                    })));
        }
    }
}
