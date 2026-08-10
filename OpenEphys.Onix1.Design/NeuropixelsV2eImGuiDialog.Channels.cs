using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Probe-type selection, channel configuration UI, contact info, and the enable/pin/unpin actions that
    // mutate the channel selection.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void DrawProbeTypeSection()
        {
            if (isBeta) ImGui.BeginDisabled();
            int newType = probeTypeIdx;
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Probe Type##probetype", ref newType, NeuropixelsV2ProbeTypes.DisplayNames, NeuropixelsV2ProbeTypes.All.Length))
                if (newType != probeTypeIdx) TrySwitchProbeType(newType);
            ImGuiControls.Tooltip("Choose the physical shank layout of the probe being configured (e.g., single-shank vs. quad-shank).",
                "Not changeable for Beta probes.");
            if (isBeta) ImGui.EndDisabled();
        }

        protected override void DrawProbeSpecificChannelControls()
        {
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Reference##ref", ref refIdx, refNames, refNames.Length))
            {
                var refValues = probeGroup.GetReferenceEnumValues();
                configureNode.ProbeConfiguration.Reference = (Enum)refValues.GetValue(refIdx);
            }
            ImGuiControls.Tooltip("Choose which electrode serves as the voltage reference for every recording channel.");

            var presetNames = presets.Select(p => p.ToString()).ToArray();
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Preset##preset", ref presetIdx, presetNames, presetNames.Length))
                ApplyPreset(presets[presetIdx]);
            ImGuiControls.Tooltip("Apply a ready-made channel selection in a single step. Note: this overrides all current enabled and/or pinned contacts.");
        }

        protected override void DrawProbeSpecificContactInfo(IReadOnlyList<int> sel)
        {
            var shanks = sel.Select(i => probeGroup.GetShank(i)).Distinct();
            ImGuiControls.InfoRow("Shank(s)", shanks.Count() == 0 ? "-" : string.Join(",", shanks));

            var banks = sel.Select(i => probeGroup.GetBank(i)).Distinct();
            ImGuiControls.InfoRow("Banks(s)", shanks.Count() == 0 ? "-" : string.Join(",", banks));
        }

        void ApplyPreset(Enum preset)
        {
            if (preset.ToString() == "None") return;
            probeGroup.SelectPreset(preset);
            RebuildMaps();
            ClearPins();
            selector.ClearSelection();
            HasChanges = true;
        }

        void TrySwitchProbeType(int newTypeIdx)
        {
            if (HasChanges)
            {
                var r = MessageBox.Show($"Changing probe type will discard unsaved {probeName} changes. Continue?",
                    $"{probeName}: Change Probe Type", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No)
                { probeTypeIdx = Array.FindIndex(NeuropixelsV2ProbeTypes.All, e => e.MatchesProbeGroup(probeGroup)); return; }
            }
            var entry = NeuropixelsV2ProbeTypes.All[newTypeIdx];
            var newCfg = entry.CreateConfiguration();
            newCfg.InvertPolarity = configureNode.ProbeConfiguration.InvertPolarity;
            newCfg.GainCalibrationFileName = configureNode.ProbeConfiguration.GainCalibrationFileName;
            newCfg.ProbeInterfaceFileName = configureNode.ProbeConfiguration.ProbeInterfaceFileName;
            probeGroup = entry.CreateGroup();
            probeTypeIdx = newTypeIdx;
            configureNode.ProbeConfiguration = newCfg;
            ClearPins();
            SetupSelectorCallbacks();
            RefreshProbeState();
            HasChanges = true;
        }
    }
}
