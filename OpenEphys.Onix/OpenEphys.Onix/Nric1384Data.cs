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
    public class Nric1384Data : Source<Nric1384DataFrame>
    {
        [TypeConverter(typeof(Nric1384.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<Nric1384DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<Nric1384DataFrame>(observer =>
                    {
                        var sampleIndex = 0;
                        var device = deviceInfo.GetDeviceContext(typeof(Nric1384));
                        var lfpBuffer = new short[NeuropixelsV1.ChannelCount * bufferSize];
                        var apBuffer = new short[NeuropixelsV1.ChannelCount * bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                        var frameCountBuffer = new uint[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                        var hubClockBuffer = new ulong[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                        var clockBuffer = new ulong[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (Nric1384Payload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->LfpData), lfpBuffer, sampleIndex * NeuropixelsV1.AdcCount, NeuropixelsV1.AdcCount);
                                Marshal.Copy(new IntPtr(payload->ApData), apBuffer, sampleIndex * NeuropixelsV1.ChannelCount, NeuropixelsV1.ChannelCount);
                                frameCountBuffer[sampleIndex] = payload->FrameCount;
                                hubClockBuffer[sampleIndex] = payload->HubClock;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize * NeuropixelsV1.SuperframesPerUltraframe)
                                {
                                    var lfpData = BufferHelper.CopyBuffer(lfpBuffer, bufferSize, NeuropixelsV1.ChannelCount, Depth.U16);
                                    var apData = BufferHelper.CopyBuffer(apBuffer, bufferSize * NeuropixelsV1.SuperframesPerUltraframe, NeuropixelsV1.ChannelCount, Depth.U16);
                                    observer.OnNext(new Nric1384DataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, apData, lfpData));
                                    frameCountBuffer = new uint[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                                    hubClockBuffer = new ulong[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                                    clockBuffer = new ulong[bufferSize * NeuropixelsV1.SuperframesPerUltraframe];
                                    sampleIndex = 0;
                                }
                            },
                            observer.OnError,
                            observer.OnCompleted);
                        return deviceInfo.Context.FrameReceived
                            .Where(frame => frame.DeviceAddress == device.Address)
                            .SubscribeSafe(frameObserver);
                    })));
        }
    }
}
