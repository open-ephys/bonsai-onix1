using System;
using System.Text;

namespace OpenEphys.Onix1
{
    class HeadstageEeprom
    {
        public const uint I2CAddress = 0x51;

        // header field offsets (absolute, in bytes)
        const uint HeaderIdentOffset = 0; // 4 bytes: "OEHS"
        const uint HeaderVersionOffset = 4; // u8
        const uint InternalAreaOffset = 5; // u8, in multiples of 8 bytes; 0 = not present
        const uint DeviceInfoOffset = 6; // u8, in multiples of 8 bytes
        const uint LinkInfoOffset = 7; // u8, in multiples of 8 bytes
        const uint PowerInfoOffset = 8; // u8, in multiples of 8 bytes
        const uint EncodingInfoOffset = 9; // u8, in multiples of 8 bytes
        const uint RecordAreaOffset = 10; // u8, in multiples of 8 bytes; 0 = not present
        // bytes 11-14: zero padding
        const uint HeaderChecksumOffset = 15; // u8, zero-checksum over bytes 0-15

        // device info field offsets (relative to the section start, in bytes)
        const uint DevInfoVersionOffset = 0; // u8
        const uint DevInfoLengthOffset = 1; // u8, in multiples of 8 bytes
        const uint DevInfoHardwareId = 2; // u32
        const uint DevInfoHwRevision = 6; // u8 minor, u8 major 
        const uint DevInfoNameLength = 8; // u8, name length in bytes (N)
        const uint DevInfoName = 9; // N x u8, name string

        const int HeaderSizeBytes = 16;
        const byte ExpectedMagic0 = (byte)'H';
        const byte ExpectedMagic1 = (byte)'S';
        const byte ExpectedMagic2 = (byte)'O';
        const byte ExpectedMagic3 = (byte)'E';
        const byte SupportedHeaderVer = 0x01;
        const byte SupportedDevInfoVer = 0x01;

        public HeadstageEeprom(DeviceContext device)
        {
            var eeprom = new I2CRegisterContext(device, I2CAddress);

            // read and validate header 
            byte[] header = eeprom.ReadBytes(HeaderIdentOffset, HeaderSizeBytes, sixteenBitAddress: true);
            ValidateChecksum(header, 0, HeaderSizeBytes, "Header");

            if (header[0] != ExpectedMagic0 ||
                header[1] != ExpectedMagic1 ||
                header[2] != ExpectedMagic2 ||
                header[3] != ExpectedMagic3)
            {
                throw new InvalidOperationException(
                    $"Headstage EEPROM identification string is invalid. " +
                    $"Expected \"HSOE\", got \"{(char)header[0]}{(char)header[1]}{(char)header[2]}{(char)header[3]}\".");
            }

            // validate header format version
            byte headerVersion = header[HeaderVersionOffset];
            if (headerVersion != SupportedHeaderVer)
            {
                throw new NotSupportedException(
                    $"Unsupported Headstage EEPROM header version: 0x{headerVersion:X2}. Expected 0x{SupportedHeaderVer:X2}.");
            }

            // retrieve device info section byte address from header
            byte devInfoSectionOffset = header[DeviceInfoOffset];
            if (devInfoSectionOffset == 0)
            {
                throw new InvalidOperationException("Headstage EEPROM header indicates no Device info section is present.");
            }

            uint devInfoBase = (uint)(devInfoSectionOffset * 8);

            // read the first two bytes to get version and length before reading the rest
            byte devInfoVersion = eeprom.ReadByte(devInfoBase + DevInfoVersionOffset, sixteenBitAddress: true);
            if (devInfoVersion != SupportedDevInfoVer)
            {
                throw new NotSupportedException(
                    $"Unsupported Headstage EEPROM Device Info version: 0x{devInfoVersion:X2}. Expected 0x{SupportedDevInfoVer:X2}.");
            }

            byte devInfoLengthMultiple = eeprom.ReadByte(devInfoBase + DevInfoLengthOffset, sixteenBitAddress: true);
            int devInfoSizeBytes = devInfoLengthMultiple * 8;
            if (devInfoSizeBytes < 10)  // Minimum: 2 fixed header bytes + 4 (hwId) + 2 (hwRev) + 1 (nameLen) + 1 (checksum)
            {
                throw new InvalidOperationException(
                    $"Headstage EEPROM Device Info section length ({devInfoSizeBytes} bytes) is too small to be valid.");
            }

            // read the full device info section
            byte[] devInfo = eeprom.ReadBytes(devInfoBase, devInfoSizeBytes, sixteenBitAddress: true);
            ValidateChecksum(devInfo, 0, devInfoSizeBytes, "Device Info");

            // hardware id
            Id = (uint)(devInfo[DevInfoHardwareId]
                      | devInfo[DevInfoHardwareId + 1] << 8
                      | devInfo[DevInfoHardwareId + 2] << 16
                      | devInfo[DevInfoHardwareId + 3] << 24);

            // hardware revision
            Revision = new(devInfo[DevInfoHwRevision + 1], devInfo[DevInfoHwRevision]);

            // harware name
            byte nameLength = devInfo[DevInfoNameLength];
            Name = Encoding.ASCII.GetString(devInfo, (int)DevInfoName, nameLength);
        }

        /// <summary>
        /// Headstage ID.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Headstage hardware revision.
        /// </summary>
        public Version Revision { get; }

        /// <summary>
        /// Human-readable headstage name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Verifies a zero-checksum over a contiguous byte range: the unsigned sum of all bytes
        /// in the range (including the checksum byte itself) must equal 0 mod 256.
        /// </summary>
        static void ValidateChecksum(byte[] data, int offset, int length, string sectionName)
        {
            byte sum = 0;
            for (int i = offset; i < offset + length; i++)
            {
                sum += data[i];
            }

            if (sum != 0)
            {
                throw new InvalidOperationException(
                    $"Checksum validation failed for \"{sectionName}\" section " +
                    $"(sum = 0x{sum:X2}, expected 0x00).");
            }
        }
    }
}
