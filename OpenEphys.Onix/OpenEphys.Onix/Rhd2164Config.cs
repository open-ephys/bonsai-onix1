using System;
using System.Collections.Generic;

namespace OpenEphys.Onix
{
    internal static class Rhd2164Config
    {
        // Page 26 of RHD2000 datasheet
        internal static IReadOnlyList<int> ToLowCutoffToRegisters(Rhd2164AnalogLowCutoff lowCut) => lowCut switch
        {
            Rhd2164AnalogLowCutoff.Low500Hz => new[] { 13, 0, 0 },
            Rhd2164AnalogLowCutoff.Low300Hz => new[] { 15, 0, 0 },
            Rhd2164AnalogLowCutoff.Low250Hz => new[] { 17, 0, 0 },
            Rhd2164AnalogLowCutoff.Low200Hz => new[] { 18, 0, 0 },
            Rhd2164AnalogLowCutoff.Low150Hz => new[] { 21, 0, 0 },
            Rhd2164AnalogLowCutoff.Low100Hz => new[] { 25, 0, 0 },
            Rhd2164AnalogLowCutoff.Low75Hz => new[] { 28, 0, 0 },
            Rhd2164AnalogLowCutoff.Low50Hz => new[] { 34, 0, 0 },
            Rhd2164AnalogLowCutoff.Low30Hz => new[] { 44, 0, 0 },
            Rhd2164AnalogLowCutoff.Low25Hz => new[] { 48, 0, 0 },
            Rhd2164AnalogLowCutoff.Low20Hz => new[] { 54, 0, 0 },
            Rhd2164AnalogLowCutoff.Low15Hz => new[] { 62, 0, 0 },
            Rhd2164AnalogLowCutoff.Low10Hz => new[] { 5, 1, 0 },
            Rhd2164AnalogLowCutoff.Low7500mHz => new[] { 18, 1, 0 },
            Rhd2164AnalogLowCutoff.Low5000mHz => new[] { 40, 1, 0 },
            Rhd2164AnalogLowCutoff.Low3000mHz => new[] { 20, 2, 0 },
            Rhd2164AnalogLowCutoff.Low2500mHz => new[] { 42, 2, 0 },
            Rhd2164AnalogLowCutoff.Low2000mHz => new[] { 8, 3, 0 },
            Rhd2164AnalogLowCutoff.Low1500mHz => new[] { 9, 4, 0 },
            Rhd2164AnalogLowCutoff.Low1000mHz => new[] { 44, 6, 0 },
            Rhd2164AnalogLowCutoff.Low750mHz => new[] { 49, 9, 0 },
            Rhd2164AnalogLowCutoff.Low500mHz => new[] { 35, 17, 0 },
            Rhd2164AnalogLowCutoff.Low300mHz => new[] { 1, 40, 0 },
            Rhd2164AnalogLowCutoff.Low250mHz => new[] { 56, 54, 0 },
            Rhd2164AnalogLowCutoff.Low100mHz => new[] { 16, 60, 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(lowCut), $"Unsupported low cutoff value : {lowCut}"),
        };

        // Page 25 of RHD2000 datasheet
        internal static IReadOnlyList<int> ToHighCutoffToRegisters(Rhd2164AnalogHighCutoff highCut) => highCut switch
        {
            Rhd2164AnalogHighCutoff.High20000Hz => new[] { 8, 0, 4, 0 },
            Rhd2164AnalogHighCutoff.High15000Hz => new[] { 11, 0, 8, 0 },
            Rhd2164AnalogHighCutoff.High10000Hz => new[] { 17, 0, 16, 0 },
            Rhd2164AnalogHighCutoff.High7500Hz => new[] { 22, 0, 23, 0 },
            Rhd2164AnalogHighCutoff.High5000Hz => new[] { 33, 0, 37, 0 },
            Rhd2164AnalogHighCutoff.High3000Hz => new[] { 3, 1, 13, 1 },
            Rhd2164AnalogHighCutoff.High2500Hz => new[] { 13, 1, 25, 1 },
            Rhd2164AnalogHighCutoff.High2000Hz => new[] { 27, 1, 44, 1 },
            Rhd2164AnalogHighCutoff.High1500Hz => new[] { 1, 2, 23, 2 },
            Rhd2164AnalogHighCutoff.High1000Hz => new[] { 46, 2, 30, 3 },
            Rhd2164AnalogHighCutoff.High750Hz => new[] { 41, 3, 36, 4 },
            Rhd2164AnalogHighCutoff.High500Hz => new[] { 30, 5, 43, 6 },
            Rhd2164AnalogHighCutoff.High300Hz => new[] { 6, 9, 2, 11 },
            Rhd2164AnalogHighCutoff.High250Hz => new[] { 42, 10, 5, 13 },
            Rhd2164AnalogHighCutoff.High200Hz => new[] { 24, 13, 7, 16 },
            Rhd2164AnalogHighCutoff.High150Hz => new[] { 44, 17, 8, 21 },
            Rhd2164AnalogHighCutoff.High100Hz => new[] { 38, 26, 5, 31 },
            _ => throw new ArgumentOutOfRangeException(nameof(highCut), $"Unsupported high cutoff value : {highCut}"),
        };
    }

    /// <summary>
    /// Specifies the lower cutoff frequency of the RHD2164 analog (pre-ADC) bandpass filter.
    /// </summary>
    public enum Rhd2164AnalogLowCutoff
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
    /// Specifies the upper cutoff frequency of the RHD2164 analog (pre-ADC) bandpass filter.
    /// </summary>
    public enum Rhd2164AnalogHighCutoff
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
    /// Specifies the cutoff frequency of the RHD2164 digital (post-ADC) high-pass filter.
    /// </summary>
    public enum Rhd2164DspCutoff
    {
        /// <summary>
        /// Specifies differences between adjacent samples of each channel (approximate first-order derivative).
        /// </summary>
        Differential = 0,
        /// <summary>
        /// Specifies 3310 Hz.
        /// </summary>
        Dsp3309Hz,
        /// <summary>
        /// Specifies 1370 Hz.
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
}
