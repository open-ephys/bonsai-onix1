using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Headstage64ElectricalStimulatorTrigger: Sink<bool>
    {
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                    return source.Do(t => device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, t ? 1u : 0u));
                }));
        }
    }
}
