using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.ComponentModel;

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

        private protected override ProbeScopeSource<NeuropixelsV2DataFrame> CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV2PsbDecoderDeviceInfo v2)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV2 probe this scope can display.");

            const int sampleRate = NeuropixelsV2.SamplesPerChannelPerSecond;
            static IObservable<Mat> Scale(IObservable<NeuropixelsV2DataFrame> frames) =>
                new NeuropixelsV2Scale().Process(frames);

            return new(v2.ProbeGroup, NeuropixelsV2.AdcChannelGroups(), new[]
            {
                new ProbeScopeBand<NeuropixelsV2DataFrame>("Wideband", "0.5 Hz to 10 kHz", sampleRate, Scale),
                new ProbeScopeBand<NeuropixelsV2DataFrame>("Spike", "300 Hz to 9 kHz", sampleRate, Scale,
                    scaled => new Butterworth
                    {
                        SampleRate = sampleRate,
                        Cutoff1 = 300.0,
                        Cutoff2 = 9000.0,
                        FilterType = FilterType.BandPass,
                        FilterOrder = 2
                    }.Process(scaled)),
                new ProbeScopeBand<NeuropixelsV2DataFrame>("LFP", "0.5 Hz to 500 Hz", sampleRate, Scale,
                    scaled => new Butterworth
                    {
                        SampleRate = sampleRate,
                        Cutoff1 = 500.0,
                        FilterType = FilterType.LowPass,
                        FilterOrder = 2
                    }.Process(scaled)),
            });
        }
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
