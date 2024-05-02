using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using Bonsai;

namespace OpenEphys.Onix
{
    [DefaultProperty(nameof(StimulusSequence))]
    public class Rhs2116StimulusTrigger : Sink<bool>
    {
        readonly BehaviorSubject<Rhs2116StimulusSequence> stimulusSequence = new(new Rhs2116StimulusSequence(true));

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
                    Observable.Create<bool>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                        var triggerObserver = Observer.Create<bool>(
                            value =>
                            {
                                device.WriteRegister(Rhs2116Trigger.TRIGGER, value ? 1u : 0u);
                                observer.OnNext(value);
                            },
                            observer.OnError,
                            observer.OnCompleted);

                        var hubAddress = GenericHelper.GetHubAddressFromDeviceAddress(deviceInfo.DeviceAddress);

                        var rhsADeviceAddress = HeadstageRhs2116.GetRhs2116ADeviceAddress(hubAddress);
                        var rhsBDeviceAddress = HeadstageRhs2116.GetRhs2116BDeviceAddress(hubAddress);

                        var deviceRhsA = deviceInfo.Context.GetDeviceContext(rhsADeviceAddress, Rhs2116.ID);
                        var deviceRhsB = deviceInfo.Context.GetDeviceContext(rhsBDeviceAddress, Rhs2116.ID);
                      
                        return new CompositeDisposable(
                            stimulusSequence.Subscribe(value =>
                            {
                                var reg = Rhs2116Config.StimulatorStepSizeToRegisters[value.CurrentStepSize];
                                deviceRhsA.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);
                                deviceRhsB.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);

                                var a = value.AnodicAmplitudes;
                                for (int i = 0; i < Rhs2116StimulusSequence.NumStimuliPerDevice; i++)
                                {
                                    deviceRhsA.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                                    deviceRhsB.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i + Rhs2116StimulusSequence.NumStimuliPerDevice));
                                }

                                var c = value.CathodicAmplitudes;
                                for (int i = 0; i < Rhs2116StimulusSequence.NumStimuliPerDevice; i++)
                                {
                                    deviceRhsA.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                                    deviceRhsB.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i + Rhs2116StimulusSequence.NumStimuliPerDevice));
                                }

                                var dt = value.DeltaTable;
                                var dtB = value.DeltaTableB;

                                deviceRhsA.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);
                                deviceRhsB.WriteRegister(Rhs2116.NUMDELTAS, (uint)dtB.Count);

                                uint j = 0;
                                foreach (var d in dt)
                                {
                                    uint indexAndTime = j++ << 22 | (d.Key & 0x003FFFFF);

                                    deviceRhsA.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                    deviceRhsA.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
                                }

                                j = 0;
                                foreach (var d in dtB)
                                {
                                    uint indexAndTime = j++ << 22 | (d.Key & 0x003FFFFF);

                                    deviceRhsB.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                    deviceRhsB.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
                                }
                            }),
                            source.SubscribeSafe(triggerObserver));
                    })));
        }
    }
}
