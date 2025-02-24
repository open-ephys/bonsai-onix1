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
        /// Data has 64 rows which represent electrophysiology channels and columns which represent
        /// samples acquired at 30 kHz. Each column corresponds to an ADC sample whose time is
        /// indicated by the corresponding elements in <see cref="DataFrame.Clock"/> and <see
        /// cref="DataFrame.HubClock"/>. Each ADC sample is a 16-bit, offset binary value encoded
        /// as a <see cref="ushort"/>. TThe following equation can be used to convert it to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.195 × (ADC Sample – 32768)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the buffered auxiliary data array.
        /// </summary>
        /// <remarks>
        /// Data has 3 rows representing electrophysiology channels and columns representing samples acquired at 30
        /// kHz. Each column corresponds to an ADC sample whose time is indicated by the
        /// corresponding element <see cref="DataFrame.Clock"/> and <see
        /// cref="DataFrame.HubClock"/>. Each ADC sample is a 16-bit, offset binary value encoded
        /// as a <see cref="ushort"/>. The following equation can be used to convert it to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.195 × (ADC Sample – 32768)
        /// </code>
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
