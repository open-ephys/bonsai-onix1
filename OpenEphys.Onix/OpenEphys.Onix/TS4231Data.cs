using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that produces a sequence of 3D position measurements produced by an array of Triad TS4231 receivers.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureTS4231"/>,
    /// in order to stream 3D position data.
    /// </remarks>
    public class TS4231Data : Source<TS4231DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(TS4231.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="TS4231DataFrame"/> objects, each of which contains a 3D position sample
        /// from a single TS4231 receiver.
        /// </summary>
        /// <returns>A sequence of <see cref="TS4231DataFrame"/> objects.</returns>
        public override IObservable<TS4231DataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(TS4231));
                return deviceInfo.Context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new TS4231DataFrame(frame));
            });
        }
    }
}
