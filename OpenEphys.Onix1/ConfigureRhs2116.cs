using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace OpenEphys.Onix1
{
    public class ConfigureRhs2116 : SingleDeviceFactory
    {

        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoff = new(Rhs2116AnalogLowCutoff.Low100mHz);
        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoffRecovery = new(Rhs2116AnalogLowCutoff.Low250Hz);
        readonly BehaviorSubject<Rhs2116AnalogHighCutoff> analogHighCutoff = new(Rhs2116AnalogHighCutoff.High10000Hz);
        readonly BehaviorSubject<bool> respectExternalActiveStim = new(true);
        readonly BehaviorSubject<Rhs2116StimulusSequence> stimulusSequence = new(new Rhs2116StimulusSequence());

        public ConfigureRhs2116()
            : base(typeof(Rhs2116))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHS2116 device is enabled.")]
        public bool Enable { get; set; } = true;

        //[Category(ConfigurationCategory)]
        //[Description("Specifies the raw ADC output format used for amplifier conversions.")]
        //public Rhs2116AmplifierDataFormat AmplifierDataFormat { get; set; }

        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the DSP high-pass filter used for amplifier offset removal.")]
        public Rhs2116DspCutoff DspCutoff { get; set; } = Rhs2116DspCutoff.Off;

        [Category(AcquisitionCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoff
        {
            get => analogLowCutoff.Value;
            set => analogLowCutoff.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers during stimulus recovery.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoffRecovery
        {
            get => analogLowCutoffRecovery.Value;
            set => analogLowCutoffRecovery.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Specifies the upper cutoff frequency of the pre-ADC amplifiers.")]
        public Rhs2116AnalogHighCutoff AnalogHighCutoff
        {
            get => analogHighCutoff.Value;
            set => analogHighCutoff.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("If true, this device will apply AnalogLowCutoffRecovery " +
            "if stimulation occurs via any RHS chip the same headstage or others that are connected" +
            "using StimActive pin. If false, this device will apply AnalogLowCutoffRecovery during its" +
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
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // config register format following RHS2116 datasheet
                // https://www.intantech.com/files/Intan_RHS2116_datasheet.pdf
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                var format = device.ReadRegister(Rhs2116.FORMAT);
                var dspCutoff = DspCutoff;
                if (dspCutoff == Rhs2116DspCutoff.Off)
                {
                    format &= ~(1u << 4);
                }
                else
                {
                    format |= 1 << 4;
                    format &= ~0xFu;
                    format |= (uint)dspCutoff;
                }

                device.WriteRegister(Rhs2116.FORMAT, format); // NB: DC data only provided in unsigned. Force amplifier data to use unsigned for consistency
                device.WriteRegister(Rhs2116.ENABLE, enable ? 1u : 0);

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType),
                    analogLowCutoff.Subscribe(newValue =>
                    {
                        var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                        var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                        device.WriteRegister(Rhs2116.BW2, reg);
                    }),
                    analogLowCutoffRecovery.Subscribe(newValue =>
                    {
                        var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                        var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                        device.WriteRegister(Rhs2116.BW3, reg);
                    }),
                    analogHighCutoff.Subscribe(newValue =>
                    {
                        var regs = Rhs2116Config.AnalogHighCutoffToRegisters[newValue];
                        device.WriteRegister(Rhs2116.BW0, regs[1] << 6 | regs[0]);
                        device.WriteRegister(Rhs2116.BW1, regs[3] << 6 | regs[2]);
                        device.WriteRegister(Rhs2116.FASTSETTLESAMPLES, Rhs2116Config.AnalogHighCutoffToFastSettleSamples[newValue]);
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

    static class Rhs2116
    {
        public const int ID = 31;

        // constants
        public const int AmplifierChannelCount = 16;
        public const int StimMemorySlotsAvailable = 1024;
        public const double SampleFrequencyHz = 30.1932367151e3;

        // managed registers
        public const uint ENABLE = 0x8000; // Enable or disable the data output stream (32767)
        public const uint MAXDELTAS = 0x8001; // Maximum number of deltas in the delta table (32769)
        public const uint NUMDELTAS = 0x8002; // Number of deltas in the delta table (32770)
        public const uint DELTAIDXTIME = 0x8003; // The delta table index and corresponding application delta application time (32771)
        public const uint DELTAPOLEN = 0x8004; // The polarity and enable vectors  (32772)
        public const uint SEQERROR = 0x8005; // Invalid sequence indicator (32773)
        public const uint TRIGGER = 0x8006;  // Writing 1 to this register will trigger a stimulation sequence for this device (32774)
        public const uint FASTSETTLESAMPLES = 0x8007; // Number of round-robbin samples to apply charge balance following the conclusion of a stimulus pulse (32775)
        public const uint RESPECTSTIMACTIVE = 0x8008; // Determines when artifact recovery sequence is applied to this chip (32776)

        // unmanaged registers
        public const uint BIAS = 0x00; // Supply Sensor and ADC Buffer Bias Current
        public const uint FORMAT = 0x01; // ADC Output Format, DSP Offset Removal, and Auxiliary Digital Outputs
        public const uint ZCHECK = 0x02; // Impedance Check Control
        public const uint DAC = 0x03; // Impedance Check DAC
        public const uint BW0 = 0x04; // On-Chip Amplifier Bandwidth Select
        public const uint BW1 = 0x05; // On-Chip Amplifier Bandwidth Select
        public const uint BW2 = 0x06; // On-Chip Amplifier Bandwidth Select
        public const uint BW3 = 0x07; // On-Chip Amplifier Bandwidth Select
        public const uint PWR = 0x08; //Individual AC Amplifier Power

        public const uint SETTLE = 0x0a; // Amplifier Fast Settle

        public const uint LOWAB = 0x0c; // Amplifier Lower Cutoff Frequency Select

        public const uint STIMENA = 0x20; // Stimulation Enable A
        public const uint STIMENB = 0x21; // Stimulation Enable B
        public const uint STEPSZ = 0x22; // Stimulation Current Step Size
        public const uint STIMBIAS = 0x23; // Stimulation Bias Voltages
        public const uint RECVOLT = 0x24; // Current-Limited Charge Recovery Target Voltage
        public const uint RECCUR = 0x25; // Charge Recovery Current Limit
        public const uint DCPWR = 0x26; // Individual DC Amplifier Power

        public const uint COMPMON = 0x28; // Compliance Monitor

        public const uint STIMON = 0x2a; // Stimulator On

        public const uint STIMPOL = 0x2c; // Stimulator Polarity

        public const uint RECOV = 0x2e; // Charge Recovery Switch

        public const uint LIMREC = 0x30; // Current-Limited Charge Recovery Enable

        public const uint FAULTDET = 0x32; // Fault Current Detector

        public const uint NEG00 = 0x40;
        public const uint NEG01 = 0x41;
        public const uint NEG02 = 0x42;
        public const uint NEG03 = 0x43;
        public const uint NEG04 = 0x44;
        public const uint NEG05 = 0x45;
        public const uint NEG06 = 0x46;
        public const uint NEG07 = 0x47;
        public const uint NEG08 = 0x48;
        public const uint NEG09 = 0x49;
        public const uint NEG010 = 0x4a;
        public const uint NEG011 = 0x4b;
        public const uint NEG012 = 0x4c;
        public const uint NEG013 = 0x4d;
        public const uint NEG014 = 0x4e;
        public const uint NEG015 = 0x4f;

        public const uint POS00 = 0x60;
        public const uint POS01 = 0x61;
        public const uint POS02 = 0x62;
        public const uint POS03 = 0x63;
        public const uint POS04 = 0x64;
        public const uint POS05 = 0x65;
        public const uint POS06 = 0x66;
        public const uint POS07 = 0x67;
        public const uint POS08 = 0x68;
        public const uint POS09 = 0x69;
        public const uint POS010 = 0x6a;
        public const uint POS011 = 0x6b;
        public const uint POS012 = 0x6c;
        public const uint POS013 = 0x6d;
        public const uint POS014 = 0x6e;
        public const uint POS015 = 0x6f;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116))
            {
            }
        }
    }
}
