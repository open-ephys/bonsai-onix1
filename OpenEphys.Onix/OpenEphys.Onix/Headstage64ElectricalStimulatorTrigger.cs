using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Headstage64ElectricalStimulatorTrigger: Sink<bool>
    {
        readonly BehaviorSubject<bool> enable = new(false);

        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        public string DeviceName { get; set; }

        [Description("Specifies whether the electrical stimulation subcircuit will respect triggers.")]
        public bool Enable
        {
            get => enable.Value;
            set => enable.OnNext(value);
        }

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                Observable.Create<bool>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                        var triggerObserver = Observer.Create<bool>(
                            value => device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, value ? 1u : 0u),
                            observer.OnError,
                            observer.OnCompleted);
                        return new CompositeDisposable(
                            enable.Subscribe(value => device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, value ? 1u : 0u)),
                            source.SubscribeSafe(triggerObserver)
                        );
                    })));
        }
    }
}
