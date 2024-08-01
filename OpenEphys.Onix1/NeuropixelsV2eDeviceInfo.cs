using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eDeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, ushort? gainCorrectionA, ushort? gainCorrectionB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA;
            GainCorrectionB = gainCorrectionB;
        }

        public ushort? GainCorrectionA { get; }

        public ushort? GainCorrectionB { get; }
    }
}
