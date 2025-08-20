using System;

namespace OpenEphys.Onix1
{
    class LoadTesterDeviceInfo : DeviceInfo
    {
        public LoadTesterDeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, uint receivedWords, uint transmittedWords)
            : base(context, deviceType, deviceAddress)
        {
            ReceivedWords = receivedWords;
            TransmittedWords = transmittedWords;
        }

        public uint ReceivedWords { get; }

        public uint TransmittedWords { get; }
    }
}
