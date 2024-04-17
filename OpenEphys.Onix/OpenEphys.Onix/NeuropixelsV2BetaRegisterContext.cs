using System;
using System.Collections;
using Bonsai;

namespace OpenEphys.Onix
{
    class NeuropixelsV2BetaRegisterContext : I2CRegisterContext
    {
        public NeuropixelsV2BetaRegisterContext(I2CRegisterContext other, uint i2cAddress)
            : base(other, i2cAddress)
        {
        }

        public NeuropixelsV2BetaRegisterContext(DeviceContext deviceContext, uint i2cAddress)
            : base(deviceContext, i2cAddress)
        {
        }

        public void WriteConfiguration(NeuropixelsV2eBetaChannelReference reference)
        {
            var shankBits = GenerateShankBits();
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN4, shankBits[0], read_check: true);
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN3, shankBits[1], read_check: true);
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN2, shankBits[2], read_check: true);
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN1, shankBits[3], read_check: true);

            var baseBits = GenerateBaseBits(reference);
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN5, baseBits[0], true);
            WriteShiftRegister(NeuropixelsV2eBeta.SR_CHAIN6, baseBits[1], true);
        }

        // Bits go into the shift registers MSB first
        // This creates a *bit-reversed* byte array from a bit array
        static byte[] BitArrayToBytes(BitArray bits)
        {
            if (bits.Length == 0)
            {
                throw new ArgumentException("Shift register data is empty", nameof(bits));
            }

            var bytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                // NB: http://graphics.stanford.edu/~seander/bithacks.html
                bytes[i] = (byte)((bytes[i] * 0x0202020202ul & 0x010884422010ul) % 1023);
            }

            return bytes;
        }

        private void WriteShiftRegister(uint sr_addr, BitArray data, bool read_check = false)
        {
            var bytes = BitArrayToBytes(data);

            var count = read_check ? 2 : 1;
            while (count-- > 0)
            {
                WriteByte(NeuropixelsV2eBeta.SR_LENGTH1, (uint)bytes.Length % 0x100);
                WriteByte(NeuropixelsV2eBeta.SR_LENGTH2, (uint)bytes.Length / 0x100);

                foreach (var b in bytes)
                {
                    WriteByte(sr_addr, b);
                }
            }

            if (read_check && ReadByte(NeuropixelsV2eBeta.STATUS) != (uint)NeuropixelsV2eBetaStatus.SR_OK)
            {
                throw new WorkflowException("Shift register programming check failed.");
            }
        }

        public static BitArray[] GenerateShankBits()
        {
            BitArray[] shankBits =
            {
                new(NeuropixelsV2eBeta.RegistersPerShank, false),
                new(NeuropixelsV2eBeta.RegistersPerShank, false),
                new(NeuropixelsV2eBeta.RegistersPerShank, false),
                new(NeuropixelsV2eBeta.RegistersPerShank, false)
            };

            const int PixelOffset = (NeuropixelsV2eBeta.PixelCount - 1) / 2;
            const int ReferencePixelOffset = 3;
            for (int i = 0; i < NeuropixelsV2eBeta.ChannelCount; i++)
            {
                var baseIndex = i % 2;
                var pixelIndex = i / 2;
                pixelIndex = baseIndex == 0
                    ? pixelIndex + PixelOffset + 2 * ReferencePixelOffset
                    : PixelOffset - pixelIndex + ReferencePixelOffset;

                shankBits[0][pixelIndex] = true;
            }

            return shankBits;
        }

        public static BitArray[] GenerateBaseBits(NeuropixelsV2eBetaChannelReference reference)
        {
            BitArray[] baseBits =
            {
                new(NeuropixelsV2eBeta.ChannelCount * NeuropixelsV2eBeta.BaseBitsPerChannel / 2, false),
                new(NeuropixelsV2eBeta.ChannelCount * NeuropixelsV2eBeta.BaseBitsPerChannel / 2, false)
            };

            var referenceBit = (int)reference;
            for (int i = 0; i < NeuropixelsV2eBeta.ChannelCount; i++)
            {
                var configIndex = i % 2;
                var bitOffset = (382 - i + configIndex) / 2 * NeuropixelsV2eBeta.BaseBitsPerChannel;
                baseBits[configIndex][0] = false; // standby bit
                baseBits[configIndex][bitOffset + referenceBit + 1] = true;
            }

            return baseBits;
        }
    }
}
