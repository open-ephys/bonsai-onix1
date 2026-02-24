using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1.Design
{
    interface INeuropixelsV2ProbeInfo
    {
        public IEnumerable<NeuropixelsV2Electrode> Electrodes { get; }

        Array GetReferenceEnumValues();

        Array GetComboBoxChannelPresets();

        Enum CheckForExistingChannelPreset(NeuropixelsV2Electrode[] channelMap);

        NeuropixelsV2Electrode[] GetChannelPreset(Enum channelPreset);
    }
}
