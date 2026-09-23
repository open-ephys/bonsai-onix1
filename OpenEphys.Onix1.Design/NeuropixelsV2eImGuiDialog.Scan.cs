using System;

namespace OpenEphys.Onix1.Design
{
    // Cross-checks the headstage-level scan's result against whatever probe interface file is loaded in
    // this dialog. Running/configuring the scan itself is a headstage-level concern: see
    // NeuropixelsV2eHeadstageControlPanel / NeuropixelsV2eHeadstageScanRunner.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void OnScanCompleted()
        {
            if (scan.Status == NeuropixelsV1ScanStatus.Detected)
                CheckScannedPartNumberAgainstLoadedFile();
        }

        void CheckScannedPartNumberAgainstLoadedFile()
        {
            if (scan.Status != NeuropixelsV1ScanStatus.Detected) return;

            if (!TryValidatePartNumber(scan.PartNumber, probeGroup, out var mismatchMessage))
                Log($"Warning: {mismatchMessage}", true);
        }

        static bool TryValidatePartNumber(string scannedPartNumber, NeuropixelsV2ProbeGroup probeGroup, out string mismatchMessage)
        {
            try
            {
                NeuropixelsV2Helper.ValidateProbePartNumber(scannedPartNumber, probeGroup);
                mismatchMessage = null;
                return true;
            }
            catch (ArgumentException ex)
            {
                mismatchMessage = ex.Message;
                return false;
            }
        }
    }
}
