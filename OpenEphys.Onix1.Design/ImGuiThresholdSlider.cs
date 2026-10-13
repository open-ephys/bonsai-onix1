using Hexa.NET.ImGui;
using System;
using System.Numerics;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// A bar showing both halves of a diverging colormap stacked, each from the colormap's middle out to its end,
    /// with one handle below which values are not shown, and the handle's value written under it.
    /// </summary>
    internal static class ImGuiThresholdSlider
    {
        const int GradientSegments = 32;
        const float HandleVisualWidth = 3f;
        const float HandleOverbitePx = 3f;
        const float LabelGapPx = 2f;
        const uint ColorHandle = ImGuiPalette.White;
        const uint ColorHandleActive = ImGuiPalette.BrightFern;
        const uint ColorText = ImGuiPalette.White;

        /// <summary>
        /// Draws the bar and handles dragging it. Must be called once per frame.
        /// </summary>
        /// <param name="id">Unique ID for the bar.</param>
        /// <param name="colormap">Packed-ABGR diverging lookup table, its middle at zero.</param>
        /// <param name="domainMax">Magnitude at the bar's right end, where the colormap's ends fall.</param>
        /// <param name="threshold">Magnitude at or below which values are not shown; clamped into [0, domainMax].</param>
        /// <param name="width">Bar width in pixels.</param>
        /// <param name="height">Bar height in pixels, not counting the value written under it.</param>
        /// <param name="valueFormat">.NET numeric format string for the value under the handle.</param>
        /// <param name="tooltip">Tooltip shown while hovering the bar, or null.</param>
        /// <returns>True if <paramref name="threshold"/> changed this frame.</returns>
        public static bool Draw(
            string id, uint[] colormap, float domainMax, ref float threshold, float width, float height,
            string valueFormat = "0", string tooltip = null)
        {
            var start = threshold;
            threshold = Math.Max(0, Math.Min(domainMax, threshold));

            var draw = ImGui.GetWindowDrawList();
            var corner = ImGui.GetCursorScreenPos();
            var bottom = corner.Y + height;
            var labelY = bottom + LabelGapPx;
            var total = height + LabelGapPx + ImGui.GetTextLineHeight();

            // NB: the whole control takes the drag, value included, since with one handle a click anywhere means
            // only one thing and the bar alone is a thin target. It reaches out to the handle's overhang too, so
            // that everywhere the handle is drawn lights it.
            ImGui.SetCursorScreenPos(new Vector2(corner.X - HandleVisualWidth / 2, corner.Y - HandleOverbitePx));
            ImGui.InvisibleButton(id, new Vector2(width + HandleVisualWidth, total + HandleOverbitePx));
            var active = ImGui.IsItemActive();
            var hovered = ImGui.IsItemHovered();
            if (active && domainMax > 0)
                threshold = Math.Max(0, Math.Min(domainMax, (ImGui.GetMousePos().X - corner.X) / width * domainMax));
            if (tooltip != null && hovered && !active)
                ImGui.SetTooltip(tooltip);

            // NB: the positive half goes on top, as positive is up in the traces.
            var middle = colormap.Length / 2;
            var split = corner.Y + height / 2;
            for (int s = 0; s < GradientSegments; s++)
            {
                var step0 = s * (middle - 1) / GradientSegments;
                var step1 = (s + 1) * (middle - 1) / GradientSegments;
                var x0 = corner.X + width * s / GradientSegments;
                var x1 = corner.X + width * (s + 1) / GradientSegments;
                Gradient(draw, x0, x1, corner.Y, split, colormap[middle + step0], colormap[middle + step1]);
                Gradient(draw, x0, x1, split, bottom, colormap[middle - 1 - step0], colormap[middle - 1 - step1]);
            }

            var x = domainMax > 0 ? corner.X + threshold / domainMax * width : corner.X;
            if (x > corner.X)
                draw.AddRectFilled(corner, new Vector2(x, bottom), ImGuiPalette.Black);

            draw.AddRectFilled(
                new Vector2(x - HandleVisualWidth / 2, corner.Y - HandleOverbitePx),
                new Vector2(x + HandleVisualWidth / 2, bottom + HandleOverbitePx),
                active || hovered ? ColorHandleActive : ColorHandle);

            var label = threshold.ToString(valueFormat);
            var labelWidth = ImGui.CalcTextSize(label).X;
            var labelX = Math.Max(corner.X, Math.Min(corner.X + width - labelWidth, x - labelWidth / 2));
            draw.AddText(new Vector2(labelX, labelY), ColorText, label);

            return threshold != start;
        }

        static void Gradient(ImDrawListPtr draw, float x0, float x1, float y0, float y1, uint left, uint right) =>
            draw.AddRectFilledMultiColor(new Vector2(x0, y0), new Vector2(x1, y1), left, right, right, left);
    }
}
