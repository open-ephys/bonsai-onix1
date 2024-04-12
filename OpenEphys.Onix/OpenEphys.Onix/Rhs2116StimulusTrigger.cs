using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using Bonsai;

namespace OpenEphys.Onix
{
    [DefaultProperty(nameof(StimulusSequence))]
    public class Rhs2116StimulusTrigger : Sink<bool>
    {
        public readonly BehaviorSubject<Rhs2116StimulusSequence> stimulusSequence = new(new Rhs2116StimulusSequence());

        [TypeConverter(typeof(Rhs2116Trigger.NameConverter))]
        public string DeviceName { get; set; }

        [Category("Acquisition")]
        [Description("Stimulus sequence.")]
        [Editor("OpenEphys.Onix.Design.Rhs2116StimulusSequenceEditor, OpenEphys.Onix.Design", typeof(UITypeEditor))]
        public Rhs2116StimulusSequence StimulusSequence
        {
            get => stimulusSequence.Value;
            set => stimulusSequence.OnNext(value);
        }

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
