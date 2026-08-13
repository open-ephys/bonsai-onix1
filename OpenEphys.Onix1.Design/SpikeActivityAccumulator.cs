using System;
using Bonsai.Dsp;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Owns and accumulates per-channel spike-waveform and noise statistics for one survey round. Shared by
    /// every probe family's electrode-activity survey runner: construct one instance per round (sized to that
    /// probe's channel count), feed it every detected spike collection paired with the exact buffer it was
    /// detected from via <see cref="Accumulate"/>, then read the finished statistics via <see
    /// cref="Summarize"/> once the round's data stream completes.
    /// </summary>
    internal sealed class SpikeActivityAccumulator
    {
        readonly float[] peakToPeakSum;
        readonly int[] spikeCount;
        readonly P2Median[] medianAbsDeviation;

        internal SpikeActivityAccumulator(int channelCount)
        {
            peakToPeakSum = new float[channelCount];
            spikeCount = new int[channelCount];
            medianAbsDeviation = new P2Median[channelCount];
            for (int i = 0; i < channelCount; i++)
                medianAbsDeviation[i] = new P2Median();
        }

        /// <summary>
        /// Accumulates one buffer's worth of spike-detection output into the running per-channel sums:
        /// peak-to-peak amplitude and spike count from <paramref name="spikeCollection"/>, and the noise
        /// (median absolute deviation) estimate from every sample in <paramref
        /// name="spikeDetectionTimeSeries"/> which should be the same buffer <paramref
        /// name="spikeCollection"/> was detected from.
        /// </summary>
        internal void Accumulate(Mat spikeDetectionTimeSeries, SpikeWaveformCollection spikeCollection)
        {
            foreach (var spike in spikeCollection)
            {
                CV.MinMaxLoc(spike.Waveform, out double minVal, out double maxVal, out _, out _);
                peakToPeakSum[spike.ChannelIndex] += (float)Math.Abs(maxVal - minVal);
                spikeCount[spike.ChannelIndex]++;
            }

            for (int ch = 0; ch < spikeDetectionTimeSeries.Rows; ch++)
                for (int s = 0; s < spikeDetectionTimeSeries.Cols; s++)
                    medianAbsDeviation[ch].Update(Math.Abs((float)spikeDetectionTimeSeries.GetReal(ch, s)));
        }

        /// <summary>
        /// Derives final per-channel amplitude, firing rate, and noise from every call to <see
        /// cref="Accumulate"/> so far.
        /// </summary>
        internal (float[] Amplitude, float[] FireRate, float[] Noise) Summarize(double durationSeconds)
        {
            int n = peakToPeakSum.Length;
            var amplitude = new float[n];
            var fireRate = new float[n];
            var noise = new float[n];
            for (int ch = 0; ch < n; ch++)
            {
                amplitude[ch] = spikeCount[ch] > 0 ? peakToPeakSum[ch] / spikeCount[ch] : 0f;
                fireRate[ch] = (float)(spikeCount[ch] / durationSeconds);
                noise[ch] = medianAbsDeviation[ch].Median / 0.6745f;
            }
            return (amplitude, fireRate, noise);
        }
    }
}
