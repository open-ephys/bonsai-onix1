using System.IO;
using System.Windows.Forms;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    // Gain calibration file UI. Probe interface file I/O, pinned-state persistence, and the
    // unsaved-changes close prompt are generic and live in ImGuiNeuropixelsDialog.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        protected override void DrawCalibrationFileSection()
        {
            float fileTargetW = ComputeFileRowInputWidth();

            ImGui.Text("Gain Calibration File");
            ImGui.Spacing();
            string gainCal = configureNode.ProbeConfiguration.GainCalibrationFileName ?? "";
            WriteString(gainCalBuf, gainCal);
            ImGui.SetNextItemWidth(fileTargetW);
            unsafe
            {
                fixed (byte* p = gainCalBuf)
                    if (ImGui.InputText("##gaincal", p, (nuint)gainCalBuf.Length))
                        configureNode.ProbeConfiguration.GainCalibrationFileName = ReadBuffer(gainCalBuf);
            }

            if (!string.IsNullOrEmpty(configureNode.ProbeConfiguration.GainCalibrationFileName))
            {
                Tooltip(configureNode.ProbeConfiguration.GainCalibrationFileName);
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
            Tooltip("Open a file browser to choose the gain calibration file for this probe. Required to acquire data from the probe and to perform an electrode survey.");
        }
    }
}
