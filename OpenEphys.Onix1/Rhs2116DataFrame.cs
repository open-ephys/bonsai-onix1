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
        /// Gets the high-gain AC-coupled ephys amplifier data.
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
        /// Gets the DC-coupled low-gain amplifier data for monitoring stimulation waveforms.
        /// </summary>
        /// <remarks>
        /// Each row corresponds to a channel. Each column corresponds to a sample whose time is indicated by
        /// the corresponding element <see cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>.
        /// Samples are 10-bits each and are represented using unsigned 16-bit integers. To convert to
        /// micro-volts, the following equation can be used:
        /// <code>
        /// V_electrode (mV) = -19.23 mV × (ADC result – 512)
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
