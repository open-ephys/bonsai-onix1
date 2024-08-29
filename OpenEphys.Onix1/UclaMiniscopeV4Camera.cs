using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    public class UclaMiniscopeV4Camera : Source<UclaMiniscopeV4Image>
    {
        [TypeConverter(typeof(UclaMiniscopeV4.NameConverter))]
        public string DeviceName { get; set; }

        public unsafe override IObservable<UclaMiniscopeV4Image> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV2eDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(NeuropixelsV2e));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var scopeData = device.Context.GetDeviceFrames(passthrough.Address);
        
                return Observable.Create<UclaMiniscopeV4Image>(observer =>
                {
                    var sampleIndex = 0;
                    var imageBuffer = new short[UclaMiniscopeV4.SensorRows * UclaMiniscopeV4.SensorColumns];
                    var hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                    var clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                    var awaitingFrameStart = true;

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (UclaMiniscopeV4ImagerPayload*)frame.Data.ToPointer();

                            // Wait for first row
                            if (awaitingFrameStart && (payload->ImageRow[0] & 0x8000) == 0)
                                return;

                            awaitingFrameStart = false;
                            Marshal.Copy(new IntPtr(payload->ImageRow), imageBuffer, sampleIndex * UclaMiniscopeV4.SensorColumns, UclaMiniscopeV4.SensorColumns);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= UclaMiniscopeV4.SensorRows)
                            {
                                var imageData = Mat.FromArray(imageBuffer, UclaMiniscopeV4.SensorRows, UclaMiniscopeV4.SensorColumns,  Depth.U16, 1);
                                CV.ConvertScale(imageData.GetRow(0), imageData.GetRow(0), 1.0f, -32768.0f); // Get rid first row's mark bit
                                observer.OnNext(new UclaMiniscopeV4Image(clockBuffer, hubClockBuffer, imageData.GetImage()));
                                hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                sampleIndex = 0;
                                awaitingFrameStart = true;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return scopeData.SubscribeSafe(frameObserver);
                });
            });
        }
    }
}
