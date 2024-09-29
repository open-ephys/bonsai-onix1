using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    public class NeuropixelsV1fData : Source<NeuropixelsV1fDataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV1f.NameConverter))]
        public string DeviceName { get; set; }

        int bufferSize = 36;
        [Description("Number of super-frames (384 channels from spike band and 32 channels from " +
            "LFP band) to buffer before propagating data. Must be a multiple of 12.")]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (int)(Math.Ceiling((double)value / NeuropixelsV1.FramesPerRoundRobin) * NeuropixelsV1.FramesPerRoundRobin);
        }

        public unsafe override IObservable<NeuropixelsV1fDataFrame> Generate()
        {
            var spikeBufferSize = BufferSize;
            var lfpBufferSize = spikeBufferSize / NeuropixelsV1.FramesPerRoundRobin;

            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<NeuropixelsV1fDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                        var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV1f));
                        var spikeBuffer = new ushort[NeuropixelsV1.ChannelCount, spikeBufferSize];
                        var lfpBuffer = new ushort[NeuropixelsV1.ChannelCount, lfpBufferSize];
                        var frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
                        var hubClockBuffer = new ulong[spikeBufferSize];
                        var clockBuffer = new ulong[spikeBufferSize];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (NeuropixelsV1fPayload*)frame.Data.ToPointer();
                                NeuropixelsV1fDataFrame.CopyAmplifierBuffer(payload->AmplifierData, frameCountBuffer, spikeBuffer, lfpBuffer, sampleIndex);
                                hubClockBuffer[sampleIndex] = payload->HubClock;
                                clockBuffer[sampleIndex] = frame.Clock;

                                if (++sampleIndex >= spikeBufferSize)
                                {
                                    var spikeData = Mat.FromArray(spikeBuffer);
                                    var lfpData = Mat.FromArray(lfpBuffer);
                                    observer.OnNext(new NeuropixelsV1fDataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, spikeData, lfpData));
                                    frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
                                    hubClockBuffer = new ulong[spikeBufferSize];
                                    clockBuffer = new ulong[spikeBufferSize];
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
