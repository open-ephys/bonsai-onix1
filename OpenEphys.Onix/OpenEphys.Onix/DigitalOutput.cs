using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class DigitalOutput : Sink<DigitalPortState>
    {
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<DigitalPortState> Process(IObservable<DigitalPortState> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                    return source.Do(device.Write);
                }));
        }
    }
}
