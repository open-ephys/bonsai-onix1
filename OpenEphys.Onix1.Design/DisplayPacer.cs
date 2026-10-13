using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Asks a window to repaint once per display refresh, from a thread of its own.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The UI thread is shared by the Bonsai editor and every visualizer, so it cannot wait on the display
    /// itself. This thread waits on <c>DwmFlush</c>, which returns when the desktop compositor next presents,
    /// then invalidates the window, so the repaint arrives as an ordinary <c>WM_PAINT</c>.
    /// </para>
    /// <para>
    /// It does not invalidate again until <see cref="Painted"/> says that paint has run. A frame longer than a
    /// refresh then skips refreshes, rather than keeping the window invalid, which would starve the lower
    /// priority <c>WM_TIMER</c> that delivers data.
    /// </para>
    /// </remarks>
    internal sealed class DisplayPacer : IDisposable
    {
        // NB: DwmFlush returns at once when the compositor has nothing to present, as while a paint is still
        // pending, and without this the thread would spin.
        static readonly long ShortestWait = Stopwatch.Frequency / 1000;
        const int IdleMilliseconds = 5;

        // NB: the monitor's own refresh rate is read again every so often, so that moving the window to another
        // monitor is picked up.
        const int RefreshesBetweenReads = 100;
        const int DefaultRefreshRate = 60;

        readonly IntPtr window;
        readonly Thread thread;
        volatile bool running = true;
        volatile int refreshRate = DefaultRefreshRate;
        int pending;

        public DisplayPacer(IntPtr window)
        {
            this.window = window;
            thread = new Thread(Run) { IsBackground = true, Name = nameof(DisplayPacer) };
            thread.Start();
        }

        /// <summary>
        /// Refresh rate, in Hz, of the monitor the window is on.
        /// </summary>
        public int RefreshRate => refreshRate;

        /// <summary>
        /// Records that the window has painted, so the next refresh may invalidate it again.
        /// </summary>
        public void Painted() => Interlocked.Exchange(ref pending, 0);

        void Run()
        {
            for (var refreshes = 0; running; refreshes++)
            {
                if (refreshes % RefreshesBetweenReads == 0)
                    refreshRate = ReadRefreshRate();

                var start = Stopwatch.GetTimestamp();
                if (DwmFlush() != 0 || Stopwatch.GetTimestamp() - start < ShortestWait)
                    Thread.Sleep(IdleMilliseconds);

                if (running && Interlocked.CompareExchange(ref pending, 1, 0) == 0)
                    InvalidateRect(window, IntPtr.Zero, false);
            }
        }

        int ReadRefreshRate()
        {
            var mode = new DisplayMode { dmSize = (short)Marshal.SizeOf<DisplayMode>() };
            var device = System.Windows.Forms.Screen.FromHandle(window).DeviceName;

            // NB: 0 and 1 stand for the hardware's default rate, which says nothing about what it is.
            return EnumDisplaySettings(device, CurrentSettings, ref mode) && mode.dmDisplayFrequency > 1
                ? mode.dmDisplayFrequency
                : DefaultRefreshRate;
        }

        public void Dispose()
        {
            running = false;
            thread.Join();
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmFlush();

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        const int CurrentSettings = -1;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DisplayMode devMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DisplayMode
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
    }
}
