using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of analog input frames from an ONIX breakout board.
    /// </summary>
    [Description("Produces a sequence of analog input frames from an ONIX breakout board.")]
    public class BreakoutAnalogInput : Source<BreakoutAnalogInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(BreakoutAnalogIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the number of samples collected for each channel that are use to create a single <see cref="BreakoutAnalogInputDataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property determines the number of analog samples that are buffered for each channel before data is propagated. For instance, if this
        /// value is set to 100, then 100 samples, along with corresponding clock values, will be collected from each of the input channels
        /// and packed into each <see cref="BreakoutAnalogInputDataFrame"/>. Because channels are sampled at 100 kHz, this is equivalent to 1
        /// millisecond of data from each channel.
        /// </remarks>
        [Description("The number of analog samples that are buffered for each channel before data is propagated.")]
        public int BufferSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the data type used to represent analog samples.
        /// </summary>
        /// <remarks>
        /// If <see cref="BreakoutAnalogIODataType.S16"/> is selected, each ADC sample is represented at a signed, twos-complement encoded
        /// 16-bit integer. <see cref="BreakoutAnalogIODataType.S16"/> samples can be converted to a voltage using each channels'
        /// <see cref="BreakoutAnalogIOVoltageRange"/> selection. For instance, channel 0 can be converted using <see cref="ConfigureBreakoutAnalogIO.InputRange0"/>.
        /// When <see cref="BreakoutAnalogIODataType.Volts"/> is selected, the voltage conversion is performed automatically and samples
        /// are represented as 32-bit floating point voltages.
        /// </remarks>
        [Description("The data type used to represent analog samples.")]
        public BreakoutAnalogIODataType DataType { get; set; } = BreakoutAnalogIODataType.S16;

        static Mat CreateVoltageScale(int bufferSize, float[] voltsPerDivision)
        {

            using var scaleHeader = Mat.CreateMatHeader(
                voltsPerDivision,
                rows: voltsPerDivision.Length,
                cols: 1,
                depth: Depth.F32,
                channels: 1);
            var voltageScale = new Mat(scaleHeader.Rows, bufferSize, scaleHeader.Depth, scaleHeader.Channels);
            CV.Repeat(scaleHeader, voltageScale);
            return voltageScale;
        }

        /// <summary>
        /// Generates a sequence of <see cref="BreakoutAnalogInputDataFrame"/>.
        /// </summary>
        /// <returns>A sequence of <see cref="BreakoutAnalogInputDataFrame"/></returns>
        public unsafe override IObservable<BreakoutAnalogInputDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            var dataType = DataType;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<BreakoutAnalogInputDataFrame>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(BreakoutAnalogIO));
                    var ioDeviceInfo = (BreakoutAnalogIODeviceInfo)deviceInfo;

                    var sampleIndex = 0;
                    var voltageScale = dataType == BreakoutAnalogIODataType.Volts
                        ? CreateVoltageScale(bufferSize, ioDeviceInfo.VoltsPerDivision)
                        : null;
                    var transposeBuffer = voltageScale != null
                        ? new Mat(BreakoutAnalogIO.ChannelCount, bufferSize, Depth.S16, 1)
                        : null;
                    var analogDataBuffer = new short[BreakoutAnalogIO.ChannelCount * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (BreakoutAnalogInputPayload*)frame.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payload->AnalogData), analogDataBuffer, sampleIndex * BreakoutAnalogIO.ChannelCount, BreakoutAnalogIO.ChannelCount);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var analogData = BufferHelper.CopyTranspose(
                                    analogDataBuffer,
                                    bufferSize,
                                    BreakoutAnalogIO.ChannelCount,
                                    Depth.S16,
                                    voltageScale,
                                    transposeBuffer);
                                observer.OnNext(new BreakoutAnalogInputDataFrame(clockBuffer, hubClockBuffer, analogData));
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    return deviceInfo.Context
                        .GetDeviceFrames(device.Address)
                        .SubscribeSafe(frameObserver);
                }));
        }
    }
}
