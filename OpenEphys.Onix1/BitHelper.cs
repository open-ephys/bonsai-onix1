using System.Net;

namespace OpenEphys.Onix1
{
    static class BitHelper
    {
        internal static uint Replace(uint value, uint mask, uint bits)
        {
            return (value & ~mask) | (bits & mask);
        }
    }
}
