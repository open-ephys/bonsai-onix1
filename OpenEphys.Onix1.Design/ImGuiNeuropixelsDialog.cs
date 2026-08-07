using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Shared scaffold for ImGui-rendered Neuropixels probe-configuration dialogs: channel/pin
    /// bookkeeping, probe-interface file load/save, the enable/pin/unpin interaction, contact info
    /// display, and keyboard shortcuts common to every Neuropixels probe family. Subclasses supply
    /// probe-specific data access and UI through the abstract/virtual members below.
    /// </summary>
    internal abstract class ImGuiNeuropixelsDialog : ImGuiProbePanel
    {
        protected readonly string probeName;
        protected IReadOnlyList<Contact> allContacts = Array.Empty<Contact>();
        protected readonly MultiplexedContactState channelState = new();
        protected bool hasChanges;
        protected readonly byte[] probeFileBuf = new byte[512];

        protected const uint ColorContactEnabled = ImGuiPalette.AmberGold;
        protected const uint ColorContactPinned = ImGuiPalette.NeonPink;
        protected const uint ColorContactDisabled = ImGuiPalette.Grey0x4D;
        protected const float ComboboxStartWidthPx = 165f;

        /// <inheritdoc/>
        public override bool HasChanges
        {
            get => hasChanges;
            protected set => hasChanges = value;
        }

        protected ImGuiNeuropixelsDialog(string probeName)
        {
            this.probeName = probeName;
        }

        #region Seams implemented by probe-specific subclasses

        /// <summary>
        /// The current probe group, upcast to the base ProbeInterface.NET type.
        /// </summary>
        protected abstract SingleProbeGroup ProbeInterfaceGroup { get; }

        /// <summary>
        /// The current probe group's multiplexed channel-mapping view.
        /// </summary>
        protected abstract IMultiplexedProbeGroup MultiplexedProbeGroup { get; }

        /// <summary>
        /// The probe configuration's ProbeInterface.NET-facing surface.
        /// </summary>
        protected abstract IProbeInterfaceConfiguration ProbeConfigurationBase { get; }

        /// <summary>
        /// The calibration file used as a fallback directory when browsing for a probe-interface file.
        /// </summary>
        protected abstract string PrimaryCalibrationFileName { get; }

        /// <summary>
        /// Whether the recorded voltage sign is inverted.
        /// </summary>
        protected abstract bool InvertPolarity { get; set; }

        /// <summary>
        /// Wires the given contacts to available recording channels.
        /// </summary>
        protected abstract void EnableElectrodes(IEnumerable<int> contactIndices);

        /// <summary>
        /// Replaces the current probe group by deserializing a probe-interface file at <paramref name="path"/>.
        /// </summary>
        protected abstract void ReplaceProbeGroupFromFile(string path);

        /// <summary>
        /// Draws whatever calibration-file input(s) this probe family needs.
        /// </summary>
        protected abstract void DrawCalibrationFileSection();

        /// <summary>
        /// Draws probe-specific channel controls (e.g. reference/preset/gain), between the
        /// invert-polarity checkbox and the enable/pin buttons.
        /// </summary>
        protected abstract void DrawProbeSpecificChannelControls();

        /// <summary>
        /// Draws probe-specific contact-info rows (e.g. shank/bank) between the Channel and X rows.
        /// </summary>
        protected abstract void DrawProbeSpecificContactInfo(IReadOnlyList<int> sel);

        /// <summary>
        /// Called after <see cref="RefreshProbeState"/> finishes its generic work.
        /// </summary>
        protected virtual void OnProbeGroupRefreshed() { }

        /// <summary>
        /// Called just before the probe group is serialized to disk.
        /// </summary>
        protected virtual void OnSavingProbeGroup() { }

        /// <summary>
        /// Called after contacts are enabled via Enable/Enable &amp; Pin.
        /// </summary>
        protected virtual void OnContactsEnabled() { }

        /// <summary>
        /// Draws any probe-specific per-contact metrics in the Contact Info panel.
        /// </summary>
        protected virtual void DrawContactMetrics(IReadOnlyList<int> sel) { }

        /// <summary>
        /// Handles probe-specific keyboard shortcuts beyond Ctrl+S/E/P/Shift+P.
        /// </summary>
        protected virtual void HandleProbeSpecificShortcuts(bool shift) { }

        #endregion

        #region Lifecycle

        protected void RefreshProbeState()
        {
            allContacts = ProbeInterfaceGroup.Probe.Contacts;

            var contacts = allContacts
                .Select((c, i) => new ProbeContact(i, new Vector2((float)c.PosX, (float)c.PosY), ContactSizeUm(c)))
                .ToList();
            selector.Refresh(ProbeInterfaceGroup, contacts);

            RebuildMaps();
            BuildPotentialChannelMap();
            ClearPins();
            RestorePinnedState();
            OnProbeGroupRefreshed();
        }

        #endregion

        #region Channel bookkeeping (wrappers over MultiplexedContactState)

        protected void RebuildMaps() => channelState.RebuildActiveMap(ProbeInterfaceGroup.ChannelMap);

        protected void BuildPotentialChannelMap() => channelState.RebuildPotentialMap(allContacts.Count, MultiplexedProbeGroup);

        protected void RecomputeBlockedIndices() => channelState.RecomputeBlocked(allContacts.Count);

        protected void ClearPins() => channelState.ClearPins();

        #endregion

        #region Channel section (invert-polarity + enable/pin/unpin)

        protected void DrawChannelSection()
        {
            ImGui.Text("Channel Configuration");
            ImGui.Spacing();

            DrawInvertPolarityCheckbox();
            DrawProbeSpecificChannelControls();

            ImGui.Spacing();

            DrawEnablePinButtons();
        }

        protected void DrawInvertPolarityCheckbox()
        {
            bool invert = InvertPolarity;
            if (ImGui.Checkbox("Invert polarity##cfg", ref invert))
                InvertPolarity = invert;
            ImGuiControls.Tooltip("Flip the sign of the recorded voltage, correcting for the inverting amplifier in Neuropixels probes that otherwise makes spikes appear as positive deflections instead of the expected negative ones.");
        }

        protected void DrawEnablePinButtons()
        {
            bool anySelected = selector.SelectedContacts.Any(s => s);

            if (!anySelected) ImGui.BeginDisabled();
            if (ImGui.Button("Enable (E)##enable")) EnableSelectedContacts();
            ImGuiControls.Tooltip("Enable the selected contacts, wiring each one to an available recording channel.",
                "Select at least one contact first.");
            ImGui.SameLine();
            if (ImGui.Button("Enable & Pin (P)##enablepin")) EnableAndPinSelectedContacts();
            ImGuiControls.Tooltip("Enable the selected contacts and pin their channel assignment so it survives future preset or probe type changes.",
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
            ImGuiControls.Tooltip("Release the pin on the selected contacts' channels, allowing a future preset or probe type change to reassign them.",
                "Select a pinned contact first.");
            if (!anySelectedPinned) ImGui.EndDisabled();

            ImGui.SameLine();

            bool noPins = channelState.PinnedChannels.Count == 0;
            if (noPins) ImGui.BeginDisabled();
            if (ImGui.Button("Unpin All##unpinall")) ClearPins();
            ImGuiControls.Tooltip("Release every pin on this probe, allowing all channels to be reassigned by a future preset or probe type change.",
                "No channels are pinned.");
            if (noPins) ImGui.EndDisabled();
        }

        protected void EnableSelectedContacts()
        {
            var selectedContactIndices = new List<int>();
            for (int i = 0; i < allContacts.Count; i++)
                if (i < selector.SelectedContacts.Length && selector.SelectedContacts[i])
                    selectedContactIndices.Add(i);
            if (selectedContactIndices.Count == 0) return;
            EnableElectrodes(selectedContactIndices);
            RebuildMaps();
            RecomputeBlockedIndices();
            OnContactsEnabled();
            HasChanges = true;
        }

        protected void EnableAndPinSelectedContacts()
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

            EnableElectrodes(candidateIndices);
            RebuildMaps();

            foreach (int idx in candidateIndices)
                if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch))
                    channelState.PinnedChannels.Add(ch);

            RecomputeBlockedIndices();
            OnContactsEnabled();
            HasChanges = true;
        }

        protected void UnpinSelectedContacts()
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

        /// <summary>Fallback contact-color logic (enabled/pinned/disabled) shared by every subclass's selector wiring.</summary>
        protected uint DefaultContactFillColor(int idx)
        {
            if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch) && channelState.PinnedChannels.Contains(ch))
                return ColorContactPinned;
            return channelState.EnabledContactIndices.Contains(idx) ? ColorContactEnabled : ColorContactDisabled;
        }

        protected bool DefaultIsBlocked(int idx) => channelState.BlockedContactIndices.Contains(idx);

        #endregion

        #region File section (calibration + probe-interface file I/O)

        protected void DrawFileSection()
        {
            DrawCalibrationFileSection();
            ImGui.Spacing();
            DrawProbeInterfaceFileSection();
        }

        /// <summary>Width for a file-path InputText that leaves room for a same-line "Open..." button.</summary>
        protected static float ComputeFileRowInputWidth()
        {
            float openButtonWidth = ImGui.CalcTextSize("Open...", true).X + ImGui.GetStyle().FramePadding.X * 2f;
            return ImGui.GetContentRegionAvail().X - openButtonWidth - ImGui.GetStyle().ItemSpacing.X;
        }

        protected void DrawProbeInterfaceFileSection()
        {
            float fileTargetW = ComputeFileRowInputWidth();

            ImGui.Text("Probe Interface File");
            ImGui.Spacing();

            string pf = ProbeConfigurationBase.ProbeInterfaceFileName ?? "";
            ImGuiControls.WriteString(probeFileBuf, pf);
            ImGui.SetNextItemWidth(fileTargetW);
            unsafe
            {
                fixed (byte* p = probeFileBuf)
                    if (ImGui.InputText("##probefile", p, (nuint)probeFileBuf.Length))
                        ProbeConfigurationBase.ProbeInterfaceFileName = ImGuiControls.ReadBuffer(probeFileBuf);
            }
            if (!string.IsNullOrEmpty(ProbeConfigurationBase.ProbeInterfaceFileName))
            {
                ImGuiControls.Tooltip(ProbeConfigurationBase.ProbeInterfaceFileName);
            }

            ImGui.SameLine();
            if (ImGui.Button("Open...##probefile"))
            {
                var calFile = PrimaryCalibrationFileName;
                using var ofd = new OpenFileDialog
                {
                    Title  = "Load Probe Interface File",
                    Filter = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                };

                if (!string.IsNullOrEmpty(pf) && File.Exists(pf))
                    ofd.InitialDirectory = Path.GetDirectoryName(pf);
                else if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                    ofd.InitialDirectory = Path.GetDirectoryName(calFile);

                if (ofd.ShowDialog() == DialogResult.OK)
                    OnProbeFileNameChanged(ofd.FileName);
            }
            ImGuiControls.Tooltip("Open a file browser to load an existing ProbeInterface file that specifies the channel map, contact state, and previous survey results.");

            bool canSave = !string.IsNullOrEmpty(ProbeConfigurationBase.ProbeInterfaceFileName);
            string saveLabel = hasChanges ? "Save Changes*##save" : "Save##save";
            if (!canSave) ImGui.BeginDisabled();
            if (ImGui.Button(saveLabel)) SaveProbeGroup();
            ImGuiControls.Tooltip("Save the current channel map, contact state, and survey data to the specified ProbeInterface file.",
                "Choose a file with Open or Save As... first.");
            if (!canSave) ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Save As...##save"))
                SaveProbeGroupAs();
            ImGuiControls.Tooltip("Choose a file path and save the current channel map, contact state, and survey data to a new ProbeInterface file.");
        }

        protected void SaveProbeGroup()
        {
            var path = ProbeConfigurationBase.ProbeInterfaceFileName;
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                WritePinnedState();
                OnSavingProbeGroup();
                File.WriteAllText(path, JsonConvert.SerializeObject(ProbeInterfaceGroup, Formatting.Indented));
                Log($"Probeinterface file saved to {path}");
                HasChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void SaveProbeGroupAs()
        {
            string currentPath = ProbeConfigurationBase.ProbeInterfaceFileName ?? "";
            string calFile = PrimaryCalibrationFileName ?? "";
            using var sfd = new SaveFileDialog
            {
                Title           = "Save Probe Interface File As",
                Filter          = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                DefaultExt      = "json",
                FileName        = Path.GetFileName(currentPath.Length > 0 ? currentPath : GetProbeSerialNumber() + ".json"),
                OverwritePrompt = true,
            };

            if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                sfd.InitialDirectory = Path.GetDirectoryName(currentPath);
            else if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                sfd.InitialDirectory = Path.GetDirectoryName(calFile);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ProbeConfigurationBase.ProbeInterfaceFileName = sfd.FileName;
                SaveProbeGroup();
            }
        }

        protected void WritePinnedState()
        {
            var pinned = allContacts
                .Select((c, i) => channelState.ElectrodeToChannel.TryGetValue(i, out int ch) && channelState.PinnedChannels.Contains(ch))
                .ToArray();
            ProbeInterfaceGroup.Probe.SetContactAnnotation<bool>("pinned", pinned);
        }

        protected void RestorePinnedState()
        {
            var pinned = ProbeInterfaceGroup.Probe.GetContactAnnotation<bool>("pinned");
            if (pinned == null) return;
            for (int i = 0; i < allContacts.Count && i < pinned.Length; i++)
                if (pinned[i] && channelState.ElectrodeToChannel.TryGetValue(i, out int ch))
                    channelState.PinnedChannels.Add(ch);
            if (channelState.PinnedChannels.Count > 0) RecomputeBlockedIndices();
        }

        protected void OnProbeFileNameChanged(string newPath)
        {
            if (hasChanges)
            {
                var r = MessageBox.Show("Changing the file will discard unsaved changes. Continue?",
                    "Change Probe Interface File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }

            var oldPath = ProbeConfigurationBase.ProbeInterfaceFileName;
            ProbeConfigurationBase.ProbeInterfaceFileName = newPath;

            try
            {
                ReplaceProbeGroupFromFile(newPath);
            }
            catch (Exception ex)
            {
                ProbeConfigurationBase.ProbeInterfaceFileName = oldPath;
                MessageBox.Show($"Failed to load probe interface file:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RefreshProbeState();

            Log($"Loaded probeinterface file {newPath}");

            HasChanges = false;
        }

        protected bool PromptSaveOnClose()
        {
            var path = ProbeConfigurationBase.ProbeInterfaceFileName;
            bool fileExists = !string.IsNullOrEmpty(path) && File.Exists(path);

            if (fileExists)
            {
                var r = MessageBox.Show($"Save changes to {Path.GetFileName(path)}?",
                    "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return false;
                if (r == DialogResult.Yes) SaveProbeGroup();
                return true;
            }

            var create = MessageBox.Show(
                $"There are unsaved {probeName} channel configuration changes.\nWould you like to save them to a probe interface file?",
                "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (create == DialogResult.No) return true;

            using var sfd = new SaveFileDialog
            {
                Title      = "Save Probe Interface File",
                Filter     = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                DefaultExt = "json",
                FileName   = GetProbeSerialNumber() + ".json"
            };

            var calFile = PrimaryCalibrationFileName;

            if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                sfd.InitialDirectory = Path.GetDirectoryName(calFile);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ProbeConfigurationBase.ProbeInterfaceFileName = sfd.FileName;
                SaveProbeGroup();
                return true;
            }

            var discard = MessageBox.Show("Discard unsaved changes and close?",
                "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return discard == DialogResult.Yes;
        }

        protected string GetProbeSerialNumber()
        {
            var calFile = PrimaryCalibrationFileName;
            if (string.IsNullOrEmpty(calFile) || !File.Exists(calFile)) return "probe";
            try { return File.ReadLines(calFile).FirstOrDefault()?.Trim() ?? "probe"; }
            catch { return "probe"; }
        }

        #endregion

        #region Keyboard shortcuts

        protected void HandleKeyboardShortcuts()
        {
            bool ctrl = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
            bool shift = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

            if (ctrl && ImGui.IsKeyPressed(ImGuiKey.S, false))
            {
                bool hasFile = !string.IsNullOrEmpty(ProbeConfigurationBase.ProbeInterfaceFileName);
                if (hasFile) SaveProbeGroup();
                else         SaveProbeGroupAs();
                return;
            }

            if (ImGui.GetIO().WantTextInput) return;
            if (ImGui.IsKeyPressed(ImGuiKey.E, false))           EnableSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.P, false) && !shift) EnableAndPinSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.P, false) &&  shift) UnpinSelectedContacts();
            HandleProbeSpecificShortcuts(shift);
        }

        #endregion

        #region Contact info

        protected void DrawTitleBar()
        {
            ImGui.TextDisabled(probeName);
        }

        protected void DrawContactInfo()
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

            ImGuiControls.InfoRow("Index", IndexList(sel));
            ImGuiControls.InfoRow("Channel", ChannelList(sel, channelState));

            DrawProbeSpecificContactInfo(sel);

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
            ImGuiControls.InfoRow("X", xMin == xMax ? $"{xMin:0} um" : $"{xMin:0} to {xMax:0} um");
            ImGuiControls.InfoRow("Y", yMin == yMax ? $"{yMin:0} um" : $"{yMin:0} to {yMax:0} um");

            DrawContactMetrics(sel);

            int pinnedCount = sel.Count(i =>
                channelState.ElectrodeToChannel.TryGetValue(i, out int pch) && channelState.PinnedChannels.Contains(pch));
            ImGuiControls.InfoRow("Pinned", pinnedCount == 0 ? "false"
                : pinnedCount == sel.Count ? "true"
                : $"{pinnedCount}/{sel.Count}");

            int blockedCount = sel.Count(i => channelState.BlockedContactIndices.Contains(i));
            ImGuiControls.InfoRow("Blocked", blockedCount == 0 ? "false"
                : blockedCount == sel.Count ? "true"
                : $"{blockedCount}/{sel.Count}");
        }

        protected static string IndexList(IReadOnlyList<int> sel)
        {
            if (sel.Count == 1) return sel[0].ToString();
            if (sel.Count < 5) return $"[{string.Join(", ", sel)}]";
            return $"[{sel.Min()}, ..., {sel.Max()}]";
        }

        protected static string ChannelList(IReadOnlyList<int> sel, MultiplexedContactState channelState)
        {
            static string ChannelFor(int idx, MultiplexedContactState channelState) =>
                channelState.ElectrodeToChannel.TryGetValue(idx, out int ch) ? ch.ToString() : "x";

            if (sel.Count == 1) return ChannelFor(sel[0], channelState);
            if (sel.Count < 5) return $"[{string.Join(", ", sel.Select(i => ChannelFor(i, channelState)))}]";
            return $"[{ChannelFor(sel.Min(), channelState)}, ..., {ChannelFor(sel.Max(), channelState)}]";
        }

        #endregion

        #region Misc helpers

        protected static Vector2 ContactSizeUm(Contact c)
        {
            var sp = c.ShapeParams;
            return c.Shape switch
            {
                ContactShape.Circle => new Vector2((float)(sp.Radius ?? 6.0) * 2f, (float)(sp.Radius ?? 6.0) * 2f),
                ContactShape.Rect   => new Vector2((float)(sp.Width ?? 12.0), (float)(sp.Height ?? sp.Width ?? 12.0)),
                _                   => new Vector2((float)(sp.Width ?? 12.0), (float)(sp.Width ?? 12.0))
            };
        }

        #endregion
    }
}
