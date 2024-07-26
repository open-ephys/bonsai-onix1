using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Sends digital output data to an ONIX breakout board.
    /// </summary>
    public class BreakoutDigitalOutput : Sink<BreakoutDigitalPortState>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(BreakoutDigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Updates the digital output port state.
        /// </summary>
        /// <param name="source"> A sequence of <see cref="BreakoutDigitalPortState"/> values indicating the state of the breakout board's 8 digital output pins</param>
        /// <returns> A sequence that is identical to <paramref name="source"/>.</returns>
        public override IObservable<BreakoutDigitalPortState> Process(IObservable<BreakoutDigitalPortState> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutDigitalIO));
                return source.Do(value => device.Write((uint)value));
            });
        }
    }
}
