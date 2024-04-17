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
        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoffRecovery = new(Rhs2116AnalogLowCutoff.Low250Hz);
        readonly BehaviorSubject<bool> respectExternalActiveStim = new(true);

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

        [Category("Acquisition")]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers during stimulus recovery.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoffRecovery
        {
            get => analogLowCutoffRecovery.Value;
            set => analogLowCutoffRecovery.OnNext(value);
        }
        
        [Category("Acquisition")]
        [Description("If true, this device will apply AnalogLowCutoffRecovery " +
            "if stimulation occurs via any RHS chip the same headstage or others that are connected " +
            "using StimActive pin. If false, this device will apply AnalogLowCutoffRecovery during its " +
            "own stimuli.")]
        public bool RespectExternalActiveStim
        {
            get => respectExternalActiveStim.Value;
            set => respectExternalActiveStim.OnNext(value);
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
                      var device_rhsA = deviceInfo.Context.GetDeviceContext(deviceAddress - 2, (int)DeviceID.Rhs2116);
                      var device_rhsB = deviceInfo.Context.GetDeviceContext(deviceAddress - 1, (int)DeviceID.Rhs2116);
                      
                      return new CompositeDisposable(
                          deviceDisposable,
                          analogLowCutoffRecovery.Subscribe(newValue =>
                          {
                              var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                              var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                              device_rhsA.WriteRegister(Rhs2116.BW3, reg);
                              device_rhsB.WriteRegister(Rhs2116.BW3, reg);
                          }),
                          respectExternalActiveStim.Subscribe(newValue =>
                          {
                            device_rhsA.WriteRegister(Rhs2116.RESPECTSTIMACTIVE, newValue ? 1u : 0u);
                          }),
                          stimulusSequence.Subscribe(newValue =>
                          {
                              // Step size
                              var reg = Rhs2116Config.StimulatorStepSizeToRegisters[newValue.CurrentStepSize];
                              device_rhsA.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);
                              device_rhsB.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);

                              // Anodic amplitudes
                              // TODO: cache last write and compare?
                              var a = newValue.AnodicAmplitudes;
                              for (int i = 0; i < a.Count(); i++)
                              {
                                  device_rhsA.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                                  device_rhsB.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                              }

                              // Cathodic amplitudes
                              // TODO: cache last write and compare?
                              var c = newValue.CathodicAmplitudes;
                              for (int i = 0; i < a.Count(); i++)
                              {
                                  device_rhsA.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                                  device_rhsB.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                              }

                              // Create delta table and set length
                              var dt = newValue.DeltaTable;
                              device_rhsA.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);
                              device_rhsB.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);

                              // TODO: If we want to do this efficently, we probably need a different data structure on the
                              // FPGA ram that allows columns to be out of order (e.g. linked list)
                              uint j = 0;
                              foreach (var d in dt)
                              {
                                  uint indexAndTime = j++ << 22 | (d.Key & 0x003FFFFF);

                                  device_rhsA.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                  device_rhsA.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);

                                  device_rhsB.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                                  device_rhsB.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
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
