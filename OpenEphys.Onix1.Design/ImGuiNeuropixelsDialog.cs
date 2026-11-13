using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Shared scaffold for ImGui-rendered Neuropixels probe-configuration dialogs: channel/pin
    /// bookkeeping, probe-interface file load/save, the enable/pin/unpin interaction, contact info
    /// display, and keyboard shortcuts common to every Neuropixels probe family. Subclasses supply
    /// probe-specific data access and UI through the abstract/virtual members below.
    /// </summary>
    internal abstract class ImGuiNeuropixelsDialog<TProbeGroup> : ImGuiProbePanel
        where TProbeGroup : SingleProbeGroup, IMultiplexedProbeGroup
    {
        protected readonly string probeName;
        protected IReadOnlyList<Contact> allContacts = Array.Empty<Contact>();
        protected readonly MultiplexedContactState channelState = new();
        static readonly Vector4 ColorTextUnsaved = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.AmberGold);
        const string BankSelectHint = "Turn off \"Select banks to survey\" to edit channels.";

        /// <summary>
        /// Whether the probe view selects banks for the survey instead of contacts. Channel editing is off meanwhile.
        /// </summary>
        protected bool bankSelectMode;

        // NB: the path is a node property, but New, Open, and Save As change it together with the channel
        // configuration. This is the path of the file that matches the last channel configuration the user kept, so
        // discarding the channel configuration can put it back. Otherwise a discarded New would leave the node
        // without its file.
        string keptProbeInterfaceFileName;
        bool discardOnClose;

        protected static readonly uint ColorContactEnabled = ImGuiPalette.AmberGold;
        protected static readonly uint ColorContactPinned = ImGuiPalette.NeonPink;
        protected static readonly uint ColorContactDisabled = ImGuiPalette.Grey0x4D;
        protected const float ComboboxStartWidthPx = 165f;

        // NB: start the view at a 2 mm vertical box that extends 300 um below contact 0
        protected ImGuiNeuropixelsDialog(string probeName, ImGuiLogConsole log)
            : base(new ImGuiProbeSelector(zoomWindow: 2000f, centerY: 700f), log)
        {
            this.probeName = probeName;
        }

        /// <inheritdoc/>
        public override bool HasChanges { get; protected set; }

        /// <summary>
        /// Whether the user has chosen a channel configuration, by opening a file or choosing a model from New.
        /// </summary>
        /// <remarks>
        /// Until then, the probe group is a placeholder that is never shown, so the probe view, the channel controls,
        /// the survey, and saving are unavailable.
        /// </remarks>
        internal bool ChannelConfigurationChosen { get; private protected set; }

        /// <summary>
        /// Where the user enables the probe, which the message shown while it is disabled names.
        /// </summary>
        internal string EnableLocation { get; set; } = "the property grid";

        /// <inheritdoc/>
        protected override bool PropsDisabled => !ProbeEnabled;

        /// <inheritdoc/>
        protected override string ProbeViewMessage =>
            !ProbeEnabled ? $"{probeName} is disabled. Enable it in {EnableLocation} to configure the probe."
            : ChannelConfigurationChosen ? null
            : DefaultGeometryOptions.Count > 0
                ? "No channel configuration. Choose a probe model from New, or open a ProbeInterface file."
                : "No channel configuration. Open a ProbeInterface file.";

        /// <inheritdoc/>
        public override bool CanClose(DialogResult pendingResult)
        {
            // NB: anything but OK discards every change, so there's nothing to ask about. A missing file alone does not
            // prompt, since the title bar already says what to do, and a probe that is not plugged in has no file.
            discardOnClose = false;
            if (pendingResult != DialogResult.OK || !HasChanges)
                return true;

            // NB: whatever is still unsaved after the prompt is discarded. That covers No, and a save that failed.
            bool close = PromptSaveOnClose();
            discardOnClose = close && HasChanges;
            return close;
        }

        /// <inheritdoc/>
        public override void Closed(DialogResult result)
        {
            if (result != DialogResult.OK)
                RestoreProbeConfiguration();
            else if (discardOnClose)
                ProbeConfigurationBase.ProbeInterfaceFileName = keptProbeInterfaceFileName;
        }

        /// <summary>
        /// Puts back the probe configuration the node had when the dialog opened.
        /// </summary>
        protected abstract void RestoreProbeConfiguration();

        /// <summary>
        /// Records the node's probe interface path as the one to restore if the channel configuration's changes are
        /// discarded. Called once the probe group is first loaded.
        /// </summary>
        protected void KeepProbeInterfaceFileName() =>
            keptProbeInterfaceFileName = ProbeConfigurationBase.ProbeInterfaceFileName;

        #region Seams implemented by probe-specific subclasses

        /// <summary>
        /// The current probe group.
        /// </summary>
        internal abstract TProbeGroup ProbeGroup { get; }

        /// <summary>
        /// The probe configuration's ProbeInterface.NET-facing surface.
        /// </summary>
        protected abstract IProbeInterfaceConfiguration ProbeConfigurationBase { get; }

        /// <summary>
        /// Whether the node configures the probe, which then needs a probe interface file to run.
        /// </summary>
        protected abstract bool ProbeEnabled { get; }

        /// <summary>
        /// The serial number a probe scan found, or <c>null</c> if no scan has found the probe.
        /// </summary>
        protected abstract ulong? ScannedSerialNumber { get; }

        /// <summary>
        /// The part number a probe scan found, or <c>null</c> if no scan has found the probe.
        /// </summary>
        protected abstract string ScannedPartNumber { get; }

        /// <summary>
        /// The calibration file used as a fallback directory when browsing for a probe-interface file.
        /// </summary>
        protected abstract string PrimaryCalibrationFileName { get; }

        /// <summary>
        /// The headstage's current survey settings, shared by every probe on it, or <c>null</c> where the dialog
        /// has no survey panel.
        /// </summary>
        internal NeuropixelsSurveyState SurveySettings { get; set; }

        /// <summary>
        /// The serial number on the first line of a Neuropixels calibration file, or <c>null</c> if it can't be read.
        /// </summary>
        protected static ulong? CalibrationFileSerialNumber(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
                using var reader = new StreamReader(path);
                return ulong.TryParse(reader.ReadLine()?.Trim(), out var serialNumber) ? serialNumber : null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }

        /// <summary>
        /// Whether survey results measured on the probe with serial number <paramref name="surveySerialNumber"/>
        /// may be shown for the probe this dialog configures. Logs why when they may not.
        /// </summary>
        /// <param name="surveySerialNumber">The serial number stored with the survey results, if any.</param>
        /// <param name="scannedSerialNumber">The serial number found by a probe scan, if one has run. Otherwise
        /// the serial number in the primary calibration file is used.</param>
        protected bool SurveyMatchesProbe(ulong? surveySerialNumber, ulong? scannedSerialNumber)
        {
            var probeSerialNumber = scannedSerialNumber ?? CalibrationFileSerialNumber(PrimaryCalibrationFileName);
            string reason =
                !surveySerialNumber.HasValue ? "they don't record which probe they were measured on" :
                !probeSerialNumber.HasValue ? "this probe's serial number is unknown. Select its calibration file or scan for probes" :
                surveySerialNumber != probeSerialNumber ? $"they were measured on probe SN {surveySerialNumber}, and this probe is SN {probeSerialNumber}" :
                null;
            if (reason != null)
                Log($"Warning: {probeName}: survey results in the probe interface file are not shown: {reason}.", true);
            return reason == null;
        }

        /// <summary>
        /// Whether the recorded voltage sign is inverted.
        /// </summary>
        protected abstract bool InvertPolarity { get; set; }

        /// <summary>
        /// Wires the given contacts to available recording channels.
        /// </summary>
        protected abstract void EnableElectrodes(IEnumerable<int> contactIndices);

        /// <summary>
        /// Returns every channel that pinning <paramref name="contactIndex"/> (currently wired to
        /// <paramref name="channel"/>) should pin. Defaults to just <paramref name="channel"/> itself.
        /// </summary>
        /// <remarks>
        /// Overridden where pinning operates at a coarser granularity than a single contact -- e.g. NP1110's
        /// channel groups, where a pinned contact's whole group (all 16 channels) should be protected, not
        /// just the one channel that contact happens to occupy.
        /// </remarks>
        protected virtual IEnumerable<int> GetChannelsToPin(int contactIndex, int channel) => new[] { channel };

        /// <summary>
        /// Replaces the current probe group by deserializing probe-interface <paramref name="json"/>.
        /// </summary>
        protected abstract void ReplaceProbeGroupFromJson(string json);

        /// <summary>
        /// The bundled default probe-interface geometries this probe family offers to quick-load, in
        /// display order. Empty hides the channel map section's New combo entirely.
        /// </summary>
        protected abstract IReadOnlyList<DefaultGeometryOption> DefaultGeometryOptions { get; }

        /// <summary>
        /// Draws whatever calibration-file input(s) this probe family needs.
        /// </summary>
        protected abstract void DrawCalibrationFileSection();

        /// <summary>
        /// Draws probe-specific settings that are saved with the workflow (e.g. reference and gain), between the
        /// calibration files and the invert-polarity checkbox.
        /// </summary>
        protected abstract void DrawProbeSpecificSettings();

        /// <summary>
        /// Draws probe-specific controls that change the channel configuration (e.g. presets), between the
        /// file buttons and the enable/pin buttons.
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
            allContacts = ProbeGroup.Probe.Contacts;
            selector.Refresh(ProbeGroup);

            RebuildMaps();
            BuildPotentialChannelMap();
            ClearPins();
            RestorePinnedState();
            OnProbeGroupRefreshed();
        }

        #endregion

        #region Channel bookkeeping (wrappers over MultiplexedContactState)

        protected void RebuildMaps() => channelState.RebuildActiveMap(ProbeGroup.ChannelMap);

        protected void BuildPotentialChannelMap() => channelState.RebuildPotentialMap(allContacts.Count, ProbeGroup);

        protected void RecomputeBlockedIndices() => channelState.RecomputeBlocked(allContacts.Count);

        protected void ClearPins() => channelState.ClearPins();

        #endregion

        #region Invert polarity and enable/pin/unpin controls

        protected void DrawInvertPolarityCheckbox()
        {
            bool invert = InvertPolarity;
            if (ImGui.Checkbox("Invert polarity##cfg", ref invert))
                InvertPolarity = invert;
            ImGuiWidgets.Tooltip("Flip the sign of the recorded voltage, correcting for the inverting amplifier in Neuropixels probes that otherwise makes spikes appear as positive deflections instead of the expected negative ones.");
        }

        protected void DrawEnablePinButtons()
        {
            bool anySelected = selector.Contacts.Any(c => c.Selected);

            if (!anySelected) ImGui.BeginDisabled();
            if (ImGui.Button("Enable (E)##enable")) EnableSelectedContacts();
            ImGuiWidgets.Tooltip("Enable the selected contacts, wiring each one to an available recording channel.",
                DisabledHint("Select at least one contact first."));
            ImGui.SameLine();
            if (ImGui.Button("Enable & Pin (P)##enablepin")) EnableAndPinSelectedContacts();
            ImGuiWidgets.Tooltip("Enable the selected contacts and pin their channel assignment so it survives future preset or probe type changes.",
                DisabledHint("Select at least one contact first."));
            if (!anySelected) ImGui.EndDisabled();

            ImGui.SameLine();

            bool anySelectedPinned = false;
            if (anySelected)
                for (int i = 0; i < selector.Contacts.Count && i < allContacts.Count; i++)
                    if (selector.Contacts[i].Selected &&
                        channelState.ElectrodeToChannel.TryGetValue(i, out int pch) &&
                        channelState.PinnedChannels.Contains(pch))
                    { anySelectedPinned = true; break; }

            if (!anySelectedPinned) ImGui.BeginDisabled();
            if (ImGui.Button("Unpin (Shift+P)##unpinsel")) UnpinSelectedContacts();
            ImGuiWidgets.Tooltip("Release the pin on the selected contacts' channels, allowing a future preset or probe type change to reassign them.",
                DisabledHint("Select a pinned contact first."));
            if (!anySelectedPinned) ImGui.EndDisabled();

            bool anyEnabledUnpinned = AnyEnabledUnpinned();
            if (!anyEnabledUnpinned) ImGui.BeginDisabled();
            if (ImGui.Button("Pin Enabled (Ctrl+P)##pinenabled")) PinEnabledContacts();
            ImGuiWidgets.Tooltip("Pin the channel assignment of every enabled contact so it survives future preset or probe type changes. For example, apply a preset, pin it, then unpin the few contacts to edit by hand.",
                DisabledHint("Every enabled contact is already pinned."));
            if (!anyEnabledUnpinned) ImGui.EndDisabled();

            ImGui.SameLine();

            bool noPins = channelState.PinnedChannels.Count == 0;
            if (noPins) ImGui.BeginDisabled();
            if (ImGui.Button("Unpin All (Ctrl+Shift+P)##unpinall")) UnpinAllContacts();
            ImGuiWidgets.Tooltip("Release every pin on this probe, allowing all channels to be reassigned by a future preset or probe type change.",
                DisabledHint("No channels are pinned."));
            if (noPins) ImGui.EndDisabled();
        }

        string DisabledHint(string hint) => bankSelectMode ? BankSelectHint : hint;

        bool AnyEnabledUnpinned()
        {
            foreach (int idx in channelState.EnabledContactIndices)
                if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch) && !channelState.PinnedChannels.Contains(ch))
                    return true;
            return false;
        }

        protected void PinEnabledContacts()
        {
            bool any = false;
            foreach (int idx in channelState.EnabledContactIndices)
                if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch))
                    foreach (var pinChannel in GetChannelsToPin(idx, ch))
                        if (channelState.PinnedChannels.Add(pinChannel))
                            any = true;
            if (!any) return;
            RecomputeBlockedIndices();
            HasChanges = true;
        }

        protected void UnpinAllContacts()
        {
            if (channelState.PinnedChannels.Count == 0) return;
            ClearPins();
            HasChanges = true;
        }

        protected void EnableSelectedContacts()
        {
            var selectedContactIndices = new List<int>();
            for (int i = 0; i < allContacts.Count; i++)
                if (i < selector.Contacts.Count && selector.Contacts[i].Selected)
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
            for (int i = 0; i < selector.Contacts.Count; i++)
                if (i < allContacts.Count && selector.Contacts[i].Selected && !channelState.BlockedContactIndices.Contains(i))
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
                    foreach (var pinChannel in GetChannelsToPin(idx, ch))
                        channelState.PinnedChannels.Add(pinChannel);

            RecomputeBlockedIndices();
            OnContactsEnabled();
            HasChanges = true;
        }

        protected void UnpinSelectedContacts()
        {
            var selected = new List<int>();
            for (int i = 0; i < selector.Contacts.Count && i < allContacts.Count; i++)
                if (selector.Contacts[i].Selected) selected.Add(i);
            UnpinContacts(selected);
        }

        protected void UnpinContacts(IEnumerable<int> contactIndices)
        {
            bool any = false;
            foreach (int i in contactIndices)
            {
                if (!channelState.ElectrodeToChannel.TryGetValue(i, out int ch)) continue;
                foreach (var pinChannel in GetChannelsToPin(i, ch))
                    if (channelState.PinnedChannels.Remove(pinChannel))
                        any = true;
            }
            if (!any) return;
            RecomputeBlockedIndices();
            HasChanges = true;
        }

        /// <summary>
        /// The enabled contact whose pinned channel blocks <paramref name="contactIndex"/>, or -1 if the
        /// contact isn't blocked by a pin.
        /// </summary>
        protected int BlockingContact(int contactIndex)
        {
            if (!channelState.BlockedContactIndices.Contains(contactIndex)) return -1;
            if (!channelState.ElectrodePotentialChannel.TryGetValue(contactIndex, out int ch)) return -1;
            foreach (var kvp in channelState.ElectrodeToChannel)
                if (kvp.Value == ch) return kvp.Key;
            return -1;
        }

        /// <summary>
        /// Why <paramref name="contactIndex"/> can't be enabled, or <c>null</c> if it can.
        /// </summary>
        protected virtual string BlockReason(int contactIndex)
        {
            if (!channelState.BlockedContactIndices.Contains(contactIndex)) return null;
            channelState.ElectrodePotentialChannel.TryGetValue(contactIndex, out int ch);
            int holder = BlockingContact(contactIndex);
            return holder >= 0 ? $"channel {ch} pinned by contact {holder}" : $"channel {ch} pinned";
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

        #region Probe settings and channel configuration sections

        // NB: the sections are split by where their settings are saved. Probe settings are node properties and are
        // saved with the workflow. The channel configuration is saved in the probe interface file.
        protected void DrawProbeSettingsSection()
        {
            ImGui.Text("Probe Settings");
            ImGuiWidgets.Tooltip("These settings are saved with the workflow.");
            ImGui.Spacing();

            DrawCalibrationFileSection();
            ImGui.Spacing();
            DrawProbeSpecificSettings();
            DrawInvertPolarityCheckbox();
        }

        /// <summary>
        /// One bundled default geometry offered by <see cref="DefaultGeometryOptions"/>: the embedded
        /// probe-interface JSON resource to load, and its display name in the quick-load dropdown.
        /// </summary>
        protected readonly struct DefaultGeometryOption
        {
            internal string ResourceName { get; }
            internal string DisplayName { get; }

            internal DefaultGeometryOption(string resourceName, string displayName)
            {
                ResourceName = resourceName;
                DisplayName = displayName;
            }
        }


        protected void DrawTitleBar()
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(probeName);
            ImGui.SameLine();

            ImGui.AlignTextToFramePadding();
            if (ChannelConfigurationChosen)
            {
                var modelName = ProbeGroup.Probe.Annotations.ModelName;
                ImGui.TextUnformatted(string.IsNullOrEmpty(modelName) ? "(no probe loaded)" : modelName);

                var description = ProbeGroup.Probe.Annotations.GetAnnotation<string>("description");
                if (!string.IsNullOrEmpty(description))
                    ImGuiWidgets.Tooltip(description);
            }
            else
            {
                ImGui.TextDisabled("No probe chosen");
            }

            DrawFileStatus();
        }

        // NB: right-aligned in the title bar, so that a missing file or unsaved changes are the first thing seen. With
        // no file, it says what to do next, which also covers any unsaved changes, since with no file nothing is saved.
        void DrawFileStatus()
        {
            if (!ProbeEnabled) return;
            var noFile = string.IsNullOrEmpty(ProbeConfigurationBase.ProbeInterfaceFileName);
            var (status, tooltip) = noFile && !ChannelConfigurationChosen
                ? DefaultGeometryOptions.Count > 0
                    ? ("Open a file or choose New", "No ProbeInterface file is set, and the workflow will not run " +
                        "without one. Open an existing file, or choose a probe model from New and save it to a file.")
                    : ("Open a file", "No ProbeInterface file is set, and the workflow will not run without one. " +
                        "Open an existing file.")
                : noFile
                ? ("Save to a file", "No ProbeInterface file is set, and the workflow will not run without one. " +
                    "Save the channel configuration to a file.")
                : HasChanges
                    ? ("Unsaved changes", "The channel configuration has changes that are not saved to its file.")
                    : (null, null);
            if (status == null) return;

            ImGui.SameLine();
            float slack = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(status).X;
            if (slack > 0f)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + slack);

            ImGui.TextColored(ColorTextUnsaved, status);
            ImGuiWidgets.Tooltip(tooltip);
        }

        /// <summary>
        /// Resolves a part number to its probe family's variant, or returns <c>null</c> if the family does not know
        /// it. Two part numbers with the same variant are the same probe model.
        /// </summary>
        protected abstract object ResolveVariant(string partNumber);

        /// <summary>
        /// The layout New offers for the probe model with part number <paramref name="partNumber"/>, or
        /// <c>null</c> if New offers none.
        /// </summary>
        DefaultGeometryOption? DefaultGeometryFor(string partNumber)
        {
            if (string.IsNullOrEmpty(partNumber)) return null;

            foreach (var option in DefaultGeometryOptions)
                if (string.Equals(Path.GetFileNameWithoutExtension(option.ResourceName), partNumber, StringComparison.OrdinalIgnoreCase))
                    return option;

            // NB: New lists one part number per model, so an alias, such as a probe with a mounting cap, is matched
            // through its variant.
            var variant = ResolveVariant(partNumber);
            if (variant == null) return null;
            foreach (var option in DefaultGeometryOptions)
                if (ReferenceEquals(ResolveVariant(Path.GetFileNameWithoutExtension(option.ResourceName)), variant))
                    return option;

            return null;
        }

        // NB: a scan finishes on a background thread, where a message box must not be shown, so the offer waits for
        // the headstage panel's next frame.
        volatile string scannedPartNumberToOffer;

        /// <summary>
        /// Records that a scan found the probe, so that the headstage panel offers its model's layout if no channel
        /// configuration is chosen. Called from the scan's thread.
        /// </summary>
        protected void OfferScannedModel(string partNumber) => scannedPartNumberToOffer = partNumber;

        /// <summary>
        /// Offers to load the layout for the probe a scan found, if the scan found one while no channel configuration
        /// was chosen. Called by the headstage panel each frame.
        /// </summary>
        internal void ShowScannedModelOffer()
        {
            var partNumber = scannedPartNumberToOffer;
            if (partNumber == null) return;
            scannedPartNumberToOffer = null;
            if (ChannelConfigurationChosen || !ProbeEnabled) return;

            if (DefaultGeometryFor(partNumber) is not DefaultGeometryOption option)
            {
                Log($"{probeName}: the scan found a {partNumber} probe, which New has no layout for. Open a ProbeInterface file for it.");
                return;
            }

            var answer = MessageBox.Show($"The scan found a {partNumber} probe for {probeName}, which has no channel " +
                $"configuration.\n\nLoad the {option.DisplayName} layout? It is not saved until you save it.",
                $"{probeName}: Load Probe Layout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
                QuickLoadDefaultGeometry(option);
        }

        /// <summary>
        /// The part number New lists for the model a probe scan found, such as NP2013 for an NP2014, or <c>null</c>
        /// if no scan has found the probe or New offers no layout for its model.
        /// </summary>
        internal string ScannedModel =>
            DefaultGeometryFor(ScannedPartNumber) is DefaultGeometryOption option
                ? Path.GetFileNameWithoutExtension(option.ResourceName)
                : null;

        /// <summary>
        /// The model of the loaded channel configuration when it does not match the probe a scan found, or
        /// <c>null</c> when it matches, no scan has found the probe, or no channel configuration is chosen.
        /// </summary>
        internal string MismatchedModel =>
            ChannelConfigurationChosen && ScannedPartNumber != null && !ScannedProbeMatches()
                ? ProbeGroup.Probe.Annotations.ModelName
                : null;

        /// <summary>
        /// Whether the loaded channel configuration is for the model of the probe a scan found.
        /// </summary>
        protected abstract bool ScannedProbeMatches();

        /// <summary>
        /// Loads the layout New offers for the model a probe scan found, as choosing it from New does.
        /// </summary>
        internal void LoadScannedModel()
        {
            if (DefaultGeometryFor(ScannedPartNumber) is DefaultGeometryOption option)
                QuickLoadDefaultGeometry(option);
        }

        void QuickLoadDefaultGeometry(DefaultGeometryOption option)
        {
            if (HasChanges)
            {
                var r = MessageBox.Show($"Loading {option.DisplayName} will discard unsaved {probeName} changes. Continue?",
                    $"{probeName}: Load Default Probe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }

            try
            {
                ReplaceProbeGroupFromJson(DesignResource.LoadDefaultJson(option.ResourceName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load {option.DisplayName}:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProbeConfigurationBase.ProbeInterfaceFileName = null;
            ChannelConfigurationChosen = true;
            RefreshProbeState();
            Log($"Loaded default probe {option.DisplayName}");
            HasChanges = true;
        }

        /// <summary>Width for a file-path InputText that leaves room for a same-line "Open..." button.</summary>
        protected static float ComputeFileRowInputWidth()
        {
            float openButtonWidth = ImGui.CalcTextSize("Open...", true).X + ImGui.GetStyle().FramePadding.X * 2f;
            return ImGui.GetContentRegionAvail().X - openButtonWidth - ImGui.GetStyle().ItemSpacing.X;
        }

        protected void DrawChannelConfigurationSection()
        {
            // NB: the path is shown as text, not an editable field, because typing a path would point the node
            // at a file without loading it, and the channel map on screen would no longer match the file. With no
            // file, the title bar says so.
            string pf = ProbeConfigurationBase.ProbeInterfaceFileName ?? "";
            if (pf.Length > 0)
            {
                // NB: TextDisabled treats its string as a printf format, and a path may contain '%'.
                var fullPath = FullPathOrAsIs(pf);
                float margin = ImGui.GetStyle().ItemSpacing.X * 2f;
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
                ImGui.TextUnformatted(FitPathToWidth(fullPath, ImGui.GetContentRegionAvail().X - margin));
                ImGui.PopStyleColor();
                ImGuiWidgets.Tooltip($"{fullPath}\n\nThe enabled and pinned contacts and the survey results are " +
                    "saved in this ProbeInterface file, not in the workflow. Saving the workflow does not save them.");
                ImGui.Spacing();
            }

            if (DrawNewChannelMapCombo())
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
            ImGuiWidgets.Tooltip("Open a file browser to load an existing ProbeInterface file that specifies the channel map, contact state, and previous survey results.");

            if (!ChannelConfigurationChosen)
                return;

            ImGui.SameLine();
            // NB: saving clears HasChanges, so it is read once, or the end would not match the begin.
            ImGui.BeginDisabled(!HasChanges);
            if (ImGui.Button("Save##save")) SaveOrSaveAs();
            ImGuiWidgets.Tooltip("Save the current channel map, contact state, and survey data to the ProbeInterface file. " +
                "With no file chosen, asks where to save it.",
                "There are no unsaved changes.");
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Save As...##save"))
                SaveProbeGroupAs();
            ImGuiWidgets.Tooltip("Choose a file path and save the current channel map, contact state, and survey data to a new ProbeInterface file.");

            // NB: in bank mode the probe view colors contacts by bank, so channel edits would happen out of sight.
            ImGui.Spacing();
            if (bankSelectMode)
                ImGui.TextDisabled("Channel editing is off while selecting banks to survey.");
            ImGui.BeginDisabled(bankSelectMode);
            DrawProbeSpecificChannelControls();
            ImGui.Spacing();
            DrawEnablePinButtons();
            ImGui.EndDisabled();
        }

        bool DrawNewChannelMapCombo()
        {
            var options = DefaultGeometryOptions;
            if (options.Count == 0) return false;

            if (ImGui.BeginCombo("##newchannelmap", "New", ImGuiComboFlags.WidthFitPreview))
            {
                foreach (var option in options)
                    if (ImGui.Selectable(option.DisplayName))
                        QuickLoadDefaultGeometry(option);
                ImGui.EndCombo();
            }
            ImGuiWidgets.Tooltip("Start a new channel map from the electrode layout of a probe model. Replaces the " +
                "current channel map, contact state, and survey data, and leaves the current file untouched.");
            return true;
        }

        // NB: cuts the middle of the path so that both the drive and the whole file name stay readable.
        static string FitPathToWidth(string path, float width)
        {
            if (ImGui.CalcTextSize(path).X <= width) return path;

            int nameStart = path.Length - Path.GetFileName(path).Length;
            if (nameStart == 0) return path;

            string tail = "..." + path.Substring(nameStart - 1);
            int lo = 0, hi = nameStart - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (ImGui.CalcTextSize(path.Substring(0, mid) + tail).X <= width) lo = mid;
                else hi = mid - 1;
            }
            return path.Substring(0, lo) + tail;
        }

        static string FullPathOrAsIs(string path)
        {
            try { return Path.GetFullPath(path); }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                return path;
            }
        }

        void SaveOrSaveAs()
        {
            if (string.IsNullOrEmpty(ProbeConfigurationBase.ProbeInterfaceFileName)) SaveProbeGroupAs();
            else SaveProbeGroup();
        }

        protected void SaveProbeGroup()
        {
            var path = ProbeConfigurationBase.ProbeInterfaceFileName;
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                WritePinnedState();
                OnSavingProbeGroup();
                File.WriteAllText(path, JsonConvert.SerializeObject(ProbeGroup, Formatting.Indented));
                Log($"Probeinterface file saved to {path}");
                HasChanges = false;
                keptProbeInterfaceFileName = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void SaveProbeGroupAs()
        {
            var path = AskForProbeInterfaceFile("Save Probe Interface File As");
            if (path == null) return;

            ProbeConfigurationBase.ProbeInterfaceFileName = path;
            SaveProbeGroup();
        }

        /// <summary>
        /// Asks where to save the channel configuration and returns the chosen path, or <c>null</c> if the user
        /// cancels. The dialog starts at the current file, or at the suggested name and folder when there is none.
        /// </summary>
        string AskForProbeInterfaceFile(string title)
        {
            var current = ProbeConfigurationBase.ProbeInterfaceFileName;
            using var sfd = new SaveFileDialog
            {
                Title            = title,
                Filter           = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                DefaultExt       = "json",
                FileName         = string.IsNullOrEmpty(current) ? SuggestedProbeFileName() : Path.GetFileName(current),
                InitialDirectory = SuggestedProbeFileFolder() ?? "",
                OverwritePrompt  = true,
            };

            return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
        }

        /// <summary>
        /// The file name suggested when no file is set: the probe's serial number, which identifies the probe, and
        /// the probe's name, which identifies its position on the headstage. The serial number comes from a scan, or
        /// else from the calibration file.
        /// </summary>
        string SuggestedProbeFileName() =>
            (ScannedSerialNumber ?? CalibrationFileSerialNumber(PrimaryCalibrationFileName)) is ulong serialNumber
                ? $"{serialNumber}_{probeName}.json"
                : $"{probeName}.json";

        /// <summary>
        /// The folder the save dialog starts in: the current file's, the workflow's, or the calibration file's,
        /// whichever is found first, or <c>null</c> if none is.
        /// </summary>
        string SuggestedProbeFileFolder()
        {
            var current = ProbeConfigurationBase.ProbeInterfaceFileName;
            if (!string.IsNullOrEmpty(current))
            {
                var folder = Path.GetDirectoryName(FullPathOrAsIs(current));
                if (Directory.Exists(folder))
                    return folder;
            }

            // NB: Bonsai sets the working directory to the workflow's folder when it opens or saves a workflow, but
            // a new workflow that has never been saved leaves it wherever it was. So it is taken for the workflow's
            // folder only when it holds a workflow.
            try
            {
                var workingDirectory = Environment.CurrentDirectory;
                if (Directory.EnumerateFiles(workingDirectory, "*.bonsai").Any())
                    return workingDirectory;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { }

            var calFile = PrimaryCalibrationFileName;
            if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                return Path.GetDirectoryName(FullPathOrAsIs(calFile));

            return null;
        }

        protected void WritePinnedState()
        {
            var pinned = allContacts
                .Select((c, i) => channelState.ElectrodeToChannel.TryGetValue(i, out int ch) && channelState.PinnedChannels.Contains(ch))
                .ToArray();
            ProbeGroup.Probe.SetContactAnnotation<bool>("pinned", pinned);
        }

        protected void RestorePinnedState()
        {
            var pinned = ProbeGroup.Probe.GetContactAnnotation<bool>("pinned");
            if (pinned == null) return;
            for (int i = 0; i < allContacts.Count && i < pinned.Length; i++)
                if (pinned[i] && channelState.ElectrodeToChannel.TryGetValue(i, out int ch))
                    channelState.PinnedChannels.Add(ch);
            if (channelState.PinnedChannels.Count > 0) RecomputeBlockedIndices();
        }

        protected void OnProbeFileNameChanged(string newPath)
        {
            if (HasChanges)
            {
                var r = MessageBox.Show("Changing the file will discard unsaved changes. Continue?",
                    "Change Probe Interface File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }

            var oldPath = ProbeConfigurationBase.ProbeInterfaceFileName;
            ProbeConfigurationBase.ProbeInterfaceFileName = newPath;

            try
            {
                ReplaceProbeGroupFromJson(File.ReadAllText(newPath));
            }
            catch (Exception ex)
            {
                ProbeConfigurationBase.ProbeInterfaceFileName = oldPath;
                MessageBox.Show($"Failed to load probe interface file:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Log($"Loaded probeinterface file {newPath}");
            ChannelConfigurationChosen = true;
            RefreshProbeState();

            HasChanges = false;
            keptProbeInterfaceFileName = newPath;
        }

        bool FileExists
        {
            get
            {
                var path = ProbeConfigurationBase.ProbeInterfaceFileName;
                return !string.IsNullOrEmpty(path) && File.Exists(path);
            }
        }

        bool PromptSaveOnClose()
        {
            var path = ProbeConfigurationBase.ProbeInterfaceFileName;
            if (FileExists)
            {
                var r = MessageBox.Show($"Save changes to {Path.GetFileName(path)}?",
                    $"{probeName}: Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return false;
                if (r == DialogResult.Yes) SaveProbeGroup();
                return true;
            }

            var missing = string.IsNullOrEmpty(path)
                ? $"{probeName} has no probe interface file"
                : $"{probeName}'s probe interface file, {Path.GetFileName(path)}, does not exist";
            var consequence = ProbeEnabled ? " The workflow will not run without the file." : "";
            var answer = MessageBox.Show($"{missing}, and its channel configuration has unsaved changes.{consequence}" +
                "\n\nSave the channel configuration to a file now?",
                $"{probeName}: No Probe Interface File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (answer == DialogResult.Cancel) return false;
            if (answer == DialogResult.No) return true;

            // NB: cancelling the file dialog keeps this dialog open, so the user can choose again.
            var chosen = AskForProbeInterfaceFile("Save Probe Interface File");
            if (chosen == null) return false;

            ProbeConfigurationBase.ProbeInterfaceFileName = chosen;
            SaveProbeGroup();
            return true;
        }

        #endregion

        #region Keyboard shortcuts

        protected void HandleKeyboardShortcuts()
        {
            bool ctrl = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
            bool shift = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

            if (!ChannelConfigurationChosen || !ProbeEnabled)
                return;

            if (ctrl && ImGui.IsKeyPressed(ImGuiKey.S, false))
            {
                SaveOrSaveAs();
                return;
            }

            if (ImGui.GetIO().WantTextInput) return;
            HandleProbeSpecificShortcuts(shift);
            if (bankSelectMode) return;
            if (ImGui.IsKeyPressed(ImGuiKey.E, false))           EnableSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.P, false))
            {
                if (ctrl && shift)  UnpinAllContacts();
                else if (ctrl)      PinEnabledContacts();
                else if (shift)     UnpinSelectedContacts();
                else                EnableAndPinSelectedContacts();
            }
        }

        #endregion

        #region Contact info


        protected void DrawContactInfo()
        {
            var sel = new List<int>();
            for (int i = 0; i < allContacts.Count; i++)
                if (i < selector.Contacts.Count && selector.Contacts[i].Inspected)
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

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Rulers");
            ImGui.SameLine();
            ImGuiRulerControls.DrawToggle(selector, "Add");
            ImGui.SameLine();
            ImGuiRulerControls.DrawClear(selector);
            ImGui.Spacing();

            if (sel.Count == 0) { ImGui.TextDisabled("No contacts selected."); return; }

            ImGui.TextDisabled(usingHover ? "Hovering" : $"{sel.Count} selected");
            ImGui.Spacing();

            ImGuiWidgets.InfoRow("Index", IndexList(sel));
            ImGuiWidgets.InfoRow("Channel", ChannelList(sel, channelState));

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
            ImGuiWidgets.InfoRow("X", xMin == xMax ? $"{xMin:0} um" : $"{xMin:0} to {xMax:0} um");
            ImGuiWidgets.InfoRow("Y", yMin == yMax ? $"{yMin:0} um" : $"{yMin:0} to {yMax:0} um");

            DrawContactMetrics(sel);

            int pinnedCount = sel.Count(i =>
                channelState.ElectrodeToChannel.TryGetValue(i, out int pch) && channelState.PinnedChannels.Contains(pch));
            ImGuiWidgets.InfoRow("Pinned", pinnedCount == 0 ? "false"
                : pinnedCount == sel.Count ? "true"
                : $"{pinnedCount}/{sel.Count}");

            int blockedCount = sel.Count(i => BlockReason(i) != null);
            ImGuiWidgets.InfoRow("Blocked", blockedCount == 0 ? "false"
                : sel.Count == 1 ? BlockReason(sel[0])
                : blockedCount == sel.Count ? "true"
                : $"{blockedCount}/{sel.Count}");

            // NB: hover-only info disappears as soon as the mouse moves to a button, so the action is offered
            // only for an inspected selection.
            if (usingHover || bankSelectMode) return;
            var blockers = sel.Select(BlockingContact).Where(j => j >= 0).Distinct().ToList();
            if (blockers.Count == 0) return;
            ImGui.Spacing();
            string label = blockers.Count == 1 ? $"Unpin Contact {blockers[0]}" : $"Unpin {blockers.Count} Blocking Contacts";
            if (ImGui.Button($"{label}##unpinblockers")) UnpinContacts(blockers);
            ImGuiWidgets.Tooltip("Release the pins that block the selected contacts, so they can be enabled. " +
                "The blocking contacts stay enabled until another contact takes their channels.");
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

    }
}
