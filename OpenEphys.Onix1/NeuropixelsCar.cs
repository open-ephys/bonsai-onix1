using Bonsai;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Applies per-ADC common average referencing to a sequence of Neuropixels amplifier data matrices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Neuropixels probes use time-division multiplexed ADCs, each of which samples a fixed
    /// subset of channels. Because each ADC can introduce a unique DC offset, common average
    /// referencing (CAR) is performed within each ADC's channel group independently rather than
    /// across all channels at once. For each group and each time sample, the group mean is
    /// subtracted from every channel in that group.
    /// </para>
    /// <para>
    /// Set <see cref="ProbeVersion"/> to match the probe that produced the input matrix so that
    /// the correct ADC channel grouping is applied. Connect
    /// <see cref="NeuropixelsV1DataFrame.SpikeData"/> or
    /// <see cref="NeuropixelsV1DataFrame.LfpData"/> for V1 data, or
    /// <see cref="NeuropixelsV2DataFrame.AmplifierData"/> for V2 data.
    /// </para>
    /// <para>
    /// The output is always <see cref="Depth.F32"/>, allowing the CAR residual to be negative
    /// without clipping.
    /// </para>
    /// </remarks>
    [Combinator]
    [Description("Applies per-ADC common average referencing (CAR) to a sequence of Neuropixels amplifier data matrices.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class NeuropixelsCar
    {
        /// <summary>
        /// Gets or sets the Neuropixels probe version that produced the input matrix.
        /// </summary>
        [Description("Neuropixels probe version that produced the input matrix.")]
        public NeuropixelsAsicVersion ProbeVersion { get; set; } = NeuropixelsAsicVersion.V2;

        /// <summary>
        /// Applies per-ADC CAR to each matrix in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">
        /// A sequence of 384×N matrices produced by a Neuropixels probe.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Depth.F32"/> matrices with the same dimensions as the input,
        /// containing per-ADC mean-subtracted values.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            var groups = ProbeVersion == NeuropixelsAsicVersion.V2
                ? NeuropixelsV2.AdcChannelGroups()
                : NeuropixelsV1.AdcChannelGroups();
            int expectedChannels = groups.Sum(g => g.Length);
            return source.Select(input => ApplyCar(input, groups, expectedChannels));
        }

        static unsafe Mat ApplyCar(Mat input, int[][] groups, int expectedChannels)
        {
            int channels = input.Rows;
            int samples = input.Cols;

            if (channels != expectedChannels)
                throw new InvalidOperationException(
                    $"Input matrix has {channels} rows but the ADC groups cover {expectedChannels} channels.");

            var output = new Mat(channels, samples, Depth.F32, 1);
            CV.Convert(input, output);

            int step = output.Step;
            byte* ptr = (byte*)output.Data.ToPointer();

            foreach (var group in groups)
            {
                for (int t = 0; t < samples; t++)
                {
                    float sum = 0f;
                    foreach (int ch in group)
                        sum += *(float*)(ptr + ch * step + t * sizeof(float));
                    float mean = sum / group.Length;
                    foreach (int ch in group)
                        *(float*)(ptr + ch * step + t * sizeof(float)) -= mean;
                }
            }

            return output;
        }
    }
}
