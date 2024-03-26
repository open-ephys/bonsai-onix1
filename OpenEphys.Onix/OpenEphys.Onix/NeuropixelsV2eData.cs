using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2eData : Source<NeuropixelsV2eDataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV2e.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public NeuropixelsV2Probe ProbeIndex { get; set; }

        public unsafe override IObservable<NeuropixelsV2eDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2e));
                    var passthrough = device.GetPassthroughDeviceContext(DS90UB9x.ID);
                    var probeData = device.Context.FrameReceived.Where(frame =>
                        frame.DeviceAddress == passthrough.Address &&
                        NeuropixelsV2eDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);
                    return Observable.Create<NeuropixelsV2eDataFrame>(observer =>
                    {
                        var sampleIndex = 0;
                        var amplifierBuffer = new ushort[NeuropixelsV2e.ChannelCount, bufferSize];
                        var hubSyncCounterBuffer = new ulong[bufferSize];
                        var clockBuffer = new ulong[bufferSize];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (NeuropixelsV2Payload*)frame.Data.ToPointer();
                                NeuropixelsV2eDataFrame.CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, sampleIndex);
                                hubSyncCounterBuffer[sampleIndex] = payload->HubSyncCounter;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize)
                                {
                                    var amplifierData = Mat.FromArray(amplifierBuffer);
                                    observer.OnNext(new NeuropixelsV2eDataFrame(clockBuffer, hubSyncCounterBuffer, amplifierData));
                                    hubSyncCounterBuffer = new ulong[bufferSize];
                                    clockBuffer = new ulong[bufferSize];
                                    sampleIndex = 0;
                                }
                            },
                            observer.OnError,
                            observer.OnCompleted);
                        return probeData.SubscribeSafe(frameObserver);
                    });
                }));
        }
    }
}
