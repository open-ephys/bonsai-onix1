using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class TS4231Data : Source<TS4231DataFrame>
    {
        [TypeConverter(typeof(TS4231.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<TS4231DataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(TS4231));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new TS4231DataFrame(frame));
                }));
        }
    }
}
