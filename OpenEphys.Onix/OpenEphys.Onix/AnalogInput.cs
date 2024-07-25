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
    public class AnalogInput : Source<AnalogInputDataFrame>
    {
        [TypeConverter(typeof(AnalogIO.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 100;

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

        public unsafe override IObservable<AnalogInputDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            var dataType = DataType;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<AnalogInputDataFrame>(observer =>
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
                        return deviceInfo.Context.FrameReceived
                            .Where(frame => frame.DeviceAddress == device.Address)
                            .SubscribeSafe(frameObserver);
                    })));
        }
    }
}
