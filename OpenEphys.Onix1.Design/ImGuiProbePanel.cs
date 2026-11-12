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
            float minimapColW = selector.MinimapWidth(topH, topAvailW);
            float sameLineGaps = 2f * ImGui.GetStyle().ItemSpacing.X; // minimap<->zoomed and zoomed<->props
            float zoomedW = topAvailW - minimapColW - PropsWidth - sameLineGaps;

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

            ImGui.BeginChild("##props", new Vector2(PropsWidth, topH), ImGuiChildFlags.Borders,
                ImGuiWindowFlags.AlwaysVerticalScrollbar);
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
