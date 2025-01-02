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
    /// Produces a sequence of <see cref="UclaMiniscopeV4CameraDataFrame"/>s from the Python-480 image sensor on a
    /// UCLA Miniscope V4.
    /// </summary>
    public class UclaMiniscopeV4CameraData : Source<UclaMiniscopeV4CameraDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(UclaMiniscopeV4.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the data type used to represent pixel intensity values.
        /// </summary>
        /// <remarks>
        /// The UCLA Miniscope V4 uses a 10-bit image sensor. To capture images that use the full ADC
        /// resolution, this value can be set to <see cref="UclaMiniscopeV4ImageDepth.U10"/>. This comes at
        /// the cost of limited codec support and larger file sizes. If <see
        /// cref="UclaMiniscopeV4ImageDepth.U8"/> is selected, the two least significant bits of each pixel
        /// sample will be discarded, which greatly increases codec options and reduces file sizes. Further,
        /// we have noticed that Bonsai introduces flickering in the real-time video visualizer when using
        /// 10-bit data. These artifacts are not present in the data itself, only in the real-time visualizer.
        /// </remarks>
        [Description("The bit-depth used to represent pixel intensity values.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public UclaMiniscopeV4ImageDepth DataType { get; set; } = UclaMiniscopeV4ImageDepth.U8;

        /// <summary>
        /// Generates a sequence of <see cref="UclaMiniscopeV4CameraDataFrame"/>s at a rate determined by <see
        /// cref="ConfigureUclaMiniscopeV4Camera.FrameRate"/>.
        /// </summary>
        /// <returns>A sequence of <see cref="UclaMiniscopeV4CameraDataFrame"/>s</returns>
        public unsafe override IObservable<UclaMiniscopeV4CameraDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(UclaMiniscopeV4));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var scopeData = device.Context.GetDeviceFrames(passthrough.Address);
                var dataType = DataType;

                return Observable.Create<UclaMiniscopeV4CameraDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var imageBuffer = new short[UclaMiniscopeV4.SensorRows * UclaMiniscopeV4.SensorColumns];
                    var hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                    var clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                    var sampleRect = new Rect(0, 1, UclaMiniscopeV4.SensorColumns, UclaMiniscopeV4.SensorRows - 1);

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

                                var imageData = Mat.FromArray(imageBuffer, UclaMiniscopeV4.SensorRows, UclaMiniscopeV4.SensorColumns, Depth.U16, 1);
                                CV.ConvertScale(imageData.GetRow(0), imageData.GetRow(0), 1.0f, -32768f); // Get rid first row's mark bit

                                switch (dataType)
                                {
                                    case UclaMiniscopeV4ImageDepth.U8:
                                        {
                                            var eightBitImageData = new Mat(imageData.Size, Depth.U8, 1);
                                            CV.ConvertScale(imageData, eightBitImageData, 0.25);
                                            observer.OnNext(new UclaMiniscopeV4CameraDataFrame(clockBuffer, hubClockBuffer, eightBitImageData.GetImage()));
                                            break;
                                        }
                                    case UclaMiniscopeV4ImageDepth.U10:
                                        {
                                            observer.OnNext(new UclaMiniscopeV4CameraDataFrame(clockBuffer, hubClockBuffer, imageData.GetImage()));
                                            break;
                                        }
                                }

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

        /// <summary>
        /// Specifies the bit-depth used to represent pixel intensity values.
        /// </summary>
        public enum UclaMiniscopeV4ImageDepth
        {
            /// <summary>
            /// 8-bit pixel values encoded as bytes.
            /// </summary>
            U8,
            /// <summary>
            /// 10-bit pixel values encoded as unsigned 16-bit integers
            /// </summary>
            U10
        }
    }
}
