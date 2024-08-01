using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that produces a sequence of memory usage data frames.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureMemoryMonitor"/>,
    /// in order to stream data.
    /// </remarks>
    [Description("Produces a sequence of memory usage data frames.")]
    public class MemoryMonitorData : Source<MemoryMonitorDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(MemoryMonitor.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="MemoryMonitorDataFrame"/> objects, which contains information
        /// about the system's low-level first-in, first-out (FIFO) data buffer.
        /// </summary>
        /// <returns>A sequence of <see cref="MemoryMonitorDataFrame"/> objects.</returns>
        public override IObservable<MemoryMonitorDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(MemoryMonitor));
                var totalMemory = device.ReadRegister(MemoryMonitor.TOTAL_MEM);

                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new MemoryMonitorDataFrame(frame, totalMemory));
            });
        }
    }
}
