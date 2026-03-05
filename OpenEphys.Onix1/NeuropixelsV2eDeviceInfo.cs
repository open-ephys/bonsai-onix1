using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, double? gainCorrectionA, double? gainCorrectionB, INeuropixelsV2Metadata probeMetadataA, INeuropixelsV2Metadata probeMetadataB, NeuropixelsV2ProbeConfiguration probeConfigurationA, NeuropixelsV2ProbeConfiguration probeConfigurationB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
            ProbeMetadataA = probeMetadataA;
            ProbeMetadataB = probeMetadataB;
            ProbeConfigurationA = probeConfigurationA;
            ProbeConfigurationB = probeConfigurationB;
        }

        public double? GainCorrectionA { get; }

        public double? GainCorrectionB { get; }

        public INeuropixelsV2Metadata ProbeMetadataA { get; }

        public INeuropixelsV2Metadata ProbeMetadataB { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationA { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationB { get; }
    }
}
