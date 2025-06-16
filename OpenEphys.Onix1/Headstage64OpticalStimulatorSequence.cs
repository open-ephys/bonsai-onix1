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
        private double delay = 0;

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in msec.
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double Delay
        {
            get => delay;
            set 
            {
                if (value < 0) delay = 0;
                else if (value > 1000) delay = 1000;
                else delay = value;
            }
        }

        private double maxCurrent = 100;

        /// <summary>
        /// Gets or sets the Maximum current per channel per pulse in mA.
        /// </summary>
        /// <remarks>
        /// This value defines the maximal possible current that can be delivered to each channel.
        /// To get different amplitudes for each channel use the <see cref="ChannelOnePercent"/> and
        /// <see cref="ChannelTwoPercent"/> properties.
        /// </remarks>
        [Description("Maximum current per channel per pulse (mA). " +
            "This value is used by both channels. To get different amplitudes " +
            "for each channel use the ChannelOnePercent and ChannelTwoPercent properties.")]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Range(0, 300)]
        [Precision(3, 0)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double MaxCurrent
        {
            get => maxCurrent;
            set
            {
                if (value < 0) maxCurrent = 0;
                else if (value > 300) maxCurrent = 300;
                else maxCurrent = value;
            }
        }

        private double channelOnePercent = 100;

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 1 in each pulse.
        /// </summary>
        [Description("Channel 1 percent of MaxCurrent. If greater than 0, channel 1 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double ChannelOnePercent
        {
            get => channelOnePercent;
            set
            {
                if (value < 0) channelOnePercent = 0;
                else if (value > 100) channelOnePercent = 100;
                else channelOnePercent = value;
            }
        }

        private double channelTwoPercent = 0;

        /// <summary>
        /// Gets or sets the percent of <see cref="MaxCurrent"/> that will delivered to channel 2 in each pulse.
        /// </summary>
        [Description("Channel 2 percent of MaxCurrent. If greater than 0, channel 2 will respond to triggers.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0, 100)]
        [Precision(1, 12.5)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double ChannelTwoPercent
        {
            get => channelTwoPercent;
            set
            {
                if (value < 0) channelTwoPercent = 0;
                else if (value > 100) channelTwoPercent = 100;
                else channelTwoPercent = value;
            }
        }

        private double pulseDuration = 5;

        /// <summary>
        /// Gets or sets the duration of each pulse in msec.
        /// </summary>
        [Description("The duration of each pulse (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.001, 1000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PulseDuration
        {
            get => pulseDuration;
            set
            {
                if (value < 0.001) pulseDuration = 0.001;
                else if (value > 1000) pulseDuration = 1000;
                else pulseDuration = value;
            }
        }

        private double pulsePeriod = 50;

        /// <summary>
        /// Gets or sets the pulse period within a burst in msec.
        /// </summary>
        [Description("The pulse period within a burst (msec).")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.01, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PulsePeriod
        {
            get => pulsePeriod;
            set
            {
                if (value < 0.01) pulsePeriod = 0.01;
                else if (value > 10000) pulsePeriod = 10000;
                else pulsePeriod = value;
            }
        }

        private uint pulsesPerBurst = 20;

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("Number of pulses to deliver in a burst.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint PulsesPerBurst
        {
            get => pulsesPerBurst;
            set
            {
                if (value < 1) pulsesPerBurst = 1;
                else if (value > int.MaxValue) pulsesPerBurst = int.MaxValue;
                else pulsesPerBurst = value;
            }
        }

        private double interBurstInterval = 0;

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in msec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (msec).")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(0.0, 10000.0)]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double InterBurstInterval
        {
            get => interBurstInterval;
            set
            {
                if (value < 0) interBurstInterval = 0;
                else if (value > 10000) interBurstInterval = 10000;
                else interBurstInterval = value;
            }
        }

        private uint burstsPerTrain = 1;

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("Number of bursts to deliver in a train.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Range(1, int.MaxValue)]
        [Precision(0, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint BurstsPerTrain
        {
            get => burstsPerTrain;
            set
            {
                if (value < 1) burstsPerTrain = 1;
                else if (value > int.MaxValue) burstsPerTrain = int.MaxValue;
                else burstsPerTrain = value;
            }
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
                return PulsePeriod * 0.001 * PulseDuration * MaxCurrent * 0.01 * (ChannelOnePercent + ChannelTwoPercent);
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
            ChannelOnePercent = channelOneCurrent;
            ChannelTwoPercent = channelTwoCurrent;
            PulseDuration = pulseDuration;
            PulsePeriod = pulsesPerSecond;
            PulsesPerBurst = pulsesPerBurst;
            InterBurstInterval = interBurstInterval;
            BurstsPerTrain = burstsPerTrain;
        }

        /// <summary>
        /// Copy constructor for the <see cref="Headstage64OpticalStimulatorSequence"/> class.
        /// </summary>
        /// <param name="stimulatorSequence">Existing sequence to copy.</param>
        public Headstage64OpticalStimulatorSequence(Headstage64OpticalStimulatorSequence stimulatorSequence)
        {
            Delay = stimulatorSequence.Delay;
            MaxCurrent = stimulatorSequence.MaxCurrent;
            ChannelOnePercent = stimulatorSequence.ChannelOnePercent;
            ChannelTwoPercent = stimulatorSequence.ChannelTwoPercent;
            PulseDuration = stimulatorSequence.PulseDuration;
            PulsePeriod = stimulatorSequence.PulsePeriod;
            PulsesPerBurst = stimulatorSequence.PulsesPerBurst;
            InterBurstInterval = stimulatorSequence.InterBurstInterval;
            BurstsPerTrain = stimulatorSequence.BurstsPerTrain;
        }
    }
}
