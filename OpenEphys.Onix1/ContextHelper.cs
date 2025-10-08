using System;
using System.Linq;
using System.Reflection;
using oni;

namespace OpenEphys.Onix1
{
    static class ContextHelper
    {
        public static DeviceContext GetDeviceContext(this ContextTask context, uint address, Type expectedType)
        {
            if (!context.DeviceTable.TryGetValue(address, out Device device))
            {
                ThrowDeviceNotFoundException(expectedType, address);
            }

            if (device.ID != GetDeviceID(expectedType))
            {
                ThrowInvalidDeviceException(expectedType, address);
            }

            var minVersion = GetMinimumFirmwareVersion(expectedType);

            if (device.Version < minVersion)
            {
                ThrowInvalidDeviceVersionException(expectedType, address, device.Version, minVersion);
            }

            return new DeviceContext(context, device);
        }

        public static DeviceContext GetDeviceContext(this DeviceInfo deviceInfo, Type expectedType)
        {
            deviceInfo.AssertType(expectedType);
            if (!deviceInfo.Context.DeviceTable.TryGetValue(deviceInfo.DeviceAddress, out Device device))
            {
                ThrowDeviceNotFoundException(expectedType, deviceInfo.DeviceAddress);
            }

            return new DeviceContext(deviceInfo.Context, device);
        }

        public static DeviceContext GetPassthroughDeviceContext(this ContextTask context, uint address, Type expectedType)
        {
            var passthroughDeviceAddress = context.GetPassthroughDeviceAddress(address);
            return GetDeviceContext(context, passthroughDeviceAddress, expectedType);
        }

        public static DeviceContext GetPassthroughDeviceContext(this DeviceContext device, Type expectedType)
        {
            return GetPassthroughDeviceContext(device.Context, device.Address, expectedType);
        }

        static int GetDeviceID(Type deviceType)
        {
            var fieldInfo = deviceType.GetField(
                "ID",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase);
            if (fieldInfo == null || !fieldInfo.IsLiteral)
            {
                throw new ArgumentException($"The specified device type {deviceType} does not have a const ID field.", nameof(deviceType));
            }

            return (int)fieldInfo.GetRawConstantValue();
        }

        static uint GetMinimumFirmwareVersion(Type deviceType)
        {
            var fieldInfo = deviceType.GetField(
                "MinimumVersion",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase);
            if (fieldInfo == null || !fieldInfo.IsLiteral)
            {
                throw new ArgumentException($"The specified device type {deviceType} does not have a const MinimumVersion field.", nameof(deviceType));
            }

            return (uint)fieldInfo.GetRawConstantValue();
        }


        static void ThrowDeviceNotFoundException(Type expectedType, uint address)
        {
            throw new InvalidOperationException($"Device '{expectedType.Name}' was not found in the device table at address {address}.");
        }

        static void ThrowInvalidDeviceException(Type expectedType, uint address)
        {
            throw new InvalidOperationException($"Invalid device ID. The device found at address {address} is not a '{expectedType.Name}' device.");
        }

        static void ThrowInvalidDeviceVersionException(Type expectedType, uint address, uint deviceVersion, uint minimumVersion)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string libraryName = assembly.GetName().Name ?? "Unknown";
            string libraryVersion = assembly.GetName().Version.ToString() ?? "Unknown";

            Console.Error.WriteLine($"Error: The {expectedType.Name} device at address {address} is v{deviceVersion}, " +
                $"but v{minimumVersion} is required by {libraryName} {libraryVersion}.");
            Console.Error.WriteLine($"In order to use {libraryName} {libraryVersion} with this device, you will need to update it firmware. " +
                $"Firmware update files and instructions can be found at https://open-ephys.github.io/onix-docs/index.html.");

            throw new InvalidOperationException($"Invalid device version. The {expectedType.Name} device at address {address} is v{deviceVersion}, " +
                $"but v{minimumVersion} is required.");
        }

        internal static bool CheckDeviceType(Type deviceType, Type targetType)
        {
            if (deviceType == targetType) return true;

            var equivalentTypes = deviceType.GetCustomAttributes(typeof(EquivalentDataSource), false).Cast<EquivalentDataSource>();

            return equivalentTypes.Any(t => t.BaseType == targetType);
        }
    }
}
