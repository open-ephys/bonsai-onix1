using System;
using System.Net;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Rhd2164DataFrame
    {
        public unsafe Rhd2164DataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (Rhd2164Payload*)frame.Data.ToPointer();
            HubClock = unchecked((ulong)IPAddress.NetworkToHostOrder(payload->HubClock));
            AmplifierData = MatHelper.GetMatData(64, 1, Depth.S16, payload->AmplifierData);
            AuxData = MatHelper.GetMatData(3, 1, Depth.S16, payload->AuxData);
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public Mat AmplifierData { get; }

        public Mat AuxData { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct Rhd2164Payload
    {
        public long HubClock;
        public fixed short AmplifierData[64];
        public fixed short AuxData[3];
    }
}
