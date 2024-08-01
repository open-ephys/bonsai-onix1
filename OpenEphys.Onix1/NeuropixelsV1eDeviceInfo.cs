using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1eRegisterContext probeControl)
            : base(context, deviceType, deviceAddress)
        {
            ApGainCorrection = probeControl.ApGainCorrection;
            LfpGainCorrection = probeControl.LfpGainCorrection;
            AdcThresholds = probeControl.AdcThresholds;
            AdcOffsets = probeControl.AdcOffsets;
        }

        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }
    }
}
