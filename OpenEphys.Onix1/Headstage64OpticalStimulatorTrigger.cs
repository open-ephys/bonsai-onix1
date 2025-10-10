using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        readonly BehaviorSubject<bool> stimEnable = new(false);

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64OpticalStimulator.NameConverter))]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the optical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the optical stimulator will respect triggers.")]
        [Category(DeviceFactory.AcquisitionCategory)]
        public bool StimEnable
        {
            get => stimEnable.Value;
            set => stimEnable.OnNext(value);
        }

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
                    var info = (Headstage64StimulatorDeviceInfo)deviceInfo;
                    var device = info.GetDeviceContext(typeof(Headstage64OpticalStimulator));
                    IObserver<double> triggerObserver;

                    if (info.PortControllerAddress != null)
                    {
                        var portController = device.Context.GetDeviceContext((uint)info.PortControllerAddress, typeof(PortController));
                        triggerObserver = Observer.Create<double>(value =>
                        {
                            if (stimEnable.Value)
                            {
                                if (value == 0)
                                {
                                    portController.WriteRegister(PortController.GPOSTATE, (byte)PortControllerGpioState.Pin1);
                                    portController.WriteRegister(PortController.GPOSTATE, 0);
                                }
                                else
                                {
                                    device.WriteRegister(Headstage64OpticalStimulator.TRIGGER, (uint)value << 8 | 0x1);
                            }
                            }
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    }
                    else
                    {
                        triggerObserver = Observer.Create<double>(value => {
                            if (stimEnable.Value)
                            {
                                device.WriteRegister(Headstage64OpticalStimulator.TRIGGER, (uint)value << 8 | 0x1);
                            }
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    }

                    return new CompositeDisposable(stimEnable.SubscribeSafe(observer, value =>
                    {
                        var stimEnableValue = device.ReadRegister(Headstage64OpticalStimulator.STIMENABLE);
                        if (value)
                            stimEnableValue |= 1u;
                        else
                            stimEnableValue &= ~1u;
                        device.WriteRegister(Headstage64OpticalStimulator.STIMENABLE, stimEnableValue);
                    }),
                    source.SubscribeSafe(triggerObserver));

                }));
        }
    }
}
