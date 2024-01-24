using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class HeartbeatCounter : Source<ManagedFrame<ushort>>
    {
        [TypeConverter(typeof(Heartbeat.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<ManagedFrame<ushort>> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Heartbeat));
                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new ManagedFrame<ushort>(frame));
                }));
        }
    }
}
