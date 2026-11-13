using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Headstage-level side panel shared by every NeuropixelsV2e-family headstage: hardware address, a
    /// probe-identity scan, and running the electrode-activity survey across one or more probes
    /// (filter/threshold/time-per-bank, which probe(s) to include, run/cancel/progress). Activity *viewing*
    /// (coloring, metric selection) stays on each probe's own dialog.
    /// </summary>
    internal sealed class NeuropixelsV2eHeadstageControlPanel : IImGuiTabPanel
    {
        readonly IReadOnlyList<NeuropixeslV2eSurveyTarget> targets;
        readonly SurveyHeadstageFactory buildHeadstage;
        readonly Action<PortName> setHeadstagePort;
        readonly Action<double?> setHeadstagePortVoltage;
        readonly PortName initialPort;
        readonly double? initialPortVoltage;
        readonly bool[] initialEnabled;
        readonly ImGuiLogConsole log;
        readonly Action<IImGuiTabPanel> selectTab;
        readonly NeuropixelsSurveyState state = new();
        readonly byte[] driverBuf = new byte[64];
        static readonly Vector4 ColorTextSuccess = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.BrightFern);
        static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eHeadstageControlPanel"/>.
        /// </summary>
        /// <param name="targets">Every probe this headstage hosts that can be scanned/surveyed.</param>
        /// <param name="buildHeadstage">Builds a fresh headstage of this headstage's own concrete type each scan/survey round.</param>
        /// <param name="initialPort">Initial hardware-address port, seeded from the headstage's own configured port.</param>
        /// <param name="setHeadstagePort">
        /// Writes an edited port back onto the headstage's own configuration node. Changing the port here is
        /// stating a physical fact about the headstage (which ONIX breakout port it's plugged into), not a
        /// survey-only parameter: unlike driver/hub index, which are PC-side connection details with no
        /// equivalent on the headstage config node.
        /// </param>
        /// <param name="initialPortVoltage">Initial voltage override, seeded from the headstage's own configured
        /// port voltage (<c>null</c> if it is auto-negotiated).</param>
        /// <param name="setHeadstagePortVoltage">
        /// Writes an edited voltage override back onto the headstage's own configuration node, for the same
        /// reason as <paramref name="setHeadstagePort"/>.
        /// </param>
        /// <param name="log">The hosting shell's console log.</param>
        /// <param name="selectTab">Shows a probe's tab in the hosting shell.</param>
        internal NeuropixelsV2eHeadstageControlPanel(IReadOnlyList<NeuropixeslV2eSurveyTarget> targets, SurveyHeadstageFactory buildHeadstage,
            PortName initialPort, Action<PortName> setHeadstagePort,
            double? initialPortVoltage, Action<double?> setHeadstagePortVoltage, ImGuiLogConsole log,
            Action<IImGuiTabPanel> selectTab)
        {
            this.targets = targets ?? throw new ArgumentNullException(nameof(targets));
            this.buildHeadstage = buildHeadstage ?? throw new ArgumentNullException(nameof(buildHeadstage));
            this.setHeadstagePort = setHeadstagePort ?? throw new ArgumentNullException(nameof(setHeadstagePort));
            this.setHeadstagePortVoltage = setHeadstagePortVoltage ?? throw new ArgumentNullException(nameof(setHeadstagePortVoltage));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.selectTab = selectTab ?? throw new ArgumentNullException(nameof(selectTab));
            this.initialPort = initialPort;
            this.initialPortVoltage = initialPortVoltage;
            state.Port = initialPort;
            state.PortVoltage = initialPortVoltage;
            initialEnabled = targets.Select(t => t.Enabled).ToArray();
            foreach (var target in targets)
            {
                target.Dialog.SurveySettings = state;
                target.Dialog.EnableLocation = "Headstage Control";
            }

            // NB: the probes' files can disagree. The first one wins, and each probe flags its own results when
            // they were measured with other settings.
            var stored = targets.Select(t => t.Dialog.Survey.Results).FirstOrDefault(r => r != null);
            if (stored != null)
            {
                state.SpikeThreshold = stored.SpikeThresholdUv;
                state.TimePerBankSeconds = stored.TimePerBankSeconds;
                state.RecordSurveyData = stored.RecordedRaw;
            }
        }

        /// <inheritdoc/>
        public bool HasChanges => false; // scan/survey config is transient session UI, not itself file-backed

        /// <inheritdoc/>
        public bool CanClose(DialogResult pendingResult)
        {
            state.Cts?.Cancel();
            return true;
        }

        /// <inheritdoc/>
        public void Closed(DialogResult result)
        {
            if (result == DialogResult.OK) return;
            if (state.Port != initialPort) setHeadstagePort(initialPort);
            if (state.PortVoltage != initialPortVoltage) setHeadstagePortVoltage(initialPortVoltage);
            for (int i = 0; i < targets.Count; i++)
                targets[i].Enabled = initialEnabled[i];
        }

        /// <inheritdoc/>
        public void Draw()
        {
            foreach (var target in targets)
                target.Dialog.ShowScannedModelOffer();

            bool isScanning = targets.Any(t => t.Dialog.Scan.Status == NeuropixelsV1ScanStatus.Running);
            // NB: not filtered by t.Enabled, since the probes are read once, when the survey starts.
            bool isSurveying = targets.Any(t => t.Dialog.Survey.Status == NeuropixelsV2eSurveyStatus.Running);
            bool hardwareBusy = isScanning || isSurveying;

            // Hardware address and probe selection are captured once, at Start() time. Editing them while
            // a scan or survey is running would silently have no effect on the run already in progress.
            if (hardwareBusy) ImGui.BeginDisabled();
            DrawHardwareAddressSection();
            if (hardwareBusy) ImGui.EndDisabled();
            ImGui.Spacing();

            ImGui.Separator();
            DrawScanSection(isScanning, blockedBySurvey: isSurveying);
            ImGui.Spacing();

            ImGui.Separator();
            if (!isSurveying)
            {
                DrawSurveySettings();
                ImGui.Spacing();
            }

            if (isSurveying)
                DrawRunning();
            else
                DrawIdle(blockedByScan: isScanning);

            ImGui.Spacing();
            DrawPerTargetStatus();
        }

        void DrawHardwareAddressSection()
        {
            ImGui.Text("Hardware configuration");
            ImGui.Spacing();

            if (!state.EditingHardwareAddr)
            {
                string voltStr = state.PortVoltage.HasValue ? $" / {state.PortVoltage:0.0}V" : "";
                ImGui.TextDisabled($"{state.Driver} / slot {state.HubIndex} / {state.Port}{voltStr}");
                ImGui.SameLine();
                if (ImGui.SmallButton("Edit##hwa"))
                {
                    state.EditingHardwareAddr = true;
                    ImGuiStrings.WriteString(driverBuf, state.Driver);
                }
                ImGuiWidgets.Tooltip("Edit the hardware address (driver, slot, port) used to scan for probes and run the electrode survey.");
            }
            else
            {
                ImGui.SetNextItemWidth(100f);
                unsafe
                {
                    fixed (byte* p = driverBuf)
                        if (ImGui.InputText("Driver##srvdrv", p, (nuint)driverBuf.Length))
                            state.Driver = ImGuiStrings.ReadBuffer(driverBuf);
                }
                ImGuiWidgets.Tooltip("Name of the device driver used to communicate with the ONIX hardware controller this headstage is connected to (e.g. \"riffa\" for a PCIe interface).");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                int hubIndex = state.HubIndex;
                if (ImGui.InputInt("Slot##srvslot", ref hubIndex))
                    state.HubIndex = Math.Max(0, hubIndex);
                ImGuiWidgets.Tooltip("Index of the host interconnect (e.g., PCIe slot) the ONIX hardware controller uses, as enumerated by the operating system.");

                var portNames = Enum.GetNames(typeof(PortName));
                int portIdx = Array.IndexOf(portNames, state.Port.ToString());
                if (portIdx < 0) portIdx = 0;
                ImGui.SetNextItemWidth(120f);
                if (ImGui.Combo("Port##srvport", ref portIdx, portNames, portNames.Length))
                {
                    state.Port = (PortName)Enum.Parse(typeof(PortName), portNames[portIdx]);
                    setHeadstagePort(state.Port);
                }
                ImGuiWidgets.Tooltip("Headstage port on the ONIX hardware controller that this headstage is physically connected to. Changing this here also updates the headstage's own port setting.");

                bool overrideV = state.PortVoltage.HasValue;
                if (ImGui.Checkbox("Override voltage##srvpv", ref overrideV))
                {
                    state.PortVoltage = overrideV ? (double?)5.0 : null;
                    setHeadstagePortVoltage(state.PortVoltage);
                }
                ImGuiWidgets.Tooltip("Manually set the headstage's supply voltage instead of auto-negotiating it. Changing this here also updates the headstage's own voltage setting.");
                if (overrideV)
                {
                    ImGui.SameLine();
                    float v = (float)(state.PortVoltage ?? 5.0);
                    ImGui.SetNextItemWidth(90f);
                    if (ImGui.SliderFloat("V##srvpvslider", ref v, 3.0f, 5.5f))
                    {
                        state.PortVoltage = Math.Round(v, 1);
                        setHeadstagePortVoltage(state.PortVoltage);
                    }
                    ImGuiWidgets.Tooltip("Supply voltage to apply to the headstage. Consult the headstage documentation for a safe range before changing this. Changing this here also updates the headstage's own voltage setting.");
                }

                ImGui.Spacing();
                if (ImGui.Button("Apply##hwa")) state.EditingHardwareAddr = false;
                ImGuiWidgets.Tooltip("Apply the hardware address above and close this editor.");
            }

            bool permissive = state.ValidationLevel == ValidationLevel.Permissive;
            if (ImGui.Checkbox("Permissive validation##srvval", ref permissive))
                state.ValidationLevel = permissive ? ValidationLevel.Permissive : ValidationLevel.Normal;
            ImGuiWidgets.Tooltip(NeuropixelsV1HeadstageControlPanel.PermissiveValidationTooltip);
        }

        void DrawScanSection(bool isScanning, bool blockedBySurvey)
        {
            ImGui.Text("Probes");
            ImGui.SameLine();

            if (isScanning)
            {
                if (ImGui.SmallButton("Cancel##hsscan")) state.Cts?.Cancel();
                ImGuiWidgets.Tooltip("Stop the probe identity scan currently in progress.");
            }
            else
            {
                if (blockedBySurvey) ImGui.BeginDisabled();
                if (ImGui.SmallButton("Scan##hsscan")) StartScan();
                ImGuiWidgets.Tooltip("Read each probe's part number and serial number directly from its EEPROM, and confirm the correct headstage is attached, without configuring or streaming from any probe.",
                    "A survey is currently running.");
                if (blockedBySurvey) ImGui.EndDisabled();
            }

            // NB: the probes are read once, when a scan or survey starts, so enabling one meanwhile would not
            // affect the run.
            bool hardwareBusy = isScanning || blockedBySurvey;
            foreach (var target in targets)
            {
                bool enabled = target.Enabled;
                if (HeadstageProbeRows.DrawEnableRow(target.Label, target.Dialog.Scan, ref enabled, hardwareBusy,
                    target.Dialog.ScannedModel, () => { target.Dialog.LoadScannedModel(); selectTab(target.Dialog); }, target.Dialog.MismatchedModel))
                    target.Enabled = enabled;
            }
        }

        // NB: every enabled probe is surveyed, so one that can't be blocks the whole survey rather than being
        // skipped without notice.
        string SurveyBlocker(bool blockedByScan)
        {
            if (blockedByScan) return "A probe identity scan is currently running.";

            var enabled = targets.Where(t => t.Enabled).ToList();
            if (enabled.Count == 0) return "Enable at least one probe first.";

            var reasons = enabled
                .Select(t => !t.Dialog.ChannelConfigurationChosen ? HeadstageProbeRows.NoChannelConfiguration(t.Label)
                    : state.ValidationLevel != ValidationLevel.Permissive &&
                      string.IsNullOrEmpty(t.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.GainCalibrationFileName)
                        ? $"{t.Label} needs a gain calibration file, unless Permissive validation is checked."
                        : null)
                .Where(reason => reason != null)
                .ToList();
            return reasons.Count > 0 ? string.Join("\n", reasons) : null;
        }

        void DrawSurveySettings()
        {
            ImGui.Text("Electrode Activity Survey");
            ImGui.Spacing();

            float thrAvail = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X;
            float thr = (float)state.SpikeThreshold;
            ImGui.SetNextItemWidth(thrAvail * 0.33f);
            if (ImGui.InputFloat("Spike threshold (uV)##thr", ref thr, 5f, 10f, "%.1f")) state.SpikeThreshold = thr;
            ImGuiWidgets.Tooltip($"Voltage a channel must cross, relative to baseline, to be counted as a spike during the survey. The survey detects spikes after a {SpikeDetection.BandDescription} band-pass filter and common median referencing.");

            float tpb = state.TimePerBankSeconds;
            ImGui.SetNextItemWidth(thrAvail * 0.33f);
            if (ImGui.InputFloat("Time per bank (s)##tpb", ref tpb, 5f, 30f, "%.0f"))
                state.TimePerBankSeconds = Math.Max(2f, Math.Min(300f, tpb));
            ImGuiWidgets.Tooltip($"How long to record from each selected bank while sweeping through the survey, in seconds. Each bank also records an extra {NeuropixelsSurveyState.SettlingSeconds:0} s first, while the amplifiers settle, which is left out of the statistics.");

            bool anyEnabledHasProbeFile = targets.Any(t => t.Enabled &&
                !string.IsNullOrEmpty(t.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.ProbeInterfaceFileName) &&
                File.Exists(t.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.ProbeInterfaceFileName));
            if (!anyEnabledHasProbeFile) { state.RecordSurveyData = false; ImGui.BeginDisabled(); }
            bool rec = state.RecordSurveyData;
            if (ImGui.Checkbox("Record raw data##recraw", ref rec)) state.RecordSurveyData = rec;
            ImGuiWidgets.Tooltip("Save the unfiltered voltage traces collected during the survey as arrow files. They go in a new folder next to the probe interface file, named <file name>_<yyyyMMdd-HHmmss>_survey-data, with one .arrow and one .json file per shank and bank. Each bank's recording includes its settling period. The log names each folder when the survey ends.",
                "Requires at least one enabled probe to have a ProbeInterface file.");
            if (!anyEnabledHasProbeFile) ImGui.EndDisabled();
        }

        void DrawIdle(bool blockedByScan)
        {
            var blocker = SurveyBlocker(blockedByScan);
            ImGui.BeginDisabled(blocker != null);
            if (ImGui.Button("Start Survey##hssurvey")) StartSurvey();
            ImGuiWidgets.Tooltip($"Sweep through the selected banks of every enabled probe, concurrently, recording from each for the configured time and computing per-contact activity statistics. Each bank first records an extra {NeuropixelsSurveyState.SettlingSeconds:0} s while the amplifiers settle, which is left out of the statistics.",
                blocker);
            ImGui.EndDisabled();
        }

        void DrawRunning()
        {
            var selected = targets.Where(t => t.Enabled).ToList();
            float avgProgress = selected.Count == 0 ? 0f : selected.Average(t => t.Dialog.Survey.Progress);
            ImGui.ProgressBar(avgProgress, new Vector2(ImGui.GetContentRegionAvail().X, 0));
            ImGui.Spacing();
            ImGui.Text("Collecting...");
            ImGui.Spacing();
            if (ImGui.Button("Cancel##hssurvey")) state.Cts?.Cancel();
            ImGuiWidgets.Tooltip("Stop the electrode survey currently in progress.");
        }

        void DrawPerTargetStatus()
        {
            foreach (var target in targets)
            {
                bool stillRunning = target.Dialog.Survey.Status == NeuropixelsV2eSurveyStatus.Running;
                if (!target.Enabled && !stillRunning) continue;
                var survey = target.Dialog.Survey;
                switch (survey.Status)
                {
                    case NeuropixelsV2eSurveyStatus.Running:
                        ImGui.TextUnformatted($"{target.Label}: running ({survey.Progress * 100f:0}%)");
                        break;
                    case NeuropixelsV2eSurveyStatus.Completed:
                        ImGui.TextColored(ColorTextSuccess, $"{target.Label}: complete" +
                            (survey.CompletedAt.HasValue ? $" ({survey.CompletedAt.Value.ToLocalTime():yyyy-MM-dd HH:mm})" : ""));
                        break;
                    case NeuropixelsV2eSurveyStatus.Failed:
                        // NB: survey.Error is arbitrary exception text and may contain '%', which
                        // TextColored's printf-style formatting would mangle. Push the color instead.
                        ImGui.PushStyleColor(ImGuiCol.Text, ColorTextError);
                        ImGui.TextUnformatted($"{target.Label}: failed - {survey.Error ?? "(unknown)"}");
                        ImGui.PopStyleColor();
                        break;
                    default:
                        ImGui.TextDisabled($"{target.Label}: idle");
                        break;
                }
            }
        }

        void StartScan()
        {
            state.Cts = new CancellationTokenSource();
            NeuropixelsV2eHeadstageScanRunner.Start(
                buildHeadstage, targets,
                state.Driver, state.HubIndex, state.ValidationLevel, state.Port, state.PortVoltage,
                Log, state.Cts.Token);
        }

        void StartSurvey()
        {
            // NB: Permissive turns a missing calibration file into a warning that only reaches the console
            if (state.ValidationLevel == ValidationLevel.Permissive)
            {
                foreach (var target in targets.Where(t => t.Enabled &&
                    !File.Exists(t.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.GainCalibrationFileName)))
                {
                    Log($"Warning: {target.Label} has no gain calibration file, so it is surveyed without gain " +
                        "correction.", true);
                }
            }

            state.Cts = new CancellationTokenSource();
            NeuropixelsV2eHeadstageSurveyRunner.Start(
                buildHeadstage, targets,
                state.Driver, state.HubIndex, state.ValidationLevel, state.Port, state.PortVoltage,
                state.SpikeThreshold, state.TimePerBankSeconds, state.RecordSurveyData,
                Log, state.Cts.Token);
        }

        void Log(string message, bool isError) => log.Log(message, isError);
    }
}
