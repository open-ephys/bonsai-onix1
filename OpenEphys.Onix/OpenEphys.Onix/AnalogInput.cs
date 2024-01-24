using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class AnalogInput : Source<ManagedFrame<short>>
    {
        [TypeConverter(typeof(AnalogIO.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<ManagedFrame<short>> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(AnalogIO));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new ManagedFrame<short>(frame));
                }));
        }
    }
}
