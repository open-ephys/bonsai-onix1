using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class HarpSyncInputData : Source<HarpSyncInputDataFrame>
    {
        [TypeConverter(typeof(HarpSyncInput.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<HarpSyncInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(HarpSyncInput));
                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new HarpSyncInputDataFrame(frame));
            });
        }
    }
}
