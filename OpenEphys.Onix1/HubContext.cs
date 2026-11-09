using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides register access to the Hub Information Device of an
    /// <see href="https://open-ephys.github.io/ONI/">ONI</see> hub.
    /// </summary>
    /// <remarks>
    /// Instances can only be obtained from classes derived from <see cref="HubAccessorDeviceFactory{TContext}"/>,
    /// which validate the hub identity and firmware version every time a <see cref="HubContext"/> is requested.
    /// </remarks>
    public sealed class HubContext
    {
        const uint HubManagerIndex = 0xFE;

        readonly ContextTask context;
        readonly uint address;

        internal HubContext(ContextTask context, uint deviceAddress)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            address = GenericHelper.GetHubAddressFromDeviceAddress(deviceAddress) | HubManagerIndex;
        }

        /// <summary>
        /// Gets the context that the hub belongs to.
        /// </summary>
        public ContextTask Context => context;

        /// <summary>
        /// Gets the fully-qualified address of the Hub Information Device.
        /// </summary>
        public uint Address => address;

        /// <summary>
        /// Gets the hub metadata, read from the hardware.
        /// </summary>
        public oni.Hub Hub => context.GetHub(address);

        /// <summary>
        /// Reads a register of the Hub Information Device.
        /// </summary>
        /// <param name="registerAddress">The register address.</param>
        /// <returns>The register value.</returns>
        public uint ReadRegister(uint registerAddress)
        {
            return context.ReadRegister(address, registerAddress);
        }

        /// <summary>
        /// Writes a register of the Hub Information Device.
        /// </summary>
        /// <param name="registerAddress">The register address.</param>
        /// <param name="value">The value to write.</param>
        public void WriteRegister(uint registerAddress, uint value)
        {
            context.WriteRegister(address, registerAddress, value);
        }
    }
}
