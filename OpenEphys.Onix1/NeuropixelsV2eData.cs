using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="NeuropixelsV2eDataFrame"/> objects from a NeuropixelsV2e headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureNeuropixelsV2e"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of NeuropixelsV2eDataFrame objects from a NeuropixelsV2e headstage.")]
    public class NeuropixelsV2eData : Source<NeuropixelsV2eDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV2e.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of samples that are collected from each of the 384 ephys
        /// channels before data is propagated. For instance, if this value is set to 30, then 384x30 samples,
        /// along with 30 corresponding clock values, will be collected and packed into each <see
        /// cref="NeuropixelsV2eDataFrame"/>. Because channels are sampled at 30 kHz, this is equivalent to 1
        /// millisecond of data from each channel.
        /// </remarks>
        [Description("The number of samples collected from each channel that are used to create a single NeuropixelsV2eDataFrame.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Gets or sets the probe index.
        /// </summary>
        [Description("The index of the probe from which to collect sample data")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public NeuropixelsV2Probe ProbeIndex { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="NeuropixelsV2eDataFrame">NeuropixelsV2eDataFrames</see>.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV2eDataFrame">NeuropixelsV2eDataFrames</see>.</returns>
        public unsafe override IObservable<NeuropixelsV2eDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV2eDeviceInfo)deviceInfo;
                var (metadata, gainCorrection) = ProbeIndex switch
                {
                    NeuropixelsV2Probe.ProbeA => (info.ProbeMetadataA, info.GainCorrectionA),
                    NeuropixelsV2Probe.ProbeB => (info.ProbeMetadataB, info.GainCorrectionB),
                    _ => throw new InvalidEnumArgumentException($"Unexpected {nameof(ProbeIndex)} value: {ProbeIndex}")
                };

                if (metadata.ProbeSerialNumber == null)
                {
                    throw new InvalidOperationException($"{ProbeIndex} is not detected. Ensure that the flex connection is properly seated.");
                }
                else if (gainCorrection == null)
                {
                    throw new NullReferenceException($"Gain correction value is null for {ProbeIndex}.");
                }

                var device = info.GetDeviceContext(typeof(NeuropixelsV2e));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context
                    .GetDeviceFrames(passthrough.Address)
                    .Where(frame => NeuropixelsV2eDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);
                var invertPolarity = info.InvertPolarity;

                return Observable.Create<NeuropixelsV2eDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var amplifierBuffer = new ushort[NeuropixelsV2e.ChannelCount, bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV2Payload*)frame.Data.ToPointer();
                            NeuropixelsV2eDataFrame.CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, sampleIndex, gainCorrection.Value, invertPolarity);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = Mat.FromArray(amplifierBuffer);
                                observer.OnNext(new NeuropixelsV2eDataFrame(clockBuffer, hubClockBuffer, amplifierData));
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
    }
}
