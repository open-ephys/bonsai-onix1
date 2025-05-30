﻿using System;
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
    /// Produces a sequence of <see cref="Rhs2116PairDataFrame"/> objects from a synchronized pair of Intan
    /// Rhs2116 bidirectional bioacquisition chips.
    /// chips.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureRhs2116Pair"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    public class Rhs2116PairData : Source<Rhs2116PairDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhs2116Pair.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are collected from each of the 32
        /// electrophysiology channels before data is propagated. For instance, if this value is set to 30,
        /// then 32x30 samples, along with 30 corresponding clock values, will be collected and packed into
        /// each <see cref="Rhs2116PairDataFrame"/>. Because channels are sampled at ~30 kHz, this is equivalent
        /// to ~1 millisecond of data from each channel.
        /// </remarks>
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Generates a sequence of <see cref="Rhs2116PairDataFrame"/>s.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhs2116PairDataFrame"/>s.</returns>
        public unsafe override IObservable<Rhs2116PairDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<Rhs2116PairDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var dualInfo = (Rhs2116PairDeviceInfo)deviceInfo;
                    var rhs2116A = dualInfo.Rhs2116A;
                    var rhs2116B = dualInfo.Rhs2116B;
                    var amplifierBuffer = new short[Rhs2116Pair.TotalChannels * bufferSize];
                    var dcBuffer = new short[Rhs2116Pair.TotalChannels * bufferSize];
                    var recovery = new uint[bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            if (frame.DeviceAddress == rhs2116A.Address)
                            {
                                var offset = sampleIndex * Rhs2116Pair.TotalChannels;
                                var payload = (Rhs2116Payload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, offset, Rhs2116.AmplifierChannelCount);
                                Marshal.Copy(new IntPtr(payload->DCData), dcBuffer, offset, Rhs2116.AmplifierChannelCount);
                                recovery[sampleIndex] |= payload->Recovery;

                                if (++sampleIndex >= bufferSize)
                                {
                                    var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhs2116Pair.TotalChannels, Depth.U16);
                                    var dcData = BufferHelper.CopyTranspose(dcBuffer, bufferSize, Rhs2116Pair.TotalChannels, Depth.U16);
                                    observer.OnNext(new Rhs2116PairDataFrame(clockBuffer, hubClockBuffer, amplifierData, dcData, recovery));
                                    hubClockBuffer = new ulong[bufferSize];
                                    recovery = new uint[bufferSize];
                                    clockBuffer = new ulong[bufferSize];
                                    sampleIndex = 0;
                                }
                                
                            } else // Chip B
                            {
                                var offset = sampleIndex * Rhs2116Pair.TotalChannels + Rhs2116Pair.ChannelsPerChip;
                                var payload = (Rhs2116Payload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, offset, Rhs2116.AmplifierChannelCount);
                                Marshal.Copy(new IntPtr(payload->DCData), dcBuffer, offset, Rhs2116.AmplifierChannelCount);
                                hubClockBuffer[sampleIndex] = payload->HubClock;
                                recovery[sampleIndex] |= (uint)payload->Recovery << 16;
                                clockBuffer[sampleIndex] = frame.Clock;
                            }

                        },
                        observer.OnError,
                        observer.OnCompleted);

                    // NB: using zip corrupts the memory stored at (Rhs2116Payload*) for some reason
                    // NB: chip B always starts a pair of samples on firmware 0.3
                    return deviceInfo.Context.GetDeviceFrames(rhs2116A.Address)
                        .Merge(deviceInfo.Context.GetDeviceFrames(rhs2116B.Address))
                        .SkipWhile(f => f.DeviceAddress != rhs2116B.Address)
                        .SubscribeSafe(frameObserver);
                }));
        }
    }
}
