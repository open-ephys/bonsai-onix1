using System;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using oni;

namespace OpenEphys.Onix
{
    public class AnalogOutput : Sink<ushort[]>
    {
        public string DeviceName { get; set; }

        public override IObservable<ushort[]> Process(IObservable<ushort[]> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var (context, deviceIndex) = deviceInfo;
                    if (!context.DeviceTable.TryGetValue(deviceIndex, out Device device))
                    {
                        throw new InvalidOperationException("Selected device index is invalid.");
                    }

                    return source.Do(data =>
                    {
                        context.Write(device.Address, data);
                    });
                }));
        }
    }
}
