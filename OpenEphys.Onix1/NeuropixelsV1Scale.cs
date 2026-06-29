using Bonsai;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Converts NeuropixelsV1 amplifier data to microvolts.
    /// </summary>
    /// <remarks>
    /// Applies the equation
    /// <code>
    /// Electrode Voltage (µV) = (1171.875 / AmplifierGain) × (ADC Sample – 512)
    /// </code>
    /// to each element of the input matrix and returns a new <see cref="Depth.F32"/> matrix
    /// in microvolts. Set <see cref="Gain"/> to match the amplifier gain configured on the probe
    /// at acquisition time. Connect either <see cref="NeuropixelsV1DataFrame.SpikeData"/> or
    /// <see cref="NeuropixelsV1DataFrame.LfpData"/> upstream and set <see cref="Gain"/>
    /// accordingly.
    /// </remarks>
    [Combinator]
    [Description("Converts NeuropixelsV1 amplifier data to microvolts.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class NeuropixelsV1Scale
    {
        /// <summary>
        /// Gets or sets the amplifier gain configured on the probe at acquisition time.
        /// </summary>
        [Description("Amplifier gain configured on the probe at acquisition time.")]
        public NeuropixelsV1Gain Gain { get; set; } = NeuropixelsV1Gain.Gain500;

        /// <summary>
        /// Converts each matrix in <paramref name="source"/> to microvolts.
        /// </summary>
        /// <param name="source">
        /// A sequence of 384×N matrices of 10-bit unsigned offset-binary samples from a
        /// NeuropixelsV1 probe (<see cref="Depth.U16"/>).
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Depth.F32"/> matrices containing electrode voltages
        /// in microvolts.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            double alpha = 1171.875 / ToGainValue(Gain);
            double beta  = alpha * -NeuropixelsV1.AdcMidpoint;
            return source.Select(input =>
            {
                var output = new Mat(input.Size, Depth.F32, input.Channels);
                CV.ConvertScale(input, output, alpha, beta);
                return output;
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
