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
                throw new InvalidOperationException("Selected device address is invalid.");
            }

            if (device.ID != (int)id)
            {
                throw new InvalidOperationException($"The selected device is not a {id} device.");
            }

            return device;
        }
    }
}
