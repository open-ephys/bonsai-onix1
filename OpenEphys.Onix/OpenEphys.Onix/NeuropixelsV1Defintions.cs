using System;

namespace OpenEphys.Onix
{
    public static class NeuropixelsV1
    {
        public const int AdcCount = 32;
        public const int ChannelCount = 384;
        public const int FramesPerRoundRobin = 12;
        public const int FramesPerSuperframe = 13;
        public const int SuperframesPerUltraframe = 12;

        public static float GainToFloat(NeuropixelsV1Gain gain) => gain switch
        {
            NeuropixelsV1Gain.x50 => 50f,
            NeuropixelsV1Gain.x125 => 125f,
            NeuropixelsV1Gain.x250 => 250f,
            NeuropixelsV1Gain.x500 => 500f,
            NeuropixelsV1Gain.x1000 => 1000f,
            NeuropixelsV1Gain.x1500 => 1500f,
            NeuropixelsV1Gain.x2000 => 2000f,
            NeuropixelsV1Gain.x3000 => 3000f,
            _ => throw new ArgumentOutOfRangeException(nameof(gain), $"Unexpected gain value: {gain}"),
        };
    }

    public class NeuropixelsV1Adc
    {
        public int CompP { get; set; } = 16;
        public int CompN { get; set; } = 16;
        public int Slope { get; set; } = 0;
        public int Coarse { get; set; } = 0;
        public int Fine { get; set; } = 0;
        public int Cfix { get; set; } = 0;
        public int Offset { get; set; } = 0;
        public int Threshold { get; set; } = 512;
    }
    public enum NeuropixelsV1Reference : byte
    {
        Ext = 0b001,
        Tip = 0b010
    }

    public enum NeuropixelsV1Gain : byte
    {
        x50 = 0b000,
        x125 = 0b001,
        x250 = 0b010,
        x500 = 0b011,
        x1000 = 0b100,
        x1500 = 0b101,
        x2000 = 0b110,
        x3000 = 0b111
    }

    [Flags]
    public enum NeuropixelsV1CalibrationRegisterValues : uint
    {
        CAL_OFF = 0,
        OSC_ACTIVE = 1 << 4, // 0 = external osc inactive, 1 = activate the external calibration oscillator
        ADC_CAL = 1 << 5, // Enable ADC calibration
        CH_CAL = 1 << 6, // Enable channel gain calibration
        PIX_CAL = 1 << 7, // Enable pixel + channel gain calibration

        // Useful combinations
        OSC_ACTIVE_AND_ADC_CAL = OSC_ACTIVE | ADC_CAL,
        OSC_ACTIVE_AND_CH_CAL = OSC_ACTIVE | CH_CAL,
        OSC_ACTIVE_AND_PIX_CAL = OSC_ACTIVE | PIX_CAL,

    };

    [Flags]
    public enum NeuropixelsV1RecordRegisterValues : uint
    {
        SR_RESET = 1 << 5, // 1 = Set analog SR chains to default values
        DIG_ENABLE = 1 << 6, // 0 = Reset the MUX, ADC, and PSB counter, 1 = Disable reset
        CH_ENABLE = 1 << 7, // 0 = Reset channel pseudo-registers, 1 = Disable reset

        // Useful combinations
        DIG_CH_RESET = 0,  // Yes, this is actually correct
        ACTIVE = DIG_ENABLE | CH_ENABLE,
    };

    [Flags]
    public enum NeuropixelsV1OperationRegisterValues : uint
    {
        TEST = 1 << 3, // Enable Test mode
        DIG_TEST = 1 << 4, // Enable Digital Test mode
        CALIBRATE = 1 << 5, // Enable calibration mode
        RECORD = 1 << 6, // Enable recording mode
        POWER_DOWN = 1 << 7, // Enable power down mode

        // Useful combinations
        RECORD_AND_DIG_TEST = RECORD | DIG_TEST,
        RECORD_AND_CALIBRATE = RECORD | CALIBRATE,
    };
}
