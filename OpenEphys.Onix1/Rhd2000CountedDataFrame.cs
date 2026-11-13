using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents electrophysiology and auxiliary data produced by an Rhd2000 bioamplifier chip, along with
    /// the sample indexes for each sample
    /// </summary>
    public class Rhd2000CountedDataFrame : Rhd2000DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rhd2000CountedDataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock"> An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of multi-channel electrophysiology data.</param>
        /// <param name="auxData">An array of auxiliary channel data.</param>
        /// <param name="sampleIndexes">An array of sample indexes for each sample.</param>
        public Rhd2000CountedDataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat auxData, ushort[] sampleIndexes)
            : base(clock, hubClock, amplifierData, auxData)
        {
            SampleIndexes = sampleIndexes;
        }

        /// <summary>
        /// Gets the sample indexes for each sample in the frame.
        /// </summary>
        public ushort[] SampleIndexes { get; }
    }
}
