using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a NeuropixelsV2 probe.
    /// </summary>
    public class NeuropixelsV2DataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2DataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of multi-channel amplifier data.</param>
        public NeuropixelsV2DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
        }

        /// <summary>
        /// Gets the buffered electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Electrophysiology samples are organized in 384xN matrix with rows representing electrophysiology
        /// channel number and N columns representing samples acquired at 30 kHz. Each column is a 384-channel
        /// vector of ADC samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 12-bit, offset
        /// binary value represented as a <see cref="ushort"/>. The following equation can be used to convert
        /// a sample to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 3.05176 × (ADC Sample – 2048)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }
    }
}
