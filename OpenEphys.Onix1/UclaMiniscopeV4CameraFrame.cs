using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Image data produced by the Python-480 CMOS image sensor on a UCLA Miniscope V4.
    /// </summary>
    public class UclaMiniscopeV4CameraFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UclaMiniscopeV4CameraFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="image">A image produced by the Python-480 on a UCLA Miniscope V4.</param>
        public UclaMiniscopeV4CameraFrame(ulong[] clock, ulong[] hubClock, IplImage image)
            : base (clock, hubClock)
        {
            Image = image;
        }

        /// <summary>
        /// Gets the 608x608 pixel, 10-bit, monochrome image.
        /// </summary>
        public IplImage Image { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct UclaMiniscopeV4ImagerPayload
    {
        public ulong HubClock;
        public fixed short ImageRow[UclaMiniscopeV4.SensorColumns];
    }
}
