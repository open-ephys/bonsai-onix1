using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that produces a sequence of digital input frames from an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureBreakoutDigitalIO"/>,
    /// in order to stream data.
    /// </remarks>
    [Description("Produces a sequence of digital input frames from an ONIX breakout board.")]
    public class BreakoutDigitalInput : Source<BreakoutDigitalInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(BreakoutDigitalIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="BreakoutDigitalInputDataFrame"/> objects, which contains information about breakout
        /// board's digital input state.
        /// </summary>
        /// <remarks>
        /// Digital inputs are not regularly sampled. Instead, a new <see cref="BreakoutDigitalInputDataFrame"/> is produced each
        /// whenever any digital state (i.e. a digital input pin, button, or switch state) changes.
        /// </remarks>
        /// <returns>A sequence of <see cref="BreakoutDigitalInputDataFrame"/> objects.</returns>
        public unsafe override IObservable<BreakoutDigitalInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutDigitalIO));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new BreakoutDigitalInputDataFrame(frame));
            });
        }
    }
}
