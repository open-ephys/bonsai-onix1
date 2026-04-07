using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1PsbDecoderDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1PsbDecoderDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1RegisterContext probeControl, bool invertPolarity, NeuropixelsV1ProbeConfiguration probeConfiguration)
            : base(context, deviceType, deviceAddress)
        {
            ApGainCorrection = probeControl.ApGainCorrection;
            LfpGainCorrection = probeControl.LfpGainCorrection;
            AdcThresholds = probeControl.AdcThresholds;
            AdcOffsets = probeControl.AdcOffsets;
            InvertPolarity = invertPolarity;
            ProbeConfiguration = probeConfiguration;
        }

        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }
        public bool InvertPolarity { get; }
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; }
    }
}
