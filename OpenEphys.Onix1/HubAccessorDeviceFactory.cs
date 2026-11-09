using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides an abstract base class for single-device configuration operators that also require register
    /// access to an <see href="https://open-ephys.github.io/ONI/">ONI</see> Hub Information Device.
    /// </summary>
    /// <remarks>
    /// Deriving from this class explicitly declares that the operator accesses hub registers, for example for
    /// hub-wide clocking or port multiplexing. Only derived classes can obtain a <see cref="HubContext"/>.
    /// </remarks>
    /// <typeparam name="TContext">
    /// A type derived from <see cref="ContextTask"/> that represents the ONI controller that this device is
    /// compatible with.
    /// </typeparam>
    public abstract class HubAccessorDeviceFactory<TContext> : SingleDeviceFactory<TContext> where TContext : ContextTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HubAccessorDeviceFactory{TContext}"/> class.
        /// </summary>
        /// <param name="deviceType">The device identity type.</param>
        protected HubAccessorDeviceFactory(Type deviceType)
            : base(deviceType)
        {
        }

        /// <summary>
        /// Gets a validated <see cref="HubContext"/> for the hub that contains the specified address.
        /// </summary>
        /// <param name="context">The acquisition context.</param>
        /// <param name="address">
        /// Any fully-qualified address on the target hub, such as the address of a device on that hub or the
        /// Hub Information Device address itself.
        /// </param>
        /// <param name="expectedType">
        /// The hub identity type. It must declare a <c>const int ID</c> field (expected
        /// <see cref="oni.Hub.HardwareID"/>) and a <c>const uint MinimumVersion</c> field (minimum
        /// <see cref="oni.Hub.FirmwareVersion"/>).
        /// </param>
        /// <returns>A <see cref="HubContext"/> for the Hub Information Device of the hub.</returns>
        /// <exception cref="InvalidOperationException">
        /// The hub hardware ID doesn't match, or its firmware version is below the minimum and the
        /// <see cref="ValidationLevel"/> is stricter than <see cref="ValidationLevel.Permissive"/>.
        /// </exception>
        protected static HubContext GetHubContext(ContextTask context, uint address, Type expectedType)
        {
            return context.GetHubContext(address, expectedType);
        }
    }
}
