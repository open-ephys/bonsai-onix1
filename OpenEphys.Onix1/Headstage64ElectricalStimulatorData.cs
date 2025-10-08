using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see
    /// cref="Headstage64ElectricalStimulatorDataFrame">Headstage64ElectricalStimulatorDataFrames</see>
    /// objects indicating the time and parameters of stimuli.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64ElectricalStimulator"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of stimulus reports containing time, trigger origin, and parameters of stimuli delivered by the headstage-64 onboard electrical stimulator.")]
    public class Headstage64ElectricalStimulatorData : Source<Headstage64ElectricalStimulatorDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see
        /// cref="Headstage64ElectricalStimulatorDataFrame">Headstage64ElectricalStimulatorDataFrames</see>.
        /// </summary>
        /// <returns>A sequence of <see
        /// cref="Headstage64ElectricalStimulatorDataFrame">Headstage64ElectricalStimulatorDataFrames</see>.</returns>
        public override IObservable<Headstage64ElectricalStimulatorDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new Headstage64ElectricalStimulatorDataFrame(frame));
            });
        }
    }
}
