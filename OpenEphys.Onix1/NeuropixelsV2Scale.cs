using Bonsai;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Converts NeuropixelsV2 amplifier data to microvolts.
    /// </summary>
    /// <remarks>
    /// Applies the equation
    /// <code>
    /// Electrode Voltage (µV) = 3.05176 × (ADC Sample – 2048)
    /// </code>
    /// to each element of the input matrix and returns a new <see cref="Depth.F32"/> matrix
    /// in microvolts. Connect <see cref="NeuropixelsV2DataFrame.AmplifierData"/> upstream.
    /// </remarks>
    [Combinator]
    [Description("Converts NeuropixelsV2 amplifier data to microvolts.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class NeuropixelsV2Scale
    {
        /// <summary>
        /// Converts each matrix in <paramref name="source"/> to microvolts.
        /// </summary>
        /// <param name="source">
        /// A sequence of 384×N matrices of 12-bit unsigned offset-binary samples from a
        /// NeuropixelsV2 probe (<see cref="Depth.U16"/>).
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Depth.F32"/> matrices containing electrode voltages
        /// in microvolts.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var output = new Mat(input.Size, Depth.F32, input.Channels);
                CV.ConvertScale(input, output, 3.05176, 3.05176 * -NeuropixelsV2.AdcMidpoint);
                return output;
            });
        }
    }
}
