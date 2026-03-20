using System;
using System.ComponentModel;
using System.Reactive.Subjects;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a <see cref="ConfigureRhd2000PsbDecoderWithMax41400"/> with an added Max41400-based
    /// thermistor amplifier circuit that is routed to Rhd2000 Aux1 input.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>.
    /// </remarks>
    [Description("Configures a secondary NeuropixelsV2 device.")]
    public class ConfigureRhd2000PsbDecoderWithMax41400 : ConfigureRhd2000PsbDecoder
    {

        readonly BehaviorSubject<Max41400AnlGain> max41400Gain= new(Max41400AnlGain.x10);

        /// <summary>
        /// Gets or sets the Max41400 gain.
        /// </summary>
        [Description("Thermistor amplifier gain.")]
        [Category(AcquisitionCategory)]
        public Max41400AnlGain ThermistorAmplifierGain
        {
            get => max41400Gain.Value;
            set => max41400Gain.OnNext(value);
        }

        /// <summary>
        /// Configures a Rhd2000 electrophysiology chip and Max41400-based thermistor amplifier.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration
        /// actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure a
        /// Rhd2000 and Max41400-based thermistor amplifier.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            const uint MAX41400GAIN = 0x82;
            var deviceAddress = DeviceAddress;

            static byte ToRegiser(Max41400AnlGain gain) => gain switch
            {
                Max41400AnlGain.x10 => 0b00,
                Max41400AnlGain.x40 => 0b01,
                Max41400AnlGain.x100 => 0b10,
                Max41400AnlGain.x200 => 0b11,
                _ => throw new ArgumentOutOfRangeException(nameof(gain), $"Invalid Max41400 gain: {gain}"),
            };

            return base.Process(source).ConfigureAndLatchDevice((context, observer) =>
            {
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var i2c = new I2CRegisterContext(device, Rhd2000PsbDecoder.I2CAddress);
                return max41400Gain.SubscribeSafe(observer, value => i2c.WriteByte(MAX41400GAIN, ToRegiser(value)));
            });
        }
    }

    /// <summary>
    /// Specifies the programmable gain of a 9-bump wafer-level-packaged Max41400 instrumentation amplifier.
    /// </summary>
    public enum Max41400AnlGain
    {
        /// <summary>
        /// Specifies a gain of 10.
        /// </summary>
        x10,
        /// <summary>
        /// Specifies a gain of 40.
        /// </summary>
        x40,
        /// <summary>
        /// Specifies a gain of 100.
        /// </summary>
        x100,
        /// <summary>
        /// Specifies a gain of 200.
        /// </summary>
        x200
    }
}
