using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class BreakoutDigitalOutput : Sink<BreakoutDigitalPortState>
    {
        [TypeConverter(typeof(BreakoutDigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<BreakoutDigitalPortState> Process(IObservable<BreakoutDigitalPortState> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutDigitalIO));
                return source.Do(value => device.Write((uint)value));
            });
        }
    }
}
