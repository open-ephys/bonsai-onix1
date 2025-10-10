﻿using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 optical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Headstage64OpticalStimulatorTrigger"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 optical stimulator.")]
    [Editor("OpenEphys.Onix1.Design.Headstage64OpticalStimulatorComponentEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureHeadstage64OpticalStimulator : SingleDeviceFactory
    {
        internal uint? PortControllerDeviceAddress { get; set; }

        readonly BehaviorSubject<bool> enableIndicationLed = new(false);
        readonly BehaviorSubject<double> maxCurrent = new(0);
        readonly BehaviorSubject<double> channelOneCurrent = new(0);
        readonly BehaviorSubject<double> channelTwoCurrent = new(0);
        readonly BehaviorSubject<double> pulseDuration = new(0);
        readonly BehaviorSubject<double> pulsesPerSecond = new(0);
        readonly BehaviorSubject<uint> pulsesPerBurst = new(0);
        readonly BehaviorSubject<double> interBurstInterval = new(0);
        readonly BehaviorSubject<uint> burstsPerTrain = new(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64OpticalStimulator"/> class.
        /// </summary>
        public ConfigureHeadstage64OpticalStimulator()
            : base(typeof(Headstage64OpticalStimulator))
        {
        }

        /// <summary>
        /// Copy constructor for the <see cref="ConfigureHeadstage64OpticalStimulator"/> class.
        /// </summary>
        /// <param name="opticalStimulator">Existing <see cref="ConfigureHeadstage64OpticalStimulator"/> object.</param>
        public ConfigureHeadstage64OpticalStimulator(ConfigureHeadstage64OpticalStimulator opticalStimulator) : this()
        {
            DeviceName = opticalStimulator.DeviceName;
            DeviceAddress = opticalStimulator.DeviceAddress;
            Enable = opticalStimulator.Enable;
            MaxCurrent = opticalStimulator.MaxCurrent;
            ChannelOneCurrent = opticalStimulator.ChannelOneCurrent;
            ChannelTwoCurrent = opticalStimulator.ChannelTwoCurrent;
            PulseDuration = opticalStimulator.PulseDuration;
            PulsesPerSecond = opticalStimulator.PulsesPerSecond;
            PulsesPerBurst = opticalStimulator.PulsesPerBurst;
            InterBurstInterval = opticalStimulator.InterBurstInterval;
            BurstsPerTrain = opticalStimulator.BurstsPerTrain;
        }

        /// <summary>
        /// Gets or sets the data enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Headstage64OpticalStimulatorData"/> will produce data. If set to
        /// false, <see cref="Headstage64OpticalStimulatorData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the headstage-64 optical stimulator will produce stimulus reports.")]
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the indication LED enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, the headstage's indication LED will turn on. When set to false, it will turn off. 
        /// </remarks>
        [Description("Specifies the state of the headstage indication LED")]
        [Category(AcquisitionCategory)]
        public bool EnableIndicationLed
        {
            get => enableIndicationLed.Value;
            set => enableIndicationLed.OnNext(value);
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
        [Range(Headstage64OpticalStimulator.MinCurrent, Headstage64OpticalStimulator.MaxCurrent)]
        [Precision(3, 0)]
        [Category(AcquisitionCategory)]
        public double MaxCurrent
        {
            get => maxCurrent.Value;
            set => maxCurrent.OnNext(Clamp(value, Headstage64OpticalStimulator.MinCurrent, Headstage64OpticalStimulator.MaxCurrent));
        }

        static double VerifyChannelPercentage(double value, double min, double max, double step)
        {
            value = Clamp(value, min, max);

            return Math.Round(value / step) * step;
        }

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 1 in each pulse.
        /// </summary>
        [Description("Channel 1 percent of MaxCurrent. If greater than 0, channel 1 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(Headstage64OpticalStimulator.MinChannelPercentage, Headstage64OpticalStimulator.MaxChannelPercentage)]
        [Precision(1, Headstage64OpticalStimulator.ChannelPercentageStep)]
        [Category(AcquisitionCategory)]
        public double ChannelOneCurrent
        {
            get => channelOneCurrent.Value;
            set => channelOneCurrent.OnNext(VerifyChannelPercentage(value,
                Headstage64OpticalStimulator.MinChannelPercentage,
                Headstage64OpticalStimulator.MaxChannelPercentage,
                Headstage64OpticalStimulator.ChannelPercentageStep));
        }

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 2 in each pulse.
        /// </summary>
        [Description("Channel 2 percent of MaxCurrent. If greater than 0, channel 2 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(Headstage64OpticalStimulator.MinChannelPercentage, Headstage64OpticalStimulator.MaxChannelPercentage)]
        [Precision(1, Headstage64OpticalStimulator.ChannelPercentageStep)]
        [Category(AcquisitionCategory)]
        public double ChannelTwoCurrent
        {
            get => channelTwoCurrent.Value;
            set => channelTwoCurrent.OnNext(VerifyChannelPercentage(value,
                Headstage64OpticalStimulator.MinChannelPercentage,
                Headstage64OpticalStimulator.MaxChannelPercentage,
                Headstage64OpticalStimulator.ChannelPercentageStep));
        }

        /// <summary>
        /// Gets or sets the duration of each pulse in msec.
        /// </summary>
        [Description("The duration of each pulse (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(Headstage64OpticalStimulator.MinPulseDuration, Headstage64OpticalStimulator.MaxPulseDuration)]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double PulseDuration
        {
            get => pulseDuration.Value;
            set => pulseDuration.OnNext(Clamp(value, Headstage64OpticalStimulator.MinPulseDuration, Headstage64OpticalStimulator.MaxPulseDuration));
        }

        /// <summary>
        /// Gets or sets the pulse period within a burst in msec.
        /// </summary>
        [Description("The pulse period within a burst (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(Headstage64OpticalStimulator.MinPulsePeriod, Headstage64OpticalStimulator.MaxPulsePeriod)]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double PulsesPerSecond
        {
            get => pulsesPerSecond.Value;
            set => pulsesPerSecond.OnNext(Clamp(value, Headstage64OpticalStimulator.MinPulsePeriod, Headstage64OpticalStimulator.MaxPulsePeriod));
        }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("Number of pulses to deliver in a burst.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(AcquisitionCategory)]
        public uint PulsesPerBurst
        {
            get => pulsesPerBurst.Value;
            set => pulsesPerBurst.OnNext(Clamp(value, 1, int.MaxValue));
        }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in msec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(Headstage64OpticalStimulator.MinInterBurstInterval, Headstage64OpticalStimulator.MaxInterBurstInterval)]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(Clamp(value, Headstage64OpticalStimulator.MinInterBurstInterval, Headstage64OpticalStimulator.MaxInterBurstInterval));
        }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("Number of bursts to deliver in a train.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(AcquisitionCategory)]
        public uint BurstsPerTrain
        {
            get => burstsPerTrain.Value;
            set => burstsPerTrain.OnNext(Clamp(value, 1, int.MaxValue));
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

        static double Clamp(double value, double min, double max) =>
            Math.Min(Math.Max(value, min), max);

        static uint Clamp(uint value, uint min, uint max) =>
            Math.Min(Math.Max(value, min), max);

        /// <summary>
        /// Configure a headstage-64 dual-channel optical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 dual-channel optical stimulator.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var enable = Enable;
            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                var deviceInfo = new Headstage64StimulatorDeviceInfo(context, DeviceType, deviceAddress, PortControllerDeviceAddress);

                device.WriteRegister(Headstage64OpticalStimulator.ENABLE, enable ? 1u : 0u);

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
                    return pulseDuration > pulsePeriod ? (uint)(1000 * pulsePeriod - 1) : (uint)(1000 * pulseDuration);
                }

                static uint pulseFrequencyToRegister(double pulseHz, double pulseDuration)
                {
                    var pulsePeriod = 1000.0 / pulseHz;
                    return pulsePeriod > pulseDuration ? (uint)(1000 * pulsePeriod) : (uint)(1000 * pulseDuration + 1);
                }

                uint stimEnableValue = 0;

                return new CompositeDisposable(
                    enableIndicationLed.SubscribeSafe(observer, value =>
                    {
                        if (value)
                            stimEnableValue |= (1u << 8);
                        else
                            stimEnableValue &= ~(1u << 8);
                        device.WriteRegister(Headstage64OpticalStimulator.STIMENABLE, stimEnableValue);
                    }),
                    maxCurrent.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.MAXCURRENT, Headstage64OpticalStimulator.MilliampsToPotSetting(value))),
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
                    pulseDuration.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEDUR, pulseDurationToRegister(value, PulsesPerSecond))),
                    pulsesPerSecond.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEPERIOD, pulseFrequencyToRegister(value, PulseDuration))),
                    pulsesPerBurst.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.BURSTCOUNT, value)),
                    interBurstInterval.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.IBI, (uint)(1000 * value))),
                    burstsPerTrain.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64OpticalStimulator.TRAINCOUNT, value)),
                    DeviceManager.RegisterDevice(deviceName, deviceInfo));
            });
        }
    }

    static class Headstage64OpticalStimulator
    {
        public const int ID = 5;
        public const uint MinimumVersion = 3;

        // NB: can be read with MINRHEOR and POTRES, but will not change
        public const uint MinRheostatResistanceOhms = 590;
        public const uint PotResistanceOhms = 100_000;

        public const double MinCurrent = 0.0;
        public const double MaxCurrent = 300.0;

        public const double MinChannelPercentage = 0.0;
        public const double MaxChannelPercentage = 100.0;
        public const double ChannelPercentageStep = 12.5;

        public const double MinPulseDuration = 0.001;
        public const double MaxPulseDuration = 1000.0;

        public const double MinPulsePeriod = 0.01;
        public const double MaxPulsePeriod = 10000.0;

        public const double MinInterBurstInterval = 0.0;
        public const double MaxInterBurstInterval = 10000.0;

        // managed registers
        public const uint ENABLE = 0; // Enable stimulus report stream
        public const uint MAXCURRENT = 1; // Max LED/LD current, (0 to 255 = 800mA to 0 mA.See fig XX of CAT4016 datasheet)
        public const uint PULSEMASK = 2; // Bitmask determining which of the(up to 32) channels is affected by trigger
        public const uint PULSEDUR = 3; // Pulse duration, microseconds
        public const uint PULSEPERIOD = 4; // Inter-pulse interval, microseconds
        public const uint BURSTCOUNT = 5; // Number of pulses in burst
        public const uint IBI = 6; // Inter-burst interval, microseconds
        public const uint TRAINCOUNT = 7; // Number of bursts in train
        public const uint TRIGGER = 8; // Trigger stimulation (0 = off, 1 = deliver)
        public const uint STIMENABLE = 9; // 1: enables the stimulator, 0: stimulator ignores triggers (so that a common trigger can be used)
        public const uint MINRHEOR = 10; // The series resistor between the potentiometer (rheostat) and RSET bin on the CAT4016
        public const uint POTRES = 11; // The resistance value of the potentiometer connected in rheostat config to RSET on CAT4016

        // NB: fit from Fig. 10 of CAT4016 datasheet
        // x = (y/a)^(1/b)
        // a = 3.833e+05
        // b = -0.9632
        internal static uint MilliampsToPotSetting(double currentMa)
        {
            double R = Math.Pow(currentMa / 3.833e+05, 1 / -0.9632);
            uint s = (uint)Math.Round(256 * (R - MinRheostatResistanceOhms) / PotResistanceOhms);
            return s > 255 ? 255 : s < 0 ? 0 :s;
        }

        internal static double PotSettingToMilliamps(uint potSetting)
        {
            var R = MinRheostatResistanceOhms + PotResistanceOhms * potSetting / 256; 
            return 3.833e+05 * Math.Pow(R, -0.9632);
        }

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64OpticalStimulator))
            {
            }
        }
    }
}
