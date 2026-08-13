using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Dsp;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One probe a headstage can survey: which dialog owns its state/results, and how to reach its decoder on
    /// a freshly-built headstage of this headstage dialog's own concrete type.
    /// </summary>
    internal sealed class NeuropixelsV1SurveyTarget
    {
        internal string Label { get; }
        internal NeuropixelsV1ImGuiDialog Dialog { get; }
        internal Func<MultiDeviceFactory, ConfigureNeuropixelsV1PsbDecoder> SelectDecoder { get; }

        /// <summary>
        /// Whether this target is included in the current/next survey run.
        /// </summary>
        internal bool Selected { get; set; }

        internal NeuropixelsV1SurveyTarget(string label, NeuropixelsV1ImGuiDialog dialog,
            Func<MultiDeviceFactory, ConfigureNeuropixelsV1PsbDecoder> selectDecoder)
        {
            Label = label;
            Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            SelectDecoder = selectDecoder ?? throw new ArgumentNullException(nameof(selectDecoder));
        }
    }

    /// <summary>
    /// Electrode-activity survey runner for NeuropixelsV1-family headstages. Selected probes are surveyed
    /// together, sharing one real hardware pipeline per round: round <c>r</c> configures every selected probe
    /// still having a bank left at index <c>r</c> of its own bank list, and every other decoder on the
    /// headstage (unselected, or already finished) is explicitly disabled .
    /// </summary>
    internal static class NeuropixelsV1HeadstageSurveyRunner
    {
        internal const string GroupName = "NeuropixelsV1Survey";
        const int BufferSize = 300;
        const int RetryDelayMs = 1000;

        // NB: If a probe goes silent during survey stream, e.g. its physically unseated, the hardware layer
        // doesn't throw, so without this timeout the round's Task.WhenAll waits forever with nothing to catch
        // or retry.
        static readonly TimeSpan DataSilenceTimeout = TimeSpan.FromSeconds(1);

        internal static void Start(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixelsV1SurveyTarget> targets,
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
            internal NeuropixelsV1SurveyTarget Target;
            internal List<NeuropixelsV1Bank> OrderedBanks;
            internal int NextIndex;
            internal float?[] AllAmplitude;
            internal float?[] AllFireRate;
            internal float?[] AllNoise;
            internal string RecordingFolder;
            internal string TempProbeFile;
        }

        readonly struct RoundTargetPrep
        {
            internal NeuropixelsV1ProbeGroup BankGroup { get; }
            internal NeuropixelsV1ProbeConfiguration BankConfig { get; }
            internal string RecordingFilePath { get; }

            internal RoundTargetPrep(NeuropixelsV1ProbeGroup bankGroup, NeuropixelsV1ProbeConfiguration bankConfig, string recordingFilePath)
            {
                BankGroup = bankGroup;
                BankConfig = bankConfig;
                RecordingFilePath = recordingFilePath;
            }
        }

        static async Task RunAsync(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixelsV1SurveyTarget> targets,
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

            var states = new Dictionary<NeuropixelsV1SurveyTarget, TargetRunState>();
            foreach (var target in selected)
            {
                var survey = target.Dialog.Survey;
                survey.Status = NeuropixelsV1SurveyStatus.Running;
                survey.Progress = 0f;
                survey.Error = null;
                survey.Results = null;

                var probeGroup = target.Dialog.ProbeGroup;
                int totalContacts = probeGroup.NumberOfContacts;
                var bankValues = Enum.GetValues(typeof(NeuropixelsV1Bank)).Cast<NeuropixelsV1Bank>().ToArray();
                var orderedBanks = bankValues.Where(bank => target.Dialog.SurveyBanks.Contains(bank)).ToList();

                string recordingFolder = null;
                if (recordSurveyData)
                {
                    var probeFile = target.Dialog.ConfigureNeuropixelsV1.ProbeConfiguration.ProbeInterfaceFileName;
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
                var survey = state.Target.Dialog.Survey;
                survey.Results = new NeuropixelsV1SurveyResults(
                    state.AllAmplitude, state.AllFireRate, state.AllNoise,
                    (float)spikeThreshold, timePerBankSeconds, state.Target.Dialog.SurveyBanks);
                survey.Status = NeuropixelsV1SurveyStatus.Completed;
                survey.CompletedAt = DateTimeOffset.Now;
            }

            double? appliedVoltage = null;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var roundTargets = states.Values.Where(s => s.NextIndex < s.OrderedBanks.Count).ToList();
                    if (roundTargets.Count == 0) break;

                    var roundPrep = new Dictionary<NeuropixelsV1SurveyTarget, RoundTargetPrep>();
                    foreach (var state in roundTargets)
                    {
                        var bank = state.OrderedBanks[state.NextIndex];
                        var bankGroup = new NeuropixelsV1ProbeGroup(state.Target.Dialog.ProbeGroup);
                        SelectBankForSurvey(bankGroup, bank);

                        if (bankGroup.ChannelMap.Count == 0)
                        {
                            state.NextIndex++;
                            ReportProgress(state);
                            if (state.NextIndex >= state.OrderedBanks.Count)
                                Complete(state, spikeThreshold, timePerBankSeconds);
                            continue;
                        }

                        var bankConfig = new NeuropixelsV1ProbeConfiguration(state.Target.Dialog.ConfigureNeuropixelsV1.ProbeConfiguration)
                        {
                            SpikeFilter = true // meaningful spike detection requires this
                        };
                        string recordingFilePath = null;
                        if (state.RecordingFolder != null)
                        {
                            var fileBase = $"{DateTime.Now:yyyyMMdd-HHmmss}_{state.Target.Label}_b{bank}";
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

                    Dictionary<NeuropixelsV1SurveyTarget, (float[] Amplitude, float[] FireRate, float[] Noise)> roundResults = null;
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

                        var bank = state.OrderedBanks[state.NextIndex];
                        log($"{target.Label}: bank {bank} complete", false);

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
                    if (state.Target.Dialog.Survey.Status == NeuropixelsV1SurveyStatus.Running)
                        state.Target.Dialog.Survey.Status = NeuropixelsV1SurveyStatus.Idle;
            }
            catch (Exception ex)
            {
                foreach (var state in states.Values)
                    if (state.Target.Dialog.Survey.Status == NeuropixelsV1SurveyStatus.Running)
                    {
                        state.Target.Dialog.Survey.Status = NeuropixelsV1SurveyStatus.Failed;
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

        static async Task<Dictionary<NeuropixelsV1SurveyTarget, (float[] Amplitude, float[] FireRate, float[] Noise)>> CollectRoundStatsAsync(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixelsV1SurveyTarget> allTargets,
            IReadOnlyDictionary<NeuropixelsV1SurveyTarget, RoundTargetPrep> roundPrep,
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
            var active = new List<(NeuropixelsV1SurveyTarget Target, string DeviceName, NeuropixelsV1Gain SpikeGain, string RecordingFilePath)>();
            foreach (var target in allTargets)
            {
                var decoder = target.SelectDecoder(headstage);
                if (roundPrep.TryGetValue(target, out var prep))
                {
                    decoder.Enable = true;
                    decoder.ProbeConfiguration = prep.BankConfig;
                    active.Add((target, decoder.DeviceName, prep.BankConfig.SpikeAmplifierGain, prep.RecordingFilePath));
                }
                else
                {
                    decoder.Enable = false;
                }
            }

            var pipeline = new StartAcquisition().Process(
                headstage.Process(new CreateContext { Driver = driver, Index = hubIndex }.Generate()));

            var tcsMap = active.ToDictionary(a => a.Target,
                _ => new TaskCompletionSource<(float[] Amplitude, float[] FireRate, float[] Noise)>(
                    TaskCreationOptions.RunContinuationsAsynchronously));

            var pipelineSub = pipeline.Subscribe(
                _ => { },
                ex =>
                {
                    foreach (var tcs in tcsMap.Values)
                    {
                        if (cancellationToken.IsCancellationRequested) tcs.TrySetCanceled();
                        else tcs.TrySetException(ex);
                    }
                });

            var dataSubs = new List<IDisposable>();
            foreach (var (target, deviceName, spikeGain, recordingFilePath) in active)
                dataSubs.Add(SubscribeTarget(deviceName, tcsMap[target], spikeGain, spikeThreshold,
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
            NeuropixelsV1Gain spikeAmplifierGain,
            double spikeThreshold,
            float timePerBankSeconds,
            string recordingFilePath,
            CancellationToken cancellationToken)
        {
            var activity = new SpikeActivityAccumulator(NeuropixelsV1.ChannelCount);

            var npxData = new NeuropixelsV1eData { DeviceName = deviceName, BufferSize = BufferSize };
            int framesToCollect = Math.Max(1, (int)Math.Round(timePerBankSeconds * NeuropixelsV1.SamplesPerChannelPerSecond / BufferSize));
            var rawStream = npxData.Generate().Take(framesToCollect).Timeout(DataSilenceTimeout);

            IObservable<NeuropixelsV1DataFrame> dataStream = recordingFilePath != null
                ? DataFrameWriter.DataFrameWriter.WriteBuffered(
                    rawStream,
                    recordingFilePath,
                    Bonsai.IO.PathSuffix.None,
                    buffered: false,
                    overwrite: true,
                    enableCompression: true)
                : rawStream;

            // NB: Shared (not re-subscribed): scaled has two downstream consumers below (DetectSpikes and the
            // Zip), and without Publish/RefCount each would independently re-run the frame buffering in
            // NeuropixelsV1eData.Generate() and the scaling/CMR work in NeuropixelsV1Scale.Process for
            // every frame.
            var scaled = new NeuropixelsV1Scale
            {
                Band = NeuropixelsV1EphysBand.Spike,
                AmplifierGain = spikeAmplifierGain,
                UseCommonMedianReference = true
            }.Process(dataStream).Publish().RefCount();

            // NB: no software bandpass since he spike band is already shaped by hardware (the round's
            // SpikeFilter bit plus the analog front end's inherent ~10 kHz low-pass).
            var spikeStream = new DetectSpikes
            {
                Threshold          = new[] { spikeThreshold },
                Length             = 60,
                Delay              = 15,
                WaveformRefinement = SpikeWaveformRefinement.AlignPeaks
            }.Process(scaled);

            var combined = spikeStream.Zip(scaled, (spikes, scaled) => (spikes, scaled));

            return combined.Subscribe(
                result => activity.Accumulate(result.scaled, result.spikes),
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

        static void SelectBankForSurvey(NeuropixelsV1ProbeGroup group, NeuropixelsV1Bank bank) =>
            group.EnableElectrodes(Enumerable.Range(0, NeuropixelsV1.ElectrodeCount)
                .Where(i => NeuropixelsV1ProbeGroup.GetBank(i) == bank));
    }
}
