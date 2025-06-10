using System;
using Bonsai.Reactive;
using Bonsai;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A stimulus sequence for the Headstage64 Optical Stimulator.
    /// </summary>
    public class Headstage64OpticalStimulatorSequence
    {
        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in msec.
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double Delay { get; set; } = 0;

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
        [Category(DeviceFactory.ConfigurationCategory)]
        public double MaxCurrent { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 1 in each pulse.
        /// </summary>
        [Description("Channel 1 percent of MaxCurrent. If greater than 0, channel 1 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double ChannelOneCurrent { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 2 in each pulse.
        /// </summary>
        [Description("Channel 2 percent of MaxCurrent. If greater than 0, channel 2 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double ChannelTwoCurrent { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of each pulse in msec.
        /// </summary>
        [Description("The duration of each pulse (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.001, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PulseDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets the pulse period within a burst in msec.
        /// </summary>
        [Description("The pulse period within a burst (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.01, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PulsesPerSecond { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("Number of pulses to deliver in a burst.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint PulsesPerBurst { get; set; } = 20;

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in msec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (msec).")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double InterBurstInterval { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("Number of bursts to deliver in a train.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint BurstsPerTrain { get; set; } = 1;

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
        /// Initializes a new instance of the <see cref="Headstage64OpticalStimulatorSequence"/> class with default values.
        /// </summary>
        public Headstage64OpticalStimulatorSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Headstage64OpticalStimulatorSequence"/> class with the given values.
        /// </summary>
        public Headstage64OpticalStimulatorSequence(double delay, double maxCurrent, double channelOneCurrent, double channelTwoCurrent, 
            double pulseDuration, double pulsesPerSecond, uint pulsesPerBurst, double interBurstInterval, uint burstsPerTrain)
        {
            Delay = delay;
            MaxCurrent = maxCurrent;
            ChannelOneCurrent = channelOneCurrent;
            ChannelTwoCurrent = channelTwoCurrent;
            PulseDuration = pulseDuration;
            PulsesPerSecond = pulsesPerSecond;
            PulsesPerBurst = pulsesPerBurst;
            InterBurstInterval = interBurstInterval;
            BurstsPerTrain = burstsPerTrain;
        }
    }
}
