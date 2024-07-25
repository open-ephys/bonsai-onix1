using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
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

        [TypeConverter(typeof(Headstage64OpticalStimulator.NameConverter))]
        public string DeviceName { get; set; }

        [Description("Specifies whether the optical stimulation subcircuit will respect triggers.")]
        public bool Enable
        {
            get => enable.Value;
            set => enable.OnNext(value);
        }

        [Description("Maximum current per channel per pulse (mA). " +
            "This value is used by both channels. To get different amplitudes " +
            "for each channel use the Channel0Level and Channel1Level parameters.")]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Range(0, 300)]
        [Precision(3, 0)]
        public double MaxCurrent
        {
            get => maxCurrent.Value;
            set => maxCurrent.OnNext(value);
        }

        [Description("Channel 1 percent of MaxCurrent. If greater than 0, channel 1 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        public double ChannelOneCurrent
        {
            get => channelOneCurrent.Value;
            set => channelOneCurrent.OnNext(value);
        }

        [Description("Channel 2 percent of MaxCurrent. If greater than 0, channel 2 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        public double ChannelTwoCurrent
        {
            get => channelTwoCurrent.Value;
            set => channelTwoCurrent.OnNext(value);
        }

        [Description("Pulse duration (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.001, 1000.0)]
        [Precision(3, 1)]
        public double PulseDuration
        {
            get => pulseDuration.Value;
            set => pulseDuration.OnNext(value);
        }

        [Description("Pulse period (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.01, 10000.0)]
        [Precision(3, 1)]
        public double PulsesPerSecond
        {
            get => pulsesPerSecond.Value;
            set => pulsesPerSecond.OnNext(value);
        }

        [Description("Number of pulses to deliver in a burst.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        public uint PulsesPerBurst
        {
            get => pulsesPerBurst.Value;
            set => pulsesPerBurst.OnNext(value);
        }

        [Description("Inter-burst interval (msec).")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 10000.0)]
        [Precision(3, 1)]
        public double InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(value);
        }

        [Description("Number of bursts to deliver in a train.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        public uint BurstsPerTrain
        {
            get => burstsPerTrain.Value;
            set => burstsPerTrain.OnNext(value);
        }

        [Description("Delay between issue of trigger and start of train (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 1000.0)]
        [Precision(3, 1)]
        public double Delay
        {
            get => delay.Value;
            set => delay.OnNext(value);
        }

        // TODO: Should this be checked before TRIGGER is written to below and an error thrown if
        // DC current is too high? Or, should settings be forced too keep DC current under some value?
        [Description("Direct current required during burst (mA). Should be less than 50 mA.")]
        public double BurstCurrent
        {
            get
            {
                return PulsesPerSecond * 0.001 * PulseDuration * MaxCurrent * 0.01 * (ChannelOneCurrent + ChannelTwoCurrent);
            }
        }

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<bool>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Headstage64OpticalStimulator));
                    var triggerObserver = Observer.Create<bool>(
                        value => device.WriteRegister(Headstage64OpticalStimulator.TRIGGER, value ? 1u : 0u),
                        observer.OnError,
                        observer.OnCompleted);

                    // NB: fit from Fig. 10 of CAT4016 datasheet
                    // x = (y/a)^(1/b)
                    // a = 3.833e+05
                    // b = -0.9632
                    static uint mAToPotSetting(double currentMa)
                    {
                        double R = Math.Pow(currentMa / 3.833e+05, 1 / -0.9632);
                        double s = 256 * (R - Headstage64OpticalStimulator.MinRheostatResistanceOhms) / Headstage64OpticalStimulator.PotResistanceOhms;
                        return s > 255 ? 255 : s < 0 ? 0 : (uint)s;
                    }

                    uint currentSourceMask = 0;
                    static uint percentToPulseMask(int channel, double percent, uint oldMask)
                    {
                        uint mask = 0x00000000;
                        var p = 0.0;
                        while (p < percent)
                        {
                            mask = (mask << 1) | 1;
                            p += 12.5;
                        }

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
                        enable.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.ENABLE, value ? 1u : 0u)),
                        maxCurrent.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.MAXCURRENT, mAToPotSetting(value))),
                        channelOneCurrent.Subscribe(value =>
                        {
                            currentSourceMask = percentToPulseMask(0, value, currentSourceMask);
                            device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        }),
                        channelTwoCurrent.Subscribe(value =>
                        {
                            currentSourceMask = percentToPulseMask(1, value, currentSourceMask);
                            device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        }),
                        pulseDuration.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.PULSEDUR, pulseDurationToRegister(value, PulsesPerSecond))),
                        pulsesPerSecond.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.PULSEPERIOD, pulseFrequencyToRegister(value, PulseDuration))),
                        pulsesPerBurst.Subscribe(value =>device.WriteRegister(Headstage64OpticalStimulator.BURSTCOUNT, value)),
                        interBurstInterval.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.IBI, (uint)(1000 * value))),
                        burstsPerTrain.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.TRAINCOUNT, value)),
                        delay.Subscribe(value => device.WriteRegister(Headstage64OpticalStimulator.TRAINDELAY, (uint)(1000 * value))),
                        source.SubscribeSafe(triggerObserver)
                    );
                }));
        }
    }
}
