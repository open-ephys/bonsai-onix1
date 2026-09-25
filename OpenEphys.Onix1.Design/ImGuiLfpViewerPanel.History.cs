using Hexa.NET.ImGui;
using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiLfpViewerPanel
    {
        Mat pausedWaveformMin;
        Mat pausedWaveformMax;
        int pausedCursor;

        long pauseSample = -1;
        long pauseViewStart;
        long viewStart;
        bool viewDirty;

        Mat windowScratch;
        Decimator pannedWaveformMinDecimator;
        Decimator pannedWaveformMaxDecimator;

        /// <summary>
        /// Whether the paused view has been moved off the samples that were on screen when it was paused.
        /// </summary>
        bool Panned => Paused && viewStart != pauseViewStart;

        /// <summary>
        /// Samples spanned by the display, which is also the distance the view must travel for the stale
        /// segment to leave the screen.
        /// </summary>
        int WindowSamples => waveformMinDecimator.Buffer.Cols * waveformMinDecimator.DownsampleFactor;

        /// <summary>
        /// Column the pause instant falls at in the panned view, past the last column once the view has
        /// moved a whole stale segment back.
        /// </summary>
        int PannedCursorColumn => (int)((pauseSample - viewStart) / waveformMinDecimator.DownsampleFactor);

        /// <summary>
        /// Freezes the display on what is drawn now.
        /// </summary>
        /// <remarks>
        /// The cursor column holds the newest data, so the columns to its right are a whole window older.
        /// Column zero is therefore <c>cursor</c> bins behind the newest sample, and the view runs past
        /// that sample into what the ring has not yet overwritten.
        /// </remarks>
        void Pause()
        {
            if (waveformMinDecimator is null)
                return;

            pausedWaveformMin = waveformMinDecimator.Buffer.Clone();
            pausedWaveformMax = waveformMaxDecimator.Buffer.Clone();
            pausedCursor = waveformMinDecimator.Cursor;

            pauseSample = history?.Count ?? 0;
            pauseViewStart = pauseSample
                - (long)waveformMinDecimator.Cursor * waveformMinDecimator.DownsampleFactor;
            viewStart = pauseViewStart;
            viewDirty = false;
        }

        /// <remarks>
        /// History stops being written while paused, so that what the user paused on cannot expire while
        /// they look at it. That leaves a gap in the timeline, which the buffer is told to abandon rather
        /// than let a later view read across it.
        /// </remarks>
        void Resume()
        {
            pausedWaveformMin?.Dispose();
            pausedWaveformMax?.Dispose();
            pausedWaveformMin = null;
            pausedWaveformMax = null;

            pauseSample = -1;
            viewDirty = false;
            history?.Clear();
        }

        /// <summary>
        /// Moves the view back through history by <paramref name="samples"/>, or forward when negative,
        /// as far as the buffer can serve.
        /// </summary>
        void Pan(long samples)
        {
            if (!Paused || history is null)
                return;

            var window = WindowSamples;

            // NB: the overhang past the pause instant is read a whole window earlier, so panning at all
            // needs that much history behind it.
            if (pauseSample - window < history.Oldest)
                return;

            var moved = Math.Max(history.Oldest, Math.Min(pauseViewStart, viewStart - samples));
            if (moved == viewStart)
                return;

            viewStart = moved;
            viewDirty = true;
        }

        /// <summary>
        /// Reads the samples the panned view covers, taking the part that runs past the pause instant a
        /// whole window earlier so that the stale segment stays where it was drawn, and reduces them.
        /// </summary>
        /// <returns>False if the buffer can no longer serve the view.</returns>
        bool TryReadPannedView()
        {
            var window = WindowSamples;
            if (windowScratch is null || windowScratch.Cols != window || windowScratch.Rows != history.Rows)
            {
                windowScratch?.Dispose();
                pannedWaveformMinDecimator?.Dispose();
                pannedWaveformMaxDecimator?.Dispose();
                windowScratch = new Mat(history.Rows, window, Depth.F32, 1);
                var columns = waveformMinDecimator.Buffer.Cols;
                var factor = waveformMinDecimator.DownsampleFactor;
                pannedWaveformMinDecimator = new Decimator(windowScratch, columns, factor, ReduceOperation.Min);
                pannedWaveformMaxDecimator = new Decimator(windowScratch, columns, factor, ReduceOperation.Max);
            }

            var overhang = (int)Math.Max(0, viewStart + window - pauseSample);
            var ahead = window - overhang;

            using (var head = windowScratch.GetSubRect(new Rect(0, 0, ahead, windowScratch.Rows)))
            {
                if (!history.CopyWindow(viewStart, head))
                    return false;
            }

            if (overhang > 0)
            {
                using var tail = windowScratch.GetSubRect(new Rect(ahead, 0, overhang, windowScratch.Rows));
                if (!history.CopyWindow(pauseSample - window, tail))
                    return false;
            }

            pannedWaveformMinDecimator.Reset();
            pannedWaveformMinDecimator.Process(windowScratch);
            pannedWaveformMaxDecimator.Reset();
            pannedWaveformMaxDecimator.Process(windowScratch);
            return true;
        }

        /// <summary>
        /// The envelope to draw this frame: two matrices of one row per channel and one column per bin across
        /// the plot, holding the smallest and largest sample that fell in each decimation bin, which the plot
        /// fills between and outlines.
        /// </summary>
        (Mat WaveformMin, Mat WaveformMax) DisplayEnvelope()
        {
            if (!Paused)
                return (waveformMinDecimator.Buffer, waveformMaxDecimator.Buffer);

            if (!Panned)
                return (pausedWaveformMin, pausedWaveformMax);

            // NB: Pan refuses to move a view the buffer cannot serve, so this is a backstop; returning
            // to where the pause left the view is always safe.
            if (viewDirty && !TryReadPannedView())
            {
                viewStart = pauseViewStart;
                return (pausedWaveformMin, pausedWaveformMax);
            }

            viewDirty = false;
            return (pannedWaveformMinDecimator.Buffer, pannedWaveformMaxDecimator.Buffer);
        }

        /// <summary>
        /// Moves the paused view: dragging the time labels below the plot, or A and D.
        /// </summary>
        /// <remarks>
        /// A drag carries the samples under the pointer with it, so the trace follows the mouse rather
        /// than merely responding to it. The keys step by a tenth of the view, which is one time
        /// division, and answer wherever the pointer is, as the pause key does, so that the left hand
        /// can move the view while the right stays on the mouse.
        /// </remarks>
        void HandlePanInput(float left, float width, float top)
        {
            if (!Paused)
                return;

            var mouse = ImGui.GetMousePos();
            var overLabels = ImGui.IsWindowHovered() &&
                mouse.X >= left && mouse.X < left + width &&
                mouse.Y >= top && mouse.Y < top + ImGui.GetTextLineHeight();

            if (overLabels && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                Pan((long)(ImGui.GetIO().MouseDelta.X * (WindowSamples / width)));

            if (ImGui.IsKeyPressed(ImGuiKey.A))
                Pan(WindowSamples / TimeDivisions);
            else if (ImGui.IsKeyPressed(ImGuiKey.D))
                Pan(-WindowSamples / TimeDivisions);
        }

        void DisposeHistoryView()
        {
            pausedWaveformMin?.Dispose();
            pausedWaveformMax?.Dispose();
            pausedWaveformMin = null;
            pausedWaveformMax = null;
            windowScratch?.Dispose();
            pannedWaveformMinDecimator?.Dispose();
            pannedWaveformMaxDecimator?.Dispose();
            windowScratch = null;
            pannedWaveformMinDecimator = null;
            pannedWaveformMaxDecimator = null;
            pauseSample = -1;
        }
    }
}
