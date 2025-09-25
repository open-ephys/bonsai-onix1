using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see
    /// cref="Headstage64OpticalStimulatorDataFrame">Headstage64OpticalStimulatorDataFrames</see> indicating
    /// the time and parameters of stimuli.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64OpticalStimulator"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of stimulus reports containing time, trigger origin, and parameters of stimuli delivered by the headstage-64 onboard optical stimulator.")]
    public class Headstage64OpticalStimulatorData : Source<Headstage64OpticalStimulatorDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64OpticalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see
        /// cref="Headstage64OpticalStimulatorDataFrame">Headstage64OpticalStimulatorDataFrames</see>.
        /// </summary>
        /// <returns>A sequence of <see
        /// cref="Headstage64OpticalStimulatorDataFrame">Headstage64OpticalStimulatorDataFrames</see>.</returns>
        public override IObservable<Headstage64OpticalStimulatorDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Headstage64OpticalStimulator));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new Headstage64OpticalStimulatorDataFrame(frame));
            });
        }
    }
}
