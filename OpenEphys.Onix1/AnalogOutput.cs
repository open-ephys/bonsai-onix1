using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <inheritdoc cref = "AnalogOutput"/>
    [Obsolete("Use AnalogOutput instead. This operator will be removed in version 1.0.0")]
    public class BreakoutAnalogOutput : AnalogOutput { }

    /// <summary>
    /// Sends analog output data to an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureAnalogIO"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Sends analog output data to an ONIX breakout board.")]
    public class AnalogOutput : Sink<Mat>
    {
        const AnalogIOVoltageRange OutputRange = AnalogIOVoltageRange.TenVolts;

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(AnalogIO.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the data type used to represent analog samples.
        /// </summary>
        /// <remarks>
        /// If <see cref="AnalogIODataType.S16"/> is selected, each DAC value is represented by a
        /// signed, twos-complement encoded 16-bit integer. In this case, the output voltage always
        /// corresponds to <see cref="AnalogIOVoltageRange.TenVolts"/>. When <see
        /// cref="AnalogIODataType.Volts"/> is selected, 32-bit floating point voltages between -10
        /// and 10 volts are sent directly to the DACs.
        /// </remarks>
        [Description("The data type used to represent analog samples.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public AnalogIODataType DataType { get; set; } = AnalogIODataType.S16;

        /// <summary>
        /// Send an matrix of samples to all enabled analog outputs.
        /// </summary>
        /// <remarks>
        /// If a matrix contains multiple samples, they will be written to hardware as quickly as
        /// communication allows. The data within each input matrix must have <see cref="Depth.S16"/> when
        /// <c>DataType</c> is set to <see cref="AnalogIODataType.S16"/> or <see cref="Depth.F32"/>
        /// when <c>DataType</c> is set to <see cref="AnalogIODataType.Volts"/>.
        /// </remarks>
        /// <param name="source"> A sequence of 12xN sample matrices containing the analog data to write to
        /// channels 0 to 11.</param>
        /// <returns> A sequence of 12xN sample matrices containing the analog data that were written to
        /// channels 0 to 11.</returns>
        public override unsafe IObservable<Mat> Process(IObservable<Mat> source)
        {
            var dataType = DataType;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var bufferSize = 0;
                var scaleBuffer = default(Mat);
                var transposeBuffer = default(Mat);
                var sampleScale = dataType == AnalogIODataType.Volts
                    ? 1 / AnalogIODeviceInfo.GetVoltsPerDivision(OutputRange)
                    : 1;
                var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                return source.Do(data =>
                {
                    if (dataType == AnalogIODataType.S16 && data.Depth != Depth.S16 ||
                        dataType == AnalogIODataType.Volts && data.Depth != Depth.F32)
                    {
                        ThrowDataTypeException(data.Depth);
                    }

                    AssertChannelCount(data.Rows);
                    if (bufferSize != data.Cols)
                    {
                        bufferSize = data.Cols;
                        transposeBuffer = bufferSize > 1
                            ? new Mat(data.Cols, data.Rows, data.Depth, 1)
                            : null;
                        if (sampleScale != 1)
                        {
                            scaleBuffer = transposeBuffer != null
                                ? new Mat(data.Cols, data.Rows, Depth.S16, 1)
                                : new Mat(data.Rows, data.Cols, Depth.S16, 1);
                        }
                        else scaleBuffer = null;
                    }

                    var outputBuffer = data;
                    if (transposeBuffer != null)
                    {
                        CV.Transpose(outputBuffer, transposeBuffer);
                        outputBuffer = transposeBuffer;
                    }

                    if (scaleBuffer != null)
                    {
                        CV.ConvertScale(outputBuffer, scaleBuffer, sampleScale);
                        outputBuffer = scaleBuffer;
                    }

                    var dataSize = outputBuffer.Step * outputBuffer.Rows;

                    // twos-complement to offset binary
                    const short Mask = -32768;
                    CV.XorS(outputBuffer, new Scalar(Mask, 0, 0), outputBuffer);
                    device.Write(outputBuffer.Data, dataSize);
                });
            });
        }

        /// <summary>
        /// Send an 12-element array of values to update all enabled analog outputs.
        /// </summary>
        /// <remarks>
        /// This overload should be used when <c>DataType</c> is set to <see
        /// cref="AnalogIODataType.S16"/> and values should be within -32,768 to 32,767, which
        /// correspond to -10.0 to 10.0 volts.
        /// </remarks>
        /// <param name="source"> A sequence of 12x1 element arrays each containing the analog data to write
        /// to channels 0 to 11.</param>
        /// <returns> A sequence of 12x1 element arrays each containing the analog data to write to channels 0
        /// to 11.</returns>
        public IObservable<short[]> Process(IObservable<short[]> source)
        {
            if (DataType != AnalogIODataType.S16)
                ThrowDataTypeException(Depth.S16);

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                return source.Do(data =>
                {
                    AssertChannelCount(data.Length);
                    var samples = new ushort[data.Length];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        const short Mask = -32768;
                        data[i] ^= Mask; // twos-complement to offset binary
                    }
                    device.Write(data);
                });
            });
        }

        /// <summary>
        /// Send an 12-element array of values to update all enabled analog outputs.
        /// </summary>
        /// <remarks>
        /// This overload should be used when <c>DataType</c> is set to <see
        /// cref="AnalogIODataType.Volts"/> and values should be within -10.0 to 10.0 volts.
        /// </remarks>
        /// <param name="source"> A sequence of 12x1 element arrays each containing the analog data to write
        /// to channels 0 to 11.</param>
        /// <returns> A sequence of 12x1 element arrays each containing the analog data to write to channels 0
        /// to 11.</returns>
        public IObservable<float[]> Process(IObservable<float[]> source)
        {
            if (DataType != AnalogIODataType.Volts)
                ThrowDataTypeException(Depth.F32);

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                var divisionsPerVolt = 1 / AnalogIODeviceInfo.GetVoltsPerDivision(OutputRange);
                return source.Do(data =>
                {
                    AssertChannelCount(data.Length);
                    var samples = new ushort[data.Length];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        samples[i] = (ushort)(data[i] * divisionsPerVolt + AnalogIO.DacMidScale);
                    }

                    device.Write(samples);
                });
            });
        }

        static void AssertChannelCount(int channels)
        {
            if (channels != AnalogIO.ChannelCount)
            {
                throw new InvalidOperationException(
                    $"The input data must have exactly {AnalogIO.ChannelCount} channels."
                );
            }
        }

        static void ThrowDataTypeException(Depth depth)
        {
            throw new InvalidOperationException(
                $"Invalid input data type '{depth}' for the specified analog IO configuration."
            );
        }
    }
}
