using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using Newtonsoft.Json;

namespace OpenEphys.Onix1.Design
{
    // Gain calibration + probe interface file UI, load/save of the probe group,
    // pinned-state persistence, and the unsaved-changes close prompt.
    internal partial class NeuropixelsV2eImGuiDialog
    {
        void DrawFileSection()
        {
            string openButtonText = "Open...##gaincal";
            float openButtonTextSize = ImGui.CalcTextSize(openButtonText, true).X; // true = hide text after ##
            float openButtonWidth = openButtonTextSize + ImGui.GetStyle().FramePadding.X * 2f;

            float fileTargetW = ImGui.GetContentRegionAvail().X
                - openButtonWidth
                - ImGui.GetStyle().ItemSpacing.X; // gap from SameLine()

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

            if (ImGui.Button(openButtonText))
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

            ImGui.Spacing();
            ImGui.Text("Probe Interface File");
            ImGui.Spacing();

            string pf = configureNode.ProbeConfiguration.ProbeInterfaceFileName ?? "";
            WriteString(probeFileBuf, pf);
            ImGui.SetNextItemWidth(fileTargetW);
            unsafe
            {
                fixed (byte* p = probeFileBuf)
                    if (ImGui.InputText("##probefile", p, (nuint)probeFileBuf.Length))
                        configureNode.ProbeConfiguration.ProbeInterfaceFileName = ReadBuffer(probeFileBuf);
            }
            if (!string.IsNullOrEmpty(configureNode.ProbeConfiguration.ProbeInterfaceFileName))
            {
                Tooltip(configureNode.ProbeConfiguration.ProbeInterfaceFileName);
            }

            ImGui.SameLine();
            if (ImGui.Button("Open...##probefile"))
            {
                var calFile = configureNode.ProbeConfiguration.GainCalibrationFileName;
                using var ofd = new OpenFileDialog
                {
                    Title  = "Load Probe Interface File",
                    Filter = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                };

                if (!string.IsNullOrEmpty(pf) && File.Exists(pf))
                    ofd.InitialDirectory = Path.GetDirectoryName(pf);
                else if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                    ofd.InitialDirectory = Path.GetDirectoryName(calFile);

                if (ofd.ShowDialog() == DialogResult.OK)
                    OnProbeFileNameChanged(ofd.FileName);
            }
            Tooltip("Open a file browser to load an existing ProbeInterface file that specifies the channel map, contact state, and previous survey results.");

            // save and save as
            bool canSave = !string.IsNullOrEmpty(configureNode.ProbeConfiguration.ProbeInterfaceFileName);
            string saveLabel = hasChanges ? "Save Changes*##save" : "Save##save";
            if (!canSave) ImGui.BeginDisabled();
            if (ImGui.Button(saveLabel)) SaveProbeGroup();
            Tooltip("Save the current channel map, contact state, and survey data to the specified ProbeInterface file.",
                "Choose a file with Open or Save As... first.");
            if (!canSave) ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Save As...##save"))
                SaveProbeGroupAs();
            Tooltip("Choose a file path and save the current channel map, contact state, and survey data to a new ProbeInterface file.");
        }

        void SaveProbeGroup()
        {
            var path = configureNode.ProbeConfiguration.ProbeInterfaceFileName;
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                WritePinnedState();
                WriteActivityData();
                File.WriteAllText(path, JsonConvert.SerializeObject(probeGroup, Formatting.Indented));
                Log($"Probeinterface file saved to {path}");
                HasChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SaveProbeGroupAs()
        {
            string currentPath = configureNode.ProbeConfiguration.ProbeInterfaceFileName ?? "";
            string calFile = configureNode.ProbeConfiguration.GainCalibrationFileName ?? "";
            using var sfd = new SaveFileDialog
            {
                Title           = "Save Probe Interface File As",
                Filter          = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                DefaultExt      = "json",
                FileName        = Path.GetFileName(currentPath.Length > 0 ? currentPath : GetProbeSerialNumber() + ".json"),
                OverwritePrompt = true,
            };

            if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                sfd.InitialDirectory = Path.GetDirectoryName(currentPath);
            else if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                sfd.InitialDirectory = Path.GetDirectoryName(calFile);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                configureNode.ProbeConfiguration.ProbeInterfaceFileName = sfd.FileName;
                SaveProbeGroup();
            }
        }

        void WritePinnedState()
        {
            var pinned = allContacts
                .Select((c, i) => channelState.ElectrodeToChannel.TryGetValue(i, out int ch) && channelState.PinnedChannels.Contains(ch))
                .ToArray();
            probeGroup.Probe.SetContactAnnotation<bool>("pinned", pinned);
        }

        void RestorePinnedState()
        {
            var pinned = probeGroup.Probe.GetContactAnnotation<bool>("pinned");
            if (pinned == null) return;
            for (int i = 0; i < allContacts.Count && i < pinned.Length; i++)
                if (pinned[i] && channelState.ElectrodeToChannel.TryGetValue(i, out int ch))
                    channelState.PinnedChannels.Add(ch);
            if (channelState.PinnedChannels.Count > 0) RecomputeBlockedIndices();
        }

        void OnProbeFileNameChanged(string newPath)
        {
            if (hasChanges)
            {
                var r = MessageBox.Show("Changing the file will discard unsaved changes. Continue?",
                    "Change Probe Interface File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }

            var oldPath = configureNode.ProbeConfiguration.ProbeInterfaceFileName;
            configureNode.ProbeConfiguration.ProbeInterfaceFileName = newPath;

            try
            {
                probeGroup = NeuropixelsV2ProbeTypes.All[probeTypeIdx].DeserializeGroup(File.ReadAllText(newPath));
            }
            catch (Exception ex)
            {
                configureNode.ProbeConfiguration.ProbeInterfaceFileName = oldPath;
                MessageBox.Show($"Failed to load probe interface file:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RefreshProbeState();

            Log($"Loaded probeinterface file {newPath}");

            HasChanges = false;
        }

        bool PromptSaveOnClose()
        {
            var path = configureNode.ProbeConfiguration.ProbeInterfaceFileName;
            bool fileExists = !string.IsNullOrEmpty(path) && File.Exists(path);

            if (fileExists)
            {
                var r = MessageBox.Show($"Save changes to {Path.GetFileName(path)}?",
                    "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) return false;
                if (r == DialogResult.Yes) SaveProbeGroup();
                return true;
            }

            var create = MessageBox.Show(
                $"There are unsaved {probeName} channel configuration changes.\nWould you like to save them to a probe interface file?",
                "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (create == DialogResult.No) return true;

            using var sfd = new SaveFileDialog
            {
                Title      = "Save Probe Interface File",
                Filter     = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                DefaultExt = "json",
                FileName   = GetProbeSerialNumber() + ".json"
            };

            var calFile = configureNode.ProbeConfiguration.GainCalibrationFileName;

            if (!string.IsNullOrEmpty(calFile) && File.Exists(calFile))
                sfd.InitialDirectory = Path.GetDirectoryName(calFile);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                configureNode.ProbeConfiguration.ProbeInterfaceFileName = sfd.FileName;
                SaveProbeGroup();
                return true;
            }

            var discard = MessageBox.Show("Discard unsaved changes and close?",
                "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return discard == DialogResult.Yes;
        }

        string GetProbeSerialNumber()
        {
            var calFile = configureNode.ProbeConfiguration.GainCalibrationFileName;
            if (string.IsNullOrEmpty(calFile) || !File.Exists(calFile)) return "probe";
            try { return File.ReadLines(calFile).FirstOrDefault()?.Trim() ?? "probe"; }
            catch { return "probe"; }
        }
    }
}
