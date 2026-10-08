using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiLfpViewerPanel
    {
        static readonly double[] standardRanges =
        {
            50, 100, 250, 500, 1000, 2500, 5000, 10000
        };

        /// <summary>
        /// Smallest height a channel row may be given in pixels.
        /// </summary>
        public const int MinChannelHeight = 1;

        /// <summary>
        /// Pixels the channel height steps by from the control strip.
        /// </summary>
        public const int ChannelHeightStep = 5;

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

        /// <summary>
        /// The colors channels are drawn in.
        /// </summary>
        public TraceColors Colors { get; } = new();
    }
}
