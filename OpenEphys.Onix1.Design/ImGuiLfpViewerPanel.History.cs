using Hexa.NET.ImGui;
using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <remarks>
    /// <para>
    /// A paused display is laid out on one axis, measured in samples from the start of the sweep that was
    /// frozen. Position zero is that start, and the frozen frame reaches from there to the width the
    /// display had when it was paused. Panning back runs into negative positions, as far as the oldest
    /// sample the history still holds.
    /// </para>
    /// <para>
    /// Up to the sweep cursor, the sample at a position is that position. At and past it, the display was
    /// still showing the tail of the previous sweep, so the sample is one frozen width earlier. That is a
    /// fixed block of samples, settled at the moment of pause, so the signal drawn there cannot slide when
    /// the timebase changes. It is the one place where two positions name the same sample, and it is why
    /// everything here moves in positions and never in sample numbers: there is no way back.
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

        long anchorPosition;
        double anchorFraction;
        bool anchorPending;

        Decimator pannedWaveformMinDecimator;
        Decimator pannedWaveformMaxDecimator;

        /// <summary>
        /// Seconds of signal the history can hold, which is what the memory it was given buys at the current
        /// sample rate.
        /// </summary>
        public double HistorySeconds => history is null ? 0 : history.Capacity / (double)sampleRate;

        /// <summary>
        /// Seconds of signal the history currently holds. It stops advancing while paused, since what the
        /// user paused on must not expire while they are looking at it.
        /// </summary>
        public double HistoryHeldSeconds =>
            history is null ? 0 : (history.Count - history.Oldest) / (double)sampleRate;

        /// <summary>
        /// Whether the paused view has been moved off the frame that was frozen.
        /// </summary>
        bool Panned => Paused && window.Start != 0;

        /// <summary>
        /// Whether the matrices cloned at the pause instant still describe the display. They stop doing so
        /// when the timebase changes, which rebuilds the decimators at a different width.
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
        /// The cursor column holds the newest sample, so the sweep began <c>cursor</c> bins before it and
        /// the frame reaches one width past it, into the tail of the previous sweep.
        /// </remarks>
        void Pause()
        {
            if (waveformMinDecimator is null)
                return;

            pausedWaveformMin = waveformMinDecimator.Buffer.Clone();
            pausedWaveformMax = waveformMaxDecimator.Buffer.Clone();
            frozen = new DisplayWindow(
                0, waveformMinDecimator.DownsampleFactor, waveformMinDecimator.Buffer.Cols);
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
            value = value.Clamp(AxisFirst, AxisLast);
            if (value.Matches(window))
                return;

            window = value;
            windowDirty = true;
        }

        /// <summary>
        /// Takes the column spacing the decimators have just been rebuilt at, keeping whatever the last
        /// gesture asked to hold in place.
        /// </summary>
        /// <remarks>
        /// Called for the live display too, where the window is only there to give the divisions something
        /// to be placed against.
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

            SetWindow(window.Shift(-distance));
        }

        /// <summary>
        /// Reduces the samples the window covers straight out of the history.
        /// </summary>
        /// <remarks>
        /// At most two runs of positions, split where the frozen tail begins. The decimators carry a
        /// part-filled bin from one call to the next, so a column lying across that split is reduced from
        /// both sides without either side knowing, and <see cref="WaveformHistory.Decimate"/> splits again
        /// where a run straddles the end of the ring. The runs add up to the window's span, so every
        /// column is filled.
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
        /// The new column spacing is not known until <see cref="Update"/> rebuilds the decimators, so what
        /// to hold is recorded here and the window solved for there.
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
        /// The envelope to draw this frame: two matrices of one row per channel and one column per bin across
        /// the plot, holding the smallest and largest sample that fell in each decimation bin, which the plot
        /// fills between and outlines.
        /// </summary>
        (Mat WaveformMin, Mat WaveformMax) DisplayEnvelope()
        {
            if (!Paused)
                return (waveformMinDecimator.Buffer, waveformMaxDecimator.Buffer);

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
                    ? (waveformMinDecimator.Buffer, waveformMaxDecimator.Buffer)
                    : (pannedWaveformMinDecimator.Buffer, pannedWaveformMaxDecimator.Buffer);
            }

            windowDirty = false;
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
                Pan((long)(ImGui.GetIO().MouseDelta.X * (window.Span / width)));

            // NB: a division at a time, or most of a window with Shift, which is the same relationship
            // W and S have for channels.
            var step = Modifiers(shift: true)
                ? (long)(window.Span * CoarsePanFraction)
                : window.Span / TimeDivisions;

            if (ImGui.IsKeyPressed(ImGuiKey.A))
                Pan(step);
            else if (ImGui.IsKeyPressed(ImGuiKey.D))
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
