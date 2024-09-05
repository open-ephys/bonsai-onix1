using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of Harp clock synchronization signals sent to the Harp input in the ONIX breakout board.
    /// </summary>
    /// <inheritdoc cref = "ConfigureHarpSyncInput"/>
    [Description("Produces a sequence of Harp clock synchronization signals sent to the Harp input in the ONIX breakout board.")]
    public class HarpSyncInputData : Source<HarpSyncInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(HarpSyncInput.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="HarpSyncInputDataFrame">HarpSyncInputDataFrames</see>, each of
        /// which contains information about a single Harp clock synchronization event.
        /// </summary>
        /// <returns>A sequence of <see cref="HarpSyncInputDataFrame">HarpSyncInputDataFrames</see>.</returns>
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
