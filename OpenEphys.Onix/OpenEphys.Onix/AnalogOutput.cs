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
                    var ioDeviceInfo = (AnalogIODeviceInfo)deviceInfo;
                    var divisionsPerVolt = Array.ConvertAll(ioDeviceInfo.VoltsPerDivision, value => 1 / value);
                    return source.Do(data =>
                    {
                        if (data.Rows != AnalogIO.ChannelCount)
                        {
                            throw new InvalidOperationException(
                                $"The input data matrix must have exactly {AnalogIO.ChannelCount} channels."
                            );
                        }

                        if (dataType == AnalogIODataType.S16 && data.Depth != Depth.S16 ||
                            dataType == AnalogIODataType.Volts && data.Depth != Depth.F32)
                        {
                            throw new InvalidOperationException(
                                $"Invalid input data type '{data.Depth}' for the specified analog IO configuration."
                            );
                        }

                        if (bufferSize != data.Cols)
                        {
                            bufferSize = data.Cols;
                            sampleScale = dataType == AnalogIODataType.Volts
                                ? CreateSampleScale(bufferSize, divisionsPerVolt)
                                : null;
                            inputBuffer = new Mat(data.Cols, data.Rows, data.Depth, 1);
                            tempBuffer = sampleScale != null ? new Mat(data.Cols, data.Rows, Depth.S16, 1) : null;
                        }

                        var outputBuffer = inputBuffer;
                        CV.Transpose(data, outputBuffer);
                        if (sampleScale != null)
                        {
                            CV.Mul(inputBuffer, sampleScale, inputBuffer);
                            CV.Convert(inputBuffer, tempBuffer);
                            outputBuffer = tempBuffer;
                        }

                        var dataSize = inputBuffer.Step * inputBuffer.Rows;
                        device.Write(inputBuffer.Data, dataSize);
                    });
                }));
        }

        public IObservable<short[]> Process(IObservable<short[]> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    return source.Do(device.Write);
                }));
        }
    }
}
