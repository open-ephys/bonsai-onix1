using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a NeuropixelsV1 probe.
    /// </summary>
    public class NeuropixelsV1DataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1DataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="frameCount">An array of frame count values.</param>
        /// <param name="spikeData">An array of multi-channel spike data as a <see cref="Mat"/> object.</param>
        /// <param name="lfpData">An array of multi-channel LFP data as a <see cref="Mat"/> object.</param>
        public NeuropixelsV1DataFrame(ulong[] clock, ulong[] hubClock, int[] frameCount, Mat spikeData, Mat lfpData)
            : base(clock, hubClock)
        {
            FrameCount = frameCount;
            SpikeData = spikeData;
            LfpData = lfpData;
        }

        /// <summary>
        /// Gets the frame count value array.
        /// </summary>
        /// <remarks>
        /// A 20-bit counter on the probe that increments its value for every "frame" produced by the probe.
        /// Thirteen frames are produced for each 384-channel column of samples in  <see cref="SpikeData"/>.
        /// The value ranges from 0 to 1048575 (2^20-1), and should always increment by 1 until it wraps
        /// around back to 0. This can be used to detect dropped frames.
        /// </remarks>
        public int[] FrameCount { get; }

        /// <summary>
        /// Gets spike-band electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Spike-band (0.3-10 kHz) samples are organized in 384xN matrix with rows representing
        /// channel number and N columns representing samples acquired at 30 kHz. Each column is a 384-channel
        /// vector of ADC samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 10-bit, offset
        /// binary value represented as a <see cref="ushort"/>. The following equation can be used to convert
        /// a sample to microvolts:
        /// <code> 
        /// Electrode Voltage (µV) = (1,171.875 / AP Gain) × (ADC Sample – 512) 
        /// </code>
        /// where <c>AP Gain</c> can be 50, 125, 250, 500, 1000, 1500, 2000, or 3000 depending on the value of <see
        /// cref="NeuropixelsV1ProbeConfiguration.SpikeAmplifierGain"/>.
        /// </remarks>
        public Mat SpikeData { get; }

        /// <summary>
        /// Gets LFP-band electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// LFP-band (0.5-500 Hz) samples are organized in 384xN matrix with rows representing channel
        /// number and N columns representing samples acquired at 2.5 kHz. Each column is a 384-channel vector
        /// of ADC samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 10-bit, offset
        /// binary value represented as a <see cref="ushort"/>. The following equation can be used to convert
        /// a sample to microvolts:
        /// <code> 
        /// Electrode Voltage (µV) = (1,171.875 / LFP Gain) × (ADC Sample – 512)
        /// </code>
        /// where <c>LFP Gain</c> can be 50, 125, 250, 500, 1000, 1500, 2000, or 3000 depending on the value of <see
        /// cref="NeuropixelsV1ProbeConfiguration.LfpAmplifierGain"/>.
        /// </remarks>
        public Mat LfpData { get; }
    }
}
