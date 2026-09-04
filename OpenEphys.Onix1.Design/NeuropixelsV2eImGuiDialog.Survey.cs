using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Bank-selection UI, activity-color viewing, and persistence of survey results to annotations.
    // Running/configuring the survey itself is a headstage-level concern -- see
    // NeuropixelsV2eHeadstageControlPanel / NeuropixelsV2eHeadstageSurveyRunner.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void DrawSurveySection()
        {
            ImGui.Text("Electrode Survey");
            ImGui.Spacing();

            if (ImGui.Checkbox("Select survey banks (B)##bsm", ref bankSelectMode))
                selector.ClearSelection();
            ImGuiControls.Tooltip("Select whole banks (384 contacts each) instead of individual contacts, to choose which banks the survey sweeps through. Equivalent to pressing B. Run the survey itself from the headstage panel.");

            ImGui.Spacing();

            switch (survey.Status)
            {
                case NeuropixelsV2eSurveyStatus.Idle:
                    ImGui.TextDisabled("No survey data yet. Run a survey from the headstage panel.");
                    break;
                case NeuropixelsV2eSurveyStatus.Running:
                    showActivityColors = true; // if cancelled or failed, then wont be effective anyway
                    ImGui.TextDisabled("Survey running (see headstage panel for progress).");
                    break;
                case NeuropixelsV2eSurveyStatus.Completed:
                    ImGui.Spacing();
                    DrawSurveyCompleted();
                    break;
                case NeuropixelsV2eSurveyStatus.Failed:
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

            ImGui.Checkbox("Show activity colors##actcol", ref showActivityColors);
            ImGuiControls.Tooltip("Color each contact on the probe view by its measured activity from the last survey, instead of its enabled/pinned state. Equivalent to pressing A.");

            if (!showActivityColors) return;

            ImGui.Spacing();

            int mi = (int)selectedMetric;
            ImGui.RadioButton("SNR##met", ref mi, (int)SurveyActivityMetric.SNR);
            ImGuiControls.Tooltip("Color contacts by signal-to-noise ratio (SNR): the average peak-to-peak amplitude of spikes detected on the contact (signal), divided by the estimated standard deviation of the background signal (noise).");
            ImGui.SameLine();
            ImGui.RadioButton("Firing Rate (Hz)##met", ref mi, (int)SurveyActivityMetric.FireRate);
            ImGuiControls.Tooltip("Color contacts by firing rate: the number of spikes detected on the contact per second.");
            ImGui.SameLine();
            ImGui.RadioButton("SNR × Rate##met", ref mi, (int)SurveyActivityMetric.Combined);
            ImGuiControls.Tooltip("Color contacts by signal-to-noise ratio (SNR) multiplied by firing rate, favoring contacts with both strong, clean spikes and frequent firing.");
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
                    ImGuiControls.InfoRow(label, v.HasValue ? $"{v.Value:F2}" : "—");
                }
                else
                {
                    var vals = sel.Where(i => i < data.Length && data[i].HasValue)
                                  .Select(i => data[i].Value).ToArray();
                    if (vals.Length > 0) ImGuiControls.InfoRow(label, $"avg {vals.Average():F2} / max {vals.Max():F2}");
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
            var bankIncluded = allContacts
                .Select((_, i) => results.SurveyBanks.Contains((probeGroup.GetShank(i), probeGroup.GetBank(i))))
                .ToArray();
            probeGroup.Probe.SetContactAnnotation<bool>("survey_bank_included", bankIncluded);
        }

        void RestoreActivityData()
        {
            // code below contains early exits, so make sure we clear the survey state so that nothing old is
            // hanging around
            survey.Restore(null, null);
            survey.Progress = 0f;
            survey.Error = null;
            showActivityColors = false;
            actRangeInitialized = false;
            actRanges.Clear();
            surveyBanks.Clear();
            pendingBanks.Clear();

            var firstProbe = probeGroup.Probe;
            if (firstProbe == null) return;

            var bi = firstProbe.GetContactAnnotation<bool>("survey_bank_included");
            if (bi != null && bi.Length > 0)
            {
                surveyBanks.Clear();
                for (int i = 0; i < allContacts.Count && i < bi.Length; i++)
                    if (bi[i])
                        surveyBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
            }

            DateTimeOffset.TryParse(firstProbe.Annotations.GetAnnotation<string>("survey_datetime"), out var dt);

            // results
            var ampValues  = firstProbe.GetContactAnnotation<float>("survey_amplitude");
            if (ampValues == null || ampValues.Length == 0) return;
            var rateValues = firstProbe.GetContactAnnotation<float>("survey_firerate");
            if (rateValues == null || rateValues.Length == 0) return;
            var noiseArr = firstProbe.GetContactAnnotation<float>("survey_noise");
            if (noiseArr == null || noiseArr.Length == 0) return;

            var thr = firstProbe.Annotations.GetAnnotation<float?>("survey_spike_threshold_uV") ?? -50f;
            var tpb = firstProbe.Annotations.GetAnnotation<float?>("survey_time_per_bank") ?? 5f;

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

            survey.Restore(new NeuropixelsV2eSurveyResults(amp, rate, noise, thr, tpb, surveyBanks), dt);
        }
    }
}
