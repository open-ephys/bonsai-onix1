using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Rhd2164Data : Source<Rhd2164DataFrame>
    {
        [TypeConverter(typeof(Rhd2164.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<Rhd2164DataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDevice(typeof(Rhd2164));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new Rhd2164DataFrame(frame));
                }));
        }
    }
}
