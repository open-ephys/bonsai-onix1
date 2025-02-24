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
        /// Data has 16 rows which represent analog input channels and columns which represent
        /// samples acquired at 100 kHz. Each column corresponds to an ADC sample whose time is
        /// indicated by the corresponding elements in <see cref="DataFrame.Clock"/> and <see
        /// cref="DataFrame.HubClock"/>. When <c>DataType</c> in <see
        /// cref="OpenEphys.Onix1.AnalogInput"/> is set to <c>Volts</c>, each pre-converted voltage
        /// value is encoded as a <see cref="float"/>. When <c>DataType</c> is set to <c>S16</c>,
        /// each raw 16-bit ADC samples is encoded as a <see cref="short"/>. In this case, the
        /// following equation can be used to convert it to volts:
        /// <code> 
        /// Analog Voltage (V) = Voltage Range / 2^16 × ADC Sample 
        /// </code> 
        /// where voltage range can be 5, 10, or 20 depending on how the analog input voltage range
        /// is configured (±2.5, ±5, or ±10 volts) in <see
        /// cref="OpenEphys.Onix1.ConfigureBreakoutBoard.AnalogIO"/>.
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
