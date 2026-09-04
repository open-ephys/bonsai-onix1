using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2PsbDecoderDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2PsbDecoderDeviceInfo(
            ContextTask context,
            Type deviceType,
            uint deviceAddress,
            ushort streamIndex,
            double gainCorrection,
            NeuropixelsV2ProbeConfiguration probeConfiguration,
            NeuropixelsV2ProbeGroup probeGroup,
            string probePartNumber,
            ulong? probeSerialNumber)
            : base(context, deviceType, deviceAddress)
        {
            StreamIndex = streamIndex;
            GainCorrection = gainCorrection;
            ProbeConfiguration = probeConfiguration;
            ProbeGroup = probeGroup;
            ProbePartNumber = probePartNumber;
            ProbeSerialNumber = probeSerialNumber;
        }

        public ushort StreamIndex { get; }

        public double GainCorrection { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; }

        public NeuropixelsV2ProbeGroup ProbeGroup { get; }

        public string ProbePartNumber { get; }

        public ulong? ProbeSerialNumber { get; }
    }
}
