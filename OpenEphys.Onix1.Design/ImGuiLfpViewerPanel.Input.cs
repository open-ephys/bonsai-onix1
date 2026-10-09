using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
using Keys = System.Windows.Forms.Keys;

namespace OpenEphys.Onix1.Design
{
    partial class ImGuiLfpViewerPanel
    {
        /// <summary>
        /// How much of a window, or of the channels on screen, a coarse step covers.
        /// </summary>
        const float CoarsePanFraction = 0.8f;

        /// <summary>
        /// Natural log of the factor the range changes by per pixel of Shift+Drag: it doubles every 140 or so.
        /// </summary>
        const double RangeDragRate = 0.005;

        /// <summary>
        /// Significant digits a dragged range is rounded to.
        /// </summary>
        const int RangeDragDigits = 3;

        bool? dragHidden;
        int dragAnchor;

        double? rangeDragStart;
        float rangeDragMouseY;

        float collapsedScroll;
        bool restoreScroll;

        float heightDragStart = -1;
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

        // NB: the scroll clamps to zero while one channel fills the table, so the one from before the expand is
        // put back once the rows are back.
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

            // NB: only from the labels, which leaves a click on a trace to select it and Shift+Drag there to set
            // the range. Set rather than toggled, so a drag across a mix of shown and hidden channels gives one
            // result.
            var rows = channelHidden.Length;
            var onLabels = ImGui.GetMousePos().X < plotLeft;
            if (onLabels && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && (Modifiers() || Modifiers(shift: true)))
            {
                Array.Copy(channelHidden, dragOriginal, rows);
                dragHidden = ImGui.GetIO().KeyShift;
                dragAnchor = channel;
                if (!ImGui.GetIO().KeyShift)
                    selectedChannel = channel;
            }

            if (dragHidden is bool hide)
            {
                var lo = Math.Min(dragAnchor, channel);
                var hi = Math.Max(dragAnchor, channel);
                for (int c = 0; c < rows; c++)
                    channelHidden[c] = c >= lo && c <= hi ? hide : dragOriginal[c];

                // NB: a hidden channel shows nothing to select, and the cursors would read nothing from it.
                if (selectedChannel >= 0 && channelHidden[selectedChannel])
                    selectedChannel = -1;
            }
        }

        readonly (Keys Key, bool Repeat, Action Answer)[] frameKeys;
        readonly (Keys Key, bool Repeat, Action<RowLayout> Answer)[] tableKeys;

        (Keys, bool, Action)[] FrameKeys() => new (Keys, bool, Action)[]
        {
            (Keys.Space, true, () => Paused = !Paused),
            (Keys.C, false, () => ShowCursors = !ShowCursors),
            (Keys.H, false, () => ShowHeatmap = !ShowHeatmap),
            (Keys.F, false, () => FitChannels = !FitChannels),
            (Keys.Escape, false, () => selectedChannel = -1),
            (Keys.A, true, () => Pan(PanStep())),
            (Keys.D, true, () => Pan(-PanStep())),
        };

        // NB: these scroll the channels, which ImGui does to the window current when asked, so they are answered
        // inside the table that owns the scroll.
        (Keys, bool, Action<RowLayout>)[] TableKeys() => new (Keys, bool, Action<RowLayout>)[]
        {
            (Keys.W, true, layout => ImGui.SetScrollY(ImGui.GetScrollY() - ChannelStep(layout))),
            (Keys.S, true, layout => ImGui.SetScrollY(ImGui.GetScrollY() + ChannelStep(layout))),
            (Keys.Q, true, layout => { if (Modifiers()) StepSelection(-1, layout); }),
            (Keys.E, true, layout => { if (Modifiers()) StepSelection(1, layout); }),
            (Keys.X, false, layout =>
            {
                if (selectedChannel < 0 && expandedChannel < 0)
                    selectedChannel = FirstShownInView(layout);
                if (expandedChannel >= 0) Collapse();
                else if (selectedChannel >= 0) Expand(selectedChannel);
            }),
        };

        /// <summary>
        /// Every key the panel answers, for a host to hand to whichever panel is under the pointer.
        /// </summary>
        public IEnumerable<Keys> Hotkeys => frameKeys.Select(k => k.Key).Concat(tableKeys.Select(k => k.Key));

        void AnswerFrameKeys()
        {
            foreach (var key in frameKeys)
            {
                if (Pressed(key.Key, key.Repeat))
                    key.Answer();
            }
        }

        void AnswerTableKeys(in RowLayout layout)
        {
            foreach (var key in tableKeys)
            {
                if (Pressed(key.Key, key.Repeat))
                    key.Answer(layout);
            }
        }

        // NB: not a key typed into a text field.
        static bool Pressed(Keys key, bool repeat) =>
            !ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ToImGuiKey(key), repeat);

        static ImGuiKey ToImGuiKey(Keys key) => key switch
        {
            Keys.Space => ImGuiKey.Space,
            Keys.Escape => ImGuiKey.Escape,
            >= Keys.A and <= Keys.Z => ImGuiKey.A + (key - Keys.A),
            _ => throw new ArgumentOutOfRangeException(nameof(key)),
        };

        /// <summary>
        /// Whether exactly the named modifiers are down and the rest are up.
        /// </summary>
        /// <remarks>
        /// Testing only whether a modifier is down lets gestures overlap: Shift+Alt+click would also fire the
        /// Shift+click gesture.
        /// </remarks>
        internal static bool Modifiers(bool ctrl = false, bool shift = false, bool alt = false)
        {
            var io = ImGui.GetIO();
            return io.KeyCtrl == ctrl && io.KeyShift == shift && io.KeyAlt == alt && !io.KeySuper;
        }

        // NB: an unclaimed wheel scrolls the channels, and ImGui applies that before this runs, so a
        // gesture that took the wheel back would show a frame of the wrong scroll. Shift avoids it by
        // making the wheel horizontal, which the table cannot do, and so do the modifier sets built on
        // Shift; Ctrl alone is claimed by ImGui's own font zoom. Those are the three used here.
        void HandleZoomInput(in RowLayout layout)
        {
            var io = ImGui.GetIO();
            var mouse = ImGui.GetMousePos();

            // NB: the panning wheel is handled ahead of the rest because it answers wherever the pointer is
            // within the pane and applies to an expanded channel as well.
            if (io.MouseWheel != 0 && Modifiers(ctrl: true, shift: true) && ImGui.IsWindowHovered())
            {
                Pan(Math.Sign(io.MouseWheel) * (window.Span / TimeDivisions));
                return;
            }

            // NB: anchored on the pointer rather than the middle of the view, so that whatever is being
            // looked at stays where it is. The dropdown has no pointer to speak of and anchors on the
            // middle instead.
            if (io.MouseWheel != 0 && Modifiers(shift: true, alt: true) && ImGui.IsWindowHovered())
            {
                StepTimebase(io.MouseWheel > 0 ? -1 : 1, PlotFraction(mouse.X));
                return;
            }

            // NB: range scales works on expanded channel, so it must go before early return due to expandedChannel >= 0
            if (io.MouseWheel != 0 && Modifiers(shift: true) && ImGui.IsWindowHovered())
                StepRange(io.MouseWheel > 0 ? 1 : -1);

            if (rangeDragStart is not null && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
                rangeDragStart = null;

            // NB: on the plot only, since Shift on the labels hides channels.
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Modifiers(shift: true) && ImGui.IsWindowHovered() &&
                mouse.X >= plotLeft && mouse.X < plotLeft + plotSpan)
            {
                rangeDragStart = rangeAmplitude;
                rangeDragMouseY = mouse.Y;
            }

            // NB: up widens the range, as the wheel does, by a factor rather than an amount so the drag feels alike
            // at every range.
            if (rangeDragStart is double startRange)
                RangeAmplitude = Significant(startRange * Math.Exp((rangeDragMouseY - mouse.Y) * RangeDragRate));

            if (heightDragStart >= 0 && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
                heightDragStart = -1;

            if (expandedChannel >= 0 || !ImGui.IsWindowHovered() && heightDragStart < 0)
                return;

            if (io.MouseWheel != 0 && Modifiers(ctrl: true) && heightDragStart < 0)
            {
                var height = ChannelHeight;
                var pivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                SetChannelHeight(StepChannelHeight(height, Math.Sign(io.MouseWheel)), height, pivot, ImGui.GetScrollY(), layout);
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Modifiers(ctrl: true) && ImGui.IsWindowHovered())
            {
                heightDragStart = ChannelHeight;
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
        void SetChannelHeight(float height, float baseHeight, float pivot, float baseScroll, in RowLayout layout)
        {
            // NB: a drag that has not moved yet leaves a fitted height fitted.
            if (height == baseHeight)
                return;

            ChannelHeight = Math.Min(layout.Bottom - layout.Top, height);
            ImGui.SetScrollY(baseScroll + pivot * (ChannelHeight - baseHeight));
        }

        void StepTimebase(int direction, float anchorFraction)
        {
            var offered = ServableTimeBases;
            var i = Array.BinarySearch(standardTimeBases, 0, offered, timebase);
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

            var stepped = standardTimeBases[Math.Max(0, Math.Min(offered - 1, i))];
            SetTimebase(stepped, PositionAtFraction(anchorFraction), anchorFraction);
        }

        static double Significant(double value)
        {
            var scale = Math.Pow(10, Math.Floor(Math.Log10(value)) - RangeDragDigits + 1);
            return Math.Round(value / scale) * scale;
        }

        void StepRange(int direction)
        {
            var i = Array.BinarySearch(standardRanges, rangeAmplitude);
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

            rangeAmplitude = standardRanges[Math.Max(0, Math.Min(standardRanges.Length - 1, i))];
        }

        static float ChannelStep(in RowLayout layout)
        {
            var visibleRows = Math.Max(1, layout.LastVisible - layout.FirstVisible);
            return Modifiers(shift: true)
                ? (int)(visibleRows * CoarsePanFraction) * layout.RowHeight
                : layout.RowHeight;
        }

        // NB: the same relationship to a window as ChannelStep has to the channels in view.
        long PanStep() => Modifiers(shift: true)
            ? (long)(window.Span * CoarsePanFraction)
            : window.Span / TimeDivisions;

        /// <summary>
        /// Selects the next shown channel in <paramref name="direction"/>, and brings it into view: expanded in
        /// its turn if a channel is expanded, or scrolled to otherwise.
        /// </summary>
        void StepSelection(int direction, in RowLayout layout)
        {
            var from = expandedChannel >= 0 ? expandedChannel
                : selectedChannel >= 0 ? selectedChannel
                : layout.FirstVisible - direction;
            for (var c = from + direction; c >= 0 && c < channelHidden.Length; c += direction)
            {
                if (!channelHidden[c])
                {
                    selectedChannel = c;
                    break;
                }
            }

            if (selectedChannel < 0)
                return;

            if (expandedChannel >= 0)
            {
                expandedChannel = selectedChannel;
                return;
            }

            var top = layout.RowTop(selectedChannel);
            if (top < layout.Top)
                ImGui.SetScrollY(ImGui.GetScrollY() - (layout.Top - top));
            else if (top + layout.RowHeight > layout.Bottom)
                ImGui.SetScrollY(ImGui.GetScrollY() + (top + layout.RowHeight - layout.Bottom));
        }

        /// <summary>
        /// The first channel in view that is not hidden, or -1, which is what anything needing a selection
        /// selects when there is none.
        /// </summary>
        int FirstShownInView(in RowLayout layout)
        {
            for (int c = layout.FirstVisible; c < layout.LastVisible; c++)
            {
                if (!channelHidden[c])
                    return c;
            }
            return -1;
        }

        void Expand(int channel)
        {
            collapsedScroll = ImGui.GetScrollY();
            expandedChannel = channel;
        }

        // NB: back where the expand began, moved only as far as brings the channel that was expanded into view,
        // since Q and E may have stepped away from it. Not at all if it is in view there already.
        void Collapse()
        {
            var height = ChannelHeight;
            var rowTop = expandedChannel * height;
            if (rowTop < collapsedScroll)
                collapsedScroll = rowTop;
            else if (rowTop + height > collapsedScroll + visibleHeight)
                collapsedScroll = rowTop + height - visibleHeight;

            expandedChannel = -1;
            restoreScroll = true;
        }
    }
}
