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
    public class Rhd2164Data : Source<Rhd2164DataFrame>
    {
        [TypeConverter(typeof(Rhd2164.NameConverter))]
        public string DeviceName { get; set; }

        public int BufferSize { get; set; } = 30;

        public unsafe override IObservable<Rhd2164DataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                    Observable.Create<Rhd2164DataFrame>(observer =>
                    {
                        var sampleIndex = 0;
                        var device = deviceInfo.GetDeviceContext(typeof(Rhd2164));
                        var amplifierBuffer = new short[Rhd2164.AmplifierChannelCount * bufferSize];
                        var auxBuffer = new short[Rhd2164.AuxChannelCount * bufferSize];
                        var hubSyncCounterBuffer = new ulong[bufferSize];
                        var clockBuffer = new ulong[bufferSize];

                        var frameObserver = Observer.Create<oni.Frame>(
                            frame =>
                            {
                                var payload = (Rhd2164Payload*)frame.Data.ToPointer();
                                Marshal.Copy(new IntPtr(payload->AmplifierData), amplifierBuffer, sampleIndex * Rhd2164.AmplifierChannelCount, Rhd2164.AmplifierChannelCount);
                                Marshal.Copy(new IntPtr(payload->AuxData), auxBuffer, sampleIndex * Rhd2164.AuxChannelCount, Rhd2164.AuxChannelCount);
                                hubSyncCounterBuffer[sampleIndex] = payload->HubSyncCounter;
                                clockBuffer[sampleIndex] = frame.Clock;
                                if (++sampleIndex >= bufferSize)
                                {
                                    var amplifierData = BufferHelper.CopyBuffer(amplifierBuffer, bufferSize, Rhd2164.AmplifierChannelCount, Depth.U16);
                                    var auxData = BufferHelper.CopyBuffer(auxBuffer, bufferSize, Rhd2164.AuxChannelCount, Depth.U16);
                                    observer.OnNext(new Rhd2164DataFrame(clockBuffer, hubSyncCounterBuffer, amplifierData, auxData));
                                    hubSyncCounterBuffer = new ulong[bufferSize];
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
