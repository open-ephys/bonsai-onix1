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
    /// Produces a sequence of <see cref="Rhd2164DataFrame"/> objects with data from an Intan
    /// Rhd2164 bioacquisition chip.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureRhd2164"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Rhd2164DataFrame objects with data from an Intan Rhd2164 bioacquisition chip.")]
    public class Rhd2164Data : Source<Rhd2164DataFrame>
    {
        private readonly static int[] EphysChannelMap = new[] {0 ,  2,  4,  6,  8, 10, 12, 14,
                                                               16, 18, 20, 22, 24, 26, 28, 30,
                                                               32, 34, 36, 38, 40, 42, 44, 46,
                                                               48, 50, 52, 54, 56, 58, 60, 62,
                                                               1 , 3 , 5 , 7 , 9 , 11, 13, 15,
                                                               17, 19, 21, 23, 25, 27, 29, 31,
                                                               33, 35, 37, 39, 41, 43, 45, 47,
                                                               49, 51, 53, 55, 57, 59, 61, 63};

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhd2164.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the number of samples collected for each channel that are used to create a single <see cref="Rhd2164DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are buffered for each electrophysiology and auxiliary channel produced by the Rhd2164 chip
        /// before data is propagated. For instance, if this value is set to 30, then 30 samples, along with corresponding clock values, will be collected
        /// from each of the electrophysiology and auxiliary channels and packed into each <see cref="Rhd2164DataFrame"/>. Because channels are sampled at
        /// 30 kHz, this is equivalent to 1 millisecond of data from each channel.
        /// </remarks>
        [Description("The number of samples collected for each channel that are used to create a single Rhd2164DataFrame.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Generates a sequence of <see cref="Rhd2164DataFrame"/> objects, each of which are a buffered set of multichannel samples an Rhd2164 device.
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
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhd2164.AmplifierChannelCount, Depth.U16, EphysChannelMap);
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
