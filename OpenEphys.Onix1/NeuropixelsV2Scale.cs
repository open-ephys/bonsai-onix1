using Bonsai;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Converts NeuropixelsV2 amplifier data to microvolts and optionally applies per-ADC common median
    /// referencing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies the equation
    /// <code>
    /// Electrode Voltage (µV) = 3.05176 × (ADC Sample – 2048)
    /// </code>
    /// to each element of the input matrix and returns a new <see cref="Depth.F32"/> matrix in microvolts.
    /// Connect <see cref="NeuropixelsV2DataFrame.AmplifierData"/> upstream.
    /// </para>
    /// <para>
    /// When <see cref="UseCommonMedianReference"/> is <see langword="true"/>, per-ADC common median
    /// referencing (CMR) is applied to the scaled output. Neuropixels probes use time-division multiplexed
    /// ADCs, each of which samples a fixed subset of channels. Because each ADC can introduce a unique DC
    /// offset, CMR is performed within each ADC's channel group independently rather than across all channels
    /// at once. For each group and each time sample, the group median is subtracted from every channel in
    /// that group. Unlike mean-based referencing, the median is robust to outliers such as channels with
    /// large artifacts or atypically high firing rates. The output is always <see cref="Depth.F32"/>,
    /// allowing the CMR residual to be negative without clipping.
    /// </para>
    /// </remarks>
    [Combinator]
    [Description("Converts NeuropixelsV2 amplifier data to microvolts and optionally applies per-ADC common median referencing.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class NeuropixelsV2Scale
    {
        /// <summary>
        /// Gets or sets a value indicating whether per-ADC common median referencing is applied to the scaled
        /// amplifier data.
        /// </summary>
        [Description("Apply per-ADC common median referencing to the scaled amplifier data.")]
        public bool UseCommonMedianReference { get; set; } = false;

        /// <summary>
        /// Converts each frame in <paramref name="source"/> to microvolts and optionally applies per-ADC
        /// common median referencing.
        /// </summary>
        /// <param name="source">
        /// A sequence of 384×N matrices of 12-bit unsigned offset-binary samples from a NeuropixelsV2 probe
        /// (<see cref="Depth.U16"/>).
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Depth.F32"/> matrices containing electrode voltages in microvolts.
        /// </returns>
        public IObservable<Mat> Process(IObservable<NeuropixelsV2DataFrame> source)
        {
            var groups = NeuropixelsV2.AdcChannelGroups();
            var useCmr = UseCommonMedianReference;

            return source.Select(input => {
                var output = new Mat(input.AmplifierData.Size, Depth.F32, input.AmplifierData.Channels);
                CV.ConvertScale(input.AmplifierData, output, 3.05176, 3.05176 * -NeuropixelsV2.AdcMidpoint);
                return useCmr ? Neuropixels.ApplyCmrF32(output, groups) : output;
            });
        }
    }
}
