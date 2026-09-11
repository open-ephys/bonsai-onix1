using Hexa.NET.ImGui;
using System.Numerics;
using System.Windows.Forms;

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

        protected ImGuiProbePanel(ImGuiLogConsole log)
        {
            this.log = log;
        }

        protected static readonly Vector4 ColorTextError = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.VibrantCoral);

        /// <summary>
        /// The probe selector component shared by all subclasses.
        /// </summary>
        protected readonly ImGuiProbeSelector selector = new();

        /// <summary>
        /// Pixel width of the props panel on the right side of the layout.
        /// </summary>
        protected virtual float PropsWidth => 500f;

        /// <summary>
        /// When false, the selector suppresses drag-select interaction.
        /// </summary>
        protected virtual bool SelectionEnabled => true;

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
        public void Draw()
        {
            float topOriginScreenY = ImGui.GetCursorScreenPos().Y;
            float topH = ImGui.GetContentRegionAvail().Y;
            float topAvailW = ImGui.GetContentRegionAvail().X;
            float minimapColW = selector.ComputeMinimapColumnWidth(topH, topAvailW);
            float sameLineGaps = 2f * ImGui.GetStyle().ItemSpacing.X; // minimap<->zoomed and zoomed<->props
            float zoomedW = topAvailW - minimapColW - PropsWidth - sameLineGaps;

            selector.UpdateLayout(topH, zoomedW);

            ImGui.BeginChild("##minimapFcol", new Vector2(minimapColW, topH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            selector.DrawMinimap(topH);
            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.BeginChild("##zoomed", new Vector2(zoomedW, topH), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            selector.DrawZoomedView(topH, SelectionEnabled);
            ImGui.EndChild();
            ImGui.SameLine();

            float propsLeftScreenX = ImGui.GetCursorScreenPos().X;

            ImGui.BeginChild("##props", new Vector2(PropsWidth, topH), ImGuiChildFlags.Borders,
                ImGuiWindowFlags.AlwaysVerticalScrollbar);
            DrawPropsPanel();
            ImGui.EndChild();

            selector.HandleScrollInput(topOriginScreenY + topH, propsLeftScreenX);
        }
    }
}
