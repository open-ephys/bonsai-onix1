using System;

namespace OpenEphys.Onix1.Design
{
    interface INeuropixelsV2ProbeInfo
    {
        Array GetReferenceEnumValues();

        Array GetComboBoxChannelPresets();

        Enum CheckForExistingChannelPreset(NeuropixelsV2Electrode[] channelMap);

        NeuropixelsV2Electrode[] GetChannelPreset(Enum channelPreset);
    }
}
