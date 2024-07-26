using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class TS4231V1GeometricPositionData : Source<TS4231V1GeometricPositionDataFrame>
    {
        [TypeConverter(typeof(TS4231V1.NameConverter))]
        public string DeviceName { get; set; }

        public Point3d P { get; set; } = new(0, 0, 0);

        public Point3d Q { get; set; } = new(1, 0, 0);

        public unsafe override IObservable<TS4231V1GeometricPositionDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<TS4231V1GeometricPositionDataFrame>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(TS4231V1));
                    var pulseConverter = new TS4231V1GeometricPositionConverter(device.Hub.ClockHz, P, Q);

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
                }));
        }
    }
}
