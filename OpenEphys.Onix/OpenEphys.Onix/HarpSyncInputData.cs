using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that generates a sequence of Harp clock synchronization events produced by
    /// the Harp sync input device in the ONIX breakout board.
    /// </summary>
    /// <inheritdoc cref = "ConfigureHarpSyncInput"/>
    [Description("Generates a sequence of Harp clock synchronization events produced by the Harp sync input device in the ONIX breakout board.")]
    public class HarpSyncInputData : Source<HarpSyncInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(HarpSyncInput.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="HarpSyncInputDataFrame"/> objects, each of which contains
        /// information about a single Harp clock synchronization event.
        /// </summary>
        /// <returns>A sequence of <see cref="HarpSyncInputDataFrame"/> objects.</returns>
        public override IObservable<HarpSyncInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(HarpSyncInput));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new HarpSyncInputDataFrame(frame));
            });
        }
    }
}
