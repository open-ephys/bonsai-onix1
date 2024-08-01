using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2RegisterContext : I2CRegisterContext
    {
        public NeuropixelsV2RegisterContext(I2CRegisterContext other, uint i2cAddress)
            : base(other, i2cAddress)
        {
        }

        public NeuropixelsV2RegisterContext(DeviceContext deviceContext, uint i2cAddress)
            : base(deviceContext, i2cAddress)
        {
        }

        public void WriteConfiguration(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            var baseBits = GenerateBaseBits(probe);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN5, baseBits[0]);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN6, baseBits[1]);

            var shankBits = GenerateShankBits(probe);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN1, shankBits[0]);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN2, shankBits[1]);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN3, shankBits[2]);
            WriteShiftRegister(NeuropixelsV2.SR_CHAIN4, shankBits[3]);

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

        // TODO: NeuropixelsV2.STATUS always fails.
        private void WriteShiftRegister(uint srAddress, BitArray data)
        {
            var bytes = BitArrayToBytes(data);

            //var count = 2;
            //while (count-- > 0)
            //{
                // This allows Base shift registers to get a good STATUS, but does not help shank registers.
                //WriteByte(NeuropixelsV2.SOFT_RESET, 0xFF);
                //WriteByte(NeuropixelsV2.SOFT_RESET, 0x00);

                WriteByte(NeuropixelsV2.SR_LENGTH1, (uint)bytes.Length % 0x100);
                WriteByte(NeuropixelsV2.SR_LENGTH2, (uint)bytes.Length / 0x100);

                foreach (var b in bytes)
                {
                    WriteByte(srAddress, b);
                }
            //}

            //if (ReadByte(NeuropixelsV2.STATUS) != (uint)NeuropixelsV2Status.SR_OK)
            //{
            //    // TODO: This check always fails
            //    throw new InvalidOperationException($"Shift register {srAddress} status check failed.");
            //}
        }

        public static BitArray[] GenerateShankBits(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            BitArray[] shankBits =
            {
                new(NeuropixelsV2.RegistersPerShank, false),
                new(NeuropixelsV2.RegistersPerShank, false),
                new(NeuropixelsV2.RegistersPerShank, false),
                new(NeuropixelsV2.RegistersPerShank, false)
            };

            // If tip reference is used, activate the tip electrodes
            if (probe.Reference != NeuropixelsV2QuadShankReference.External)
            {
                shankBits[(int)probe.Reference - 1][643] = true;
                shankBits[(int)probe.Reference - 1][644] = true;
            }

            const int PixelOffset = (NeuropixelsV2.ElectrodePerShank - 1) / 2;
            const int ReferencePixelOffset = 3;
            foreach (var c in probe.ChannelMap)
            { 
                var baseIndex = c.IntraShankElectrodeIndex % 2;
                var pixelIndex = c.IntraShankElectrodeIndex / 2;
                pixelIndex = baseIndex == 0
                    ? pixelIndex + PixelOffset + 2 * ReferencePixelOffset
                    : PixelOffset - pixelIndex + ReferencePixelOffset;

                shankBits[c.Shank][pixelIndex] = true;
            }

            return shankBits;
        }

        public static BitArray[] GenerateBaseBits(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            BitArray[] baseBits =
            {
                new(NeuropixelsV2.ChannelCount * NeuropixelsV2.BaseBitsPerChannel / 2, false),
                new(NeuropixelsV2.ChannelCount * NeuropixelsV2.BaseBitsPerChannel / 2, false)
            };

            var referenceBit = probe.Reference switch
            {
                NeuropixelsV2QuadShankReference.External => 1,
                NeuropixelsV2QuadShankReference.Tip1 => 2,
                NeuropixelsV2QuadShankReference.Tip2 => 2,
                NeuropixelsV2QuadShankReference.Tip3 => 2,
                NeuropixelsV2QuadShankReference.Tip4 => 2,
                _ => throw new InvalidOperationException("Invalid reference selection."),
            };

            for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            {
                var configIndex = i % 2;
                var bitOffset = (382 - i + configIndex) / 2 * NeuropixelsV2.BaseBitsPerChannel;
                baseBits[configIndex][bitOffset + 0] = false; // standby bit
                baseBits[configIndex][bitOffset + referenceBit ] = true;
            }

            return baseBits;
        }
    }
}
