namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The plot as it is drawn in one frame: what is shown, where it is on screen, and how screen positions map to
    /// time and amplitude.
    /// </summary>
    /// <param name="Envelope">Per-column minima and maxima, one row per channel.</param>
    /// <param name="Layout">Where the channels' rows fall on screen.</param>
    /// <param name="Axis">
    /// Where the window's columns fall on the plot's pixels. Its window has as many columns as the envelope, which
    /// the decimators and the window are rebuilt together to keep, and which the painters read by raw pointer.
    /// </param>
    /// <param name="Top">Top of the plot's visible area on screen.</param>
    /// <param name="Bottom">Bottom of the plot's visible area on screen.</param>
    /// <param name="SampleRate">Samples per second on the axis.</param>
    /// <param name="Paused">Whether the plot is frozen.</param>
    /// <param name="PauseOrigin">Axis position of the sweep cursor while paused, which the time labels count from.</param>
    /// <param name="Timebase">Seconds the plot spans, as offered, which the time labels are given in.</param>
    /// <param name="PausedTimebase">The timebase when paused, how much older the frozen tail is.</param>
    /// <param name="Range">Amplitude spanned by one channel's row.</param>
    /// <param name="Unit">Unit of the amplitudes.</param>
    /// <param name="Hidden">Hidden channels, by index.</param>
    /// <param name="Expanded">The channel drawn alone, or -1.</param>
    /// <param name="Selected">The selected channel, or -1.</param>
    /// <param name="Heatmap">Whether channels are drawn as rows of color rather than as traces.</param>
    /// <param name="ColorThreshold">Magnitude at or below which a row of color is black.</param>
    internal readonly record struct PlotFrame(
        Envelope Envelope,
        ImGuiLfpViewerPanel.RowLayout Layout, PixelAxis Axis, float Top, float Bottom, int SampleRate,
        bool Paused, long PauseOrigin, double Timebase, double PausedTimebase,
        double Range, string Unit,
        bool[] Hidden, int Expanded, int Selected, bool Heatmap, double ColorThreshold)
    {
        /// <summary>
        /// The stretch of the axis the columns cover.
        /// </summary>
        public DisplayWindow Window => Axis.Window;

        /// <summary>
        /// Left edge of the plot on screen, on a whole pixel.
        /// </summary>
        public float Left => Axis.Left;

        /// <summary>
        /// Width of the plot in whole pixels.
        /// </summary>
        public float Width => Axis.Width;
    }
}
