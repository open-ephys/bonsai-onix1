using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1PsbDecoderDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1PsbDecoderDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1RegisterContext probeControl, NeuropixelsV1ProbeConfiguration probeConfiguration, NeuropixelsV1eProbeGroup probeGroup)
            : base(context, deviceType, deviceAddress)
        {
            ApGainCorrection = probeControl.ApGainCorrection;
            LfpGainCorrection = probeControl.LfpGainCorrection;
            AdcThresholds = probeControl.AdcThresholds;
            AdcOffsets = probeControl.AdcOffsets;
            ProbeConfiguration = probeConfiguration;
            ProbeGroup = probeGroup;
        }

        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; }
        public NeuropixelsV1eProbeGroup ProbeGroup { get; }
    }
}
