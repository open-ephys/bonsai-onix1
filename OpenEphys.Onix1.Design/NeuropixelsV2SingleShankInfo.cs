using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix1.Design
{
    internal class NeuropixelsV2SingleShankInfo : INeuropixelsV2ProbeInfo
    {
        const int BankDStartIndex = 896;

        enum SingleShankChannelPreset
        {
            BankA,
            BankB,
            BankC,
            BankD,
            None
        }

        public IEnumerable<NeuropixelsV2Electrode> Electrodes { get; init; }

        public NeuropixelsV2SingleShankInfo(NeuropixelsV2SingleShankProbeConfiguration probeConfiguration)
        {
            Electrodes = probeConfiguration.ProbeGroup.ToElectrodes();
        }

        public Array GetReferenceEnumValues()
        {
            return Enum.GetValues(typeof(NeuropixelsV2SingleShankReference));
        }

        public Array GetComboBoxChannelPresets()
        {
            return Enum.GetValues(typeof(SingleShankChannelPreset));
        }

        public Enum CheckForExistingChannelPreset(NeuropixelsV2Electrode[] channelMap)
        {
            if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A))
            {
                return SingleShankChannelPreset.BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B))
            {
                return SingleShankChannelPreset.BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C))
            {
                return SingleShankChannelPreset.BankC;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.D ||
                                        (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)))
            {
                return SingleShankChannelPreset.BankD;
            }
            else
            {
                return SingleShankChannelPreset.None;
            }
        }

        public NeuropixelsV2Electrode[] GetChannelPreset(Enum channelPreset)
        {
            var preset = (SingleShankChannelPreset)channelPreset;

            return preset switch
            {
                SingleShankChannelPreset.BankA => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A).ToArray(),
                SingleShankChannelPreset.BankB => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B).ToArray(),
                SingleShankChannelPreset.BankC => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C).ToArray(),
                SingleShankChannelPreset.BankD => Electrodes.Where(e => e.Bank == NeuropixelsV2Bank.D || (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)).ToArray(),
                SingleShankChannelPreset.None => Array.Empty<NeuropixelsV2Electrode>(),
                _ => throw new InvalidEnumArgumentException($"Unknown value of {nameof(SingleShankChannelPreset)}: {channelPreset}")
            };
        }
    }
}
