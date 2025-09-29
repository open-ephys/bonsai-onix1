﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="NeuropixelsV1DataFrame">NeuropixelsV1DataFrames</see> from a
    /// NeuropixelsV1e headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureNeuropixelsV1e"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of NeuropixelsV1eDataFrame objects from a NeuropixelsV1e headstage.")]
    public class NeuropixelsV1eData : Source<NeuropixelsV1DataFrame>
    {
        int bufferSize = 36;

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV1e.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

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
        /// Gets or sets a boolean value that controls if the channels are ordered by depth.
        /// </summary>
        /// <remarks>
        /// If <see cref="OrderByDepth"/> is false, then channels are ordered from 0 to 383. 
        /// If <see cref="OrderByDepth"/> is true, then channels are ordered based on the depth
        /// of the electrodes
        /// </remarks>
        [Description("Determines if the channels are returned ordered by depth.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public bool OrderByDepth { get; set; } = false;

        /// <summary>
        /// Generates a sequence of <see cref="NeuropixelsV1DataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV1DataFrame"/> objects.</returns>
        public unsafe override IObservable<NeuropixelsV1DataFrame> Generate()
        {
            var spikeBufferSize = BufferSize;
            var lfpBufferSize = spikeBufferSize / NeuropixelsV1.FramesPerRoundRobin;
            var orderByDepth = OrderByDepth;

            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV1eDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(NeuropixelsV1e));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context.GetDeviceFrames(passthrough.Address);
                int[,] channelOrder = orderByDepth ? NeuropixelsV1eDataFrame.OrderChannelsByDepth(info.ProbeConfiguration.ChannelMap) : null;

                return Observable.Create<NeuropixelsV1DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var spikeBuffer = new ushort[NeuropixelsV1.ChannelCount, spikeBufferSize];
                    var lfpBuffer = new ushort[NeuropixelsV1.ChannelCount, lfpBufferSize];
                    var frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
                    var hubClockBuffer = new ulong[spikeBufferSize];
                    var clockBuffer = new ulong[spikeBufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV1ePayload*)frame.Data.ToPointer();
                            NeuropixelsV1eDataFrame.CopyAmplifierBuffer(payload->AmplifierData, frameCountBuffer, spikeBuffer, lfpBuffer, sampleIndex, info.ApGainCorrection, info.LfpGainCorrection, info.AdcThresholds, info.AdcOffsets, info.InvertPolarity, channelOrder);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= spikeBufferSize)
                            {
                                var spikeData = Mat.FromArray(spikeBuffer);
                                var lfpData = Mat.FromArray(lfpBuffer);
                                observer.OnNext(new NeuropixelsV1DataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, spikeData, lfpData));
                                frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
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
