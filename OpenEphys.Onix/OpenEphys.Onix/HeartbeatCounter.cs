﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using oni;

namespace OpenEphys.Onix
{
    public class HeartbeatCounter : Source<ManagedFrame<ushort>>
    {
        public string DeviceName { get; set; }

        public override IObservable<ManagedFrame<ushort>> Generate()
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    deviceInfo.AssertType(typeof(Heartbeat));
                    var (context, deviceIndex) = deviceInfo;
                    if (!context.DeviceTable.TryGetValue(deviceIndex, out Device device))
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