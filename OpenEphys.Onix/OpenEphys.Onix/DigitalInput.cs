using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class DigitalInput : Source<DigitalInputDataFrame>
    {
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        public unsafe override IObservable<DigitalInputDataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<DigitalInputDataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                        var frameObserver = Observer.Create<oni.Frame>(
                            frame => new DigitalInputDataFrame(frame),
                            observer.OnError,
                            observer.OnCompleted);
                        return deviceInfo.Context.FrameReceived
                            .Where(frame => frame.DeviceAddress == device.Address)
                            .SubscribeSafe(frameObserver);
                    })));
        }
    }
}
