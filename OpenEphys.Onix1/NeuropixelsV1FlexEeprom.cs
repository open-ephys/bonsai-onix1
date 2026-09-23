using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1FlexEeprom
    {
        const uint OFFSET_ID = 0;
        const uint OFFSET_VERSION = 10;
        const uint OFFSET_REVISION = 11;
        const uint OFFSET_FLEXPN = 20;
        const uint OFFSET_PROBEPN = 40;

        readonly I2CRegisterContext flexEeprom;

        public NeuropixelsV1FlexEeprom(DeviceContext deviceContext)
        {
            flexEeprom = new I2CRegisterContext(deviceContext, NeuropixelsV1.FlexEepromI2CAddress);
        }

        public bool TryRead(out NeuropixelsProbeMetadata metadata)
        {
            const int FailureToReadRegister = -5;

            try
            {
                var probeSerialNumber = BitConverter.ToUInt64(flexEeprom.ReadBytes(OFFSET_ID, 8), 0);
                var flexVersion = flexEeprom.ReadByte(OFFSET_VERSION);
                var flexRevision = flexEeprom.ReadByte(OFFSET_REVISION);
                var flexPartNumber = flexEeprom.ReadString(OFFSET_FLEXPN, 20);
                var probePartNumber = flexEeprom.ReadString(OFFSET_PROBEPN, 20);
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
