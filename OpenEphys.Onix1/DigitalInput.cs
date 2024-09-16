using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    [Obsolete]
    public class BreakoutDigitalInput : DigitalInput { }

    /// <summary>
    /// Produces a sequence of digital input data from an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureDigitalIO"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of digital input frames from an ONIX breakout board.")]
    public class DigitalInput : Source<DigitalInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of digital input data frames, each of which contains information about
        /// breakout board's digital input state.
        /// </summary>
        /// <remarks>
        /// Digital inputs are sampled at 4 MHz but a <see cref="DigitalInputDataFrame"/> is produced
        /// only when a button, switch, or digital input pin is toggled.
        /// </remarks>
        /// <returns>A sequence of <see cref="DigitalInputDataFrame"/> objects.</returns>
        public unsafe override IObservable<DigitalInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new DigitalInputDataFrame(frame));
            });
        }
    }
}
