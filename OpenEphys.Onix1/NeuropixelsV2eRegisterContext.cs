using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    class NeuropixelsV2eRegisterContext : I2CRegisterContext
    {
        public NeuropixelsV2eRegisterContext(I2CRegisterContext other, uint i2cAddress)
            : base(other, i2cAddress)
        {
        }

        public NeuropixelsV2eRegisterContext(DeviceContext deviceContext, uint i2cAddress)
            : base(deviceContext, i2cAddress)
        {
        }

        public void WriteConfiguration(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            var baseBits = NeuropixelsV2.GenerateBaseBits(probe);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN5, baseBits[0]);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN6, baseBits[1]);

            var shankBits = NeuropixelsV2.GenerateShankBits(probe);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN1, shankBits[0]);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN2, shankBits[1]);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN3, shankBits[2]);
            WriteShiftRegister(NeuropixelsV2e.SR_CHAIN4, shankBits[3]);
        }

        void WriteShiftRegister(uint srAddress, BitArray data)
        {
            var bytes = BitHelper.ToBitReversedBytes(data);

            var count = 2;
            while (count-- > 0)
            {
                // This allows Base shift registers to get a good STATUS
                WriteByte(NeuropixelsV2e.SOFT_RESET, 0xFF);
                WriteByte(NeuropixelsV2e.SOFT_RESET, 0x00);

                WriteByte(NeuropixelsV2e.SR_LENGTH1, (uint)(bytes.Length % 0x100));
                WriteByte(NeuropixelsV2e.SR_LENGTH2, (uint)(bytes.Length / 0x100));

                foreach (var b in bytes)
                {
                    WriteByte(srAddress, b);
                }
            }
;
            if (ReadByte(NeuropixelsV2e.STATUS) != (uint)NeuropixelsV2Status.SR_OK)
            {
                Console.Error.WriteLine($"Warning: shift register {srAddress:X} status check failed. " +
                    $"{ShankName(srAddress)} may be damaged.");
            }
        }

        static string ShankName(uint shiftRegisterAddress) => shiftRegisterAddress switch
        {
            NeuropixelsV2e.SR_CHAIN1 => "Shank 1",
            NeuropixelsV2e.SR_CHAIN2 => "Shank 2",
            NeuropixelsV2e.SR_CHAIN3 => "Shank 3",
            NeuropixelsV2e.SR_CHAIN4 => "Shank 4",
            _ => throw new InvalidOperationException("Shift register address is not valid."),
        };
    }
}
