using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eBetaFlexEeprom
    {
        const uint OFFSET_FLEX_VERSION = 0x00;
        const uint OFFSET_FLEX_REVISION = 0x01;
        const uint OFFSET_FLEX_PN = 0x0a;
        const uint OFFSET_PROBE_SN = 0x42;
        const uint OFFSET_PROBE_PN = 0x4a;

        readonly I2CRegisterContext flexEeprom;

        public NeuropixelsV2eBetaFlexEeprom(I2CRegisterContext serializer)
        {
            flexEeprom = new I2CRegisterContext(serializer, NeuropixelsV2Beta.FlexEEPROMAddress);
        }

        public bool TryRead(out NeuropixelsProbeMetadata metadata)
        {
            const int FailureToReadRegister = -5;

            try
            {
                var probePartNumber = flexEeprom.ReadString(OFFSET_PROBE_PN, 20);
                var probeSerialNumber = BitConverter.ToUInt64(flexEeprom.ReadBytes(OFFSET_PROBE_SN, 8), 0);
                var flexPartNumber = flexEeprom.ReadString(OFFSET_FLEX_PN, 20);
                var flexVersion = flexEeprom.ReadByte(OFFSET_FLEX_VERSION);
                var flexRevision = flexEeprom.ReadByte(OFFSET_FLEX_REVISION);
                metadata = new NeuropixelsProbeMetadata(probePartNumber, probeSerialNumber, flexPartNumber, $"{flexVersion}.{flexRevision}");
                return true;
            }
            catch (oni.ONIException ex) when (ex.Number == FailureToReadRegister)
            {
                metadata = null;
                return false;
            }
        }
    }
}
