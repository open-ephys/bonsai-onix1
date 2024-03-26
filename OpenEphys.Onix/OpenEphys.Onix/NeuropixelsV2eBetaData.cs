using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2eBetaData : Source<NeuropixelsV2eBetaDataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV2eBeta.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public NeuropixelsV2Probe ProbeIndex { get; set; }

        public unsafe override IObservable<NeuropixelsV2eBetaDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2eBeta));
                    var passthrough = device.GetPassthroughDeviceContext(DS90UB9x.ID);
                    var probeData = device.Context.FrameReceived.Where(frame =>
                        frame.DeviceAddress == passthrough.Address &&
                        NeuropixelsV2eBetaDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);
                    return Observable.Create<NeuropixelsV2eBetaDataFrame>(observer =>
                    {
                        var sampleIndex = 0;
                        var amplifierBuffer = new ushort[NeuropixelsV2eBeta.ChannelCount, bufferSize];
                        var frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
                        var hubSyncCounterBuffer = new ulong[bufferSize];
                        var clockBuffer = new ulong[bufferSize];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (NeuropixelsV2BetaPayload*)frame.Data.ToPointer();
                                NeuropixelsV2eBetaDataFrame.CopyAmplifierBuffer(payload->SuperFrame, amplifierBuffer, frameCounter, sampleIndex);
                                hubSyncCounterBuffer[sampleIndex] = payload->HubSyncCounter;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize)
                                {
                                    var amplifierData = Mat.FromArray(amplifierBuffer);
                                    var dataFrame = new NeuropixelsV2eBetaDataFrame(
                                        clockBuffer,
                                        hubSyncCounterBuffer,
                                        amplifierData,
                                        frameCounter);
                                    observer.OnNext(dataFrame);
                                    frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
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
