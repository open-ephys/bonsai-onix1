using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Controls a headstage-64 onboard optical stimulus sequencer.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64OpticalStimulator"/>, using a shared <c>DeviceName</c>.
    /// Headstage-64's onboard optical stimulator can be used to drive current through laser diodes or LEDs
    /// connected to two contacts on the probe connector on the bottom of the headstage or the corresponding
    /// contacts on a compatible electrode interface board.
    /// </remarks>
    [Description("Controls a headstage-64 onboard optical stimulus sequencer.")]
    public class Headstage64OpticalStimulatorTrigger : Sink<double>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64OpticalStimulator.NameConverter))]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Start an optical stimulus sequence with an optional hardware delay.
        /// </summary>
        /// <param name="source">A sequence of double values that serve as a combined stimulus trigger and
        /// delay in microseconds. For instance, a value of 0 results in immediate stimulus delivery, a value
        /// of 100 results in stimulus delivery following a 100 microsecond delay, etc. Delays are implemented in
        /// hardware and are exact. </param>
        /// <returns>A sequence of double values that is identical to <paramref name="source"/></returns>
        public override IObservable<double> Process(IObservable<double> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<double>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Headstage64OpticalStimulator));
                    var triggerObserver = Observer.Create<double>(
                        value =>
                        {
                            device.WriteRegister(Headstage64OpticalStimulator.TRIGGER, (uint)value << 8 | 0x1);
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return source.SubscribeSafe(triggerObserver);
                }));
        }
    }
}
