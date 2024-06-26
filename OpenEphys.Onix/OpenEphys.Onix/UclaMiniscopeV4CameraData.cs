using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class UclaMiniscopeV4CameraData : Source<UclaMiniscopeV4DataFrame>
    {
        readonly BehaviorSubject<double> ledBrightness = new(0);
        readonly BehaviorSubject<UclaMiniscopeV4SensorGain> sensorGain = new(UclaMiniscopeV4SensorGain.Low);
        readonly BehaviorSubject<double> liquidLensVoltage = new(0);

        [TypeConverter(typeof(UclaMiniscopeV4.NameConverter))]
        public string DeviceName { get; set; }

        [Description("Excitation LED brightness (0-100%).")]
        [Range(0, 100)]
        [Precision(1, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double LedBrightness
        {
            get => ledBrightness.Value;
            set => ledBrightness.OnNext(value);
        }

        [Description("Camera sensor analog gain.")]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public UclaMiniscopeV4SensorGain SensorGain
        {
            get => sensorGain.Value;
            set => sensorGain.OnNext(value);
        }

        [Description("Liquid lens voltage(Volts RMS).")]
        [Range(24.4, 69.7)]
        [Precision(1, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double LiquidLensVoltage
        {
            get => liquidLensVoltage.Value;
            set => liquidLensVoltage.OnNext(value);
        }

        public unsafe override IObservable<UclaMiniscopeV4DataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(UclaMiniscopeV4));
                    var passthrough = device.GetPassthroughDeviceContext(DS90UB9x.ID);
                    var scopeData = device.Context.FrameReceived.Where(frame => frame.DeviceAddress == passthrough.Address);
                    return Observable.Create<UclaMiniscopeV4DataFrame>(observer =>
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

                                // Await for first row
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
                                    observer.OnNext(new UclaMiniscopeV4DataFrame(clockBuffer, hubClockBuffer, imageData.GetImage()));
                                    hubClockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                    clockBuffer = new ulong[UclaMiniscopeV4.SensorRows];
                                    sampleIndex = 0;
                                    awaitingFrameStart = true;
                                }
                            },
                            observer.OnError,
                            observer.OnCompleted);

                        return new CompositeDisposable(
                           ledBrightness.Subscribe(value => ConfigureUclaMiniscopeV4Camera.SetLedBrightness(device, value)),
                           sensorGain.Subscribe(value => ConfigureUclaMiniscopeV4Camera.SetSensorGain(device, value)),
                           liquidLensVoltage.Subscribe(value => ConfigureUclaMiniscopeV4Camera.SetLiquidLensVoltage(device, value)),
                           scopeData.SubscribeSafe(frameObserver)
                       ); ;
                    });
                }));
        }
    }
}
