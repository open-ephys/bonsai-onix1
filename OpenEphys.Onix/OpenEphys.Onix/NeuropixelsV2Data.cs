using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2Data : Source<NeuropixelsV2DataFrame>
    {
        [TypeConverter(typeof(NeuropixelsV2.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<NeuropixelsV2DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV2));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .GroupBy(frame => NeuropixelsV2DataFrame.GetProbeIndex(frame))
                        .SelectMany(probe => Observable.Create<NeuropixelsV2DataFrame>(observer =>
                        {
                            var sampleIndex = 0;
                            var amplifierBuffer = new ushort[NeuropixelsV2.ChannelCount, bufferSize];
                            var hubClockBuffer = new ulong[bufferSize];
                            var clockBuffer = new ulong[bufferSize];

                            var frameObserver = Observer.Create<oni.Frame>(
                                frame =>
                                {
                                    var payload = (NeuropixelsV2Payload*)frame.Data.ToPointer();
                                    NeuropixelsV2DataFrame.CopyAmplifierBuffer(payload->AmplifierData, amplifierBuffer, sampleIndex);
                                    hubClockBuffer[sampleIndex] = BitHelper.SwapEndian(payload->HubClock);
                                    clockBuffer[sampleIndex] = frame.Clock;
                                    if (++sampleIndex >= bufferSize)
                                    {
                                        var amplifierData = Mat.FromArray(amplifierBuffer);
                                        observer.OnNext(new NeuropixelsV2DataFrame(clockBuffer, hubClockBuffer, probe.Key, amplifierData));
                                        hubClockBuffer = new ulong[bufferSize];
                                        clockBuffer = new ulong[bufferSize];
                                        sampleIndex = 0;
                                    }
                                },
                                observer.OnError,
                                observer.OnCompleted);
                            return probe.SubscribeSafe(frameObserver);
                        }));
                }));
        }
    }
}
