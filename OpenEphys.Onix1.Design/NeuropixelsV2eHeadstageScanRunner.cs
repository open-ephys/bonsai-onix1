using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Headstage-level identity scan shared by every NeuropixelsV2e-family headstage: reads probe part
    /// number/serial number, configuring nothing and streaming no data. Every probe decoder is forced to
    /// <c>Enable = false</c>, which already means "read EEPROM, register <c>DeviceInfo</c>, touch nothing
    /// else" for V2.
    /// </summary>
    /// <remarks>
    /// A single pass: a target not detected is reported as such, not retried automatically. The user can
    /// press the scan button again to try once more. If the wrong headstage is attached, the headstage's
    /// own EEPROM identity check throws, tearing the <see cref="ContextTask"/> down; that surfaces here as
    /// an ordinary <c>scanException</c>, logged, with every target reported <c>NotDetected</c>.
    /// NeuropixelsV2Rhd2000e reuses this runner and its target type without any special handling.
    /// </remarks>
    internal static class NeuropixelsV2eHeadstageScanRunner
    {
        internal static void Start(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixeslV2eSurveyTarget> targets,
            string driver,
            int hubIndex,
            PortName port,
            double? portVoltage,
            Action<string, bool> log,
            CancellationToken cancellationToken)
        {
            Task.Run(() => RunAsync(buildHeadstage, targets, driver, hubIndex, port, portVoltage, log, cancellationToken));
        }

        static void RunAsync(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixeslV2eSurveyTarget> targets,
            string driver,
            int hubIndex,
            PortName port,
            double? portVoltage,
            Action<string, bool> log,
            CancellationToken cancellationToken)
        {
            foreach (var target in targets)
                target.Dialog.Scan.Status = NeuropixelsV1ScanStatus.Running;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (headstage, _) = buildHeadstage(port, portVoltage);
                foreach (var target in targets)
                    target.SelectDecoder(headstage).Enable = false;

                Exception scanException = null;
                var resolved = new List<(NeuropixeslV2eSurveyTarget Target, string PartNumber, ulong? SerialNumber)>();

                const int ReadSize = 1200;  // Minimal for NeuropixeslV2eHeadstage
                var pipelineSub = HeadstageConnection.Configure(headstage, driver, hubIndex, ReadSize, ex => scanException = ex);

                try
                {
                    foreach (var target in targets)
                    {
                        var decoder = target.SelectDecoder(headstage);
                        var info = TryGetDeviceInfo<NeuropixelsV2PsbDecoderDeviceInfo>(decoder.DeviceName);
                        if (info != null && info.ProbeSerialNumber.HasValue)
                            resolved.Add((target, info.ProbePartNumber, info.ProbeSerialNumber));
                    }
                }
                finally
                {
                    // NB: blocks until the ContextTask fully tears down. Every Scan.Status write below must
                    // stay after this returns: the panel re-enables its buttons the instant a target's
                    // status leaves Running, and a click that races a still-in-progress hardware teardown
                    // throws ONIException on the next context.
                    pipelineSub.Dispose();
                }

                var resolvedTargets = resolved.Select(r => r.Target).ToHashSet();
                foreach (var (target, partNumber, serialNumber) in resolved)
                {
                    target.Dialog.Scan.Complete(partNumber, serialNumber);
                    log($"{target.Label}: detected {partNumber} (SN {serialNumber})", false);
                }

                if (scanException != null)
                    log($"Scan [{scanException.GetType().Name}]: {scanException.Message}.", true);

                foreach (var target in targets.Where(t => !resolvedTargets.Contains(t)))
                    target.Dialog.Scan.NotDetected();
            }
            catch (OperationCanceledException)
            {
                foreach (var target in targets.Where(t => t.Dialog.Scan.Status == NeuropixelsV1ScanStatus.Running))
                    target.Dialog.Scan.Status = NeuropixelsV1ScanStatus.Idle;
            }
        }

        static T TryGetDeviceInfo<T>(string deviceName) where T : DeviceInfo
        {
            T result = null;
            DeviceManager.GetDevice(deviceName).Subscribe(info => result = info as T, _ => { });
            return result;
        }
    }
}
