using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.Win32;
using HexaGen.Runtime;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    internal class ImGuiGLControl : GLControl, IGLContext
    {
        static readonly object RenderEvent = new();
#if DEBUG
        bool showMetrics = false;
#endif
        ImGuiContextPtr imGuiCtx;
        bool disposed;
        bool resizing;
        bool initialized;
        int renderDepth;

        public ImGuiGLControl()
        {
            GraphicsContext.ShareContexts = false;
            Size = new Size(640, 480);
        }

        public event EventHandler Render
        {
            add    { Events.AddHandler(RenderEvent, value); }
            remove { Events.RemoveHandler(RenderEvent, value); }
        }

        protected virtual void OnRender(EventArgs e)
        {
            if (Events[RenderEvent] is EventHandler h) h(this, e);
        }

        protected unsafe override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!HasValidContext) return;

            var form = FindForm();
            form.ResizeBegin += (_, _) => resizing = true;
            form.ResizeEnd += (_, _) => resizing = false;
            form.FormClosing += (_, _) => MakeCurrent();

            imGuiCtx = ImGui.CreateContext(null);
            ImGui.SetCurrentContext(imGuiCtx);
            ImGuiImplOpenGL3.SetCurrentContext(imGuiCtx);
            ImGuiImplWin32.SetCurrentContext(imGuiCtx);

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.IniFilename  = null;

            ImGui.StyleColorsDark();
            var style = ImGui.GetStyle();
            style.WindowRounding = 4;
            style.FrameRounding = 3;
            style.GrabRounding = 3;

            ImGuiImplWin32.InitForOpenGL(Handle.ToPointer());
            ImGuiImplOpenGL3.Init((string)null);
            initialized = true;
        }

        public new virtual void MakeCurrent()
        {
            base.MakeCurrent();
            ImGui.SetCurrentContext(imGuiCtx);
            ImGuiImplWin32.SetCurrentContext(imGuiCtx);
            ImGuiImplOpenGL3.SetCurrentContext(imGuiCtx);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (initialized && renderDepth == 0 && !DesignMode && HasValidContext && !resizing)
            {
                renderDepth++;
                try
                {
                    MakeCurrent();
                    ImGuiImplOpenGL3.NewFrame();
                    ImGuiImplWin32.NewFrame();
                    ImGui.NewFrame();

                    OnRender(EventArgs.Empty);

#if DEBUG
                    if (ImGui.IsKeyPressed(ImGuiKey.F9, false))
                        showMetrics = !showMetrics;

                    if (showMetrics)
                    {
                        ImGui.SetNextWindowFocus();
                        ImGui.ShowMetricsWindow(ref showMetrics);
                    }
#endif
                    ImGui.Render();
                    GL.Viewport(0, 0, Width, Height);
                    GL.ClearColor(Color.Black);
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

                    SwapBuffers();
                }
                finally
                {
                    renderDepth--;
                }
            }
            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (initialized && !disposed)
            {
                ImGuiImplWin32.SetCurrentContext(imGuiCtx);
                if (ImGuiImplWin32.WndProcHandler(Handle, (uint)m.Msg, (nuint)(ulong)m.WParam.ToInt64(), m.LParam) != 0)
                    return;
            }
            base.WndProc(ref m);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            initialized = false;
            if (HasValidContext && !disposed)
            {
                ImGuiImplOpenGL3.Shutdown();
                ImGuiImplWin32.Shutdown();
                ImGui.SetCurrentContext(null);
                ImGui.DestroyContext(imGuiCtx);
                disposed = true;
            }
            base.OnHandleDestroyed(e);
        }

        bool IGLContext.IsCurrent => Context.IsCurrent;

        nint INativeContext.GetProcAddress(string procName)
            => ((IGraphicsContextInternal)Context).GetAddress(procName);

        bool INativeContext.TryGetProcAddress(string procName, out nint procAddress)
        {
            procAddress = ((IGraphicsContextInternal)Context).GetAddress(procName);
            return procAddress != 0;
        }

        bool INativeContext.IsExtensionSupported(string extensionName) => true;

        void IGLContext.SwapInterval(int interval) => Context.SwapInterval = interval;
    }
}
