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
        /// Draws a probe's enable checkbox, labeled with the probe's name in its scan result's color, followed by
        /// the scan result. Returns whether the user changed <paramref name="enabled"/>.
        /// </summary>
        /// <param name="label">The probe's name.</param>
        /// <param name="scan">The probe's scan result.</param>
        /// <param name="enabled">Whether the probe is enabled.</param>
        /// <param name="busy">Whether a scan or survey is running, which disables the checkbox.</param>
        internal static bool DrawEnableRow(string label, NeuropixelsV1ScanState scan, ref bool enabled, bool busy)
        {
            // NB: a probe the scan did not find is greyed out, so that enabling it looks wrong, though it is still
            // allowed.
            var status = scan.Status;
            bool colored = status == NeuropixelsV1ScanStatus.Detected || status == NeuropixelsV1ScanStatus.NotDetected;
            if (colored)
                ImGui.PushStyleColor(ImGuiCol.Text, status == NeuropixelsV1ScanStatus.Detected
                    ? ImGuiPalette.BrightFern
                    : ImGui.GetColorU32(ImGuiCol.TextDisabled));

            ImGui.BeginDisabled(busy);
            bool changed = ImGui.Checkbox($"{label}##enable", ref enabled);
            ImGui.EndDisabled();
            if (colored) ImGui.PopStyleColor();
            ImGuiWidgets.Tooltip($"Enable {label}. A disabled probe is not configured or recorded by the workflow, " +
                "is left out of the survey, and its tab is greyed out. This sets the probe's Enable property.",
                "Not while a scan or survey is running.");

            switch (status)
            {
                case NeuropixelsV1ScanStatus.Running:
                    ImGui.SameLine();
                    ImGui.TextUnformatted("scanning...");
                    break;
                case NeuropixelsV1ScanStatus.Detected:
                    ImGuiWidgets.InfoRow("Part No.", scan.PartNumber);
                    ImGuiWidgets.InfoRow("SN", scan.SerialNumber.ToString());
                    break;
                case NeuropixelsV1ScanStatus.NotDetected:
                    ImGui.SameLine();
                    ImGui.TextDisabled("not detected");
                    break;
                default:
                    ImGui.SameLine();
                    ImGui.TextDisabled("not scanned");
                    break;
            }

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
