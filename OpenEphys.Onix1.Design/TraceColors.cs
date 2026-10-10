using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    internal enum ColorPalette
    {
        OpenEphysGui,
        Custom,
    }

    /// <summary>
    /// The colors channels are drawn in: a palette, and whether consecutive channels share a color in groups.
    /// </summary>
    internal sealed class TraceColors
    {
        const int PaletteSteps = 16;

        int grouping = 1;
        uint[] packed = Array.Empty<uint>();

        /// <summary>
        /// Whether consecutive channels are colored in groups rather than all alike.
        /// </summary>
        public bool GroupingEnabled { get; set; }

        /// <summary>
        /// Number of consecutive channels sharing a color when <see cref="GroupingEnabled"/>.
        /// </summary>
        public int Grouping
        {
            get => grouping;
            set => grouping = Math.Max(1, value);
        }

        /// <summary>
        /// Palette the colors are drawn from.
        /// </summary>
        public ColorPalette Palette { get; set; } = ColorPalette.OpenEphysGui;

        /// <summary>
        /// Hue the <see cref="ColorPalette.Custom"/> palette is built from.
        /// </summary>
        public Vector3 CustomColor { get; set; } = new(140 / 255f, 219 / 255f, 142 / 255f); // suggested default: a soft green

        /// <summary>
        /// The color channel <paramref name="channel"/> is drawn in.
        /// </summary>
        /// <remarks>
        /// Read by the trace, its outline and its label's highlight alike, so they always agree.
        /// </remarks>
        public Vector4 Of(int channel) => Group(GroupingEnabled
            ? channel / grouping
            : Palette == ColorPalette.OpenEphysGui ? channel : 0);

        /// <summary>
        /// The colors of the first <paramref name="channels"/> channels, packed as RGBA, for drawing in bulk. The
        /// array is reused from call to call.
        /// </summary>
        public uint[] Packed(int channels)
        {
            if (packed.Length != channels)
                packed = new uint[channels];

            for (int i = 0; i < channels; i++)
            {
                var c = Of(i);
                packed[i] = (uint)(c.X * 255) | (uint)(c.Y * 255) << 8 | (uint)(c.Z * 255) << 16 | 0xFFu << 24;
            }

            return packed;
        }

        /// <summary>
        /// The color for channel group <paramref name="group"/> under the selected <see cref="Palette"/>.
        /// </summary>
        /// <remarks>
        /// In <see cref="ColorPalette.Custom"/>, groups are shades of the picked hue, taken out of order from
        /// <see cref="PaletteSteps"/> saturation and value steps so that neighboring groups differ clearly.
        /// Group 0 is the picked color itself.
        /// </remarks>
        Vector4 Group(int group)
        {
            if (Palette == ColorPalette.OpenEphysGui)
                return ImGui.ColorConvertU32ToFloat4(ImGuiPalette.OpenEphysGuiLfp[group % ImGuiPalette.OpenEphysGuiLfp.Length]);

            float hue = 0, saturation = 0, value = 0;
            ImGui.ColorConvertRGBtoHSV(CustomColor.X, CustomColor.Y, CustomColor.Z, ref hue, ref saturation, ref value);

            var step = group * 7 % PaletteSteps;
            var fraction = step / (float)(PaletteSteps - 1);
            saturation = Math.Min(1f, saturation + 0.4f * fraction);
            value *= 1f - 0.6f * fraction;

            float r = 0, g = 0, b = 0;
            ImGui.ColorConvertHSVtoRGB(hue, saturation, value, ref r, ref g, ref b);
            return new Vector4(r, g, b, 1f);
        }
    }
}
