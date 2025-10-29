using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eBetaMetadata : INeuropixelsV2eMetadata
    {
        const uint OFFSET_FLEX_VERSION = 0x00;
        const uint OFFSET_FLEX_REVISION = 0x01;
        const uint OFFSET_FLEX_PN = 0x0a;
        const uint OFFSET_PROBE_SN = 0x42;
        const uint OFFSET_PROBE_PN = 0x4a;

        public NeuropixelsV2eBetaMetadata(I2CRegisterContext serializer)
        {
            var flexI2C = new I2CRegisterContext(serializer, NeuropixelsV2e.FlexEEPROMAddress);
            try
            {
                ProbePartNumber = flexI2C.ReadString(OFFSET_PROBE_PN, 20);
                ProbeSerialNumber = BitConverter.ToUInt64(flexI2C.ReadBytes(OFFSET_PROBE_SN, 8), 0);
                FlexPartNumber = flexI2C.ReadString(OFFSET_FLEX_PN, 20);
                var flexVersion = flexI2C.ReadByte(OFFSET_FLEX_VERSION);
                var flexRevision = flexI2C.ReadByte(OFFSET_FLEX_REVISION);
                FlexVersion = $"{flexVersion}.{flexRevision}";
            }
            catch (oni.ONIException ex)
            {
                const int FailureToReadRegister = -5;
                if (ex.Number != FailureToReadRegister)
                {
                    throw;
                }
            }
        }

        public string ProbePartNumber { get; }

        public ulong? ProbeSerialNumber { get; }

        public string FlexPartNumber { get; }

        public string FlexVersion { get; }
    }
}
