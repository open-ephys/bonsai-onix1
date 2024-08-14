using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that produces a sequence of port status information.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate headstage, miniscope, or similar configuration.
    /// </remarks>
    [Description("Produces a sequence of port status information.")]
    public class PortStatus : Source<PortStatusFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(FmcLinkController.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="MemoryMonitorDataFrame"/> objects, which contains information
        /// about the system's low-level first-in, first-out (FIFO) data buffer.
        /// </summary>
        /// <returns>A sequence of <see cref="MemoryMonitorDataFrame"/> objects.</returns>
        public override IObservable<PortStatusFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(FmcLinkController));

                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new PortStatusFrame(frame));
            });
        }
    }
}
