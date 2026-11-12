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

            // NB: the scanned serial number can show or hide stored survey results that were judged against the
            // calibration file's. Results without a serial number are a survey just run, so they stay.
            var results = survey.Results;
            if (results == null || (results.ProbeSerialNumber.HasValue && results.ProbeSerialNumber != scan.SerialNumber))
                RestoreSurveyResults();
        }

        protected override ulong? ScannedSerialNumber =>
            scan.Status == NeuropixelsV1ScanStatus.Detected ? scan.SerialNumber : null;

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
