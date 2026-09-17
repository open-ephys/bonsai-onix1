using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eBetaMetadata : I2CRegisterContext, INeuropixelsV2Metadata
    {
        const uint OFFSET_FLEX_VERSION = 0x00;
        const uint OFFSET_FLEX_REVISION = 0x01;
        const uint OFFSET_FLEX_PN = 0x0a;
        const uint OFFSET_PROBE_SN = 0x42;
        const uint OFFSET_PROBE_PN = 0x4a;

        public NeuropixelsV2eBetaMetadata(DeviceContext deviceContext, string deviceName)
            : base(deviceContext, NeuropixelsV2.FlexEepromI2CAddress)
        {
            try
            {
                ProbePartNumber = ReadString(OFFSET_PROBE_PN, 20);
                ProbeSerialNumber = BitConverter.ToUInt64(ReadBytes(OFFSET_PROBE_SN, 8), 0);
                FlexPartNumber =ReadString(OFFSET_FLEX_PN, 20);
                var flexVersion = ReadByte(OFFSET_FLEX_VERSION);
                var flexRevision = ReadByte(OFFSET_FLEX_REVISION);
                FlexVersion = $"{flexVersion}.{flexRevision}";
            }
            catch (oni.ONIException ex)
            {
                throw new InvalidOperationException($"Could not communicate with probe \"{deviceName}\". Check that the flex cable is " +
                    "properly seated. If this probe is not in use, set its Enable property to false.", ex);
            }
        }

        public string ProbePartNumber { get; }

        public ulong ProbeSerialNumber { get; }

        public string FlexPartNumber { get; }

        public string FlexVersion { get; }
    }
}
