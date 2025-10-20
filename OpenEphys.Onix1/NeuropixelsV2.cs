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
        public const int ElectrodePerBlockQuadShank = 48;
        public const int ElectrodePerBlockSingleShank = 32;
        public const int ReferencePixelCount = 4;
        public const int DummyRegisterCount = 4;
        public const int RegistersPerShank = ElectrodePerShank + ReferencePixelCount + DummyRegisterCount;

        internal static BitArray[] GenerateShankBits(NeuropixelsV2ProbeConfiguration probe)
        {
            BitArray[] shankBits;

            const int ShiftRegisterBitExternalElectrode0 = 1285;
            const int ShiftRegisterBitExternalElectrode1 = 2;

            const int ShiftRegisterBitTipElectrode0 = 644;
            const int ShiftRegisterBitTipElectrode1 = 643;

            if (probe.ProbeType == NeuropixelsV2ProbeType.SingleShank)
            {
                shankBits = new BitArray[]
                {
                    new(RegistersPerShank, false)
                };
                const int Shank = 0;

                if (probe.Reference == NeuropixelsV2ShankReference.Tip)
                {
                    shankBits[Shank][ShiftRegisterBitTipElectrode1] = true;
                    shankBits[Shank][ShiftRegisterBitTipElectrode0] = true;
                }
                else if (probe.Reference == NeuropixelsV2ShankReference.External)
                {
                    shankBits[Shank][ShiftRegisterBitExternalElectrode0] = true;
                    shankBits[Shank][ShiftRegisterBitExternalElectrode1] = true;
                }
            }
            else if (probe.ProbeType == NeuropixelsV2ProbeType.QuadShank)
            {
                shankBits = new BitArray[]
                {
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false)
                };

                if (probe.Reference != NeuropixelsV2ShankReference.External && probe.Reference != NeuropixelsV2ShankReference.Ground)
                {
                    var shank = probe.Reference switch
                    {
                        NeuropixelsV2ShankReference.Tip1 => 0,
                        NeuropixelsV2ShankReference.Tip2 => 1,
                        NeuropixelsV2ShankReference.Tip3 => 2,
                        NeuropixelsV2ShankReference.Tip4 => 3,
                        _ => throw new InvalidOperationException($"Invalid reference chosen for {probe.ProbeType} probe.")
                    };

                    // If tip reference is used, activate the tip electrode
                    shankBits[shank][ShiftRegisterBitTipElectrode1] = true;
                    shankBits[shank][ShiftRegisterBitTipElectrode0] = true;
                }
                else if (probe.Reference == NeuropixelsV2ShankReference.External)
                {
                    // TODO: is this the right approach or should only those
                    // connections to external reference on shanks with active
                    // electrodes be activated?

                    // If external electrode is used, activate on each shank
                    shankBits[0][ShiftRegisterBitExternalElectrode1] = true;
                    shankBits[0][ShiftRegisterBitExternalElectrode0] = true;
                    shankBits[1][ShiftRegisterBitExternalElectrode1] = true;
                    shankBits[1][ShiftRegisterBitExternalElectrode0] = true;
                    shankBits[2][ShiftRegisterBitExternalElectrode1] = true;
                    shankBits[2][ShiftRegisterBitExternalElectrode0] = true;
                    shankBits[3][ShiftRegisterBitExternalElectrode1] = true;
                    shankBits[3][ShiftRegisterBitExternalElectrode0] = true;
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown probe configuration type given.");
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

        internal static BitArray[] GenerateBaseBits(NeuropixelsV2ProbeConfiguration probe)
        {
            BitArray[] baseBits =
            {
                new(ChannelCount * BaseBitsPerChannel / 2, false),
                new(ChannelCount * BaseBitsPerChannel / 2, false)
            };

            var referenceBit = probe.ProbeType switch
            {
                NeuropixelsV2ProbeType.SingleShank => probe.Reference switch
                {
                    NeuropixelsV2ShankReference.External => 1,
                    NeuropixelsV2ShankReference.Tip => 2,
                    NeuropixelsV2ShankReference.Ground => 3,
                    _ => throw new InvalidOperationException("Invalid reference selection."),
                },
                NeuropixelsV2ProbeType.QuadShank => probe.Reference switch
                {
                    NeuropixelsV2ShankReference.External => 1,
                    NeuropixelsV2ShankReference.Tip1 => 2,
                    NeuropixelsV2ShankReference.Tip2 => 2,
                    NeuropixelsV2ShankReference.Tip3 => 2,
                    NeuropixelsV2ShankReference.Tip4 => 2,
                    NeuropixelsV2ShankReference.Ground => 3,
                    _ => throw new InvalidOperationException("Invalid reference selection."),
                },
                _ => throw new InvalidOperationException("Invalid probe type given.")
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

