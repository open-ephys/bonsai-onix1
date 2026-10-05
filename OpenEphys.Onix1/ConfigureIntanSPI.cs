using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an Intan SPI decoder device.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageIntanSPI"/>.
    /// </remarks>
    [Description("Configures an Intan SPI decoderdevice")]
    public class ConfigureIntanSPI : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhd2000DspCutoff> dspCutoff = new(Rhd2000DspCutoff.Off);
        readonly BehaviorSubject<Rhd2000AnalogLowCutoff> analogLowCutoff = new(Rhd2000AnalogLowCutoff.Low100mHz);
        readonly BehaviorSubject<Rhd2000AnalogHighCutoff> analogHighCutoff = new(Rhd2000AnalogHighCutoff.High10000Hz);
        readonly BehaviorSubject<bool> externalAnalogFilter = new(false);
        readonly BehaviorSubject<Rhd2000DigitalOutState> digitalOutState = new(Rhd2000DigitalOutState.HighZ);

        public ConfigureIntanSPI()
            : base(typeof(IntanSPI))
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
        [Category(AcquisitionCategory)]
        [Description("Specifies the cutoff frequency for the digital (post-ADC) high-pass filter used for amplifier offset removal.")]
        public Rhd2000DspCutoff DspCutoff
        {
            get => dspCutoff.Value;
            set => dspCutoff.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the low cutoff frequency of the analog (pre-ADC) bandpass filter.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Specifies the low cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2000AnalogLowCutoff AnalogLowCutoff
        {
            get => analogLowCutoff.Value;
            set => analogLowCutoff.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the high cutoff frequency of the analog (pre-ADC) bandpass filter.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Specifies the high cutoff frequency of the analog (pre-ADC) bandpass filter.")]
        public Rhd2000AnalogHighCutoff AnalogHighCutoff
        {
            get => analogHighCutoff.Value;
            set => analogHighCutoff.OnNext(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use an external analog filter instead of the internal analog filter.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="AnalogHighCutoff"/> and <see cref="AnalogLowCutoff"/> will be ignored and the external analog filter will be used instead.
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("Specifies whether to use an external analog filter instead of the internal analog filter.")]
        public bool ExternalAnalogFilter
        {
            get => externalAnalogFilter.Value;
            set => externalAnalogFilter.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the digital output state of the RHD2000 chip.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Specifies the digital output state of the RHD2000 chip.")]
        public Rhd2000DigitalOutState DigitalOutState
        {
            get => digitalOutState.Value;
            set => digitalOutState.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="IntanSPIData"/> will produce data. If set to false, 
        /// <see cref="IntanSPIData "/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Intan SPI device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var samplesPerSecond = SamplesPerSecond;
            return source.ConfigureAndLatchDevice(context =>
            {
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var disposables = new List<IDisposable>();
                Rhd2000ChipId rhdMiso1 = 0;
                Rhd2000ChipId rhdMiso2 = 0;

                // NB: any I2C transaction can throw, including those triggered by the initial value of each
                // subscription. Dispose whatever has been created so far so that no subscription is left dangling.
                try
                {
                    if (enable)
                    {
                        device.WriteRegister(DS90UB9x.ENABLE, 1u);
                        var i2c = new I2CRegisterContext(device, IntanSPI.I2CAddress);

                        var adcMux = Rhd2000.ToAdcAndMuxBias(samplesPerSecond.Value * IntanSPI.NumAdcSamplesPerRoundRobbin);
                        var adcBuffBias = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_ADCBUFF, 0b00111111, (uint)adcMux[0]);
                        var muxBias = BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_MUXBIAS, 0b00111111, (uint)adcMux[1]);

                        i2c.WriteByte(IntanSPI.CLK_DIV, Rhd2000PsbDecoderSampleRate.ToRegister(samplesPerSecond.Value));
                        i2c.WriteByte(IntanSPI.ADC_BIAS, adcBuffBias);
                        i2c.WriteByte(IntanSPI.MUX_BIAS, muxBias);
                        i2c.WriteByte(IntanSPI.FAST_SETTLE, 0u);

                        disposables.Add(dspCutoff.Subscribe(value => SetDspCutoff(i2c, value)));
                        disposables.Add(analogHighCutoff.Subscribe(value => SetAnalogHighCutoff(i2c, value)));
                        disposables.Add(analogLowCutoff.Subscribe(value => SetAnalogLowCutoff(i2c, value)));
                        disposables.Add(externalAnalogFilter.Subscribe(value => i2c.WriteByte(IntanSPI.EXT_FILTER, value ? 1u : 0u)));
                        disposables.Add(digitalOutState.Subscribe(value => SetDigitalOutState(i2c, value)));

                        i2c.WriteByte(IntanSPI.SYS_ENABLE, 1u);

                        // NB: the chip IDs read 0xFF while the device is still scanning for chips. Both registers
                        // change together, so only one needs to be polled. If the scan never finishes, assume
                        // that no chips are present.
                        var stopwatch = Stopwatch.StartNew();
                        byte id1;
                        while ((id1 = i2c.ReadByte(IntanSPI.RHD_ID_1)) == IntanSPI.ScanningChipId &&
                               stopwatch.ElapsedMilliseconds < IntanSPI.ChipScanTimeoutMilliseconds)
                        {
                            // NB: Wait for the scan to finish. The Intan SPI device will return 0xFF for both chip ID registers while scanning.
                        }

                        byte id2 = i2c.ReadByte(IntanSPI.RHD_ID_2);
                        rhdMiso1 = id1 == IntanSPI.ScanningChipId ? (Rhd2000ChipId)0 : (Rhd2000ChipId)id1;
                        rhdMiso2 = id2 == IntanSPI.ScanningChipId ? (Rhd2000ChipId)0 : (Rhd2000ChipId)id2;

                        if (rhdMiso1 == 0 && rhdMiso2 == 0)
                        {
                            throw new InvalidOperationException(
                                $"No Rhd2000 chip was detected by the Intan SPI device \"{deviceName}\".");
                        }

                        i2c.WriteByte(IntanSPI.DATA_ENABLE, 1u);
                    }

                    var deviceInfo = new IntanSPIDeviceInfo(context, DeviceType, deviceAddress, rhdMiso1, rhdMiso2);
                    disposables.Add(DeviceManager.RegisterDevice(deviceName, deviceInfo));
                }
                catch
                {
                    foreach (var disposable in disposables)
                    {
                        disposable.Dispose();
                    }

                    throw;
                }

                return new CompositeDisposable(disposables);
            });
        }

        static void SetDspCutoff(I2CRegisterContext i2c, Rhd2000DspCutoff cutoff)
        {
            uint value = cutoff == Rhd2000DspCutoff.Off ? 0u : (1u << 4) | (uint)cutoff;
            i2c.WriteByte(IntanSPI.DSP, value);
        }

        static void SetAnalogHighCutoff(I2CRegisterContext i2c, Rhd2000AnalogHighCutoff cutoff)
        {
            var highCutoff = Rhd2000.ToHighCutoffToRegisters(cutoff);
            i2c.WriteByte(IntanSPI.RH1_DAC1, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW0, 0b00111111, (uint)highCutoff[0]));
            i2c.WriteByte(IntanSPI.RH1_DAC2, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW1, 0b00011111, (uint)highCutoff[1]));
            i2c.WriteByte(IntanSPI.RH2_DAC1, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW2, 0b00111111, (uint)highCutoff[2]));
            i2c.WriteByte(IntanSPI.RH2_DAC2, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW3, 0b00011111, (uint)highCutoff[3]));
        }

        static void SetAnalogLowCutoff(I2CRegisterContext i2c, Rhd2000AnalogLowCutoff cutoff)
        {
            var lowCutoff = Rhd2000.ToLowCutoffToRegisters(cutoff);
            i2c.WriteByte(IntanSPI.RL_DAC1, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW4, 0b01111111, (uint)lowCutoff[0]));
            i2c.WriteByte(IntanSPI.RL_DAC23, BitHelper.Replace(Rhd2000PsbDecoder.DEFAULT_BW5, 0b01111111, ((uint)lowCutoff[2] << 6) & 0b01000000 |
                                                                                                         (uint)lowCutoff[1] & 0b00111111));
        }

        static void SetDigitalOutState(I2CRegisterContext i2c, Rhd2000DigitalOutState state)
        {
            uint value = state switch
            {
                Rhd2000DigitalOutState.Low => 0b00,
                Rhd2000DigitalOutState.High => 0b01,
                _ => 0b10
            };
            i2c.WriteByte(IntanSPI.DIGOUT, value);
        }
    }

    static class IntanSPI
    {
        public const int I2CAddress = 0x60;

        // After a hardware reset, the whole system is halt until this register is set to '1'.
        // System-level configuration should be done before setting this register to '1'.
        public const uint SYS_ENABLE = 0;
        // Enables RHD data streaming. Bit 0: '0' = RHD data streaming disabled, '1' = RHD data streaming enabled
        public const uint DATA_ENABLE = 1;
        // Chip id for RHD in MISO1
        public const uint RHD_ID_1 = 2;
        // Chip id for RHD in MISO2
        public const uint RHD_ID_2 = 3;
        // Clock divider to control sample rate. Actual divider us CLK_DIV + 1 (e.g. 0 is full clock)
        public const uint CLK_DIV = 4;
        // ADC Buffer bias. Bits 5:0 of RHD register 1
        public const uint ADC_BIAS = 5;
        // MUX bias. Bits 5:0 of RHD register 2
        public const uint MUX_BIAS = 6;
        // Bit 0: '0' = Normal operation '1' = enable amplifier fast settle
        public const uint FAST_SETTLE = 7;
        // Bit 0: '0' = Use internal analog filter resistors '1' = Use external resistors and disable aux inputs
        public const uint EXT_FILTER = 8;
        // High pass filter control. Bits 5:0 of RHD register 8
        public const uint RH1_DAC1 = 9;
        // High pass filter control. Bits 4:0 of RHD register 9
        public const uint RH1_DAC2 = 10;
        // High pass filter control. Bits 5:0 of RHD register 10
        public const uint RH2_DAC1 = 11;
        // High pass filter control. Bits 4:0 of RHD register 11
        public const uint RH2_DAC2 = 12;
        // Low pass filter control. Bits 6:0 of RHD register 12
        public const uint RL_DAC1 = 13;
        // High pass filter control. Bits 6:0 of RHD register 13
        public const uint RL_DAC23 = 14;
        // Bit 4: Enable DSP filter, Bits 3:0 DSP cutoff frequency. Bits 4:0 of RHD register 4
        public const uint DSP = 15;
        // Bit 1: '0' RHD digout enabled, '1' RHD digout in HiZ, Bit 0: digout value. Bits 1:0 of RHD register 3
        public const uint DIGOUT = 16;

        public const int NumAdcSamplesPerRoundRobbin = 34; //32 amplifiers + aux + cfg channel
        public const int FrameSizeBytes = NumAdcSamplesPerRoundRobbin * 8; // 16bit workds, 4 streams per frame (2 miso x 2 ddr)

        public const byte ScanningChipId = 0xFF; // Chip id register value while the chip scan is in progress
        public const int ChipScanTimeoutMilliseconds = 1000;
    }

}
