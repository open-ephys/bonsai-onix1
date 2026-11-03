using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Marks a point downstream of a <see cref="NeuropixelsV1eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    /// <remarks>
    /// Offers the hardware spike and LFP bands, plus a wideband view and a software spike band when the probe's
    /// spike filter is off.
    /// </remarks>
    [TypeVisualizer(typeof(ProbeScopeVisualizer<NeuropixelsV1DataFrame>))]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV1 probe.")]
    public class NeuropixelsV1ProbeScope : ProbeScope<NeuropixelsV1DataFrame>
    {
        const double MaxHistorySeconds = 10;

        /// <inheritdoc/>
        [TypeConverter(typeof(NeuropixelsV1.NameConverter))]
        [Description("The name of the device whose data is displayed. Leave empty to use the upstream data " +
            "operator's device if it can be unambiguously resolved.")]
        public override string DeviceName { get; set; }

        /// <inheritdoc/>
        internal override long HistoryBytes =>
            (long)(Math.Min(HistorySeconds, MaxHistorySeconds)
                * NeuropixelsV1.SamplesPerChannelPerSecond
                * NeuropixelsV1.ChannelCount
                * sizeof(float));

        internal override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV1eData data ? data.DeviceName : null;

        internal override string Unit => "uV";

        internal override double MaxRange => 5000;

        internal override double MinRange => 10;

        internal override ProbeScopeSource<NeuropixelsV1DataFrame> CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV1PsbDecoderDeviceInfo v1)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV1 probe this scope can display.");

            var configuration = v1.ProbeConfiguration
                ?? throw new InvalidOperationException("The device has no probe configuration, so its amplifier gain is unknown.");

            const int spikeRate = NeuropixelsV1.SamplesPerChannelPerSecond;
            const int lfpRate = NeuropixelsV1.SamplesPerChannelPerSecond / NeuropixelsV1.FramesPerRoundRobin;

            var spikeGain = configuration.SpikeAmplifierGain;
            var lfpGain = configuration.LfpAmplifierGain;
            IObservable<Mat> ScaleSpike(IObservable<Mat> counts) =>
                counts.Select(data => NeuropixelsV1Scale.ToMicrovolts(data, spikeGain));
            IObservable<Mat> ScaleLfp(IObservable<Mat> counts) =>
                counts.Select(data => NeuropixelsV1Scale.ToMicrovolts(data, lfpGain));

            static IObservable<Mat> AcCouple(IObservable<Mat> source, int sampleRate) =>
                new Butterworth
                {
                    SampleRate = sampleRate,
                    Cutoff1 = 1.0,
                    FilterType = FilterType.HighPass,
                    FilterOrder = 2
                }.Process(source);

            // NB: the spike and LFP amplifiers have their own gains and clip apart, so each band is checked on the
            // stream it comes from.
            static Mat RawSpike(NeuropixelsV1DataFrame frame) => frame.SpikeData;
            static Mat RawLfp(NeuropixelsV1DataFrame frame) => frame.LfpData;

            var adcGroups = NeuropixelsV1.AdcChannelGroups();
            IObservable<Mat> Reference(IObservable<Mat> source) =>
                source.Select(data => Neuropixels.ApplyCmrF32(data, adcGroups));

            // NB: each band is declared beside the transforms that produce it so they cannot drift.
            var bands = new List<ProbeScopeBand<NeuropixelsV1DataFrame>>();

            // NB: with the hardware spike filter off, the spike stream is wideband, so it is offered as
            // such and a software spike band is carved out of it, as for NeuropixelsV2.
            if (configuration.SpikeFilter)
            {
                bands.Add(new("Spike", "300 Hz to 9 kHz", spikeRate, RawSpike, ScaleSpike));
            }
            else
            {
                bands.Add(new("Wideband", "0.2 Hz to 9 kHz", spikeRate, RawSpike, ScaleSpike,
                    AcDescription: "1 Hz to 9 kHz",
                    AcTransform: counts => AcCouple(ScaleSpike(counts), spikeRate)));
                bands.Add(new("Spike", SpikeDetection.BandDescription, spikeRate, RawSpike,
                    counts => SpikeDetection.SpikeBand(ScaleSpike(counts), spikeRate)));
            }

            bands.Add(new("LFP", "0.2 Hz to 500 Hz", lfpRate, RawLfp, ScaleLfp,
                AcDescription: "1 Hz to 500 Hz",
                AcTransform: counts => AcCouple(ScaleLfp(counts), lfpRate)));

            return new(v1.ProbeGroup, bands, Reference, (1 << NeuropixelsV1.AdcBits) - 1);
        }
    }
}
