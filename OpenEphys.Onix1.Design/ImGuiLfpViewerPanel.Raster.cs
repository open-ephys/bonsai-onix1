using System;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;

namespace OpenEphys.Onix1.Design
{
    /// <remarks>
    /// The traces are drawn into a texture a pixel column at a time and shown as one image. A sweep changes only
    /// the columns it crossed since the last frame, so only those are drawn and uploaded; anything else that
    /// changes what the plot shows redraws the whole texture once. The cost of a frame then follows what changed
    /// on screen rather than how many channels and columns are visible.
    /// </remarks>
    partial class ImGuiLfpViewerPanel
    {
        /// <summary>
        /// Everything a drawn texture depends on besides the samples. A change in any of it redraws it whole.
        /// </summary>
        readonly record struct RasterKey(
            Mat Envelope, long Start, int Step, int Columns, int Width, int Height,
            int FirstVisible, int LastVisible, float RowHeight, float Origin, double Range, int Expanded,
            ColorPalette Palette, Vector3 CustomColor, bool Grouping, int GroupSize);

        uint[] raster = Array.Empty<uint>();
        int rasterWidth;
        int rasterHeight;
        int rasterTexture;
        RasterKey rasterKey;
        bool[] rasterHidden = Array.Empty<bool>();
        int rasterCursor = -1;
        long rasterSamples;

        /// <summary>
        /// Brings the texture up to date with the envelope and draws it over the plot's visible area.
        /// </summary>
        /// <remarks>
        /// Must run with the plot's GL context current, as it is during a frame. The texture lives in that context
        /// and goes with it.
        /// </remarks>
        unsafe void DrawTraces(
            Mat waveformMin, Mat waveformMax, in RowLayout layout, float left, float width, float top, float bottom)
        {
            var w = (int)width;
            var h = (int)(bottom - top);
            if (w <= 0 || h <= 0)
                return;

            if (rasterTexture == 0 || w != rasterWidth || h != rasterHeight)
                CreateRaster(w, h);

            var key = new RasterKey(
                waveformMin, window.Start, window.Step, window.Columns, w, h,
                layout.FirstVisible, layout.LastVisible, layout.RowHeight, layout.Origin - top, rangeAmplitude,
                expandedChannel, Palette, CustomColor, ColorGroupingEnabled, ColorGrouping);

            var columns = waveformMin.Cols;
            if (key != rasterKey || !channelHidden.AsSpan().SequenceEqual(rasterHidden))
            {
                RepaintPixels(waveformMin, waveformMax, layout, top, 0, w);
                rasterKey = key;
                rasterHidden = (bool[])channelHidden.Clone();
            }
            else if (!Paused)
            {
                // NB: from the column the sweep was filling at the last frame to the one it is filling now, both
                // included, since each was part filled when last drawn. How far the sweep went is taken from the
                // samples that arrived, not from where the cursor stands, which repeats every sweep: at the shortest
                // timebases a sweep can go round between frames and end a few columns on. More than half a sweep
                // is cheaper drawn whole than worked out.
                var cursor = waveformMinDecimator.Cursor;
                if (rasterCursor < 0 || 2 * (history.Count - rasterSamples) > window.Span)
                {
                    RepaintPixels(waveformMin, waveformMax, layout, top, 0, w);
                }
                else if (cursor >= rasterCursor)
                {
                    RepaintColumns(waveformMin, waveformMax, layout, top, rasterCursor, cursor);
                }
                else
                {
                    RepaintColumns(waveformMin, waveformMax, layout, top, rasterCursor, columns - 1);
                    RepaintColumns(waveformMin, waveformMax, layout, top, 0, cursor);
                }
            }

            rasterCursor = Paused ? -1 : waveformMinDecimator.Cursor;
            rasterSamples = history?.Count ?? 0;

            var corner = new Vector2(MathF.Floor(left), MathF.Floor(top));
            ImGui.GetWindowDrawList().AddImage(
                new ImTextureRef(null, (ulong)rasterTexture), corner, corner + new Vector2(w, h));
        }

        void CreateRaster(int width, int height)
        {
            if (rasterTexture != 0)
                GL.DeleteTexture(rasterTexture);

            rasterTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, rasterTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            raster = new uint[width * height];
            rasterWidth = width;
            rasterHeight = height;
            rasterKey = default;
        }

        void RepaintColumns(Mat waveformMin, Mat waveformMax, in RowLayout layout, float top, int first, int last)
        {
            // NB: a pixel past each end as well, since a pixel's span reaches to its neighbor's.
            var columns = waveformMin.Cols;
            var from = (int)((long)first * rasterWidth / columns) - 1;
            var to = (int)((long)(last + 1) * rasterWidth / columns) + 2;
            RepaintPixels(waveformMin, waveformMax, layout, top, Math.Max(0, from), Math.Min(rasterWidth, to));
        }

        /// <summary>
        /// Clears and redraws pixel columns <paramref name="from"/> up to <paramref name="to"/>, and uploads them.
        /// </summary>
        unsafe void RepaintPixels(Mat waveformMin, Mat waveformMax, in RowLayout layout, float top, int from, int to)
        {
            if (to <= from)
                return;

            var w = rasterWidth;
            var h = rasterHeight;
            for (int y = 0; y < h; y++)
                Array.Clear(raster, y * w + from, to - from);

            PaintTraces(waveformMin, waveformMax, layout, top, from, to);

            GL.BindTexture(TextureTarget.Texture2D, rasterTexture);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, w);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            fixed (uint* data = &raster[from])
            {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, from, 0, to - from, h,
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
        /// the raster: another, such as a color-mapped view of every channel, fills the same columns its own way.
        /// </remarks>
        unsafe void PaintTraces(Mat waveformMin, Mat waveformMax, in RowLayout layout, float top, int from, int to)
        {
            var w = rasterWidth;
            var h = rasterHeight;
            waveformMin.GetRawData(out IntPtr minPtr, out int minStep, out Size shape);
            waveformMax.GetRawData(out IntPtr maxPtr, out int maxStep, out Size _);
            var columns = shape.Width;
            var scale = layout.RowHeight / (float)rangeAmplitude;

            // NB: an expanded channel is drawn alone and translucent, under the outlines that show its shape.
            var alpha = expandedChannel >= 0 ? 0x40u : 0xFFu;

            for (int i = layout.FirstVisible; i < layout.LastVisible; i++)
            {
                if (channelHidden[i] && i != expandedChannel)
                    continue;

                var minLine = (float*)((byte*)minPtr + i * minStep);
                var maxLine = (float*)((byte*)maxPtr + i * maxStep);
                var center = layout.RowTop(i) + layout.RowHeight / 2 - top;
                var c = ChannelColor(i);
                var color = (uint)(c.X * 255) | (uint)(c.Y * 255) << 8 | (uint)(c.Z * 255) << 16 | alpha << 24;

                var (prevLow, prevHigh) = from > 0 ? Bin(minLine, maxLine, columns, from - 1) : (float.NaN, float.NaN);
                for (int p = from; p < to; p++)
                {
                    var (low, high) = Bin(minLine, maxLine, columns, p);
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

                    var y0 = (int)MathF.Floor(center - spanHigh * scale);
                    var y1 = (int)MathF.Floor(center - spanLow * scale);
                    if (y1 < 0 || y0 >= h)
                        continue;

                    for (int y = Math.Max(0, y0); y <= Math.Min(h - 1, y1); y++)
                        raster[y * w + p] = color;
                }
            }
        }

        /// <summary>
        /// The min and max drawn at pixel column <paramref name="p"/> of a channel.
        /// </summary>
        unsafe (float Low, float High) Bin(float* minLine, float* maxLine, int columns, int p)
        {
            var w = rasterWidth;
            if (columns >= w)
            {
                var start = (int)((long)p * columns / w);
                var end = Math.Max(start + 1, (int)((long)(p + 1) * columns / w));
                float low = minLine[start], high = maxLine[start];
                for (int c = start + 1; c < end; c++)
                {
                    low = Math.Min(low, minLine[c]);
                    high = Math.Max(high, maxLine[c]);
                }
                return (low, high);
            }

            // NB: a sample at its column's left edge, where the plot's column axis puts the outlines drawn over it.
            var x = Math.Max(0, Math.Min(columns - 1, (p + 0.5f) * columns / w));
            var c0 = (int)x;
            var c1 = Math.Min(columns - 1, c0 + 1);
            var f = x - c0;
            return (minLine[c0] + f * (minLine[c1] - minLine[c0]), maxLine[c0] + f * (maxLine[c1] - maxLine[c0]));
        }
    }
}
