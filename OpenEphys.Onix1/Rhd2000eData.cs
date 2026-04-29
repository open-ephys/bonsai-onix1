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
    /// Produces a sequence of <see cref="Rhd2000DataFrame">Rhd2000DataFrames</see> from a Rhd2000
    /// bioamplifier chip.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureRhd2000PsbDecoder"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Rhd2000DataFrame objects from a Rhd2000 bioamplifier chip.")]
    public class Rhd2000eData : Source<Rhd2000DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhd2000PsbDecoder.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are collected from each of the M Rhd2000 ephys
        /// channels before data is propagated. For instance, if this value is set to 30, then Mx30 samples,
        /// along with 30 corresponding clock values, will be collected and packed into each <see
        /// cref="Rhd2000DataFrame"/>.
        /// </remarks>
        [Description("The number of samples collected from each channel that are used to create a single Rhd2000DataFrame.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Generates a sequence of <see cref="Rhd2000DataFrame">Rhd2000DataFrames</see>.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhd2000DataFrame">Rhd2000DataFrames</see>.</returns>
        public unsafe override IObservable<Rhd2000DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (Rhd2000PsbDecoderDeviceInfo)deviceInfo;
                var probeIndex = info.StreamIndex;
                var ephysChannelCount = info.EphysChannelCount;

                var device = info.GetDeviceContext(typeof(Rhd2000PsbDecoder));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context
                    .GetDeviceFrames(passthrough.Address)
                    .Where(frame => GetProbeIndex(frame) == probeIndex);

                return Observable.Create<Rhd2000DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var amplifierBuffer = new ushort[ephysChannelCount, bufferSize];
                    var auxBuffer = new ushort[Rhd2000.AuxChannelCount, bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    ushort lastAux2 = 0;
                    ushort lastAux3 = 0;

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (Rhd2216ePayload*)frame.Data.ToPointer();
                            CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, auxBuffer, sampleIndex, ref lastAux2, ref lastAux3);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = Mat.FromArray(amplifierBuffer);
                                var auxData = Mat.FromArray(auxBuffer);
                                observer.OnNext(new Rhd2000DataFrame(clockBuffer, hubClockBuffer, amplifierData, auxData));
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

        static unsafe ushort GetProbeIndex(oni.Frame frame)
        {
            var data = (Rhd2216ePayload*)frame.Data.ToPointer();
            return data->StreamIndex;
        }

        static unsafe void CopyAmplifierBuffer(ushort* amplifierData, ushort[,] amplifierBuffer, ushort[,] auxBuffer, int index, ref ushort lastAux2, ref ushort lastAux3)
        {

            static ushort ReadWord(ushort* p) => (ushort)(p[1] << 8 | p[0]);

            // loop over 16 "frames" within each "super-frame"
            int channel = 0;
            for (int i = 0; i < amplifierBuffer.GetLength(0) / Rhd2000PsbDecoder.EphysChannelsPerFrame; i++)
            {
                // the period of ADC data within data array is 36 words
                var adcDataOffset = Rhd2000PsbDecoder.TrashWords + i * Rhd2000PsbDecoder.WordsPerFrame;

                for (int k = 0; k < 32; k += Rhd2000PsbDecoder.SampleWords)
                {
                    amplifierBuffer[channel++, index] = ReadWord(amplifierData + adcDataOffset + k);
                }
            }

            ushort* p = amplifierData + Rhd2000PsbDecoder.TrashWords + Rhd2000PsbDecoder.FramesForEphysData * Rhd2000PsbDecoder.WordsPerFrame;
            auxBuffer[0, index] = ReadWord(p);

            p += Rhd2000PsbDecoder.SampleWords;

            var aux23 = ReadWord(p);
            auxBuffer[1, index] = p[2] == 0 ? aux23 : lastAux2;
            auxBuffer[2, index] = p[2] == 1 ? aux23 : lastAux3;
            lastAux2 = auxBuffer[1, index];
            lastAux3 = auxBuffer[2, index];
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhd2216ePayload
    {
        public ulong HubClock;
        public ushort StreamIndex;
        public fixed ushort AmplifierData[Rhd2000PsbDecoder.FramesPerSuperFrame * Rhd2000PsbDecoder.WordsPerFrame];
    }
}
