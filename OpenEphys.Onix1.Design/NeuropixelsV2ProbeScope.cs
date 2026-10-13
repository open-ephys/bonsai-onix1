using Bonsai;
using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Marks a point downstream of a <see cref="NeuropixelsV2eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    /// <remarks>
    /// Offers the probe's single wideband output, and spike and LFP bands filtered from it.
    /// </remarks>
    [TypeVisualizer(typeof(ProbeScopeVisualizer<NeuropixelsV2DataFrame>))]
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

        internal override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV2eData data ? data.DeviceName : null;

        internal override string Unit => "uV";

        internal override ProbeScopeSource<NeuropixelsV2DataFrame> CreateSource(DeviceInfo info)
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

            // NB: every band comes from the one wideband stream.
            static Mat Raw(NeuropixelsV2DataFrame frame) => frame.AmplifierData;

            var adcGroups = NeuropixelsV2.AdcChannelGroups();
            IObservable<Mat> Reference(IObservable<Mat> source) =>
                source.Select(data => Neuropixels.ApplyCmrF32(data, adcGroups));

            // NB: each band is declared beside the transforms that produce it so they cannot drift.
            var bands = new ProbeScopeBand<NeuropixelsV2DataFrame>[]
            {
                new("Wideband", "0.5 Hz to 10 kHz", sampleRate, Scale, Raw,
                    AcDescription: "1 Hz to 10 kHz",
                    AcTransform: frames => AcCouple(Scale(frames), sampleRate)),
                new("Spike", "300 Hz to 9 kHz", sampleRate,
                    frames => new Butterworth
                    {
                        SampleRate = sampleRate,
                        Cutoff1 = 300.0,
                        Cutoff2 = 9000.0,
                        FilterType = FilterType.BandPass,
                        FilterOrder = 2
                    }.Process(Scale(frames)),
                    Raw),
                new("LFP", "0.5 Hz to 500 Hz", lfpRate, Lfp, Raw,
                    AcDescription: "1 Hz to 500 Hz",
                    AcTransform: frames => AcCouple(Lfp(frames), lfpRate)),
            };

            return new(v2.ProbeGroup, bands, Reference, (1 << NeuropixelsV2.AdcBits) - 1);
        }
    }
}
