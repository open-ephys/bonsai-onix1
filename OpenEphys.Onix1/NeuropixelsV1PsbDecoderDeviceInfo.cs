using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1PsbDecoderDeviceInfo : DeviceInfo
    {
        public NeuropixelsV1PsbDecoderDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, NeuropixelsV1RegisterContext probeControl,
            NeuropixelsV1ProbeConfiguration probeConfiguration, NeuropixelsV1ProbeGroup probeGroup, string probePartNumber, ulong probeSerialNumber)
            : base(context, deviceType, deviceAddress)
        {
            ApGainCorrection = probeControl?.ApGainCorrection ?? 1.0;
            LfpGainCorrection = probeControl?.LfpGainCorrection ?? 1.0;
            AdcThresholds = probeControl?.AdcThresholds ?? Array.Empty<ushort>();
            AdcOffsets = probeControl?.AdcOffsets ?? Array.Empty<ushort>();
            ProbeConfiguration = probeConfiguration;
            ProbeGroup = probeGroup;
            ProbePartNumber = probePartNumber;
            ProbeSerialNumber = probeSerialNumber;
        }

        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; }
        public NeuropixelsV1ProbeGroup ProbeGroup { get; }
        public string ProbePartNumber { get; }
        public ulong ProbeSerialNumber { get; }
    }
}
