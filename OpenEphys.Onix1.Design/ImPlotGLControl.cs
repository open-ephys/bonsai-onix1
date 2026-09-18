using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// An <see cref="ImGuiGLControl"/> that also owns an ImPlot context, for surfaces that plot.
    /// </summary>
    /// <remarks>
    /// The ImPlot context is bound to the control's ImGui context and follows its lifecycle: created once
    /// that context exists, made current alongside it, and destroyed before it.
    /// </remarks>
    internal class ImPlotGLControl : ImGuiGLControl
    {
        ImPlotContextPtr imPlotCtx;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!HasValidContext) return;

            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            imPlotCtx = ImPlot.CreateContext();
            ImPlot.SetCurrentContext(imPlotCtx);
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            ImPlot.SetCurrentContext(imPlotCtx);
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
    }
}
