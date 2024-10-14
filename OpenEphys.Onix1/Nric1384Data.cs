using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Nric1384DataFrame"/> objects from a Nric1384 bioacquisition device.
    /// </summary>
    public class Nric1384Data : Source<Nric1384DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        [TypeConverter(typeof(Nric1384.NameConverter))]
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
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (int)(Math.Ceiling((double)value / NeuropixelsV1.FramesPerRoundRobin) * NeuropixelsV1.FramesPerRoundRobin);
        }

        /// <summary>
        /// Generates a sequence of <see cref="Nric1384DataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="Nric1384DataFrame"/> objects.</returns>
        public unsafe override IObservable<Nric1384DataFrame> Generate()
        {
            var spikeBufferSize = BufferSize;
            var lfpBufferSize = spikeBufferSize / NeuropixelsV1.FramesPerRoundRobin;

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Nric1384));

                return Observable.Create<Nric1384DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var spikeBuffer = new short[NeuropixelsV1.ChannelCount * spikeBufferSize];
                    var lfpBuffer = new short[NeuropixelsV1.ChannelCount * lfpBufferSize];
                    var frameCountBuffer = new int[spikeBufferSize];
                    var hubClockBuffer = new ulong[spikeBufferSize];
                    var clockBuffer = new ulong[spikeBufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(frame =>
                    {
                        var payload = (Nric1384Payload*)frame.Data.ToPointer();
                        Marshal.Copy(new IntPtr(payload->LfpData), lfpBuffer, sampleIndex * NeuropixelsV1.AdcCount, NeuropixelsV1.AdcCount);
                        Marshal.Copy(new IntPtr(payload->ApData), spikeBuffer, sampleIndex * NeuropixelsV1.ChannelCount, NeuropixelsV1.ChannelCount);
                        frameCountBuffer[sampleIndex] = payload->FrameCount;
                        hubClockBuffer[sampleIndex] = payload->HubClock;
                        clockBuffer[sampleIndex] = frame.Clock;
                        if (++sampleIndex >= spikeBufferSize)
                        {
                            var lfpData = BufferHelper.CopyTranspose(lfpBuffer, lfpBufferSize, NeuropixelsV1.ChannelCount, Depth.U16);
                            var apData = BufferHelper.CopyTranspose(spikeBuffer, spikeBufferSize, NeuropixelsV1.ChannelCount, Depth.U16);
                            observer.OnNext(new Nric1384DataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, apData, lfpData));
                            frameCountBuffer = new int[spikeBufferSize];
                            hubClockBuffer = new ulong[spikeBufferSize];
                            clockBuffer = new ulong[spikeBufferSize];
                            sampleIndex = 0;
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);

                    return device.Context.GetDeviceFrames(device.Address).SubscribeSafe(frameObserver);
                });
            });
        }
    }
}
