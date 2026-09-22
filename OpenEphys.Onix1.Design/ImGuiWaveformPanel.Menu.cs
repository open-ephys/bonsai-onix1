using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiWaveformPanel
    {
        const float TextBoxWidth = 80;
        const int MinChannelHeight = 2;
        const int PaletteSteps = 16;

        enum ColorPalette
        {
            OpenEphysGui,
            Custom,
        }

        static readonly double[] StandardRanges =
        {
            50, 100, 250, 500, 1000, 2500, 5000, 10000
        };

        const double ShortestTimeBase = 0.01;
        static readonly double[] TimeBaseSteps = { 1.0, 2.5, 5.0 };
        double[] standardTimeBases = { ShortestTimeBase };

        void RebuildTimeBases(double historySeconds)
        {
            var ladder = new List<double>();
            var nextDecade = true;
            for (var decade = ShortestTimeBase; nextDecade; decade *= 10)
            {
                foreach (var step in TimeBaseSteps)
                {
                    var span = decade * step;
                    if (span > historySeconds && ladder.Count > 0) // NB: ladder.Count > 0 -> shortest span is always offered
                    {
                        nextDecade = false;
                        break;
                    }

                    ladder.Add(span);
                }
            }

            standardTimeBases = ladder.ToArray();
            timebase = Math.Min(timebase, standardTimeBases[standardTimeBases.Length - 1]);
        }

        bool colorGroupingEnabled;
        int colorGrouping = 1;
        ColorPalette palette = ColorPalette.OpenEphysGui;
        Vector3 customColor = new(140 / 255f, 219 / 255f, 142 / 255f); // suggested default: a soft green

        void MenuWidgets()
        {
            if (!ImGui.BeginTable("##menu", columns: 7, ImGuiTableFlags.NoSavedSettings))
                return;

            ImGui.TableNextRow();
            ImGui.PushItemWidth(TextBoxWidth);

            var paused = Paused;

            if (BeginMenuColumn("Band"))
            {
                ImGui.BeginDisabled(paused); // NB: changing would cause buffer refresh and unpause
                BandCombo();
                ImGui.EndDisabled();
                ImGui.SameLine();
                var cmr = UseCommonMedianReference;
                if (ImGui.Checkbox("Apply CMR", ref cmr))
                    UseCommonMedianReference = cmr;
                EndMenuColumn();
            }

            if (BeginMenuColumn("Timebase (s)"))
            {
                ImGui.BeginDisabled(paused); // NB: changing would cause buffer refresh and unpause
                InputDoubleCombo("##timebase", ref timebase, standardTimeBases);
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            if (BeginMenuColumn("Chan. Height"))
            {
                ImGui.DragInt(
                    "##channelHeight",
                    ref channelHeight,
                    vSpeed: 1,
                    MinChannelHeight,
                    int.MaxValue,
                    ImGuiSliderFlags.AlwaysClamp);
                EndMenuColumn();
            }

            var rangeLabel = string.IsNullOrEmpty(RangeLabel) ? "Range" : $"Range ({RangeLabel})";
            if (BeginMenuColumn(rangeLabel))
            {
                if (InputDoubleCombo("##range", ref rangeAmplitude, StandardRanges))
                    rangeAmplitude = Math.Max(1, rangeAmplitude);
                EndMenuColumn();
            }

            ImGui.TableNextColumn();
            PauseButton();

            if (BeginMenuColumn("Palette"))
            {
                PaletteCombo();
                if (palette == ColorPalette.Custom)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit3("##customColor", ref customColor, ImGuiColorEditFlags.NoInputs);
                }
                EndMenuColumn();
            }

            if (BeginMenuColumn("Color Groups"))
            {
                ImGui.Checkbox("##colorGroupingEnabled", ref colorGroupingEnabled);
                ImGui.SameLine();
                ImGui.BeginDisabled(!colorGroupingEnabled);
                if (ImGui.InputInt("##colorGrouping", ref colorGrouping))
                    colorGrouping = Math.Max(1, colorGrouping);
                ImGui.EndDisabled();
                EndMenuColumn();
            }

            ImGui.PopItemWidth();
            ImGui.EndTable();
        }

        static bool BeginMenuColumn(string label)
        {
            ImGui.TableNextColumn();
            if (!ImGui.BeginTable(label, 1, ImGuiTableFlags.NoSavedSettings))
                return false;

            ImGui.TableNextColumn();
            ImGui.Text(label);
            return true;
        }

        static void EndMenuColumn() => ImGui.EndTable();

        static bool InputDoubleCombo(string label, ref double value, double[] comboItems)
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
                for (int i = 0; i < comboItems.Length; i++)
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

        void BandCombo()
        {
            var singleBand = Bands.Count <= 1;
            var preview = SelectedBand >= 0 && SelectedBand < Bands.Count ? Bands[SelectedBand].Name : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##band", preview))
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    var isSelected = i == SelectedBand;
                    var (name, description) = Bands[i];
                    if (ImGui.Selectable($"{name}: {description}", isSelected) && !isSelected)
                        SelectedBand = i;
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (singleBand) ImGui.EndDisabled();
        }

        void PaletteCombo()
        {
            var preview = palette == ColorPalette.OpenEphysGui ? "Open Ephys GUI" : "Custom";
            if (ImGui.BeginCombo("##palette", preview))
            {
                if (ImGui.Selectable("Open Ephys GUI", palette == ColorPalette.OpenEphysGui))
                    palette = ColorPalette.OpenEphysGui;
                if (palette == ColorPalette.OpenEphysGui)
                    ImGui.SetItemDefaultFocus();

                if (ImGui.Selectable("Custom", palette == ColorPalette.Custom))
                    palette = ColorPalette.Custom;
                if (palette == ColorPalette.Custom)
                    ImGui.SetItemDefaultFocus();

                ImGui.EndCombo();
            }
        }

        /// <summary>
        /// The color for channel group <paramref name="group"/> under the selected <see
        /// cref="palette"/>.
        /// </summary>
        /// <remarks>
        /// In <see cref="ColorPalette.Custom"/>, groups are variants of the one picked hue, spread
        /// across <see cref="PaletteSteps"/> saturation/value combinations rather than assigned in
        /// order, so that consecutive groups land far apart in the ramp instead of a barely
        /// different neighboring shade — group 0 is always the picked color unmodified, which is
        /// also what a single-group plot gets.
        /// </remarks>
        Vector4 GroupColor(int group)
        {
            if (palette == ColorPalette.OpenEphysGui)
                return ImGui.ColorConvertU32ToFloat4(ImGuiPalette.OpenEphysGuiLfp[group % ImGuiPalette.OpenEphysGuiLfp.Length]);

            float hue = 0, saturation = 0, value = 0;
            ImGui.ColorConvertRGBtoHSV(customColor.X, customColor.Y, customColor.Z, ref hue, ref saturation, ref value);

            var step = group * 7 % PaletteSteps;
            var fraction = step / (float)(PaletteSteps - 1);
            saturation = Math.Min(1f, saturation + 0.4f * fraction);
            value *= 1f - 0.6f * fraction;

            float r = 0, g = 0, b = 0;
            ImGui.ColorConvertHSVtoRGB(hue, saturation, value, ref r, ref g, ref b);
            return new Vector4(r, g, b, 1f);
        }

        void PauseButton()
        {
            var paused = Paused;
            if (paused)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

            var buttonSize = new Vector2(TextBoxWidth, ImGui.GetFrameHeight() * 2);
            if (ImGui.Button("Pause", buttonSize) || ImGui.IsKeyPressed(ImGuiKey.Space))
                TogglePause();

            if (paused)
                ImGui.PopStyleColor();
        }
    }
}
