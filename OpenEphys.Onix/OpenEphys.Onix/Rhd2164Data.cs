using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that produces a sequence of RHD2164 data frames.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureRhd2164"/>,
    /// in order to stream electrophysiology data.
    /// </remarks>
    public class Rhd2164Data : Source<Rhd2164DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhd2164.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the number of samples collected for each channel that are used to create a single <see cref="Rhd2164DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are buffered for each electrophysiology and auxiliary channel produced by the RHD2164 chip
        /// before data is propagated. For instance, if this value is set to 30, then 30 samples, along with corresponding clock values, will be collected
        /// from each of the electrophysiology and auxiliary channels and packed into each <see cref="Rhd2164DataFrame"/>. Because channels are sampled at
        /// 30 kHz, this is equivalent to 1 millisecond of data from each channel.
        /// </remarks>
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Generates a sequence of <see cref="Rhd2164DataFrame"/> objects, each of which are a buffered set of multichannel samples an RHD2164 device.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhd2164DataFrame"/> objects.</returns>
        public unsafe override IObservable<Rhd2164DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<Rhd2164DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var device = deviceInfo.GetDeviceContext(typeof(Rhd2164));
                    var amplifierBuffer = new short[Rhd2164.AmplifierChannelCount * bufferSize];
                    var auxBuffer = new short[Rhd2164.AuxChannelCount * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (Rhd2164Payload*)frame.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * Rhd2164.AmplifierChannelCount, Rhd2164.AmplifierChannelCount);
                            Marshal.Copy(new IntPtr(payload->AuxData), auxBuffer, sampleIndex * Rhd2164.AuxChannelCount, Rhd2164.AuxChannelCount);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhd2164.AmplifierChannelCount, Depth.U16);
                                var auxData = BufferHelper.CopyTranspose(auxBuffer, bufferSize, Rhd2164.AuxChannelCount, Depth.U16);
                                observer.OnNext(new Rhd2164DataFrame(clockBuffer, hubClockBuffer, amplifierData, auxData));
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
