using System;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class Test0DataFrame
    {
        public unsafe Test0DataFrame(oni.Frame frame, int dummyWords)
        {
            Clock = frame.Clock;
            var payload = (Test0Payload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            Message = payload->Message;

            Dummy = new short[dummyWords];
            Marshal.Copy(new IntPtr(payload + 10), Dummy, 0, dummyWords);
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public short Message { get; }

        public short[] Dummy { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Test0Payload
    {
        public ulong HubClock;
        public short Message;
    }
}
