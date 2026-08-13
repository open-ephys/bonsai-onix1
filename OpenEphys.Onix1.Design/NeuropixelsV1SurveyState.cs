using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1.Design
{
    internal enum NeuropixelsV1SurveyStatus { Idle, Running, Completed, Failed }

    /// <summary>
    /// Immutable per-contact results of a completed survey, together with the parameters that produced
    /// them. Derived metrics and the bank-set copy are computed/taken at construction, so neither the
    /// metrics nor the recorded parameters can ever go stale against the data they describe or against
    /// live, freely-mutable UI state. A null element means the contact was not measured (e.g. its bank
    /// was excluded from the survey).
    /// </summary>
    internal sealed class NeuropixelsV1SurveyResults
    {
        // Results
        internal float?[] ActivityAmplitude { get; }
        internal float?[] ActivityFireRate { get; }
        internal float?[] Noise { get; }
        internal float?[] Snr { get; }
        internal float?[] SnrTimesFireRate { get; }

        // Parameters
        internal float SpikeThresholdUv { get; }
        internal float TimePerBankSeconds { get; }
        internal IReadOnlyCollection<NeuropixelsV1Bank> SurveyBanks { get; }

        internal NeuropixelsV1SurveyResults(
            float?[] amplitude, float?[] fireRate, float?[] noise,
            float spikeThresholdUv, float timePerBankSeconds,
            IReadOnlyCollection<NeuropixelsV1Bank> surveyBanks)
        {
            ActivityAmplitude = amplitude ?? throw new ArgumentNullException(nameof(amplitude));
            ActivityFireRate = fireRate ?? throw new ArgumentNullException(nameof(fireRate));
            Noise = noise ?? throw new ArgumentNullException(nameof(noise));

            Snr = new float?[amplitude.Length];
            SnrTimesFireRate = new float?[amplitude.Length];
            for (int i = 0; i < amplitude.Length; i++)
            {
                Snr[i] = amplitude[i].HasValue && noise[i].HasValue && noise[i].Value > 0f
                    ? amplitude[i].Value / noise[i].Value : null;
                SnrTimesFireRate[i] = Snr[i].HasValue && fireRate[i].HasValue
                    ? Snr[i].Value * fireRate[i].Value : null;
            }

            SpikeThresholdUv = spikeThresholdUv;
            TimePerBankSeconds = timePerBankSeconds;
            SurveyBanks = new HashSet<NeuropixelsV1Bank>(surveyBanks ?? Array.Empty<NeuropixelsV1Bank>());
        }
    }

    internal class NeuropixelsV1SurveyState
    {
        internal NeuropixelsV1SurveyStatus Status { get; set; } = NeuropixelsV1SurveyStatus.Idle;
        internal NeuropixelsV1SurveyResults Results { get; set; }
        internal float Progress { get; set; }
        internal string Error { get; set; }
        internal DateTimeOffset? CompletedAt { get; set; }
        internal bool IsStale =>
            CompletedAt.HasValue && (DateTimeOffset.Now - CompletedAt.Value).TotalHours > 24;
    }
}
