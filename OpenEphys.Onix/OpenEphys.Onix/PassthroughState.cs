using System;

namespace OpenEphys.Onix
{
    [Flags]
    public enum PassthroughState
    {
        PortA = 1 << 0,
        PortB = 1 << 2
    }
}
