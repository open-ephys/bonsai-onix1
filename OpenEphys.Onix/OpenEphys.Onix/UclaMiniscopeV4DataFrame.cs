using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class UclaMiniscopeV4DataFrame
    {
        public UclaMiniscopeV4DataFrame(ulong[] clock, ulong[] hubClock, IplImage image)
        {
            Clock = clock;
            HubClock = hubClock;
            Image = image;
        }

        public ulong[] Clock { get; }

        public ulong[] HubClock { get; }

        public IplImage Image { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct UclaMiniscopeV4ImagerPayload
    {
        public ulong HubClock;
        public fixed short ImageRow[UclaMiniscopeV4.SensorColumns];
    }
}
