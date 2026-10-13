using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenCV.Net;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One selectable band of a probe: a short display name, a description of its passband for the dropdown, the
    /// rate at which that band produces samples, how it is computed from the device's frames, and the raw counts it
    /// comes from, which clipping is checked on. Where it has an AC coupled variant, that variant's passband and
    /// how it is computed; a band already high-passed well above the coupling's cutoff has none.
    /// </summary>
    internal sealed record ProbeScopeBand<TFrame>(
        string Name, string Description, int SampleRate,
        Func<IObservable<TFrame>, IObservable<Mat>> Transform,
        Func<TFrame, Mat> Raw,
        string AcDescription = null,
        Func<IObservable<TFrame>, IObservable<Mat>> AcTransform = null);

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
    /// Provides a sink that shows a probe schematic beside a live multi-channel waveform viewer for a streaming
    /// probe, opened from the editor as a visualizer is, with the settings a probe scope adds to every waveform
    /// viewer's.
    /// </summary>
    /// <remarks>
    /// Each concrete scope supplies how to read the device name off the upstream data operator and how to turn its
    /// <see cref="DeviceInfo"/> into geometry and bands. The frames pass through unchanged, and while the scope's
    /// window is open the editor hands it a copy of them, as it does any visualizer.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScope<TFrame> : WaveformScope
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
        /// By name rather than position, since which bands a probe offers depends on its configuration. A name the
        /// probe does not offer shows its first band.
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
        /// What <see cref="WaveformScope.HistorySeconds"/> costs on this probe, which only a concrete scope can say,
        /// knowing its own channel count and fastest band, and how much memory is reasonable to spend.
        /// </summary>
        internal abstract long HistoryBytes { get; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            if (source.Type != typeof(IObservable<TFrame>))
                throw new InvalidOperationException($"{GetType().Name} displays a sequence of {typeof(TFrame).Name}.");

            // NB: through a call of its own rather than returning the source, which the editor would take for the
            // upstream node's output and offer that node's visualizers instead of the scope.
            return Expression.Call(Expression.Constant(this, typeof(ProbeScope<TFrame>)), nameof(Process), null, source);
        }

        IObservable<TFrame> Process(IObservable<TFrame> source) => source;

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
    }
}
