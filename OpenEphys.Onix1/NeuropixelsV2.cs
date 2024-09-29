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
        public const int ElectrodePerBlock = 48;
        public const int ReferencePixelCount = 4;
        public const int DummyRegisterCount = 4;
        public const int RegistersPerShank = ElectrodePerShank + ReferencePixelCount + DummyRegisterCount;

        internal static BitArray[] GenerateShankBits(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            BitArray[] shankBits =
            {
                new(RegistersPerShank, false),
                new(RegistersPerShank, false),
                new(RegistersPerShank, false),
                new(RegistersPerShank, false)
            };


            if (probe.Reference != NeuropixelsV2QuadShankReference.External)
            {
                // If tip reference is used, activate the tip electrodes
                shankBits[(int)probe.Reference - 1][643] = true;
                shankBits[(int)probe.Reference - 1][644] = true;
            }
            else
            {
                // TODO: is this the right approach or should only those
                // connections to external reference on shanks with active
                // electrodes be activated?

                // If external electrode is used, activate on each shank
                shankBits[0][2] = true;
                shankBits[0][1285] = true;
                shankBits[1][2] = true;
                shankBits[1][1285] = true;
                shankBits[2][2] = true;
                shankBits[2][1285] = true;
                shankBits[3][2] = true;
                shankBits[3][1285] = true;
            }

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

        internal static BitArray[] GenerateBaseBits(NeuropixelsV2QuadShankProbeConfiguration probe)
        {
            BitArray[] baseBits =
            {
                new(ChannelCount * BaseBitsPerChannel / 2, false),
                new(ChannelCount * BaseBitsPerChannel / 2, false)
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

