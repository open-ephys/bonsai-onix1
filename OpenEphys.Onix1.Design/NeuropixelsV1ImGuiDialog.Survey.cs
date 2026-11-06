using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    // Bank-selection UI, activity-color viewing, and persistence of survey results to annotations.
    // Running/configuring the survey itself is a headstage-level concern -- see
    // NeuropixelsV1HeadstageControlPanel / NeuropixelsV1HeadstageSurveyRunner.
    internal partial class NeuropixelsV1ImGuiDialog
    {
        static readonly Vector4 ColorTextWarning = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.AmberGold);
        static readonly Vector4 ColorTextSuccess = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.BrightFern);

        void DrawSurveySection()
        {
            ImGui.Text("Electrode Survey");
            ImGui.Spacing();

            switch (survey.Status)
            {
                case NeuropixelsV1SurveyStatus.Idle:
                    ImGui.TextDisabled("No survey data yet. Run a survey from the headstage panel.");
                    break;
                case NeuropixelsV1SurveyStatus.Running:
                    showActivityColors = true; // if cancelled or failed, then wont be effective anyway
                    ImGui.TextDisabled("Survey running (see headstage panel for progress).");
                    break;
                case NeuropixelsV1SurveyStatus.Completed:
                    ImGui.Spacing();
                    DrawSurveyCompleted();
                    break;
                case NeuropixelsV1SurveyStatus.Failed:
                    showActivityColors = false;
                    ImGui.TextColored(ColorTextError, "Survey failed:");
                    // NB: TextWrapped (like Text) treats its string as a printf format -- survey.Error is
                    // arbitrary exception text and may contain '%', so wrap manually around TextUnformatted.
                    ImGui.PushTextWrapPos(0f);
                    ImGui.TextUnformatted(survey.Error ?? "(unknown)");
                    ImGui.PopTextWrapPos();
                    break;
            }
        }

        void DrawSurveyCompleted()
        {
            ImGui.TextColored(ColorTextSuccess, "Survey complete.");
            if (survey.CompletedAt.HasValue)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(survey.CompletedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }

            if (survey.IsStale)
                ImGui.TextColored(ColorTextWarning, "Survey data is over 24h old.");

            var changes = survey.Results == null ? null : SurveySettingsChanges(survey.Results);
            if (changes?.Count > 0)
            {
                ImGui.PushTextWrapPos(0f);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorTextWarning);
                ImGui.TextUnformatted($"Settings changed since this survey: {string.Join(", ", changes)}.");
                ImGui.PopStyleColor();
                ImGui.PopTextWrapPos();
            }

            // NB: Banks colors contacts by bank, so activity colors can't show there. The checkbox is disabled so it
            // doesn't look like it does something.
            ImGui.BeginDisabled(bankSelectMode);
            ImGui.Checkbox("Show activity colors##actcol", ref showActivityColors);
            ImGuiWidgets.Tooltip("Color each contact on the probe view by its measured activity from the last survey, instead of its enabled/pinned state. Equivalent to pressing A.",
                "Activity colors are not shown while selecting banks.");
            ImGui.EndDisabled();

            if (!showActivityColors || bankSelectMode) return;

            ImGui.Spacing();

            int mi = (int)selectedMetric;
            ImGui.RadioButton("SNR##met", ref mi, (int)SurveyActivityMetric.SNR);
            ImGuiWidgets.Tooltip("Color contacts by signal-to-noise ratio (SNR): the average peak-to-peak amplitude of spikes detected on the contact (signal), divided by the estimated standard deviation of the background signal (noise).");
            ImGui.SameLine();
            ImGui.RadioButton("Firing Rate (Hz)##met", ref mi, (int)SurveyActivityMetric.FireRate);
            ImGuiWidgets.Tooltip("Color contacts by firing rate: the number of spikes detected on the contact per second.");
            ImGui.SameLine();
            ImGui.RadioButton("SNR × Rate##met", ref mi, (int)SurveyActivityMetric.Combined);
            ImGuiWidgets.Tooltip("Color contacts by signal-to-noise ratio (SNR) multiplied by firing rate, favoring contacts with both strong, clean spikes and frequent firing.");
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
        }

        /// <summary>
        /// The survey settings that differ from those <paramref name="results"/> were measured with, described for
        /// the user. Each probe checks its own, since the probes' stored settings can differ from each other.
        /// </summary>
        List<string> SurveySettingsChanges(NeuropixelsV1SurveyResults results)
        {
            var changes = new List<string>();
            var settings = SurveySettings;
            if (settings != null)
            {
                if (Math.Abs(settings.SpikeThreshold - results.SpikeThresholdUv) > 0.05)
                    changes.Add($"spike threshold (was {results.SpikeThresholdUv:0.0} uV)");
                if (Math.Abs(settings.TimePerBankSeconds - results.TimePerBankSeconds) > 0.05)
                    changes.Add($"time per bank (was {results.TimePerBankSeconds:0} s)");
            }
            if (results.SpikeAmplifierGain.HasValue && results.SpikeAmplifierGain != configureNode.ProbeConfiguration.SpikeAmplifierGain)
                changes.Add($"spike gain (was {results.SpikeAmplifierGain})");
            if (!surveyBanks.SetEquals(results.SurveyBanks))
                changes.Add("banks");
            return changes;
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
                    ImGuiWidgets.InfoRow(label, v.HasValue ? $"{v.Value:F2}" : "—");
                }
                else
                {
                    var vals = sel.Where(i => i < data.Length && data[i].HasValue)
                                  .Select(i => data[i].Value).ToArray();
                    if (vals.Length > 0) ImGuiWidgets.InfoRow(label, $"avg {vals.Average():F2} / max {vals.Max():F2}");
                }
            }

            ShowMetric("Amplitude (uV)", ampData);
            ShowMetric("Noise std dev (uV)", noiseData);
            ShowMetric("SNR", snrData);
            ShowMetric("Firing rate (Hz)", rateData);
            ShowMetric("SNR × Firing rate", snrRateData);
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
            probeGroup.Probe.Annotations.SetAnnotation<float>("survey_time_per_bank", results.TimePerBankSeconds);
            probeGroup.Probe.Annotations.SetAnnotation<bool>("survey_recorded_raw", results.RecordedRaw);
            if (results.SpikeAmplifierGain.HasValue)
                probeGroup.Probe.Annotations.SetAnnotation<string>("survey_spike_amplifier_gain", results.SpikeAmplifierGain.Value.ToString());
            else
                probeGroup.Probe.Annotations.RemoveAnnotation("survey_spike_amplifier_gain");
            if (results.ProbeSerialNumber.HasValue)
                probeGroup.Probe.Annotations.SetAnnotation<string>("serial_number", results.ProbeSerialNumber.Value.ToString());
            else
                probeGroup.Probe.Annotations.RemoveAnnotation("serial_number");
            var bankIncluded = allContacts
                .Select((_, i) => results.SurveyBanks.Contains(NeuropixelsV1ProbeGroup.GetBank(i)))
                .ToArray();
            probeGroup.Probe.SetContactAnnotation<bool>("survey_bank_included", bankIncluded);
        }

        void RestoreActivityData()
        {
            surveyBanks.Clear();
            pendingBanks.Clear();
            if (probeGroup.Probe != null)
                surveyBanks.UnionWith(StoredSurveyBanks());
            RestoreSurveyResults();
        }

        IEnumerable<int> StoredSurveyBanks()
        {
            var bi = probeGroup.Probe.GetContactAnnotation<bool>("survey_bank_included");
            if (bi == null) yield break;
            for (int i = 0; i < allContacts.Count && i < bi.Length; i++)
                if (bi[i])
                    yield return NeuropixelsV1ProbeGroup.GetBank(i);
        }

        // NB: leaves the bank selection alone, so it can run again when a scan finds the probe's serial number.
        void RestoreSurveyResults()
        {
            // code below contains early exits, so make sure we clear the survey state so that nothing old is
            // hanging around
            survey.Restore(null, null);
            survey.Progress = 0f;
            survey.Error = null;
            showActivityColors = false;
            actRangeInitialized = false;
            actRanges.Clear();

            var probe = probeGroup.Probe;
            if (probe == null) return;

            DateTimeOffset.TryParse(probe.Annotations.GetAnnotation<string>("survey_datetime"), out var dt);

            // results
            var ampValues  = probe.GetContactAnnotation<float>("survey_amplitude");
            if (ampValues == null || ampValues.Length == 0) return;
            var rateValues = probe.GetContactAnnotation<float>("survey_firerate");
            if (rateValues == null || rateValues.Length == 0) return;
            var noiseArr = probe.GetContactAnnotation<float>("survey_noise");
            if (noiseArr == null || noiseArr.Length == 0) return;

            ulong? serialNumber = ulong.TryParse(probe.Annotations.GetAnnotation<string>("serial_number"), out var sn)
                ? sn : null;
            if (!SurveyMatchesProbe(serialNumber, scan.Status == NeuropixelsV1ScanStatus.Detected ? scan.SerialNumber : null))
                return;

            var thr = probe.Annotations.GetAnnotation<float?>("survey_spike_threshold_uV") ?? -50f;
            var tpb = probe.Annotations.GetAnnotation<float?>("survey_time_per_bank") ?? 5f;
            var recordedRaw = probe.Annotations.GetAnnotation<bool?>("survey_recorded_raw") ?? false;
            NeuropixelsV1Gain? spikeGain = Enum.TryParse<NeuropixelsV1Gain>(
                probe.Annotations.GetAnnotation<string>("survey_spike_amplifier_gain"), out var gain) ? gain : null;

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

            survey.Restore(new NeuropixelsV1SurveyResults(amp, rate, noise, thr, tpb, recordedRaw, serialNumber, spikeGain,
                StoredSurveyBanks().ToList()), dt);
        }
    }
}
