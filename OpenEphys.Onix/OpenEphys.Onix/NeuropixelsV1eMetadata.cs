using System;

namespace OpenEphys.Onix
{
    class NeuropixelsV1eMetadata
    {
        const uint OFFSET_ID = 0;
        const uint OFFSET_VERSION = 10;
        const uint OFFSET_REVISION = 11;
        const uint OFFSET_FLEXPN = 20;
        const uint OFFSET_PROBEPN = 40;

        public NeuropixelsV1eMetadata(I2CRegisterContext serializer)
        {
            var flexI2C = new I2CRegisterContext(serializer, NeuropixelsV2e.FlexEEPROMAddress);
            var sn = flexI2C.ReadBytes(OFFSET_ID, 8);
            ProbeSN = BitConverter.ToUInt64(sn, 0);
            Version = flexI2C.ReadByte(OFFSET_VERSION);
            Revision = flexI2C.ReadByte(OFFSET_REVISION);
            PartNumber = flexI2C.ReadString(OFFSET_FLEXPN, 20);
            ProbePartNumber = flexI2C.ReadString(OFFSET_PROBEPN, 20);
        }

        public ulong ProbeSN { get; }

        public byte Version { get; }

        public byte Revision { get; }

        public string PartNumber { get; }

        public string ProbePartNumber { get; }
    }
}
