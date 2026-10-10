using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

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
        private protected override string Unit => "uV";

        (ProbeScopeBand Band,
            Func<IObservable<NeuropixelsV2DataFrame>, IObservable<Mat>> Transform,
            Func<IObservable<NeuropixelsV2DataFrame>, IObservable<Mat>> AcTransform)[] bands;

        private protected override ProbeScopeSource CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV2PsbDecoderDeviceInfo v2)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV2 probe this scope can display.");

            const int sampleRate = NeuropixelsV2.SamplesPerChannelPerSecond;
            const int lfpDecimation = 12;
            const int lfpRate = sampleRate / lfpDecimation;

            static IObservable<Mat> Scale(IObservable<NeuropixelsV2DataFrame> frames) =>
                new NeuropixelsV2Scale().Process(frames);

            static IObservable<Mat> LowPass(IObservable<Mat> source) =>
                new Butterworth
                {
                    SampleRate = sampleRate,
                    Cutoff1 = 500.0,
                    FilterType = FilterType.LowPass,
                    FilterOrder = 2
                }.Process(source);

            // NB: down to the rate NeuropixelsV1's LFP band arrives at, which makes everything downstream twelve
            // times cheaper and the history twelve times longer. Content within 500 Hz of a multiple of the new
            // rate folds into the passband, so the low-pass runs twice ahead of keeping every 12th sample, some
            // 48 dB down by 2 kHz; Decimate's own anti-aliasing FIR barely attenuates there.
            static IObservable<Mat> Lfp(IObservable<NeuropixelsV2DataFrame> frames) =>
                new Decimate
                {
                    Factor = lfpDecimation,
                    Downsampling = DownsamplingMethod.None
                }.Process(LowPass(LowPass(Scale(frames))));

            static IObservable<Mat> AcCouple(IObservable<Mat> source, int rate) =>
                new Butterworth
                {
                    SampleRate = rate,
                    Cutoff1 = 1.0,
                    FilterType = FilterType.HighPass,
                    FilterOrder = 2
                }.Process(source);

            // NB: each band is declared beside the transforms that produce it so they cannot drift.
            bands = new (ProbeScopeBand Band,
                Func<IObservable<NeuropixelsV2DataFrame>, IObservable<Mat>> Transform,
                Func<IObservable<NeuropixelsV2DataFrame>, IObservable<Mat>> AcTransform)[]
            {
                (new("Wideband", "0.5 Hz to 10 kHz", sampleRate, "1 Hz to 10 kHz"),
                    Scale,
                    frames => AcCouple(Scale(frames), sampleRate)),
                (new("Spike", "300 Hz to 9 kHz", sampleRate, null), frames => new Butterworth
                {
                    SampleRate = sampleRate,
                    Cutoff1 = 300.0,
                    Cutoff2 = 9000.0,
                    FilterType = FilterType.BandPass,
                    FilterOrder = 2
                }.Process(Scale(frames)), null),
                (new("LFP", "0.5 Hz to 500 Hz", lfpRate, "1 Hz to 500 Hz"),
                    Lfp,
                    frames => AcCouple(Lfp(frames), lfpRate)),
            };

            return new(v2.ProbeGroup, bands.Select(b => b.Band).ToArray());
        }

        private protected override bool[] Clipped(NeuropixelsV2DataFrame frame, BandSelection selection) =>
            ClippedRows(frame.AmplifierData, (1 << NeuropixelsV2.AdcBits) - 1);

        private protected override IObservable<Mat> ProcessBand(
            BandSelection selection, IObservable<NeuropixelsV2DataFrame> frames)
        {
            var (_, transform, acTransform) = bands[selection.Band];
            var output = selection.AcCoupled && acTransform is not null ? acTransform(frames) : transform(frames);
            if (selection.CommonMedianReference)
            {
                var adcGroups = NeuropixelsV2.AdcChannelGroups();
                output = output.Select(data => Neuropixels.ApplyCmrF32(data, adcGroups));
            }
            return output;
        }
    }

    /// <summary>
    /// Marks a point downstream of a <see cref="NeuropixelsV2eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    [TypeVisualizer(typeof(NeuropixelsV2ProbeScopeVisualizer))]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV2 probe.")]
    public class NeuropixelsV2ProbeScope : ProbeScope<NeuropixelsV2DataFrame>
    {
        const double MaxHistorySeconds = 10;

        /// <inheritdoc/>
        [TypeConverter(typeof(NeuropixelsV2.NameConverter))]
        [Description("The name of the device whose data is displayed. Leave empty to use the upstream data " +
            "operator's device if it can be unambiguously resolved.")]
        public override string DeviceName { get; set; }

        /// <inheritdoc/>
        internal override long HistoryBytes =>
            (long)(Math.Min(HistorySeconds, MaxHistorySeconds)
                * NeuropixelsV2.SamplesPerChannelPerSecond
                * NeuropixelsV2.ChannelCount
                * sizeof(float));
    }
}
