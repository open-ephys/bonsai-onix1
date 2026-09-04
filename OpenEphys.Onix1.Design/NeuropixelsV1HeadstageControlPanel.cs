using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Headstage-level side panel for a NeuropixelsV1 headstage: hardware address, a probe-identity scan,
    /// and running the electrode-activity survey across one or more probes (filter/threshold/time-per-bank,
    /// which probe(s) to include, run/cancel/progress).
    /// </summary>
    internal sealed class NeuropixelsV1HeadstageControlPanel : IImGuiTabPanel
    {
        readonly IReadOnlyList<NeuropixelsV1SurveyTarget> targets;
        readonly SurveyHeadstageFactory buildHeadstage;
        readonly Action<PortName> setHeadstagePort;
        readonly Action<double?> setHeadstagePortVoltage;
        readonly ImGuiLogConsole log;
        readonly NeuropixelsSurveyState state = new();
        readonly byte[] driverBuf = new byte[64];
        static readonly Vector4 ColorTextSuccess = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.BrightFern);
        static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1HeadstageControlPanel"/>.
        /// </summary>
        /// <param name="targets">Every probe this headstage hosts that can be scanned/surveyed.</param>
        /// <param name="buildHeadstage">Builds a fresh headstage of this headstage's own concrete type each
        /// scan/survey round.</param>
        /// <param name="initialPort">Initial hardware-address port, seeded from the headstage's own
        /// configured port.</param>
        /// <param name="setHeadstagePort">
        /// Writes an edited port back onto the headstage's own configuration node.
        /// </param>
        /// <param name="setHeadstagePortVoltage">
        /// Writes an edited voltage override back onto the headstage's own configuration node, for the same
        /// reason as <paramref name="setHeadstagePort"/>.
        /// </param>
        /// <param name="log">The hosting shell's console log.</param>
        internal NeuropixelsV1HeadstageControlPanel(IReadOnlyList<NeuropixelsV1SurveyTarget> targets, SurveyHeadstageFactory buildHeadstage,
            PortName initialPort, Action<PortName> setHeadstagePort, Action<double?> setHeadstagePortVoltage, ImGuiLogConsole log)
        {
            this.targets = targets ?? throw new ArgumentNullException(nameof(targets));
            this.buildHeadstage = buildHeadstage ?? throw new ArgumentNullException(nameof(buildHeadstage));
            this.setHeadstagePort = setHeadstagePort ?? throw new ArgumentNullException(nameof(setHeadstagePort));
            this.setHeadstagePortVoltage = setHeadstagePortVoltage ?? throw new ArgumentNullException(nameof(setHeadstagePortVoltage));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            state.Port = initialPort;
            if (targets.Count == 1) targets[0].Selected = true;
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
        public void Draw()
        {
            bool isScanning = targets.Any(t => t.Dialog.Scan.Status == NeuropixelsV1ScanStatus.Running);
            // NB: Do not filter by t.Selected since checkbox is checked a single time at Start() time
            bool isSurveying = targets.Any(t => t.Dialog.Survey.Status == NeuropixelsV1SurveyStatus.Running);
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
            if (isSurveying) ImGui.BeginDisabled();
            DrawProbeSelection();
            if (isSurveying) ImGui.EndDisabled();
            ImGui.Spacing();

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
                    ImGuiControls.WriteString(driverBuf, state.Driver);
                }
                ImGuiControls.Tooltip("Edit the hardware address (driver, slot, port) used to scan for probes and run the electrode survey.");
            }
            else
            {
                ImGui.SetNextItemWidth(100f);
                unsafe
                {
                    fixed (byte* p = driverBuf)
                        if (ImGui.InputText("Driver##srvdrv", p, (nuint)driverBuf.Length))
                            state.Driver = ImGuiControls.ReadBuffer(driverBuf);
                }
                ImGuiControls.Tooltip("Name of the device driver used to communicate with the ONIX hardware controller this headstage is connected to (e.g. \"riffa\" for a PCIe interface).");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                int hubIndex = state.HubIndex;
                if (ImGui.InputInt("Slot##srvslot", ref hubIndex))
                    state.HubIndex = Math.Max(0, hubIndex);
                ImGuiControls.Tooltip("Index of the host interconnect (e.g., PCIe slot) the ONIX hardware controller uses, as enumerated by the operating system.");

                var portNames = Enum.GetNames(typeof(PortName));
                int portIdx = Array.IndexOf(portNames, state.Port.ToString());
                if (portIdx < 0) portIdx = 0;
                ImGui.SetNextItemWidth(120f);
                if (ImGui.Combo("Port##srvport", ref portIdx, portNames, portNames.Length))
                {
                    state.Port = (PortName)Enum.Parse(typeof(PortName), portNames[portIdx]);
                    setHeadstagePort(state.Port);
                }
                ImGuiControls.Tooltip("Headstage port on the ONIX hardware controller that this headstage is physically connected to. Changing this here also updates the headstage's own port setting.");

                bool overrideV = state.PortVoltage.HasValue;
                if (ImGui.Checkbox("Override voltage##srvpv", ref overrideV))
                {
                    state.PortVoltage = overrideV ? (double?)5.0 : null;
                    setHeadstagePortVoltage(state.PortVoltage);
                }
                ImGuiControls.Tooltip("Manually set the headstage's supply voltage instead of auto-negotiating it.");
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
                    ImGuiControls.Tooltip("Supply voltage to apply to the headstage. Consult the headstage documentation for a safe range before changing this. Changing this here also updates the headstage's own voltage setting.");
                }

                ImGui.Spacing();
                if (ImGui.Button("Apply##hwa")) state.EditingHardwareAddr = false;
                ImGuiControls.Tooltip("Apply the hardware address above and close this editor.");
            }
        }

        void DrawScanSection(bool isScanning, bool blockedBySurvey)
        {
            ImGui.Text("Probe Scan");
            ImGui.Spacing();

            if (isScanning)
            {
                if (ImGui.Button("Cancel##hsscan")) state.Cts?.Cancel();
                ImGuiControls.Tooltip("Stop the probe identity scan currently in progress.");
            }
            else
            {
                if (blockedBySurvey) ImGui.BeginDisabled();
                if (ImGui.Button("Start Scan##hsscan")) StartScan();
                ImGuiControls.Tooltip("Read each probe's part number and serial number directly from its EEPROM, and confirm the correct headstage is attached, without configuring or streaming from any probe.",
                    "A survey is currently running.");
                if (blockedBySurvey) ImGui.EndDisabled();
            }

            foreach (var target in targets)
            {
                var scan = target.Dialog.Scan;
                switch (scan.Status)
                {
                    case NeuropixelsV1ScanStatus.Running:
                        ImGui.TextUnformatted($"{target.Label}: scanning...");
                        break;
                    case NeuropixelsV1ScanStatus.Detected:
                        ImGui.TextColored(ColorTextSuccess, target.Label);
                        ImGuiControls.InfoRow("Part No.", scan.PartNumber);
                        ImGuiControls.InfoRow("SN", scan.SerialNumber.ToString());
                        break;
                    case NeuropixelsV1ScanStatus.NotDetected:
                        ImGui.TextDisabled($"{target.Label}: not detected");
                        break;
                    default:
                        ImGui.TextDisabled($"{target.Label}: not scanned");
                        break;
                }
            }
        }

        void DrawProbeSelection()
        {
            ImGui.Text("Probes to survey");
            ImGui.Spacing();
            foreach (var target in targets)
            {
                var pc = target.Dialog.ConfigureNeuropixelsV1.ProbeConfiguration;
                bool hasCal = !string.IsNullOrEmpty(pc.GainCalibrationFileName) && !string.IsNullOrEmpty(pc.AdcCalibrationFileName);
                if (!hasCal) { target.Selected = false; ImGui.BeginDisabled(); }
                bool selected = hasCal && target.Selected;
                if (ImGui.Checkbox($"{target.Label}##survsel", ref selected))
                    target.Selected = selected;
                if (!hasCal)
                {
                    ImGui.EndDisabled();
                    ImGuiControls.Tooltip("Requires gain and ADC calibration files.");
                }
            }
        }

        void DrawSurveySettings()
        {
            ImGui.Text("Electrode Activity Survey");
            ImGui.Spacing();

            float thrAvail = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X;
            float thr = (float)state.SpikeThreshold;
            ImGui.SetNextItemWidth(thrAvail * 0.33f);
            if (ImGui.InputFloat("Spike threshold (uV)##thr", ref thr, 5f, 10f, "%.1f")) state.SpikeThreshold = thr;
            ImGuiControls.Tooltip("Voltage a channel must cross, relative to baseline, to be counted as a spike during the survey.");

            float tpb = state.TimePerBankSeconds;
            ImGui.SetNextItemWidth(thrAvail * 0.33f);
            if (ImGui.InputFloat("Time per bank (s)##tpb", ref tpb, 5f, 30f, "%.0f"))
                state.TimePerBankSeconds = Math.Max(2f, Math.Min(300f, tpb));
            ImGuiControls.Tooltip("How long to record from each selected bank while sweeping through the survey, in seconds.");

            bool anySelectedHasProbeFile = targets.Any(t => t.Selected &&
                !string.IsNullOrEmpty(t.Dialog.ConfigureNeuropixelsV1.ProbeConfiguration.ProbeInterfaceFileName) &&
                File.Exists(t.Dialog.ConfigureNeuropixelsV1.ProbeConfiguration.ProbeInterfaceFileName));
            if (!anySelectedHasProbeFile) { state.RecordSurveyData = false; ImGui.BeginDisabled(); }
            bool rec = state.RecordSurveyData;
            if (ImGui.Checkbox("Record raw data##recraw", ref rec)) state.RecordSurveyData = rec;
            ImGuiControls.Tooltip("Save the raw, unfiltered voltage traces collected during the survey to disk, alongside the summary statistics, for each selected probe that has a ProbeInterface file.",
                "Requires at least one selected probe to have a ProbeInterface file.");
            if (!anySelectedHasProbeFile) ImGui.EndDisabled();
        }

        void DrawIdle(bool blockedByScan)
        {
            bool anySelected = targets.Any(t => t.Selected);
            bool disabled = !anySelected || blockedByScan;
            if (disabled) ImGui.BeginDisabled();
            if (ImGui.Button("Start Survey##hssurvey")) StartSurvey();
            ImGuiControls.Tooltip("Sweep through the selected banks of every checked probe, concurrently, recording from each for the configured time and computing per-contact activity statistics.",
                blockedByScan ? "A probe identity scan is currently running." : "Select at least one probe first.");
            if (disabled) ImGui.EndDisabled();
        }

        void DrawRunning()
        {
            var selected = targets.Where(t => t.Selected).ToList();
            float avgProgress = selected.Count == 0 ? 0f : selected.Average(t => t.Dialog.Survey.Progress);
            ImGui.ProgressBar(avgProgress, new Vector2(ImGui.GetContentRegionAvail().X, 0));
            ImGui.Spacing();
            ImGui.Text("Collecting...");
            ImGui.Spacing();
            if (ImGui.Button("Cancel##hssurvey")) state.Cts?.Cancel();
            ImGuiControls.Tooltip("Stop the electrode survey currently in progress.");
        }

        void DrawPerTargetStatus()
        {
            foreach (var target in targets)
            {
                bool stillRunning = target.Dialog.Survey.Status == NeuropixelsV1SurveyStatus.Running;
                if (!target.Selected && !stillRunning) continue;
                var survey = target.Dialog.Survey;
                switch (survey.Status)
                {
                    case NeuropixelsV1SurveyStatus.Running:
                        ImGui.TextUnformatted($"{target.Label}: running ({survey.Progress * 100f:0}%)");
                        break;
                    case NeuropixelsV1SurveyStatus.Completed:
                        ImGui.TextColored(ColorTextSuccess, $"{target.Label}: complete" +
                            (survey.CompletedAt.HasValue ? $" ({survey.CompletedAt.Value.ToLocalTime():yyyy-MM-dd HH:mm})" : ""));
                        break;
                    case NeuropixelsV1SurveyStatus.Failed:
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
            NeuropixelsV1HeadstageScanRunner.Start(
                buildHeadstage, targets,
                state.Driver, state.HubIndex, state.Port, state.PortVoltage,
                Log, state.Cts.Token);
        }

        void StartSurvey()
        {
            state.Cts = new CancellationTokenSource();
            NeuropixelsV1HeadstageSurveyRunner.Start(
                buildHeadstage, targets,
                state.Driver, state.HubIndex, state.Port, state.PortVoltage,
                state.SpikeThreshold, state.TimePerBankSeconds, state.RecordSurveyData,
                Log, state.Cts.Token);
        }

        void Log(string message, bool isError) => log.Log(message, isError);
    }
}
