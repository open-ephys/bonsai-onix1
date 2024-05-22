using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class AnalogOutput : Sink<Mat>
    {
        [TypeConverter(typeof(AnalogIO.NameConverter))]
        public string DeviceName { get; set; }

        public AnalogIODataType DataType { get; set; } = AnalogIODataType.S16;

        static float[] GetDivisionsPerVolt(DeviceInfo deviceInfo)
        {
            var ioDeviceInfo = (AnalogIODeviceInfo)deviceInfo;
            return Array.ConvertAll(ioDeviceInfo.VoltsPerDivision, value => 1 / value);
        }

        static Mat CreateSampleScale(int bufferSize, float[] divisionsPerVolt)
        {
            using var scaleHeader = Mat.CreateMatHeader(divisionsPerVolt);
            var voltageScale = new Mat(bufferSize, scaleHeader.Cols, scaleHeader.Depth, scaleHeader.Channels);
            CV.Repeat(scaleHeader, voltageScale);
            return voltageScale;
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            var dataType = DataType;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var bufferSize = 0;
                    var tempBuffer = default(Mat);
                    var sampleScale = default(Mat);
                    var inputBuffer = default(Mat);
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    var divisionsPerVolt = GetDivisionsPerVolt(deviceInfo);
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
                            sampleScale = dataType == AnalogIODataType.Volts
                                ? CreateSampleScale(bufferSize, divisionsPerVolt)
                                : null;
                            if (bufferSize > 1 || sampleScale != null)
                            {
                                inputBuffer = new Mat(data.Cols, data.Rows, data.Depth, 1);
                                tempBuffer = sampleScale != null ? new Mat(data.Cols, data.Rows, Depth.S16, 1) : null;
                            }
                            else inputBuffer = tempBuffer = null;
                        }

                        var outputBuffer = inputBuffer;
                        if (inputBuffer == null) outputBuffer = data;
                        else
                        {
                            CV.Transpose(data, inputBuffer);
                            if (sampleScale != null)
                            {
                                CV.Mul(inputBuffer, sampleScale, inputBuffer);
                                CV.Convert(inputBuffer, tempBuffer);
                                outputBuffer = tempBuffer;
                            }
                        }

                        var dataSize = outputBuffer.Step * outputBuffer.Rows;
                        device.Write(outputBuffer.Data, dataSize);
                    });
                }));
        }

        public IObservable<short[]> Process(IObservable<short[]> source)
        {
            if (DataType != AnalogIODataType.S16)
                ThrowDataTypeException(Depth.S16);

            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    return source.Do(data =>
                    {
                        AssertChannelCount(data.Length);
                        device.Write(data);
                    });
                }));
        }

        public IObservable<float[]> Process(IObservable<float[]> source)
        {
            if (DataType != AnalogIODataType.Volts)
                ThrowDataTypeException(Depth.F32);

            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    var divisionsPerVolt = GetDivisionsPerVolt(deviceInfo);
                    return source.Do(data =>
                    {
                        AssertChannelCount(data.Length);
                        var samples = new short[data.Length];
                        for (int i = 0; i < samples.Length; i++)
                        {
                            samples[i] = (short)(data[i] * divisionsPerVolt[i]);
                        }

                        device.Write(samples);
                    });
                }));
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
