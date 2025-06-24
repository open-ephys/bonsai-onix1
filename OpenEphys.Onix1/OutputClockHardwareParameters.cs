using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence with a single element containing the output clock's exact hardware parameters for
    /// each subscription.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureOutputClock"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence with a single element containing the output clock's hardware parameters for each subscription.")]
    public class OutputClockHardwareParameters : Source<OutputClockParameters>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(OutputClock.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence containing a single <see cref="OutputClockParameters"/> structure.
        /// </summary>
        /// <returns>A sequence containing a single <see cref="OutputClockParameters"/></returns> structure.
        public override IObservable<OutputClockParameters> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo =>
                {
                    var clockOutDeviceInfo = (OutputClockDeviceInfo)deviceInfo;
                    return Observable.Defer(() => Observable.Return(clockOutDeviceInfo.Parameters));
                });
        }
    }

    /// <inheritdoc cref = "OutputClockHardwareParameters"/>v
    [Obsolete("This operator is obsolete. Use OutputClockHardwareParameters instead. Will be removed in version 1.0.0.")]
    public class OutputClockData : OutputClockHardwareParameters { }

}
