using Hexa.NET.ImGui;
using System.Numerics;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// A gradient colorbar with two draggable handles that compress a colormap's full dynamic range into
    /// the interval between them, similar to a gradient-stop editor. The bar's left/right edges represent a
    /// fixed data domain; the handles represent an adjustable sub-range within that domain that is mapped
    /// to the full colormap. Outside the handles the bar shows the colormap's clamped start/end color.
    /// </summary>
    internal static class ImGuiColormapRangeSlider
    {
        // visual constants
        const int   GradientSegments  = 32;
        const float HandleVisualWidth = 3f;
        const float HandleHitWidth    = 14f; // wider than the visual handle, for easier grabbing
        const float HandleOverbitePx  = 3f;  // handle extends this far above/below the bar
        const uint  ColorHandle       = ImGuiPalette.White;
        const uint  ColorHandleActive = ImGuiPalette.BrightFern;
        const uint  ColorText         = ImGuiPalette.White;
        const float TextPadPx         = 4f;
        const float LabelGapPx        = 4f;

        /// <summary>
        /// Draws the control and handles handle-drag interaction. Must be called once per frame.
        /// </summary>
        /// <param name="id">Unique ID for this control's interactive elements.</param>
        /// <param name="colormap">Packed-ABGR lookup table (e.g. <see cref="Plasma.DefaultMap"/>).</param>
        /// <param name="domainMin">Fixed lower bound of the axis (left edge of the bar).</param>
        /// <param name="domainMax">Fixed upper bound of the axis (right edge of the bar).</param>
        /// <param name="rangeMin">Current lower handle value; clamped into [domainMin, rangeMax].</param>
        /// <param name="rangeMax">Current upper handle value; clamped into [rangeMin, domainMax].</param>
        /// <param name="width">Bar width in pixels, or -1 to use the available content region width.</param>
        /// <param name="height">Bar height in pixels, or -1 to use the default ImGui frame height.</param>
        /// <param name="valueFormat">.NET numeric format string used for all labels.</param>
        /// <param name="minHandleTooltip">Tooltip shown while hovering the min handle, or null.</param>
        /// <param name="maxHandleTooltip">Tooltip shown while hovering the max handle, or null.</param>
        /// <returns>True if <paramref name="rangeMin"/> or <paramref name="rangeMax"/> changed this frame.</returns>
        public static bool Draw(
            string id,
            uint[] colormap,
            float domainMin, float domainMax,
            ref float rangeMin, ref float rangeMax,
            float width = -1f,
            float height = -1f,
            string valueFormat = "F1",
            string minHandleTooltip = null,
            string maxHandleTooltip = null)
        {
            float startMin = rangeMin;
            float startMax = rangeMax;

            rangeMin = Clamp(rangeMin, domainMin, domainMax);
            rangeMax = Clamp(rangeMax, rangeMin, domainMax);

            var dl = ImGui.GetWindowDrawList();
            var cp = ImGui.GetCursorScreenPos();
            float avail = width > 0f ? width : ImGui.GetContentRegionAvail().X;
            float barH = height > 0f ? height : ImGui.GetFrameHeight();

            if (avail <= 0f)
            {
                ImGui.Dummy(new Vector2(avail, barH));
                return false;
            }

            string domainMinStr = domainMin.ToString(valueFormat);
            string domainMaxStr = domainMax.ToString(valueFormat);
            float textH = ImGui.CalcTextSize(domainMinStr).Y;
            float labelY = cp.Y + barH + LabelGapPx;
            float domainSpan = domainMax - domainMin;

            if (domainSpan <= 0f)
            {
                // Degenerate domain (e.g. all data identical): nothing meaningful to drag.
                dl.AddRectFilled(cp, new Vector2(cp.X + avail, cp.Y + barH), colormap[colormap.Length / 2]);
                float w = ImGui.CalcTextSize(domainMinStr).X;
                dl.AddText(new Vector2(cp.X + avail / 2f - w / 2f, labelY), ColorText, domainMinStr);
                ImGui.SetCursorScreenPos(cp);
                ImGui.Dummy(new Vector2(avail, barH + LabelGapPx + textH));
                return false;
            }

            float ValueToX(float v) => cp.X + (v - domainMin) / domainSpan * avail;
            float XToValue(float x) => domainMin + (x - cp.X) / avail * domainSpan;

            float xMin = ValueToX(rangeMin);
            float xMax = ValueToX(rangeMax);

            if (xMin > cp.X)
                dl.AddRectFilled(cp, new Vector2(xMin, cp.Y + barH), colormap[0]);

            if (xMax > xMin)
            {
                for (int s = 0; s < GradientSegments; s++)
                {
                    int idxL = (s * (colormap.Length - 1)) / GradientSegments;
                    int idxR = ((s + 1) * (colormap.Length - 1)) / GradientSegments;
                    float x0 = xMin + (xMax - xMin) * s / GradientSegments;
                    float x1 = xMin + (xMax - xMin) * (s + 1) / GradientSegments;
                    dl.AddRectFilledMultiColor(
                        new Vector2(x0, cp.Y), new Vector2(x1, cp.Y + barH),
                        colormap[idxL], colormap[idxR], colormap[idxR], colormap[idxL]);
                }
            }

            if (xMax < cp.X + avail)
                dl.AddRectFilled(new Vector2(xMax, cp.Y), new Vector2(cp.X + avail, cp.Y + barH), colormap[colormap.Length - 1]);

            // Min handle (submitted first so an overlapping max handle wins the grab).
            ImGui.SetCursorScreenPos(new Vector2(xMin - HandleHitWidth / 2f, cp.Y - HandleOverbitePx));
            ImGui.InvisibleButton(id + "_minHandle", new Vector2(HandleHitWidth, barH + 2f * HandleOverbitePx));
            bool minActive = ImGui.IsItemActive();
            bool minHovered = ImGui.IsItemHovered();
            if (minActive)
                rangeMin = Clamp(XToValue(ImGui.GetMousePos().X), domainMin, rangeMax);
            if (minHandleTooltip != null && minHovered && !minActive)
                ImGui.SetTooltip(minHandleTooltip);

            // Max handle.
            ImGui.SetCursorScreenPos(new Vector2(xMax - HandleHitWidth / 2f, cp.Y - HandleOverbitePx));
            ImGui.InvisibleButton(id + "_maxHandle", new Vector2(HandleHitWidth, barH + 2f * HandleOverbitePx));
            bool maxActive = ImGui.IsItemActive();
            bool maxHovered = ImGui.IsItemHovered();
            if (maxActive)
                rangeMax = Clamp(XToValue(ImGui.GetMousePos().X), rangeMin, domainMax);
            if (maxHandleTooltip != null && maxHovered && !maxActive)
                ImGui.SetTooltip(maxHandleTooltip);

            // Recompute handle X positions in case dragging just moved them, then draw the handle bars on top.
            xMin = ValueToX(rangeMin);
            xMax = ValueToX(rangeMax);
            dl.AddRectFilled(
                new Vector2(xMin - HandleVisualWidth / 2f, cp.Y - HandleOverbitePx),
                new Vector2(xMin + HandleVisualWidth / 2f, cp.Y + barH + HandleOverbitePx),
                minActive || minHovered ? ColorHandleActive : ColorHandle);
            dl.AddRectFilled(
                new Vector2(xMax - HandleVisualWidth / 2f, cp.Y - HandleOverbitePx),
                new Vector2(xMax + HandleVisualWidth / 2f, cp.Y + barH + HandleOverbitePx),
                maxActive || maxHovered ? ColorHandleActive : ColorHandle);

            // Labels: the two domain extremes are always shown; each handle's own value is shown beneath it
            // unless that would overlap a domain label or the other handle's label (common at rest, when
            // both handles sit exactly at the domain extremes and would otherwise duplicate those labels).
            string minStr = rangeMin.ToString(valueFormat);
            string maxStr = rangeMax.ToString(valueFormat);

            float domainMinX0 = cp.X + TextPadPx;
            float domainMinX1 = domainMinX0 + ImGui.CalcTextSize(domainMinStr).X;
            float domainMaxX1 = cp.X + avail - TextPadPx;
            float domainMaxX0 = domainMaxX1 - ImGui.CalcTextSize(domainMaxStr).X;

            float minLabelW = ImGui.CalcTextSize(minStr).X;
            float minLabelX0 = xMin - minLabelW / 2f;
            float minLabelX1 = xMin + minLabelW / 2f;

            float maxLabelW = ImGui.CalcTextSize(maxStr).X;
            float maxLabelX0 = xMax - maxLabelW / 2f;
            float maxLabelX1 = xMax + maxLabelW / 2f;

            bool showMinLabel = !Overlaps(minLabelX0, minLabelX1, domainMinX0, domainMinX1);
            bool showMaxLabel = !Overlaps(maxLabelX0, maxLabelX1, domainMaxX0, domainMaxX1);
            if (showMinLabel && showMaxLabel && Overlaps(minLabelX0, minLabelX1, maxLabelX0, maxLabelX1))
                showMaxLabel = false;

            dl.AddText(new Vector2(domainMinX0, labelY), ColorText, domainMinStr);
            dl.AddText(new Vector2(domainMaxX0, labelY), ColorText, domainMaxStr);
            if (showMinLabel)
                dl.AddText(new Vector2(minLabelX0, labelY), ColorText, minStr);
            if (showMaxLabel)
                dl.AddText(new Vector2(maxLabelX0, labelY), ColorText, maxStr);

            ImGui.SetCursorScreenPos(cp);
            ImGui.Dummy(new Vector2(avail, barH + LabelGapPx + textH));

            return rangeMin != startMin || rangeMax != startMax;
        }

        static bool Overlaps(float a0, float a1, float b0, float b1) => a0 < b1 + LabelGapPx && b0 < a1 + LabelGapPx;

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
