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
                        var lfpBuffer = new short[Nric1384.ChannelCount * bufferSize];
                        var apBuffer = new short[Nric1384.ChannelCount * bufferSize * Nric1384.SuperframesPerUltraframe];
                        var frameCountBuffer = new uint[bufferSize * Nric1384.SuperframesPerUltraframe];
                        var hubClockBuffer = new ulong[bufferSize * Nric1384.SuperframesPerUltraframe];
                        var clockBuffer = new ulong[bufferSize * Nric1384.SuperframesPerUltraframe];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (Nric1384Payload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->LfpData), lfpBuffer, sampleIndex * Nric1384.AdcCount, Nric1384.AdcCount);
                                Marshal.Copy(new IntPtr(payload->ApData), apBuffer, sampleIndex * Nric1384.ChannelCount, Nric1384.ChannelCount);
                                frameCountBuffer[sampleIndex] = payload->FrameCount;
                                hubClockBuffer[sampleIndex] = payload->HubClock;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize * Nric1384.SuperframesPerUltraframe)
                                {
                                    var lfpData = BufferHelper.CopyBuffer(lfpBuffer, bufferSize, Nric1384.ChannelCount, Depth.U16);
                                    var apData = BufferHelper.CopyBuffer(apBuffer, bufferSize * Nric1384.SuperframesPerUltraframe, Nric1384.ChannelCount, Depth.U16);
                                    observer.OnNext(new Nric1384DataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, apData, lfpData));
                                    frameCountBuffer = new uint[bufferSize * Nric1384.SuperframesPerUltraframe];
                                    hubClockBuffer = new ulong[bufferSize * Nric1384.SuperframesPerUltraframe];
                                    clockBuffer = new ulong[bufferSize * Nric1384.SuperframesPerUltraframe];
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
