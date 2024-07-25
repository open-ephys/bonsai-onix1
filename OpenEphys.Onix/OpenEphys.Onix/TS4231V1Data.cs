using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class TS4231V1Data : Source<TS4231V1DataFrame>
    {
        [TypeConverter(typeof(TS4231V1.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<TS4231V1DataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(TS4231V1));
                return deviceInfo.Context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new TS4231V1DataFrame(frame));
            });
        }
    }
}
