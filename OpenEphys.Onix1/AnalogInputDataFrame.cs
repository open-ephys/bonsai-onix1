using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered analog data produced by the ONIX breakout board.
    /// </summary>
    public class AnalogInputDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogInputDataFrame"/> class.
        /// </summary>
        /// <param name="clock">A buffered array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock"> A buffered array of hub clock counter values.</param>
        /// <param name="analogData">A buffered array of multi-channel analog data.</param>
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubClock, Mat analogData)
            : base(clock, hubClock)
        {
            AnalogData = analogData;
        }

        /// <summary>
        /// Gets the buffered analog data array.
        /// </summary>
        /// <remarks>
        /// Analog samples are organized in 12xN matrix with rows representing channel number and
        /// N columns representing samples acquired at 100 kHz. Each column is a 12-channel vector of ADC
        /// samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. When <see
        /// cref="AnalogInput.DataType"/> is set to <see cref="AnalogIODataType.Volts"/>, each sample is
        /// internally converted to a voltage value and represented using a <see cref="float"/>. When <see
        /// cref="AnalogInput.DataType"/> is set to <see cref="AnalogIODataType.S16"/>, each 16-bit ADC sample
        /// is represented as a <see cref="short"/>. In this case, the following equation can be used to
        /// convert a sample to volts:
        /// <code> 
        /// Channel Voltage (V) = ADC Sample × (Input Span / 2^16)
        /// </code> 
        /// where <c>Input Span</c> is 5V, 10V, or 20V  when the <see cref="AnalogIOVoltageRange"/> is set to
        /// ±2.5V, ±5V, or ±10V, respectively. Note that <see cref="AnalogIOVoltageRange"/> can be set
        /// independently for each channel in <see cref="ConfigureBreakoutBoard.AnalogIO"/>. Therefore, the
        /// conversion factor may be different for each channel.
        /// </remarks>
        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AnalogInputPayload
    {
        public ulong HubClock;
        public fixed short AnalogData[AnalogIO.ChannelCount];
    }
}
