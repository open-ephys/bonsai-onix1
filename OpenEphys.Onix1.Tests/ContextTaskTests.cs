using System;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    [Collection(TestDriverCollection.Name)]
    public class ContextTaskTests
    {
        // A constructor that fails must release the hardware, so that opening it again fails the same way
        // rather than because the first context still holds it.
        [Fact]
        public void Constructor_UnsupportedFirmware_ReleasesHardware()
        {
            Assert.Throws<NotSupportedException>(() => new OnixContextTask(TestDriver.Name, TestDriver.UnsupportedFirmwareIndex, ValidationLevel.Normal));

            // NB: If the first context was not disposed, it still holds the host index, and the test driver
            // refuses to open it again with ONI_ERETRIG, which ContextTask throws as an ONIException
            Assert.Throws<NotSupportedException>(() => new OnixContextTask(TestDriver.Name, TestDriver.UnsupportedFirmwareIndex, ValidationLevel.Normal));
        }

        // A context whose initialization fails after the driver has opened the hardware must release it, so
        // that opening it again fails the same way.
        [Fact]
        public void Constructor_BadDeviceTable_ReleasesHardware()
        {
            var error = Assert.Throws<oni.ONIException>(() => new GenericContextTask(TestDriver.Name, TestDriver.BadDeviceTableIndex));
            Assert.Equal(TestDriver.RepeatedDeviceIndexError, error.Number);

            // NB: If the first context was not destroyed, it still holds the host index, and the test driver
            // refuses to open it again with ONI_ERETRIG
            error = Assert.Throws<oni.ONIException>(() => new GenericContextTask(TestDriver.Name, TestDriver.BadDeviceTableIndex));
            Assert.Equal(TestDriver.RepeatedDeviceIndexError, error.Number);
        }

        sealed class GenericContextTask : ContextTask
        {
            public GenericContextTask(string driver, int index)
                : base(driver, index)
            {
            }
        }
    }
}
