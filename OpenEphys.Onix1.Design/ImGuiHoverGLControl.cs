using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// An <see cref="ImGuiGLControl"/> that answers the pointer whichever window is active.
    /// </summary>
    /// <remarks>
    /// A plot of live data has to answer the user the moment they act on it, whichever window was
    /// active, so the plot under the pointer answers a click, a wheel, or one of its hover keys,
    /// taking focus as it does.
    /// </remarks>
    internal class ImGuiHoverGLControl : ImGuiGLControl
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_MOUSEWHEEL = 0x020A;

        static bool keyFilterInstalled;

        readonly HashSet<ImGuiKey> hoverKeys;

        /// <param name="hoverKeys">
        /// Keys that reach this control while the pointer is over it even when another window has the
        /// keyboard. Any other key goes wherever the keyboard already is.
        /// </param>
        public ImGuiHoverGLControl(IEnumerable<ImGuiKey> hoverKeys) => this.hoverKeys = new(hoverKeys);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!HasValidContext) return;

            if (!keyFilterInstalled)
            {
                Application.AddMessageFilter(new HoverKeyFilter());
                keyFilterInstalled = true;
            }
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

        // NB: a key message is addressed to the focused window alone, so the only place another
        // control can act on it is the application message filter, which sees every message before
        // the loop delivers it. The same filter stops a hover key from reaching a control the
        // pointer has left.
        sealed class HoverKeyFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_KEYDOWN) return false;

                var key = ToImGuiKey((Keys)(int)m.WParam);
                var handle = WindowFromPoint(Cursor.Position);
                var under = Control.FromHandle(handle) as ImGuiHoverGLControl;
                var focused = Control.FromHandle(m.HWnd) as ImGuiHoverGLControl;

                if (under is not null && under != focused && under.hoverKeys.Contains(key))
                {
                    under.TakeFocus();
                    SendMessage(handle, m.Msg, m.WParam, m.LParam);
                    return true;
                }

                return under is null && focused is not null && focused.hoverKeys.Contains(key);
            }

            static ImGuiKey ToImGuiKey(Keys key) => key switch
            {
                Keys.Space => ImGuiKey.Space,
                Keys.Escape => ImGuiKey.Escape,
                Keys.Delete => ImGuiKey.Delete,
                >= Keys.A and <= Keys.Z => ImGuiKey.A + (key - Keys.A),
                _ => ImGuiKey.None,
            };

            [DllImport("user32.dll")]
            static extern IntPtr WindowFromPoint(Point point);

            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
