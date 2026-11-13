using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Rhd2000CountedDataFrame">Rhd2000CountedDataFrames</see> from the Rhd2000
    /// bioamplifier chips connected to an Intan SPI adapter.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureIntanSPI"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Rhd2000CountedDataFrame objects from the Rhd2000 bioamplifier chips connected to an Intan SPI adapter.")]
    public class IntanSPIData : Source<Rhd2000CountedDataFrame>
    {
        int bufferSize = 32;

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(IntanSPI.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are collected from each of the M Rhd2000 ephys
        /// channels before data is propagated. For instance, if this value is set to 32, then Mx32 samples,
        /// along with 32 corresponding clock values and sample indexes, will be collected and packed into each
        /// <see cref="Rhd2000CountedDataFrame"/>. Auxiliary channels are sampled at a quarter of the ephys rate,
        /// so the buffer size must be a multiple of 4.
        /// </remarks>
        [Description("The number of samples collected from each channel that are used to create a single Rhd2000CountedDataFrame. Must be a multiple of 4.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (value + IntanSPI.AuxCycleLength - 1) / IntanSPI.AuxCycleLength * IntanSPI.AuxCycleLength;
        }

        /// <summary>
        /// Generates a sequence of <see cref="Rhd2000CountedDataFrame">Rhd2000CountedDataFrames</see>.
        /// </summary>
        /// <returns>A sequence of <see cref="Rhd2000CountedDataFrame">Rhd2000CountedDataFrames</see>.</returns>
        public unsafe override IObservable<Rhd2000CountedDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            var auxBufferSize = bufferSize / IntanSPI.AuxCycleLength;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (IntanSPIDeviceInfo)deviceInfo;
                var (channelOffsets, auxOffsets) = GetOffsets(info.RhdMiso1, info.RhdMiso2);

                var device = info.GetDeviceContext(typeof(IntanSPI));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var deviceData = device.Context.GetDeviceFrames(passthrough.Address);

                return Observable.Create<Rhd2000CountedDataFrame>(observer =>
                {
                    var bufferIndex = 0;
                    var auxIndex = -1;
                    var auxCycle = -1;
                    var amplifierBuffer = new ushort[channelOffsets.Length, bufferSize];
                    var auxBuffer = new ushort[auxOffsets.Length * Rhd2000.AuxChannelCount, auxBufferSize];
                    var auxCarryBuffer = new ushort[auxBuffer.GetLength(0)];
                    var sampleIndexBuffer = new ushort[bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (IntanSPIPayload*)frame.Data.ToPointer();
                            var sampleIndex = payload->AmplifierData[IntanSPI.SampleIndexOffset];

                            // Each aux cycle of 4 samples fills one column. A cycle that does not fit in this
                            // buffer is carried over to the first column of the next one.
                            var cycle = sampleIndex / IntanSPI.AuxCycleLength;
                            if (cycle != auxCycle)
                            {
                                auxCycle = cycle;
                                auxIndex++;
                            }

                            CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, channelOffsets, bufferIndex);
                            CopyAuxBuffer(payload->AmplifierData, auxBuffer, auxCarryBuffer, auxOffsets, sampleIndex % IntanSPI.AuxCycleLength, auxIndex);
                            sampleIndexBuffer[bufferIndex] = sampleIndex;
                            hubClockBuffer[bufferIndex] = payload->HubClock;
                            clockBuffer[bufferIndex] = frame.Clock;
                            if (++bufferIndex >= bufferSize)
                            {
                                var amplifierData = Mat.FromArray(amplifierBuffer);
                                var auxData = Mat.FromArray(auxBuffer);
                                observer.OnNext(new Rhd2000CountedDataFrame(clockBuffer, hubClockBuffer, amplifierData, auxData, sampleIndexBuffer));


                                // NB : We clear the buffers because if there were any dropped sample mid-cycle
                                // the aux sample corresponding to the skipped sample will be the one
                                // pertaining to the previous buffer. It is better for the aux sample
                                // to have a zero value than a carry over from the previous buffer.
                                Array.Clear(auxBuffer, 0, auxBuffer.Length);
                                for (int i = 0; i < auxCarryBuffer.Length; i++) auxBuffer[i, 0] = auxCarryBuffer[i];
                                Array.Clear(auxCarryBuffer, 0, auxCarryBuffer.Length);
                                auxIndex -= auxBufferSize;

                                sampleIndexBuffer = new ushort[bufferSize];
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                bufferIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    return deviceData.SubscribeSafe(frameObserver);
                });
            });
        }

        // Each channel is interleaved as A1, A2, B1, B2, where the letter is the stream and the number is the MISO
        // line. Output channels are ordered as line 1 stream A, line 1 stream B, line 2 stream A, line 2 stream B.
        static (int[] ChannelOffsets, int[] AuxOffsets) GetOffsets(Rhd2000ChipId rhdMiso1, Rhd2000ChipId rhdMiso2)
        {
            var channelOffsets = new List<int>();
            var auxOffsets = new List<int>();
            var chips = new[] { rhdMiso1, rhdMiso2 };
            for (int line = 0; line < chips.Length; line++)
            {
                var (channelCount, streamCount) = chips[line] switch
                {
                    Rhd2000ChipId.Rhd2216 => (16, 1),
                    Rhd2000ChipId.Rhd2132 => (IntanSPI.AmplifierChannelsPerStream, 1),
                    Rhd2000ChipId.Rhd2164 => (IntanSPI.AmplifierChannelsPerStream, 2),
                    _ => (0, 0)
                };

                if (streamCount == 0) continue;
                auxOffsets.Add(IntanSPI.AuxOffset + line);
                for (int stream = 0; stream < streamCount; stream++)
                {
                    for (int channel = 0; channel < channelCount; channel++)
                    {
                        channelOffsets.Add(channel * IntanSPI.WordsPerChannel + stream * IntanSPI.NumMisoLines + line);
                    }
                }
            }

            if (auxOffsets.Count == 0)
            {
                throw new InvalidOperationException(
                    "No supported Rhd2000 chip is connected.");
            }

            return (channelOffsets.ToArray(), auxOffsets.ToArray());
        }

        static unsafe void CopyAmplifierBuffer(ushort* amplifierData, ushort[,] amplifierBuffer, int[] channelOffsets, int index)
        {
            for (int i = 0; i < channelOffsets.Length; i++)
            {
                amplifierBuffer[i, index] = amplifierData[channelOffsets[i]];
            }
        }

        static unsafe void CopyAuxBuffer(ushort* amplifierData, ushort[,] auxBuffer, ushort[] auxCarryBuffer, int[] auxOffsets, int auxChannel, int index)
        {
            // NB: aux channel 3 contains a voltage measurement that is currently not used
            if (auxChannel >= Rhd2000.AuxChannelCount) return;

            for (int i = 0; i < auxOffsets.Length; i++)
            {
                var row = i * Rhd2000.AuxChannelCount + auxChannel;
                if (index < auxBuffer.GetLength(1)) auxBuffer[row, index] = amplifierData[auxOffsets[i]];
                else auxCarryBuffer[row] = amplifierData[auxOffsets[i]];
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct IntanSPIPayload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[IntanSPI.FrameSizeWords];
    }
}
