using System;
using oni;

namespace OpenEphys.Onix
{
    static class ContextHelper
    {
        public static Device GetDevice(this ContextTask context, uint address, DeviceID id)
        {
            if (!context.DeviceTable.TryGetValue(address, out Device device))
            {
                throw new InvalidOperationException($"The specified device '{id}:{address}' is not present in the device table.");
            }

            if (device.ID != (int)id)
            {
                throw new InvalidOperationException($"The selected device is not a {id} device.");
            }

            return device;
        }

        public static Device GetDevice(this DeviceInfo deviceInfo, Type expectedType)
        {
            deviceInfo.AssertType(expectedType);
            if (!deviceInfo.Context.DeviceTable.TryGetValue(deviceInfo.DeviceAddress, out Device device))
            {
                throw new InvalidOperationException(
                    $"The specified device '{expectedType}:{deviceInfo.DeviceAddress}' is not present in the device table."
                );
            }

            return device;
        }
    }
}
