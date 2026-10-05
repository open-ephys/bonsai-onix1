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
        /// Smallest height a channel row may be given in pixels.
        /// </summary>
        public const int MinChannelHeight = 5;

        /// <summary>
        /// Amplitude ranges offered as presets.
        /// </summary>
        public IReadOnlyList<double> StandardRanges => standardRanges;

        /// <summary>
        /// Timebases offered as presets. While paused, only those the history can fill.
        /// </summary>
        public IReadOnlyList<double> StandardTimeBases =>
            new ArraySegment<double>(standardTimeBases, 0, ServableTimeBases);

        /// <summary>
        /// How many of the offered timebases fit in what the history holds while paused.
        /// </summary>
        int ServableTimeBases
        {
            get
            {
                if (!Paused || history is null)
                    return standardTimeBases.Length;

                var held = (pauseSample - history.Oldest) / (double)sampleRate;
                var count = 0;
                while (count < standardTimeBases.Length && standardTimeBases[count] <= held)
                    count++;

                return Math.Max(1, count);
            }
        }

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

        int colorGrouping = 1;

        /// <summary>
        /// Whether consecutive channels are colored in groups rather than all alike.
        /// </summary>
        public bool ColorGroupingEnabled { get; set; }

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
        public ColorPalette Palette { get; set; } = ColorPalette.OpenEphysGui;

        /// <summary>
        /// Hue the <see cref="ColorPalette.Custom"/> palette is built from.
        /// </summary>
        public Vector3 CustomColor { get; set; } = new(140 / 255f, 219 / 255f, 142 / 255f); // suggested default: a soft green

        /// <summary>
        /// The color channel <paramref name="channel"/> is drawn in.
        /// </summary>
        /// <remarks>
        /// Read by both the trace and its label's hover highlight, so the two always agree.
        /// </remarks>
        Vector4 ChannelColor(int channel) => GroupColor(ColorGroupingEnabled
            ? channel / colorGrouping
            : Palette == ColorPalette.OpenEphysGui ? channel : 0);

        /// <summary>
        /// The color for channel group <paramref name="group"/> under the selected <see
        /// cref="Palette"/>.
        /// </summary>
        /// <remarks>
        /// In <see cref="ColorPalette.Custom"/>, groups are shades of the picked hue, taken out of order from
        /// <see cref="PaletteSteps"/> saturation and value steps so that neighboring groups differ clearly.
        /// Group 0 is the picked color itself.
        /// </remarks>
        Vector4 GroupColor(int group)
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
