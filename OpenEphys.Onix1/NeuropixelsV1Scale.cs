using Bonsai;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Converts a single frequency band of NeuropixelsV1 amplifier data to microvolts and
    /// optionally applies per-ADC common median referencing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies the equation
    /// <code>
    /// Electrode Voltage (µV) = (1171.875 / AmplifierGain) × (ADC Sample – 512)
    /// </code>
    /// to each element of the selected band matrix and returns a new <see cref="Depth.F32"/>
    /// matrix in microvolts. Set <see cref="Band"/> to select the spike or LFP band and set
    /// <see cref="AmplifierGain"/> to match the gain configured for that band on the probe at
    /// acquisition time. Connect <see cref="NeuropixelsV1DataFrame"/> upstream.
    /// </para>
    /// <para>
    /// When <see cref="UseCommonMedianReference"/> is <see langword="true"/>, per-ADC common
    /// median referencing (CMR) is applied to the scaled output. Neuropixels probes use
    /// time-division multiplexed ADCs, each of which samples a fixed subset of channels.
    /// Because each ADC can introduce a unique DC offset, CMR is performed within each ADC's
    /// channel group independently rather than across all channels at once. For each group and
    /// each time sample, the group median is subtracted from every channel in that group.
    /// Unlike mean-based referencing, the median is robust to outliers such as channels with
    /// large artifacts or atypically high firing rates. The output is always
    /// <see cref="Depth.F32"/>, allowing the CMR residual to be negative without clipping.
    /// </para>
    /// </remarks>
    [Combinator]
    [Description("Converts a single frequency band of NeuropixelsV1 amplifier data to microvolts and optionally applies per-ADC common median referencing.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class NeuropixelsV1Scale
    {
        /// <summary>
        /// Gets or sets the frequency band to extract from each <see cref="NeuropixelsV1DataFrame"/>.
        /// </summary>
        [Description("Frequency band to extract and scale: Spike or Lfp.")]
        public NeuropixelsV1EphysBand Band { get; set; } = NeuropixelsV1EphysBand.Spike;

        /// <summary>
        /// Gets or sets the amplifier gain configured for the selected band at acquisition time.
        /// </summary>
        /// <remarks>
        /// Set to match <see cref="NeuropixelsV1ProbeConfiguration.SpikeAmplifierGain"/> when
        /// <see cref="Band"/> is <see cref="NeuropixelsV1EphysBand.Spike"/>, or
        /// <see cref="NeuropixelsV1ProbeConfiguration.LfpAmplifierGain"/> when
        /// <see cref="Band"/> is <see cref="NeuropixelsV1EphysBand.Lfp"/>.
        /// </remarks>
        [Description("Amplifier gain configured for the selected band at acquisition time.")]
        public NeuropixelsV1Gain AmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        /// <summary>
        /// Gets or sets a value indicating whether per-ADC common median referencing is
        /// applied to the scaled output.
        /// </summary>
        [Description("Apply per-ADC common median referencing to the scaled amplifier data.")]
        public bool UseCommonMedianReference { get; set; } = false;

        /// <summary>
        /// Converts each frame in <paramref name="source"/> to microvolts and optionally
        /// applies per-ADC common median referencing.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="NeuropixelsV1DataFrame"/> items containing 10-bit unsigned
        /// offset-binary spike and LFP samples (<see cref="Depth.U16"/>).
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Depth.F32"/> matrices containing electrode voltages in
        /// microvolts for the selected band.
        /// </returns>
        public IObservable<Mat> Process(IObservable<NeuropixelsV1DataFrame> source)
        {
            double alpha = 1171.875 / ToGainValue(AmplifierGain);
            double beta = alpha * -NeuropixelsV1.AdcMidpoint;
            var useCmr = UseCommonMedianReference;
            var adcGroups = NeuropixelsV1.AdcChannelGroups();

            Func<NeuropixelsV1DataFrame, Mat> getRaw = Band switch
            {
                NeuropixelsV1EphysBand.Spike => f => f.SpikeData,
                NeuropixelsV1EphysBand.Lfp => f => f.LfpData,
                _ => throw new ArgumentOutOfRangeException(nameof(Band), $"Unexpected {nameof(NeuropixelsV1EphysBand)} value: {Band}"),
            };

            return source.Select(input =>
            {
                var raw = getRaw(input);
                var output = new Mat(raw.Size, Depth.F32, raw.Channels);
                CV.ConvertScale(raw, output, alpha, beta);
                return useCmr ? Neuropixels.ApplyCmrF32(output, adcGroups) : output;
            });
        }

        static int ToGainValue(NeuropixelsV1Gain gain) => gain switch
        {
            NeuropixelsV1Gain.Gain50   => 50,
            NeuropixelsV1Gain.Gain125  => 125,
            NeuropixelsV1Gain.Gain250  => 250,
            NeuropixelsV1Gain.Gain500  => 500,
            NeuropixelsV1Gain.Gain1000 => 1000,
            NeuropixelsV1Gain.Gain1500 => 1500,
            NeuropixelsV1Gain.Gain2000 => 2000,
            NeuropixelsV1Gain.Gain3000 => 3000,
            _ => throw new ArgumentOutOfRangeException(nameof(gain))
        };
    }
}
