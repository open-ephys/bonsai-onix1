using System;

namespace OpenEphys.Onix1.Design
{
    internal partial class NeuropixelsV1ImGuiDialog
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

        static bool TryValidatePartNumber(string scannedPartNumber, NeuropixelsV1ProbeGroup probeGroup, out string mismatchMessage)
        {
            try
            {
                NeuropixelsV1Helper.ValidateProbePartNumber(scannedPartNumber, probeGroup);
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
