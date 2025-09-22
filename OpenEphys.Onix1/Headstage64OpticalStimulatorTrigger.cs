﻿using System;
using System.ComponentModel;
using System.Drawing.Design;
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
    public class Headstage64OpticalStimulatorTrigger : Sink<bool>
    {
        readonly BehaviorSubject<bool> enable = new(true);
        readonly BehaviorSubject<double> maxCurrent = new(100);
        readonly BehaviorSubject<double> channelOneCurrent = new(100);
        readonly BehaviorSubject<double> channelTwoCurrent = new(0);
        readonly BehaviorSubject<double> pulseDuration = new(5);
        readonly BehaviorSubject<double> pulsesPerSecond = new(50);
        readonly BehaviorSubject<uint> pulsesPerBurst = new(20);
        readonly BehaviorSubject<double> interBurstInterval = new(0);
        readonly BehaviorSubject<uint> burstsPerTrain = new(1);
        readonly BehaviorSubject<double> delay = new(0);

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
        public bool Enable
        {
            get => enable.Value;
            set => enable.OnNext(value);
        }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in msec.
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double Delay
        {
            get => delay.Value;
            set => delay.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the Maximum current per channel per pulse in mA.
        /// </summary>
        /// <remarks>
        /// This value defines the maximal possible current that can be delivered to each channel.
        /// To get different amplitudes for each channel use the <see cref="ChannelOneCurrent"/> and
        /// <see cref="ChannelTwoCurrent"/> properties.
        /// </remarks>
        [Description("Maximum current per channel per pulse (mA). " +
            "This value is used by both channels. To get different amplitudes " +
            "for each channel use the ChannelOneCurrent and ChannelTwoCurrent properties.")]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Range(0, 300)]
        [Precision(3, 0)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double MaxCurrent
        {
            get => maxCurrent.Value;
            set => maxCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 1 in each pulse.
        /// </summary>
        [Description("Channel 1 percent of MaxCurrent. If greater than 0, channel 1 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double ChannelOneCurrent
        {
            get => channelOneCurrent.Value;
            set => channelOneCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 2 in each pulse.
        /// </summary>
        [Description("Channel 2 percent of MaxCurrent. If greater than 0, channel 2 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double ChannelTwoCurrent
        {
            get => channelTwoCurrent.Value;
            set => channelTwoCurrent.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of each pulse in msec.
        /// </summary>
        [Description("The duration of each pulse (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.001, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double PulseDuration
        {
            get => pulseDuration.Value;
            set => pulseDuration.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the pulse period within a burst in msec.
        /// </summary>
        [Description("The pulse period within a burst (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.01, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double PulsesPerSecond
        {
            get => pulsesPerSecond.Value;
            set => pulsesPerSecond.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("Number of pulses to deliver in a burst.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint PulsesPerBurst
        {
            get => pulsesPerBurst.Value;
            set => pulsesPerBurst.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in msec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (msec).")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public double InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("Number of bursts to deliver in a train.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.AcquisitionCategory)]
        public uint BurstsPerTrain
        {
            get => burstsPerTrain.Value;
            set => burstsPerTrain.OnNext(value);
        }

        // TODO: Should this be checked before TRIGGER is written to below and an error thrown if
        // DC current is too high? Or, should settings be forced too keep DC current under some value?
        /// <summary>
        /// Gets total direct current required during the application of a burst.
        /// </summary>
        /// <remarks>
        /// This value should be kept below 50 mA to prevent excess head accumulation on the headstage.
        /// </remarks>
        [Description("The total direct current required during the application of a burst (mA). Should be less than 50 mA.")]
        public double BurstCurrent
        {
            get
            {
                return PulsesPerSecond * 0.001 * PulseDuration * MaxCurrent * 0.01 * (ChannelOneCurrent + ChannelTwoCurrent);
            }
        }

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

                    uint currentSourceMask = 0;
                    static uint percentToPulseMask(int channel, double percent, uint oldMask)
                    {
                        var n = (int)(percent / 100 * 8);
                        uint mask = (1u << n) - 1;
                        return channel == 0 ? (oldMask & 0x0000FF00) | mask : (mask << 8) | (oldMask & 0x000000FF);
                    }

                    static uint pulseDurationToRegister(double pulseDuration, double pulseHz)
                    {
                        var pulsePeriod = 1000.0 / pulseHz;
                        return pulseDuration > pulsePeriod ? (uint)(1000 * pulsePeriod - 1): (uint)(1000 * pulseDuration);
                    }

                    static uint pulseFrequencyToRegister(double pulseHz, double pulseDuration)
                    {
                        var pulsePeriod = 1000.0 / pulseHz;
                        return pulsePeriod > pulseDuration ? (uint)(1000 * pulsePeriod) : (uint)(1000 * pulseDuration + 1);
                    }

                    return new CompositeDisposable(
                        enable.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.ENABLE, value ? 1u : 0u)),
                        maxCurrent.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.MAXCURRENT, Headstage64OpticalStimulator.MilliampsToPotSetting(value))),
                        channelOneCurrent.SubscribeSafe(observer, value =>
                        {
                            currentSourceMask = percentToPulseMask(0, value, currentSourceMask);
                            device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        }),
                        channelTwoCurrent.SubscribeSafe(observer, value =>
                        {
                            currentSourceMask = percentToPulseMask(1, value, currentSourceMask);
                            device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        }),
                        pulseDuration.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.PULSEDUR, pulseDurationToRegister(value, PulsesPerSecond))),
                        pulsesPerSecond.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.PULSEPERIOD, pulseFrequencyToRegister(value, PulseDuration))),
                        pulsesPerBurst.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.BURSTCOUNT, value)),
                        interBurstInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.IBI, (uint)(1000 * value))),
                        burstsPerTrain.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.TRAINCOUNT, value)),
                        delay.SubscribeSafe(observer, value => device.WriteRegister(Headstage64OpticalStimulator.TRAINDELAY, (uint)(1000 * value))),
                        source.SubscribeSafe(triggerObserver)
                    );
                }));
        }
    }
}
