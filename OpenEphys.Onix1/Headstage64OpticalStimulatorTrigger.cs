using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Trigger a headstage-64 onboard optical stimulus sequencer.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64OpticalStimulator"/>, using a shared <c>DeviceName</c>.
    /// Headstage-64's onboard optical stimulator can be used to drive current through laser diodes or LEDs
    /// connected to two contacts on the probe connector on the bottom of the headstage or the corresponding
    /// contacts on a compatible electrode interface board.
    /// </remarks>
    [Description("Controls a headstage-64 onboard optical stimulus sequencer.")]
    public class Headstage64OpticalStimulatorTrigger : Sink<bool>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64OpticalStimulator.NameConverter))]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Start an optical stimulus sequence.
        /// </summary>
        /// <param name="source">A sequence of boolean values indicating the start of a stimulus sequence when true.</param>
        /// <returns>A sequence of boolean values that is identical to <paramref name="source"/></returns>
        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<bool>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Headstage64OpticalStimulator));
                    var triggerObserver = Observer.Create<bool>(
                        value =>
                        {
                            device.WriteRegister(Headstage64OpticalStimulator.TRIGGER, value ? 1u : 0u);
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return source.SubscribeSafe(triggerObserver);
                }));
        }
    }
}
