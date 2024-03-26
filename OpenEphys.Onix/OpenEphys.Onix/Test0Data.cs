using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using Bonsai.Reactive;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Test0Data : Source<Test0DataFrame>
    {
        [TypeConverter(typeof(Test0.NameConverter))]
        public string DeviceName { get; set; }

        [Category(DeviceFactory.ConfigurationCategory)]
        [Range(1, 1000000)]
        [Description("The number of frames making up a single data block that is propagated in the observable sequence.")]
        public int BufferSize { get; set; } = 100;

        public unsafe override IObservable<Test0DataFrame> Generate()
        {
            var bufferSize = BufferSize; // TODO: Branch for bufferSize = 1?

            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                Observable.Create<Test0DataFrame>(observer =>
                {
                    // Find number of dummy words in the frame
                    var device = deviceInfo.GetDeviceContext(typeof(Test0));
                    var dummyWords = (int)device.ReadRegister(Test0.NUMTESTWORDS);

                    var sampleIndex = 0;
                    var dummyBuffer = new short[dummyWords * bufferSize];
                    var messageBuffer = new short[bufferSize];
                    var hubSyncCounterBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (Test0PayloadHeader*)frame.Data.ToPointer();
                            Marshal.Copy(new IntPtr(payload + 1), dummyBuffer, sampleIndex * dummyWords, dummyWords);
                            messageBuffer[sampleIndex] = payload->Message;
                            hubSyncCounterBuffer[sampleIndex] = payload->HubSyncCounter;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var dummy = BufferHelper.CopyBuffer(dummyBuffer, bufferSize, dummyWords, Depth.S16);
                                var message = BufferHelper.CopyBuffer(messageBuffer, bufferSize, 1, Depth.S16);
                                observer.OnNext(new Test0DataFrame(clockBuffer, hubSyncCounterBuffer, message, dummy));
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
