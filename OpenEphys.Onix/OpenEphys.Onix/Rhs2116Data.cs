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
    public class Rhs2116Data : Source<Rhs2116DataFrame>
    {
        [TypeConverter(typeof(Rhs2116.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<Rhs2116DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<Rhs2116DataFrame>(observer =>
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
                                hubClockBuffer[sampleIndex] = BitHelper.SwapEndian(payload->HubClock);
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize)
                                {
                                    var amplifierData = BufferHelper.CopyBuffer(amplifierBuffer, bufferSize, Rhs2116.AmplifierChannelCount, Depth.U16);
                                    var dcData = BufferHelper.CopyBuffer(dcBuffer, bufferSize, Rhs2116.AmplifierChannelCount, Depth.U16);
                                    observer.OnNext(new Rhs2116DataFrame(clockBuffer, hubClockBuffer, amplifierData, dcData));
                                    hubClockBuffer = new ulong[bufferSize];
                                    clockBuffer = new ulong[bufferSize];
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
