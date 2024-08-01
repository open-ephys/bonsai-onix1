using System;

namespace OpenEphys.Onix1
{
    [Flags]
    internal enum PassthroughState
    {
        PortA = 1 << 0,
        PortB = 1 << 2
    }
}
