using System;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1fMetadata :  I2CRegisterContext
    {
        const uint OFFSET_ID = 0;
        const uint OFFSET_VERSION = 10;
        const uint OFFSET_REVISION = 11;
        const uint OFFSET_FLEXPN = 20;
        const uint OFFSET_PROBEPN = 40;

        public NeuropixelsV1fMetadata(DeviceContext deviceContext)
            : base(deviceContext, NeuropixelsV1.FlexEepromI2CAddress)
        {
            try
            {
                var sn = ReadBytes(OFFSET_ID, 8);
                ProbeSerialNumber = BitConverter.ToUInt64(sn, 0);
                Version = ReadByte(OFFSET_VERSION);
                Revision = ReadByte(OFFSET_REVISION);
                PartNumber = ReadString(OFFSET_FLEXPN, 20);
                ProbePartNumber = ReadString(OFFSET_PROBEPN, 20);
            }
            catch (oni.ONIException ex)
            {
                throw new InvalidOperationException("Could not communicate with probe. Ensure that the " +
                    "flex connection is properly seated.", ex);
            }
        }

        public ulong ProbeSerialNumber { get; }

        public byte Version { get; }

        public byte Revision { get; }

        public string PartNumber { get; }

        public string ProbePartNumber { get; }
    }
}
