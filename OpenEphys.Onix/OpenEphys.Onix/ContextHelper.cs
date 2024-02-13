using System;
using oni;

namespace OpenEphys.Onix
{
    static class ContextHelper
    {
        public static DeviceContext GetDeviceContext(this ContextTask context, uint address, int id)
        {
            if (!context.DeviceTable.TryGetValue(address, out Device device))
            {
                throw new InvalidOperationException($"The specified device '{id}:{address}' is not present in the device table.");
            }

            if (device.ID != id)
            {
                throw new InvalidOperationException($"The selected device is not a {id} device.");
            }

            return new DeviceContext(context, device);
        }

        public static DeviceContext GetDeviceContext(this DeviceInfo deviceInfo, Type expectedType)
        {
            deviceInfo.AssertType(expectedType);
            if (!deviceInfo.Context.DeviceTable.TryGetValue(deviceInfo.DeviceAddress, out Device device))
            {
                throw new InvalidOperationException(
                    $"The specified device '{expectedType}:{deviceInfo.DeviceAddress}' is not present in the device table."
                );
            }

            return new DeviceContext(deviceInfo.Context, device);
        }

        public static DeviceContext GetPassthroughDeviceContext(this ContextTask context, uint address, int id)
        {
            var passthroughDeviceAddress = context.GetPassthroughDeviceAddress(address);
            return GetDeviceContext(context, passthroughDeviceAddress, id);
        }

        public static DeviceContext GetPassthroughDeviceContext(this DeviceContext device, int id)
        {
            return GetPassthroughDeviceContext(device.Context, device.Address, id);
        }
    }
}
