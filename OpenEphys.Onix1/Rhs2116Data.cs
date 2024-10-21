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
    /// Produces a sequence of <see cref="Rhs2116DataFrame"/> objects from a NeuropixelsV2e headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureRhs2116"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    public class Rhs2116Data : Source<Rhs2116DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhs2116.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are collected from each of the 16 ephys
        /// channels before data is propagated. For instance, if this value is set to 30, then 16x30 samples,
        /// along with 30 corresponding clock values, will be collected and packed into each <see
        /// cref="Rhs2116DataFrame"/>. Because channels are sampled at 30 kHz, this is equivalent to 1
        /// millisecond of data from each channel.
        /// </remarks>
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Generates a sequence of <see cref="Rhs2116DataFrame"/>s.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhs2116DataFrame"/>s.</returns>
        public unsafe override IObservable<Rhs2116DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<Rhs2116DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var device = deviceInfo.GetDeviceContext(typeof(Rhs2116));
                    var amplifierBuffer = new short[Rhs2116.AmplifierChannelCount * bufferSize];
                    var dcBuffer = new short[Rhs2116.AmplifierChannelCount * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (Rhs2116Payload*)frame.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * Rhs2116.AmplifierChannelCount, Rhs2116.AmplifierChannelCount);
                            Marshal.Copy(new IntPtr(payload->DCData), dcBuffer, sampleIndex * Rhs2116.AmplifierChannelCount, Rhs2116.AmplifierChannelCount);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhs2116.AmplifierChannelCount, Depth.U16);
                                var dcData = BufferHelper.CopyTranspose(dcBuffer, bufferSize, Rhs2116.AmplifierChannelCount, Depth.U16);
                                observer.OnNext(new Rhs2116DataFrame(clockBuffer, hubClockBuffer, amplifierData, dcData));
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
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
