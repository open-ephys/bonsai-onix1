using System;

namespace OpenEphys.Onix1
{
    class Rhd2000PsbDecoderDeviceInfo : DeviceInfo
    {
        public Rhd2000PsbDecoderDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, ushort streamIndex, int ephysChannelCount)
            : base(context, deviceType, deviceAddress)
        {
            StreamIndex = streamIndex;
            EphysChannelCount = ephysChannelCount;
        }

        public ushort StreamIndex { get; }

        public int EphysChannelCount { get; }
    }
}
