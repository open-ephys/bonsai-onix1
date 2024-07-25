using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class NeuropixelsV1eData : Source<NeuropixelsV1eDataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV1e.NameConverter))]
        public string DeviceName { get; set; }

        int bufferSize = 36;
        [Description("Number of super-frames (384 channels from spike band and 32 channels from " +
            "LFP band) to buffer before propogating data. Must be a mulitple of 12.")]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (int)(Math.Ceiling((double)value / NeuropixelsV1e.FramesPerRoundRobin) * NeuropixelsV1e.FramesPerRoundRobin);
        }

        public unsafe override IObservable<NeuropixelsV1eDataFrame> Generate()
        {
            var spikeBufferSize = BufferSize;
            var lfpBufferSize = spikeBufferSize / NeuropixelsV1e.FramesPerRoundRobin;

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV1eDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(NeuropixelsV1e));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context.GetDeviceFrames(passthrough.Address);

                return Observable.Create<NeuropixelsV1eDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var spikeBuffer = new ushort[NeuropixelsV1e.ChannelCount, spikeBufferSize];
                    var lfpBuffer = new ushort[NeuropixelsV1e.ChannelCount, lfpBufferSize];
                    var frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1e.FramesPerSuperFrame];
                    var hubClockBuffer = new ulong[spikeBufferSize];
                    var clockBuffer = new ulong[spikeBufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV1ePayload*)frame.Data.ToPointer();
                            NeuropixelsV1eDataFrame.CopyAmplifierBuffer(payload->AmplifierData, frameCountBuffer, spikeBuffer, lfpBuffer, sampleIndex, info.ApGainCorrection, info.LfpGainCorrection, info.AdcThresholds, info.AdcOffsets);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= spikeBufferSize)
                            {
                                var spikeData = Mat.FromArray(spikeBuffer);
                                var lfpData = Mat.FromArray(lfpBuffer);
                                observer.OnNext(new NeuropixelsV1eDataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, spikeData, lfpData));
                                frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1e.FramesPerSuperFrame];
                                hubClockBuffer = new ulong[spikeBufferSize];
                                clockBuffer = new ulong[spikeBufferSize];
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
