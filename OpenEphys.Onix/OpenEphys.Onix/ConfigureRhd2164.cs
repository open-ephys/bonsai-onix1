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
        public Rhd2164DspCutoff DspCutoff { get; set; }

        [Category(ConfigurationCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers.")]
        public Rhd2164AnalogLowCutoff AnalogLowCutoff { get; set; }

        [Category(ConfigurationCategory)]
        [Description("Specifies the upper cutoff frequency of the pre-ADC amplifiers.")]
        public Rhd2164AnalogHighCutoff AnalogHighCutoff { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // config register format following RHD2164 datasheet
                // https://intantech.com/files/Intan_RHD2000_series_datasheet.pdf
                var device = context.GetDevice(deviceAddress, Rhd2164.ID);

                var format = 0;
                var amplifierDataFormat = AmplifierDataFormat;
                format |= (int)amplifierDataFormat << 6;

                var dspCutoff = DspCutoff;
                if (dspCutoff != Rhd2164DspCutoff.Off)
                {
                    format |= 1 << 4;
                    format |= (int)dspCutoff;
                }

                var highCutoff = Rhd2164Config.AnalogHighCutoffToRegisters[AnalogHighCutoff];
                var lowCutoff = Rhd2164Config.AnalogLowCutoffToRegisters[AnalogLowCutoff];
                var bw0 = highCutoff[0] & 0b00111111;
                var bw1 = highCutoff[1] & 0b00011111;
                var bw2 = highCutoff[2] & 0b00111111;
                var bw3 = highCutoff[3] & 0b00011111;
                var bw4 = lowCutoff[0] & 0b01111111;
                var bw5 = (lowCutoff[2] << 6) & 0b01000000 | lowCutoff[1] & 0b00111111;
                context.WriteRegister(deviceAddress, Rhd2164.BW0, (uint)bw0);
                context.WriteRegister(deviceAddress, Rhd2164.BW1, (uint)bw1);
                context.WriteRegister(deviceAddress, Rhd2164.BW2, (uint)bw2);
                context.WriteRegister(deviceAddress, Rhd2164.BW3, (uint)bw3);
                context.WriteRegister(deviceAddress, Rhd2164.BW4, (uint)bw4);
                context.WriteRegister(deviceAddress, Rhd2164.BW5, (uint)bw5);
                context.WriteRegister(deviceAddress, Rhd2164.FORMAT, (uint)format);
                context.WriteRegister(deviceAddress, Rhd2164.ENABLE, enable ? 1u : 0);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return disposable;
            });
        }
    }

    static class Rhd2164
    {
        public const int ID = 3;

        public const uint ENABLE = 0x10000;  // Enable the heartbeat

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
        public const uint BW5 = 0x0e; // On-Chip Amplifier Bandwidth Select 5
        public const uint PWR0 = 0x0f; // Individual Amplifier Power 0
        public const uint PWR1 = 0x10; // Individual Amplifier Power 1
        public const uint PWR2 = 0x11; // Individual Amplifier Power 2
        public const uint PWR3 = 0x12; // Individual Amplifier Power 3
        public const uint PWR4 = 0x13; // Individual Amplifier Power 4
        public const uint PWR5 = 0x14; // Individual Amplifier Power 5
        public const uint PWR6 = 0x15; // Individual Amplifier Power 6
        public const uint PWR7 = 0x16; // Individual Amplifier Power 7

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhd2164))
            {
            }
        }
    }
}
