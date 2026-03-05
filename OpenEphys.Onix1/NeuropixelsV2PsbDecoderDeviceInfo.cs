using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2PsbDecoderDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2PsbDecoderDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, ushort streamIndex, double? gainCorrection, NeuropixelsV2ProbeConfiguration probeConfiguration)
            : base(context, deviceType, deviceAddress)
        {
            StreamIndex = streamIndex;
            GainCorrection = gainCorrection;
            ProbeConfiguration = probeConfiguration;

        }

        public ushort StreamIndex { get; }

        public double? GainCorrection { get; }

        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; }

    }
}
