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
    /// <summary>
    /// Produces a sequence of <see cref="UclaMiniscopeV4CameraFrame"/>s from the Python-480 image sensor on a
    /// UCLA Miniscope V4.
    /// </summary>
    public class UclaMiniscopeV4CameraData : Source<UclaMiniscopeV4CameraFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(UclaMiniscopeV4.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="UclaMiniscopeV4CameraFrame"/>s at a rate determined by <see
        /// cref="ConfigureUclaMiniscopeV4Camera.FrameRate"/>.
        /// </summary>
        /// <returns>A sequence of <see cref="UclaMiniscopeV4CameraFrame"/>s</returns>
        public unsafe override IObservable<UclaMiniscopeV4CameraFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(UclaMiniscopeV4));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var scopeData = device.Context.GetDeviceFrames(passthrough.Address);
        
                return Observable.Create<UclaMiniscopeV4CameraFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var imageBuffer = new short[UclaMiniscopeV4.SensorRows * UclaMiniscopeV4.SensorColumns];
                    var hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                    var clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (UclaMiniscopeV4ImagerPayload*)frame.Data.ToPointer();

                            // Wait for first row
                            if (sampleIndex == 0 && (payload->ImageRow[0] & 0x8000) == 0)
                               return;

                            Marshal.Copy(new IntPtr(payload->ImageRow), imageBuffer, sampleIndex * UclaMiniscopeV4.SensorColumns, UclaMiniscopeV4.SensorColumns);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= UclaMiniscopeV4.SensorRows)
                            {
                                var imageData = Mat.FromArray(imageBuffer, UclaMiniscopeV4.SensorRows, UclaMiniscopeV4.SensorColumns,  Depth.U16, 1);
                                CV.ConvertScale(imageData.GetRow(0), imageData.GetRow(0), 1.0f, -32768.0f); // Get rid first row's mark bit
                                observer.OnNext(new UclaMiniscopeV4CameraFrame(clockBuffer, hubClockBuffer, imageData.GetImage()));
                                hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                sampleIndex = 0;
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
