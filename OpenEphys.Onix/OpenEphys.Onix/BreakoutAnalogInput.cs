using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class BreakoutAnalogInput : Source<BreakoutAnalogInputDataFrame>
    {
        [TypeConverter(typeof(BreakoutAnalogIO.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 100;

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
