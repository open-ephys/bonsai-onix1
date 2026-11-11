using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    /// <summary>
    /// The ONI test driver that ships with liboni. Like hardware, it lets one context at a time hold a
    /// host index.
    /// </summary>
    static class TestDriver
    {
        internal const string Name = "test";

        /// <summary>A host index that opens without injected failures.</summary>
        internal const int Index = 0;

        /// <summary>A host index whose hubs report firmware version 0xFFFF, which is never supported.</summary>
        internal const int UnsupportedFirmwareIndex = 128;

        /// <summary>A host index that fails to open with ONI_EINIT, without being held.</summary>
        internal const int InitFailureIndex = 129;

        /// <summary>A host index that opens and is held, but whose device table repeats a device index, so
        /// that initialization fails with ONI_EDEVIDXREPEAT.</summary>
        internal const int BadDeviceTableIndex = 130;

        /// <summary>A register of any test device that, written with N > 0, makes every data stream read
        /// after the next N fail with ONI_EREADFAILURE.</summary>
        internal const uint ReadFaultRegister = 4;

        // liboni error codes
        internal const int InitError = -22; // ONI_EINIT
        internal const int RepeatedDeviceIndexError = -26; // ONI_EDEVIDXREPEAT
    }

    /// <summary>
    /// Makes the ONI drivers in the test output loadable.
    /// </summary>
    /// <remarks>
    /// liboni loads drivers with LOAD_LIBRARY_SEARCH_DEFAULT_DIRS, which searches the directory of the
    /// executable. Under a test runner that is the runner's own directory, not the test output, so the
    /// output directory is added to the search path.
    /// </remarks>
    public sealed class TestDriverFixture
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr AddDllDirectory(string newDirectory);

        public TestDriverFixture()
        {
            // NB: Not Assembly.Location, which is a shadow copy without the native libraries when the
            // runner shadow copies test assemblies.
            var directory = Path.GetDirectoryName(new Uri(typeof(TestDriverFixture).Assembly.CodeBase).LocalPath);
            if (AddDllDirectory(directory) == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not add {directory} to the DLL search path.");
        }
    }

    /// <summary>
    /// Tests that open the ONI test driver. They run one at a time, as tests that open the same
    /// hardware would have to.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class TestDriverCollection : ICollectionFixture<TestDriverFixture>
    {
        public const string Name = "ONI test driver";
    }
}
