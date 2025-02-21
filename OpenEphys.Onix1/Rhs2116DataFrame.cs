using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from an RHS2116 device.
    /// </summary>
    public class Rhs2116DataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116DataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of multi-channel amplifier data.</param>
        /// <param name="dcData">An array of multi-channel DC data.</param>
        public Rhs2116DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat dcData)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
            DCData = dcData;
        }

        /// <summary>
        /// Gets the high-gain AC-coupled amplifier data array.
        /// </summary>
        /// <remarks>
        /// Data has 16 rows which represent AC-coupled electrophysiology channels and columns which represent
        /// samples acquired at 30 kHz. Each column corresponds to an ADC sample whose time is
        /// indicated by the corresponding elements in <see cref="DataFrame.Clock"/> and <see
        /// cref="DataFrame.HubClock"/>. Each ADC sample is an 16-bit, offset binary value encoded
        /// as a <see cref="ushort"/>. The following equation can be used to convert it to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.195 × (ADC Sample – 32768)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the DC-coupled low-gain amplifier data array for monitoring stimulation waveforms.
        /// </summary>
        /// <remarks>
        /// Data has 16 rows which represent DC-coupled electrophysiology channels and columns which represent
        /// samples acquired at 30 kHz. Each column corresponds to an ADC sample whose time is
        /// indicated by the corresponding elements in <see cref="DataFrame.Clock"/> and <see
        /// cref="DataFrame.HubClock"/>. Each ADC sample is a 16-bit, offset binary value encoded
        /// as a <see cref="ushort"/>. The following equation can be used to convert it to microvolts:
        /// <code>
        /// Electrode Voltage (mV) = -19.23 × (ADC Sample – 512)
        /// </code>
        /// </remarks>
        public Mat DCData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhs2116Payload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[Rhs2116.AmplifierChannelCount];
        public fixed ushort DCData[Rhs2116.AmplifierChannelCount];
        public short Pad;
    }
}
