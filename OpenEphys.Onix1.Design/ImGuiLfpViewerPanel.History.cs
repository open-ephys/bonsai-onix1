using Hexa.NET.ImGui;
using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <remarks>
    /// <para>
    /// A paused view is laid out on an axis of positions, in samples, with zero at the first sample of the
    /// frozen sweep. The frozen frame covers zero to its own width, and panning back reaches negative
    /// positions, down to the oldest sample the history holds.
    /// </para>
    /// <para>
    /// Left of the sweep cursor, a position shows the sample at that position. From the cursor on, the frame
    /// was still showing the previous sweep, so a position there shows the sample one frozen width earlier.
    /// Two positions can therefore show the same sample, so the view is moved by position, never by sample.
    /// </para>
    /// </remarks>
    partial class ImGuiLfpViewerPanel
    {
        Mat pausedWaveformMin;
        Mat pausedWaveformMax;

        long pauseSample = -1;
        long sweepOrigin;

        /// <summary>
        /// The frame the display was frozen on. The tail past the cursor is one of its widths behind.
        /// </summary>
        DisplayWindow frozen;

        // NB: the frozen frame's width in seconds as it was asked for, which is how much older the samples
        // past the cursor are. Taken from the timebase rather than from the frame's own span, whose whole
        // number of columns falls a little short of it and would put a ragged number on every division.
        double pausedTimebase;

        DisplayWindow window;
        bool windowDirty;
        long panCarry;

        long anchorPosition;
        double anchorFraction;
        bool anchorPending;

        Decimator pannedWaveformMinDecimator;
        Decimator pannedWaveformMaxDecimator;

        /// <summary>
        /// Seconds of signal the history can hold at the current sample rate.
        /// </summary>
        public double HistorySeconds => history is null ? 0 : history.Capacity / (double)sampleRate;

        /// <summary>
        /// Seconds of signal the history currently holds. It does not grow while paused, when the history is
        /// not written.
        /// </summary>
        public double HistoryHeldSeconds =>
            history is null ? 0 : (history.Count - history.Oldest) / (double)sampleRate;

        /// <summary>
        /// Fewest frames a sweep must take to be seen moving across the screen rather than jumping.
        /// </summary>
        const int FramesToFollowSweep = 10;

        /// <summary>
        /// Whether a live sweep is too fast to show as it is written, so that whole sweeps are drawn instead, as a
        /// scope does at fast sweeps.
        /// </summary>
        /// <remarks>
        /// A sweep is drawn in as many steps as there are frames during it, and too few steps read as jumps, not
        /// motion. Under about two, each frame also catches the sweep somewhere new, so its cursor appears to move
        /// at a speed that is not its own and the seam between the new sweep and the last jumps about.
        /// </remarks>
        bool FastSweep =>
            !Paused && waveformMinDecimator?.LastSweep is not null &&
            timebase * RefreshRate <= FramesToFollowSweep;

        /// <summary>
        /// Whether the paused view has been moved off the frame that was frozen.
        /// </summary>
        bool Panned => Paused && window.Start != 0;

        /// <summary>
        /// Whether the buffers copied at pause still match the display's columns, which a timebase change
        /// alters.
        /// </summary>
        bool SnapshotCurrent =>
            pausedWaveformMin is not null &&
            frozen.Columns == window.Columns &&
            frozen.Step == window.Step;

        /// <summary>
        /// Axis position of the sweep cursor, which is where the newest sample meets the frozen tail.
        /// </summary>
        long CursorPosition => pauseSample - sweepOrigin;

        /// <summary>
        /// The sample drawn at <paramref name="position"/>.
        /// </summary>
        long SampleAt(long position) =>
            sweepOrigin + position - (position >= CursorPosition ? frozen.Span : 0);

        /// <summary>
        /// Freezes the display on what is drawn now.
        /// </summary>
        /// <remarks>
        /// The sweep began <c>Cursor</c> columns before the newest sample, and the frame runs one full width
        /// from there, into the tail of the previous sweep.
        /// </remarks>
        void Pause()
        {
            if (waveformMinDecimator is null)
                return;

            pausedWaveformMin = waveformMinDecimator.Sweep.Clone();
            pausedWaveformMax = waveformMaxDecimator.Sweep.Clone();
            frozen = new DisplayWindow(
                0, waveformMinDecimator.DownsampleFactor, waveformMinDecimator.Sweep.Cols);
            pausedTimebase = timebase;

            pauseSample = history?.Count ?? 0;

            // NB: the cursor column is part filled, and by this much, so it began that far before the
            // newest sample rather than on it. Without the term the whole axis sits up to a column early,
            // and a view read back out of the history groups its samples differently from the live one.
            sweepOrigin = pauseSample - waveformMinDecimator.Filled
                - (long)waveformMinDecimator.Cursor * frozen.Step;

            window = frozen;
            windowDirty = false;
            anchorPending = false;
        }

        /// <remarks>
        /// The history is not written while paused, so that what was paused on cannot expire. Resuming
        /// clears it rather than leave a gap that a later view could read across.
        /// </remarks>
        void Resume()
        {
            pausedWaveformMin?.Dispose();
            pausedWaveformMax?.Dispose();
            pausedWaveformMin = null;
            pausedWaveformMax = null;

            pauseSample = -1;
            windowDirty = false;
            anchorPending = false;
            window = new DisplayWindow(0, window.Step, window.Columns);
            history?.Clear();
        }

        /// <summary>
        /// Oldest position the history can serve.
        /// </summary>
        long AxisFirst => history.Oldest - sweepOrigin;

        /// <summary>
        /// Position one past the right edge of the frame that was frozen, which is as far forward as there
        /// is anything to draw.
        /// </summary>
        long AxisLast => frozen.Span;

        /// <summary>
        /// Shows <paramref name="value"/>, as much of it as the axis reaches.
        /// </summary>
        void SetWindow(DisplayWindow value)
        {
            // NB: the start is kept a whole number of columns from the sweep origin, as the live decimators'
            // columns are, so a column always covers the same samples and a pan moves what is drawn without
            // binning it afresh, which would change the envelope and what a cursor reads.
            value = value.Clamp(AxisFirst, AxisLast);
            var step = value.Step;
            var start = (long)Math.Round(value.Start / (double)step) * step;
            if (start < AxisFirst)
                start += step;
            else if (start + value.Span > AxisLast && start - step >= AxisFirst)
                start -= step;

            value = new DisplayWindow(start, step, value.Columns);
            if (value.Matches(window))
                return;

            window = value;
            windowDirty = true;
        }

        /// <summary>
        /// Adopts the column spacing the decimators were just rebuilt at, keeping in place whatever the last
        /// timebase change asked to hold.
        /// </summary>
        /// <remarks>
        /// Also called live, where the window only places the divisions.
        /// </remarks>
        void RebuildWindow(int step, int columns)
        {
            if (!Paused || history is null)
            {
                window = new DisplayWindow(0, step, columns);
                anchorPending = false;
                return;
            }

            var anchor = anchorPending ? anchorPosition : window.Start;
            var fraction = anchorPending ? anchorFraction : 0;
            anchorPending = false;
            SetWindow(window.Rescale(step, columns, anchor, fraction));

            // NB: a decimator starts empty, so the columns have to be read again whether or not the
            // window moved, and a window already where the new spacing wants it does not move.
            windowDirty = true;
        }

        /// <summary>
        /// Moves the view back through history by <paramref name="distance"/>, or forward when negative.
        /// </summary>
        void Pan(long distance)
        {
            if (!Paused || history is null)
                return;

            // NB: the frozen tail is read from one frozen width behind the pause instant, so a history that
            // no longer reaches back that far cannot serve any view that shows it.
            if (pauseSample - frozen.Span < history.Oldest)
                return;

            // NB: whole columns only, with the rest carried to the next pan, so that a slow drag still moves.
            panCarry += distance;
            var columns = panCarry / window.Step;
            panCarry -= columns * window.Step;
            SetWindow(window.Shift(-columns * window.Step));
        }

        /// <summary>
        /// Reduces the samples the window covers straight out of the history.
        /// </summary>
        /// <remarks>
        /// Read as at most two runs, split where the frozen tail begins. The decimators carry a part-filled
        /// column from one run into the next, so a column across the split is reduced from both.
        /// </remarks>
        /// <returns>False if the history can no longer serve the window.</returns>
        bool TryReadWindow()
        {
            if (pannedWaveformMinDecimator is null)
                return false;

            var split = Math.Max(window.Start, Math.Min(window.End, CursorPosition));
            var fresh = (int)(split - window.Start);
            var tail = (int)(window.End - split);

            pannedWaveformMinDecimator.Reset();
            pannedWaveformMaxDecimator.Reset();

            if (fresh > 0 && !history.Decimate(
                    SampleAt(window.Start), fresh, pannedWaveformMinDecimator, pannedWaveformMaxDecimator))
                return false;

            return tail <= 0 || history.Decimate(
                SampleAt(split), tail, pannedWaveformMinDecimator, pannedWaveformMaxDecimator);
        }

        /// <summary>
        /// The axis position drawn at <paramref name="fraction"/> across the plot.
        /// </summary>
        long PositionAtFraction(double fraction)
        {
            // NB: the last column, not one past it. The fraction reaches one on the rightmost pixel.
            var column = (int)(fraction * (window.Columns - 1));
            return window.PositionOf(column);
        }

        /// <summary>
        /// Changes the timebase, holding the axis position <paramref name="anchor"/> at the place it
        /// currently occupies.
        /// </summary>
        /// <remarks>
        /// The new column spacing is not known until <see cref="RebuildBuffers"/> runs, so the anchor is
        /// recorded here and the window solved there.
        /// </remarks>
        void SetTimebase(double value, long anchor, double fraction)
        {
            // NB: a backstop for a value typed into the combo rather than picked from it. The offered
            // list already stops at the longest window the history can serve.
            if (Paused)
                value = Math.Min(value, standardTimeBases[ServableTimeBases - 1]);

            if (value == timebase)
                return;

            if (Paused)
            {
                anchorPosition = anchor;
                anchorFraction = fraction;
                anchorPending = true;
            }

            timebase = value;
        }

        /// <summary>
        /// The min and max buffers to draw this frame: the live ones, the copies taken at pause, or ones read
        /// from the history for a view that has been moved.
        /// </summary>
        (Mat WaveformMin, Mat WaveformMax) DisplayEnvelope()
        {
            if (FastSweep)
                return (waveformMinDecimator.LastSweep, waveformMaxDecimator.LastSweep);

            if (!Paused)
                return (waveformMinDecimator.Sweep, waveformMaxDecimator.Sweep);

            if (!Panned && SnapshotCurrent)
                return (pausedWaveformMin, pausedWaveformMax);

            // NB: a backstop. Whatever the read reached before it gave up stands, so the plot draws a
            // gap or part of a frame, and the window goes back to the frozen frame to be tried again next
            // frame. Returning the live buffers here would show the trace running while the cursor and the
            // pause button both say it is frozen.
            if (windowDirty && !TryReadWindow())
            {
                SetWindow(new DisplayWindow(0, window.Step, window.Columns));
                return pannedWaveformMinDecimator is null
                    ? (waveformMinDecimator.Sweep, waveformMaxDecimator.Sweep)
                    : (pannedWaveformMinDecimator.Sweep, pannedWaveformMaxDecimator.Sweep);
            }

            windowDirty = false;
            return (pannedWaveformMinDecimator.Sweep, pannedWaveformMaxDecimator.Sweep);
        }

        /// <summary>
        /// Moves the paused view by dragging the time labels, or with A and D.
        /// </summary>
        /// <remarks>
        /// A drag keeps the samples under the pointer. A and D step one division, or most of a window with
        /// Shift, and work wherever the pointer is, like the pause key.
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
                Pan((long)(ImGui.GetIO().MouseDelta.X * (window.Span / width)));

            // NB: a division at a time, or most of a window with Shift, which is the same relationship
            // W and S have for channels.
            var step = Modifiers(shift: true)
                ? (long)(window.Span * CoarsePanFraction)
                : window.Span / TimeDivisions;

            if (HotkeyPressed(ImGuiKey.A))
                Pan(step);
            else if (HotkeyPressed(ImGuiKey.D))
                Pan(-step);
        }

        void DisposeHistoryView()
        {
            pausedWaveformMin?.Dispose();
            pausedWaveformMax?.Dispose();
            pausedWaveformMin = null;
            pausedWaveformMax = null;
            pannedWaveformMinDecimator?.Dispose();
            pannedWaveformMaxDecimator?.Dispose();
            pannedWaveformMinDecimator = null;
            pannedWaveformMaxDecimator = null;
            pauseSample = -1;
            anchorPending = false;
        }
    }
}
