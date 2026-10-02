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
        /// <summary>
        /// Width every control is given. Wide enough for the longest palette name, which is the widest
        /// preview text any of these show, and for every label, so one width serves them all.
        /// </summary>
        private protected const float ControlWidth = 150;

        /// <summary>
        /// Width the pause button is given where there is room for it, and the least it will be squeezed
        /// to where there is not. It is not a parameter and is sized to say so.
        /// </summary>
        const float PauseWidth = 200f;
        const float MinPauseWidth = 60f;

        /// <summary>
        /// Space either side of the rule separating pause from the parameters.
        /// </summary>
        const float RuleGap = 30f;

        /// <summary>
        /// Space between one column's controls and the next, and between one row and the next.
        /// </summary>
        const float ColumnGap = 20f;
        const float RowGap = 8f;

        /// <summary>
        /// Space between the controls and the left and right edges of the strip.
        /// </summary>
        const float EdgeMargin = 20f;

        /// <summary>
        /// Left inset of the collapse toggle and the strip's title, which sit closer to the edge than
        /// the controls do.
        /// </summary>
        const float HeaderInset = 6f;

        /// <summary>
        /// Space above and below everything, inside whatever frame the owner draws. A collapsed strip
        /// is meant to take almost no room, so it keeps only enough to clear the toggle.
        /// </summary>
        const float VerticalMargin = 6f;
        const float CollapsedMargin = 1f;

        Vector2 flowOrigin;
        float columnPitch;
        float rowPitch;
        int columnsPerRow;
        int flowIndex;

        /// <summary>
        /// Number of columns holding parameters. The strip adds the flexible gap and the pause button
        /// after them itself.
        /// </summary>
        private protected virtual int ParameterColumns => 5;

        /// <summary>
        /// Columns drawn ahead of the standard ones.
        /// </summary>
        private protected virtual void LeadingColumns(ImGuiLfpViewerPanel panel) { }

        /// <summary>
        /// Whether the controls are folded away, leaving only the toggle that brings them back.
        /// </summary>
        public bool Collapsed { get; set; }

        /// <summary>
        /// Unit shown beside the amplitude range control.
        /// </summary>
        public string RangeLabel { get; set; }

        /// <summary>
        /// Draws the strip into the region the caller has opened. Called once per frame.
        /// </summary>
        public void Draw(ImGuiLfpViewerPanel panel)
        {
            var margin = Collapsed ? CollapsedMargin : VerticalMargin;
            ImGui.Dummy(new Vector2(0, margin));

            ImGui.Indent(HeaderInset);
            if (ImGui.ArrowButton("##collapse", Collapsed ? ImGuiDir.Up : ImGuiDir.Down))
                Collapsed = !Collapsed;

            ImGui.SameLine();
            ImGui.Text("Control Panel");
            ImGui.Unindent(HeaderInset);

            if (!Collapsed)
                DrawControls(panel);

            // NB: an item rather than a cursor move, since nothing follows it to extend the content
            // extent and ImGui asserts when a cursor move is the last thing in a window.
            ImGui.Dummy(new Vector2(0, margin));
        }

        /// <summary>
        /// Lays the parameter controls out as a wrapping grid, with pause kept to the right of a rule.
        /// </summary>
        /// <remarks>
        /// A table cannot wrap, so the controls are positioned outright. Every control takes one width, which
        /// makes the wrap arithmetic rather than measurement, and the column pitch is worked out once from a
        /// full row so that a partly filled last row keeps the columns of the rows above it rather than
        /// spreading its own few items across the whole width.
        /// </remarks>
        void DrawControls(ImGuiLfpViewerPanel panel)
        {
            // NB: the flow is positioned outright rather than indented, so every horizontal distance here is
            // measured from one origin and the rule and pause go exactly where the layout reserved room for
            // them.
            var origin = ImGui.GetCursorScreenPos();
            var available = ImGui.GetContentRegionAvail().X;
            var usable = available - EdgeMargin * 2;
            var rightZone = RuleGap * 2 + ImGuiLfpViewerPanel.FrameWeight + PauseWidth;
            var flowWidth = Math.Max(ControlWidth, usable - rightZone);

            // NB: one control column is the floor for the flow, so a window narrow enough to reach it
            // leaves pause less than it asked for. It gives width up rather than being clipped.
            var pauseWidth = Math.Max(MinPauseWidth, Math.Min(
                PauseWidth,
                usable - flowWidth - RuleGap * 2 - ImGuiLfpViewerPanel.FrameWeight));

            columnsPerRow = Math.Max(1, (int)((flowWidth + ColumnGap) / (ControlWidth + ColumnGap)));
            columnsPerRow = Math.Min(columnsPerRow, ParameterColumns);
            columnPitch = columnsPerRow > 1
                ? (flowWidth - columnsPerRow * ControlWidth) / (columnsPerRow - 1) + ControlWidth
                : ControlWidth;

            rowPitch = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y
                + ImGui.GetFrameHeight() + RowGap;
            var rows = (ParameterColumns + columnsPerRow - 1) / columnsPerRow;

            flowOrigin = new Vector2(origin.X + EdgeMargin, origin.Y);
            flowIndex = 0;

            var paused = panel.Paused;

            LeadingColumns(panel);

            var timebaseWidth = MenuColumn("Timebase (s)");
            var timebase = panel.Timebase;
            if (InputDoubleCombo("##timebase", ref timebase, panel.StandardTimeBases, timebaseWidth))
                panel.Timebase = timebase;

            ImGui.SetNextItemWidth(MenuColumn("Chan. Height (px)"));
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

            var rangeWidth = MenuColumn(
                string.IsNullOrEmpty(RangeLabel) ? "Range" : $"Range ({RangeLabel})");
            var range = panel.RangeAmplitude;
            if (InputDoubleCombo("##range", ref range, panel.StandardRanges, rangeWidth))
                panel.RangeAmplitude = range;

            var paletteWidth = MenuColumn("Palette");
            var swatch = panel.Palette == ColorPalette.Custom
                ? ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X
                : 0f;
            ImGui.SetNextItemWidth(paletteWidth - swatch);
            PaletteCombo(panel);
            if (panel.Palette == ColorPalette.Custom)
            {
                ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
                var customColor = panel.CustomColor;
                if (ImGui.ColorEdit3("##customColor", ref customColor, ImGuiColorEditFlags.NoInputs))
                    panel.CustomColor = customColor;
            }

            var groupWidth = MenuColumn("Color Groups");
            var enabled = panel.ColorGroupingEnabled;
            if (ImGui.Checkbox("##colorGroupingEnabled", ref enabled))
                panel.ColorGroupingEnabled = enabled;

            ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.BeginDisabled(!enabled);
            var grouping = panel.ColorGrouping;
            ImGui.SetNextItemWidth(
                groupWidth - ImGui.GetFrameHeight() - ImGui.GetStyle().ItemInnerSpacing.X);
            if (ImGui.InputInt("##colorGrouping", ref grouping, 1))
                panel.ColorGrouping = grouping;
            ImGui.EndDisabled();

            var style = ImGui.GetStyle();
            var pauseHeight = rowPitch - RowGap;
            var barHeight = ImGui.GetFrameHeight() / 2;
            var stackHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y
                + barHeight + style.ItemSpacing.Y + pauseHeight;

            // NB: at one row the pause zone is taller than the controls beside it, so it sets the height.
            var flowHeight = Math.Max(rows * rowPitch - RowGap, stackHeight);
            var ruleX = MathF.Floor(flowOrigin.X + flowWidth + RuleGap);
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(ruleX, flowOrigin.Y),
                new Vector2(ruleX + ImGuiLfpViewerPanel.FrameWeight, flowOrigin.Y + flowHeight),
                ImGuiLfpViewerPanel.FrameColor);

            // NB: the history bar and pause travel together as one stack, centered: a narrow window wraps
            // the controls into several rows, and a pause button that grew with them would be absurdly tall.
            var stackX = ruleX + ImGuiLfpViewerPanel.FrameWeight + RuleGap;
            var stackY = flowOrigin.Y + (flowHeight - stackHeight) / 2;

            var held = panel.HistoryHeldSeconds;
            var span = panel.HistorySeconds;

            ImGui.SetCursorScreenPos(new Vector2(stackX, stackY));
            PauseButton(panel, new Vector2(pauseWidth, pauseHeight));

            ImGui.SetCursorScreenPos(new Vector2(
                stackX, stackY + pauseHeight + 2 * style.ItemSpacing.Y));
            ImGui.ProgressBar(
                span > 0 ? (float)(held / span) : 0f,
                new Vector2(pauseWidth, barHeight),
                string.Empty);

            ImGui.SetCursorScreenPos(new Vector2(stackX, stackY + pauseHeight + 2 * style.ItemSpacing.Y + ImGui.GetTextLineHeight()));
            ImGui.Text($"Record Length: {held:0.0} / {span:0.0} s");

            // NB: positioning everything outright leaves the parent's content extent where it was, so
            // the whole laid-out area is claimed in one go. It goes last, so that the cursor ends below
            // every row rather than below whichever item happened to be drawn last.
            ImGui.SetCursorScreenPos(origin);
            ImGui.Dummy(new Vector2(available, flowHeight));
        }

        /// <summary>
        /// Takes the next slot in the flow, heads it with <paramref name="label"/>, and leaves the cursor
        /// under it for the control. Returns the width that control should take.
        /// </summary>
        private protected float MenuColumn(string label)
        {
            var row = flowIndex / columnsPerRow;
            var column = flowIndex % columnsPerRow;
            flowIndex++;

            var x = flowOrigin.X + column * columnPitch;
            var y = flowOrigin.Y + row * rowPitch;

            ImGui.SetCursorScreenPos(new Vector2(x, y));
            ImGui.Text(label);
            ImGui.SetCursorScreenPos(new Vector2(
                x, y + ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y));
            return ControlWidth;
        }

        // NB: the preset arrow sits beside the input, so the input takes the slot less the arrow.
        private protected static bool InputDoubleCombo(
            string label, ref double value, IReadOnlyList<double> comboItems, float width)
        {
            var changed = false;
            var editValue = value;
            ImGui.SetNextItemWidth(width - ImGui.GetFrameHeight());
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

        static void PauseButton(ImGuiLfpViewerPanel panel, Vector2 buttonSize)
        {
            var paused = panel.Paused;
            if (paused)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

            if (ImGui.Button("Pause (space)", buttonSize))
                panel.Paused = !paused;

            if (paused)
                ImGui.PopStyleColor();
        }
    }
}
