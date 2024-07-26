using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Produces a sequence of NeuropixelsV1e frames from a NeuropixelsV1e headstage.
    /// </summary>
    public class NeuropixelsV1eData : Source<NeuropixelsV1eDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV1e.NameConverter))]
        public string DeviceName { get; set; }

        int bufferSize = 36;

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// Buffer size sets the number of super frames that are buffered before propagating data.
        /// A super frame consists of 384 channels from the spike-band and 32 channels from the LFP band.
        /// The buffer size must be a multiple of 12.
        /// </remarks>
        [Description("Number of super-frames (384 channels from spike band and 32 channels from " +
            "LFP band) to buffer before propagating data. Must be a multiple of 12.")]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (int)(Math.Ceiling((double)value / NeuropixelsV1e.FramesPerRoundRobin) * NeuropixelsV1e.FramesPerRoundRobin);
        }

        /// <summary>
        /// Generates a sequence of <see cref="NeuropixelsV1eDataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV1eDataFrame"/> objects.</returns>
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
