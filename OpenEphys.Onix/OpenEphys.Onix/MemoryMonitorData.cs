using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class MemoryMonitorData : Source<MemoryMonitorDataFrame>
    {
        [TypeConverter(typeof(MemoryMonitor.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<MemoryMonitorDataFrame> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(MemoryMonitor));
                    var totalMemory = device.ReadRegister(MemoryMonitor.TOTAL_MEM);

                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new MemoryMonitorDataFrame(frame, totalMemory));
                }));
        }
    }
}
