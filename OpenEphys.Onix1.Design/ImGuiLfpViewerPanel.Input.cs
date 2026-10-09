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
        /// Natural log of the factor the range changes by per pixel of Alt+Drag: it halves every 140 or so upward.
        /// </summary>
        const double RangeDragRate = 0.005;

        /// <summary>
        /// Natural log of the factor the timebase changes by per pixel of Ctrl+Drag: it halves every 140 or so
        /// rightward.
        /// </summary>
        const double TimebaseDragRate = 0.005;

        /// <summary>
        /// Significant digits a dragged range or timebase is rounded to.
        /// </summary>
        const int RangeDragDigits = 3;

        bool? dragHidden;
        int dragAnchor;

        // NB: while a drag is held, the value it began from and where the pointer was then.
        (double Value, float Y)? rangeDrag;
        (float Value, float Y, float Pivot, float Scroll)? heightDrag;
        (double Value, float X, long Anchor, double Fraction)? timebaseDrag;
        bool grabbing;

        // NB: wheel movement not yet a whole step, which a touchpad sends in fractions of a notch.
        float wheelSteps;

        float collapsedScroll;
        bool restoreScroll;

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
            (Keys.A, true, () => TimeKey(-1)),
            (Keys.D, true, () => TimeKey(1)),
        };

        // NB: these scroll the channels, which ImGui does to the window current when asked, so they are answered
        // inside the table that owns the scroll.
        (Keys, bool, Action<RowLayout>)[] TableKeys() => new (Keys, bool, Action<RowLayout>)[]
        {
            (Keys.W, true, layout => ChannelKey(-1, layout)),
            (Keys.S, true, layout => ChannelKey(1, layout)),
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

        // NB: every wheel over the rows is answered here, since their child is not scrolled by ImGui (WaveformPlot).
        // The gestures follow two conventions every application shares: Shift turns the wheel horizontal, as a
        // touchpad's sideways swipe is, and Ctrl zooms, as a touchpad's pinch arrives. So the wheel alone and with
        // Shift moves along the channels and through time, anywhere over the rows; with Ctrl it zooms the channel
        // height, and with Ctrl+Shift the timebase; with Alt it sets the gain, the range. Zooming and gain answer
        // from the plot only, and upward always magnifies.
        void HandleWheel(in RowLayout layout)
        {
            var io = ImGui.GetIO();
            var wheel = io.MouseWheel;
            var sideways = io.MouseWheelH;
            var mouse = ImGui.GetMousePos();
            if (wheel == 0 && sideways == 0 || !ImGui.IsWindowHovered())
                return;

            if (Modifiers())
            {
                var step = MathF.Floor(Math.Min(5 * ImGui.GetFontSize(), visibleHeight * 0.67f));
                ImGui.SetScrollY(ImGui.GetScrollY() - wheel * step);
                if (sideways != 0)
                    Pan((long)(sideways * (window.Span / TimeDivisions)));
                return;
            }

            if (Modifiers(shift: true))
            {
                Pan((long)(wheel * (window.Span / TimeDivisions)));
                return;
            }

            if (wheel == 0 || !OverPlot(mouse.X))
                return;

            wheelSteps += wheel;
            var steps = (int)wheelSteps;
            wheelSteps -= steps;
            for (var direction = Math.Sign(steps); steps != 0; steps -= direction)
            {
                // NB: anchored on the pointer rather than the middle of the view, so that whatever is being
                // looked at stays where it is. The keys and the dropdown have no pointer to speak of and anchor
                // on the middle instead.
                if (Modifiers(ctrl: true, shift: true))
                    StepTimebase(-direction, mouse.X);
                else if (Modifiers(alt: true))
                    StepRange(-direction);
                else if (Modifiers(ctrl: true) && expandedChannel < 0 && heightDrag is null)
                {
                    var height = ChannelHeight;
                    var pivot = (mouse.Y - layout.Origin) / layout.RowHeight;
                    SetChannelHeight(StepChannelHeight(height, direction), height, pivot, ImGui.GetScrollY(), layout);
                }
            }
        }

        /// <summary>
        /// W and S: alone or with Shift, scroll the channels; with Ctrl, step the channel height; with Alt, step the
        /// range. <paramref name="direction"/> is -1 for W, upward, which magnifies, as the wheel's upward does.
        /// </summary>
        void ChannelKey(int direction, in RowLayout layout)
        {
            if (Modifiers() || Modifiers(shift: true))
                ImGui.SetScrollY(ImGui.GetScrollY() + direction * ChannelStep(layout));
            else if (Modifiers(alt: true))
                StepRange(direction);
            else if (Modifiers(ctrl: true) && expandedChannel < 0)
            {
                var height = ChannelHeight;
                var pivot = ((layout.Top + layout.Bottom) / 2 - layout.Origin) / layout.RowHeight;
                SetChannelHeight(StepChannelHeight(height, -direction), height, pivot, ImGui.GetScrollY(), layout);
            }
        }

        /// <summary>
        /// A and D: alone or with Shift, move through time; with Ctrl, step the timebase about the middle of the
        /// plot. <paramref name="direction"/> is 1 for D, rightward, which magnifies, as a rightward Ctrl+Drag does.
        /// </summary>
        void TimeKey(int direction)
        {
            if (Modifiers() || Modifiers(shift: true))
                Pan(-direction * PanStep());
            else if (Modifiers(ctrl: true))
                StepTimebase(-direction, plotLeft + plotSpan / 2);
        }

        // NB: the modifier wheels and drags set the plot's scales, so they start on the plot only. The channel labels
        // have gestures of their own, and the plain wheel scrolls the channels anywhere, as ImGui does.
        bool OverPlot(float x) => x >= plotLeft && x < plotLeft + plotSpan;

        // NB: the modifier picks what a drag sets and its direction which axis: with Ctrl, up raises the channel
        // height and right shortens the timebase, so a diagonal zooms both; with Alt, up narrows the range. Each
        // works from where it began rather than frame to frame, so it ends where the pointer does, and an axis
        // answers only past the drag threshold, so a drag along one barely moves the other. A right drag grabs
        // the plot instead and moves it with the pointer, through the channels and, paused, through time.
        void HandleDrags(in RowLayout layout)
        {
            var mouse = ImGui.GetMousePos();
            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                (rangeDrag, heightDrag, timebaseDrag) = (null, null, null);

            if (!ImGui.IsMouseDown(ImGuiMouseButton.Right))
                grabbing = false;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsWindowHovered() && Modifiers())
                grabbing = true;

            if (grabbing && ImGui.IsMouseDragging(ImGuiMouseButton.Right))
            {
                var delta = ImGui.GetIO().MouseDelta;
                ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
                if (plotSpan > 0)
                    Pan((long)(delta.X * (window.Span / plotSpan)));
            }

            var clicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered() && OverPlot(mouse.X);

            if (clicked && Modifiers(alt: true))
                rangeDrag = (rangeAmplitude, mouse.Y);

            // NB: by a factor rather than an amount, so the drag feels alike at every range.
            if (rangeDrag is { } range)
                RangeAmplitude = Significant(range.Value * Math.Exp(Beyond(mouse.Y - range.Y) * RangeDragRate));

            if (clicked && Modifiers(ctrl: true))
            {
                var anchor = Axis.PositionAt(mouse.X);
                timebaseDrag = (timebase, mouse.X, anchor, window.FractionOf(anchor));
                if (expandedChannel < 0)
                    heightDrag = (ChannelHeight, mouse.Y, (mouse.Y - layout.Origin) / layout.RowHeight, ImGui.GetScrollY());
            }

            // NB: about the press point, which stays where it was pressed, as the wheel zooms about the pointer.
            // Rounded, which keeps the division labels readable and the decimators from being rebuilt on every
            // pixel of the drag.
            if (timebaseDrag is { } zoom)
            {
                var longest = standardTimeBases[ServableTimeBases - 1];
                var value = Significant(zoom.Value * Math.Exp(-Beyond(mouse.X - zoom.X) * TimebaseDragRate));
                SetTimebase(Math.Max(ShortestTimeBase, Math.Min(longest, value)), zoom.Anchor, zoom.Fraction);
            }

            if (heightDrag is { } height && expandedChannel < 0)
            {
                var delta = (int)Math.Round(-0.2f * Beyond(mouse.Y - height.Y));
                if (height.Value > 100)
                    delta *= 3;
                SetChannelHeight(height.Value + delta, height.Value, height.Pivot, height.Scroll, layout);
            }
        }

        // NB: how far a drag has gone along one axis past the drag threshold, so a drag held to one axis does not
        // move the other by the pointer's wobble.
        static float Beyond(float distance)
        {
            var threshold = ImGui.GetIO().MouseDragThreshold;
            return Math.Sign(distance) * Math.Max(0, Math.Abs(distance) - threshold);
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

        void StepTimebase(int direction, float x)
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
            SetTimebaseAt(stepped, x);
        }

        // NB: whatever is drawn at x stays there, so that what is being looked at stays put.
        void SetTimebaseAt(double value, float x)
        {
            var anchor = Axis.PositionAt(x);
            SetTimebase(value, anchor, window.FractionOf(anchor));
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
