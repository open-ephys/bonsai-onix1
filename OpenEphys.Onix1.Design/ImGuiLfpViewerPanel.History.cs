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
        Envelope pausedEnvelope;

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

        Decimator pannedDecimator;

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
            !Paused && decimator?.LastSweep is not null &&
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
            pausedEnvelope is not null &&
            frozen.Columns == window.Columns &&
            frozen.Step == window.Step;

        /// <summary>
        /// Axis position of the sweep cursor, which is where the newest sample meets the frozen tail.
        /// </summary>
        long CursorPosition => pauseSample - sweepOrigin;

        /// <summary>
        /// Freezes the display on what is drawn now.
        /// </summary>
        /// <remarks>
        /// The sweep began <c>Cursor</c> columns before the newest sample, and the frame runs one full width
        /// from there, into the tail of the previous sweep.
        /// </remarks>
        void Pause()
        {
            if (decimator is null)
                return;

            pausedEnvelope = decimator.Sweep.Clone();
            frozen = new DisplayWindow(0, decimator.DownsampleFactor, decimator.Sweep.Cols);
            pausedTimebase = timebase;

            pauseSample = history?.Count ?? 0;

            sweepOrigin = SweepOrigin(pauseSample, decimator);

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
            pausedEnvelope?.Dispose();
            pausedEnvelope = null;

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
            // NB: the start is kept on a boundary between the cells the screen shows, counted from the sweep
            // origin, so a column always covers the same samples, and a pixel the same columns, as live. A pan then
            // moves what is drawn without grouping it afresh, which would change the traces and what a cursor reads.
            value = value.Clamp(AxisFirst, AxisLast);
            var cells = new PixelAxis(value, plotLeft, plotSpan);
            var start = cells.Boundary(value.Start, 0);
            if (start < AxisFirst)
                start = cells.Boundary(AxisFirst, 1);
            else if (start + value.Span > AxisLast && cells.Boundary(AxisLast - value.Span, -1) >= AxisFirst)
                start = cells.Boundary(AxisLast - value.Span, -1);

            value = new DisplayWindow(start, value.Step, value.Columns);
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
            if (!CanPan)
                return;

            // NB: to the nearest cell boundary, with the rest carried to the next pan so that a slow drag still
            // moves. No more than the widest cell is carried, which is enough to reach the next boundary from any
            // start, and little enough that a drag back from the end of the history answers within a cell.
            var target = window.Start + panCarry - distance;
            SetWindow(window.Shift(target - window.Start));
            var reach = Axis.WidestCell;
            panCarry = Math.Max(-reach, Math.Min(reach, target - window.Start));
        }

        /// <summary>
        /// Moves the view to center <paramref name="position"/>, as near as the cell boundaries and the history
        /// allow.
        /// </summary>
        /// <remarks>
        /// Straight to the window rather than through <see cref="Pan"/>, whose carry would put a position
        /// halfway between two boundaries on alternate ones each time.
        /// </remarks>
        void CenterOn(long position)
        {
            if (!CanPan)
                return;

            panCarry = 0;
            SetWindow(window.Shift(position - window.Span / 2 - window.Start));
        }

        // NB: the frozen tail is read from one frozen width behind the pause instant, so a history that no longer
        // reaches back that far cannot serve any view that shows it.
        bool CanPan => Paused && history is not null && pauseSample - frozen.Span >= history.Oldest;

        bool TryReadWindow() =>
            pannedDecimator is not null &&
            ReadWindow(history, pannedDecimator, window, sweepOrigin, CursorPosition, frozen.Span);

        /// <summary>
        /// The sample at axis position zero: where the sweep on screen began, given the newest sample written
        /// and the live decimator that has reduced everything up to it.
        /// </summary>
        /// <remarks>
        /// The cursor column is part filled, and by <see cref="Decimator.Filled"/>, so it began that far before
        /// the newest sample rather than on it. Without the term the whole axis sits up to a column early, and a
        /// view read back out of the history groups its samples differently from the live one.
        /// </remarks>
        internal static long SweepOrigin(long newestSample, Decimator live) =>
            newestSample - live.Filled - (long)live.Cursor * live.DownsampleFactor;

        /// <summary>
        /// Reduces the samples <paramref name="window"/> covers straight out of <paramref name="history"/> into
        /// <paramref name="into"/>.
        /// </summary>
        /// <remarks>
        /// Left of <paramref name="cursorPosition"/> a position shows the sample at that position from
        /// <paramref name="sweepOrigin"/>, and from it on the sample one <paramref name="frozenSpan"/> earlier,
        /// the tail of the previous sweep. So the window is read as at most two runs, split at the cursor. The
        /// decimator carries a part-filled column from one run into the next, so a column across the split is
        /// reduced from both.
        /// </remarks>
        /// <returns>False if the history can no longer serve the window.</returns>
        internal static bool ReadWindow(
            WaveformHistory history, Decimator into, DisplayWindow window,
            long sweepOrigin, long cursorPosition, long frozenSpan)
        {
            var split = Math.Max(window.Start, Math.Min(window.End, cursorPosition));
            var fresh = (int)(split - window.Start);
            var tail = (int)(window.End - split);

            into.Reset();
            if (fresh > 0 && !history.Decimate(SampleAt(window.Start), fresh, into))
                return false;

            return tail <= 0 || history.Decimate(SampleAt(split), tail, into);

            long SampleAt(long position) =>
                sweepOrigin + position - (position >= cursorPosition ? frozenSpan : 0);
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
        /// The envelope to draw this frame: the live one, the copy taken at pause, or one read from the history
        /// for a view that has been moved.
        /// </summary>
        Envelope DisplayEnvelope()
        {
            if (FastSweep)
                return decimator.LastSweep;

            if (!Paused)
                return decimator.Sweep;

            if (!Panned && SnapshotCurrent)
                return pausedEnvelope;

            // NB: a backstop. Whatever the read reached before it gave up stands, so the plot draws a
            // gap or part of a frame, and the window goes back to the frozen frame to be tried again next
            // frame. Returning the live buffers here would show the trace running while the cursor and the
            // pause button both say it is frozen.
            if (windowDirty && !TryReadWindow())
            {
                SetWindow(new DisplayWindow(0, window.Step, window.Columns));
                return (pannedDecimator ?? decimator).Sweep;
            }

            windowDirty = false;
            return pannedDecimator.Sweep;
        }

        /// <summary>
        /// Moves the paused view by dragging the time labels, keeping the samples under the pointer.
        /// </summary>
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
        }

        void DisposeHistoryView()
        {
            pausedEnvelope?.Dispose();
            pausedEnvelope = null;
            pannedDecimator?.Dispose();
            pannedDecimator = null;
            pauseSample = -1;
            anchorPending = false;
        }
    }
}
