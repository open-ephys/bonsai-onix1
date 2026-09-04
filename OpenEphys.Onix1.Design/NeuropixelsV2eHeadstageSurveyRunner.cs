using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Dsp;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One probe a headstage can survey: which dialog owns its state/results, and how to reach its decoder on
    /// a freshly-built headstage of this headstage dialog's own concrete type.
    /// </summary>
    internal sealed class NeuropixeslV2eSurveyTarget
    {
        internal string Label { get; }
        internal NeuropixelsV2eImGuiDialog Dialog { get; }
        internal Func<MultiDeviceFactory, ConfigureNeuropixelsV2PsbDecoder> SelectDecoder { get; }

        /// <summary>
        /// Whether this target is included in the current/next survey run.
        /// </summary>
        internal bool Selected { get; set; }

        internal NeuropixeslV2eSurveyTarget(string label, NeuropixelsV2eImGuiDialog dialog,
            Func<MultiDeviceFactory, ConfigureNeuropixelsV2PsbDecoder> selectDecoder)
        {
            Label = label;
            Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            SelectDecoder = selectDecoder ?? throw new ArgumentNullException(nameof(selectDecoder));
        }
    }

    /// <summary>
    /// Builds and returns a fresh headstage (<c>Name</c>/<c>Port</c>/<c>PortVoltage</c> already set) for one
    /// survey round, together with the <see cref="AutoPortVoltage"/> instance assigned to it (its <c>Applied</c>
    /// value is only readable after the pipeline built from this headstage has been subscribed to). The runner
    /// never references a concrete headstage type or decoder property name directly -- only through
    /// <see cref="NeuropixeslV2eSurveyTarget.SelectDecoder"/>.
    /// </summary>
    internal delegate (MultiDeviceFactory Headstage, AutoPortVoltage PortVoltage) SurveyHeadstageFactory(
        PortName port, double? portVoltageOverride);

    /// <summary>
    /// Concurrent, round-based electrode-activity survey runner shared by every NeuropixelsV2e-family
    /// headstage dialog. Selected probes are surveyed together, sharing one real hardware pipeline per
    /// round: round <c>r</c> configures every selected probe still having a bank left at index <c>r</c> of
    /// its own bank list, and every other decoder on the headstage (unselected, or already finished) is
    /// explicitly disabled -- a probe that isn't part of a round must never be left enabled by default.
    /// </summary>
    internal static class NeuropixelsV2eHeadstageSurveyRunner
    {
        internal const string GroupName = "NeuropixelsV2eSurvey";
        const int BufferSize = 300;
        const int RetryDelayMs = 1000;

        // NB: If a probe goes silent during survey stream, e.g. its physically unseated), the hardware layer
        // doesn't throw, so without this timeout the round's Task.WhenAll waits forever with nothing to catch
        // or retry.
        static readonly TimeSpan DataSilenceTimeout = TimeSpan.FromSeconds(1);

        internal static void Start(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixeslV2eSurveyTarget> targets,
            string driver,
            int hubIndex,
            PortName port,
            double? portVoltage,
            double spikeThreshold,
            float timePerBankSeconds,
            bool recordSurveyData,
            Action<string, bool> log,
            CancellationToken cancellationToken)
        {
            Task.Run(() => RunAsync(
                buildHeadstage, targets, driver, hubIndex, port, portVoltage,
                spikeThreshold, timePerBankSeconds, recordSurveyData,
                log, cancellationToken));
        }

        sealed class TargetRunState
        {
            internal NeuropixeslV2eSurveyTarget Target;
            internal List<(int shank, int bank)> OrderedBanks;
            internal int NextIndex;
            internal float?[] AllAmplitude;
            internal float?[] AllFireRate;
            internal float?[] AllNoise;
            internal string RecordingFolder;
            internal string TempProbeFile;
        }

        readonly struct RoundTargetPrep
        {
            internal NeuropixelsV2ProbeGroup BankGroup { get; }
            internal NeuropixelsV2ProbeConfiguration BankConfig { get; }
            internal string RecordingFilePath { get; }

            internal RoundTargetPrep(NeuropixelsV2ProbeGroup bankGroup, NeuropixelsV2ProbeConfiguration bankConfig, string recordingFilePath)
            {
                BankGroup = bankGroup;
                BankConfig = bankConfig;
                RecordingFilePath = recordingFilePath;
            }
        }

        static async Task RunAsync(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixeslV2eSurveyTarget> targets,
            string driver,
            int hubIndex,
            PortName port,
            double? portVoltageOverride,
            double spikeThreshold,
            float timePerBankSeconds,
            bool recordSurveyData,
            Action<string, bool> log,
            CancellationToken cancellationToken)
        {
            var selected = targets.Where(t => t.Selected).ToList();
            if (selected.Count == 0) return;

            var states = new Dictionary<NeuropixeslV2eSurveyTarget, TargetRunState>();
            foreach (var target in selected)
            {
                var survey = target.Dialog.Survey;
                survey.Status = NeuropixelsV2eSurveyStatus.Running;
                survey.Progress = 0f;
                survey.Error = null;
                survey.Results = null;

                var probeGroup = target.Dialog.ProbeGroup;
                int totalContacts = probeGroup.NumberOfContacts;
                var shanks = Enumerable.Range(0, totalContacts)
                    .Select(i => probeGroup.GetShank(i))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                var orderedBanks = new List<(int shank, int bank)>();
                foreach (var shank in shanks)
                    for (int bank = 0; bank < probeGroup.BankCount; bank++)
                        if (target.Dialog.SurveyBanks.Contains((shank, bank)))
                            orderedBanks.Add((shank, bank));

                string recordingFolder = null;
                if (recordSurveyData)
                {
                    var probeFile = target.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.ProbeInterfaceFileName;
                    if (!string.IsNullOrEmpty(probeFile))
                    {
                        var dir = Path.GetDirectoryName(probeFile);
                        var name = Path.GetFileNameWithoutExtension(probeFile);
                        recordingFolder = Path.Combine(dir, $"{name}_{DateTime.Now:yyyyMMdd-HHmmss}_survey-data");
                        Directory.CreateDirectory(recordingFolder);
                    }
                }

                states[target] = new TargetRunState
                {
                    Target = target,
                    OrderedBanks = orderedBanks,
                    NextIndex = 0,
                    AllAmplitude = new float?[totalContacts],
                    AllFireRate = new float?[totalContacts],
                    AllNoise = new float?[totalContacts],
                    RecordingFolder = recordingFolder,
                    TempProbeFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"),
                };
            }

            static void ReportProgress(TargetRunState state)
            {
                int totalRuns = Math.Max(1, state.OrderedBanks.Count);
                state.Target.Dialog.Survey.Progress = (float)state.NextIndex / totalRuns;
            }

            static void Complete(TargetRunState state, double spikeThreshold, float timePerBankSeconds)
            {
                state.Target.Dialog.Survey.Complete(new NeuropixelsV2eSurveyResults(
                    state.AllAmplitude, state.AllFireRate, state.AllNoise,
                    (float)spikeThreshold, timePerBankSeconds, state.Target.Dialog.SurveyBanks));
            }

            double? appliedVoltage = null;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var roundTargets = states.Values.Where(s => s.NextIndex < s.OrderedBanks.Count).ToList();
                    if (roundTargets.Count == 0) break;

                    var roundPrep = new Dictionary<NeuropixeslV2eSurveyTarget, RoundTargetPrep>();
                    foreach (var state in roundTargets)
                    {
                        var (shank, bank) = state.OrderedBanks[state.NextIndex];
                        var bankGroup = CloneProbeGroup(state.Target.Dialog.ProbeGroup);
                        bankGroup.SelectBank(shank, bank);

                        if (bankGroup.ChannelMap.Count == 0)
                        {
                            state.NextIndex++;
                            ReportProgress(state);
                            if (state.NextIndex >= state.OrderedBanks.Count)
                                Complete(state, spikeThreshold, timePerBankSeconds);
                            continue;
                        }

                        var bankConfig = state.Target.Dialog.ConfigureNeuropixelsV2.ProbeConfiguration.Clone();
                        string recordingFilePath = null;
                        if (state.RecordingFolder != null)
                        {
                            var fileBase = $"{DateTime.Now:yyyyMMdd-HHmmss}_{state.Target.Label}_s{shank}_b{Neuropixels.BankDisplayName(bank)}";
                            recordingFilePath = Path.Combine(state.RecordingFolder, $"{fileBase}.arrow");
                            var piFile = Path.Combine(state.RecordingFolder, $"{fileBase}.json");
                            ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(bankGroup, piFile);
                            bankConfig.ProbeInterfaceFileName = piFile;
                        }
                        else
                        {
                            ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(bankGroup, state.TempProbeFile);
                            bankConfig.ProbeInterfaceFileName = state.TempProbeFile;
                        }

                        roundPrep[state.Target] = new RoundTargetPrep(bankGroup, bankConfig, recordingFilePath);
                    }

                    if (roundPrep.Count == 0) continue; // everyone active this round was an empty-bank skip

                    Dictionary<NeuropixeslV2eSurveyTarget, (float[] Amplitude, float[] FireRate, float[] Noise)> roundResults = null;
                    for (int attempt = 0; ; attempt++) // only exit via Cancel or break
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            roundResults = await CollectRoundStatsAsync(
                                buildHeadstage, targets, roundPrep, driver, hubIndex, port,
                                appliedVoltage ?? portVoltageOverride, spikeThreshold,
                                timePerBankSeconds, cancellationToken, v => appliedVoltage = v);
                            break;
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            log($"Survey round, attempt {attempt + 1} [{ex.GetType().Name}]: {ex.Message}. Retrying...", true);
                            await Task.Delay(RetryDelayMs, cancellationToken);
                        }
                    }

                    foreach (var roundPrepEntry in roundPrep)
                    {
                        var target = roundPrepEntry.Key;
                        var prep = roundPrepEntry.Value;
                        var state = states[target];
                        var stats = roundResults[target];
                        foreach (var kvp in prep.BankGroup.ChannelMap)
                        {
                            int ch = kvp.Key;
                            int idx = kvp.Value;
                            state.AllAmplitude[idx] = stats.Amplitude[ch];
                            state.AllFireRate[idx] = stats.FireRate[ch];
                            state.AllNoise[idx] = stats.Noise[ch];
                        }

                        var (shank, bank) = state.OrderedBanks[state.NextIndex];
                        log($"{target.Label}: shank {shank}, bank {Neuropixels.BankDisplayName(bank)} complete", false);

                        state.NextIndex++;
                        ReportProgress(state);

                        if (state.NextIndex >= state.OrderedBanks.Count)
                            Complete(state, spikeThreshold, timePerBankSeconds);
                    }
                }

                // Targets with nothing selected to survey (e.g. an empty SurveyBanks) never entered the round
                // loop at all. Finalize them here with empty-but-valid results.
                foreach (var state in states.Values.Where(s => s.OrderedBanks.Count == 0))
                    Complete(state, spikeThreshold, timePerBankSeconds);
            }
            catch (OperationCanceledException)
            {
                foreach (var state in states.Values)
                    if (state.Target.Dialog.Survey.Status == NeuropixelsV2eSurveyStatus.Running)
                        state.Target.Dialog.Survey.Status = NeuropixelsV2eSurveyStatus.Idle;
            }
            catch (Exception ex)
            {
                foreach (var state in states.Values)
                    if (state.Target.Dialog.Survey.Status == NeuropixelsV2eSurveyStatus.Running)
                    {
                        state.Target.Dialog.Survey.Status = NeuropixelsV2eSurveyStatus.Failed;
                        state.Target.Dialog.Survey.Error = ex.Message;
                    }
            }
            finally
            {
                foreach (var state in states.Values)
                    if (File.Exists(state.TempProbeFile))
                        File.Delete(state.TempProbeFile);
            }
        }

        static async Task<Dictionary<NeuropixeslV2eSurveyTarget, (float[] Amplitude, float[] FireRate, float[] Noise)>> CollectRoundStatsAsync(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixeslV2eSurveyTarget> allTargets,
            IReadOnlyDictionary<NeuropixeslV2eSurveyTarget, RoundTargetPrep> roundPrep,
            string driver,
            int hubIndex,
            PortName port,
            double? previouslyAppliedVoltage,
            double spikeThreshold,
            float timePerBankSeconds,
            CancellationToken cancellationToken,
            Action<double?> onAppliedVoltage)
        {
            var (headstage, portVoltage) = buildHeadstage(port, previouslyAppliedVoltage);

            // Every decoder this headstage has must be explicitly touched: enabled + configured if it's
            // active this round, or explicitly disabled otherwise.
            var active = new List<(NeuropixeslV2eSurveyTarget Target, string DeviceName, string RecordingFilePath)>();
            foreach (var target in allTargets)
            {
                var decoder = target.SelectDecoder(headstage);
                if (roundPrep.TryGetValue(target, out var prep))
                {
                    decoder.Enable = true;
                    decoder.ProbeConfiguration = prep.BankConfig;
                    active.Add((target, decoder.DeviceName, prep.RecordingFilePath));
                }
                else
                {
                    decoder.Enable = false;
                }
            }

            var tcsMap = active.ToDictionary(a => a.Target,
                _ => new TaskCompletionSource<(float[] Amplitude, float[] FireRate, float[] Noise)>(
                    TaskCreationOptions.RunContinuationsAsynchronously));

            var pipelineSub = HeadstageConnection.Configure(headstage, driver, hubIndex, ex =>
            {
                foreach (var tcs in tcsMap.Values)
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetException(ex);
                }
            });

            var dataSubs = new List<IDisposable>();
            foreach (var (target, deviceName, recordingFilePath) in active)
                dataSubs.Add(SubscribeTarget(deviceName, tcsMap[target], spikeThreshold,
                    timePerBankSeconds, recordingFilePath, cancellationToken));

            var cancelReg = cancellationToken.Register(() =>
            {
                foreach (var s in dataSubs) s.Dispose();
                foreach (var tcs in tcsMap.Values) tcs.TrySetCanceled();
            });

            try
            {
                var stats = await Task.WhenAll(tcsMap.Values.Select(t => t.Task));
                onAppliedVoltage(portVoltage.Applied);
                return tcsMap.Keys.Zip(stats, (target, s) => (target, s))
                    .ToDictionary(x => x.target, x => x.s);
            }
            finally
            {
                cancelReg.Dispose();
                foreach (var s in dataSubs) s.Dispose();
                pipelineSub.Dispose(); // NB: blocks until the ContextTask fully tears down. Safe to build the next round right after
            }
        }

        static IDisposable SubscribeTarget(
            string deviceName,
            TaskCompletionSource<(float[] Amplitude, float[] FireRate, float[] Noise)> tcs,
            double spikeThreshold,
            float timePerBankSeconds,
            string recordingFilePath,
            CancellationToken cancellationToken)
        {
            var activity = new SpikeActivityAccumulator(NeuropixelsV2.ChannelCount);

            var npxData = new NeuropixelsV2eData { DeviceName = deviceName, BufferSize = BufferSize };
            int framesToCollect = Math.Max(1, (int)Math.Round(timePerBankSeconds * NeuropixelsV2.SamplesPerChannelPerSecond / BufferSize));
            var rawStream = npxData.Generate().Take(framesToCollect).Timeout(DataSilenceTimeout);

            IObservable<NeuropixelsV2DataFrame> dataStream = recordingFilePath != null
                ? DataFrameWriter.DataFrameWriter.WriteBuffered(
                    rawStream,
                    recordingFilePath,
                    Bonsai.IO.PathSuffix.None,
                    buffered: false,
                    overwrite: true,
                    enableCompression: true)
                : rawStream;

            var scaled = new NeuropixelsV2Scale { UseCommonMedianReference = true }.Process(dataStream);

            IObservable<Mat> filtered = new Butterworth
            {
                SampleRate  = NeuropixelsV2.SamplesPerChannelPerSecond,
                Cutoff1     = 300.0,
                Cutoff2     = 6000.0,
                FilterType  = FilterType.BandPass,
                FilterOrder = 2
            }.Process(scaled).Publish().RefCount();

            var spikeStream = new DetectSpikes
            {
                Threshold          = new[] { spikeThreshold },
                Length             = 60,
                Delay              = 15,
                WaveformRefinement = SpikeWaveformRefinement.AlignPeaks
            }.Process(filtered);

            var combined = spikeStream.Zip(filtered, (spikes, filtered) => (spikes, filtered));

            return combined.Subscribe(
                result => activity.Accumulate(result.filtered, result.spikes),
                ex =>
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetException(ex);
                },
                () =>
                {
                    if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                    else tcs.TrySetResult(activity.Summarize(timePerBankSeconds));
                });
        }

        static NeuropixelsV2ProbeGroup CloneProbeGroup(NeuropixelsV2ProbeGroup source) => new(source);
    }
}
