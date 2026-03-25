using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a parallel serial bus decoder for an embedded Rhd2000 data stream.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>.
    /// </remarks>
    [Description("Configures a parallel serial bus decoder for an embedded Rhd2000 data stream.")]
    public class ConfigureRhd2000PsbDecoder : SingleDeviceFactory
    {
        internal ushort StreamIndex { private get; set; } = 0;

        internal Func<Rhd2000ChipId> GetChipId { private get; set; } = () => Rhd2000ChipId.Rhd2216;

        internal Action<I2CRegisterContext> EnableController { private get; set; } = _ => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureRhd2000PsbDecoder"/> class.
        /// </summary>
        public ConfigureRhd2000PsbDecoder()
            : base(typeof(Rhd2000PsbDecoder))
        {
        }

        /// <summary>
        /// Gets or sets the per-channel ADC sampling rate.
        /// </summary>
        /// <remarks>
        /// The amplifiers on the RHD2164 chip, past the analog filter, introduce a DC offset that varies with
        /// each channel. The <see cref="Rhd2000DspCutoff"/> exists to remove this DC offset and ensure that
        /// all signals are centered at zero. With it disabled, all the signals will appear centered at
        /// different values.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies the per-channel ADC sampling rate.")]
        public Rhd2000PsbDecoderSampleRate SamplesPerSecond { get; set; } = Rhd2000PsbDecoderSampleRate.ThirtyKiloHertz;

        /// <summary>
        /// Gets or sets the cutoff frequency for the digital (post-ADC) high-pass filter used for amplifier
        /// offset removal.
        /// </summary>
        /// <remarks>
        /// The amplifiers on the RHD2164 chip, past the analog filter, introduce a DC offset that varies with
        /// each channel. The <see cref="Rhd2000DspCutoff"/> exists to remove this DC offset and ensure that
        /// all signals are centered at zero. With it disabled, all the signals will appear centered at
        /// different values.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the digital (post-ADC) high-pass filter used for amplifier offset removal.")]
        public Rhd2000DspCutoff DspCutoff { get; set; } = Rhd2000DspCutoff.Off;

        /// <summary>
        /// Gets or sets the low cutoff frequency of the analog (pre-ADC) bandpass filter.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the low cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2000AnalogLowCutoff AnalogLowCutoff { get; set; } = Rhd2000AnalogLowCutoff.Low100mHz;

        /// <summary>
        /// Gets or sets the high cutoff frequency of the analog (pre-ADC) bandpass filter.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the high cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2000AnalogHighCutoff AnalogHighCutoff { get; set; } = Rhd2000AnalogHighCutoff.High10000Hz;

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Rhd2000eData"/> will produce data. If set to false,
        /// <see cref="Rhd2000eData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Rhd2000 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures a Rhd2000 electrophysiology chip.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration
        /// actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure a
        /// Rhd2000 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var streamIndex = StreamIndex;
            var chip = GetChipId();
            return source.ConfigureAndLatchDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // this device can only enable the stream, cannot disable
                if (enable)
                {
                    device.WriteRegister(DS90UB9x.ENABLE, 1u);

                    EnableController(serializer);

                    // create RHD2000 register values
                    var adcMux = Rhd2000.ToAdcAndMuxBias(SamplesPerSecond.Value * Rhd2000PsbDecoder.NumAdcSamplesPerRoundRobbin);
                    var adcBuffBias = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_ADCBUFF, 0b00111111, (uint)adcMux[0]);
                    var muxBias = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_MUXBIAS, 0b00111111, (uint)adcMux[1]);

                    var format = Rhd2000PsbDecoder.DEFAULT_FORMAT;
                    format &= ~(1u << 6); // hard-code amplifier data format to offset binary

                    var dspCutoff = DspCutoff;
                    if (dspCutoff == Rhd2000DspCutoff.Off)
                    {
                        format &= ~(1u << 4);
                    }
                    else
                    {
                        format |= 1 << 4;
                        format &= ~0xFu;
                        format |= (uint)dspCutoff;
                    }

                    var highCutoff = Rhd2000.ToHighCutoffToRegisters(AnalogHighCutoff);
                    var lowCutoff = Rhd2000.ToLowCutoffToRegisters(AnalogLowCutoff);
                    var bw0 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW0, 0b00111111, (uint)highCutoff[0]);
                    var bw1 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW1, 0b00011111, (uint)highCutoff[1]);
                    var bw2 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW2, 0b00111111, (uint)highCutoff[2]);
                    var bw3 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW3, 0b00011111, (uint)highCutoff[3]);
                    var bw4 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW4, 0b01111111, (uint)lowCutoff[0]);
                    var bw5 = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW5, 0b01111111, ((uint)lowCutoff[2] << 6) & 0b01000000 |
                                                              (uint)lowCutoff[1] & 0b00111111);

                    // perform RHD2000 configuration
                    var i2c = new I2CRegisterContext(device, Rhd2000PsbDecoder.I2CAddress);
                    i2c.WriteByte(Rhd2000.ADCCONF, Rhd2000PsbDecoder.DEFAULT_ADCCONF);
                    i2c.WriteByte(Rhd2000.ADCBUFF, adcBuffBias);
                    i2c.WriteByte(Rhd2000.MUXBIAS, muxBias);
                    i2c.WriteByte(Rhd2000.MUXLOAD, Rhd2000PsbDecoder.DEFAULT_MUXLOAD);
                    i2c.WriteByte(Rhd2000.FORMAT, format);
                    i2c.WriteByte(Rhd2000.ZCHECK, Rhd2000PsbDecoder.DEFAULT_ZCHECK);
                    i2c.WriteByte(Rhd2000.ZDAC, Rhd2000PsbDecoder.DEFAULT_ZDAC);
                    i2c.WriteByte(Rhd2000.ZSELECT, Rhd2000PsbDecoder.DEFAULT_ZSELECT);
                    i2c.WriteByte(Rhd2000.BW0, bw0);
                    i2c.WriteByte(Rhd2000.BW1, bw1);
                    i2c.WriteByte(Rhd2000.BW2, bw2);
                    i2c.WriteByte(Rhd2000.BW3, bw3);
                    i2c.WriteByte(Rhd2000.BW4, bw4);
                    i2c.WriteByte(Rhd2000.BW5, bw5);
                    i2c.WriteByte(Rhd2000.PWR0, Rhd2000PsbDecoder.DEFAULT_PWR0);
                    i2c.WriteByte(Rhd2000.PWR1, Rhd2000PsbDecoder.DEFAULT_PWR1);
                    i2c.WriteByte(Rhd2000.PWR2, Rhd2000PsbDecoder.DEFAULT_PWR2);
                    i2c.WriteByte(Rhd2000.PWR3, Rhd2000PsbDecoder.DEFAULT_PWR3);

                    i2c.WriteByte(Rhd2000PsbDecoder.ENABLE, enable ? 1u : 0);
                    i2c.WriteByte(Rhd2000PsbDecoder.CLKDIVIDER, Rhd2000PsbDecoderSampleRate.ToRegister(SamplesPerSecond.Value));
                }

                var deviceInfo = new Rhd2000PsbDecoderDeviceInfo(context, DeviceType, deviceAddress, streamIndex, Rhd2000.ToEphysChannelCount(chip));
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }

    static class Rhd2000PsbDecoder
    {
        public const int I2CAddress = 0x60;

        public const uint CLKDIVIDER = 0x80; // 0 = bypass, N = Fs/N+1
        public const uint ENABLE = 0x81;
        public const uint MAX41400GAIN = 0x82;

        public const int NumAdcSamplesPerRoundRobbin = 34; // Number of ADC samples transactions per channel round robbin
                                                           // (aux 2/3 are sampled every other round robbin; 16 dummy cycles are performed on Rhd2216)
        public const int FramesPerSuperFrame = 16; // Total number of frames per superframe
        public const int FramesForEphysData = 4; // Number of frames used to encode ephys data
        public const int EphysChannelsPerFrame = 8; // Number of ephys channels embedded in each frame
        public const int WordsPerFrame = 36; // Number of 16-bit words in each frame
        public const int TrashWords = 4; // Number of magic 16-bit words at the start of each frame
        public const int SampleWords = 4; // Number of 16-bit words used to encode each 16-bit sample (ephys or aux)

        public const uint DEFAULT_ADCCONF = 0b11011110;
        public const uint DEFAULT_ADCBUFF = 0b00000010;
        public const uint DEFAULT_MUXBIAS = 0b00000100;
        public const uint DEFAULT_MUXLOAD = 0b00000010;
        public const uint DEFAULT_FORMAT = 0b00011111;
        public const uint DEFAULT_ZCHECK = 0b00000000;
        public const uint DEFAULT_ZDAC = 0b00000000;
        public const uint DEFAULT_ZSELECT = 0b00000000;
        public const uint DEFAULT_BW0 = 0b00010001;
        public const uint DEFAULT_BW1 = 0b10000000;
        public const uint DEFAULT_BW2 = 0b00010000;
        public const uint DEFAULT_BW3 = 0b10000000;
        public const uint DEFAULT_BW4 = 0b00010000;
        public const uint DEFAULT_BW5 = 0b11011100;
        public const uint DEFAULT_PWR0 = 0b11111111;
        public const uint DEFAULT_PWR1 = 0b11111111;
        public const uint DEFAULT_PWR2 = 0b11111111;
        public const uint DEFAULT_PWR3 = 0b11111111;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhd2000PsbDecoder))
            {
            }
        }
    }
}
