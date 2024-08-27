using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of digital events from an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data stream operator must be linked to an appropriate configuration, such as a
    /// <see cref="ConfigureBreakoutDigitalIO"/>, in order to stream data.
    /// </remarks>
    [Description("Produces a sequence of digital input frames from an ONIX breakout board.")]
    public class BreakoutDigitalInput : Source<BreakoutDigitalInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(BreakoutDigitalIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of  events, each of which contain information about breakout
        /// board's digital input state.
        /// </summary>
        /// <remarks>
        /// Digital inputs are sampled at 5 MHz but a <see cref="BreakoutDigitalInputDataFrame"/> is only propagated if a change
        /// in digital state has occurred. Changes in digital state include:
        /// <list type="bullet">
        /// <item>
        /// <description>A digital input pin is toggled</description>
        /// </item>
        /// <item>
        /// <description>A button is pressed or released</description>
        /// </item>
        /// <item>
        /// <description>A switch is flipped</description>
        /// </item>
        /// </list>
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
