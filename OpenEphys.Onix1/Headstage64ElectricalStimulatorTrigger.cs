using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Trigger a headstage-64 onboard electrical stimulus sequencer.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64ElectricalStimulator"/>, using a shared <c>DeviceName</c>.
    /// Headstage-64's onboard electrical stimulator can be used to deliver current controlled
    /// micro-stimulation through a contact on the probe connector on the bottom of the headstage or the
    /// corresponding contact on a compatible electrode interface board.
    /// </remarks>
    [Description("Controls a headstage-64 onboard electrical stimulus sequencer.")]
    public class Headstage64ElectricalStimulatorTrigger : Sink<double>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Start an electrical stimulus sequence with an optional hardware delay.
        /// </summary>
        /// <param name="source">A sequence of double values that serve as a combined stimulus trigger and
        /// delay in microseconds. A value of 0 results in immediate stimulus delivery. A value of 100 results in
        /// stimulus delivery following a 100 microsecond delay. Delays are implemented in hardware and are
        /// exact. </param>
        /// <returns>A sequence of double values that is identical to <paramref name="source"/></returns>
        public override IObservable<double> Process(IObservable<double> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<double>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                    var triggerObserver = Observer.Create<double>(
                        value =>
                        {
                            device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, (uint)value << 8 | 0x1);
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return source.SubscribeSafe(triggerObserver);
                }));
        }
    }
}
