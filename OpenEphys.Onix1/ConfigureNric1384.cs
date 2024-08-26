using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures a Nric184 bioacquisition device.
    /// </summary>
    public class ConfigureNric1384 : SingleDeviceFactory
    {
        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNric1384"/> class.
        /// </summary>
        public ConfigureNric1384()
            : base(typeof(Nric1384))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Nric1384Data"/> will produce data. If set to false, 
        /// <see cref="Nric1384Data"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Nric1384 data stream is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the amplifier gain for the spike-band.
        /// </summary>
        /// <remarks>
        /// The spike-band is from DC to 10 kHz if <see cref="SpikeFilter"/> is set to false, while the 
        /// spike-band is from 300 Hz to 10 kHz if <see cref="SpikeFilter"/> is set to true.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        /// <summary>
        /// Gets or sets the amplifier gain for the LFP-band.
        /// </summary>
        /// <remarks>
        /// The LFP band is from 0.5 to 500 Hz.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for LFP-band.")]
        public NeuropixelsV1Gain LfpAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain50;

        /// <summary>
        /// Gets or sets the state of the spike-band filter.
        /// </summary>
        /// <remarks>
        /// If set to true, the spike-band has a 300 Hz high-pass filter which will be activated. If set to
        /// false, the high-pass filter will not to be activated.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("If true, activates a 300 Hz high-pass in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each chip is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across chips. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are chip-specific and not interchangeable across chips. Calibration files must contain the 
        /// serial number of the corresponding chip on their first line of text. If you have lost track of a calibration 
        /// file for your chip, email IMEC at neuropixels.info@imec.be with the chip serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the Nric1384 gain calibraiton file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the ADC calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each chip must be provided with an ADC calibration file that contains chip-specific hardware settings that is
        /// created by IMEC during factory calibration. These files are used to set internal bias currents, correct for ADC
        /// nonlinearities, correct ADC-zero crossing non-monotonicities, etc. Using the correct calibration file is mandatory
        /// for the chip to operate correctly. 
        /// </para>
        /// <para>
        /// Calibration files are chip-specific and not interchangeable across chips. Calibration files must contain the 
        /// serial number of the corresponding chip on their first line of text. If you have lost track of a calibration 
        /// file for your chip, email IMEC at neuropixels.info@imec.be with the chip serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv")]
        [Description("Path to the Nric1384 ADC calibraiton file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string AdcCalibrationFile { get; set; }

        /// <summary>
        /// Configures a Nric1384 bioacquisition device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a Nric1384 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, typeof(Nric1384));
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
