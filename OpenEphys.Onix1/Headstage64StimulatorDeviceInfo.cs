using System;

namespace OpenEphys.Onix1
{
    class Headstage64StimulatorDeviceInfo : DeviceInfo
    {
        public Headstage64StimulatorDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, uint? portControllerAddress)
            : base(context, deviceType, deviceAddress)
        {
            PortControllerAddress = portControllerAddress;
        }

        public uint? PortControllerAddress {get; }
    }
}
