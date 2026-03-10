using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, double? gainCorrectionA, double? gainCorrectionB, INeuropixelsV2eMetadata probeMetadataA, INeuropixelsV2eMetadata probeMetadataB, NeuropixelsV2ProbeConfiguration probeConfigurationA, NeuropixelsV2ProbeConfiguration probeConfigurationB, NeuropixelsV2eProbeGroup probeGroupA, NeuropixelsV2eProbeGroup probeGroupB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
            ProbeMetadataA = probeMetadataA;
            ProbeMetadataB = probeMetadataB;
            ProbeConfigurationA = probeConfigurationA;
            ProbeConfigurationB = probeConfigurationB;
            ProbeGroupA = probeGroupA;
            ProbeGroupB = probeGroupB;
        }

        public double? GainCorrectionA { get; }

        public double? GainCorrectionB { get; }

        public INeuropixelsV2eMetadata ProbeMetadataA { get; }

        public INeuropixelsV2eMetadata ProbeMetadataB { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationA { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationB { get; }

        public NeuropixelsV2eProbeGroup ProbeGroupA { get; }

        public NeuropixelsV2eProbeGroup ProbeGroupB { get; }
    }
}
