using System;
using Hexa.NET.ImGui;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Draws the per-probe rows shared by the Neuropixels headstage panels.
    /// </summary>
    static class HeadstageProbeRows
    {
        /// <summary>
        /// Draws a probe's scan result under a heading with the probe's name and scanned model, in the scan result's
        /// color, with its enable checkbox. Returns whether the user changed <paramref name="enabled"/>.
        /// </summary>
        /// <param name="label">The probe's name.</param>
        /// <param name="scan">The probe's scan result.</param>
        /// <param name="enabled">Whether the probe is enabled.</param>
        /// <param name="busy">Whether a scan or survey is running, which disables the checkbox.</param>
        /// <param name="scannedModel">The model New offers a layout for that matches the scanned probe, or
        /// <c>null</c> for none, which hides the button that loads it.</param>
        /// <param name="loadScannedModel">Loads the layout for <paramref name="scannedModel"/>.</param>
        /// <param name="mismatchedModel">The model of the loaded channel configuration when it does not match the
        /// scanned probe, or <c>null</c>.</param>
        internal static bool DrawEnableRow(string label, NeuropixelsV1ScanState scan, ref bool enabled, bool busy,
            string scannedModel, Action loadScannedModel, string mismatchedModel)
        {
            // NB: a probe the scan did not find is greyed out, so that enabling it looks wrong, though it is still
            // allowed.
            var status = scan.Status;
            bool detected = status == NeuropixelsV1ScanStatus.Detected;
            var result = status switch
            {
                NeuropixelsV1ScanStatus.Running => "scanning...",
                NeuropixelsV1ScanStatus.Detected => scan.PartNumber,
                NeuropixelsV1ScanStatus.NotDetected => "not detected",
                _ => "not scanned",
            };
            ImGui.PushStyleColor(ImGuiCol.Text, detected ? ImGuiPalette.BrightFern
                : status == NeuropixelsV1ScanStatus.Running ? ImGui.GetColorU32(ImGuiCol.Text)
                : ImGui.GetColorU32(ImGuiCol.TextDisabled));
            ImGui.TextUnformatted($"{label}: {result}");
            ImGui.PopStyleColor();

            ImGui.BeginDisabled(busy);
            bool changed = ImGui.Checkbox($"Enabled##{label}", ref enabled);
            ImGui.EndDisabled();
            ImGuiWidgets.Tooltip($"Enable {label}. A disabled probe is not configured or recorded by the workflow, " +
                "is left out of the survey, and its tab is greyed out. This sets the probe's Enable property.",
                "Not while a scan or survey is running.");

            if (detected)
            {
                if (scannedModel != null)
                {
                    ImGui.BeginDisabled(busy || !enabled);
                    if (ImGui.Button($"New config.##{label}"))
                        loadScannedModel();
                    ImGui.EndDisabled();
                    ImGuiWidgets.Tooltip($"Load the {scannedModel} layout into {label}, as choosing it from New " +
                        "does. Asks first if there are unsaved changes, and leaves the current file untouched.",
                        busy ? "Not while a scan or survey is running." : $"{label} is disabled.");
                }

                // NB: next to the button that fixes it.
                if (mismatchedModel != null)
                {
                    if (scannedModel != null) ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiPalette.AmberGold);
                    ImGui.TextUnformatted($"layout is {mismatchedModel}");
                    ImGui.PopStyleColor();
                    ImGuiWidgets.Tooltip($"The scan found a {scan.PartNumber}, but {label}'s channel configuration " +
                        $"is for a {mismatchedModel}, so the workflow will not run." +
                        (scannedModel != null ? $" Use New config. to load the {scannedModel} layout." : ""));
                }

                ImGuiWidgets.InfoRow("Part No.", scan.PartNumber);
                ImGuiWidgets.InfoRow("SN", scan.SerialNumber.ToString());
            }

            ImGui.Spacing();
            return changed;
        }

        /// <summary>
        /// Says that the probe <paramref name="label"/> needs a channel configuration, and how to choose one.
        /// </summary>
        internal static string NoChannelConfiguration(string label) =>
            $"{label} needs a channel configuration. In its tab, choose a probe model from New, or open a " +
            "ProbeInterface file.";
    }
}
