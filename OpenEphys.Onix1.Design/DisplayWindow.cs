using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The stretch of a display axis a plot is showing: where it starts, how far apart its columns are,
    /// and how many there are. Nothing about the signal, and nothing about pixels beyond the fraction of
    /// the width a position falls at.
    /// </summary>
    /// <remarks>
    /// Panning and zooming are the whole of what this type does, and they are the whole of what they are
    /// allowed to do: what a column then draws, and what a division is labeled, are read back out of the
    /// same window, so the two cannot disagree.
    /// </remarks>
    readonly struct DisplayWindow
    {
        /// <param name="start">Axis position of the left edge.</param>
        /// <param name="step">Axis distance between columns.</param>
        /// <param name="columns">Number of columns across the plot.</param>
        public DisplayWindow(long start, int step, int columns)
        {
            if (step < 1)
                throw new ArgumentOutOfRangeException(nameof(step));

            if (columns < 1)
                throw new ArgumentOutOfRangeException(nameof(columns));

            Start = start;
            Step = step;
            Columns = columns;
        }

        /// <summary>
        /// Axis position of the left edge.
        /// </summary>
        public long Start { get; }

        /// <summary>
        /// Axis distance between one column and the next.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Number of columns across the plot.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Axis distance the whole plot covers.
        /// </summary>
        public long Span => (long)Columns * Step;

        /// <summary>
        /// Axis position one past the right edge.
        /// </summary>
        public long End => Start + Span;

        /// <summary>
        /// Axis position column <paramref name="column"/> begins at.
        /// </summary>
        public long PositionOf(int column) => Start + (long)column * Step;

        /// <summary>
        /// Column holding <paramref name="position"/>, outside <see cref="Columns"/> if it is off the plot.
        /// </summary>
        public int ColumnOf(long position) => (int)Math.Floor((position - Start) / (double)Step);

        /// <summary>
        /// How far across the plot <paramref name="position"/> falls, zero at the left edge and one at the
        /// right.
        /// </summary>
        public double FractionOf(double position) => (position - Start) / Span;

        /// <summary>
        /// The same window moved <paramref name="distance"/> further along the axis.
        /// </summary>
        public DisplayWindow Shift(long distance) => new(Start + distance, Step, Columns);

        /// <summary>
        /// The same stretch of axis at a different column spacing, keeping <paramref name="anchor"/> at
        /// <paramref name="fraction"/> of the way across the plot.
        /// </summary>
        public DisplayWindow Rescale(int step, int columns, long anchor, double fraction)
        {
            var span = (long)columns * step;
            return new DisplayWindow(anchor - (long)(fraction * span), step, columns);
        }

        /// <summary>
        /// The same window moved as little as possible to lie between <paramref name="first"/> and
        /// <paramref name="last"/>.
        /// </summary>
        /// <remarks>
        /// A window wider than that stretch is put at <paramref name="first"/> and left hanging over the
        /// end, since there is nowhere it would fit.
        /// </remarks>
        public DisplayWindow Clamp(long first, long last) =>
            new(Math.Max(first, Math.Min(Start, last - Span)), Step, Columns);

        /// <summary>
        /// Whether the two windows show the same stretch of axis at the same spacing.
        /// </summary>
        public bool Matches(DisplayWindow other) =>
            Start == other.Start && Step == other.Step && Columns == other.Columns;
    }
}
