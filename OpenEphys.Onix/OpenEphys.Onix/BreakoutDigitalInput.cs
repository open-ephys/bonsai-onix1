using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class BreakoutDigitalInput : Source<BreakoutDigitalInputDataFrame>
    {
        [TypeConverter(typeof(BreakoutDigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        public unsafe override IObservable<BreakoutDigitalInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(BreakoutDigitalIO));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new BreakoutDigitalInputDataFrame(frame));
            });
        }
    }
}
