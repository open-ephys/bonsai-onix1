using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eBetaRegisterContext : I2CRegisterContext
    {
        public NeuropixelsV2eBetaRegisterContext(I2CRegisterContext other, uint i2cAddress)
            : base(other, i2cAddress)
        {
        }

        public NeuropixelsV2eBetaRegisterContext(DeviceContext deviceContext, uint i2cAddress)
            : base(deviceContext, i2cAddress)
        {
        }

        public void WriteConfiguration(NeuropixelsV2ProbeConfiguration probe, NeuropixelsV2ProbeGroup probeGroup)
        {
            var baseBits = NeuropixelsV2.GenerateBaseBits(probe);
            WriteShiftRegister(NeuropixelsV2Beta.SR_CHAIN5, baseBits[0]);
            WriteShiftRegister(NeuropixelsV2Beta.SR_CHAIN6, baseBits[1]);

            var shankBits = NeuropixelsV2.GenerateShankBits(probe, probeGroup);

            if (shankBits.Length < 1 || shankBits.Length > ShankChains.Length)
            {
                throw new InvalidOperationException(
                    $"Attempted to write an invalid probe configuration: {shankBits.Length} shank(s), but " +
                    $"the register map only defines {ShankChains.Length} shank shift-register chains.");
            }

            for (int shank = 0; shank < shankBits.Length; shank++)
                WriteShiftRegister(ShankChains[shank], shankBits[shank]);
        }

        static readonly uint[] ShankChains =
        {
            NeuropixelsV2Beta.SR_CHAIN1,
            NeuropixelsV2Beta.SR_CHAIN2,
            NeuropixelsV2Beta.SR_CHAIN3,
            NeuropixelsV2Beta.SR_CHAIN4,
        };

        void WriteShiftRegister(uint srAddress, BitArray data)
        {
            var bytes = BitHelper.ToBitReversedBytes(data);

            var count = 2;
            while (count-- > 0)
            {
                // This allows Base shift registers to get a good STATUS, but does not help shank registers.
                WriteByte(NeuropixelsV2Beta.SOFT_RESET, 0xFF);
                WriteByte(NeuropixelsV2Beta.SOFT_RESET, 0x00);

                WriteByte(NeuropixelsV2Beta.SR_LENGTH1, (uint)(bytes.Length % 0x100));
                WriteByte(NeuropixelsV2Beta.SR_LENGTH2, (uint)(bytes.Length / 0x100));

                foreach (var b in bytes)
                {
                    WriteByte(srAddress, b);
                }
            }

            if (ReadByte(NeuropixelsV2.STATUS) != (uint)NeuropixelsV2Status.SR_OK)
            {
                if (srAddress == NeuropixelsV2Beta.SR_CHAIN5 || srAddress == NeuropixelsV2Beta.SR_CHAIN6)
                {
                    ContextHelper.Validate(ValidationLevel.Permissive, new InvalidOperationException(
                        $"Shift register 0x{srAddress:X2} status check failed."));
                }
                else
                {
                    ContextHelper.Validate(ValidationLevel.Normal, new InvalidOperationException(
                        $"Shift register 0x{srAddress:X2} status check failed. {ShankName(srAddress)} may be damaged."));
                }
            }
        }

        static string ShankName(uint shiftRegisterAddress) => shiftRegisterAddress switch
        {
            NeuropixelsV2Beta.SR_CHAIN1 => "Shank 0",
            NeuropixelsV2Beta.SR_CHAIN2 => "Shank 1",
            NeuropixelsV2Beta.SR_CHAIN3 => "Shank 2",
            NeuropixelsV2Beta.SR_CHAIN4 => "Shank 3",
            _ => throw new InvalidOperationException("Shift register address is not valid."),
        };
    }
}
