using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the probe as A or B.
    /// </summary>
    public enum NeuropixelsV2Probe
    {
        /// <summary>
        /// Specifies that this is Probe A.
        /// </summary>
        ProbeA = 0,
        /// <summary>
        /// Specifies that this is Probe B.
        /// </summary>
        ProbeB = 1
    }

    [Flags]
    enum NeuropixelsV2Status : uint
    {
        SR_OK = 1 << 7 // Indicates the SR chain comparison is OK
    }

    static class NeuropixelsV2
    {
        public const int ChannelCount = 384;
        public const int BaseBitsPerChannel = 4;
        public const int ElectrodePerShank = 1280;

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
    }
}

