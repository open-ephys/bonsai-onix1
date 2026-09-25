using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    internal enum ColorPalette
    {
        OpenEphysGui,
        Custom,
    }

    partial class ImGuiLfpViewerPanel
    {
        const int PaletteSteps = 16;

        static readonly double[] standardRanges =
        {
            50, 100, 250, 500, 1000, 2500, 5000, 10000
        };

        /// <summary>
        /// Smallest height a channel row may be given.
        /// </summary>
        /// <remarks>
        /// A constant for now. It is a property rather than a constant so that it can later be
        /// derived from the pane height and the channel count without anything driving the panel
        /// having to change.
        /// </remarks>
        public int MinChannelHeight => 2;

        /// <summary>
        /// Amplitude ranges offered as presets.
        /// </summary>
        public IReadOnlyList<double> StandardRanges => standardRanges;

        /// <summary>
        /// Timebases offered as presets, capped by what the history behind the display can serve.
        /// </summary>
        public IReadOnlyList<double> StandardTimeBases => standardTimeBases;

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

        /// <summary>
        /// Whether consecutive channels are colored in groups rather than all alike.
        /// </summary>
        public bool ColorGroupingEnabled
        {
            get => colorGroupingEnabled;
            set => colorGroupingEnabled = value;
        }

        /// <summary>
        /// Number of consecutive channels sharing a color when <see cref="ColorGroupingEnabled"/>.
        /// </summary>
        public int ColorGrouping
        {
            get => colorGrouping;
            set => colorGrouping = Math.Max(1, value);
        }

        /// <summary>
        /// Palette the trace colors are drawn from.
        /// </summary>
        public ColorPalette Palette
        {
            get => palette;
            set => palette = value;
        }

        /// <summary>
        /// Hue the <see cref="ColorPalette.Custom"/> palette is built from.
        /// </summary>
        public Vector3 CustomColor
        {
            get => customColor;
            set => customColor = value;
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

    }
}
