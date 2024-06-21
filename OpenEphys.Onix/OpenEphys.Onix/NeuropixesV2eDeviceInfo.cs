using System;

namespace OpenEphys.Onix
{
    class NeuropixesV2eDeviceInfo : DeviceInfo
    {
        public NeuropixesV2eDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, ushort? gainCorrectionA, ushort? gainCorrectionB)
            : base(context, deviceType, deviceAddress)
        {
            GainCorrectionA = gainCorrectionA; 
            GainCorrectionB = gainCorrectionB;
        }

        public ushort? GainCorrectionA { get; }
        public ushort? GainCorrectionB { get; }
    }
}
