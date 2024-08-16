using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using oni;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    public class Rhs2116DualData : Source<Rhs2116DataFrame>
    {
        [TypeConverter(typeof(Rhs2116Dual.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<Rhs2116DataFrame> Generate()
        {

            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<Rhs2116DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var dualInfo = (Rhs2116DualDeviceInfo)deviceInfo;

                    var rhs2116A = dualInfo.Rhs2116A;
                    var rhs2116B = dualInfo.Rhs2116B;

                    var amplifierBuffer = new short[Rhs2116Dual.TotalChannels * bufferSize];
                    var dcBuffer = new short[Rhs2116Dual.TotalChannels * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<(Frame, Frame)>(
                        frames =>
                        {
                            var payloadA = (Rhs2116Payload*)frames.Item1.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payloadA->AmplifierData), amplifierBuffer, sampleIndex * Rhs2116Dual.TotalChannels, Rhs2116Dual.ChannelsPerChip);
                            Marshal.Copy(new IntPtr(payloadA->DCData), dcBuffer, sampleIndex * Rhs2116Dual.TotalChannels, Rhs2116Dual.ChannelsPerChip);
                            hubClockBuffer[sampleIndex] = payloadA->HubClock;
                            clockBuffer[sampleIndex] = frames.Item1.Clock;

                            var payloadB = (Rhs2116Payload*)frames.Item2.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payloadB->AmplifierData), amplifierBuffer, sampleIndex * Rhs2116Dual.TotalChannels + Rhs2116Dual.ChannelsPerChip, Rhs2116Dual.ChannelsPerChip);
                            Marshal.Copy(new IntPtr(payloadB->DCData), dcBuffer, sampleIndex * Rhs2116Dual.TotalChannels + Rhs2116Dual.ChannelsPerChip, Rhs2116Dual.ChannelsPerChip);

                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = BufferHelper.CopyTranspose(amplifierBuffer, bufferSize, Rhs2116Dual.TotalChannels, Depth.U16);
                                var dcData = BufferHelper.CopyTranspose(dcBuffer, bufferSize, Rhs2116Dual.TotalChannels, Depth.U16);
                                observer.OnNext(new Rhs2116DataFrame(clockBuffer, hubClockBuffer, amplifierData, dcData));
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return dualInfo.Context.GetDeviceFrames(rhs2116A.Address)
                        //.Select(f => (f, f))
                        .Zip(dualInfo.Context.GetDeviceFrames(rhs2116B.Address), (f1, f2) => (f1, f2))
                        .SubscribeSafe(frameObserver);
                }));
        }
    }
}

