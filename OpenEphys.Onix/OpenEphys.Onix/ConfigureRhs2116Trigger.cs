using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Linq;

namespace OpenEphys.Onix
{
    public class ConfigureRhs2116Trigger : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoffRecovery = new(Rhs2116AnalogLowCutoff.Low250Hz);
        readonly BehaviorSubject<bool> respectExternalActiveStim = new(true);
        readonly BehaviorSubject<Rhs2116StimulusSequence> stimulusSequence = new(new Rhs2116StimulusSequence());

        public ConfigureRhs2116Trigger()
            : base(typeof(Rhs2116Trigger))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHS2116 device is enabled.")]
        public Rhs2116TriggerSource TriggerSource { get; set; } = Rhs2116TriggerSource.Local;

        [Category(AcquisitionCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers during stimulus recovery.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoffRecovery
        {
            get => analogLowCutoffRecovery.Value;
            set => analogLowCutoffRecovery.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("If true, this device will apply AnalogLowCutoffRecovery " +
            "if stimulation occurs via any RHS chip the same headstage or others that are connected " +
            "using StimActive pin. If false, this device will apply AnalogLowCutoffRecovery during its " +
            "own stimuli.")]
        public bool RespectExternalActiveStim
        {
            get => respectExternalActiveStim.Value;
            set => respectExternalActiveStim.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Stimulus sequence.")]
        public Rhs2116StimulusSequence StimulusSequence
        {
            get => stimulusSequence.Value;
            set => stimulusSequence.OnNext(value);
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var triggerSource = TriggerSource;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Rhs2116Trigger.ID);
                device.WriteRegister(Rhs2116Trigger.TRIGGERSOURCE, (uint)triggerSource);
                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType),
                    analogLowCutoffRecovery.Subscribe(newValue =>
                    {
                        var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                        var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                    }),
                    stimulusSequence.Subscribe(newValue =>
                    {
                        // Step size
                        var reg = Rhs2116Config.StimulatorStepSizeToRegisters[newValue.CurrentStepSize];
                        device.WriteRegister(Rhs2116.STEPSZ, reg[2] << 13 | reg[1] << 7 | reg[0]);

                        // Anodic amplitudes
                        // TODO: cache last write and compare?
                        var a = newValue.AnodicAmplitudes;
                        for (int i = 0; i < a.Count(); i++)
                        {
                            device.WriteRegister(Rhs2116.POS00 + (uint)i, a.ElementAt(i));
                        }

                        // Cathodic amplitudes
                        // TODO: cache last write and compare?
                        var c = newValue.CathodicAmplitudes;
                        for (int i = 0; i < a.Count(); i++)
                        {
                            device.WriteRegister(Rhs2116.NEG00 + (uint)i, c.ElementAt(i));
                        }

                        // Create delta table and set length
                        var dt = newValue.DeltaTable;
                        device.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);

                        // TODO: If we want to do this efficently, we probably need a different data structure on the
                        // FPGA ram that allows columns to be out of order (e.g. linked list)
                        uint j = 0;
                        foreach (var d in dt)
                        {
                            uint indexAndTime = j++ << 22 | (d.Key & 0x003FFFFF);
                            device.WriteRegister(Rhs2116.DELTAIDXTIME, indexAndTime);
                            device.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
                        }
                    })
                );
            });
        }
    }

    static class Rhs2116Trigger
    {
        public const int ID = (int)DeviceID.Rhs2116Trigger;

        // managed registers
        public const uint ENABLE = 0; // Writes and reads to ENABLE are ignored without error
        public const uint TRIGGERSOURCE = 1; // The LSB is used to determine the trigger source
        public const uint TRIGGER = 2; // Writing 0x1 to this register will trigger a stimulation sequence if the TRIGGERSOURCE is set to 0.

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116Trigger))
            {
            }
        }
    }

    public enum Rhs2116TriggerSource
    {
        [Description("Respect local triggers (e.g. via GPIO or TRIGGER register) and broadcast via sync pin. ")]
        Local = 0,
        [Description("Receiver. Only respect triggers received from sync pin")]
        External = 1,
    }
}
