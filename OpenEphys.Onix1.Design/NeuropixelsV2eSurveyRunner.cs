using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Dsp;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    internal static class NeuropixelsV2eSurveyRunner
    {
        const int BufferSize = 300;
        const string GroupName = "NeuropixelsV2eSurvey";
        const int RetryDelayMs = 1000;

        internal static void Start(
            NeuropixelsV2ProbeGroup probeGroup,
            NeuropixelsV2ProbeConfiguration probeConfig,
            NeuropixelsV2eSurveyState survey,
            Action<string, bool> log,
            string driver,
            int hubIndex,
            PortName port,
            bool isProbeA,
            double? portVoltage,
            double spikeThreshold,
            bool useBandpassFilter,
            float timePerBankSeconds,
            IReadOnlyCollection<(int shank, NeuropixelsV2Bank bank)> surveyBanks,
            string recordingFolder,
            CancellationToken cancellationToken)
        {
            Task.Run(() => RunAsync(
                probeGroup, probeConfig, survey, log,
                driver, hubIndex, port, isProbeA, portVoltage,
                spikeThreshold, useBandpassFilter, timePerBankSeconds, surveyBanks,
                recordingFolder, cancellationToken));
        }

        static async Task RunAsync(
            NeuropixelsV2ProbeGroup probeGroup,
            NeuropixelsV2ProbeConfiguration probeConfig,
            NeuropixelsV2eSurveyState survey,
            Action<string, bool> log,
            string driver,
            int hubIndex,
            PortName port,
            bool isProbeA,
            double? portVoltageOverride,
            double spikeThreshold,
            bool useBandpassFilter,
            float timePerBankSeconds,
            IReadOnlyCollection<(int shank, NeuropixelsV2Bank bank)> surveyBanks,
            string recordingFolder,
            CancellationToken cancellationToken)
        {
            survey.Status = NeuropixelsV2eSurveyStatus.Running;
            survey.Progress = 0f;
            survey.Error = null;
            survey.Results = null;

            var tempProbeFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + ".json");

            try
            {
                int totalContacts = probeGroup.NumberOfContacts;
                var allAmplitude = new float?[totalContacts];
                var allFireRate = new float?[totalContacts];
                var allNoise = new float?[totalContacts];

                var shanks = Enumerable.Range(0, totalContacts)
                    .Select(i => probeGroup.GetShank(i))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                var banks = Enum.GetValues(typeof(NeuropixelsV2Bank)).Cast<NeuropixelsV2Bank>().ToArray();
                int totalRuns = shanks.Sum(s => banks.Count(b => surveyBanks.Contains((s, b))));
                if (totalRuns == 0) totalRuns = 1;
                int runsDone = 0;

                if (recordingFolder != null)
                    System.IO.Directory.CreateDirectory(recordingFolder);

                double? appliedVoltage = null;

                for (int si = 0; si < shanks.Count; si++)
                {
                    var shank = shanks[si];
                    for (int bi = 0; bi < banks.Length; bi++)
                    {
                        var bank = banks[bi];
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!surveyBanks.Contains((shank, bank)))
                            continue;

                        var bankGroup = CloneProbeGroup(probeGroup);
                        bankGroup.SelectBank(shank, bank);

                        if (bankGroup.ChannelMap.Count == 0)
                        {
                            survey.Progress = (float)(++runsDone) / totalRuns;
                            continue;
                        }

                        string bankFilePath = null;
                        var bankConfig = probeConfig.Clone();

                        if (recordingFolder != null)
                        {
                            var fileBase = $"{DateTime.Now:yyyyMMdd-HHmmss}_s{shank}_b{bank}";
                            bankFilePath = System.IO.Path.Combine(recordingFolder, $"{fileBase}.arrow");
                            var piFile = System.IO.Path.Combine(recordingFolder, $"{fileBase}.json");
                            ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(bankGroup, piFile);
                            bankConfig.ProbeInterfaceFileName = piFile;
                        }
                        else
                        {
                            ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(bankGroup, tempProbeFile);
                            bankConfig.ProbeInterfaceFileName = tempProbeFile;
                        }

                        (float[] Amplitude, float[] FireRate, float[] Noise) bankStats = default;
                        for (int attempt = 0; ; attempt++) // Only exit via Cancel
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            try
                            {
                                var (Amplitude, FireRate, Noise, AppliedVoltage) = await CollectBankStatsAsync(
                                    driver, hubIndex, port, isProbeA,
                                    bankConfig,
                                    appliedVoltage ?? portVoltageOverride,
                                    spikeThreshold, useBandpassFilter, timePerBankSeconds,
                                    bankFilePath, cancellationToken);

                                bankStats = (Amplitude, FireRate, Noise);
                                appliedVoltage = AppliedVoltage;

                                log($"Shank {shank}, bank {bank}: survey complete", false);
                                break;
                            }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception ex)
                            {
                                log($"Shank {shank}, bank {bank}, attempt {attempt + 1} [{ex.GetType().Name}]: {ex.Message}. Retrying...", true);
                                await Task.Delay(RetryDelayMs, cancellationToken);
                            }
                        }

                        foreach (var kvp in bankGroup.ChannelMap)
                        {
                            int ch = kvp.Key;
                            int idx = kvp.Value;
                            allAmplitude[idx] = bankStats.Amplitude[ch];
                            allFireRate[idx] = bankStats.FireRate[ch];
                            allNoise[idx] = bankStats.Noise[ch];
                        }

                        survey.Progress = (float)(++runsDone) / totalRuns;
                    }
                }

                survey.Results = new NeuropixelsV2eSurveyResults(
                    allAmplitude, allFireRate, allNoise,
                    (float)spikeThreshold, useBandpassFilter, timePerBankSeconds, surveyBanks);
                survey.Status = NeuropixelsV2eSurveyStatus.Completed;
                survey.CompletedAt = DateTimeOffset.Now;
            }
            catch (OperationCanceledException)
            {
                survey.Status = NeuropixelsV2eSurveyStatus.Idle;
            }
            catch (Exception ex)
            {
                survey.Status = NeuropixelsV2eSurveyStatus.Failed;
                survey.Error = ex.Message;
            }
            finally
            {
                if (System.IO.File.Exists(tempProbeFile))
                    System.IO.File.Delete(tempProbeFile);
            }
        }

        static NeuropixelsV2ProbeGroup CloneProbeGroup(NeuropixelsV2ProbeGroup source)
        {
            if (source is NeuropixelsV2QuadShankProbeGroup quad)
                return new NeuropixelsV2QuadShankProbeGroup(quad);
            return new NeuropixelsV2SingleShankProbeGroup((NeuropixelsV2SingleShankProbeGroup)source);
        }

        static async Task<(float[] Amplitude, float[] FireRate, float[] Noise, double? AppliedVoltage)> CollectBankStatsAsync(
            string driver,
            int hubIndex,
            PortName port,
            bool isProbeA, // HACK: see TODO below
            NeuropixelsV2ProbeConfiguration bankConfig,
            double? previouslyAppliedVoltage,
            double spikeThreshold,
            bool useBandpassFilter,
            float timePerBankSeconds,
            string recordingFilePath,
            CancellationToken cancellationToken)
        {
            // NB: RunContinuationsAsynchronously prevents the TrySetResult continuation from running
            // synchronously on the distributeFrames thread, which would deadlock because Dispose() waits for
            // that same thread to exit.
            var tcs = new TaskCompletionSource<(float[] Amplitude, float[] FireRate, float[] Noise)>(TaskCreationOptions.RunContinuationsAsynchronously);
            var ampSum = new float[NeuropixelsV2.ChannelCount];
            var spikeCount = new int[NeuropixelsV2.ChannelCount];
            var medianAbsDeviation = new P2Median[NeuropixelsV2.ChannelCount];
            for (int i = 0; i < medianAbsDeviation.Length; i++) medianAbsDeviation[i] = new P2Median();


            // TODO: This block needs to be moved to the headstage dialog level along with the survey
            // controls. The reference to the probeDecoder can be provided in place of isProbeA, which is
            // a hack.
            // ------------------------------------------------------------------------------------------------
            var headstage = new ConfigureHeadstageNeuropixelsV2e
            {
                Name = GroupName,
                Port = port,
                PortVoltage = new AutoPortVoltage(previouslyAppliedVoltage)
            };

            var probeDecoder = isProbeA ? headstage.NeuropixelsV2A : headstage.NeuropixelsV2B;
            probeDecoder.ProbeConfiguration = bankConfig;

            var otherDecoder = isProbeA ? headstage.NeuropixelsV2B : headstage.NeuropixelsV2A;
            otherDecoder.Enable = false;

            var deviceName = isProbeA
                ? $"{GroupName}/NeuropixelsV2A"
                : $"{GroupName}/NeuropixelsV2B";

            var pipeline = new StartAcquisition().Process(
                headstage.Process(
                    new CreateContext { Driver = driver, Index = hubIndex }.Generate()));
            // ------------------------------------------------------------------------------------------------

            var npxData = new NeuropixelsV2eData { DeviceName = deviceName, BufferSize = BufferSize };

            int framesToCollect = Math.Max(1, (int)Math.Round(timePerBankSeconds * NeuropixelsV2.SamplesPerChannelPerSecond / BufferSize));
            var rawStream = npxData.Generate().Take(framesToCollect);

            IObservable<NeuropixelsV2DataFrame> dataStream = recordingFilePath != null
                ? new DataFrameWriter.DataFrameWriter
                {
                      FileName          = recordingFilePath,
                      Suffix            = Bonsai.IO.PathSuffix.None,
                      Buffered          = false,
                      Overwrite         = true,
                      EnableCompression = true
                  }.Process(rawStream.Cast<BufferedDataFrame>()).Cast<NeuropixelsV2DataFrame>()
                : rawStream;

            var ampStream = dataStream.Select(frame => frame.AmplifierData);

            var scaled = new NeuropixelsV2Scale{ UseCommonMedianReference = true}.Process(dataStream);

            IObservable<Mat> filtered = useBandpassFilter
                ? new Butterworth
                  {
                      SampleRate  = NeuropixelsV2.SamplesPerChannelPerSecond,
                      Cutoff1     = 300.0,
                      Cutoff2     = 6000.0,
                      FilterType  = FilterType.BandPass,
                      FilterOrder = 2
                  }.Process(scaled)
                : scaled;

            var filteredWithNoiseTap = filtered.Do(mat =>
            {
                for (int ch = 0; ch < mat.Rows; ch++)
                    for (int s = 0; s < mat.Cols; s++)
                        medianAbsDeviation[ch].Update(Math.Abs((float)mat.GetReal(ch, s)));
            });

            var spikeStream = new DetectSpikes
            {
                Threshold          = new[] { spikeThreshold },
                Length             = 60,
                Delay              = 15,
                WaveformRefinement = SpikeWaveformRefinement.AlignPeaks
            }.Process(filteredWithNoiseTap);

            IDisposable pipelineSub = null;
            IDisposable dataSub = null;

            pipelineSub = pipeline.Subscribe(
                _ => { },
                ex =>
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetException(ex);
                });

            double? appliedVoltage = headstage.PortVoltage.Applied;

            dataSub = spikeStream.Subscribe(
                spikes => AccumulateSpikeStats(spikes, ampSum, spikeCount),
                ex =>
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetException(ex);
                },
                () =>
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetResult(ComputeStats(ampSum, spikeCount, timePerBankSeconds, medianAbsDeviation));
                });

            var cancelReg = cancellationToken.Register(() =>
            {
                dataSub?.Dispose();
                tcs.TrySetCanceled();
            });

            try
            {
                var stats = await tcs.Task;
                return (stats.Amplitude, stats.FireRate, stats.Noise, appliedVoltage);
            }
            finally
            {
                cancelReg.Dispose();
                dataSub?.Dispose();
                pipelineSub?.Dispose();
            }
        }

        static void AccumulateSpikeStats(SpikeWaveformCollection spikeCollection, float[] peakToPeakSum, int[] spikeCount)
        {
            foreach (var spike in spikeCollection)
            {
                CV.MinMaxLoc(spike.Waveform, out double minVal, out double maxVal, out _, out _);
                peakToPeakSum[spike.ChannelIndex] += (float)Math.Abs(maxVal - minVal);
                spikeCount[spike.ChannelIndex]++;
            }
        }

        static (float[] Amplitude, float[] FireRate, float[] Noise) ComputeStats(
            float[] peakToPeakSum, int[] spikeCount, double durationSeconds, P2Median[] medianAbsDeviation)
        {
            int n = peakToPeakSum.Length;
            var amplitude = new float[n];
            var fireRate = new float[n];
            var noise = new float[n];
            for (int ch = 0; ch < n; ch++)
            {
                amplitude[ch] = spikeCount[ch] > 0 ? peakToPeakSum[ch] / spikeCount[ch] : 0f;
                fireRate[ch] = (float)(spikeCount[ch] / durationSeconds);
                noise[ch] = medianAbsDeviation[ch].Median / 0.6745f;
            }
            return (amplitude, fireRate, noise);
        }
    }
}
