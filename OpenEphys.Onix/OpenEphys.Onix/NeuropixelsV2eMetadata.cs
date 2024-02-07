using System;

namespace OpenEphys.Onix
{
    class NeuropixelsV2eMetadata
    {
        const uint OFFSET_ID = 0;
        const uint OFFSET_VERSION = 10;
        const uint OFFSET_REVISION = 11;
        const uint OFFSET_FLEXPN = 20;
        const uint OFFSET_PROBEPN = 40;

        public NeuropixelsV2eMetadata(I2CRegisterContext serializer)
        {
            var flexI2C = new I2CRegisterContext(serializer, NeuropixelsV2e.FlexEEPROMAddress);
            try
            {
                var sn = flexI2C.ReadBytes(OFFSET_ID, 8);
                ProbeSN = BitConverter.ToUInt64(sn, 0);
                Version = flexI2C.ReadByte(OFFSET_VERSION);
                Revision = flexI2C.ReadByte(OFFSET_REVISION);
                PartNumber = flexI2C.ReadString(OFFSET_FLEXPN, 20);
                ProbePartNumber = flexI2C.ReadString(OFFSET_PROBEPN, 20);
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

        public ulong? ProbeSN { get; }

        public byte Version { get; }

        public byte Revision { get; }

        public string PartNumber { get; }

        public string ProbePartNumber { get; }
    }
}
