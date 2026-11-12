using Hexa.NET.ImGui;
using System;
using System.Numerics;
using System.Windows.Forms;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Non-Form content base for ImGui-rendered probe-configuration panels: the three-panel layout
    /// (minimap | zoomed view | properties). Subclasses supply the properties panel content via
    /// <see cref="DrawPropsPanel"/> and append log lines via <see cref="Log"/>, writing into the
    /// <see cref="ImGuiLogConsole"/> given at construction. Hosted by an <see cref="ImGuiShellDialog"/>,
    /// which owns the actual Form/GL context/render timer as well as that log console.
    /// </summary>
    internal abstract class ImGuiProbePanel : IImGuiTabPanel
    {
        readonly ImGuiLogConsole log;

        protected ImGuiProbePanel(ImGuiProbeSelector selector, ImGuiLogConsole log)
        {
            this.selector = selector;
            this.log = log;
        }

        protected static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        /// <summary>
        /// The probe selector component shared by all subclasses.
        /// </summary>
        protected readonly ImGuiProbeSelector selector;

        /// <summary>
        /// Pixel width of the props panel on the right side of the layout.
        /// </summary>
        protected virtual float PropsWidth => 500f;

        /// <summary>
        /// The narrowest the props panel gets before it scrolls sideways instead.
        /// </summary>
        const float MinimumPropsWidth = 320f;

        /// <summary>
        /// The width of the whole ImGui display this panel needs at the least, as measured on its last frame, or 0
        /// before its first. The hosting shell keeps its window at least this wide.
        /// </summary>
        internal float MinimumDisplayWidth { get; private set; }

        /// <summary>
        /// When false, the selector suppresses drag-select interaction.
        /// </summary>
        protected virtual bool SelectionEnabled => true;

        /// <summary>
        /// A message shown in place of the minimap and zoomed view, or <c>null</c> to show the probe.
        /// </summary>
        protected virtual string ProbeViewMessage => null;

        /// <summary>
        /// Whether the props panel is drawn disabled.
        /// </summary>
        protected virtual bool PropsDisabled => false;

        /// <summary>
        /// Draw the contents of the right-hand props panel each frame.
        /// </summary>
        protected abstract void DrawPropsPanel();

        /// <summary>
        /// Appends a line to the hosting shell's console log.
        /// </summary>
        protected void Log(string message, bool isError = false) => log.Log(message, isError);

        /// <inheritdoc/>
        public virtual bool HasChanges { get; protected set; } = false;

        /// <inheritdoc/>
        public virtual bool CanClose(DialogResult pendingResult) => true;

        /// <inheritdoc/>
        public virtual void Closed(DialogResult result) { }

        /// <inheritdoc/>
        public void Draw()
        {
            float topH = ImGui.GetContentRegionAvail().Y;
            float topAvailW = ImGui.GetContentRegionAvail().X;
            var style = ImGui.GetStyle();
            float minimapColW = selector.MinimapWidth(topH, topAvailW);
            float sameLineGaps = 2f * style.ItemSpacing.X; // minimap<->zoomed and zoomed<->props

            // NB: the props panel gives up width before the probe view does, so that the probe, its depth axes
            // and the legend stay readable. Below its minimum, it scrolls sideways.
            float minimumZoomedW = 2f * ImGui.CalcTextSize("-10000um").X + 160f;
            float propsW = Math.Max(MinimumPropsWidth,
                Math.Min(PropsWidth, topAvailW - minimapColW - minimumZoomedW - sameLineGaps));
            float zoomedW = topAvailW - minimapColW - propsW - sameLineGaps;
            MinimumDisplayWidth = ImGui.GetIO().DisplaySize.X - topAvailW
                + minimapColW + minimumZoomedW + MinimumPropsWidth + sameLineGaps;

            var message = ProbeViewMessage;
            if (message != null)
            {
                DrawProbeViewMessage(message, new Vector2(minimapColW + ImGui.GetStyle().ItemSpacing.X + zoomedW, topH));
            }
            else
            {
                ImGui.BeginChild("##minimapFcol", new Vector2(minimapColW, topH), ImGuiChildFlags.None,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                selector.DrawMinimap();
                ImGui.EndChild();
                ImGui.SameLine();

                ImGui.BeginChild("##zoomed", new Vector2(zoomedW, topH), ImGuiChildFlags.None,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                selector.DrawZoomedView(SelectionEnabled);
                ImGui.EndChild();
            }
            ImGui.SameLine();

            var propsFlags = ImGuiWindowFlags.AlwaysVerticalScrollbar;
            if (propsW < PropsWidth)
            {
                // NB: the content keeps the width it has at full size, so the controls are laid out the same and
                // scroll into view instead of being squeezed.
                ImGui.SetNextWindowContentSize(new Vector2(PropsWidth - 2f * style.WindowPadding.X - style.ScrollbarSize, 0f));
                propsFlags |= ImGuiWindowFlags.HorizontalScrollbar;
            }
            ImGui.BeginChild("##props", new Vector2(propsW, topH), ImGuiChildFlags.Borders, propsFlags);
            ImGui.BeginDisabled(PropsDisabled);
            DrawPropsPanel();
            ImGui.EndDisabled();
            ImGui.EndChild();
        }

        static void DrawProbeViewMessage(string message, Vector2 size)
        {
            ImGui.BeginChild("##probeViewMessage", size, ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            float wrap = Math.Min(size.X * 0.6f, 480f);
            var textSize = ImGui.CalcTextSize(message, false, wrap);
            ImGui.SetCursorPos(new Vector2((size.X - textSize.X) * 0.5f, (size.Y - textSize.Y) * 0.4f));
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + wrap);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
            ImGui.TextUnformatted(message);
            ImGui.PopStyleColor();
            ImGui.PopTextWrapPos();

            ImGui.EndChild();
        }
    }
}
