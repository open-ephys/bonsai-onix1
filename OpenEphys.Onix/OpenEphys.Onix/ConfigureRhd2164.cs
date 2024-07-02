using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureRhd2164 : SingleDeviceFactory
    {
        public ConfigureRhd2164()
            : base(typeof(Rhd2164))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHD2164 device is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Specifies the raw ADC output format used for amplifier conversions.")]
        public Rhd2164AmplifierDataFormat AmplifierDataFormat { get; set; }

        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the DSP high-pass filter used for amplifier offset removal.")]
        public Rhd2164DspCutoff DspCutoff { get; set; } = Rhd2164DspCutoff.Dsp146mHz;

        [Category(ConfigurationCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers.")]
        public Rhd2164AnalogLowCutoff AnalogLowCutoff { get; set; } = Rhd2164AnalogLowCutoff.Low100mHz;

        [Category(ConfigurationCategory)]
        [Description("Specifies the upper cutoff frequency of the pre-ADC amplifiers.")]
        public Rhd2164AnalogHighCutoff AnalogHighCutoff { get; set; } = Rhd2164AnalogHighCutoff.High10000Hz;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // config register format following RHD2164 datasheet
                // https://intantech.com/files/Intan_RHD2000_series_datasheet.pdf
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                var format = device.ReadRegister(Rhd2164.FORMAT);
                var amplifierDataFormat = AmplifierDataFormat;
                format &= ~(1u << 6);
                format |= (uint)amplifierDataFormat << 6;

                var dspCutoff = DspCutoff;
                if (dspCutoff == Rhd2164DspCutoff.Off)
                {
                    format &= ~(1u << 4);
                }
                else
                {
                    format |= 1 << 4;
                    format &= ~0xFu;
                    format |= (uint)dspCutoff;
                }

                var highCutoff = Rhd2164Config.AnalogHighCutoffToRegisters[AnalogHighCutoff];
                var lowCutoff = Rhd2164Config.AnalogLowCutoffToRegisters[AnalogLowCutoff];
                var bw0 = device.ReadRegister(Rhd2164.BW0);
                var bw1 = device.ReadRegister(Rhd2164.BW1);
                var bw2 = device.ReadRegister(Rhd2164.BW2);
                var bw3 = device.ReadRegister(Rhd2164.BW3);
                var bw4 = device.ReadRegister(Rhd2164.BW4);
                var bw5 = device.ReadRegister(Rhd2164.BW5);
                bw0 = BitHelper.Replace(bw0, 0b00111111, (uint)highCutoff[0]);
                bw1 = BitHelper.Replace(bw1, 0b00011111, (uint)highCutoff[1]);
                bw2 = BitHelper.Replace(bw2, 0b00111111, (uint)highCutoff[2]);
                bw3 = BitHelper.Replace(bw3, 0b00011111, (uint)highCutoff[3]);
                bw4 = BitHelper.Replace(bw4, 0b01111111, (uint)lowCutoff[0]);
                bw5 = BitHelper.Replace(bw5, 0b01111111, ((uint)lowCutoff[2] << 6) & 0b01000000 |
                                                          (uint)lowCutoff[1] & 0b00111111);
                device.WriteRegister(Rhd2164.BW0, bw0);
                device.WriteRegister(Rhd2164.BW1, bw1);
                device.WriteRegister(Rhd2164.BW2, bw2);
                device.WriteRegister(Rhd2164.BW3, bw3);
                device.WriteRegister(Rhd2164.BW4, bw4);
                device.WriteRegister(Rhd2164.BW5, bw5);
                device.WriteRegister(Rhd2164.FORMAT, format);
                device.WriteRegister(Rhd2164.ENABLE, enable ? 1u : 0);

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Rhd2164
    {
        public const int ID = 3;

        // constants
        public const int AmplifierChannelCount = 64;
        public const int AuxChannelCount = 3;

        // managed registers
        public const uint ENABLE = 0x8000; // Enable or disable the data output stream

        // unmanaged registers
        public const uint ADCCONF = 0x00; // ADC Configuration and Amplifier Fast Settle
        public const uint ADCBUFF = 0x01; // Supply Sensor and ADC Buffer Bias Current
        public const uint MUXBIAS = 0x02; // MUX Bias Current
        public const uint MUXLOAD = 0x03; // MUX Load, Temperature Sensor, and Auxiliary Digital Output
        public const uint FORMAT = 0x04; // ADC Output Format and DSP Offset Removal
        public const uint ZCHECK = 0x05; // Impedance Check Control
        public const uint ZDAC = 0x06; // Impedance Check DAC
        public const uint ZSELECT = 0x07; // Impedance Check Amplifier Select
        public const uint BW0 = 0x08; // On-Chip Amplifier Bandwidth Select 0
        public const uint BW1 = 0x09; // On-Chip Amplifier Bandwidth Select 1
        public const uint BW2 = 0x0a; // On-Chip Amplifier Bandwidth Select 2
        public const uint BW3 = 0x0b; // On-Chip Amplifier Bandwidth Select 3
        public const uint BW4 = 0x0c; // On-Chip Amplifier Bandwidth Select 4
        public const uint BW5 = 0x0d; // On-Chip Amplifier Bandwidth Select 5
        public const uint PWR0 = 0x0e; // Individual Amplifier Power 0
        public const uint PWR1 = 0x0f; // Individual Amplifier Power 1
        public const uint PWR2 = 0x10; // Individual Amplifier Power 2
        public const uint PWR3 = 0x11; // Individual Amplifier Power 3
        public const uint PWR4 = 0x12; // Individual Amplifier Power 4
        public const uint PWR5 = 0x13; // Individual Amplifier Power 5
        public const uint PWR6 = 0x14; // Individual Amplifier Power 6
        public const uint PWR7 = 0x15; // Individual Amplifier Power 7

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhd2164))
            {
            }
        }
    }
}
