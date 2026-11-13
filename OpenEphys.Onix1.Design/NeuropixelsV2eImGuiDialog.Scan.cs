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
            if (scan.Status != NeuropixelsV1ScanStatus.Detected) return;
            CheckScannedPartNumberAgainstLoadedFile();
            OfferScannedModel(scan.PartNumber);

            // NB: a scan hides stored survey results measured on another probe, or shows them again when it finds
            // that probe. Results without a serial number can't be shown to differ, so they stay.
            var results = survey.Results;
            if (results == null || (results.ProbeSerialNumber.HasValue && results.ProbeSerialNumber != scan.SerialNumber))
                RestoreSurveyResults();
        }

        protected override ulong? ScannedSerialNumber =>
            scan.Status == NeuropixelsV1ScanStatus.Detected ? scan.SerialNumber : null;

        // NB: a part number the family does not know cannot be shown to match.
        protected override bool ScannedProbeMatches()
        {
            try { return TryValidatePartNumber(scan.PartNumber, probeGroup, out _); }
            catch (NotSupportedException) { return false; }
        }

        protected override string ScannedPartNumber =>
            scan.Status == NeuropixelsV1ScanStatus.Detected ? scan.PartNumber : null;

        protected override object ResolveVariant(string partNumber)
        {
            try { return NeuropixelsV2VariantRegistry.Resolve(partNumber); }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { return null; }
        }

        void CheckScannedPartNumberAgainstLoadedFile()
        {
            if (scan.Status != NeuropixelsV1ScanStatus.Detected || !ChannelConfigurationChosen) return;

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
