using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Text drawn over a plot: readings, labels and the boxes they sit in.
    /// </summary>
    internal static class PlotText
    {
        /// <summary>
        /// Fill behind text drawn over traces, dark enough to read against any of them.
        /// </summary>
        public static readonly uint LabelBackground = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0xBB);

        /// <summary>
        /// Writes <paramref name="text"/> at <paramref name="corner"/> in a box framed like the cursor table, or
        /// only filled where <paramref name="outline"/> is false.
        /// </summary>
        /// <remarks>
        /// The cursor table is a bordered child window, so the box takes its rounding and border color from the
        /// style, and the overlays on the plot read as one set.
        /// </remarks>
        public static void Framed(ImDrawListPtr draw, Vector2 corner, string text, bool outline = true)
        {
            var pad = ImGui.GetStyle().FramePadding;
            var rounding = ImGui.GetStyle().ChildRounding;
            var size = ImGui.CalcTextSize(text);
            draw.AddRectFilled(corner - pad, corner + size + pad, LabelBackground, rounding);
            if (outline)
                draw.AddRect(corner - pad, corner + size + pad, ImGui.GetColorU32(ImGuiCol.Border), rounding);
            draw.AddText(corner, ImGui.GetColorU32(ImGuiCol.Text), text);
        }

        // NB: three significant figures, as fine as a value read off a trace by eye is worth.
        public static double Significant(double value)
        {
            if (value == 0)
                return 0;

            var decimals = 2 - (int)Math.Floor(Math.Log10(Math.Abs(value)));
            return Math.Round(value, Math.Max(0, Math.Min(15, decimals)));
        }
    }
}
