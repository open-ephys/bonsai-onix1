using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai;
using oni;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class AnalogOutput : Sink<ushort[]>
    {
        public string Driver { get; set; } = "riffa";

        public int Index { get; set; }

        public uint DeviceIndex { get; set; }

        public override IObservable<ushort[]> Process(IObservable<ushort[]> source)
        {
            return Observable.Using(
                () => ContextManager.ReserveContext(Driver, Index),
                disposable => disposable.Subject.SelectMany(context =>
                {
                    if (!context.DeviceTable.TryGetValue(DeviceIndex, out oni.Device device))
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
