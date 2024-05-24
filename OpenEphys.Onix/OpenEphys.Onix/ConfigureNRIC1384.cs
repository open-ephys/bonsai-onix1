using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureNric1384 : SingleDeviceFactory
    {
        public ConfigureNric1384()
            : base(typeof(Nric1384))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NRIC1384 data stream is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.x1000;

        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for LFP-band.")]
        public NeuropixelsV1Gain LfpAmplifierGain { get; set; } = NeuropixelsV1Gain.x50;

        [Category(ConfigurationCategory)]
        [Description("If true, activates a 300 Hz high-pass in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the NRIC1384 gain calibraiton file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFile { get; set; }

        [FileNameFilter("ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv")]
        [Description("Path to the NRIC1384 ADC calibraiton file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string AdcCalibrationFile { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Nric1384.ID);
                device.WriteRegister(Nric1384.ENABLE, enable ? 1u : 0);

                if (enable)
                {
                    var probeControl = new Nric1384RegisterContext(device, SpikeAmplifierGain, LfpAmplifierGain, SpikeFilter, GainCalibrationFile, AdcCalibrationFile);
                    probeControl.InitializeChip();
                    probeControl.WriteShiftRegisters();
                }

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);

            });
        }
    }

    static class Nric1384
    {
        public const int ID = 33;

        public const int I2cAddress = 0x70;

        // managed registers
        public const uint ENABLE = 0x8000; // Enable or disable the data output stream
        public const uint ADC00_OFF_THRESH = 0x8001; // ADC 0 offset and threshold parameters: [6-bit ADC 00 Offset, 10-bit ADC 00 Threshold]
        public const uint ADC01_OFF_THRESH = 0x8002;
        public const uint ADC02_OFF_THRESH = 0x8003;
        public const uint ADC03_OFF_THRESH = 0x8004;
        public const uint ADC04_OFF_THRESH = 0x8005;
        public const uint ADC05_OFF_THRESH = 0x8006;
        public const uint ADC06_OFF_THRESH = 0x8007;
        public const uint ADC07_OFF_THRESH = 0x8008;
        public const uint ADC08_OFF_THRESH = 0x8009;
        public const uint ADC09_OFF_THRESH = 0x800a;
        public const uint ADC10_OFF_THRESH = 0x800b;
        public const uint ADC11_OFF_THRESH = 0x800c;
        public const uint ADC12_OFF_THRESH = 0x800d;
        public const uint ADC13_OFF_THRESH = 0x800e;
        public const uint ADC14_OFF_THRESH = 0x800f;
        public const uint ADC15_OFF_THRESH = 0x8010;
        public const uint ADC16_OFF_THRESH = 0x8011;
        public const uint ADC17_OFF_THRESH = 0x8012;
        public const uint ADC18_OFF_THRESH = 0x8013;
        public const uint ADC19_OFF_THRESH = 0x8014;
        public const uint ADC20_OFF_THRESH = 0x8015;
        public const uint ADC21_OFF_THRESH = 0x8016;
        public const uint ADC22_OFF_THRESH = 0x8017;
        public const uint ADC23_OFF_THRESH = 0x8018;
        public const uint ADC24_OFF_THRESH = 0x8019;
        public const uint ADC25_OFF_THRESH = 0x801a;
        public const uint ADC26_OFF_THRESH = 0x801b;
        public const uint ADC27_OFF_THRESH = 0x801c;
        public const uint ADC28_OFF_THRESH = 0x801d;
        public const uint ADC29_OFF_THRESH = 0x801e;
        public const uint ADC30_OFF_THRESH = 0x801f;
        public const uint ADC31_OFF_THRESH = 0x8020; // ADC 31 offset and threshold parameters: [6-bit ADC 31 Offset , 10-bit ADC 31 Threshold]
        public const uint LFP_GAIN = 0x8021; // LFP gain correction parameter: [X Q1.14]
        public const uint AP_GAIN = 0x8022; // AP gain correction parameter: [X Q1.14]

        // unmanaged regiseters
        public const uint OP_MODE = 0X00;
        public const uint REC_MOD = 0X01;
        public const uint CAL_MOD = 0X02;
        public const uint STATUS = 0X08;
        public const uint SYNC = 0X09;
        public const uint SR_CHAIN3 = 0X0C; // Odd channels
        public const uint SR_CHAIN2 = 0X0D; // Even channels
        public const uint SR_LENGTH2 = 0X0F;
        public const uint SR_LENGTH1 = 0X10;
        public const uint SOFT_RESET = 0X11;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Nric1384))
            {
            }
        }
    }
}
