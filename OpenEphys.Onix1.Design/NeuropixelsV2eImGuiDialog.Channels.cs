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

        const float ComboboxStartWidthPx = 165f;

        void DrawProbeTypeSection()
        {
            if (isBeta) ImGui.BeginDisabled();
            int newType = probeTypeIdx;
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Probe Type##probetype", ref newType, NeuropixelsV2ProbeTypes.DisplayNames, NeuropixelsV2ProbeTypes.All.Length))
                if (newType != probeTypeIdx) TrySwitchProbeType(newType);
            Tooltip("Choose the physical shank layout of the probe being configured (e.g., single-shank vs. quad-shank).",
                "Not changeable for Beta probes.");
            if (isBeta) ImGui.EndDisabled();
        }

        void DrawChannelSection()
        {
            ImGui.Text("Channel Configuration");
            ImGui.Spacing();

            bool invert = configureNode.ProbeConfiguration.InvertPolarity;
            if (ImGui.Checkbox("Invert polarity##cfg", ref invert))
                configureNode.ProbeConfiguration.InvertPolarity = invert;
            Tooltip("Flip the sign of the recorded voltage, correcting for the inverting amplifier in Neuropixels probes that otherwise makes spikes appear as positive deflections instead of the expected negative ones.");

            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Reference##ref", ref refIdx, refNames, refNames.Length))
            {
                var refValues = probeGroup.GetReferenceEnumValues();
                configureNode.ProbeConfiguration.Reference = (Enum)refValues.GetValue(refIdx);
            }
            Tooltip("Choose which electrode serves as the voltage reference for every recording channel.");

            var presetNames = presets.Select(p => p.ToString()).ToArray();
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Preset##preset", ref presetIdx, presetNames, presetNames.Length))
                ApplyPreset(presets[presetIdx]);
            Tooltip("Apply a ready-made channel selection in a single step. Note: this overrides all current enabled and/or pinned contacts.");

            ImGui.Spacing();

            bool anySelected = selector.SelectedContacts.Any(s => s);

            if (!anySelected) ImGui.BeginDisabled();
            if (ImGui.Button("Enable (E)##enable")) EnableSelectedContacts();
            Tooltip("Enable the selected contacts, wiring each one to an available recording channel.",
                "Select at least one contact first.");
            ImGui.SameLine();
            if (ImGui.Button("Enable & Pin (P)##enablepin")) EnableAndPinSelectedContacts();
            Tooltip("Enable the selected contacts and pin their channel assignment so it survives future preset or probe type changes.",
                "Select at least one contact first.");
            if (!anySelected) ImGui.EndDisabled();

            ImGui.SameLine();

            bool anySelectedPinned = false;
            if (anySelected)
                for (int i = 0; i < selector.SelectedContacts.Length && i < allContacts.Count; i++)
                    if (selector.SelectedContacts[i] &&
                        channelState.ElectrodeToChannel.TryGetValue(i, out int pch) &&
                        channelState.PinnedChannels.Contains(pch))
                    { anySelectedPinned = true; break; }

            if (!anySelectedPinned) ImGui.BeginDisabled();
            if (ImGui.Button("Unpin (Shift+P)##unpinsel")) UnpinSelectedContacts();
            Tooltip("Release the pin on the selected contacts' channels, allowing a future preset or probe type change to reassign them.",
                "Select a pinned contact first.");
            if (!anySelectedPinned) ImGui.EndDisabled();

            ImGui.SameLine();

            bool noPins = channelState.PinnedChannels.Count == 0;
            if (noPins) ImGui.BeginDisabled();
            if (ImGui.Button("Unpin All##unpinall")) ClearPins();
            Tooltip("Release every pin on this probe, allowing all channels to be reassigned by a future preset or probe type change.",
                "No channels are pinned.");
            if (noPins) ImGui.EndDisabled();
        }

        static string IndexList(IReadOnlyList<int> sel)
        {
            if (sel.Count == 1) return sel[0].ToString();
            if (sel.Count < 5) return $"[{string.Join(", ", sel)}]";
            return $"[{sel.Min()}, ..., {sel.Max()}]";
        }

        static string ChannelList(IReadOnlyList<int> sel, MultiplexedContactState channelState)
        {
            static string ChannelFor(int idx, MultiplexedContactState channelState) =>
                channelState.ElectrodeToChannel.TryGetValue(idx, out int ch) ? ch.ToString() : "x";

            if (sel.Count == 1) return ChannelFor(sel[0], channelState);
            if (sel.Count < 5) return $"[{string.Join(", ", sel.Select(i => ChannelFor(i, channelState)))}]";
            return $"[{ChannelFor(sel.Min(), channelState)}, ..., {ChannelFor(sel.Max(), channelState)}]";
        }

        void DrawContactInfo()
        {
            var sel = new List<int>();
            for (int i = 0; i < allContacts.Count; i++)
                if (i < selector.InspectedContacts.Length && selector.InspectedContacts[i])
                    sel.Add(i);

            bool usingHover = false;
            if (sel.Count == 0)
            {
                int hov = selector.HoveredContactIndex;
                if (hov >= 0 && hov < allContacts.Count)
                { sel.Add(hov); usingHover = true; }
            }

            ImGui.Spacing();
            ImGui.Text("Contact Info");
            ImGui.Spacing();

            if (sel.Count == 0) { ImGui.TextDisabled("No contacts selected."); return; }

            ImGui.TextDisabled(usingHover ? "Hovering" : $"{sel.Count} selected");
            ImGui.Spacing();

            InfoRow("Index", IndexList(sel));
            InfoRow("Channel", ChannelList(sel, channelState));

            var shanks = sel.Select(i => probeGroup.GetShank(i)).Distinct();
            InfoRow("Shank(s)", shanks.Count() == 0 ? "-" : string.Join(",", shanks));

            var banks = sel.Select(i => probeGroup.GetBank(i)).Distinct();
            InfoRow("Banks(s)", shanks.Count() == 0 ? "-" : string.Join(",", banks));

            double xMin = double.MaxValue, xMax = double.MinValue;
            double yMin = double.MaxValue, yMax = double.MinValue;
            foreach (int i in sel)
            {
                var c = allContacts[i];
                if (c.PosX < xMin) xMin = c.PosX;
                if (c.PosX > xMax) xMax = c.PosX;
                if (c.PosY < yMin) yMin = c.PosY;
                if (c.PosY > yMax) yMax = c.PosY;
            }
            InfoRow("X", xMin == xMax ? $"{xMin:0} um" : $"{xMin:0} to {xMax:0} um");
            InfoRow("Y", yMin == yMax ? $"{yMin:0} um" : $"{yMin:0} to {yMax:0} um");

            var results = survey.Results;
            var ampData = results?.ActivityAmplitude;
            var rateData = results?.ActivityFireRate;
            var noiseData = results?.Noise;
            var snrData = results?.Snr;
            var snrRateData = results?.SnrTimesFireRate;

            void ShowMetric(string label, float?[] data)
            {
                if (data == null) return;
                if (sel.Count == 1 && sel[0] < data.Length)
                {
                    var v = data[sel[0]];
                    InfoRow(label, v.HasValue ? $"{v.Value:F2}" : "—");
                }
                else
                {
                    var vals = sel.Where(i => i < data.Length && data[i].HasValue)
                                  .Select(i => data[i].Value).ToArray();
                    if (vals.Length > 0) InfoRow(label, $"avg {vals.Average():F2} / max {vals.Max():F2}");
                }
            }

            ShowMetric("Amplitude (uV)", ampData);
            ShowMetric("Noise std dev (uV)", noiseData);
            ShowMetric("SNR", snrData);
            ShowMetric("Firing rate (Hz)", rateData);
            ShowMetric("SNR × Firing rate", snrRateData);

            int pinnedCount = sel.Count(i =>
                channelState.ElectrodeToChannel.TryGetValue(i, out int pch) && channelState.PinnedChannels.Contains(pch));
            InfoRow("Pinned", pinnedCount == 0 ? "false"
                : pinnedCount == sel.Count ? "true"
                : $"{pinnedCount}/{sel.Count}");

            int blockedCount = sel.Count(i => channelState.BlockedContactIndices.Contains(i));
            InfoRow("Blocked", blockedCount == 0 ? "false"
                : blockedCount == sel.Count ? "true"
                : $"{blockedCount}/{sel.Count}");
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

        void EnableSelectedContacts()
        {
            var selectedContactIndices = new List<int>();
            for (int i = 0; i < allContacts.Count; i++)
                if (i < selector.SelectedContacts.Length && selector.SelectedContacts[i])
                    selectedContactIndices.Add(i);
            if (selectedContactIndices.Count == 0) return;
            probeGroup.EnableElectrodes(selectedContactIndices);
            RebuildMaps();
            RecomputeBlockedIndices();
            presetIdx = Array.FindIndex(presets, p => p.ToString() == "None");
            HasChanges = true;
        }

        void EnableAndPinSelectedContacts()
        {
            var candidateIndices = new List<int>();
            for (int i = 0; i < selector.SelectedContacts.Length; i++)
                if (i < allContacts.Count && selector.SelectedContacts[i] && !channelState.BlockedContactIndices.Contains(i))
                    candidateIndices.Add(i);
            if (candidateIndices.Count == 0) return;

            var channelUse = new Dictionary<int, int>();
            foreach (int idx in candidateIndices)
            {
                if (!channelState.ElectrodePotentialChannel.TryGetValue(idx, out int ch)) continue;
                if (channelUse.TryGetValue(ch, out _))
                {
                    MessageBox.Show(
                        "The selection contains contacts that share hardware channels and cannot all be pinned simultaneously.\n" +
                        "Refine the selection so each channel has at most one contact.",
                        "Overlapping Channels", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                channelUse[ch] = idx;
            }

            probeGroup.EnableElectrodes(candidateIndices);
            RebuildMaps();

            foreach (int idx in candidateIndices)
                if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch))
                    channelState.PinnedChannels.Add(ch);

            RecomputeBlockedIndices();
            presetIdx = Array.FindIndex(presets, p => p.ToString() == "None");
            HasChanges = true;
        }

        void UnpinSelectedContacts()
        {
            bool any = false;
            for (int i = 0; i < selector.SelectedContacts.Length && i < allContacts.Count; i++)
            {
                if (!selector.SelectedContacts[i]) continue;
                if (channelState.ElectrodeToChannel.TryGetValue(i, out int ch) && channelState.PinnedChannels.Remove(ch))
                    any = true;
            }
            if (any) RecomputeBlockedIndices();
        }

        void TrySwitchProbeType(int newTypeIdx)
        {
            if (hasChanges)
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
