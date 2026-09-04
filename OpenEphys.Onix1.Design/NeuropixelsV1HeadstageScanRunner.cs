using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Headstage-level identity scan for NeuropixelsV1e: reads probe part number/serial number, configuring
    /// nothing and streaming no data. Every probe decoder is forced to <c>Enable = false</c>, which skips
    /// its PI-file load, part-number check, and register configuration entirely while leaving the EEPROM
    /// read (and <c>DeviceInfo</c> registration) unaffected.
    /// </summary>
    /// <remarks>
    /// A single pass: a target not detected is reported as such, not retried automatically. The user can
    /// press the scan button again to try once more. If the wrong headstage is attached, <see
    /// cref="ConfigureHeadstageNeuropixelsV1e"/>'s own EEPROM identity check throws, tearing the
    /// <see cref="ContextTask"/> down; that surfaces here as an ordinary <c>scanException</c>, logged, with
    /// every target reported <c>NotDetected</c>.
    /// </remarks>
    internal static class NeuropixelsV1HeadstageScanRunner
    {
        internal static void Start(
            SurveyHeadstageFactory buildHeadstage,
            IReadOnlyList<NeuropixelsV1SurveyTarget> targets,
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
            IReadOnlyList<NeuropixelsV1SurveyTarget> targets,
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
                var resolved = new List<(NeuropixelsV1SurveyTarget Target, string PartNumber, ulong SerialNumber)>();

                var pipelineSub = HeadstageConnection.Configure(headstage, driver, hubIndex, ex => scanException = ex);

                try
                {
                    foreach (var target in targets)
                    {
                        var decoder = target.SelectDecoder(headstage);
                        var info = TryGetDeviceInfo<NeuropixelsV1PsbDecoderDeviceInfo>(decoder.DeviceName);
                        if (info != null)
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
