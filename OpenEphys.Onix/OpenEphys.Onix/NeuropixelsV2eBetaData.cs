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
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV2eDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(NeuropixelsV2eBeta));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context
                    .GetDeviceFrames(passthrough.Address)
                    .Where(frame => NeuropixelsV2eBetaDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);

                var gainCorrection = ProbeIndex switch
                {
                    NeuropixelsV2Probe.ProbeA => (ushort)info.GainCorrectionA,
                    NeuropixelsV2Probe.ProbeB => (ushort)info.GainCorrectionB,
                    _ => throw new ArgumentOutOfRangeException(nameof(ProbeIndex), $"Unexpected ProbeIndex value: {ProbeIndex}"),
                };

                return Observable.Create<NeuropixelsV2eBetaDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var amplifierBuffer = new ushort[NeuropixelsV2.ChannelCount, bufferSize];
                    var frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV2BetaPayload*)frame.Data.ToPointer();
                            NeuropixelsV2eBetaDataFrame.CopyAmplifierBuffer(payload->SuperFrame, amplifierBuffer, frameCounter, sampleIndex, gainCorrection);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = Mat.FromArray(amplifierBuffer);
                                var dataFrame = new NeuropixelsV2eBetaDataFrame(
                                    clockBuffer,
                                    hubClockBuffer,
                                    amplifierData,
                                    frameCounter);
                                observer.OnNext(dataFrame);
                                frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    return probeData.SubscribeSafe(frameObserver);
                });
            });
        }
    }
}
