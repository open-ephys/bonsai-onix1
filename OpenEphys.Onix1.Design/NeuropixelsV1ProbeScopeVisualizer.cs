using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Probe scope for a NeuropixelsV1 probe. Offers the hardware spike and LFP bands, plus a wideband
    /// view and a software spike band when the probe's spike filter is off.
    /// </summary>
    public class NeuropixelsV1ProbeScopeVisualizer : ProbeScopeVisualizer<NeuropixelsV1DataFrame>
    {
        private protected override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV1eData data ? data.DeviceName : null;

        /// <inheritdoc/>
        private protected override string RangeLabel => "uV";

        private protected override ProbeScopeSource<NeuropixelsV1DataFrame> CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV1PsbDecoderDeviceInfo v1)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV1 probe this scope can display.");

            var configuration = v1.ProbeConfiguration
                ?? throw new InvalidOperationException("The device has no probe configuration, so its amplifier gain is unknown.");

            const int spikeRate = NeuropixelsV1.SamplesPerChannelPerSecond;
            const int lfpRate = NeuropixelsV1.SamplesPerChannelPerSecond / NeuropixelsV1.FramesPerRoundRobin;

            IObservable<Mat> ScaleSpike(IObservable<NeuropixelsV1DataFrame> frames) =>
                new NeuropixelsV1Scale
                {
                    Band = NeuropixelsV1EphysBand.Spike,
                    AmplifierGain = configuration.SpikeAmplifierGain
                }.Process(frames);

            IObservable<Mat> ScaleLfp(IObservable<NeuropixelsV1DataFrame> frames) =>
                new NeuropixelsV1Scale
                {
                    Band = NeuropixelsV1EphysBand.Lfp,
                    AmplifierGain = configuration.LfpAmplifierGain
                }.Process(frames);

            var bands = new List<ProbeScopeBand<NeuropixelsV1DataFrame>>();

            // NB: with the hardware spike filter off, the spike stream is wideband, so it is offered as
            // such and a software spike band is carved out of it, as for NeuropixelsV2.
            if (configuration.SpikeFilter)
            {
                bands.Add(new("Spike", "300 Hz to 9 kHz", spikeRate, ScaleSpike));
            }
            else
            {
                bands.Add(new("Wideband", "0.2 Hz to 9 kHz", spikeRate, ScaleSpike));
                bands.Add(new("Spike", "300 Hz to 9 kHz", spikeRate, ScaleSpike,
                    scaled => new Butterworth
                    {
                        SampleRate = spikeRate,
                        Cutoff1 = 300.0,
                        Cutoff2 = 9000.0,
                        FilterType = FilterType.BandPass,
                        FilterOrder = 2
                    }.Process(scaled)));
            }

            bands.Add(new("LFP", "0.2 Hz to 500 Hz", lfpRate, ScaleLfp));

            return new(v1.ProbeGroup, NeuropixelsV1.AdcChannelGroups(), bands);
        }
    }

    /// <summary>
    /// Marks a point directly downstream of a <see cref="NeuropixelsV1eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    [TypeVisualizer(typeof(NeuropixelsV1ProbeScopeVisualizer))]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV1 probe.")]
    public class NeuropixelsV1ProbeScope : ProbeScope<NeuropixelsV1DataFrame>
    {
        const double MaxHistorySeconds = 10;

        /// <inheritdoc/>
        internal override long HistoryBytes =>
            (long)(Math.Min(HistorySeconds, MaxHistorySeconds)
                * NeuropixelsV1.SamplesPerChannelPerSecond
                * NeuropixelsV1.ChannelCount
                * sizeof(float));
    }
}
