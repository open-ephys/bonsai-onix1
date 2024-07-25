using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class HeartbeatData : Source<HeartbeatDataFrame>
    {
        [TypeConverter(typeof(Heartbeat.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<HeartbeatDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Heartbeat));
                return deviceInfo.Context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new HeartbeatDataFrame(frame));
            });
        }
    }
}
