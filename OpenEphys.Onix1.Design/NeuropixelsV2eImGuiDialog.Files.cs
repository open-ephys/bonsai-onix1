using System;
using System.IO;
using System.Windows.Forms;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Quick-load default geometry buttons and gain calibration file UI. Probe interface file I/O,
    // pinned-state persistence, and the unsaved-changes close prompt are generic and live in
    // ImGuiNeuropixelsDialog.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void DrawQuickLoadSection()
        {
            // Beta probes are quad-shank-only hardware; there is no alternate default to offer.
            if (isBeta) return;

            ImGui.Text("Load Default Geometry");
            ImGui.Spacing();
            if (ImGui.Button("NP2003 (1-shank)##loadnp2003")) QuickLoadDefaultGeometry("NP2003.json", "NP2003");
            ImGuiControls.Tooltip("Load the bundled single-shank Neuropixels 2.0 probe interface file through the same load path as browsing to any other file.");
            ImGui.SameLine();
            if (ImGui.Button("NP2013 (4-shank)##loadnp2013")) QuickLoadDefaultGeometry("NP2013.json", "NP2013");
            ImGuiControls.Tooltip("Load the bundled quad-shank Neuropixels 2.0 probe interface file through the same load path as browsing to any other file.");
        }

        void QuickLoadDefaultGeometry(string embeddedResourceName, string displayName)
        {
            if (HasChanges)
            {
                var r = MessageBox.Show($"Loading the {displayName} default geometry will discard unsaved {probeName} changes. Continue?",
                    $"{probeName}: Load Default Geometry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }

            try
            {
                LoadProbeGroupFromJson(ProbeGroupResource.LoadDefaultJson(embeddedResourceName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load {displayName} default geometry:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            configureNode.ProbeConfiguration.ProbeInterfaceFileName = null;
            RefreshProbeState();
            Log($"Loaded {displayName} default probeinterface geometry");
            HasChanges = true;
        }

        protected override void DrawCalibrationFileSection()
        {
            float fileTargetW = ComputeFileRowInputWidth();

            ImGui.Text("Gain Calibration File");
            ImGui.Spacing();
            string gainCal = configureNode.ProbeConfiguration.GainCalibrationFileName ?? "";
            ImGuiControls.WriteString(gainCalBuf, gainCal);
            ImGui.SetNextItemWidth(fileTargetW);
            unsafe
            {
                fixed (byte* p = gainCalBuf)
                    if (ImGui.InputText("##gaincal", p, (nuint)gainCalBuf.Length))
                        configureNode.ProbeConfiguration.GainCalibrationFileName = ImGuiControls.ReadBuffer(gainCalBuf);
            }

            if (!string.IsNullOrEmpty(configureNode.ProbeConfiguration.GainCalibrationFileName))
            {
                ImGuiControls.Tooltip(configureNode.ProbeConfiguration.GainCalibrationFileName);
            }

            ImGui.SameLine();

            if (ImGui.Button("Open...##gaincal"))
            {
                using var ofd = new OpenFileDialog
                {
                    Title = "Select Gain Calibration File",
                    Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All files (*.*)|*.*"
                };

                if (!string.IsNullOrEmpty(gainCal) && File.Exists(gainCal))
                    ofd.InitialDirectory = Path.GetDirectoryName(gainCal);

                if (ofd.ShowDialog() == DialogResult.OK)
                    configureNode.ProbeConfiguration.GainCalibrationFileName = ofd.FileName;
            }
            ImGuiControls.Tooltip("Open a file browser to choose the gain calibration file for this probe. Required to acquire data from the probe and to perform an electrode survey.");
        }
    }
}
