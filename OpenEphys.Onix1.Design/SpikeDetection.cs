using System;
using Bonsai.Dsp;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// How spikes are found, shared by the configuration dialogs' activity survey and the probe scopes, so the two
    /// always detect alike: the spike band, the detector's settings, and each channel's noise.
    /// </summary>
    internal static class SpikeDetection
    {
        /// <summary>
        /// The lower edge of the spike band, in Hz.
        /// </summary>
        internal const double LowCutoff = 300;

        /// <summary>
        /// The upper edge of the spike band, in Hz.
        /// </summary>
        internal const double HighCutoff = 6000;

        /// <summary>
        /// The spike band as shown to the user.
        /// </summary>
        internal const string BandDescription = "300 Hz to 6 kHz";

        /// <summary>
        /// The duration of each detected waveform, in seconds.
        /// </summary>
        internal const double WaveformSeconds = 0.002;

        /// <summary>
        /// The time in each waveform before the threshold crossing, in seconds.
        /// </summary>
        internal const double WaveformDelaySeconds = 0.0005;

        /// <summary>
        /// The samples in each waveform, and those of them before the threshold crossing, in a signal sampled at
        /// <paramref name="sampleRate"/>, so a waveform spans the same time in any band.
        /// </summary>
        internal static (int Length, int Delay) WaveformSamples(int sampleRate) =>
            WaveformSamples(sampleRate, WaveformSeconds, WaveformDelaySeconds);

        /// <summary>
        /// The samples in a waveform <paramref name="seconds"/> long, and those of them in the first
        /// <paramref name="delaySeconds"/>, in a signal sampled at <paramref name="sampleRate"/>, kept to at least two
        /// samples with the delay inside the waveform.
        /// </summary>
        internal static (int Length, int Delay) WaveformSamples(int sampleRate, double seconds, double delaySeconds)
        {
            var length = Math.Max(2, (int)Math.Round(seconds * sampleRate));
            return (length, Math.Max(0, Math.Min(length - 1, (int)Math.Round(delaySeconds * sampleRate))));
        }

        /// <summary>
        /// The threshold the survey starts at, in microvolts, crossed going down.
        /// </summary>
        internal const double DefaultThreshold = -50;

        /// <summary>
        /// Keeps the spike band of a signal sampled at <paramref name="sampleRate"/>.
        /// </summary>
        internal static IObservable<Mat> SpikeBand(IObservable<Mat> source, int sampleRate) =>
            new Butterworth
            {
                SampleRate = sampleRate,
                Cutoff1 = LowCutoff,
                Cutoff2 = HighCutoff,
                FilterType = FilterType.BandPass,
                FilterOrder = 2
            }.Process(source);

        /// <summary>
        /// The signal spikes are found in for a NeuropixelsV2 probe: scaled to microvolts, common median referenced,
        /// and kept to the spike band.
        /// </summary>
        internal static IObservable<Mat> NeuropixelsV2Signal(IObservable<NeuropixelsV2DataFrame> frames) =>
            SpikeBand(new NeuropixelsV2Scale { UseCommonMedianReference = true }.Process(frames),
                NeuropixelsV2.SamplesPerChannelPerSecond);

        /// <summary>
        /// The signal spikes are found in for a NeuropixelsV1 probe: its spike stream scaled to microvolts and common
        /// median referenced, kept to the spike band in software where the probe's spike filter is off.
        /// </summary>
        /// <param name="frames">The probe's frames.</param>
        /// <param name="gain">The spike amplifier's gain.</param>
        /// <param name="spikeFilter">Whether the probe's own spike filter is on, which already shapes the band.</param>
        internal static IObservable<Mat> NeuropixelsV1Signal(
            IObservable<NeuropixelsV1DataFrame> frames, NeuropixelsV1Gain gain, bool spikeFilter)
        {
            var scaled = new NeuropixelsV1Scale
            {
                Band = NeuropixelsV1EphysBand.Spike,
                AmplifierGain = gain,
                UseCommonMedianReference = true
            }.Process(frames);
            return spikeFilter ? scaled : SpikeBand(scaled, NeuropixelsV1.SamplesPerChannelPerSecond);
        }

        /// <summary>
        /// Finds the spikes in <paramref name="signal"/>, by where each channel crosses its threshold.
        /// </summary>
        /// <param name="signal">The signal, one row per channel.</param>
        /// <param name="sampleRate">The rate the signal is sampled at, in Hz.</param>
        /// <param name="thresholds">One threshold for every channel, or one per channel, in the signal's units.</param>
        internal static IObservable<SpikeWaveformCollection> Detect(
            IObservable<Mat> signal, int sampleRate, double[] thresholds)
        {
            var (length, delay) = WaveformSamples(sampleRate);
            return Detect(signal, length, delay, thresholds, SpikeWaveformRefinement.AlignPeaks);
        }

        /// <summary>
        /// Finds the spikes in <paramref name="signal"/>, by where each channel crosses its threshold, each waveform
        /// <paramref name="length"/> samples with <paramref name="delay"/> of them before the crossing, or before the
        /// peak where <paramref name="refinement"/> aligns peaks.
        /// </summary>
        internal static IObservable<SpikeWaveformCollection> Detect(
            IObservable<Mat> signal, int length, int delay, double[] thresholds, SpikeWaveformRefinement refinement) =>
            new DetectSpikes
            {
                Threshold = thresholds,
                Length = length,
                Delay = delay,
                WaveformRefinement = refinement
            }.Process(signal);
    }

    /// <summary>
    /// Estimates each channel's noise from the median absolute deviation of its samples, as MAD / 0.6745, which
    /// spikes barely move.
    /// </summary>
    internal sealed class ChannelNoise
    {
        readonly P2Median[] medianAbsDeviation;

        internal ChannelNoise(int channelCount)
        {
            medianAbsDeviation = new P2Median[channelCount];
            for (int i = 0; i < channelCount; i++)
                medianAbsDeviation[i] = new P2Median();
        }

        /// <summary>
        /// Takes in every sample of a block, one row per channel.
        /// </summary>
        internal void Update(Mat block)
        {
            for (int ch = 0; ch < Math.Min(block.Rows, medianAbsDeviation.Length); ch++)
                for (int s = 0; s < block.Cols; s++)
                    medianAbsDeviation[ch].Update(Math.Abs((float)block.GetReal(ch, s)));
        }

        /// <summary>
        /// Each channel's noise so far, as a standard deviation.
        /// </summary>
        internal float[] Sigma()
        {
            var sigma = new float[medianAbsDeviation.Length];
            for (int ch = 0; ch < sigma.Length; ch++)
                sigma[ch] = medianAbsDeviation[ch].Median / 0.6745f;
            return sigma;
        }
    }
}
