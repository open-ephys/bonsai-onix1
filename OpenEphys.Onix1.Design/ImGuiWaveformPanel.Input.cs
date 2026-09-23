using System;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiWaveformPanel
    {
        bool? dragHidden;
        int dragAnchor;
        bool dragLeftAnchor;

        float collapsedScroll;
        bool restoreScroll;

        int heightDragStart = -1;
        float heightDragMouseY;
        float heightDragPivot;
        float heightDragScroll;

        // NB: the channel under the mouse is found from its y anywhere across the label column and
        // the plot, so a trace can be acted on where it is looked at.
        static int HoveredChannel(in RowLayout layout)
        {
            var mouse = ImGui.GetMousePos();
            var left = ImGui.GetCursorScreenPos().X;
            var right = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X;
            if (ImGui.GetScrollMaxY() > 0)
                right -= ImGui.GetStyle().ScrollbarSize;

            if (!ImGui.IsWindowHovered() ||
                mouse.X < left || mouse.X >= right ||
                mouse.Y < layout.Top || mouse.Y >= layout.Bottom)
            {
                return -1;
            }

            var channel = layout.ChannelAt(mouse.Y);
            return channel >= layout.FirstRow && channel < layout.LastRow ? channel : -1;
        }

        // NB: the scroll clamps to zero while one band fills the table, so the position from
        // before the expand is put back on collapse.
        void RestoreScrollIfPending()
        {
            if (restoreScroll)
            {
                ImGui.SetScrollY(collapsedScroll);
                restoreScroll = false;
            }
        }

        void HandleChannelInput(int channel)
        {
            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                dragHidden = null;
            if (channel < 0)
                return;

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (expandedChannel >= 0) Collapse();
                else Expand(channel);
                return;
            }

            if (expandedChannel >= 0)
                return;

            var rows = channelHidden.Length;
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.GetIO().KeyShift)
            {
                Array.Copy(channelHidden, dragOriginal, rows);
                dragHidden = !channelHidden[channel];
                dragAnchor = channel;
                dragLeftAnchor = false;
            }

            // NB: a drag that comes back to the pressed channel undoes it too, unlike a click that
            // never left it.
            if (dragHidden is bool paint)
            {
                dragLeftAnchor |= channel != dragAnchor;
                var lo = Math.Min(dragAnchor, channel);
                var hi = dragLeftAnchor && channel == dragAnchor ? lo - 1 : Math.Max(dragAnchor, channel);
                for (int c = 0; c < rows; c++)
                    channelHidden[c] = c >= lo && c <= hi ? paint : dragOriginal[c];
            }
        }

        // NB: an unclaimed wheel scrolls the channels, and ImGui applies that before this runs, so a
        // gesture that took the wheel back would show a frame of the wrong scroll. Shift avoids it by
        // making the wheel horizontal, which the table cannot do, and so do the modifier sets built on
        // Shift; Ctrl alone is claimed by ImGui's own font zoom. Those are the three used here.
        void HandleZoomInput(in RowLayout layout)
        {
            var io = ImGui.GetIO();
            var mouse = ImGui.GetMousePos();

            // NB: the keys and the panning wheel are handled ahead of the rest because they answer
            // wherever the pointer is within the pane, as the pause key does, and apply to an expanded
            // channel as well. Scrolling has to be asked for inside the table that owns it.
            if (ImGui.IsKeyPressed(ImGuiKey.W))
                ImGui.SetScrollY(ImGui.GetScrollY() - layout.RowHeight);
            else if (ImGui.IsKeyPressed(ImGuiKey.S))
                ImGui.SetScrollY(ImGui.GetScrollY() + layout.RowHeight);

            if (io.MouseWheel != 0 && io.KeyCtrl && io.KeyShift && ImGui.IsWindowHovered())
            {
                Pan(Math.Sign(io.MouseWheel) * (WindowSamples / TimeDivisions));
                return;
            }

            // NB: range scales works on expanded channel, so it must go before early return due to expandedChannel >= 0
            if (io.MouseWheel != 0 && io.KeyShift && ImGui.IsWindowHovered())
                StepRange(io.MouseWheel > 0 ? 1 : -1);

            if (heightDragStart >= 0 && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
                heightDragStart = -1;

            if (expandedChannel >= 0 || !ImGui.IsWindowHovered() && heightDragStart < 0)
                return;

            if (io.MouseWheel != 0 && io.KeyCtrl && heightDragStart < 0)
            {
                var step = io.MouseWheel > 0 ? 2 : -2;
                if (channelHeight > 100)
                    step *= 3;
                var pivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                SetChannelHeight(channelHeight + step, channelHeight, pivot, ImGui.GetScrollY(), layout);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && io.KeyCtrl && ImGui.IsWindowHovered())
            {
                heightDragStart = channelHeight;
                heightDragMouseY = mouse.Y;
                heightDragPivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                heightDragScroll = ImGui.GetScrollY();
            }

            if (heightDragStart >= 0)
            {
                var delta = (int)Math.Round(-0.2f * (mouse.Y - heightDragMouseY));
                if (heightDragStart > 100)
                    delta *= 3;
                SetChannelHeight(heightDragStart + delta, heightDragStart, heightDragPivot, heightDragScroll, layout);
            }
        }

        // NB: the channel that was under the pointer when the gesture began is kept at the same
        // screen position by moving the scroll with the height change.
        void SetChannelHeight(int height, int baseHeight, float pivot, float baseScroll, in RowLayout layout)
        {
            height = Math.Max(MinChannelHeight, Math.Min((int)(layout.Bottom - layout.Top), height));
            if (height == channelHeight)
                return;

            channelHeight = height;
            ImGui.SetScrollY(baseScroll + pivot * (height - baseHeight));
        }

        void StepRange(int direction)
        {
            var i = Array.BinarySearch(StandardRanges, rangeAmplitude);
            if (i < 0)
            {
                i = ~i;
                if (direction < 0)
                    i--;
            }
            else
            {
                i += direction;
            }

            rangeAmplitude = StandardRanges[Math.Max(0, Math.Min(StandardRanges.Length - 1, i))];
        }

        void Expand(int channel)
        {
            collapsedScroll = ImGui.GetScrollY();
            expandedChannel = channel;
        }

        void Collapse()
        {
            expandedChannel = -1;
            restoreScroll = true;
        }
    }
}
