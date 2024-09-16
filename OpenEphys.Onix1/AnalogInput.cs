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
    [Obsolete]
    public class BreakoutAnalogInput : AnalogInput { }

    /// <summary>
    /// Produces a sequence of analog input frames from an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureAnalogIO"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of analog input frames from an ONIX breakout board.")]
    public class AnalogInput : Source<AnalogInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(AnalogIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the number of samples collected for each channel that are use to create a single <see cref="AnalogInputDataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property determines the number of analog samples that are buffered for each channel before data is propagated. For instance, if this
        /// value is set to 100, then 100 samples, along with corresponding clock values, will be collected from each of the input channels
        /// and packed into each <see cref="AnalogInputDataFrame"/>. Because channels are sampled at 100 kHz, this is equivalent to 1
        /// millisecond of data from each channel.
        /// </remarks>
        [Description("The number of analog samples that are buffered for each channel before data is propagated.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the data type used to represent analog samples.
        /// </summary>
        /// <remarks>
        /// If <see cref="AnalogIODataType.S16"/> is selected, each ADC sample is represented at a signed, twos-complement encoded
        /// 16-bit integer. <see cref="AnalogIODataType.S16"/> samples can be converted to a voltage using each channels'
        /// <see cref="AnalogIOVoltageRange"/> selection. For instance, channel 0 can be converted using <see cref="ConfigureAnalogIO.InputRange0"/>.
        /// When <see cref="AnalogIODataType.Volts"/> is selected, the voltage conversion is performed automatically and samples
        /// are represented as 32-bit floating point voltages.
        /// </remarks>
        [Description("The data type used to represent analog samples.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public AnalogIODataType DataType { get; set; } = AnalogIODataType.S16;

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
        /// Generates a sequence of <see cref="AnalogInputDataFrame"/>.
        /// </summary>
        /// <returns>A sequence of <see cref="AnalogInputDataFrame"/></returns>
        public unsafe override IObservable<AnalogInputDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            var dataType = DataType;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<AnalogInputDataFrame>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    var ioDeviceInfo = (AnalogIODeviceInfo)deviceInfo;

                    var sampleIndex = 0;
                    var voltageScale = dataType == AnalogIODataType.Volts
                        ? CreateVoltageScale(bufferSize, ioDeviceInfo.VoltsPerDivision)
                        : null;
                    var transposeBuffer = voltageScale != null
                        ? new Mat(AnalogIO.ChannelCount, bufferSize, Depth.S16, 1)
                        : null;
                    var analogDataBuffer = new short[AnalogIO.ChannelCount * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (AnalogInputPayload*)frame.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payload->AnalogData), analogDataBuffer, sampleIndex * AnalogIO.ChannelCount, AnalogIO.ChannelCount);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var analogData = BufferHelper.CopyTranspose(
                                    analogDataBuffer,
                                    bufferSize,
                                    AnalogIO.ChannelCount,
                                    Depth.S16,
                                    voltageScale,
                                    transposeBuffer);
                                observer.OnNext(new AnalogInputDataFrame(clockBuffer, hubClockBuffer, analogData));
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
