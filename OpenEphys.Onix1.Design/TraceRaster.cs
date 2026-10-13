using System;
using System.IO;
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
    /// The envelope is uploaded to the GPU where it changed, which for a sweep is the columns it crossed since the
    /// last frame, and the GPU draws the texture from it: a quad for each channel's pixel column for the traces, or a
    /// pass over every pixel for the heatmap. Which columns each pixel shows is worked out here, by the plot's pixel
    /// axis, so the drawing agrees with everything placed over it.
    /// </remarks>
    internal sealed class TraceRaster
    {
        /// <summary>
        /// Everything a drawn texture depends on besides the samples, colors and hidden channels. A change in any
        /// of it uploads the envelope whole.
        /// </summary>
        readonly record struct Key(
            Envelope Envelope, DisplayWindow Window, double Range, double Center, int Expanded, bool Heatmap, double ColorThreshold,
            int Width, int Height,
            int FirstVisible, int LastVisible, float RowHeight, float Origin);

        readonly GpuPainter gpu = new();
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
        /// <param name="wholeSweeps">
        /// Whether the envelope holds the last whole sweep, as at fast sweeps, rather than the one being written.
        /// </param>
        public unsafe void Draw(in PlotFrame frame, uint[] colors, int sweepCursor, long samples, bool wholeSweeps)
        {
            var w = (int)frame.Width;
            var h = (int)(frame.Bottom - frame.Top);
            var layout = frame.Layout;
            var hidden = frame.Hidden;
            if (w <= 0 || h <= 0)
                return;

            var corner = new Vector2(MathF.Floor(frame.Left), MathF.Floor(frame.Top));
            if (!gpu.Ready())
            {
                ImGui.GetWindowDrawList().AddText(corner + new Vector2(8, 8), ImGuiPalette.VibrantCoral,
                    $"The traces cannot be drawn: {gpu.Error}");
                return;
            }

            if (texture == 0 || w != width || h != height)
                Create(w, h);

            var current = new Key(
                frame.Envelope, frame.Window, frame.Range, frame.Center, frame.Expanded, frame.Heatmap, frame.ColorThreshold,
                w, h,
                layout.FirstVisible, layout.LastVisible, layout.RowHeight, layout.Origin - frame.Top);

            // NB: the columns changed since the last frame: all of them, or from the column the sweep was filling at
            // the last frame to the one it is filling now, both included, since each was part filled when last drawn.
            // A sweep that went all the way round since then changed every column, which is told from the samples
            // that arrived, since the cursor repeats every sweep. Not so the last whole sweep, which is replaced every
            // few frames anyway.
            var columns = frame.Envelope.Cols;
            var all = current != key ||
                !hidden.AsSpan().SequenceEqual(drawnHidden) ||
                !colors.AsSpan().SequenceEqual(drawnColors);
            (int First, int Last) run = (0, -1);
            (int First, int Last) wrapped = (0, -1);
            if (!all && sweepCursor >= 0)
            {
                var wentRound = samples - drawnSamples >= frame.Window.Span;
                if (drawnCursor < 0 || wentRound && !wholeSweeps)
                    all = true;
                else if (sweepCursor >= drawnCursor)
                    run = (drawnCursor, sweepCursor);
                else
                {
                    run = (drawnCursor, columns - 1);
                    wrapped = (0, sweepCursor);
                }
            }

            if (all)
                run = (0, columns - 1);

            if (run.Last >= run.First)
            {
                gpu.Upload(frame.Envelope, run.First, run.Last, reallocate: all);
                gpu.Upload(frame.Envelope, wrapped.First, wrapped.Last, reallocate: false);
                gpu.Draw(frame, colors, texture, w, h);
            }

            if (all)
            {
                key = current;
                drawnHidden = (bool[])hidden.Clone();
                drawnColors = (uint[])colors.Clone();
            }

            drawnCursor = sweepCursor;
            drawnSamples = samples;
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

            width = w;
            height = h;
            key = default;
        }

        /// <summary>
        /// The min and max drawn at pixel column <paramref name="p"/> of a channel.
        /// </summary>
        internal static unsafe (float Low, float High) Bin(float* minLine, float* maxLine, in PixelAxis axis, int p)
        {
            var (first, last, fraction) = Cell(axis, p);
            if (axis.Combines)
            {
                if (last <= first)
                    return (float.NaN, float.NaN);

                float low = minLine[first], high = maxLine[first];
                for (int c = first + 1; c < last; c++)
                {
                    low = Math.Min(low, minLine[c]);
                    high = Math.Max(high, maxLine[c]);
                }
                return (low, high);
            }

            return (minLine[first] + fraction * (minLine[last] - minLine[first]),
                maxLine[first] + fraction * (maxLine[last] - maxLine[first]));
        }

        /// <summary>
        /// The columns pixel column <paramref name="p"/> takes its min and max from: those it combines, from
        /// <c>First</c> up to <c>Last</c>, or where there are fewer columns than pixels, the two it interpolates
        /// between, <c>Fraction</c> of the way from <c>First</c> to <c>Last</c>.
        /// </summary>
        static (int First, int Last, float Fraction) Cell(in PixelAxis axis, int p)
        {
            if (axis.Combines)
            {
                var (start, end) = axis.ColumnsAt(p);
                return (start, end, 0);
            }

            // NB: a sample at its column's left edge, where the plot's column axis puts the outlines drawn over it.
            var columns = axis.Window.Columns;
            var x = Math.Max(0, Math.Min(columns - 1, (p + 0.5f) * columns / axis.Width));
            var c0 = (int)x;
            return (c0, Math.Min(columns - 1, c0 + 1), x - c0);
        }

        /// <summary>
        /// The GL objects the GPU draws the texture with, made the first time they are needed in the plot's context.
        /// </summary>
        sealed class GpuPainter
        {
            int framebuffer;
            int vertexArray;
            int traces;
            int heatmap;
            int minima;
            int maxima;
            int cellTexture;
            int colorTexture;
            int colormap;
            int envelopeRows;
            int envelopeColumns;
            bool tried;
            float[] cellData = Array.Empty<float>();
            uint[] colorData = Array.Empty<uint>();

            /// <summary>
            /// Why the traces cannot be drawn, or null.
            /// </summary>
            public string Error { get; private set; }

            public bool Ready()
            {
                if (!tried)
                {
                    tried = true;
                    try { Build(); }
                    catch (Exception ex) { Error = ex.Message; }
                }

                return Error is null;
            }

            void Build()
            {
                var common = Source("TraceRaster.Common.glsl");
                traces = Link(Source("TraceRaster.Traces.vert"), Source("TraceRaster.Traces.frag"), common);
                heatmap = Link(Source("TraceRaster.Heatmap.vert"), Source("TraceRaster.Heatmap.frag"), common);
                vertexArray = GL.GenVertexArray();
                framebuffer = GL.GenFramebuffer();
                minima = DataTexture();
                maxima = DataTexture();
                cellTexture = DataTexture();
                colorTexture = DataTexture();
                colormap = DataTexture();

                var map = Icefire.Map;
                GL.BindTexture(TextureTarget.Texture2D, colormap);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, map.Length, 1, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, map);
            }

            static string Source(string name)
            {
                using var stream = typeof(TraceRaster).Assembly.GetManifestResourceStream($"OpenEphys.Onix1.Design.Shaders.{name}")
                    ?? throw new InvalidOperationException($"The shader {name} is missing.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            static int Link(string vertex, string fragment, string common)
            {
                const string Version = "#version 330 core\n";
                var vertexShader = Compile(ShaderType.VertexShader, Version + common + vertex);
                var fragmentShader = Compile(ShaderType.FragmentShader, Version + common + fragment);
                var program = GL.CreateProgram();
                GL.AttachShader(program, vertexShader);
                GL.AttachShader(program, fragmentShader);
                GL.LinkProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linked);
                if (linked == 0)
                    throw new InvalidOperationException(GL.GetProgramInfoLog(program));
                return program;
            }

            static int Compile(ShaderType type, string source)
            {
                var shader = GL.CreateShader(type);
                GL.ShaderSource(shader, source);
                GL.CompileShader(shader);
                GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);
                if (compiled == 0)
                    throw new InvalidOperationException(GL.GetShaderInfoLog(shader));
                return shader;
            }

            // NB: read with texelFetch, which ignores filtering, but a texture whose filter asks for mipmaps it does
            // not have is incomplete and reads as zero.
            static int DataTexture()
            {
                var texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                return texture;
            }

            /// <summary>
            /// Uploads columns <paramref name="first"/> to <paramref name="last"/> of the envelope, both included, first
            /// making room for all of it where <paramref name="reallocate"/> asks or its size changed.
            /// </summary>
            public void Upload(Envelope envelope, int first, int last, bool reallocate)
            {
                var rows = envelope.Rows;
                var columns = envelope.Cols;
                if (reallocate || rows != envelopeRows || columns != envelopeColumns)
                {
                    foreach (var texture in new[] { minima, maxima })
                    {
                        GL.BindTexture(TextureTarget.Texture2D, texture);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, columns, rows, 0,
                            PixelFormat.Red, PixelType.Float, IntPtr.Zero);
                    }

                    envelopeRows = rows;
                    envelopeColumns = columns;
                    (first, last) = (0, columns - 1);
                }

                if (last < first)
                    return;

                UploadColumns(minima, envelope.Min, first, last);
                UploadColumns(maxima, envelope.Max, first, last);
            }

            static void UploadColumns(int texture, Mat data, int first, int last)
            {
                data.GetRawData(out IntPtr pointer, out int step, out Size size);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.PixelStore(PixelStoreParameter.UnpackRowLength, step / sizeof(float));
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, first, 0, last - first + 1, size.Height,
                    PixelFormat.Red, PixelType.Float, pointer + first * sizeof(float));
                GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            }

            /// <summary>
            /// Draws the traces or the heatmap into <paramref name="target"/> from the envelope as uploaded.
            /// </summary>
            public void Draw(in PlotFrame frame, uint[] colors, int target, int width, int height)
            {
                var layout = frame.Layout;
                var heatmapView = frame.Heatmap && frame.Expanded < 0;

                // NB: which columns each pixel takes its min and max from is the same for every channel, so it is
                // worked out once a frame, here, rather than by every channel's pass in the shaders.
                if (cellData.Length != width * 3)
                    cellData = new float[width * 3];
                for (int p = 0; p < width; p++)
                {
                    var (first, last, fraction) = Cell(frame.Axis, p);
                    cellData[3 * p] = first;
                    cellData[3 * p + 1] = last;
                    cellData[3 * p + 2] = fraction;
                }
                GL.BindTexture(TextureTarget.Texture2D, cellTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, width, 1, 0,
                    PixelFormat.Rgb, PixelType.Float, cellData);

                // NB: alpha 0 for a channel not drawn: a hidden one, unless it is the expanded channel.
                if (colorData.Length != colors.Length)
                    colorData = new uint[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    var drawn = !frame.Hidden[i] || i == frame.Expanded && !heatmapView;
                    colorData[i] = drawn ? colors[i] | 0xFF000000u : colors[i] & 0x00FFFFFFu;
                }
                GL.BindTexture(TextureTarget.Texture2D, colorTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, colorData.Length, 1, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, colorData);

                var viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);

                // NB: attached every time, since the texture is made again when the plot is resized, and GL can give
                // the new one the old one's name, which an attachment made once would take for the same texture.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D, target, 0);
                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    Error = $"framebuffer {status}";
                    return;
                }

                GL.Viewport(0, 0, width, height);
                GL.Disable(EnableCap.ScissorTest);
                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                var program = heatmapView ? heatmap : traces;
                GL.UseProgram(program);
                GL.BindVertexArray(vertexArray);
                Bind(program, "minimaTexture", 0, minima);
                Bind(program, "maximaTexture", 1, maxima);
                Bind(program, "cellTexture", 2, cellTexture);
                Bind(program, "colorTexture", 3, colorTexture);
                GL.Uniform1(GL.GetUniformLocation(program, "combines"), frame.Axis.Combines ? 1 : 0);
                GL.Uniform1(GL.GetUniformLocation(program, "firstRow"), layout.FirstRow);
                GL.Uniform1(GL.GetUniformLocation(program, "firstVisible"), layout.FirstVisible);
                GL.Uniform1(GL.GetUniformLocation(program, "origin"), layout.Origin - frame.Top);
                GL.Uniform1(GL.GetUniformLocation(program, "rowHeight"), layout.RowHeight);

                var visible = layout.LastVisible - layout.FirstVisible;
                if (heatmapView)
                {
                    Bind(program, "colormapTexture", 4, colormap);
                    GL.Uniform1(GL.GetUniformLocation(program, "colormapLength"), Icefire.Map.Length);
                    GL.Uniform1(GL.GetUniformLocation(program, "lastVisible"), layout.LastVisible);
                    GL.Uniform1(GL.GetUniformLocation(program, "range"), (float)frame.Range);
                    GL.Uniform1(GL.GetUniformLocation(program, "threshold"), (float)frame.ColorThreshold);
                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                }
                else if (visible > 0)
                {
                    var (_, scale) = frame.ValueMapping(layout.FirstVisible);
                    GL.Uniform1(GL.GetUniformLocation(program, "width"), width);
                    GL.Uniform1(GL.GetUniformLocation(program, "height"), height);
                    GL.Uniform1(GL.GetUniformLocation(program, "center"), (float)frame.Center);
                    GL.Uniform1(GL.GetUniformLocation(program, "scale"), scale);

                    // NB: an expanded channel is drawn alone and translucent, under the outlines that show its shape.
                    GL.Uniform1(GL.GetUniformLocation(program, "alpha"), frame.Expanded >= 0 ? 0x40 / 255f : 1f);
                    GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, visible * width);
                }

                GL.BindVertexArray(0);
                GL.UseProgram(0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
            }

            static void Bind(int program, string name, int unit, int texture)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + unit);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.Uniform1(GL.GetUniformLocation(program, name), unit);
            }
        }
    }
}
