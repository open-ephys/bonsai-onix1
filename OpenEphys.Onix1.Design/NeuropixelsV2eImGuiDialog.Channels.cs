using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Channel configuration UI, contact info, and the enable/pin/unpin actions that mutate the
    // channel selection.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        protected override void DrawProbeSpecificChannelControls()
        {
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Reference##ref", ref refIdx, ReferenceNames, ReferenceNames.Length))
                configureNode.ProbeConfiguration.Reference = ReferenceValues[refIdx];
            ImGuiControls.Tooltip("Choose which electrode serves as the voltage reference for every recording channel.");

            if (configureNode.ProbeConfiguration.Reference == NeuropixelsV2ReferenceSource.Tip)
                DrawTipReferenceShankCheckboxes();

            var presetNames = presets.Select(p => p.ToString()).ToArray();
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Preset##preset", ref presetIdx, presetNames, presetNames.Length))
                ApplyPreset(presets[presetIdx]);
            ImGuiControls.Tooltip("Apply a ready-made channel selection in a single step. Note: this overrides all current enabled and/or pinned contacts.");
        }

        void DrawTipReferenceShankCheckboxes()
        {
            ImGui.Indent();
            var tipShanks = configureNode.ProbeConfiguration.TipReferenceShanks;
            for (int s = 0; s < probeGroup.ShankCount; s++)
            {
                bool selected = tipShanks.Contains(s);
                if (ImGui.Checkbox($"Shank {s}##tipshank{s}", ref selected))
                {
                    if (selected) tipShanks.Add(s);
                    else tipShanks.Remove(s);
                }
                if (s < probeGroup.ShankCount - 1) ImGui.SameLine();
            }
            ImGuiControls.Tooltip("Choose which shank(s) contribute their tip structure to the reference. Selecting more than one shorts them together, which is sometimes desirable.");
            ImGui.Unindent();
        }

        protected override void DrawProbeSpecificContactInfo(IReadOnlyList<int> sel)
        {
            var shanks = sel.Select(i => probeGroup.GetShank(i)).Distinct();
            ImGuiControls.InfoRow("Shank(s)", shanks.Count() == 0 ? "-" : string.Join(",", shanks));

            var banks = sel.Select(i => probeGroup.GetBank(i)).Distinct();
            ImGuiControls.InfoRow("Banks(s)", shanks.Count() == 0 ? "-" : string.Join(",", banks));
        }

        void ApplyPreset(NeuropixelsV2ChannelPreset preset)
        {
            if (preset == NeuropixelsV2ChannelPreset.None) return;
            probeGroup.SelectPreset(preset);
            RebuildMaps();
            ClearPins();
            selector.ClearSelection();
            HasChanges = true;
        }
    }
}
