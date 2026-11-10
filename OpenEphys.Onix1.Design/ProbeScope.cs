using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenCV.Net;
using OpenEphys.ProbeInterface.NET;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One selectable band of a probe: a short display name, a description of its passband for the dropdown, the
    /// rate at which that band produces samples, the raw counts in each frame it comes from, which clipping is
    /// checked on, and how it is computed from those counts. Where it has an AC coupled variant, that variant's
    /// passband and how it is computed; a band already high-passed well above the coupling's cutoff has none.
    /// </summary>
    internal sealed record ProbeScopeBand<TFrame>(
        string Name, string Description, int SampleRate,
        Func<TFrame, Mat> Source,
        Func<IObservable<Mat>, IObservable<Mat>> Transform,
        string AcDescription = null,
        Func<IObservable<Mat>, IObservable<Mat>> AcTransform = null);

    /// <summary>
    /// Everything a <see cref="ProbeScopeVisualizer{TFrame}"/> needs from a resolved device: the probe geometry, the
    /// bands available for display, how the probe references a band's output, or null where it offers no
    /// referencing, and the ADC's largest count, which with zero bounds the raw counts.
    /// </summary>
    internal sealed record ProbeScopeSource<TFrame>(
        SingleProbeGroup ProbeGroup,
        IReadOnlyList<ProbeScopeBand<TFrame>> Bands,
        Func<IObservable<Mat>, IObservable<Mat>> Reference,
        double AdcMaximum);

    /// <summary>
    /// One block of band output on its way to the display, with the rate of the band that produced it.
    /// </summary>
    internal sealed record ProbeScopeSample(Mat Data, int SampleRate);

    /// <summary>
    /// The channels one frame found at either end of the ADC's range.
    /// </summary>
    internal sealed record ProbeScopeClipping(bool[] Channels);

    /// <summary>
    /// The channel each row of the blocks that follow shows and where each row lies, or null for each row showing its
    /// own channel.
    /// </summary>
    internal sealed record ProbeScopeOrder(RowOrder Order);

    /// <summary>
    /// Specifies the order of a probe scope's rows.
    /// </summary>
    public enum ChannelOrder
    {
        /// <summary>
        /// By channel number.
        /// </summary>
        Channel,

        /// <summary>
        /// By the depth of each channel's electrode, the deepest at the bottom as the probe is drawn, and across
        /// the probe where electrodes share a depth, or each shank in a lane of its own, lined up by depth.
        /// </summary>
        Depth,

        /// <summary>
        /// Like <see cref="Depth"/>, with each row placed at its electrode's depth, so unused stretches of the
        /// probe show as gaps of their size and electrodes at the same depth on a shank overlap.
        /// </summary>
        Physical,
    }

    /// <summary>
    /// Provides a sink that shows a probe schematic beside a live multi-channel waveform viewer for a streaming
    /// probe, opened from the editor as a visualizer is, with the settings a probe scope adds to every waveform
    /// viewer's.
    /// </summary>
    /// <remarks>
    /// The frames pass through unchanged, and while the scope's window is open the editor hands it a copy of them,
    /// as it does any visualizer.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScope<TFrame> : WaveformScopeBuilder<TFrame>
    {
        /// <summary>
        /// Gets or sets the name of the device whose data is displayed.
        /// </summary>
        /// <remarks>
        /// Left empty, the upstream data operator's device is used if it can be unambiguously resolved. It needs
        /// setting only when it cannot, as when the data arrives through a subject, or through a combinator
        /// such as Zip that hides its source.
        /// </remarks>
        public abstract string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the band displayed.
        /// </summary>
        /// <remarks>
        /// By name rather than position, since which bands a probe offers depends on its configuration.
        /// </remarks>
        [Description("The name of the band displayed. Left empty, or naming a band the probe does not offer, the " +
            "first band is shown.")]
        public string Band { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether each channel is referenced against the median of the channels
        /// sharing its converter.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether each channel is referenced against the median of the channels sharing its " +
            "converter.")]
        public bool CommonMedianReference { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether each channel is high-passed at 1 Hz, on bands that offer it.
        /// </summary>
        [DefaultValue(true)]
        [Description("Specifies whether each channel is high-passed at 1 Hz, as AC coupling does on a scope, on " +
            "bands that offer it.")]
        public bool AcCoupled { get; set; } = true;

        /// <summary>
        /// Gets or sets the order of the rows.
        /// </summary>
        /// <remarks>
        /// Only the display is reordered. The data the scope passes on, and anything written from it, keeps the
        /// device's channel order.
        /// </remarks>
        [DefaultValue(ChannelOrder.Depth)]
        [Description("The order of the rows: by channel number, by the depth of each channel's electrode on the " +
            "probe, or by depth with each row placed at its electrode's depth. Only the display is reordered.")]
        public ChannelOrder ChannelOrder { get; set; } = ChannelOrder.Depth;

        /// <summary>
        /// Gets or sets a value specifying whether each shank of a multi-shank probe is drawn in a lane of its own
        /// when the rows are ordered by depth.
        /// </summary>
        [DefaultValue(true)]
        [Description("Specifies whether each shank of a multi-shank probe is drawn in a lane of its own when the " +
            "rows are ordered by depth.")]
        public bool LanePerShank { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying whether the probe is shown beside the plot.
        /// </summary>
        [DefaultValue(true)]
        [Description("Specifies whether the probe is shown beside the plot.")]
        public bool ShowProbe { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying whether channels are drawn in the colors of the groups assigned to them on
        /// the probe, and channels in no group in grey, rather than in the palette's.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether channels are drawn in the colors of the groups assigned to them on the probe, " +
            "and channels in no group in grey, rather than in the palette's.")]
        public bool ShowGroups { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether threshold crossings are detected on every channel, and an expanded
        /// channel's waveforms shown beside it.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether threshold crossings are detected on every channel, and an expanded channel's " +
            "waveforms shown beside it.")]
        public bool ShowThresholdCrossings { get; set; }

        /// <summary>
        /// Gets or sets the threshold, in microvolts, whose crossing going down is detected on every channel.
        /// </summary>
        [DefaultValue(SpikeDetection.DefaultThreshold)]
        [Description("The threshold, in microvolts, whose crossing going down is detected on every channel.")]
        public double CrossingThreshold { get; set; } = SpikeDetection.DefaultThreshold;

        /// <summary>
        /// Gets or sets the duration of the waveform kept at each threshold crossing, in milliseconds.
        /// </summary>
        [DefaultValue(SpikeDetection.WaveformSeconds * 1000)]
        [Description("The duration of the waveform kept at each threshold crossing, in milliseconds.")]
        public double CrossingWindow { get; set; } = SpikeDetection.WaveformSeconds * 1000;

        /// <summary>
        /// Gets or sets the time in each crossing's waveform before its peak, or before the crossing itself where the
        /// peak comes first, in milliseconds.
        /// </summary>
        [DefaultValue(SpikeDetection.WaveformDelaySeconds * 1000)]
        [Description("The time in each crossing's waveform before its peak, or before the crossing itself where the " +
            "peak comes first, in milliseconds.")]
        public double CrossingPreTrigger { get; set; } = SpikeDetection.WaveformDelaySeconds * 1000;

        /// <summary>
        /// Gets or sets the time constant, in seconds, over which each channel's threshold crossing rate is averaged.
        /// </summary>
        [DefaultValue(1.0)]
        [Description("The time constant, in seconds, over which each channel's threshold crossing rate is averaged.")]
        public double CrossingRateTimeConstant { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying whether the probe's contacts are colored by their channels' threshold
        /// crossing rates while crossings are detected, rather than as groups color them.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether the probe's contacts are colored by their channels' threshold crossing rates " +
            "while crossings are detected, rather than as groups color them.")]
        public bool ColorContactsByEventRate { get; set; }

        /// <summary>
        /// Gets or sets the threshold crossing rate, in Hz, at the top of the probe's color scale.
        /// </summary>
        [DefaultValue(50.0)]
        [Description("The threshold crossing rate, in Hz, at the top of the probe's color scale.")]
        public double EventRateScale { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of seconds of data retained behind the display.
        /// </summary>
        [DefaultValue(10.0)]
        [Description("The number of seconds of data retained behind the display, which a paused view can " +
            "be moved back through. Longer histories cost proportionally more memory.")]
        public double HistorySeconds { get; set; } = 10;

        /// <summary>
        /// What <see cref="HistorySeconds"/> costs on this probe, which only a concrete scope can say,
        /// knowing its own channel count and fastest band, and how much memory is reasonable to spend.
        /// </summary>
        internal abstract long HistoryBytes { get; }

        /// <summary>
        /// Returns the device name declared on <paramref name="upstreamOperator"/>, or null if
        /// it is not a data operator this scope can display.
        /// </summary>
        internal abstract string DeviceNameOf(object upstreamOperator);

        /// <summary>
        /// Builds the geometry and bands for a resolved device, each band with everything done to the device's data
        /// to produce what is drawn, in the unit named by <see cref="Unit"/>. Throw to report a device this scope
        /// cannot display.
        /// </summary>
        internal abstract ProbeScopeSource<TFrame> CreateSource(DeviceInfo info);

        /// <summary>
        /// Unit the bands produce.
        /// </summary>
        internal abstract string Unit { get; }

        /// <summary>
        /// The largest amplitude a channel's row may span, in <see cref="Unit"/>.
        /// </summary>
        internal abstract double MaxRange { get; }

        /// <summary>
        /// The smallest amplitude a channel's row may span, in <see cref="Unit"/>.
        /// </summary>
        internal abstract double MinRange { get; }
    }
}
