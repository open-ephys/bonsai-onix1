using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1
{
    static class Rhd2000
    {
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
        public const uint PWR4 = 0x12; // Individual Amplifier Power 4 (Rhd2164 only)
        public const uint PWR5 = 0x13; // Individual Amplifier Power 5 (Rhd2164 only)
        public const uint PWR6 = 0x14; // Individual Amplifier Power 6 (Rhd2164 only)
        public const uint PWR7 = 0x15; // Individual Amplifier Power 7 (Rhd2164 only)

        public const uint DIEREV = 0x3C; // Read-only die revision
        public const uint AMPPOL = 0x3D; // Read-only amplifier polarity type
        public const uint NUMAMP = 0x3E; // Read-only number of amplifiers
        public const uint CHIPID = 0x3F; // Read-only chip ID

        public const int AuxChannelCount = 3;

        // Page 26 of Rhd2000 datasheet
        internal static IReadOnlyList<int> ToLowCutoffToRegisters(Rhd2000AnalogLowCutoff lowCut) => lowCut switch
        {
            Rhd2000AnalogLowCutoff.Low500Hz => new[] { 13, 0, 0 },
            Rhd2000AnalogLowCutoff.Low300Hz => new[] { 15, 0, 0 },
            Rhd2000AnalogLowCutoff.Low250Hz => new[] { 17, 0, 0 },
            Rhd2000AnalogLowCutoff.Low200Hz => new[] { 18, 0, 0 },
            Rhd2000AnalogLowCutoff.Low150Hz => new[] { 21, 0, 0 },
            Rhd2000AnalogLowCutoff.Low100Hz => new[] { 25, 0, 0 },
            Rhd2000AnalogLowCutoff.Low75Hz => new[] { 28, 0, 0 },
            Rhd2000AnalogLowCutoff.Low50Hz => new[] { 34, 0, 0 },
            Rhd2000AnalogLowCutoff.Low30Hz => new[] { 44, 0, 0 },
            Rhd2000AnalogLowCutoff.Low25Hz => new[] { 48, 0, 0 },
            Rhd2000AnalogLowCutoff.Low20Hz => new[] { 54, 0, 0 },
            Rhd2000AnalogLowCutoff.Low15Hz => new[] { 62, 0, 0 },
            Rhd2000AnalogLowCutoff.Low10Hz => new[] { 5, 1, 0 },
            Rhd2000AnalogLowCutoff.Low7500mHz => new[] { 18, 1, 0 },
            Rhd2000AnalogLowCutoff.Low5000mHz => new[] { 40, 1, 0 },
            Rhd2000AnalogLowCutoff.Low3000mHz => new[] { 20, 2, 0 },
            Rhd2000AnalogLowCutoff.Low2500mHz => new[] { 42, 2, 0 },
            Rhd2000AnalogLowCutoff.Low2000mHz => new[] { 8, 3, 0 },
            Rhd2000AnalogLowCutoff.Low1500mHz => new[] { 9, 4, 0 },
            Rhd2000AnalogLowCutoff.Low1000mHz => new[] { 44, 6, 0 },
            Rhd2000AnalogLowCutoff.Low750mHz => new[] { 49, 9, 0 },
            Rhd2000AnalogLowCutoff.Low500mHz => new[] { 35, 17, 0 },
            Rhd2000AnalogLowCutoff.Low300mHz => new[] { 1, 40, 0 },
            Rhd2000AnalogLowCutoff.Low250mHz => new[] { 56, 54, 0 },
            Rhd2000AnalogLowCutoff.Low100mHz => new[] { 16, 60, 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(lowCut), $"Unsupported low cutoff value: {lowCut}"),
        };

        // Page 25 of Rhd2000 datasheet
        internal static IReadOnlyList<int> ToHighCutoffToRegisters(Rhd2000AnalogHighCutoff highCut) => highCut switch
        {
            Rhd2000AnalogHighCutoff.High20000Hz => new[] { 8, 0, 4, 0 },
            Rhd2000AnalogHighCutoff.High15000Hz => new[] { 11, 0, 8, 0 },
            Rhd2000AnalogHighCutoff.High10000Hz => new[] { 17, 0, 16, 0 },
            Rhd2000AnalogHighCutoff.High7500Hz => new[] { 22, 0, 23, 0 },
            Rhd2000AnalogHighCutoff.High5000Hz => new[] { 33, 0, 37, 0 },
            Rhd2000AnalogHighCutoff.High3000Hz => new[] { 3, 1, 13, 1 },
            Rhd2000AnalogHighCutoff.High2500Hz => new[] { 13, 1, 25, 1 },
            Rhd2000AnalogHighCutoff.High2000Hz => new[] { 27, 1, 44, 1 },
            Rhd2000AnalogHighCutoff.High1500Hz => new[] { 1, 2, 23, 2 },
            Rhd2000AnalogHighCutoff.High1000Hz => new[] { 46, 2, 30, 3 },
            Rhd2000AnalogHighCutoff.High750Hz => new[] { 41, 3, 36, 4 },
            Rhd2000AnalogHighCutoff.High500Hz => new[] { 30, 5, 43, 6 },
            Rhd2000AnalogHighCutoff.High300Hz => new[] { 6, 9, 2, 11 },
            Rhd2000AnalogHighCutoff.High250Hz => new[] { 42, 10, 5, 13 },
            Rhd2000AnalogHighCutoff.High200Hz => new[] { 24, 13, 7, 16 },
            Rhd2000AnalogHighCutoff.High150Hz => new[] { 44, 17, 8, 21 },
            Rhd2000AnalogHighCutoff.High100Hz => new[] { 38, 26, 5, 31 },
            _ => throw new ArgumentOutOfRangeException(nameof(highCut), $"Unsupported high cutoff value: {highCut}"),
        };

        // Page 27 of Rhd2000 datasheet
        internal static IReadOnlyList<int> ToAdcAndMuxBias(float adcKiloHertz) => adcKiloHertz switch
        {
            _ when adcKiloHertz <= 120.0f => new[] { 32, 40 },
            _ when adcKiloHertz <= 140.0f => new[] { 16, 40 },
            _ when adcKiloHertz <= 175.0f => new[] { 8, 40 },
            _ when adcKiloHertz <= 220.0f => new[] { 8, 32 },
            _ when adcKiloHertz <= 280.0f => new[] { 8, 26 },
            _ when adcKiloHertz <= 350.0f => new[] { 4, 18 },
            _ when adcKiloHertz <= 440.0f => new[] { 3, 16 },
            _ when adcKiloHertz <= 525.0f => new[] { 3, 7 },
            _ => new[] { 2, 4 },
        };

        internal static int ToEphysChannelCount(Rhd2000ChipId chipId)
        {
            return chipId switch
            {
                Rhd2000ChipId.Rhd2216 => 16,
                Rhd2000ChipId.Rhd2132 => 32,
                Rhd2000ChipId.Rhd2164 => 64,
                _ => throw new ArgumentOutOfRangeException(nameof(chipId), $"Unsupported chip ID: {chipId}"),
            };
        }
    }



    /// <summary>
    /// Specifies the lower cutoff frequency of the Rhd2164 analog (pre-ADC) bandpass filter.
    /// </summary>
    public enum Rhd2000AnalogLowCutoff
    {
        /// <summary>
        /// Specifies 500 Hz.
        /// </summary>
        Low500Hz,
        /// <summary>
        /// Specifies 300 Hz.
        /// </summary>
        Low300Hz,
        /// <summary>
        /// Specifies 250 Hz.
        /// </summary>
        Low250Hz,
        /// <summary>
        /// Specifies 200 Hz.
        /// </summary>
        Low200Hz,
        /// <summary>
        /// Specifies 150 Hz.
        /// </summary>
        Low150Hz,
        /// <summary>
        /// Specifies 100 Hz.
        /// </summary>
        Low100Hz,
        /// <summary>
        /// Specifies 75 Hz.
        /// </summary>
        Low75Hz,
        /// <summary>
        /// Specifies 50 Hz.
        /// </summary>
        Low50Hz,
        /// <summary>
        /// Specifies 30 Hz.
        /// </summary>
        Low30Hz,
        /// <summary>
        /// Specifies 25 Hz.
        /// </summary>
        Low25Hz,
        /// <summary>
        /// Specifies 20 Hz.
        /// </summary>
        Low20Hz,
        /// <summary>
        /// Specifies 15 Hz.
        /// </summary>
        Low15Hz,
        /// <summary>
        /// Specifies 10 Hz.
        /// </summary>
        Low10Hz,
        /// <summary>
        /// Specifies 7.5 Hz.
        /// </summary>
        Low7500mHz,
        /// <summary>
        /// Specifies 5 Hz.
        /// </summary>
        Low5000mHz,
        /// <summary>
        /// Specifies 3 Hz.
        /// </summary>
        Low3000mHz,
        /// <summary>
        /// Specifies 2.5 Hz.
        /// </summary>
        Low2500mHz,
        /// <summary>
        /// Specifies 2 Hz.
        /// </summary>
        Low2000mHz,
        /// <summary>
        /// Specifies 1.5 Hz.
        /// </summary>
        Low1500mHz,
        /// <summary>
        /// Specifies 1 Hz.
        /// </summary>
        Low1000mHz,
        /// <summary>
        /// Specifies 0.75 Hz.
        /// </summary>
        Low750mHz,
        /// <summary>
        /// Specifies 0.5 Hz.
        /// </summary>
        Low500mHz,
        /// <summary>
        /// Specifies 0.3 Hz.
        /// </summary>
        Low300mHz,
        /// <summary>
        /// Specifies 0.25 Hz.
        /// </summary>
        Low250mHz,
        /// <summary>
        /// Specifies 0.1 Hz.
        /// </summary>
        Low100mHz,
    }

    /// <summary>
    /// Specifies the upper cutoff frequency of the Rhd2164 analog (pre-ADC) bandpass filter.
    /// </summary>
    public enum Rhd2000AnalogHighCutoff
    {
        /// <summary>
        /// Specifies 20 kHz.
        /// </summary>
        High20000Hz,
        /// <summary>
        /// Specifies 15 kHz.
        /// </summary>
        High15000Hz,
        /// <summary>
        /// Specifies 10 kHz.
        /// </summary>
        High10000Hz,
        /// <summary>
        /// Specifies 7.5 kHz.
        /// </summary>
        High7500Hz,
        /// <summary>
        /// Specifies 5 kHz.
        /// </summary>
        High5000Hz,
        /// <summary>
        /// Specifies 3 kHz.
        /// </summary>
        High3000Hz,
        /// <summary>
        /// Specifies 2.5 kHz.
        /// </summary>
        High2500Hz,
        /// <summary>
        /// Specifies 2 kHz.
        /// </summary>
        High2000Hz,
        /// <summary>
        /// Specifies 1.5 kHz.
        /// </summary>
        High1500Hz,
        /// <summary>
        /// Specifies 1 kHz.
        /// </summary>
        High1000Hz,
        /// <summary>
        /// Specifies 750 Hz.
        /// </summary>
        High750Hz,
        /// <summary>
        /// Specifies 500 Hz.
        /// </summary>
        High500Hz,
        /// <summary>
        /// Specifies 300 Hz.
        /// </summary>
        High300Hz,
        /// <summary>
        /// Specifies 250 Hz.
        /// </summary>
        High250Hz,
        /// <summary>
        /// Specifies 200 Hz.
        /// </summary>
        High200Hz,
        /// <summary>
        /// Specifies 150 Hz.
        /// </summary>
        High150Hz,
        /// <summary>
        /// Specifies 100 Hz.
        /// </summary>
        High100Hz,
    }

    /// <summary>
    /// Specifies the cutoff frequency of the Rhd2164 digital (post-ADC) high-pass filter.
    /// </summary>
    public enum Rhd2000DspCutoff
    {
        /// <summary>
        /// Specifies differences between adjacent samples of each channel (approximate first-order derivative).
        /// </summary>
        Differential = 0,
        /// <summary>
        /// Specifies 3309 Hz.
        /// </summary>
        Dsp3309Hz,
        /// <summary>
        /// Specifies 1374 Hz.
        /// </summary>
        Dsp1374Hz,
        /// <summary>
        /// Specifies 638 Hz.
        /// </summary>
        Dsp638Hz,
        /// <summary>
        /// Specifies 308 Hz.
        /// </summary>
        Dsp308Hz,
        /// <summary>
        /// Specifies 152 Hz.
        /// </summary>
        Dsp152Hz,
        /// <summary>
        /// Specifies 75.2 Hz.
        /// </summary>
        Dsp75Hz,
        /// <summary>
        /// Specifies 37.4 Hz.
        /// </summary>
        Dsp37Hz,
        /// <summary>
        /// Specifies 18.7 Hz.
        /// </summary>
        Dsp19Hz,
        /// <summary>
        /// Specifies 9.34 Hz.
        /// </summary>
        Dsp9336mHz,
        /// <summary>
        /// Specifies 4.67 Hz.
        /// </summary>
        Dsp4665mHz,
        /// <summary>
        /// Specifies 2.33 Hz.
        /// </summary>
        Dsp2332mHz,
        /// <summary>
        /// Specifies 1.17 Hz.
        /// </summary>
        Dsp1166mHz,
        /// <summary>
        /// Specifies 0.583 Hz.
        /// </summary>
        Dsp583mHz,
        /// <summary>
        /// Specifies 0.291 Hz.
        /// </summary>
        Dsp291mHz,
        /// <summary>
        /// Specifies 0.146 Hz.
        /// </summary>
        Dsp146mHz,
        /// <summary>
        /// Specifies that no digital high-pass filtering should be applied.
        /// </summary>
        Off,
    }

    /// <summary>
    /// Specifies a chip from the Intan Rhd2000 series.
    /// </summary>
    public enum Rhd2000ChipId
    {
        /// <summary>
        /// Specifies an Intan Rhd2216 a 16 differential channel chip.
        /// </summary>
        Rhd2216,
        /// <summary>
        /// Specifies an Intan Rhd2132 a 32 unipolar channel chip.
        /// </summary>
        Rhd2132,
        /// <summary>
        /// Specifies an Intan Rhd2132 a 64 unipolar channel chip.
        /// </summary>
        Rhd2164,
    }
}
