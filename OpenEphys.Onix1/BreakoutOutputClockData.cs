using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{

    /// <summary>
    /// Produces a sequence with a single element containing the output clock's exact hardware parameters.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureBreakoutOutputClock"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of analog input frames from an ONIX breakout board.")]
    public class BreakoutOutputClockData : Source<BreakoutOutputClockParameters>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(BreakoutOutputClock.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence containing a single <see cref="BreakoutOutputClockParameters"/>.
        /// </summary>
        /// <returns>A sequence containing a single <see cref="BreakoutOutputClockParameters"/></returns>
        public unsafe override IObservable<BreakoutOutputClockParameters> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo =>
                {
                    var clockOutDeviceInfo = (BreakoutOutputClockDeviceInfo)deviceInfo;
                    return Observable.Defer(() => Observable.Return(clockOutDeviceInfo.Parameters));
                });
        }
    }
}
