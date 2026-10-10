using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Represents where a display window's columns fall on the plot's pixels: the one mapping between the axis and
    /// the screen, which everything drawn on the plot or read from it goes through, so that all of it agrees with
    /// what is drawn.
    /// </summary>
    /// <remarks>
    /// The screen shows the axis in cells:
    /// <list type="bullet">
    /// <item><description>Where there are at least as many columns as pixels, a cell is the run of columns one
    /// pixel combines. The runs are counted from the sweep origin rather than from the window's edge, so a window
    /// that starts on a cell boundary shows the same columns together wherever it is panned to.</description></item>
    /// <item><description>Where there are fewer columns than pixels, a cell is one column, spread over the pixels
    /// it covers.</description></item>
    /// </list>
    /// </remarks>
    readonly struct PixelAxis
    {
        readonly long firstColumn;
        readonly long firstCell;

        /// <param name="window">The stretch of the axis the plot shows.</param>
        /// <param name="left">Left edge of the plot on screen.</param>
        /// <param name="width">Width of the plot in pixels.</param>
        public PixelAxis(DisplayWindow window, float left, float width)
        {
            Window = window;
            Left = MathF.Floor(left);
            Width = Math.Max(0, (int)width);
            firstColumn = window.Step > 0 ? FloorDiv(window.Start, window.Step) : 0;
            firstCell = 0;
            firstCell = CellOf(firstColumn);
        }

        /// <summary>
        /// The stretch of the axis the plot shows.
        /// </summary>
        public DisplayWindow Window { get; }

        /// <summary>
        /// Left edge of the plot on screen, on a whole pixel.
        /// </summary>
        public float Left { get; }

        /// <summary>
        /// Width of the plot in whole pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Whether a pixel combines a run of whole columns, as where there are at least as many columns as pixels.
        /// </summary>
        public bool Combines => Width > 0 && Window.Columns >= Width;

        /// <summary>
        /// Whether every cell is a single column, so that a cell holds one sample where a column does.
        /// </summary>
        public bool SingleColumnCells => Window.Columns <= Width;

        /// <summary>
        /// The axis distance a cell spans, on average.
        /// </summary>
        public long CellSpan => Combines ? Window.Span / Width : Window.Step;

        /// <summary>
        /// The axis distance the widest cell spans, a column more than the narrowest where the columns do not
        /// divide evenly among the pixels.
        /// </summary>
        public long WidestCell => (Combines ? (Window.Columns + Width - 1) / Width : 1) * Window.Step;

        /// <summary>
        /// The pixel column <paramref name="column"/> of the window is drawn in, or begins in where it covers
        /// several.
        /// </summary>
        public int PixelOf(int column) => Combines
            ? (int)(CellOf(firstColumn + column) - firstCell)
            : Width > 0 ? (int)((long)column * Width / Window.Columns) : 0;

        /// <summary>
        /// The screen x where column <paramref name="column"/> of the window is drawn: the left edge of the pixel
        /// it is combined into, or of the stretch of pixels it covers.
        /// </summary>
        public float ColumnX(int column) => Combines
            ? Left + PixelOf(column)
            : Width > 0 ? Left + (float)column * Width / Window.Columns : Left;

        /// <summary>
        /// The pixel <paramref name="position"/> is drawn in.
        /// </summary>
        public int PixelAt(double position) =>
            Width <= 0 || Window.Step <= 0 ? 0
            : Combines ? (int)(CellOf(ColumnAt(position)) - firstCell)
            : (int)Math.Floor(Window.FractionOf(position) * Width);

        /// <summary>
        /// The screen x of the left edge of the pixel <paramref name="position"/> is drawn in.
        /// </summary>
        public float X(double position) => Left + PixelAt(position);

        /// <summary>
        /// The columns of the window that pixel <paramref name="pixel"/> combines, where <see cref="Combines"/>.
        /// </summary>
        public (int Start, int End) ColumnsAt(int pixel) => Columns(firstCell + pixel);

        /// <summary>
        /// The columns of the window in the cell <paramref name="position"/> falls in.
        /// </summary>
        public (int Start, int End) CellColumns(long position) => Columns(CellOf(ColumnAt(position)));

        /// <summary>
        /// The position that stands for the cell drawn at screen x <paramref name="x"/>, clamped to the plot.
        /// </summary>
        public long PositionAt(float x)
        {
            if (Width <= 0 || Window.Step <= 0)
                return Window.Start + Window.Span / 2;

            var pixel = Math.Max(0, Math.Min(Width - 1, (int)Math.Floor(x - Left)));
            var cell = Combines
                ? firstCell + pixel
                : firstColumn + (long)((pixel + 0.5) * Window.Columns / Width);
            return PositionOf(cell);
        }

        /// <summary>
        /// The position that stands for the cell <paramref name="position"/> falls in, within the window.
        /// </summary>
        public long Snap(long position)
        {
            if (Window.Step <= 0)
                return position;

            position = Math.Max(Window.Start, Math.Min(Window.End - 1, position));
            return PositionOf(CellOf(ColumnAt(position)));
        }

        /// <summary>
        /// The cell boundary at or below <paramref name="position"/> for a negative <paramref name="round"/>, at or
        /// above it for a positive one, or the nearer of the two for zero.
        /// </summary>
        public long Boundary(long position, int round)
        {
            if (Window.Step <= 0)
                return position;

            var cell = CellOf(ColumnAt(position));
            var below = CellStart(cell) * Window.Step;
            var above = CellStart(cell + 1) * Window.Step;
            return round < 0 || position == below ? below
                : round > 0 ? above
                : position - below <= above - position ? below : above;
        }

        long ColumnAt(double position) => (long)Math.Floor(position / Window.Step);

        // NB: cell g starts at column floor(g * columns / width), so the cell holding a column is the last whose
        // start is not past it.
        long CellOf(long column) => Combines ? CeilDiv((column + 1) * Width, Window.Columns) - 1 : column;

        long CellStart(long cell) => Combines ? FloorDiv(cell * Window.Columns, Width) : cell;

        // NB: a pixel's run of columns is drawn as one, so its middle stands for it. A lone column is drawn at its
        // first sample, so that does, and a cursor's line and time then sit where the sample is drawn.
        long PositionOf(long cell)
        {
            var start = CellStart(cell);
            return Combines
                ? start * Window.Step + (CellStart(cell + 1) - start) * Window.Step / 2
                : start * Window.Step;
        }

        (int Start, int End) Columns(long cell) =>
            (ClampColumn(CellStart(cell) - firstColumn), ClampColumn(CellStart(cell + 1) - firstColumn));

        int ClampColumn(long column) => (int)Math.Max(0, Math.Min(Window.Columns, column));

        static long FloorDiv(long a, long b) => a / b - (a % b != 0 && (a < 0) != (b < 0) ? 1 : 0);

        static long CeilDiv(long a, long b) => -FloorDiv(-a, b);
    }
}
