using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1fDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1fDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1ProbeGroup probeGroup)
            : base(context, deviceType, deviceAddress)
        {
            ProbeGroup = probeGroup;
        }

        public NeuropixelsV1ProbeGroup ProbeGroup { get; }
    }
}
