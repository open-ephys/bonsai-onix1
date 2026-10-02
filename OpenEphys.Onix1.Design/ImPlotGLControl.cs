using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// An <see cref="ImGuiGLControl"/> that also owns an ImPlot context, for surfaces that plot.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ImPlot context is bound to the control's ImGui context and follows its lifecycle: created once
    /// that context exists, made current alongside it, and destroyed before it.
    /// </para>
    /// <para>
    /// A plot of live data has to answer the user the moment they act on it, whichever window was
    /// active, so the plot under the pointer answers a click, a wheel, or a key in <see cref="HoverKeys"/>,
    /// taking focus as it does.
    /// </para>
    /// </remarks>
    internal class ImPlotGLControl : ImGuiGLControl
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_MOUSEWHEEL = 0x020A;

        static bool keyFilterInstalled;

        ImPlotContextPtr imPlotCtx;

        /// <summary>
        /// Keys that reach this control while the pointer is over it even when another window has the
        /// keyboard. Left empty, keys go wherever the keyboard already is.
        /// </summary>
        public HashSet<Keys> HoverKeys { get; } = new();

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!HasValidContext) return;

            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            imPlotCtx = ImPlot.CreateContext();
            ImPlot.SetCurrentContext(imPlotCtx);

            if (!keyFilterInstalled)
            {
                Application.AddMessageFilter(new HoverKeyFilter());
                keyFilterInstalled = true;
            }
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            ImPlot.SetCurrentContext(imPlotCtx);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                TakeFocus();

                // NB: Windows delivers the wheel to the window under the pointer whether or not it is focused, but
                // keys are only delivered to the focused window, and the backend polls the modifiers on mouse move
                // and focus but not on wheel. The first wheel at a control that has just taken focus would read stale
                // modifiers, so they are polled here the way the backend does for a move.
                var io = ImGui.GetIO();
                var keys = ModifierKeys;
                io.AddKeyEvent(ImGuiKey.ModCtrl, (keys & Keys.Control) != 0);
                io.AddKeyEvent(ImGuiKey.ModShift, (keys & Keys.Shift) != 0);
                io.AddKeyEvent(ImGuiKey.ModAlt, (keys & Keys.Alt) != 0);
            }
            base.WndProc(ref m);
        }

        void TakeFocus()
        {
            if (Focused) return;
            FindForm()?.Activate();
            Focus();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!imPlotCtx.IsNull)
            {
                ImPlot.SetCurrentContext(null);
                ImPlot.DestroyContext(imPlotCtx);
                imPlotCtx = default;
            }
            base.OnHandleDestroyed(e);
        }

        // NB: a key message is addressed to the focused window alone, so the only place another
        // control can act on it is the application message filter, which sees every message before
        // the loop delivers it. The same filter stops a hover key from reaching a control the
        // pointer has left.
        sealed class HoverKeyFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_KEYDOWN) return false;

                var key = (Keys)(int)m.WParam;
                var handle = WindowFromPoint(Cursor.Position);
                var under = Control.FromHandle(handle) as ImPlotGLControl;
                var focused = Control.FromHandle(m.HWnd) as ImPlotGLControl;

                if (under is not null && under != focused && under.HoverKeys.Contains(key))
                {
                    under.TakeFocus();
                    SendMessage(handle, m.Msg, m.WParam, m.LParam);
                    return true;
                }

                return under is null && focused is not null && focused.HoverKeys.Contains(key);
            }

            [DllImport("user32.dll")]
            static extern IntPtr WindowFromPoint(Point point);

            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
