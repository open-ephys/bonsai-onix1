using System;

namespace OpenEphys.Onix1.Design
{
    internal partial class NeuropixelsV1ImGuiDialog
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
            try { return NeuropixelsV1VariantRegistry.Resolve(partNumber); }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { return null; }
        }

        void CheckScannedPartNumberAgainstLoadedFile()
        {
            if (scan.Status != NeuropixelsV1ScanStatus.Detected || !ChannelConfigurationChosen) return;

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
