using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a synchronized pair of Intan Rhs2116 bioamplifier and stimulator chips.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="Rhs2116Data"/>,
    /// using a shared <c>DeviceName</c>.
    /// </remarks>
    public class ConfigureRhs2116Pair : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoff = new(Rhs2116AnalogLowCutoff.Low100mHz);
        readonly BehaviorSubject<Rhs2116AnalogLowCutoff> analogLowCutoffRecovery = new(Rhs2116AnalogLowCutoff.Low250Hz);
        readonly BehaviorSubject<Rhs2116AnalogHighCutoff> analogHighCutoff = new(Rhs2116AnalogHighCutoff.High10000Hz);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureRhs2116Pair"/> class.
        /// </summary>
        public ConfigureRhs2116Pair()
            : base(typeof(Rhs2116Pair))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureRhs2116Pair"/> class with the given settings.
        /// </summary>
        /// <param name="rhs2116">Existing <see cref="ConfigureRhs2116Pair"/> device, whose settings will be copied.</param>
        public ConfigureRhs2116Pair(ConfigureRhs2116Pair rhs2116)
            : base(typeof(Rhs2116Pair))
        {
            Enable = rhs2116.Enable;
            DspCutoff = rhs2116.DspCutoff;
            AnalogLowCutoffRecovery = rhs2116.AnalogLowCutoffRecovery;
            AnalogLowCutoff = rhs2116.AnalogLowCutoff;
            AnalogHighCutoff = rhs2116.AnalogHighCutoff;
            DeviceAddress = rhs2116.DeviceAddress;
            DeviceName = rhs2116.DeviceName;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Rhs2116Data"/> will produce data. If set to false, 
        /// <see cref="Rhs2116Data"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Rhs2116 pair is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116DspCutoff"/> value.
        /// </summary>
        /// <remarks>
        /// Specifies the cutoff frequency for the DSP high-pass filter used for amplifier offset removal.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies the cutoff frequency for the DSP high-pass filter used for amplifier offset removal.")]
        public Rhs2116DspCutoff DspCutoff { get; set; } = Rhs2116DspCutoff.Off;

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116AnalogLowCutoff"/>.
        /// </summary>
        /// <remarks>
        /// Specifies the lower cutoff frequency of the pre-ADC amplifiers.
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoff
        {
            get => analogLowCutoff.Value;
            set => analogLowCutoff.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116AnalogLowCutoff"/> directly after stimulation occurs.
        /// </summary>
        /// <remarks>
        /// Specifies the lower cutoff frequency of the pre-ADC amplifiers during stimulus recovery.
        /// Note that this is only active for a short period of time, and reverts back to <see cref="AnalogLowCutoff"/>.
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("Specifies the lower cutoff frequency of the pre-ADC amplifiers during stimulus recovery.")]
        public Rhs2116AnalogLowCutoff AnalogLowCutoffRecovery
        {
            get => analogLowCutoffRecovery.Value;
            set => analogLowCutoffRecovery.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116AnalogHighCutoff"/>.
        /// </summary>
        /// <remarks>
        /// Specifies the upper cutoff frequency of the pre-ADC amplifiers.
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("Specifies the upper cutoff frequency of the pre-ADC amplifiers.")]
        public Rhs2116AnalogHighCutoff AnalogHighCutoff
        {
            get => analogHighCutoff.Value;
            set => analogHighCutoff.OnNext(value);
        }

        /// <summary>
        /// Configures an Rhs2116Pair device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// an Rhs2116Pair device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var dspCutoff = DspCutoff;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;

            return source.ConfigureDevice((context, observer) =>
            {
                var rhs2116A = context.GetDeviceContext(deviceAddress + Rhs2116Pair.Rhs2116AAddressOffset, typeof(Rhs2116));
                var rhs2116B = context.GetDeviceContext(deviceAddress + Rhs2116Pair.Rhs2116BAddressOffset, typeof(Rhs2116));

                static void ConfigureChip(DeviceContext device, bool enable, Rhs2116DspCutoff dspCutoff)
                {
                    var format = device.ReadRegister(Rhs2116.FORMAT);

                    if (dspCutoff == Rhs2116DspCutoff.Off)
                    {
                        format &= ~(1u << 4);
                    }
                    else
                    {
                        format |= 1 << 4;
                        format &= ~0xFu;
                        format |= (uint)dspCutoff;
                    }

                    // NB: DC data only provided in unsigned. Force amplifier data to use unsigned for
                    // consistency
                    device.WriteRegister(Rhs2116.FORMAT, format);
                    device.WriteRegister(Rhs2116.ENABLE, enable ? 1u : 0);
                }

                ConfigureChip(rhs2116A, enable, dspCutoff);
                ConfigureChip(rhs2116B, enable, dspCutoff);

                var deviceInfo = new Rhs2116PairDeviceInfo(
                    context,
                    rhs2116A,
                    rhs2116B,
                    typeof(Rhs2116Pair),
                    deviceAddress);

                return new CompositeDisposable(
                   analogLowCutoff.SubscribeSafe(observer, newValue =>
                   {
                       var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                       var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                       rhs2116A.WriteRegister(Rhs2116.BW2, reg);
                       rhs2116B.WriteRegister(Rhs2116.BW2, reg);
                   }),
                   analogLowCutoffRecovery.SubscribeSafe(observer, newValue =>
                   {
                       var regs = Rhs2116Config.AnalogLowCutoffToRegisters[newValue];
                       var reg = regs[2] << 13 | regs[1] << 7 | regs[0];
                       rhs2116A.WriteRegister(Rhs2116.BW3, reg);
                       rhs2116B.WriteRegister(Rhs2116.BW3, reg);
                   }),
                   analogHighCutoff.SubscribeSafe(observer, newValue =>
                   {
                       var regs = Rhs2116Config.AnalogHighCutoffToRegisters[newValue];
                       rhs2116A.WriteRegister(Rhs2116.BW0, regs[1] << 6 | regs[0]);
                       rhs2116A.WriteRegister(Rhs2116.BW1, regs[3] << 6 | regs[2]);
                       rhs2116A.WriteRegister(Rhs2116.FASTSETTLESAMPLES, Rhs2116Config.AnalogHighCutoffToFastSettleSamples[newValue]);
                       rhs2116B.WriteRegister(Rhs2116.BW0, regs[1] << 6 | regs[0]);
                       rhs2116B.WriteRegister(Rhs2116.BW1, regs[3] << 6 | regs[2]);
                       rhs2116B.WriteRegister(Rhs2116.FASTSETTLESAMPLES, Rhs2116Config.AnalogHighCutoffToFastSettleSamples[newValue]);
                   }),
                   DeviceManager.RegisterDevice(deviceName, deviceInfo)
               );
            });
        }
    }

    class Rhs2116PairDeviceInfo : DeviceInfo
    {
        public Rhs2116PairDeviceInfo(
            ContextTask context,
            DeviceContext rhs2116A,
            DeviceContext rhs2116B,
            Type deviceType,
            uint deviceAddress)
            : base(context, deviceType, deviceAddress)
        {
            Rhs2116A = rhs2116A;
            Rhs2116B = rhs2116B;
        }

        public DeviceContext Rhs2116A { get; }

        public DeviceContext Rhs2116B { get; }
    }

    static class Rhs2116Pair
    {

        public const int Rhs2116AAddressOffset = 0;
        public const int Rhs2116BAddressOffset = 1;

        public const int ChannelsPerChip = 16;
        public const int TotalChannels = 32;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116Pair))
            {
            }
        }
    }

}
