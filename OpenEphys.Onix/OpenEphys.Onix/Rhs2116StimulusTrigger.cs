using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Rhs2116StimulusTrigger : Sink<bool>
    {
        [TypeConverter(typeof(Rhs2116Trigger.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                    return source.Do(t => device.WriteRegister(Rhs2116Trigger.TRIGGER, t ? 1u : 0u));
                }));
        }
    }
}
