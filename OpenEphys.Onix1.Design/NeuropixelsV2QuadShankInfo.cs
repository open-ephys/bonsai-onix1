using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix1.Design
{
    internal class NeuropixelsV2QuadShankInfo : INeuropixelsV2ProbeInfo
    {
        const int BankDStartIndex = 896;

        enum QuadShankChannelPreset
        {
            Shank0BankA,
            Shank0BankB,
            Shank0BankC,
            Shank0BankD,
            Shank1BankA,
            Shank1BankB,
            Shank1BankC,
            Shank1BankD,
            Shank2BankA,
            Shank2BankB,
            Shank2BankC,
            Shank2BankD,
            Shank3BankA,
            Shank3BankB,
            Shank3BankC,
            Shank3BankD,
            AllShanks0_95,
            AllShanks96_191,
            AllShanks192_287,
            AllShanks288_383,
            AllShanks384_479,
            AllShanks480_575,
            AllShanks576_671,
            AllShanks672_767,
            AllShanks768_863,
            AllShanks864_959,
            AllShanks960_1055,
            AllShanks1056_1151,
            AllShanks1152_1247,
            None
        }

        IEnumerable<NeuropixelsV2Electrode> Electrodes { get; init; }

        public NeuropixelsV2QuadShankInfo(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Electrodes = probeConfiguration.ProbeGroup.ToElectrodes();
        }

        public Array GetReferenceEnumValues()
        {
            return Enum.GetValues(typeof(NeuropixelsV2QuadShankReference));
        }

        public Array GetComboBoxChannelPresets()
        {
            return Enum.GetValues(typeof(QuadShankChannelPreset));
        }

        public Enum CheckForExistingChannelPreset(NeuropixelsV2Electrode[] channelMap)
        {
            static bool CheckShankBank(NeuropixelsV2Electrode[] channelMap, int shank, NeuropixelsV2Bank bank, out QuadShankChannelPreset preset)
            {
                preset = (shank, bank) switch
                {
                    (0, NeuropixelsV2Bank.A) => QuadShankChannelPreset.Shank0BankA,
                    (0, NeuropixelsV2Bank.B) => QuadShankChannelPreset.Shank0BankB,
                    (0, NeuropixelsV2Bank.C) => QuadShankChannelPreset.Shank0BankC,
                    (1, NeuropixelsV2Bank.A) => QuadShankChannelPreset.Shank1BankA,
                    (1, NeuropixelsV2Bank.B) => QuadShankChannelPreset.Shank1BankB,
                    (1, NeuropixelsV2Bank.C) => QuadShankChannelPreset.Shank1BankC,
                    (2, NeuropixelsV2Bank.A) => QuadShankChannelPreset.Shank2BankA,
                    (2, NeuropixelsV2Bank.B) => QuadShankChannelPreset.Shank2BankB,
                    (2, NeuropixelsV2Bank.C) => QuadShankChannelPreset.Shank2BankC,
                    (3, NeuropixelsV2Bank.A) => QuadShankChannelPreset.Shank3BankA,
                    (3, NeuropixelsV2Bank.B) => QuadShankChannelPreset.Shank3BankB,
                    (3, NeuropixelsV2Bank.C) => QuadShankChannelPreset.Shank3BankC,
                    _ => QuadShankChannelPreset.None
                };

                return channelMap.All(e => e.Bank == bank && e.Shank == shank);
            }

            static bool CheckShankBankD(NeuropixelsV2Electrode[] channelMap, int shank, out QuadShankChannelPreset preset)
            {
                preset = shank switch
                {
                    0 => QuadShankChannelPreset.Shank0BankD,
                    1 => QuadShankChannelPreset.Shank1BankD,
                    2 => QuadShankChannelPreset.Shank2BankD,
                    3 => QuadShankChannelPreset.Shank3BankD,
                    _ => QuadShankChannelPreset.None
                };

                return channelMap.All(e => (e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.IntraShankElectrodeIndex >= BankDStartIndex)) && e.Shank == shank);
            }

            for (int shank = 0; shank <= 3; shank++)
            {
                if (CheckShankBank(channelMap, shank, NeuropixelsV2Bank.A, out var preset))
                    return preset;

                if (CheckShankBank(channelMap, shank, NeuropixelsV2Bank.B, out preset))
                    return preset;

                if (CheckShankBank(channelMap, shank, NeuropixelsV2Bank.C, out preset))
                    return preset;

                if (CheckShankBankD(channelMap, shank, out preset))
                    return preset;
            }

            var ranges = new[]
            {
                (0, 95, QuadShankChannelPreset.AllShanks0_95),
                (192, 287, QuadShankChannelPreset.AllShanks192_287),
                (288, 383, QuadShankChannelPreset.AllShanks288_383),
                (394, 479, QuadShankChannelPreset.AllShanks384_479),
                (480, 575, QuadShankChannelPreset.AllShanks480_575),
                (576, 671, QuadShankChannelPreset.AllShanks576_671),
                (672, 767, QuadShankChannelPreset.AllShanks672_767),
                (768, 863, QuadShankChannelPreset.AllShanks768_863),
                (864, 959, QuadShankChannelPreset.AllShanks864_959),
                (960, 1055, QuadShankChannelPreset.AllShanks960_1055),
                (1056, 1151, QuadShankChannelPreset.AllShanks1056_1151),
                (1152, 1247, QuadShankChannelPreset.AllShanks1152_1247)
            };

            static bool CheckAllShanksRange(NeuropixelsV2Electrode[] channelMap, int start, int end)
            {
                return channelMap.All(e => e.Shank >= 0 && e.Shank <= 3 &&
                                           e.IntraShankElectrodeIndex >= start &&
                                           e.IntraShankElectrodeIndex <= end);
            }

            foreach (var (start, end, presetValue) in ranges)
            {
                if (CheckAllShanksRange(channelMap, start, end))
                    return presetValue;
            }

            return QuadShankChannelPreset.None;
        }

        public NeuropixelsV2Electrode[] GetChannelPreset(Enum channelPreset)
        {
            var preset = (QuadShankChannelPreset)channelPreset;

            static NeuropixelsV2Electrode[] GetAllShanks(IEnumerable<NeuropixelsV2Electrode> electrodes, int startIndex, int endIndex)
            {
                return electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= startIndex && e.IntraShankElectrodeIndex <= endIndex)
                            || (e.Shank == 1 && e.IntraShankElectrodeIndex >= startIndex && e.IntraShankElectrodeIndex <= endIndex)
                            || (e.Shank == 2 && e.IntraShankElectrodeIndex >= startIndex && e.IntraShankElectrodeIndex <= endIndex)
                            || (e.Shank == 3 && e.IntraShankElectrodeIndex >= startIndex && e.IntraShankElectrodeIndex <= endIndex)).ToArray();
            };

            return preset switch
            {
                QuadShankChannelPreset.Shank0BankA => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A && e.Shank == 0).ToArray(),
                QuadShankChannelPreset.Shank0BankB => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B && e.Shank == 0).ToArray(),
                QuadShankChannelPreset.Shank0BankC => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C && e.Shank == 0).ToArray(),
                QuadShankChannelPreset.Shank0BankD => Electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) && e.Shank == 0).ToArray(),
                QuadShankChannelPreset.Shank1BankA => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A && e.Shank == 1).ToArray(),
                QuadShankChannelPreset.Shank1BankB => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B && e.Shank == 1).ToArray(),
                QuadShankChannelPreset.Shank1BankC => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C && e.Shank == 1).ToArray(),
                QuadShankChannelPreset.Shank1BankD => Electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) && e.Shank == 1).ToArray(),
                QuadShankChannelPreset.Shank2BankA => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A && e.Shank == 2).ToArray(),
                QuadShankChannelPreset.Shank2BankB => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B && e.Shank == 2).ToArray(),
                QuadShankChannelPreset.Shank2BankC => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C && e.Shank == 2).ToArray(),
                QuadShankChannelPreset.Shank2BankD => Electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) && e.Shank == 2).ToArray(),
                QuadShankChannelPreset.Shank3BankA => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A && e.Shank == 3).ToArray(),
                QuadShankChannelPreset.Shank3BankB => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B && e.Shank == 3).ToArray(),
                QuadShankChannelPreset.Shank3BankC => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C && e.Shank == 3).ToArray(),
                QuadShankChannelPreset.Shank3BankD => Electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) && e.Shank == 3).ToArray(),
                QuadShankChannelPreset.AllShanks0_95 => GetAllShanks(Electrodes, 0, 95),
                QuadShankChannelPreset.AllShanks96_191 => GetAllShanks(Electrodes, 96, 191),
                QuadShankChannelPreset.AllShanks192_287 => GetAllShanks(Electrodes, 192, 287),
                QuadShankChannelPreset.AllShanks288_383 => GetAllShanks(Electrodes, 288, 383),
                QuadShankChannelPreset.AllShanks384_479 => GetAllShanks(Electrodes, 384, 479),
                QuadShankChannelPreset.AllShanks480_575 => GetAllShanks(Electrodes, 480, 575),
                QuadShankChannelPreset.AllShanks576_671 => GetAllShanks(Electrodes, 576, 671),
                QuadShankChannelPreset.AllShanks672_767 => GetAllShanks(Electrodes, 672, 767),
                QuadShankChannelPreset.AllShanks768_863 => GetAllShanks(Electrodes, 768, 863),
                QuadShankChannelPreset.AllShanks864_959 => GetAllShanks(Electrodes, 864, 959),
                QuadShankChannelPreset.AllShanks960_1055 => GetAllShanks(Electrodes, 960, 1055),
                QuadShankChannelPreset.AllShanks1056_1151 => GetAllShanks(Electrodes, 1056, 1151),
                QuadShankChannelPreset.AllShanks1152_1247 => GetAllShanks(Electrodes, 1152, 1247),
                QuadShankChannelPreset.None => Array.Empty<NeuropixelsV2Electrode>(),
                _ => throw new InvalidEnumArgumentException($"Unknown value of {nameof(QuadShankChannelPreset)}: {channelPreset}")
            };
        }
    }
}
