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

        public unsafe override IObservable<AnalogInputDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<AnalogInputDataFrame>(observer =>
                    {
                        var sampleIndex = 0;
                        var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                        var analogDataBuffer = new short[AnalogIO.ChannelCount * bufferSize];
                        var hubSyncCounterBuffer = new ulong[bufferSize];
                        var clockBuffer = new ulong[bufferSize];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (AnalogInputPayload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->AnalogData), analogDataBuffer, sampleIndex * AnalogIO.ChannelCount, AnalogIO.ChannelCount);
                                hubSyncCounterBuffer[sampleIndex] = payload->HubSyncCounter;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize)
                                {
                                    var analogData = BufferHelper.CopyBuffer(analogDataBuffer, bufferSize, AnalogIO.ChannelCount, Depth.S16);
                                    observer.OnNext(new AnalogInputDataFrame(clockBuffer, hubSyncCounterBuffer, analogData));
                                    hubSyncCounterBuffer = new ulong[bufferSize];
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
