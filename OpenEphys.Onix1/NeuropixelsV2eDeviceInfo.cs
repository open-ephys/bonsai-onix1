using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, double? gainCorrectionA, double? gainCorrectionB, bool invertPolarity, INeuropixelsV2eMetadata probeMetadataA, INeuropixelsV2eMetadata probeMetadataB, NeuropixelsV2ProbeConfiguration probeConfigurationA, NeuropixelsV2ProbeConfiguration probeConfigurationB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
            InvertPolarity = invertPolarity;
            ProbeMetadataA = probeMetadataA;
            ProbeMetadataB = probeMetadataB;
            ProbeConfigurationA = probeConfigurationA;
            ProbeConfigurationB = probeConfigurationB;
        }

        public double? GainCorrectionA { get; }

        public double? GainCorrectionB { get; }

        public bool InvertPolarity { get; }

        public INeuropixelsV2eMetadata ProbeMetadataA { get; }

        public INeuropixelsV2eMetadata ProbeMetadataB { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationA { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfigurationB { get; }
    }
}
