using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using Bonsai;
using System.Reactive.Disposables;

namespace OpenEphys.Onix
{
    [DefaultProperty(nameof(StimulusSequence))]
    public class Rhs2116StimulusTrigger : Sink<bool>
    {
        readonly BehaviorSubject<Rhs2116StimulusSequence> stimulusSequence = new(new Rhs2116StimulusSequence());

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

        private uint deviceAddress;

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            var deviceDisposable = DeviceManager.ReserveDevice(DeviceName);
            
            return Observable.Using(
                () => 
                {
                  deviceDisposable.Subject.SelectMany(deviceInfo =>
                  {
                      deviceAddress = deviceInfo.DeviceAddress;

                      var baseaddress = (deviceAddress & 0xFF00u) >> 8;

                      var rhsADeviceAddress = HeadstageRhs2116.GetRhs2116ADeviceAddress(baseaddress);
                      var rhsBDeviceAddress = HeadstageRhs2116.GetRhs2116BDeviceAddress(baseaddress);

                      var deviceRhsA = deviceInfo.Context.GetDeviceContext(rhsADeviceAddress, Rhs2116.ID);
                      var deviceRhsB = deviceInfo.Context.GetDeviceContext(rhsBDeviceAddress, Rhs2116.ID);
                      
                      return new CompositeDisposable(
                          deviceDisposable,
                          stimulusSequence.Subscribe(newValue =>
                          {
                              // Step size
                              var reg = Rhs2116Config.StimulatorStepSizeToRegisters[newValue.CurrentStepSize];
                              deviceRhsA.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);
                              deviceRhsB.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);

                              // Anodic amplitudes
                              var a = newValue.AnodicAmplitudes;
                              for (int i = 0; i < a.Count(); i++)
                              {
                                  deviceRhsA.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                                  deviceRhsB.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                              }

                              // Cathodic amplitudes
                              var c = newValue.CathodicAmplitudes;
                              for (int i = 0; i < a.Count(); i++)
                              {
                                  deviceRhsA.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                                  deviceRhsB.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                              }

                              // Create delta table and set length
                              var dt = newValue.DeltaTable;
                              deviceRhsA.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);
                              deviceRhsB.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);

                              uint j = 0;
                              foreach (var d in dt)
                              {
                                  uint indexAndTime = j++ << 22 | (d.Key & 0x003FFFFF);

                                  deviceRhsA.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                  deviceRhsA.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);

                                  deviceRhsB.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                  deviceRhsB.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
                              }
                          })
                        );
                    });

                    return new CompositeDisposable();
                },
                disposable => deviceDisposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                    return source.Do(t => device.WriteRegister(Rhs2116Trigger.TRIGGER, t ? 1u : 0u));
                }));
        }
    }
}
