using System;

namespace OpenEphys.Onix1.Design
{
    internal enum NeuropixelsV1ScanStatus { Idle, Running, Detected, NotDetected }

    /// <summary>
    /// This probe's own headstage-scan result/status. Written into by the headstage-level scan runner,
    /// read by this dialog's control panel. Shared by every NeuropixelsV1/V2 probe family, since a scan
    /// result is just an identity pair with no per-family shape difference (unlike survey results).
    /// </summary>
    internal sealed class NeuropixelsV1ScanState
    {
        /// <summary>
        /// Raised by <see cref="Complete"/> or <see cref="NotDetected"/> only, i.e. when the scan runner
        /// finishes a genuine new attempt for this target.
        /// </summary>
        internal event Action ScanCompleted;

        internal NeuropixelsV1ScanStatus Status { get; set; } = NeuropixelsV1ScanStatus.Idle;
        internal string PartNumber { get; set; }
        internal ulong? SerialNumber { get; set; }
        internal DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// Records a successfully identified probe.
        /// </summary>
        internal void Complete(string partNumber, ulong? serialNumber)
        {
            PartNumber = partNumber;
            SerialNumber = serialNumber;
            Status = NeuropixelsV1ScanStatus.Detected;
            CompletedAt = DateTimeOffset.Now;
            ScanCompleted?.Invoke();
        }

        /// <summary>
        /// Records that the scan completed without finding this probe.
        /// </summary>
        internal void NotDetected()
        {
            PartNumber = null;
            SerialNumber = null;
            Status = NeuropixelsV1ScanStatus.NotDetected;
            CompletedAt = DateTimeOffset.Now;
            ScanCompleted?.Invoke();
        }
    }
}
