using Bonsai;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Reflection-driven ImGui replacement for <see cref="GenericDeviceDialog"/>'s WinForms
    /// <see cref="System.Windows.Forms.PropertyGrid"/>: renders one row per browsable public property
    /// of <see cref="Device"/>, grouped by <see cref="CategoryAttribute"/>, using the same
    /// <see cref="TypeDescriptor"/>-based attribute resolution WinForms' own PropertyGrid uses
    /// internally.
    /// </summary>
    internal sealed class ImGuiPropertyPanel : IImGuiTabPanel
    {
        readonly object device;
        readonly PropertyDescriptorCollection properties;
        readonly Dictionary<string, byte[]> fileBuffers = new();

        // Computed once, on the first Draw() call (text measurement needs an active ImGui frame,
        // which doesn't exist yet at construction time). Equivalent to double-clicking each
        // column's resize border to auto-fit it to its widest content, just computed analytically
        // instead of relying on ImGui's internal per-frame content-width tracking.
        float? labelColumnWidth;
        float? valueColumnWidth;

        /// <summary>
        /// The object being edited. Mirrors <see cref="GenericDeviceDialog.Device"/> for read-back
        /// after the shell closes.
        /// </summary>
        internal object Device => device;

        /// <inheritdoc/>
        public bool HasChanges => false; // matches GenericDeviceDialog: no dirty-tracking today

        /// <inheritdoc/>
        public bool CanClose(DialogResult pendingResult) => true;

        /// <summary>
        /// Initializes a new instance of <see cref="ImGuiPropertyPanel"/>.
        /// </summary>
        /// <param name="device">The device configuration object whose properties will be edited.</param>
        /// <param name="filterDeviceTableProperties">
        /// <see langword="true"/> to hide properties tagged <see cref="DeviceTablePropertyAttribute"/>
        /// (e.g. <c>DeviceName</c>/<c>DeviceAddress</c>, which are assigned by the parent device table,
        /// not user-editable per-tab) — the only mode any current caller actually uses.
        /// </param>
        internal ImGuiPropertyPanel(object device, bool filterDeviceTableProperties = false)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            var attrs = filterDeviceTableProperties
                ? new Attribute[] { new BrowsableAttribute(true), new DeviceTablePropertyAttribute(false) }
                : Array.Empty<Attribute>();
            properties = TypeDescriptor.GetProperties(device, attrs);
        }

        /// <inheritdoc/>
        public void Draw()
        {
            ImGui.Spacing();

            labelColumnWidth ??= MeasureLabelColumnWidth();
            valueColumnWidth ??= MeasureValueColumnWidth();

            // Two fixed-width columns, auto-fit once above to their widest content and freely
            // resizable thereafter (matching WinForms PropertyGrid's name/value splitter).
            // NoHostExtendX stops the table stretching to fill the panel — with no stretch column,
            // it just ends after the value column, leaving plain background beyond it instead of a
            // third "filler" column whose leading border would look grabbable without being so.
            if (!ImGui.BeginTable("##properties", 2,
                ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.NoHostExtendX))
                return;

            ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, labelColumnWidth.Value);
            ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthFixed, valueColumnWidth.Value);

            string category = null;
            foreach (PropertyDescriptor prop in properties)
            {
                if (prop.Category != category)
                {
                    category = prop.Category;
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled(category ?? "Misc");
                }
                DrawPropertyRow(prop);
            }

            ImGui.EndTable();
        }

        float MeasureLabelColumnWidth()
        {
            float max = 0f;
            string category = null;
            foreach (PropertyDescriptor prop in properties)
            {
                max = Math.Max(max, ImGui.CalcTextSize(prop.DisplayName).X);
                if (prop.Category != category)
                {
                    category = prop.Category;
                    max = Math.Max(max, ImGui.CalcTextSize(category ?? "Misc").X);
                }
            }
            return max + ImGui.GetStyle().CellPadding.X * 2f;
        }

        float MeasureValueColumnWidth()
        {
            float comboChrome = ImGui.GetFrameHeight() + ImGui.GetStyle().FramePadding.X * 2f;
            float max = ImGui.GetFrameHeight(); // floor: enough to comfortably show a checkbox
            foreach (PropertyDescriptor prop in properties)
            {
                if (prop.PropertyType.IsEnum)
                {
                    foreach (var name in Enum.GetNames(prop.PropertyType))
                        max = Math.Max(max, ImGui.CalcTextSize(name).X + comboChrome);
                }
                else if (prop.PropertyType == typeof(string) &&
                         prop.Attributes[typeof(FileNameFilterAttribute)] is FileNameFilterAttribute)
                {
                    max = Math.Max(max, 260f); // room for a file path + Open... button
                }
                else if (prop.Converter != null && prop.Converter.GetStandardValuesSupported())
                {
                    foreach (var v in prop.Converter.GetStandardValues())
                        max = Math.Max(max, ImGui.CalcTextSize(prop.Converter.ConvertToString(v) ?? "").X + comboChrome);
                }
            }
            return max + ImGui.GetStyle().CellPadding.X * 2f;
        }

        static void DescriptionTooltip(PropertyDescriptor prop)
        {
            if (!string.IsNullOrEmpty(prop.Description)) ImGuiControls.Tooltip(prop.Description);
        }

        void DrawPropertyRow(PropertyDescriptor prop)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(prop.DisplayName);
            DescriptionTooltip(prop);

            ImGui.TableNextColumn();
            ImGui.PushID(prop.Name);
            DrawValueWidget(prop);
            ImGui.PopID();
        }

        void DrawValueWidget(PropertyDescriptor prop)
        {
            object value = prop.GetValue(device);

            if (prop.PropertyType == typeof(bool))
            {
                bool b = (bool)value;
                if (ImGui.Checkbox("##value", ref b)) prop.SetValue(device, b);
                DescriptionTooltip(prop);
            }
            else if (prop.PropertyType == typeof(string) &&
                     prop.Attributes[typeof(FileNameFilterAttribute)] is FileNameFilterAttribute filterAttr)
            {
                DrawFilePicker(prop, filterAttr, (string)value);
            }
            else if (prop.PropertyType.IsEnum)
            {
                var names = Enum.GetNames(prop.PropertyType);
                var values = Enum.GetValues(prop.PropertyType);
                int idx = Math.Max(0, Array.IndexOf(values, value));
                ImGui.SetNextItemWidth(-1);
                if (ImGui.Combo("##value", ref idx, names, names.Length))
                    prop.SetValue(device, values.GetValue(idx));
                DescriptionTooltip(prop);
            }
            else if (prop.Converter != null && prop.Converter.GetStandardValuesSupported())
            {
                var standardValues = prop.Converter.GetStandardValues().Cast<object>().ToArray();
                var names = standardValues.Select(v => prop.Converter.ConvertToString(v)).ToArray();
                int idx = Math.Max(0, Array.IndexOf(standardValues, value));
                ImGui.SetNextItemWidth(-1);
                if (ImGui.Combo("##value", ref idx, names, names.Length))
                    prop.SetValue(device, standardValues[idx]);
                DescriptionTooltip(prop);
            }
            else
            {
                ImGui.TextDisabled(value?.ToString() ?? "");
                DescriptionTooltip(prop);
            }
        }

        void DrawFilePicker(PropertyDescriptor prop, FileNameFilterAttribute filterAttr, string currentValue)
        {
            string current = currentValue ?? "";
            var buf = fileBuffers.TryGetValue(prop.Name, out var b) ? b : fileBuffers[prop.Name] = new byte[512];
            ImGuiControls.WriteString(buf, current);

            float openButtonWidth = ImGui.CalcTextSize("Open...", true).X + ImGui.GetStyle().FramePadding.X * 2f;
            float inputWidth = ImGui.GetContentRegionAvail().X - openButtonWidth - ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(inputWidth);
            unsafe
            {
                fixed (byte* p = buf)
                    if (ImGui.InputText("##input", p, (nuint)buf.Length))
                        prop.SetValue(device, ImGuiControls.ReadBuffer(buf));
            }
            ImGuiControls.Tooltip(prop.Description);
            ImGui.SameLine();
            if (ImGui.Button("Open...##open"))
            {
                using var ofd = new OpenFileDialog { Title = $"Select {prop.DisplayName}", Filter = filterAttr.Filter };
                if (!string.IsNullOrEmpty(current) && File.Exists(current))
                    ofd.InitialDirectory = Path.GetDirectoryName(current);
                if (ofd.ShowDialog() == DialogResult.OK)
                    prop.SetValue(device, ofd.FileName);
            }
        }
    }
}
