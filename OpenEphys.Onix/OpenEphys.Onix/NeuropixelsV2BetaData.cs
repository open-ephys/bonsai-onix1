using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2BetaData : Source<NeuropixelsV2BetaDataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV2Beta.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<NeuropixelsV2BetaDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2Beta));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .GroupBy(frame => NeuropixelsV2BetaDataFrame.GetProbeIndex(frame))
                        .SelectMany(probe => Observable.Create<NeuropixelsV2BetaDataFrame>(observer =>
                        {
                            var sampleIndex = 0;
                            var amplifierBuffer = new ushort[NeuropixelsV2Beta.ChannelCount, bufferSize];
                            var frameCounter = new int[NeuropixelsV2Beta.FramesPerSuperFrame * bufferSize];
                            var hubClockBuffer = new ulong[bufferSize];
                            var clockBuffer = new ulong[bufferSize];

                            var frameObserver = Observer.Create<oni.Frame>(
                                frame =>
                                {
                                    var payload = (NeuropixelsV2BetaPayload*)frame.Data.ToPointer();
                                    NeuropixelsV2BetaDataFrame.CopyAmplifierBuffer(payload->SuperFrame, amplifierBuffer, frameCounter, sampleIndex);
                                    hubClockBuffer[sampleIndex] = BitHelper.SwapEndian(payload->HubClock);
                                    clockBuffer[sampleIndex] = frame.Clock;
                                    if (++sampleIndex >= bufferSize)
                                    {
                                        var amplifierData = Mat.FromArray(amplifierBuffer);
                                        var dataFrame = new NeuropixelsV2BetaDataFrame(
                                            clockBuffer,
                                            hubClockBuffer,
                                            probe.Key,
                                            amplifierData,
                                            frameCounter);
                                        observer.OnNext(dataFrame);
                                        frameCounter = new int[NeuropixelsV2Beta.FramesPerSuperFrame * bufferSize];
                                        hubClockBuffer = new ulong[bufferSize];
                                        clockBuffer = new ulong[bufferSize];
                                        sampleIndex = 0;
                                    }
                                },
                                observer.OnError,
                                observer.OnCompleted);
                            return probe.SubscribeSafe(frameObserver);
                        }));
                }));
        }
    }
}
