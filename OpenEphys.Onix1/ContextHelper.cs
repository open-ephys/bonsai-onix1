using System;
using System.Linq;
using System.Reflection;
using System.Reactive.Disposables;
using System.Threading;
using oni;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies how strictly hardware validation checks are enforced.
    /// </summary>
    public enum ValidationLevel
    {
        /// <summary>
        /// All validation checks throw an exception on failure.
        /// </summary>
        Strict,

        /// <summary>
        /// The default set of validation checks throw an exception on failure. Some checks that are
        /// considered non-critical only produce a warning.
        /// </summary>
        Normal,

        /// <summary>
        /// Validation checks produce a warning instead of throwing an exception on failure.
        /// </summary>
        Permissive
    }

    /// <summary>
    /// Holds the <see cref="ValidationLevel"/> in effect for whatever configuration action is
    /// currently running, so deeply-nested code (e.g. a register context with no <see cref="ContextTask"/>
    /// reference) can consult it without threading a value through every constructor.
    /// </summary>
    static class ValidationScope
    {
        static readonly AsyncLocal<ValidationLevel?> current = new();

        /// <summary>
        /// Gets the <see cref="ValidationLevel"/> established by the innermost enclosing <see
        /// cref="Enter"/> scope, or <see cref="ValidationLevel.Normal"/> if none is active.
        /// </summary>
        public static ValidationLevel Level => current.Value ?? ValidationLevel.Normal;

        /// <summary>
        /// Establishes <paramref name="validationLevel"/> as <see cref="Level"/> until the returned <see
        /// cref="IDisposable"/> is disposed, restoring whatever was in effect before.
        /// </summary>
        public static IDisposable Enter(ValidationLevel validationLevel)
        {
            var previous = current.Value;
            current.Value = validationLevel;
            return Disposable.Create(() => current.Value = previous);
        }
    }

    static class ContextHelper
    {
        /// <summary>
        /// Enforces a validation check according to <see cref="ValidationScope.Level"/>.
        /// </summary>
        /// <param name="relaxedAt">The <see cref="ValidationLevel"/> level at which this check stops being
        /// fatal.</param>
        /// <param name="exception">The exception describing the validation failure. Thrown if <see
        /// cref="ValidationScope.Level"/> is stricter than <paramref name="relaxedAt"/>, otherwise its
        /// message is printed as a warning.</param>
        public static void Validate(ValidationLevel relaxedAt, Exception exception)
        {
            if (ValidationScope.Level >= relaxedAt)
            {
                Console.Error.WriteLine($"Warning: {exception.Message}");
            }
            else
            {
                throw exception;
            }
        }

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
            Console.Error.WriteLine($"In order to use this device, you will need to use a previous version of {libraryName} " +
                $"that is compatible with {expectedType.Name} v{deviceVersion}.");

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
