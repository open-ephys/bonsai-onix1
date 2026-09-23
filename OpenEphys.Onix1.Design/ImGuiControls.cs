using System;
using System.Text;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Small ImGui rendering helpers shared across dialogs and panels: tooltips, fixed-size text
    /// buffer marshaling, and label/value row layout.
    /// </summary>
    internal static class ImGuiControls
    {
        public static void InfoRow(string label, string value)
        {
            ImGui.TextDisabled(label + ":");
            ImGui.SameLine();
            ImGui.TextUnformatted(value);
        }

        // Item ID + timestamp of the most recent click/drag/edit to finish. ImGui's own hover-delay
        // timer accumulates for as long as the mouse sits on an item, click or no click, so a
        // control that was already hovered-with-delay before being clicked stays "primed" through
        // the click and shows its tooltip instantly on release. Tracking this ourselves lets us
        // force the normal delay to re-elapse after an interaction, regardless of ImGui's own timer.
        static uint lastDeactivatedItemId;
        static double lastDeactivatedTime = -1.0;

        public static void Tooltip(string text, string disabledHint = null)
        {
            if (ImGui.IsItemDeactivated())
            {
                lastDeactivatedItemId = ImGui.GetItemID();
                lastDeactivatedTime = ImGui.GetTime();
            }

            if (ImGui.IsItemActive())
                return; // don't show tooltips while the user is interacting (e.g. dragging a slider)

            if (ImGui.GetItemID() == lastDeactivatedItemId &&
                ImGui.GetTime() - lastDeactivatedTime < ImGui.GetStyle().HoverDelayNormal)
                return; // just interacted with this control; wait out the normal delay again

            if (!ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                return; // only show when hovering for a delay and allow for disabled components as well
            bool disabled = ((ImGuiItemFlagsPrivate)ImGuiP.GetItemFlags() & ImGuiItemFlagsPrivate.Disabled) != 0;
            ImGui.SetTooltip(disabled && disabledHint != null ? $"{text}\n({disabledHint})" : text);
        }

        public static void WriteString(byte[] buf, string value)
        {
            Array.Clear(buf, 0, buf.Length);
            if (!string.IsNullOrEmpty(value))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                Array.Copy(bytes, buf, Math.Min(bytes.Length, buf.Length - 1));
            }
        }

        public static string ReadBuffer(byte[] buf)
        {
            int len = Array.IndexOf(buf, (byte)0);
            return Encoding.UTF8.GetString(buf, 0, len < 0 ? buf.Length : len);
        }

        public enum ModalChoice { Pending, Accepted, Declined }

        /// <summary>
        /// Draws the modal popup identified by <paramref name="id"/> if it's currently open (via
        /// <see cref="ImGui.OpenPopup(string)"/>), showing <paramref name="message"/> above an accept/decline
        /// button pair. Call every frame regardless of whether the popup is open; it draws nothing otherwise.
        /// </summary>
        public static ModalChoice ConfirmModal(string id, string message, string acceptLabel = "Continue", string declineLabel = "Cancel")
        {
            var choice = ModalChoice.Pending;
            if (ImGui.BeginPopupModal(id, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25f);
                ImGui.TextUnformatted(message);
                ImGui.PopTextWrapPos();
                ImGui.Spacing();
                if (ImGui.Button(acceptLabel)) { choice = ModalChoice.Accepted; ImGui.CloseCurrentPopup(); }
                ImGui.SameLine();
                if (ImGui.Button(declineLabel)) { choice = ModalChoice.Declined; ImGui.CloseCurrentPopup(); }
                ImGui.EndPopup();
            }
            return choice;
        }
    }
}
