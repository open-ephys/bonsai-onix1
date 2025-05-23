using System;
using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a Rhs2116 device.
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
        /// <param name="recoveryMask">An array of stimulus artifact recovery mask values.</param>
        public Rhs2116DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat dcData, ushort[] recoveryMask)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
            DCData = dcData;
            RecoveryMask = recoveryMask;
        }

        /// <summary>
        /// Gets the high-gain electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Electrophysiology samples are organized in a 16xN matrix with 16 rows representing electrophysiology
        /// channel and N columns representing sample index. Each column is a 16-channel vector of ADC
        /// samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is an 16-bit,
        /// offset binary value encoded as a <see cref="ushort"/>. The following equation can be used to
        /// convert a sample to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.195 × (ADC Sample – 32768)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the DC-coupled, low-gain amplifier data array for monitoring stimulation waveforms.
        /// </summary>
        /// <remarks>
        /// DC-coupled samples are organized in 16xN matrix with 16 rows representing electrophysiology
        /// channel and N columns representing sample index. Each column is a 16-channel vector of ADC
        /// samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is an 10-bit,
        /// offset binary value encoded as a <see cref="ushort"/>. The following equation can be used to
        /// convert a sample to millivolts:
        /// <code>
        /// Electrode Voltage (mV) = -19.23 × (ADC Sample – 512)
        /// </code>
        /// </remarks>
        public Mat DCData { get; }

        /// <summary>
        /// Gets the stimulus artifact recovery mask array.
        /// </summary>
        /// <remarks>
        /// During and following stimulus pulses, electrophysiology amplifiers can enter an artifact recovery
        /// mode which discharges the electrode and applies an aggressive high-pass filter to prevent
        /// amplifier saturation (see <see cref="ConfigureRhs2116.AnalogLowCutoffRecovery"/>). This 1xN vector
        /// provides a per-sample bit mask indicating if artifact recovery mode is active on each of the 16
        /// electrophysiology channels. The bit position in each value indicates the channel number and the
        /// logical state indicates if recovery is active. For instance a value of 0x0011 would indicate that
        /// channels 0 and 4 were in artifact recovery mode and all other channels were in normal operation.
        /// </remarks>
        public ushort[] RecoveryMask { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhs2116Payload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[Rhs2116.AmplifierChannelCount];
        public fixed ushort DCData[Rhs2116.AmplifierChannelCount];
        public ushort Recovery;
    }
}
