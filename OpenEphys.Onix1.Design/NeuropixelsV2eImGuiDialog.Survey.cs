using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Electrode survey UI (hardware setup, run/progress/completed views), the survey
    // log, activity-color mapping, and persistence of survey results to annotations.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void DrawSurveySection()
        {
            ImGui.Text("Electrode Survey");
            ImGui.Spacing();

            bool gainCalSet = !string.IsNullOrEmpty(configureNode.ProbeConfiguration.GainCalibrationFileName);

            if (!gainCalSet && survey.Status != NeuropixelsV2eSurveyStatus.Completed)
            {
                ImGui.TextColored(ColorTextWarning, "Requires gain calibration file.");
                return;
            }

            if (gainCalSet)
            {
                if (!editingHardwareAddr)
                {
                    string voltStr = portVoltage.HasValue ? $" / {portVoltage:0.0}V" : "";
                    ImGui.TextDisabled($"{driver} / slot {hubIndex} / {port}{voltStr}");
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Edit##hwa")) editingHardwareAddr = true;
                    Tooltip("Edit the hardware address (driver, slot, port) used to run the electrode survey.");
                }
                else
                {
                    WriteString(driverBuf, driver);
                    ImGui.SetNextItemWidth(100f);
                    unsafe
                    {
                        fixed (byte* p = driverBuf)
                            if (ImGui.InputText("Driver##srvdrv", p, (nuint)driverBuf.Length)) driver = ReadBuffer(driverBuf);
                    }
                    Tooltip("Name of the device driver used to communicate with the ONIX hardware controller this probe is connected to (e.g. \"riffa\" for a PCIe interface).");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100f);
                    ImGui.InputInt("Slot##srvslot", ref hubIndex);
                    if (hubIndex < 0) hubIndex = 0;
                    Tooltip("Index of the host interconnect (e.g., PCIe slot) the ONIX hardware controller uses, as enumerated by the operating system.");

                    var portNames = Enum.GetNames(typeof(PortName));
                    int portIdx = Array.IndexOf(portNames, port.ToString());
                    if (portIdx < 0) portIdx = 0;
                    ImGui.SetNextItemWidth(120f);
                    if (ImGui.Combo("Port##srvport", ref portIdx, portNames, portNames.Length))
                        port = (PortName)Enum.Parse(typeof(PortName), portNames[portIdx]);
                    Tooltip("Headstage port on the ONIX hardware controller that this probe is physically connected to.");

                    bool overrideV = portVoltage.HasValue;
                    if (ImGui.Checkbox("Override voltage##srvpv", ref overrideV))
                        portVoltage = overrideV ? (double?)5.0 : null;
                    Tooltip("Manually set the headstage's supply voltage instead of auto-negotiating it.");
                    if (overrideV)
                    {
                        ImGui.SameLine();
                        float v = (float)(portVoltage ?? 5.0);
                        ImGui.SetNextItemWidth(90f);
                        if (ImGui.SliderFloat("V##srvpvslider", ref v, 3.0f, 5.5f))
                            portVoltage = Math.Round(v, 1);
                        Tooltip("Supply voltage to apply to the headstage. Consult the headstage documentation for a safe range before changing this.");
                    }

                    ImGui.Spacing();
                    if (ImGui.Button("Apply##hwa")) editingHardwareAddr = false;
                    Tooltip("Apply the hardware address above and close this editor.");
                }

                ImGui.Spacing();

                if (ImGui.Checkbox("Select survey banks (B)##bsm", ref bankSelectMode))
                    selector.ClearSelection();
                Tooltip("Select whole banks (384 contacts each) instead of individual contacts, to choose which banks the survey sweeps through. Equivalent to pressing B.");

                ImGui.Spacing();

                bool bpf = useBandpassFilter;
                if (ImGui.Checkbox("300-6000 Hz bandpass##bpf", ref bpf)) useBandpassFilter = bpf;
                Tooltip("Band-pass filter the signal to 300-6000 Hz before detecting spikes, removing slow drift and high-frequency noise outside the typical spike frequency band.");

                float thrAvail = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X;
                ImGui.SetNextItemWidth(thrAvail * 0.75f);
                ImGui.SliderFloat("##thr", ref spikeThreshold, -500f, 500f, "%.1f uV");
                Tooltip("Voltage a channel must cross, relative to baseline, to be counted as a spike during the survey.");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(thrAvail * 0.25f - ImGui.CalcTextSize("Threshold").X);
                ImGui.InputFloat("Threshold##thrinput", ref spikeThreshold, 0f, 0f, "%.1f");
                Tooltip("Voltage a channel must cross, relative to baseline, to be counted as a spike during the survey.");

                float tpb = timePerBank;
                ImGui.SetNextItemWidth(thrAvail * 0.33f);
                if (ImGui.InputFloat("Time per bank (s)##tpb", ref tpb, 5f, 30f, "%.0f"))
                    timePerBank = Math.Max(2f, Math.Min(300f, tpb));
                Tooltip("How long to record from each selected bank while sweeping through the survey, in seconds.");

                bool hasProbeFile = !string.IsNullOrEmpty(configureNode.ProbeConfiguration.ProbeInterfaceFileName)
                                    && System.IO.File.Exists(configureNode.ProbeConfiguration.ProbeInterfaceFileName);
                if (!hasProbeFile) { recordSurveyData = false; ImGui.BeginDisabled(); }
                ImGui.Checkbox("Record raw data##recraw", ref recordSurveyData);
                Tooltip("Save the raw, unfiltered voltage traces collected during the survey to disk, alongside the summary statistics.",
                    "Requires a ProbeInterface file.");
                if (!hasProbeFile) ImGui.EndDisabled();

                ImGui.Spacing();
            }

            switch (survey.Status)
            {
                case NeuropixelsV2eSurveyStatus.Idle:
                    DrawSurveyIdle();
                    break;
                case NeuropixelsV2eSurveyStatus.Running:
                    showActivityColors = true; // if cancelled or failed, then wont be effective anyway
                    DrawSurveyRunning();
                    break;
                case NeuropixelsV2eSurveyStatus.Completed:
                    ImGui.Spacing();
                    DrawSurveyCompleted(gainCalSet);
                    break;
                case NeuropixelsV2eSurveyStatus.Failed:
                    showActivityColors = false;
                    ImGui.TextColored(ColorTextError, "Survey failed:");
                    ImGui.TextWrapped(survey.Error ?? "(unknown)");
                    ImGui.Spacing();
                    DrawSurveyIdle();
                    break;
            }
        }

        void DrawSurveyIdle()
        {
            if (survey.IsStale) { ImGui.TextColored(ColorTextWarning, "Survey data is over 24h old."); ImGui.Spacing(); }
            if (ImGui.Button("Start Survey##survey")) StartSurvey();
            Tooltip("Sweep through the selected banks, recording from each for the configured time and computing per-contact activity statistics (amplitude, noise, firing rate).");
        }

        void DrawSurveyRunning()
        {
            ImGui.ProgressBar(survey.Progress, new Vector2(ImGui.GetContentRegionAvail().X, 0));
            ImGui.Spacing();
            ImGui.Text($"Collecting...");
            ImGui.Spacing();
            if (ImGui.Button("Cancel##survey")) cts?.Cancel();
            Tooltip("Stop the electrode survey currently in progress.");
        }

        void DrawSurveyCompleted(bool gainCalSet)
        {
            ImGui.TextColored(ColorTextSuccess, "Survey complete.");
            if (survey.CompletedAt.HasValue)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(survey.CompletedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }

            if (survey.IsStale)
                ImGui.TextColored(ColorTextWarning, "Survey data is over 24h old.");

            ImGui.Checkbox("Show activity colors##actcol", ref showActivityColors);
            Tooltip("Color each contact on the probe view by its measured activity from the last survey, instead of its enabled/pinned state. Equivalent to pressing A.");

            if (!showActivityColors)
            {
                DrawRerunSurveyButton(gainCalSet);
                return;
            }

            ImGui.Spacing();

            int mi = (int)selectedMetric;
            ImGui.RadioButton("SNR##met", ref mi, (int)SurveyActivityMetric.SNR);
            Tooltip("Color contacts by signal-to-noise ratio (SNR): the average peak-to-peak amplitude of spikes detected on the contact (signal), divided by the estimated standard deviation of the background signal (noise).");
            ImGui.SameLine();
            ImGui.RadioButton("Firing Rate (Hz)##met", ref mi, (int)SurveyActivityMetric.FireRate);
            Tooltip("Color contacts by firing rate: the number of spikes detected on the contact per second.");
            ImGui.SameLine();
            ImGui.RadioButton("SNR × Rate##met", ref mi, (int)SurveyActivityMetric.Combined);
            Tooltip("Color contacts by signal-to-noise ratio (SNR) multiplied by firing rate, favoring contacts with both strong, clean spikes and frequent firing.");
            if ((SurveyActivityMetric)mi != selectedMetric)
            {
                actRanges[selectedMetric] = (actMin, actMax);
                selectedMetric = (SurveyActivityMetric)mi;
                actRangeInitialized = false;
            }

            ImGui.Spacing();

            var displayData = CurrentDisplayData();
            if (!actRangeInitialized && displayData != null)
            {
                actDomainMax = displayData.Max() ?? 1f;
                actDomainMin = displayData.Min() ?? 0f;
                if (actRanges.TryGetValue(selectedMetric, out var saved)) // attemped to restore last range setting
                {
                    actMin = Math.Max(actDomainMin, Math.Min(saved.min, actDomainMax));
                    actMax = Math.Max(actDomainMin, Math.Min(saved.max, actDomainMax));
                    if (actMin > actMax) (actMin, actMax) = (actDomainMin, actDomainMax);
                }
                else
                {
                    actMin = actDomainMin;
                    actMax = actDomainMax;
                }
                actRanges[selectedMetric] = (actMin, actMax);
                actRangeInitialized = true;
            }

            if (displayData == null)
            {
                ImGui.TextDisabled("No data available for this metric.");
            }
            else if (ImGuiColormapRangeSlider.Draw(
                "actrange", Plasma.DefaultMap,
                actDomainMin, actDomainMax,
                ref actMin, ref actMax,
                minHandleTooltip: "Lower bound of the value range mapped to color; values at or below this appear as the darkest color.",
                maxHandleTooltip: "Upper bound of the value range mapped to color; values at or above this appear as the brightest color."))
            {
                actRanges[selectedMetric] = (actMin, actMax);
            }
            ImGui.Spacing();

            DrawRerunSurveyButton(gainCalSet);
        }

        void DrawRerunSurveyButton(bool gainCalSet)
        {
            if (!gainCalSet) ImGui.BeginDisabled();
            if (ImGui.Button("Re-run Survey##survey")) { actRangeInitialized = false; StartSurvey(); }
            if (!gainCalSet) ImGui.EndDisabled();

            if (gainCalSet)
                Tooltip("Run the electrode survey again with the current settings, replacing the previous results.");
            else
            {
                ImGui.SameLine();
                ImGui.TextColored(ColorTextWarning, "Requires gain calibration file.");
            }
        }

        float?[] CurrentDisplayData()
        {
            var results = survey.Results;
            return results == null ? null : selectedMetric switch
                                            {
                                                SurveyActivityMetric.FireRate => results.ActivityFireRate,
                                                SurveyActivityMetric.Combined => results.SnrTimesFireRate,
                                                _                             => results.Snr
                                            };
        }

        uint ActivityColor(float? v)
        {
            if (!v.HasValue) return ColorContactDisabled;
            float t = actMax > actMin
                ? Math.Max(0f, Math.Min(1f, (v.Value - actMin) / (actMax - actMin))) : 0f;
            return Plasma.DefaultMap[(int)(t * 255f)];
        }

        protected override void DrawContactMetrics(IReadOnlyList<int> sel)
        {
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
        }

        void StartSurvey()
        {
            cts = new CancellationTokenSource();
            actRangeInitialized = false;

            string recordingFolder = null;
            if (recordSurveyData)
            {
                var probeFile = configureNode.ProbeConfiguration.ProbeInterfaceFileName;
                var dir = System.IO.Path.GetDirectoryName(probeFile);
                var name = System.IO.Path.GetFileNameWithoutExtension(probeFile);
                recordingFolder = System.IO.Path.Combine(dir, $"{name}_{DateTime.Now:yyyyMMdd-HHmmss}_survey-data");
            }

            var frozenSurveyBanks = new HashSet<(int shank, NeuropixelsV2Bank bank)>(surveyBanks);

            NeuropixelsV2eSurveyRunner.Start(probeGroup, configureNode.ProbeConfiguration,
                survey, Log, driver, hubIndex, port, isProbeA, portVoltage,
                spikeThreshold, useBandpassFilter, timePerBank, frozenSurveyBanks,
                recordingFolder, cts.Token);
        }

        protected override void OnSavingProbeGroup() => WriteActivityData();

        void WriteActivityData()
        {
            var results = survey.Results;
            if (results == null) return;
            var amp = results.ActivityAmplitude;
            var rate = results.ActivityFireRate;
            var noise = results.Noise;

            var ampValues   = allContacts.Select((_, i) => i < amp.Length   ? (amp[i]   ?? float.NaN) : float.NaN).ToArray();
            var rateValues  = allContacts.Select((_, i) => i < rate.Length  ? (rate[i]  ?? float.NaN) : float.NaN).ToArray();
            var noiseValues = allContacts.Select((_, i) => i < noise.Length ? (noise[i] ?? float.NaN) : float.NaN).ToArray();

            probeGroup.Probe.SetContactAnnotation<float>("survey_amplitude", ampValues);
            probeGroup.Probe.SetContactAnnotation<float>("survey_firerate", rateValues);
            probeGroup.Probe.SetContactAnnotation<float>("survey_noise", noiseValues);
            probeGroup.Probe.Annotations.SetAnnotation<string>("survey_datetime", survey.CompletedAt.Value.ToString("o"));
            probeGroup.Probe.Annotations.SetAnnotation<float>("survey_spike_threshold_uV", results.SpikeThresholdUv);
            probeGroup.Probe.Annotations.SetAnnotation<bool>("survey_bandpass_filter", results.UseBandpassFilter);
            probeGroup.Probe.Annotations.SetAnnotation<float>("survey_time_per_bank", results.TimePerBankSeconds);
            var bankIncluded = allContacts
                .Select((_, i) => results.SurveyBanks.Contains((probeGroup.GetShank(i), probeGroup.GetBank(i))))
                .ToArray();
            probeGroup.Probe.SetContactAnnotation<bool>("survey_bank_included", bankIncluded);
        }

        void RestoreActivityData()
        {
            // code below contains early exits, so make sure we clear the survey state so that nothing old is
            // hanging around
            survey.Results = null;
            survey.Status = NeuropixelsV2eSurveyStatus.Idle;
            survey.CompletedAt = null;
            survey.Progress = 0f;
            survey.Error = null;
            showActivityColors = false;
            actRangeInitialized = false;
            actRanges.Clear();

            var firstProbe = probeGroup.Probe;
            if (firstProbe == null) return;

            // parameters
            var thr = firstProbe.Annotations.GetAnnotation<float?>("survey_spike_threshold_uV");
            spikeThreshold = thr ?? spikeThreshold;
            var bpf = firstProbe.Annotations.GetAnnotation<bool?>("survey_bandpass_filter");
            useBandpassFilter = bpf ?? useBandpassFilter;
            var tpb = firstProbe.Annotations.GetAnnotation<float?>("survey_time_per_bank");
            timePerBank = tpb ?? timePerBank;
            DateTimeOffset.TryParse(firstProbe.Annotations.GetAnnotation<string>("survey_datetime"), out var dt);

            var bi = firstProbe.GetContactAnnotation<bool>("survey_bank_included");
            if (bi != null && bi.Length > 0)
            {
                surveyBanks.Clear();
                for (int i = 0; i < allContacts.Count && i < bi.Length; i++)
                    if (bi[i])
                        surveyBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
            }

            // results
            var ampValues  = firstProbe.GetContactAnnotation<float>("survey_amplitude");
            if (ampValues == null || ampValues.Length == 0) return;
            var rateValues = firstProbe.GetContactAnnotation<float>("survey_firerate");
            if (rateValues == null || rateValues.Length == 0) return;
            var noiseArr = firstProbe.GetContactAnnotation<float>("survey_noise");
            if (noiseArr == null || noiseArr.Length == 0) return;

            var amp   = new float?[allContacts.Count];
            var rate  = new float?[allContacts.Count];
            var noise = new float?[allContacts.Count];
            for (int i = 0; i < allContacts.Count && i < ampValues.Length; i++)
            {
                amp[i]  = float.IsNaN(ampValues[i]) ? (float?)null : ampValues[i];
                float rv = i < rateValues.Length ? rateValues[i] : float.NaN;
                rate[i] = float.IsNaN(rv) ? (float?)null : rv;
            }
            for (int i = 0; i < allContacts.Count && i < noiseArr.Length; i++)
                noise[i] = float.IsNaN(noiseArr[i]) ? (float?)null : noiseArr[i];

            survey.Results = new NeuropixelsV2eSurveyResults(
                amp, rate, noise,
                spikeThreshold, useBandpassFilter, timePerBank, surveyBanks);
            survey.CompletedAt = dt;
            survey.Status = NeuropixelsV2eSurveyStatus.Completed;
        }
    }
}
