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
    /// Controls a headstage-64 onboard electrical stimulus sequencer.
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

        readonly BehaviorSubject<bool> stimEnable = new(false);

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Headstage64ElectricalStimulator.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator's ±15V power supplies will be turned on and the
        /// electrical stimulator circuit will respect triggers. If set to false, the power supplies will be
        /// shut down and triggers will be ignored.It may be desirable to power down the electrical
        /// stimulator's power supplies outside of stimulation windows to reduce power consumption and
        /// electrical noise. This property must be set to true in order for electrical stimuli to be
        /// delivered properly. It takes ~10 milliseconds for these supplies to stabilize.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(DeviceFactory.AcquisitionCategory)]
        public bool Enable
        {
            get => stimEnable.Value;
            set => stimEnable.OnNext(value);
        }

        /// <summary>
        /// Start an electrical stimulus sequence with an optional hardware delay.
        /// </summary>
        /// <param name="source">A sequence of doubles that serve as a combined stimulus trigger and
        /// delay in microseconds. A value of 0 results in immediate stimulus delivery. A value of 100 results in
        /// stimulus delivery following a 100 microsecond delay. Delays are implemented in hardware and are
        /// exact. </param>
        /// <returns>A sequence of doubles that is identical to <paramref name="source"/></returns>
        public override IObservable<double> Process(IObservable<double> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<double>(observer =>
                {
                    var info = (Headstage64StimulatorDeviceInfo)deviceInfo;
                    var device = info.GetDeviceContext(typeof(Headstage64ElectricalStimulator));
                    IObserver<double> triggerObserver;

                    if (info.PortControllerAddress != null)
                    {
                        var portController = device.Context.GetDeviceContext((uint)info.PortControllerAddress, typeof(PortController));
                        triggerObserver = Observer.Create<double>(value => {
                            if (stimEnable.Value)
                            {
                                if (value == 0)
                                {
                                    portController.WriteRegister(PortController.GPOSTATE, (byte)PortControllerGpioState.Pin1);
                                    portController.WriteRegister(PortController.GPOSTATE, 0);
                                }
                                else
                                {
                                    device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, (uint)value << 8 | 0x1);
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
                                device.WriteRegister(Headstage64ElectricalStimulator.TRIGGER, (uint)value << 8 | 0x1);
                            }
                            observer.OnNext(value);
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    }

                    return new CompositeDisposable(
                          stimEnable.SubscribeSafe(observer, value =>
                              device.WriteRegister(Headstage64ElectricalStimulator.STIMENABLE, value ? 3u : 0u)),
                          source.SubscribeSafe(triggerObserver));
                }));
        }
    }
}
