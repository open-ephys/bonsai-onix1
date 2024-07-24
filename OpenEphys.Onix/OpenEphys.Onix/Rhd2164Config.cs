using System.Collections.Generic;

namespace OpenEphys.Onix
{
    public static class Rhd2164Config
    {
        public static readonly IReadOnlyDictionary<Rhd2164AnalogLowCutoff, IReadOnlyList<int>> AnalogLowCutoffToRegisters =
            new Dictionary<Rhd2164AnalogLowCutoff, IReadOnlyList<int>>()
        {
            { Rhd2164AnalogLowCutoff.Low500Hz, new[] { 13, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low300Hz, new[] { 15, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low250Hz, new[] { 17, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low200Hz, new[] { 18, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low150Hz, new[] { 21, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low100Hz, new[] { 25, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low75Hz, new[] { 28, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low50Hz, new[] { 34, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low30Hz, new[] { 44, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low25Hz, new[] { 48, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low20Hz, new[] { 54, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low15Hz, new[] { 62, 0, 0 } },
            { Rhd2164AnalogLowCutoff.Low10Hz, new[] { 5, 1, 0 } },
            { Rhd2164AnalogLowCutoff.Low7500mHz, new[] { 18, 1, 0 } },
            { Rhd2164AnalogLowCutoff.Low5000mHz, new[] { 40, 1, 0 } },
            { Rhd2164AnalogLowCutoff.Low3090mHz, new[] { 20, 2, 0 } },
            { Rhd2164AnalogLowCutoff.Low2500mHz, new[] { 42, 2, 0 } },
            { Rhd2164AnalogLowCutoff.Low2000mHz, new[] { 8, 3, 0 } },
            { Rhd2164AnalogLowCutoff.Low1500mHz, new[] { 9, 4, 0 } },
            { Rhd2164AnalogLowCutoff.Low1000mHz, new[] { 44, 6, 0 } },
            { Rhd2164AnalogLowCutoff.Low750mHz, new[] { 49, 9, 0 } },
            { Rhd2164AnalogLowCutoff.Low500mHz, new[] { 35, 17, 0 } },
            { Rhd2164AnalogLowCutoff.Low300mHz, new[] { 1, 40, 0 } },
            { Rhd2164AnalogLowCutoff.Low250mHz, new[] { 56, 54, 0 } },
            { Rhd2164AnalogLowCutoff.Low100mHz, new[] { 16, 60, 1 } },
        };

        public static readonly IReadOnlyDictionary<Rhd2164AnalogHighCutoff, IReadOnlyList<int>> AnalogHighCutoffToRegisters =
            new Dictionary<Rhd2164AnalogHighCutoff, IReadOnlyList<int>>()
        {
            { Rhd2164AnalogHighCutoff.High20000Hz, new[] { 8, 0, 4, 0 } },
            { Rhd2164AnalogHighCutoff.High15000Hz, new[] { 11, 0, 8, 0 } },
            { Rhd2164AnalogHighCutoff.High10000Hz, new[] { 17, 0, 16, 0 } },
            { Rhd2164AnalogHighCutoff.High7500Hz, new[] { 22, 0, 23, 0 } },
            { Rhd2164AnalogHighCutoff.High5000Hz, new[] { 33, 0, 37, 0 } },
            { Rhd2164AnalogHighCutoff.High3000Hz, new[] { 3, 1, 13, 1 } },
            { Rhd2164AnalogHighCutoff.High2500Hz, new[] { 13, 1, 25, 1 } },
            { Rhd2164AnalogHighCutoff.High2000Hz, new[] { 27, 1, 44, 1 } },
            { Rhd2164AnalogHighCutoff.High1500Hz, new[] { 1, 2, 23, 2 } },
            { Rhd2164AnalogHighCutoff.High1000Hz, new[] { 46, 2, 30, 3 } },
            { Rhd2164AnalogHighCutoff.High750Hz, new[] { 41, 3, 36, 4 } },
            { Rhd2164AnalogHighCutoff.High500Hz, new[] { 30, 5, 43, 6 } },
            { Rhd2164AnalogHighCutoff.High300Hz, new[] { 6, 9, 2, 11 } },
            { Rhd2164AnalogHighCutoff.High250Hz, new[] { 42, 10, 5, 13 } },
            { Rhd2164AnalogHighCutoff.High200Hz, new[] { 24, 13, 7, 16 } },
            { Rhd2164AnalogHighCutoff.High150Hz, new[] { 44, 17, 8, 21 } },
            { Rhd2164AnalogHighCutoff.High100Hz, new[] { 38, 26, 5, 31 } },
        };


    }

    public enum Rhd2164AnalogLowCutoff
    {
        Low500Hz,
        Low300Hz,
        Low250Hz,
        Low200Hz,
        Low150Hz,
        Low100Hz,
        Low75Hz,
        Low50Hz,
        Low30Hz,
        Low25Hz,
        Low20Hz,
        Low15Hz,
        Low10Hz,
        Low7500mHz,
        Low5000mHz,
        Low3090mHz,
        Low2500mHz,
        Low2000mHz,
        Low1500mHz,
        Low1000mHz,
        Low750mHz,
        Low500mHz,
        Low300mHz,
        Low250mHz,
        Low100mHz
    }

    public enum Rhd2164AnalogHighCutoff
    {
        High20000Hz,
        High15000Hz,
        High10000Hz,
        High7500Hz,
        High5000Hz,
        High3000Hz,
        High2500Hz,
        High2000Hz,
        High1500Hz,
        High1000Hz,
        High750Hz,
        High500Hz,
        High300Hz,
        High250Hz,
        High200Hz,
        High150Hz,
        High100Hz
    }

    public enum Rhd2164DspCutoff
    {
        /// <summary>
        /// 
        /// </summary>
        Differential = 0,

        /// <summary>
        /// 3310 Hz
        /// </summary>
        Dsp3309Hz,

        /// <summary>
        /// 1370 Hz
        /// </summary>
        Dsp1374Hz,
        
        /// <summary>
        /// 638 Hz
        /// </summary>
        Dsp638Hz,
        
        /// <summary>
        /// 308 Hz
        /// </summary>
        Dsp308Hz,
        
        /// <summary>
        /// 152 Hz
        /// </summary>
        Dsp152Hz,
        
        /// <summary>
        /// 75.2 Hz
        /// </summary>
        Dsp75Hz,
        
        /// <summary>
        /// 37.4 Hz
        /// </summary>
        Dsp37Hz,
        
        /// <summary>
        /// 18.7 Hz
        /// </summary>
        Dsp19Hz,
        
        /// <summary>
        /// 9.34 Hz
        /// </summary>
        Dsp9336mHz,
        
        /// <summary>
        /// 4.67 Hz
        /// </summary>
        Dsp4665mHz,
        
        /// <summary>
        /// 2.33 Hz
        /// </summary>
        Dsp2332mHz,
        
        /// <summary>
        /// 1.17 Hz
        /// </summary>
        Dsp1166mHz,
        
        /// <summary>
        /// 0.583 Hz
        /// </summary>
        Dsp583mHz,
        
        /// <summary>
        /// 0.291 Hz
        /// </summary>
        Dsp291mHz,
        
        /// <summary>
        /// 0.146 Hz
        /// </summary>
        Dsp146mHz,

        /// <summary>
        /// 
        /// </summary>
        Off
    }
}
