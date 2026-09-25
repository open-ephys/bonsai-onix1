using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The row of widgets that drives an <see cref="ImGuiLfpViewerPanel"/>.
    /// </summary>
    /// <remarks>
    /// One way to reach the panel's settings rather than a part of it: the panel plots and interprets
    /// gestures whether or not a strip is drawn, and reads nothing back from here. Widgets take the
    /// current value from the panel every frame, so a setting changed by gesture shows up here without
    /// anything being told about it.
    /// </remarks>
    internal class ImGuiLfpViewerControlStrip
    {
        private protected const float TextBoxWidth = 80;

        /// <summary>
        /// Number of columns the strip's table is opened with.
        /// </summary>
        private protected virtual int Columns => 6;

        /// <summary>
        /// Columns drawn ahead of the standard ones.
        /// </summary>
        private protected virtual void LeadingColumns(ImGuiLfpViewerPanel panel) { }

        /// <summary>
        /// Draws the strip into the region the caller has opened. Called once per frame.
        /// </summary>
        public void Draw(ImGuiLfpViewerPanel panel)
        {
            if (!ImGui.BeginTable("##menu", Columns, ImGuiTableFlags.NoSavedSettings))
                return;

            ImGui.TableNextRow();
            ImGui.PushItemWidth(TextBoxWidth);

            var paused = panel.Paused;

            LeadingColumns(panel);

            if (BeginMenuColumn("Timebase (s)"))
            {
                ImGui.BeginDisabled(paused); // NB: changing would cause buffer refresh and unpause
                var timebase = panel.Timebase;
                if (InputDoubleCombo("##timebase", ref timebase, panel.StandardTimeBases))
                    panel.Timebase = timebase;
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            if (BeginMenuColumn("Chan. Height"))
            {
                var channelHeight = panel.ChannelHeight;
                if (ImGui.DragInt(
                        "##channelHeight",
                        ref channelHeight,
                        vSpeed: 1,
                        panel.MinChannelHeight,
                        int.MaxValue,
                        ImGuiSliderFlags.AlwaysClamp))
                {
                    panel.ChannelHeight = channelHeight;
                }
                EndMenuColumn();
            }

            var rangeLabel = string.IsNullOrEmpty(panel.RangeLabel) ? "Range" : $"Range ({panel.RangeLabel})";
            if (BeginMenuColumn(rangeLabel))
            {
                var range = panel.RangeAmplitude;
                if (InputDoubleCombo("##range", ref range, panel.StandardRanges))
                    panel.RangeAmplitude = range;
                EndMenuColumn();
            }

            ImGui.TableNextColumn();
            PauseButton(panel);

            if (BeginMenuColumn("Palette"))
            {
                PaletteCombo(panel);
                if (panel.Palette == ColorPalette.Custom)
                {
                    ImGui.SameLine();
                    var customColor = panel.CustomColor;
                    if (ImGui.ColorEdit3("##customColor", ref customColor, ImGuiColorEditFlags.NoInputs))
                        panel.CustomColor = customColor;
                }
                EndMenuColumn();
            }

            if (BeginMenuColumn("Color Groups"))
            {
                var enabled = panel.ColorGroupingEnabled;
                if (ImGui.Checkbox("##colorGroupingEnabled", ref enabled))
                    panel.ColorGroupingEnabled = enabled;

                ImGui.SameLine();
                ImGui.BeginDisabled(!enabled);
                var grouping = panel.ColorGrouping;
                if (ImGui.InputInt("##colorGrouping", ref grouping))
                    panel.ColorGrouping = grouping;
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            ImGui.PopItemWidth();
            ImGui.EndTable();
        }

        private protected static bool BeginMenuColumn(string label)
        {
            ImGui.TableNextColumn();
            if (!ImGui.BeginTable(label, 1, ImGuiTableFlags.NoSavedSettings))
                return false;

            ImGui.TableNextColumn();
            ImGui.Text(label);
            return true;
        }

        private protected static void EndMenuColumn() => ImGui.EndTable();

        private protected static bool InputDoubleCombo(string label, ref double value, IReadOnlyList<double> comboItems)
        {
            var changed = false;
            var editValue = value;
            ImGui.InputDouble(label, ref editValue, "%g");
            if (changed = ImGui.IsItemDeactivatedAfterEdit())
                value = editValue;
            ImGui.SameLine(0, 0);

            var comboFlags = ImGuiComboFlags.NoPreview | ImGuiComboFlags.PopupAlignLeft;
            if (ImGui.BeginCombo(label + "C", string.Empty, comboFlags))
            {
                for (int i = 0; i < comboItems.Count; i++)
                {
                    var isSelected = value == comboItems[i];
                    if (ImGui.Selectable(comboItems[i].ToString("G"), isSelected))
                    {
                        value = comboItems[i];
                        changed = true;
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            return changed;
        }

        static void PaletteCombo(ImGuiLfpViewerPanel panel)
        {
            var preview = panel.Palette == ColorPalette.OpenEphysGui ? "Open Ephys GUI" : "Custom";
            if (ImGui.BeginCombo("##palette", preview))
            {
                if (ImGui.Selectable("Open Ephys GUI", panel.Palette == ColorPalette.OpenEphysGui))
                    panel.Palette = ColorPalette.OpenEphysGui;
                if (panel.Palette == ColorPalette.OpenEphysGui)
                    ImGui.SetItemDefaultFocus();

                if (ImGui.Selectable("Custom", panel.Palette == ColorPalette.Custom))
                    panel.Palette = ColorPalette.Custom;
                if (panel.Palette == ColorPalette.Custom)
                    ImGui.SetItemDefaultFocus();

                ImGui.EndCombo();
            }
        }

        static void PauseButton(ImGuiLfpViewerPanel panel)
        {
            var paused = panel.Paused;
            if (paused)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

            var buttonSize = new Vector2(TextBoxWidth, ImGui.GetFrameHeight() * 2);
            if (ImGui.Button("Pause", buttonSize))
                panel.Paused = !paused;

            if (paused)
                ImGui.PopStyleColor();
        }
    }
}
