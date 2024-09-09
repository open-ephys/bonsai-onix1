using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of 3D orientation measurements produced by Bno055 9-axis inertial measurement unit.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration operator, such as a <see
    /// cref="ConfigureBno055"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of 3D orientation measurements produced by a Bno055 9-axis inertial measurement unit.")]
    public class Bno055Data : Source<Bno055DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Bno055.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame"/> objects, each of which contains a 3D orientation sample
        /// in various formats along with device metadata.
        /// </summary>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/> objects.</returns>
        public override IObservable<Bno055DataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Bno055));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new Bno055DataFrame(frame));
            });
        }
    }
}
