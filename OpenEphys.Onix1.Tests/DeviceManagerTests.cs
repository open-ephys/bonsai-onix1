using System;
using System.Reactive.Linq;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    [Collection(TestDriverCollection.Name)]
    public class DeviceManagerTests
    {
        // NB: The device map is static and shared by every test in the process, so each test uses names of its own,
        // and a name leaked by a failing test cannot make another test fail.
        static string DeviceName(string testName, string device) => $"{nameof(DeviceManagerTests)}.{testName}.{device}";

        static IDisposable Register(string name, ContextTask context, uint address) =>
            DeviceManager.RegisterDevice(name, new DeviceInfo(context, typeof(ContextTask), address));

        // NB: With a timeout, so that a device that is in the map but never completed fails the test rather than
        // blocking it
        static DeviceInfo GetDevice(string name) =>
            DeviceManager.GetDevice(name).Timeout(TimeSpan.FromSeconds(5)).Wait();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RegisterDevice_NoName_Throws(string name)
        {
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            var error = Assert.Throws<ArgumentException>(() => Register(name, context, 0));
            Assert.Equal("name", error.ParamName);
        }

        // Registering a device makes it available by name, with the information it was registered with.
        [Fact]
        public void GetDevice_RegisteredName_ReturnsDeviceInfo()
        {
            var name = DeviceName(nameof(GetDevice_RegisteredName_ReturnsDeviceInfo), "A");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            var deviceInfo = new DeviceInfo(context, typeof(ContextTask), 3);

            using (DeviceManager.RegisterDevice(name, deviceInfo))
            {
                Assert.Same(deviceInfo, GetDevice(name));
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetDevice_NoName_Throws(string name)
        {
            var error = Assert.Throws<ArgumentException>(() => DeviceManager.GetDevice(name));
            Assert.Equal("name", error.ParamName);
        }

        // Looking up a name is deferred to subscription, which fails if no device with that name is registered.
        [Fact]
        public void GetDevice_UnknownName_ThrowsOnSubscription()
        {
            var name = DeviceName(nameof(GetDevice_UnknownName_ThrowsOnSubscription), "A");

            var device = DeviceManager.GetDevice(name);
            var error = Assert.Throws<ArgumentException>(() => device.Timeout(TimeSpan.FromSeconds(5)).Wait());
            Assert.Equal("name", error.ParamName);
        }

        // Disposing a registration removes the device, so it can no longer be looked up.
        [Fact]
        public void GetDevice_AfterDispose_Throws()
        {
            var name = DeviceName(nameof(GetDevice_AfterDispose_Throws), "A");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            Register(name, context, 0).Dispose();

            var error = Assert.Throws<ArgumentException>(() => GetDevice(name));
            Assert.Equal("name", error.ParamName);
        }

        // A name can be registered once at a time, and a failed attempt does not affect the device registered first.
        [Fact]
        public void RegisterDevice_DuplicateName_Throws()
        {
            var name = DeviceName(nameof(RegisterDevice_DuplicateName_Throws), "A");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            var deviceInfo = new DeviceInfo(context, typeof(ContextTask), 0);

            using (DeviceManager.RegisterDevice(name, deviceInfo))
            {
                var error = Assert.Throws<ArgumentException>(() => Register(name, context, 1));
                Assert.Equal("name", error.ParamName);
                Assert.Same(deviceInfo, GetDevice(name));
            }
        }

        // A device with the same name and the same address (e.g. a copied node left with its default name) is
        // reported as a duplicate name, which is the error that tells the user which node to change.
        [Fact]
        public void RegisterDevice_DuplicateNameAndAddress_ReportsName()
        {
            var name = DeviceName(nameof(RegisterDevice_DuplicateNameAndAddress_ReportsName), "A");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            using (Register(name, context, 0))
            {
                var error = Assert.Throws<ArgumentException>(() => Register(name, context, 0));
                Assert.Equal("name", error.ParamName);
            }
        }

        // An address can be registered once at a time in a context, and a failed attempt does not affect the device
        // registered first.
        [Fact]
        public void RegisterDevice_DuplicateAddress_Throws()
        {
            var nameA = DeviceName(nameof(RegisterDevice_DuplicateAddress_Throws), "A");
            var nameB = DeviceName(nameof(RegisterDevice_DuplicateAddress_Throws), "B");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            var deviceInfo = new DeviceInfo(context, typeof(ContextTask), 0);

            using (DeviceManager.RegisterDevice(nameA, deviceInfo))
            {
                var error = Assert.Throws<ArgumentException>(() => Register(nameB, context, 0));
                Assert.Equal("deviceInfo", error.ParamName);
                Assert.Same(deviceInfo, GetDevice(nameA));
            }
        }

        // A device that fails to register because its address is taken must release its name, so that it can be
        // registered once its address is corrected. The device map is static, so a leaked name would remain for
        // the rest of the process (e.g. until Bonsai is restarted).
        [Fact]
        public void RegisterDevice_DuplicateAddress_ReleasesName()
        {
            var nameA = DeviceName(nameof(RegisterDevice_DuplicateAddress_ReleasesName), "A");
            var nameB = DeviceName(nameof(RegisterDevice_DuplicateAddress_ReleasesName), "B");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            using (Register(nameA, context, 0))
            {
                var error = Assert.Throws<ArgumentException>(() => Register(nameB, context, 0));

                // NB: The address error, not the duplicate name error
                Assert.Equal("deviceInfo", error.ParamName);
            }

            // NB: A has been released and B was never registered, so B can be registered at a free address
            using var deviceB = Register(nameB, context, 1);
        }

        // Addresses only have to be unique within a context: each context has its own devices.
        [Fact]
        public void RegisterDevice_SameAddressInOtherContext_Succeeds()
        {
            var nameA = DeviceName(nameof(RegisterDevice_SameAddressInOtherContext_Succeeds), "A");
            var nameB = DeviceName(nameof(RegisterDevice_SameAddressInOtherContext_Succeeds), "B");
            using var contextA = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            using var contextB = new GenericContextTask(TestDriver.Name, TestDriver.OtherIndex);

            using var deviceA = Register(nameA, contextA, 0);
            using var deviceB = Register(nameB, contextB, 0);
        }

        // Disposing a registration releases both its name and its address, as stopping a workflow must so that it
        // can be started again.
        [Fact]
        public void Dispose_ReleasesNameAndAddress()
        {
            var nameA = DeviceName(nameof(Dispose_ReleasesNameAndAddress), "A");
            var nameB = DeviceName(nameof(Dispose_ReleasesNameAndAddress), "B");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            Register(nameA, context, 0).Dispose();

            using (Register(nameA, context, 0)) { }
            using (Register(nameB, context, 0)) { }
        }

        // Disposing a registration again, after its name has been registered by another device, must not remove
        // the new device.
        [Fact]
        public void Dispose_Repeated_DoesNotRemoveNewRegistration()
        {
            var name = DeviceName(nameof(Dispose_Repeated_DoesNotRemoveNewRegistration), "A");
            using var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            var deviceInfo = new DeviceInfo(context, typeof(ContextTask), 1);

            var first = Register(name, context, 0);
            first.Dispose();

            using (DeviceManager.RegisterDevice(name, deviceInfo))
            {
                first.Dispose();
                Assert.Same(deviceInfo, GetDevice(name));
            }
        }
    }
}
