using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Probe scope for a NeuropixelsV2 probe, which has a single wideband output.
    /// </summary>
    public class NeuropixelsV2ProbeScopeVisualizer : ProbeScopeVisualizer<NeuropixelsV2DataFrame>
    {
        private protected override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV2eData data ? data.DeviceName : null;

        /// <inheritdoc/>
        private protected override string RangeLabel => "uV";

        Func<IObservable<NeuropixelsV2DataFrame>, bool, IObservable<Mat>>[] transforms;

        private protected override ProbeScopeSource CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV2PsbDecoderDeviceInfo v2)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV2 probe this scope can display.");

            const int sampleRate = NeuropixelsV2.SamplesPerChannelPerSecond;
            static IObservable<Mat> Scale(IObservable<NeuropixelsV2DataFrame> frames, bool cmr) =>
                new NeuropixelsV2Scale { UseCommonMedianReference = cmr }.Process(frames);

            // NB: each band is declared beside the transform that produces it so the two cannot drift.
            var bands = new (ProbeScopeBand Band, Func<IObservable<NeuropixelsV2DataFrame>, bool, IObservable<Mat>> Transform)[]
            {
                (new("Wideband", "0.5 Hz to 10 kHz", sampleRate), Scale),
                (new("Spike", "300 Hz to 9 kHz", sampleRate), (frames, cmr) => new Butterworth
                {
                    SampleRate = sampleRate,
                    Cutoff1 = 300.0,
                    Cutoff2 = 9000.0,
                    FilterType = FilterType.BandPass,
                    FilterOrder = 2
                }.Process(Scale(frames, cmr))),
                (new("LFP", "0.5 Hz to 500 Hz", sampleRate), (frames, cmr) => new Butterworth
                {
                    SampleRate = sampleRate,
                    Cutoff1 = 500.0,
                    FilterType = FilterType.LowPass,
                    FilterOrder = 2
                }.Process(Scale(frames, cmr))),
            };

            transforms = bands.Select(b => b.Transform).ToArray();
            return new(v2.ProbeGroup, bands.Select(b => b.Band).ToArray());
        }

        private protected override IObservable<Mat> ProcessBand(
            int band, bool commonMedianReference, IObservable<NeuropixelsV2DataFrame> frames) =>
            transforms[band](frames, commonMedianReference);
    }

    /// <summary>
    /// Marks a point directly downstream of a <see cref="NeuropixelsV2eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    [TypeVisualizer(typeof(NeuropixelsV2ProbeScopeVisualizer))]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV2 probe.")]
    public class NeuropixelsV2ProbeScope : ProbeScope<NeuropixelsV2DataFrame>
    {
        const double MaxHistorySeconds = 10;

        /// <inheritdoc/>
        internal override long HistoryBytes =>
            (long)(Math.Min(HistorySeconds, MaxHistorySeconds)
                * NeuropixelsV2.SamplesPerChannelPerSecond
                * NeuropixelsV2.ChannelCount
                * sizeof(float));
    }
}
