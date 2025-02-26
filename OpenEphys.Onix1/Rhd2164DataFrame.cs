using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Electrophysiology data produced by an Rhd2164 bioamplifier chip.
    /// </summary>
    public class Rhd2164DataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rhd2164DataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock"> An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of Rhd2164 multi-channel electrophysiology data.</param>
        /// <param name="auxData">An array of Rhd2164 auxiliary channel data.</param>
        public Rhd2164DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat auxData)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
            AuxData = auxData;
        }

        /// <summary>
        /// Gets the buffered electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Electrophysiology samples are organized in 64xN matrix with rows representing electrophysiology
        /// channel number and N columns representing sample index. Each column is a 64-channel vector of ADC
        /// samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 16-bit,
        /// offset binary value encoded as a <see cref="ushort"/>. The following equation can be used to
        /// convert a sample to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.195 × (ADC Sample – 32768)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the buffered auxiliary data array. 
        /// </summary>
        /// <remarks>
        /// Auxiliary samples are organized in 3xN matrix with rows representing electrophysiology channel
        /// number and N columns representing sample index. Each column is a 3-channel vector of ADC samples
        /// whose acquisition time is indicated by the corresponding elements in <see cref="DataFrame.Clock"/>
        /// and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 16-bit <see cref="ushort"/>. The
        /// following equation can be used to convert a sample to volts:
        /// <code>
        /// Auxiliary Voltage (V) = 0.0000374 × ADC Sample
        /// </code>
        /// Note that auxiliary inputs have a 0.10-2.45V input range. Nonlinearities may occur if voltages
        /// outside of this range are applied to auxiliary inputs.
        /// </remarks>
        public Mat AuxData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhd2164Payload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[Rhd2164.AmplifierChannelCount];
        public fixed ushort AuxData[Rhd2164.AuxChannelCount];
    }
}
