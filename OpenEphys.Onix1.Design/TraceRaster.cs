using System;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Draws a stack of traces from per-column minima and maxima into a texture, and shows it as one image.
    /// </summary>
    /// <remarks>
    /// The texture is drawn a pixel column at a time. A sweep changes only the columns it crossed since the last
    /// frame, so only those are drawn and uploaded; anything else that changes what the plot shows redraws it whole
    /// once. The cost of a frame then follows what changed on screen rather than how many channels and columns are
    /// visible.
    /// </remarks>
    internal sealed class TraceRaster
    {
        /// <summary>
        /// Everything a drawn texture depends on besides the samples, colors and hidden channels. A change in any
        /// of it redraws it whole.
        /// </summary>
        readonly record struct Key(
            Envelope Envelope, DisplayWindow Window, double Range, double Center, int Expanded, bool Heatmap, double ColorThreshold,
            int Width, int Height,
            int FirstVisible, int LastVisible, float RowHeight, float Origin);

        uint[] pixels = Array.Empty<uint>();
        int width;
        int height;
        int texture;
        Key key;
        bool[] drawnHidden = Array.Empty<bool>();
        uint[] drawnColors = Array.Empty<uint>();
        int drawnCursor = -1;
        long drawnSamples;

        /// <summary>
        /// Brings the texture up to date with the envelope and draws it over the plot's visible area.
        /// </summary>
        /// <remarks>
        /// Must run with the plot's GL context current, as it is during a frame. The texture lives in that context
        /// and goes with it.
        /// </remarks>
        /// <param name="frame">The plot as it is drawn this frame.</param>
        /// <param name="colors">Each channel's color, packed as RGBA.</param>
        /// <param name="sweepCursor">The column a live sweep is filling, or -1 when not live.</param>
        /// <param name="samples">Samples that have arrived so far, which only grows while live.</param>
        public unsafe void Draw(in PlotFrame frame, uint[] colors, int sweepCursor, long samples)
        {
            var w = (int)frame.Width;
            var h = (int)(frame.Bottom - frame.Top);
            var layout = frame.Layout;
            var hidden = frame.Hidden;
            if (w <= 0 || h <= 0)
                return;

            if (texture == 0 || w != width || h != height)
                Create(w, h);

            var current = new Key(
                frame.Envelope, frame.Window, frame.Range, frame.Center, frame.Expanded, frame.Heatmap, frame.ColorThreshold,
                w, h,
                layout.FirstVisible, layout.LastVisible, layout.RowHeight, layout.Origin - frame.Top);

            var columns = frame.Envelope.Cols;
            if (current != key ||
                !hidden.AsSpan().SequenceEqual(drawnHidden) ||
                !colors.AsSpan().SequenceEqual(drawnColors))
            {
                RepaintPixels(frame, colors, 0, w);
                key = current;
                drawnHidden = (bool[])hidden.Clone();
                drawnColors = (uint[])colors.Clone();
            }
            else if (sweepCursor >= 0)
            {
                // NB: from the column the sweep was filling at the last frame to the one it is filling now, both
                // included, since each was part filled when last drawn. How far the sweep went is taken from the
                // samples that arrived, not from where the cursor stands, which repeats every sweep: at the shortest
                // timebases a sweep can go round between frames and end a few columns on. More than half a sweep
                // is cheaper drawn whole than worked out.
                if (drawnCursor < 0 || 2 * (samples - drawnSamples) > frame.Window.Span)
                {
                    RepaintPixels(frame, colors, 0, w);
                }
                else if (sweepCursor >= drawnCursor)
                {
                    RepaintColumns(frame, colors, drawnCursor, sweepCursor);
                }
                else
                {
                    RepaintColumns(frame, colors, drawnCursor, columns - 1);
                    RepaintColumns(frame, colors, 0, sweepCursor);
                }
            }

            drawnCursor = sweepCursor;
            drawnSamples = samples;

            var corner = new Vector2(MathF.Floor(frame.Left), MathF.Floor(frame.Top));
            ImGui.GetWindowDrawList().AddImage(new ImTextureRef(null, (ulong)texture), corner, corner + new Vector2(w, h));
        }

        void Create(int w, int h)
        {
            if (texture != 0)
                GL.DeleteTexture(texture);

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, w, h, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            pixels = new uint[w * h];
            width = w;
            height = h;
            key = default;
        }

        void RepaintColumns(in PlotFrame frame, uint[] colors, int first, int last)
        {
            // NB: a pixel past each end as well, since a pixel's span reaches to its neighbor's.
            var from = frame.Axis.PixelOf(first) - 1;
            var to = frame.Axis.PixelOf(last + 1) + 2;
            RepaintPixels(frame, colors, Math.Max(0, from), Math.Min(width, to));
        }

        /// <summary>
        /// Clears and redraws pixel columns <paramref name="from"/> up to <paramref name="to"/>, and uploads them.
        /// </summary>
        unsafe void RepaintPixels(in PlotFrame frame, uint[] colors, int from, int to)
        {
            if (to <= from)
                return;

            for (int y = 0; y < height; y++)
                Array.Clear(pixels, y * width + from, to - from);

            // NB: an expanded channel is drawn as a trace in either view, which is how a row of color is read.
            if (frame.Heatmap && frame.Expanded < 0)
                PaintHeatmap(frame, from, to);
            else
                PaintTraces(frame, colors, from, to);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, width);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            fixed (uint* data = &pixels[from])
            {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, from, 0, to - from, height,
                    PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)data);
            }
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        }

        /// <summary>
        /// Draws each visible channel's trace into pixel columns <paramref name="from"/> up to
        /// <paramref name="to"/>, which are clear.
        /// </summary>
        /// <remarks>
        /// Each pixel holds the span from the min to the max of the columns it covers, in the channel's color, drawn
        /// in channel order so that a trace running into its neighbors' rows lies over them. Where there are fewer
        /// columns than pixels, as at the shortest timebases, the samples are interpolated instead. One painter of
        /// the texture: another, such as a color-mapped view of every channel, fills the same columns its own way.
        /// </remarks>
        unsafe void PaintTraces(in PlotFrame frame, uint[] colors, int from, int to)
        {
            frame.Envelope.Min.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            frame.Envelope.Max.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            var layout = frame.Layout;

            // NB: an expanded channel is drawn alone and translucent, under the outlines that show its shape.
            var alpha = frame.Expanded >= 0 ? 0x40u << 24 : 0xFFu << 24;

            for (int i = layout.FirstVisible; i < layout.LastVisible; i++)
            {
                if (frame.Hidden[i] && i != frame.Expanded)
                    continue;

                var minLine = (float*)((byte*)minPtr + i * minStep);
                var maxLine = (float*)((byte*)maxPtr + i * maxStep);
                var color = colors[i] & 0x00FFFFFFu | alpha;

                var (prevLow, prevHigh) = from > 0 ? Bin(minLine, maxLine, frame.Axis, from - 1) : (float.NaN, float.NaN);
                for (int p = from; p < to; p++)
                {
                    var (low, high) = Bin(minLine, maxLine, frame.Axis, p);
                    if (float.IsNaN(low))
                    {
                        prevLow = prevHigh = float.NaN;
                        continue;
                    }

                    // NB: stretched to reach the pixel before, so the span covers every step between neighbors as
                    // a line would, rather than a band a pixel tall that a steep step thins to nothing.
                    var spanLow = float.IsNaN(prevLow) ? low : Math.Min(low, prevHigh);
                    var spanHigh = float.IsNaN(prevLow) ? high : Math.Max(high, prevLow);
                    prevLow = low;
                    prevHigh = high;

                    var y0 = (int)MathF.Floor(frame.ValueY(i, spanHigh) - frame.Top);
                    var y1 = (int)MathF.Floor(frame.ValueY(i, spanLow) - frame.Top);
                    if (y1 < 0 || y0 >= height)
                        continue;

                    for (int y = Math.Max(0, y0); y <= Math.Min(height - 1, y1); y++)
                        pixels[y * width + p] = color;
                }
            }
        }

        /// <summary>
        /// Fills each visible channel's row in pixel columns <paramref name="from"/> up to <paramref name="to"/>
        /// with the color of its signal there.
        /// </summary>
        /// <remarks>
        /// A pixel's value is whichever of the min and max of the columns it covers lies further from zero, with its
        /// sign, so a brief excursion shows rather than averaging away. The colormap is diverging and black at zero,
        /// the two signs brightening in two hues out to the range. Magnitudes up to the threshold are black, and those
        /// above it keep their color rather than the colormap being squeezed above the threshold, so a color means
        /// one amplitude whatever the threshold.
        /// </remarks>
        unsafe void PaintHeatmap(in PlotFrame frame, int from, int to)
        {
            frame.Envelope.Min.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            frame.Envelope.Max.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            var layout = frame.Layout;
            var map = Icefire.Map;
            var middle = map.Length / 2;

            for (int i = layout.FirstVisible; i < layout.LastVisible; i++)
            {
                if (frame.Hidden[i])
                    continue;

                var y0 = Math.Max(0, (int)MathF.Floor(layout.RowTop(i) - frame.Top));
                var y1 = Math.Min(height, (int)MathF.Floor(layout.RowTop(i) + layout.RowHeight - frame.Top));
                if (y1 <= y0)
                    continue;

                var minLine = (float*)((byte*)minPtr + i * minStep);
                var maxLine = (float*)((byte*)maxPtr + i * maxStep);
                for (int p = from; p < to; p++)
                {
                    var (low, high) = Bin(minLine, maxLine, frame.Axis, p);
                    if (float.IsNaN(low))
                        continue;

                    var value = Math.Abs(high) >= Math.Abs(low) ? high : low;
                    var magnitude = Math.Abs(value);
                    var step = (int)(Math.Min(1, magnitude / frame.Range) * (middle - 1) + 0.5);
                    var color = magnitude <= frame.ColorThreshold ? ImGuiPalette.Black
                        : map[value >= 0 ? middle + step : middle - 1 - step];
                    for (int y = y0; y < y1; y++)
                        pixels[y * width + p] = color;
                }
            }
        }

        /// <summary>
        /// The min and max drawn at pixel column <paramref name="p"/> of a channel.
        /// </summary>
        internal static unsafe (float Low, float High) Bin(float* minLine, float* maxLine, in PixelAxis axis, int p)
        {
            var columns = axis.Window.Columns;
            if (axis.Combines)
            {
                var (start, end) = axis.ColumnsAt(p);
                if (end <= start)
                    return (float.NaN, float.NaN);

                float low = minLine[start], high = maxLine[start];
                for (int c = start + 1; c < end; c++)
                {
                    low = Math.Min(low, minLine[c]);
                    high = Math.Max(high, maxLine[c]);
                }
                return (low, high);
            }

            // NB: a sample at its column's left edge, where the plot's column axis puts the outlines drawn over it.
            var x = Math.Max(0, Math.Min(columns - 1, (p + 0.5f) * columns / axis.Width));
            var c0 = (int)x;
            var c1 = Math.Min(columns - 1, c0 + 1);
            var f = x - c0;
            return (minLine[c0] + f * (minLine[c1] - minLine[c0]), maxLine[c0] + f * (maxLine[c1] - maxLine[c0]));
        }
    }
}
