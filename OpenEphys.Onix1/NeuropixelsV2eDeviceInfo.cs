using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, double? gainCorrectionA, double? gainCorrectionB, bool invertPolarity)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
            InvertPolarity = invertPolarity;
        }

        public double? GainCorrectionA { get; }

        public double? GainCorrectionB { get; }

        public bool InvertPolarity {  get; }
    }
}
