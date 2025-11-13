using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1fDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1fDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1ProbeConfiguration probeConfiguration)
            : base(context, deviceType, deviceAddress)
        {
            ProbeConfiguration = probeConfiguration;
        }

        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; }
    }
}
