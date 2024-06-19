using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class DigitalInput : Source<DigitalInputDataFrame>
    {
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        public unsafe override IObservable<DigitalInputDataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new DigitalInputDataFrame(frame));
                }));
        }
    }
}
