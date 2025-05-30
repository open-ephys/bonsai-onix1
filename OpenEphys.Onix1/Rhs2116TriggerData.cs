using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Rhs2116TriggerDataFrame"/> objects indicating the time and
    /// status of stimulus trigger events.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureRhs2116Trigger"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Rhs2116TriggerDataFrame objects indicating the time and status of stimulus trigger events.")]
    public class Rhs2116TriggerData : Source<Rhs2116TriggerDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhs2116Trigger.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="Rhs2116TriggerDataFrame"/>s.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhs2116TriggerDataFrame"/>s.</returns>
        public override IObservable<Rhs2116TriggerDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new Rhs2116TriggerDataFrame(frame));
            });
        }
    }
}
