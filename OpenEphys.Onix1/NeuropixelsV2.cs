using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    static class NeuropixelsV2
    {
        public const int ProbeAddress = 0x10;
        public const int FlexEEPROMAddress = 0x50;

        public const int ChannelCount = 384;
        public const int BaseBitsPerChannel = 4;
        public const int ElectrodePerShank = 1280;

        public const int FramesPerSuperFrame = 16;
        public const int AdcsPerProbe = 24;
        public const int TrashWords = 4;
        public const int FrameWords = 36; // TRASH TRASH TRASH 0 ADC0 ADC8 ADC16 0 ADC1 ADC9 ADC17 0 ... ADC7 ADC15 ADC23 0

        // unmanaged register map
        public const uint OP_MODE = 0x00;
        public const uint REC_MODE = 0x01;
        public const uint CAL_MODE = 0x02;
        public const uint ADC_CONFIG = 0x03;
        public const uint TEST_CONFIG1 = 0x04;
        public const uint TEST_CONFIG2 = 0x05;
        public const uint TEST_CONFIG3 = 0x06;
        public const uint TEST_CONFIG4 = 0x07;
        public const uint TEST_CONFIG5 = 0x08;
        public const uint STATUS = 0x09;
        public const uint SUPERSYNC0 = 0x0A;
        public const uint SUPERSYNC1 = 0x0B;
        public const uint SUPERSYNC2 = 0x0C;
        public const uint SUPERSYNC3 = 0x0D;
        public const uint SUPERSYNC4 = 0x0E;
        public const uint SUPERSYNC5 = 0x0F;
        public const uint SUPERSYNC6 = 0x10;
        public const uint SUPERSYNC7 = 0x11;
        public const uint SUPERSYNC8 = 0x12;
        public const uint SUPERSYNC9 = 0x13;
        public const uint SUPERSYNC10 = 0x14;
        public const uint SUPERSYNC11 = 0x15;
        public const uint SR_CHAIN6 = 0x16; // Odd channel base config
        public const uint SR_CHAIN5 = 0x17; // Even channel base config
        public const uint SR_CHAIN4 = 0x18; // Shank 4
        public const uint SR_CHAIN3 = 0x19; // Shank 3
        public const uint SR_CHAIN2 = 0x1A; // Shank 2
        public const uint SR_CHAIN1 = 0x1B; // Shank 1
        public const uint SR_LENGTH2 = 0x1C;
        public const uint SR_LENGTH1 = 0x1D;
        public const uint PROBE_ID = 0x1E;
        public const uint SOFT_RESET = 0x1F;

        internal static BitArray[] GenerateShankBits(NeuropixelsV2ProbeConfiguration probe)
        {
            BitArray[] shankBits = probe.CreateShankBits(probe.Reference);

            const int PixelOffset = (ElectrodePerShank - 1) / 2;
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

        internal static BitArray[] GenerateBaseBits(NeuropixelsV2ProbeConfiguration probe)
        {
            BitArray[] baseBits =
            {
                new(ChannelCount * BaseBitsPerChannel / 2, false),
                new(ChannelCount * BaseBitsPerChannel / 2, false)
            };

            var referenceBit = probe.GetReferenceBit(probe.Reference);

            for (int i = 0; i < ChannelCount; i++)
            {
                var configIndex = i % 2;
                var bitOffset = (382 - i + configIndex) / 2 * BaseBitsPerChannel;
                baseBits[configIndex][bitOffset + 0] = false; // standby bit
                baseBits[configIndex][bitOffset + referenceBit] = true;
            }

            return baseBits;
        }

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2))
            {
            }
        }
    }

    [Flags]
    enum NeuropixelsV2Status : uint
    {
        SR_OK = 1 << 7 // Indicates the SR chain comparison is OK
    }
}

