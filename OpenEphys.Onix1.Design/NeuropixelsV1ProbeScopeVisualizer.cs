using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        Func<IObservable<NeuropixelsV1DataFrame>, bool, IObservable<Mat>>[] transforms;

        private protected override ProbeScopeSource CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV1PsbDecoderDeviceInfo v1)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV1 probe this scope can display.");

            var configuration = v1.ProbeConfiguration
                ?? throw new InvalidOperationException("The device has no probe configuration, so its amplifier gain is unknown.");

            const int spikeRate = NeuropixelsV1.SamplesPerChannelPerSecond;
            const int lfpRate = NeuropixelsV1.SamplesPerChannelPerSecond / NeuropixelsV1.FramesPerRoundRobin;

            IObservable<Mat> ScaleSpike(IObservable<NeuropixelsV1DataFrame> frames, bool cmr) =>
                new NeuropixelsV1Scale
                {
                    Band = NeuropixelsV1EphysBand.Spike,
                    AmplifierGain = configuration.SpikeAmplifierGain,
                    UseCommonMedianReference = cmr
                }.Process(frames);

            IObservable<Mat> ScaleLfp(IObservable<NeuropixelsV1DataFrame> frames, bool cmr) =>
                new NeuropixelsV1Scale
                {
                    Band = NeuropixelsV1EphysBand.Lfp,
                    AmplifierGain = configuration.LfpAmplifierGain,
                    UseCommonMedianReference = cmr
                }.Process(frames);

            // NB: each band is declared beside the transform that produces it so the two cannot drift.
            var bands = new List<(ProbeScopeBand Band, Func<IObservable<NeuropixelsV1DataFrame>, bool, IObservable<Mat>> Transform)>();

            // NB: with the hardware spike filter off, the spike stream is wideband, so it is offered as
            // such and a software spike band is carved out of it, as for NeuropixelsV2.
            if (configuration.SpikeFilter)
            {
                bands.Add((new("Spike", "300 Hz to 9 kHz", spikeRate), ScaleSpike));
            }
            else
            {
                bands.Add((new("Wideband", "0.2 Hz to 9 kHz", spikeRate), ScaleSpike));
                bands.Add((new("Spike", "300 Hz to 9 kHz", spikeRate), (frames, cmr) => new Butterworth
                {
                    SampleRate = spikeRate,
                    Cutoff1 = 300.0,
                    Cutoff2 = 9000.0,
                    FilterType = FilterType.BandPass,
                    FilterOrder = 2
                }.Process(ScaleSpike(frames, cmr))));
            }

            bands.Add((new("LFP", "0.2 Hz to 500 Hz", lfpRate), ScaleLfp));

            transforms = bands.Select(b => b.Transform).ToArray();
            return new(v1.ProbeGroup, bands.Select(b => b.Band).ToArray());
        }

        private protected override IObservable<Mat> ProcessBand(
            int band, bool commonMedianReference, IObservable<NeuropixelsV1DataFrame> frames) =>
            transforms[band](frames, commonMedianReference);
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
