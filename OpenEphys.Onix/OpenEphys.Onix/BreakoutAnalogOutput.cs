using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class BreakoutAnalogOutput : Sink<Mat>
    {
        const BreakoutAnalogIOVoltageRange OutputRange = BreakoutAnalogIOVoltageRange.TenVolts;

        [TypeConverter(typeof(BreakoutAnalogIO.NameConverter))]
        public string DeviceName { get; set; }

        public BreakoutAnalogIODataType DataType { get; set; } = BreakoutAnalogIODataType.S16;

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            var dataType = DataType;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var bufferSize = 0;
                var scaleBuffer = default(Mat);
                var transposeBuffer = default(Mat);
                var sampleScale = dataType == BreakoutAnalogIODataType.Volts
                    ? 1 / BreakoutAnalogIODeviceInfo.GetVoltsPerDivision(OutputRange)
                    : 1;
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutAnalogIO));
                return source.Do(data =>
                {
                    if (dataType == BreakoutAnalogIODataType.S16 && data.Depth != Depth.S16 ||
                        dataType == BreakoutAnalogIODataType.Volts && data.Depth != Depth.F32)
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
                    device.Write(outputBuffer.Data, dataSize);
                });
            });
        }

        public IObservable<short[]> Process(IObservable<short[]> source)
        {
            if (DataType != BreakoutAnalogIODataType.S16)
                ThrowDataTypeException(Depth.S16);

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutAnalogIO));
                return source.Do(data =>
                {
                    AssertChannelCount(data.Length);
                    device.Write(data);
                });
            });
        }

        public IObservable<float[]> Process(IObservable<float[]> source)
        {
            if (DataType != BreakoutAnalogIODataType.Volts)
                ThrowDataTypeException(Depth.F32);

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutAnalogIO));
                var divisionsPerVolt = 1 / BreakoutAnalogIODeviceInfo.GetVoltsPerDivision(OutputRange);
                return source.Do(data =>
                {
                    AssertChannelCount(data.Length);
                    var samples = new short[data.Length];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        samples[i] = (short)(data[i] * divisionsPerVolt);
                    }

                    device.Write(samples);
                });
            });
        }

        static void AssertChannelCount(int channels)
        {
            if (channels != BreakoutAnalogIO.ChannelCount)
            {
                throw new InvalidOperationException(
                    $"The input data must have exactly {BreakoutAnalogIO.ChannelCount} channels."
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
