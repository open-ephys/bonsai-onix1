using System;

namespace OpenEphys.Onix1
{
    internal class IntanSPIDeviceInfo : DeviceInfo
    {
        public IntanSPIDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, Rhd2000ChipId rhdMiso1, Rhd2000ChipId rhdMiso2)
            : base(context, deviceType, deviceAddress)
        {
            RhdMiso1 = rhdMiso1;
            RhdMiso2 = rhdMiso2;
        }

        public Rhd2000ChipId RhdMiso1 { get; }
        public Rhd2000ChipId RhdMiso2 { get; }
    }
}
