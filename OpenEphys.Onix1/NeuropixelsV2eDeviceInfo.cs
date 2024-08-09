using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, double? gainCorrectionA, double? gainCorrectionB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
        }

        public double? GainCorrectionA { get; }

        public double? GainCorrectionB { get; }
    }
}
