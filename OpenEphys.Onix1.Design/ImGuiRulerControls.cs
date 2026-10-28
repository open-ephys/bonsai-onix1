using Hexa.NET.ImGui;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    static class ImGuiRulerControls
    {
        // NB: lit while a ruler waits to be placed, since R arms one with nothing else on screen to say so until
        // the pointer is over the view.
        public static void DrawToggle(ImGuiProbeSelector selector, string label)
        {
            var placing = selector.PlacingRuler;
            if (placing)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

            if (ImGui.Button(label + "##ruler"))
                selector.ToggleRuler();
            ImGuiWidgets.Tooltip("Add a ruler to the probe view. Click and drag in the view, or click its two ends, " +
                "to place it. Click a ruler to select it, and press Delete to remove it, or Shift+Delete to remove " +
                "them all.\nKey: R over the probe view");

            if (placing)
                ImGui.PopStyleColor();
        }

        public static void DrawClear(ImGuiProbeSelector selector)
        {
            ImGui.BeginDisabled(selector.RulerCount == 0);
            if (ImGui.Button("Clear##rulers"))
                selector.RemoveRulers();
            ImGuiWidgets.Tooltip("Remove every ruler from the probe view.\nKey: Shift+Delete over the probe view",
                "There are no rulers.");
            ImGui.EndDisabled();
        }
    }
}
