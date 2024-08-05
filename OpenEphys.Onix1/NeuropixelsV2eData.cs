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
    [Description("Produces a sequence of NeuropixelsV2eDataFrame objects from a NeuropixelsV2e headstage.")]
    public class NeuropixelsV2eData : Source<NeuropixelsV2eDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV2e.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// This property determines the number of super-frames that are buffered before data is propagated. A super-frame consists of 384 
        /// channels from the spike-band and 32 channels from the LFP band. If this value is set to 30, then 30 super-frames, along with 
        /// corresponding clock values, will be collected and packed into each <see cref="NeuropixelsV2eDataFrame"/>. Because channels are 
        /// sampled at 30 kHz, this is equivalent to 1 millisecond of data from each channel.
        /// </remarks>
        [Description("The number of samples collected for each channel that are used to create a single NeuropixelsV2eDataFrame.")]
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Gets or sets the probe index.
        /// </summary>
        [Description("The index of the probe from which to collect sample data")]
        public NeuropixelsV2Probe ProbeIndex { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="NeuropixelsV2eDataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV2eDataFrame"/> objects.</returns>
        public unsafe override IObservable<NeuropixelsV2eDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV2eDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(NeuropixelsV2e));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context
                    .GetDeviceFrames(passthrough.Address)
                    .Where(frame => NeuropixelsV2eDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);

                var gainCorrection = ProbeIndex switch
                {
                    NeuropixelsV2Probe.ProbeA => (double)info.GainCorrectionA,
                    NeuropixelsV2Probe.ProbeB => (double)info.GainCorrectionB,
                    _ => throw new ArgumentOutOfRangeException(nameof(ProbeIndex), $"Unexpected {nameof(ProbeIndex)} value: {ProbeIndex}"),
                };

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
                            NeuropixelsV2eDataFrame.CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, sampleIndex, gainCorrection);
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
