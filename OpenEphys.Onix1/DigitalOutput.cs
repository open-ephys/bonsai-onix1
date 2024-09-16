using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    [Obsolete]
    public class BreakoutDigitalOutput : DigitalOutput { }

    /// <summary>
    /// Sends digital output data to an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureDigitalIO"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Sends digital output data to an ONIX breakout board.")]
    public class DigitalOutput : Sink<DigitalPortState>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Updates the digital output port state.
        /// </summary>
        /// <param name="source"> A sequence of <see cref="DigitalPortState"/> values indicating the state of the breakout board's 8 digital output pins</param>
        /// <returns> A sequence that is identical to <paramref name="source"/>.</returns>
        public override IObservable<DigitalPortState> Process(IObservable<DigitalPortState> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                return source.Do(value => device.Write((uint)value));
            });
        }
    }
}
