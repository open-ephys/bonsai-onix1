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
        /// Each row corresponds to a channel. Each column corresponds to a sample whose time is indicated by
        /// the corresponding element <see cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>.
        /// Samples are 16-bits each and are represented using unsigned 16-bit integers. To convert to
        /// micro-volts, the following equation can be used:
        /// <code>
        /// V_electrode (uV) = 0.195 µV × (ADC result – 32768)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the buffered auxiliary data array.
        /// </summary>
        /// <remarks>
        /// Each row corresponds to a channel. Each column corresponds to a sample whose time is indicated by
        /// the corresponding element <see cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>.
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
