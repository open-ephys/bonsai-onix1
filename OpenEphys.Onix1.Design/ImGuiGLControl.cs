using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.Win32;
using HexaGen.Runtime;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
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

                    // NB: the Win32 and OpenGL3 backends share one current-context global inside
                    // ImGuiImpl.dll, and WndProc on any ImGuiGLControl repoints it. A message for another
                    // control dispatched mid-frame leaves it pointing at that control's context, so it is
                    // re-asserted immediately before each backend call rather than once per frame.
                    ImGuiImplOpenGL3.SetCurrentContext(imGuiCtx);
                    ImGuiImplOpenGL3.NewFrame();
                    ImGuiImplWin32.SetCurrentContext(imGuiCtx);
                    ImGuiImplWin32.NewFrame();
                    ReleaseMissedButtons();
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
                    ImGuiImplOpenGL3.SetCurrentContext(imGuiCtx);
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

        // NB: ImGui learns of a release only from the window's message, which is lost if something takes the
        // mouse capture while a button is down. It then believes the button still held, and nothing takes a
        // click until focus leaves and clears it, while hovering goes on working. So the buttons are checked
        // against the device each frame, and a release that was missed is sent.
        static void ReleaseMissedButtons()
        {
            // NB: the device reports the physical buttons, which are the other way round when swapped.
            var swapped = GetSystemMetrics(SM_SWAPBUTTON) != 0;
            ReleaseIfUp(ImGuiMouseButton.Left, swapped ? VK_RBUTTON : VK_LBUTTON);
            ReleaseIfUp(ImGuiMouseButton.Right, swapped ? VK_LBUTTON : VK_RBUTTON);
            ReleaseIfUp(ImGuiMouseButton.Middle, VK_MBUTTON);
        }

        static void ReleaseIfUp(ImGuiMouseButton button, int virtualKey)
        {
            if (ImGui.IsMouseDown(button) && (GetAsyncKeyState(virtualKey) & 0x8000) == 0)
                ImGui.GetIO().AddMouseButtonEvent((int)button, false);
        }

        const int VK_LBUTTON = 0x01;
        const int VK_RBUTTON = 0x02;
        const int VK_MBUTTON = 0x04;
        const int SM_SWAPBUTTON = 23;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int virtualKey);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int index);

        // NB: WinForms takes the arrows for moving focus between controls, and drops the message, unless the
        // control claims them as input. ImGui needs them for the caret in text fields and for keyboard navigation.
        protected override bool IsInputKey(Keys keyData) =>
            (keyData & Keys.KeyCode) is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

        // NB: Bonsai's visualizer window closes on Escape, which ImGui uses to cancel an edit or a selection. A key
        // taken here is taken ahead of the window, but is then never dispatched, so it is handed to ImGui directly.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && initialized && !disposed)
            {
                ImGuiImplWin32.SetCurrentContext(imGuiCtx);
                ImGuiImplWin32.WndProcHandler(Handle, (uint)msg.Msg, (nuint)(ulong)msg.WParam.ToInt64(), msg.LParam);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (initialized && !disposed)
            {
                ImGuiImplWin32.SetCurrentContext(imGuiCtx);
                if (ImGuiImplWin32.WndProcHandler(Handle, (uint)m.Msg, (nuint)(ulong)m.WParam.ToInt64(), m.LParam) != 0)
                    return;

                // NB: Alt pressed and released with no key between puts the window into menu mode, which a wheel
                // turn or a drag between does not cancel, so the key after an Alt gesture would go to the menu.
                // ImGui has had the release; Windows does not get it.
                if (m.Msg == WM_SYSKEYUP && (int)m.WParam.ToInt64() == VK_MENU)
                    return;

                // NB: Alt with a letter looks for a menu item with that mnemonic and beeps when there is none. ImGui
                // has had the key; Windows does not get the character, except Alt+Space, the window's own menu.
                if (m.Msg == WM_SYSCHAR && (int)m.WParam.ToInt64() != ' ')
                    return;
            }
            base.WndProc(ref m);
        }

        const int WM_SYSKEYUP = 0x0105;
        const int WM_SYSCHAR = 0x0106;
        const int VK_MENU = 0x12;

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
