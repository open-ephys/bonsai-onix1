using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai;
using oni;

namespace OpenEphys.Onix
{
    public class Heartbeat : Source<ManagedFrame<ushort>>
    {
        public string Driver { get; set; } = "riffa";

        public int Index { get; set; }

        public uint DeviceIndex { get; set; }

        public override IObservable<ManagedFrame<ushort>> Generate()
        {
            return Observable.Using(
                () => ContextManager.ReserveContext(Driver, Index),
                disposable => disposable.Subject.SelectMany(context =>
                {
                    if (!context.DeviceTable.TryGetValue(DeviceIndex, out oni.Device device))
                    {
                        throw new InvalidOperationException("Selected device index is invalid.");
                    }

                    return context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new ManagedFrame<ushort>(frame));
                }));
        }
    }
}
