using Hexa.NET.ImGui;
using OpenEphys.ProbeInterface.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Minimal contact descriptor passed from the owning dialog to the probe selector.
    /// </summary>
    internal readonly record struct ProbeContact(int Index, Vector2 Position, Vector2 SizeUm);

    /// <summary>
    /// One entry in the mode legend.
    /// </summary>
    internal readonly record struct LegendEntry(uint Color, string Label, bool DottedOutline = false, bool OutlineOnly = false);

    internal enum DragSelectIntent { Idle, Add, Remove }

    /// <summary>
    /// Self-contained ImGui rendering component that displays a probe minimap, a scrollable/zoomable zoomed
    /// view, and drag-select interaction for probe contacts. The three <c>Func</c> callback properties allow
    /// the owning dialog to supply probe-specific coloring and blocked-contact logic while the selector
    /// remains probe-agnostic.
    /// </summary>
    internal class ImGuiProbeSelector
    {
        /// <summary>
        /// Returns the ABGR fill color for the contact at the given hardware index.
        /// </summary>
        public Func<int, uint> GetFillColor { get; set; } = _ => ColContactDefault;

        /// <summary>
        /// Returns true if the contact at the given hardware index is blocked from selection.
        /// </summary>
        public Func<int, bool> IsBlocked { get; set; } = _ => false;

        /// <summary>
        /// When true, blocked contacts use GetFillColor instead of ColContactBlocked.
        /// </summary>
        public bool FillColorOverridesBlocked { get; set; } = false;

        /// <summary>
        /// True for each contact geometrically inside the current drag box, regardless of intent (ADD or
        /// REMOVE). Unlike <see cref="SelectedContacts"/> and <see cref="InspectedContacts"/>, this does not
        /// accumulate across gestures: it is cleared and rebuilt from just the current box every frame of a
        /// drag (and on a plain click). Filtered by <see cref="SelectionSkipsBlocked"/>, same as
        /// SelectedContacts. Used by bank-select mode, which reads it once per gesture to know what was just
        /// swept and then calls <see cref="ClearSelection"/> immediately after committing.
        /// </summary>
        public bool[] DragBoxContacts { get; private set; } = Array.Empty<bool>();

        /// <summary>
        /// Boolean selection flags, one per contact, aligned with the contacts list supplied to <see
        /// cref="Refresh"/>. True means selected. Accumulates across gestures (shift-drag adds, ctrl+shift
        /// removes) and is filtered by <see cref="SelectionSkipsBlocked"/>. This is the actionable
        /// selection that Enable/Pin/Unpin can act on.
        /// </summary>
        public bool[] SelectedContacts { get; private set; } = Array.Empty<bool>();

        /// <summary>
        /// Boolean flags, one per contact, aligned with <see cref="SelectedContacts"/>. Accumulates across
        /// gestures the same way SelectedContacts does (same add/remove modifier semantics, same gesture
        /// history), but is never filtered by <see cref="SelectionSkipsBlocked"/> — so it is always a
        /// superset of SelectedContacts, and identical to it whenever SelectionSkipsBlocked is false.
        /// Intended for display-only consumers (e.g. a Contact Info panel) that want blocked contacts swept
        /// up in a selection gesture to remain visible, without exposing them to actionable operations.
        /// Cleared alongside SelectedContacts.
        /// </summary>
        public bool[] InspectedContacts { get; private set; } = Array.Empty<bool>();

        /// <summary>
        /// Current drag-select mode: Add (default), Remove (Ctrl+Shift), or Idle when no drag is active.
        /// </summary>
        public DragSelectIntent DragIntent { get; private set; } = DragSelectIntent.Idle;

        /// <summary>
        /// Index into the contacts list of the contact currently under the mouse cursor in the zoomed view,
        /// or -1 when the cursor is not over any contact or the window is not hovered. Reset to -1 each frame
        /// by <see cref="DrawZoomedView"/>.
        /// </summary>
        public int HoveredContactIndex { get; private set; } = -1;

        /// <summary>
        /// When false, blocked contacts are included in drag-box selection (bank-select mode).
        /// </summary>
        public bool SelectionSkipsBlocked { get; set; } = true;

        /// <summary>
        /// Mode label rendered in the lower-left of the zoomed view.
        /// </summary>
        public string ModeLabel { get; set; } = string.Empty;

        /// <summary>
        /// Legend swatches rendered to the right of the mode label. Set per-frame by the owner.
        /// </summary>
        public LegendEntry[] Legend { get; set; } = Array.Empty<LegendEntry>();

        /// <summary>
        /// The outline color drawn around <see cref="SelectedContacts"/> in <see cref="DrawZoomedView"/>.
        /// Exposed so an owner building <see cref="Legend"/> entries can reference the color actually
        /// rendered instead of re-declaring a copy of it.
        /// </summary>
        public const uint SelectionBorderColor = ColContactSelBorder;

        /// <summary>
        /// The outline color drawn around contacts that are in <see cref="InspectedContacts"/> but not
        /// <see cref="SelectedContacts"/>. Exposed for the same reason as <see cref="SelectionBorderColor"/>.
        /// </summary>
        public const uint InspectionBorderColor = ColContactInspectBorder;

        /// <summary>
        /// The fill color used for blocked contacts (unless overridden by <see
        /// cref="FillColorOverridesBlocked"/>). Exposed for the same reason as
        /// <see cref="SelectionBorderColor"/>.
        /// </summary>
        public const uint BlockedFillColor = ColContactBlocked;

        /// <summary>
        /// Fired after any change to <see cref="SelectedContacts"/>.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Pixel width reserved for the depth-ruler column on the left of the minimap and on
        /// both sides of the zoomed view.
        /// </summary>
        public const float RulerColumnWidthPx = 68f;

        /// <summary>
        /// Pixel padding applied above and below the contact area inside the minimap panel.
        /// </summary>
        public const float MinimapPaddingPx = 8f;

        /// <summary>
        /// Default horizontal position of the center of the zoom window on the probe in microns. If null,
        /// defaults to geometic center of probe.
        /// </summary>
        internal float? DefaultScrollXMicrons = null;

        /// <summary>
        /// Default vertical position of the center of the zoom window on the probe in microns. If null,
        /// defaults to geometic center of probe.
        /// </summary>
        internal float? DefaultScrollYMicrons = null;

        /// <summary>
        /// Default major extent of the zoom window in microns.
        /// </summary>
        internal float DefaultZoomWindowMicrons = 1000f;

        // probe state
        ProbeGroup probeGroup;
        IReadOnlyList<ProbeContact> contacts = Array.Empty<ProbeContact>();

        // probe bounds in microns
        float probeXMin, probeXMax, probeYMin, probeYMax;

        // scroll / zoom
        float scrollYMicrons, scrollXMicrons; // Defaulted to half probe width
        float zoomWindowUm; // defines the major axis length of the zoom window.
        float effectiveXWindowUm;

        // drag-select state machine
        bool isDragging, dragHadShift, dragHadCtrl;
        Vector2 dragStart, dragCurrent;
        bool[] preDragSelection = Array.Empty<bool>();
        bool[] preDragInspected = Array.Empty<bool>();

        // middle-mouse pan
        bool isMiddlePanning;
        Vector2 middlePanLastPos;

        // ruler tool
        readonly ImGuiProbeRuler ruler = new();
        bool rulerMode;

        // visual constants
        const uint  ColContactDefault   = ImGuiPalette.Grey0x4D;
        const uint  ColContactBlocked   = ImGuiPalette.Black;
        const uint  ColBlockedOutline   = ImGuiPalette.Grey0x77;
        const uint  ColContactSelBorder = ImGuiPalette.Yellow;
        const uint  ColContactInspectBorder = ImGuiPalette.Grey0x99;
        const float ContactSelBorderWidth = 2f;
        const uint  ColContour          = ImGuiPalette.Grey0x80;
        const float ContourWidthMinimap = 1f;
        const float ContourWidthZoomed  = 2f;
        static readonly uint ColZoomBoxFill = ImGuiPalette.WithAlpha(ImGuiPalette.Yellow, 0x22);
        const uint  ColZoomBoxBorder    = ImGuiPalette.Yellow;
        const uint  ColRulerAxis        = ImGuiPalette.Grey0x55;
        const uint  ColRulerMajorTick   = ImGuiPalette.Grey0x88;
        const uint  ColRulerLabel       = ImGuiPalette.Grey0x99;
        const uint  ColGridMajor        = ImGuiPalette.Grey0x55;
        const uint  ColGridMinor        = ImGuiPalette.Grey0x30;
        const float RulerMajorTickLen   = 6f;
        const float RulerMinorTickLen   = 3f;
        const uint  ColDragSelect       = ImGuiPalette.Yellow;
        const uint  ColDragDeselect     = ImGuiPalette.Red;
        const float ClickThresholdSq    = 16f;
        static readonly uint ColCoordOverlayBg = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0xBB);
        const uint  ColCoordOverlayText = ImGuiPalette.White;
        const float GridMarginHPx       = 60f;
        const float GridMarginVPx       = 60f;
        static readonly uint ColContourFill = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0xBB);

        /// <summary>
        /// Loads a new probe group and contact list. Expands the internal probe bounds from
        /// each contact's full extent (position ± SizeUm/2) plus the planar contour vertices,
        /// resets the selection array, and centers the scroll position.
        /// </summary>
        /// <param name="probeGroup">The probe group providing the planar contour.</param>
        /// <param name="contacts">
        /// Ordered list of contacts to display, each carrying its own position and size in
        /// microns. Selection indices are aligned to this list.
        /// </param>
        public void Refresh(ProbeGroup probeGroup, IReadOnlyList<ProbeContact> contacts)
        {
            this.probeGroup = probeGroup;
            this.contacts = contacts;
            SelectedContacts = new bool[contacts.Count];
            DragBoxContacts  = new bool[contacts.Count];
            InspectedContacts = new bool[contacts.Count];
            preDragSelection = new bool[contacts.Count];
            preDragInspected = new bool[contacts.Count];
            ruler.Units = probeGroup?.Probes.FirstOrDefault()?.SiUnits.ToString() ?? "um";

            probeXMin = probeXMax = probeYMin = probeYMax = 0f;
            bool init = false;
            void Expand(float x, float y)
            {
                if (!init) { probeXMin = probeXMax = x; probeYMin = probeYMax = y; init = true; return; }
                if (x < probeXMin) probeXMin = x; if (x > probeXMax) probeXMax = x;
                if (y < probeYMin) probeYMin = y; if (y > probeYMax) probeYMax = y;
            }

            // Expand by full contact extents so edge contacts are not clipped.
            foreach (var c in contacts)
            {
                Expand(c.Position.X - c.SizeUm.X / 2f, c.Position.Y - c.SizeUm.Y / 2f);
                Expand(c.Position.X + c.SizeUm.X / 2f, c.Position.Y + c.SizeUm.Y / 2f);
            }

            if (probeGroup != null)
            {
                foreach (var probe in probeGroup.Probes)
                {
                    if (probe?.ProbePlanarContour == null) continue;
                    foreach (var v in probe.ProbePlanarContour) Expand((float)v[0], (float)v[1]);
                }
            }

            zoomWindowUm = DefaultZoomWindowMicrons;
            scrollYMicrons = DefaultScrollYMicrons ?? (probeYMin + probeYMax) / 2f;
            ClampScroll();
            scrollXMicrons = DefaultScrollXMicrons ?? (probeXMin + probeXMax) / 2f;
            var (mxMin, mxMax, _, _) = GetMmBounds();
            effectiveXWindowUm = mxMax - mxMin;
        }

        /// <summary>
        /// Returns the full minimap column width in pixels (ruler column + contact area), capped
        /// to 18% of <paramref name="displayWidth"/> so that the minimap does not dominate wide
        /// windows.
        /// </summary>
        public float ComputeMinimapColumnWidth(float availH, float displayWidth)
        {
            if (contacts.Count == 0) return RulerColumnWidthPx + 24f;
            float probeHUm = probeYMax - probeYMin;
            float probeWUm = probeXMax - probeXMin;
            float mmH = availH - MinimapPaddingPx * 2f;
            float scale = mmH > 0 ? mmH / probeHUm : 0.01f;
            float contactW = Math.Max(24f, probeWUm * scale + 8f);
            return RulerColumnWidthPx + Math.Min(contactW * 1.33f, displayWidth * 0.24f);
        }

        /// <summary>
        /// Updates <c>effectiveXWindowUm</c> to match the aspect ratio of the zoomed view panel.
        /// Must be called once per frame after the zoomed panel width is known, before
        /// <see cref="DrawMinimap"/> or <see cref="DrawZoomedView"/>.
        /// </summary>
        public void UpdateLayout(float availH, float zoomedW)
        {
            var (mxMin, mxMax, _, _) = GetMmBounds();
            float mmTotalW = mxMax - mxMin;
            effectiveXWindowUm = availH > 0 && zoomedW > 0
                ? Math.Min(mmTotalW, zoomWindowUm * zoomedW / availH)
                : mmTotalW;
            ClampScrollX();
        }

        /// <summary>
        /// Deselects all contacts and clears drag state. Does not fire <see cref="SelectionChanged"/>.
        /// </summary>
        public void ClearSelection()
        {
            Array.Clear(SelectedContacts,  0, SelectedContacts.Length);
            Array.Clear(DragBoxContacts,   0, DragBoxContacts.Length);
            Array.Clear(InspectedContacts, 0, InspectedContacts.Length);
            DragIntent = DragSelectIntent.Idle;
        }

        /// <summary>
        /// Draws the minimap overview inside the current ImGui child window. Must be called once per frame
        /// inside a child window sized to the minimap column.
        /// </summary>
        public void DrawMinimap(float availH)
        {
            if (contacts.Count == 0) return;

            var (mmXMin, mmXMax, mmYMin, mmYMax) = GetMmBounds();
            float mmTotalW = mmXMax - mmXMin;
            float mmTotalH = mmYMax - mmYMin;
            float mmH  = availH - MinimapPaddingPx * 2f;
            float scale = mmH > 0 ? mmH / mmTotalH : 0.01f;
            float rendW = mmTotalW * scale;

            var dl = ImGui.GetWindowDrawList();
            var cp = ImGui.GetCursorScreenPos();
            float viewTop     = cp.Y + MinimapPaddingPx;
            float viewBot     = viewTop + mmH;
            float contactLeft = cp.X + RulerColumnWidthPx;

            // probeYMin/Max already include contact half-extents, so no adjustment needed here.
            float rulerTop = viewTop + (mmYMax - probeYMax) * scale;
            float rulerBot = viewTop + (mmYMax - probeYMin) * scale;
            DrawDepthRuler(dl, contactLeft - 2f, rulerTop, rulerBot, probeYMin, probeYMax);

            if (probeGroup != null)
                foreach (var probe in probeGroup.Probes)
                    DrawContour(dl, probe, contactLeft, viewTop, mmXMin, mmYMax, scale, ContourWidthMinimap);

            for (int i = 0; i < contacts.Count; i++)
            {
                var c  = contacts[i];
                float ew = Math.Max(1f, c.SizeUm.X * scale);
                float eh = Math.Max(1f, c.SizeUm.Y * scale);
                float sx = contactLeft + (c.Position.X - mmXMin) * scale - ew / 2f;
                float sy = viewTop     + (mmYMax - c.Position.Y) * scale - eh / 2f;
                dl.AddRectFilled(new Vector2(sx, sy), new Vector2(sx + ew, sy + eh), GetFillColor(c.Index));
            }

            float yHalf    = zoomWindowUm / 2f;
            float xHalf    = effectiveXWindowUm / 2f;
            float boxTop   = Clamp(viewTop + (mmYMax - (scrollYMicrons + yHalf)) * scale, viewTop, viewBot);
            float boxBot   = Clamp(viewTop + (mmYMax - (scrollYMicrons - yHalf)) * scale, viewTop, viewBot);
            float boxLeft  = Clamp(contactLeft + (scrollXMicrons - xHalf - mmXMin) * scale, contactLeft, contactLeft + rendW);
            float boxRight = Clamp(contactLeft + (scrollXMicrons + xHalf - mmXMin) * scale, contactLeft, contactLeft + rendW);
            dl.AddRectFilled(new Vector2(boxLeft, boxTop), new Vector2(boxRight, boxBot), ColZoomBoxFill);
            dl.AddRect(new Vector2(boxLeft, boxTop), new Vector2(boxRight, boxBot), ColZoomBoxBorder);

            ImGui.SetCursorScreenPos(new Vector2(contactLeft, viewTop));
            ImGui.InvisibleButton("##minimapbtn", new Vector2(Math.Max(1f, rendW), Math.Max(1f, mmH)));
            if (ImGui.IsItemActive())
            {
                var mmp = ImGui.GetMousePos();
                scrollYMicrons = mmYMax - (mmp.Y - viewTop)     / mmH   * mmTotalH;
                scrollXMicrons = mmXMin + (mmp.X - contactLeft) / rendW * mmTotalW;
                ClampScroll(); ClampScrollX();
            }

            ImGui.SetCursorScreenPos(cp);
            ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, availH));
        }

        /// <summary>
        /// Draws the scrollable/zoomable probe view inside the current ImGui child window. Must be called
        /// once per frame. Pass <paramref name="selectionEnabled"/> as false to suppress drag-select
        /// interaction (e.g., when no calibration file is loaded).
        /// </summary>
        public void DrawZoomedView(float availH, bool selectionEnabled)
        {
            if (contacts.Count == 0) { ImGui.TextDisabled("No contacts."); return; }

            HoveredContactIndex = -1;

            float xLow = scrollXMicrons - effectiveXWindowUm / 2f;
            float yLow = scrollYMicrons - zoomWindowUm       / 2f;

            var avail  = ImGui.GetContentRegionAvail();
            float zW   = avail.X;
            float scaleXY = effectiveXWindowUm > 0 && zoomWindowUm > 0
                ? Math.Min(zW / effectiveXWindowUm, avail.Y / zoomWindowUm) : 1f;
            float drawW = effectiveXWindowUm * scaleXY;
            float drawH = zoomWindowUm       * scaleXY;

            var dl = ImGui.GetWindowDrawList();
            var cp = ImGui.GetCursorScreenPos();
            float viewLeft = cp.X + (zW - drawW) / 2f;
            float viewTop  = cp.Y + (avail.Y - drawH) / 2f;
            float viewBot  = viewTop + drawH;

            float gridLeft  = cp.X + GridMarginHPx;
            float gridRight = cp.X + avail.X - GridMarginHPx;
            float gridTop   = cp.Y + GridMarginVPx;
            float gridBot   = cp.Y + avail.Y - GridMarginVPx;

            DrawGridLines(dl, gridLeft, gridRight, gridTop, gridBot,
                viewLeft, viewBot, xLow, yLow, scaleXY);

            ImGui.PushClipRect(new Vector2(gridLeft, gridTop), new Vector2(gridRight, gridBot), true);

            if (probeGroup != null)
                foreach (var probe in probeGroup.Probes)
                {
                    DrawContourFill(dl, probe, viewLeft, viewBot, xLow, yLow, scaleXY);
                    DrawContour(dl, probe, viewLeft, viewBot, xLow, -1f /* unused */, scaleXY,
                        ContourWidthZoomed, yOffset: yLow, yUp: true);
                }

            for (int i = 0; i < contacts.Count; i++)
            {
                var c = contacts[i];
                if (c.Position.Y + c.SizeUm.Y / 2f < yLow ||
                    c.Position.Y - c.SizeUm.Y / 2f > yLow + zoomWindowUm) continue;
                if (c.Position.X + c.SizeUm.X / 2f < xLow ||
                    c.Position.X - c.SizeUm.X / 2f > xLow + effectiveXWindowUm) continue;

                float rendEW = Math.Max(2f, c.SizeUm.X * scaleXY);
                float rendEH = Math.Max(2f, c.SizeUm.Y * scaleXY);
                float sx = viewLeft + (c.Position.X - xLow) * scaleXY - rendEW / 2f;
                float sy = viewBot  - (c.Position.Y - yLow) * scaleXY - rendEH / 2f;
                var rMin = new Vector2(sx, sy);
                var rMax = new Vector2(sx + rendEW, sy + rendEH);

                bool blocked = IsBlocked(c.Index);
                dl.AddRectFilled(rMin, rMax, blocked && !FillColorOverridesBlocked
                    ? ColContactBlocked
                    : GetFillColor(c.Index));
                if (blocked && !FillColorOverridesBlocked)
                    DrawDottedRect(dl, rMin, rMax, ColBlockedOutline);

                if (SelectedContacts[i])
                    dl.AddRect(new Vector2(sx - 1, sy - 1), new Vector2(sx + rendEW + 1, sy + rendEH + 1),
                        ColContactSelBorder, 0f, ContactSelBorderWidth);
                else if (InspectedContacts[i])
                    dl.AddRect(new Vector2(sx - 1, sy - 1), new Vector2(sx + rendEW + 1, sy + rendEH + 1),
                        ColContactInspectBorder, 0f, ContactSelBorderWidth);
            }

            if (selectionEnabled && !rulerMode)
                HandleDragSelect(dl, viewLeft, viewBot, xLow, yLow, scaleXY);

            if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            {
                isMiddlePanning = true;
                middlePanLastPos = ImGui.GetMousePos();
            }
            if (isMiddlePanning)
            {
                if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                {
                    var mp = ImGui.GetMousePos();
                    scrollXMicrons -= (mp.X - middlePanLastPos.X) / scaleXY;
                    scrollYMicrons += (mp.Y - middlePanLastPos.Y) / scaleXY;
                    ClampScroll(); ClampScrollX();
                    middlePanLastPos = mp;
                }
                else isMiddlePanning = false;
            }

            ImGui.PopClipRect();

            DrawGridLabels(dl, cp.X, cp.X + avail.X, cp.Y, cp.Y + avail.Y,
                gridLeft, gridRight, gridTop, gridBot, viewLeft, viewBot, xLow, yLow, scaleXY);

            if (!ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.R, false))
            {
                rulerMode = !rulerMode;
                if (!rulerMode) ruler.ResetHover();
            }

            if (ImGui.IsWindowHovered())
            {
                var mp = ImGui.GetMousePos();
                float mousePx = xLow + (mp.X - viewLeft) / scaleXY;
                float mousePy = yLow + (viewBot - mp.Y)  / scaleXY;
                bool inCanvas = mp.X >= gridLeft && mp.X <= gridRight &&
                                mp.Y >= gridTop  && mp.Y <= gridBot;

                if (rulerMode)
                    ruler.HandleRulerModeInput(new Vector2(mousePx, mousePy), inCanvas,
                        mp, viewLeft, viewBot, xLow, yLow, scaleXY);

                if (inCanvas)
                {
                    string coord = $"({mousePx / 1000f:0.00} mm, {mousePy / 1000f:0.00} mm)";
                    var sz  = ImGui.CalcTextSize(coord);
                    const float pad = 5f;
                    float refBot  = cp.Y + avail.Y - 18f;
                    var bgMin = new Vector2(cp.X + avail.X - sz.X - pad * 3f, refBot - sz.Y - pad);
                    var bgMax = new Vector2(cp.X + avail.X - pad, refBot);
                    dl.AddRectFilled(bgMin, bgMax, ColCoordOverlayBg, 3f);
                    dl.AddText(new Vector2(bgMin.X + pad, bgMin.Y + pad), ColCoordOverlayText, coord);

                    for (int i = 0; i < contacts.Count; i++)
                    {
                        var c = contacts[i];
                        float hw = c.SizeUm.X / 2f, hh = c.SizeUm.Y / 2f;
                        if (mousePx >= c.Position.X - hw && mousePx <= c.Position.X + hw &&
                            mousePy >= c.Position.Y - hh && mousePy <= c.Position.Y + hh)
                        { HoveredContactIndex = i; break; }
                    }
                }
            }

            // TODO: Hack. Make the interaciton state coherent
            string overlayTag = rulerMode ? "MODE: RULER (R to exit)" : ModeLabel;
            if (!string.IsNullOrEmpty(overlayTag))
            {
                const float pad = 5f;
                var tsz    = ImGui.CalcTextSize(overlayTag);
                float refBot = cp.Y + avail.Y - 18f;
                var tagMin = new Vector2(cp.X + pad, refBot - tsz.Y - pad);
                var tagMax = new Vector2(cp.X + pad * 3f + tsz.X, refBot);
                dl.AddRectFilled(tagMin, tagMax, ColCoordOverlayBg, 3f);
                dl.AddText(new Vector2(tagMin.X + pad, tagMin.Y + pad / 2f), ImGuiPalette.Yellow, overlayTag);
            }

            DrawLegend(dl, cp, avail);
            ruler.Draw(dl, viewLeft, viewBot, xLow, yLow, scaleXY, rulerMode);

            ImGui.Dummy(avail);
        }

        /// <summary>
        /// Handles mouse-wheel scroll and zoom input. Must be called from the root ImGui window (after all
        /// child windows). Input is ignored when the cursor is at or below <paramref name="bottomScreenY"/>
        /// or at or beyond <paramref name="rightScreenX"/>.
        /// </summary>
        public void HandleScrollInput(float bottomScreenY, float rightScreenX)
        {
            var io = ImGui.GetIO();
            if (io.MouseWheel == 0f) return;
            var mp = ImGui.GetMousePos();
            if (mp.Y >= bottomScreenY || mp.X >= rightScreenX) return;

            bool ctrl  = ImGui.IsKeyDown(ImGuiKey.LeftCtrl)  || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
            bool shift = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
            var (_, _, mmYMin, mmYMax) = GetMmBounds();

            if (ctrl)
            {
                float factor = io.MouseWheel > 0f ? 0.85f : 1.15f;
                zoomWindowUm = Math.Max(50f, Math.Min(mmYMax - mmYMin, zoomWindowUm * factor));
                ClampScroll(); ClampScrollX();
            }
            else if (shift)
            {
                scrollXMicrons -= io.MouseWheel * effectiveXWindowUm * 0.1f;
                ClampScrollX();
            }
            else
            {
                scrollYMicrons += io.MouseWheel * zoomWindowUm * 0.1f;
                ClampScroll();
            }
        }

        #region Private: scroll clamping

        (float xMin, float xMax, float yMin, float yMax) GetMmBounds()
        {
            float pw = probeXMax - probeXMin;
            float ph = probeYMax - probeYMin;
            const float m = 0.1f;
            return (probeXMin - m * pw, probeXMax + m * pw,
                    probeYMin - m * ph, probeYMax + m * ph);
        }

        void ClampScroll()
        {
            var (_, _, lo, hi) = GetMmBounds();
            float half = zoomWindowUm / 2f;
            if (lo + half > hi - half) { scrollYMicrons = (lo + hi) / 2f; return; }
            scrollYMicrons = Clamp(scrollYMicrons, lo + half, hi - half);
        }

        void ClampScrollX()
        {
            var (lo, hi, _, _) = GetMmBounds();
            float half = effectiveXWindowUm / 2f;
            if (lo + half > hi - half) { scrollXMicrons = (lo + hi) / 2f; return; }
            scrollXMicrons = Clamp(scrollXMicrons, lo + half, hi - half);
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;

        #endregion

        #region Private: drag-select state machine

        void HandleDragSelect(ImDrawListPtr dl, float viewLeft, float viewBot,
            float xLow, float yLow, float scaleXY)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            {
                Array.Clear(SelectedContacts,  0, SelectedContacts.Length);
                Array.Clear(DragBoxContacts,   0, DragBoxContacts.Length);
                Array.Clear(InspectedContacts, 0, InspectedContacts.Length);
                isDragging = false;
                DragIntent = DragSelectIntent.Idle;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!ImGui.IsWindowHovered()) return;

            var mp = ImGui.GetMousePos();
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !isDragging)
            {
                dragStart    = mp;
                dragHadShift = ImGui.IsKeyDown(ImGuiKey.LeftShift)  || ImGui.IsKeyDown(ImGuiKey.RightShift);
                dragHadCtrl  = ImGui.IsKeyDown(ImGuiKey.LeftCtrl)   || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
                isDragging   = true;
                Array.Copy(SelectedContacts,  preDragSelection, SelectedContacts.Length);
                Array.Copy(InspectedContacts, preDragInspected, InspectedContacts.Length);
                DragIntent = (dragHadShift && dragHadCtrl) ? DragSelectIntent.Remove : DragSelectIntent.Add;
            }

            if (!isDragging) return;

            dragCurrent   = mp;
            bool deselect = dragHadShift && dragHadCtrl;
            bool additive = dragHadShift && !dragHadCtrl;
            float dx = dragCurrent.X - dragStart.X;
            float dy = dragCurrent.Y - dragStart.Y;
            bool aboveThreshold = dx * dx + dy * dy >= ClickThresholdSq;

            if (aboveThreshold)
            {
                float minX = xLow + (Math.Min(dragStart.X, dragCurrent.X) - viewLeft) / scaleXY;
                float maxX = xLow + (Math.Max(dragStart.X, dragCurrent.X) - viewLeft) / scaleXY;
                float minY = yLow + (viewBot - Math.Max(dragStart.Y, dragCurrent.Y))  / scaleXY;
                float maxY = yLow + (viewBot - Math.Min(dragStart.Y, dragCurrent.Y))  / scaleXY;

                Array.Clear(DragBoxContacts, 0, DragBoxContacts.Length);
                UpdateBoxContacts(minX, maxX, minY, maxY, DragBoxContacts, true, SelectionSkipsBlocked);

                AccumulateSelection(SelectedContacts, preDragSelection, SelectionSkipsBlocked,
                    minX, maxX, minY, maxY, additive, deselect);
                AccumulateSelection(InspectedContacts, preDragInspected, false,
                    minX, maxX, minY, maxY, additive, deselect);

                dl.AddRect(dragStart, dragCurrent, deselect ? ColDragDeselect : ColDragSelect);
            }

            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                if (!aboveThreshold)
                    ApplyClick(viewLeft, viewBot, xLow, yLow, scaleXY, additive, deselect);
                isDragging = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        void AccumulateSelection(bool[] target, bool[] preDrag, bool skipBlocked,
            float minX, float maxX, float minY, float maxY, bool additive, bool deselect)
        {
            if (!additive && !deselect)
                Array.Clear(target, 0, target.Length);
            else
                Array.Copy(preDrag, target, target.Length);
            UpdateBoxContacts(minX, maxX, minY, maxY, target, !deselect, skipBlocked);
        }

        void ApplyClick(float viewLeft, float viewBot, float xLow, float yLow,
            float scaleXY, bool additive, bool deselect)
        {
            float cx = xLow + (dragStart.X - viewLeft) / scaleXY;
            float cy = yLow + (viewBot - dragStart.Y)  / scaleXY;

            Array.Clear(DragBoxContacts, 0, DragBoxContacts.Length);
            if (!additive && !deselect)
                Array.Clear(SelectedContacts, 0, SelectedContacts.Length);
            else
                Array.Copy(preDragSelection, SelectedContacts, SelectedContacts.Length);
            if (!additive && !deselect)
                Array.Clear(InspectedContacts, 0, InspectedContacts.Length);
            else
                Array.Copy(preDragInspected, InspectedContacts, InspectedContacts.Length);

            for (int i = 0; i < contacts.Count; i++)
            {
                var c = contacts[i];
                float hw = c.SizeUm.X / 2f, hh = c.SizeUm.Y / 2f;
                if (cx < c.Position.X - hw || cx > c.Position.X + hw ||
                    cy < c.Position.Y - hh || cy > c.Position.Y + hh) continue;

                bool blocked = IsBlocked(contacts[i].Index);
                if (!(SelectionSkipsBlocked && blocked))
                {
                    SelectedContacts[i] = !deselect;
                    DragBoxContacts[i]  = true;
                }
                InspectedContacts[i] = !deselect;
            }
        }

        void UpdateBoxContacts(float minX, float maxX, float minY, float maxY,
            bool[] target, bool value, bool skipBlocked)
        {
            for (int i = 0; i < contacts.Count; i++)
            {
                if (skipBlocked && IsBlocked(contacts[i].Index)) continue;
                var c = contacts[i];
                if (c.Position.X + c.SizeUm.X / 2f >= minX && c.Position.X - c.SizeUm.X / 2f <= maxX &&
                    c.Position.Y + c.SizeUm.Y / 2f >= minY && c.Position.Y - c.SizeUm.Y / 2f <= maxY)
                    target[i] = value;
            }
        }

        #endregion

        #region Private: rendering helpers

        // Minimap path: origin=(contactLeft, viewTop), y-axis grows down, probe-y via (mmYMax - probeY).
        // Zoomed path:  origin=(viewLeft, viewBot),    y-axis grows up,   probe-y via (probeY - yOffset).
        static void DrawContour(ImDrawListPtr dl, Probe probe,
            float originX, float originY, float xRef, float yRef,
            float scale, float width,
            float yOffset = 0f, bool yUp = false)
        {
            if (probe?.ProbePlanarContour == null || probe.ProbePlanarContour.Length < 2) return;
            var pts = probe.ProbePlanarContour;
            for (int j = 0; j < pts.Length; j++)
            {
                var c0 = pts[j];
                var c1 = pts[(j + 1) % pts.Length];
                Vector2 p0, p1;
                if (yUp)
                {
                    p0 = new Vector2(originX + ((float)c0[0] - xRef) * scale, originY - ((float)c0[1] - yOffset) * scale);
                    p1 = new Vector2(originX + ((float)c1[0] - xRef) * scale, originY - ((float)c1[1] - yOffset) * scale);
                }
                else
                {
                    p0 = new Vector2(originX + ((float)c0[0] - xRef) * scale, originY + (yRef - (float)c0[1]) * scale);
                    p1 = new Vector2(originX + ((float)c1[0] - xRef) * scale, originY + (yRef - (float)c1[1]) * scale);
                }
                dl.AddLine(p0, p1, ColContour, width);
            }
        }

        static void DrawDepthRuler(ImDrawListPtr dl, float rightX, float viewTop, float viewBot,
            float yMinUm, float yMaxUm, float gridLeft = -1f, float gridRight = -1f,
            bool rightSide = false, float yLabelBaseUm = 0f)
        {
            float pxH = viewBot - viewTop;
            float rangeUm = yMaxUm - yMinUm;
            if (pxH <= 0 || rangeUm <= 0) return;

            float pxPerUm = pxH / rangeUm;
            float pxPerMm = pxPerUm * 1000f;
            bool hasGrid = gridLeft >= 0f && gridRight > gridLeft;

            dl.AddLine(new Vector2(rightX, viewTop), new Vector2(rightX, viewBot), ColRulerAxis);

            float[] majorCands = { 0.1f, 0.2f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f };
            float majorMm = majorCands[majorCands.Length - 1];
            foreach (var c in majorCands) { if (c * pxPerMm >= 60f) { majorMm = c; break; } }
            float majorUm = majorMm * 1000f;

            float minorMm = -1f;
            if (hasGrid)
            {
                float[] minorCands = { 0.01f, 0.02f, 0.05f, 0.1f, 0.2f, 0.5f };
                foreach (var c in minorCands)
                {
                    if (c >= majorMm) break;
                    if (c * pxPerMm >= 8f) { minorMm = c; break; }
                }
            }

            if (minorMm > 0f)
            {
                float minorUm = minorMm * 1000f;
                float first = (float)(Math.Ceiling((double)yMinUm / minorUm) * minorUm);
                for (float yUm = first; yUm <= yMaxUm + 1f; yUm += minorUm)
                {
                    float sy = viewBot - (yUm - yMinUm) * pxPerUm;
                    if (rightSide) dl.AddLine(new Vector2(rightX, sy), new Vector2(rightX + RulerMinorTickLen, sy), ColRulerAxis);
                    else           dl.AddLine(new Vector2(rightX - RulerMinorTickLen, sy), new Vector2(rightX, sy), ColRulerAxis);
                    if (hasGrid) DrawDottedHLine(dl, gridLeft, gridRight, sy, ColGridMinor);
                }
            }

            float firstMaj = (float)(Math.Ceiling((double)yMinUm / majorUm) * majorUm);
            for (float yUm = firstMaj; yUm <= yMaxUm + 1f; yUm += majorUm)
            {
                float sy = viewBot - (yUm - yMinUm) * pxPerUm;
                if (rightSide) dl.AddLine(new Vector2(rightX, sy), new Vector2(rightX + RulerMajorTickLen, sy), ColRulerMajorTick);
                else           dl.AddLine(new Vector2(rightX - RulerMajorTickLen, sy), new Vector2(rightX, sy), ColRulerMajorTick);
                if (hasGrid)
                    dl.AddLine(new Vector2(gridLeft, sy), new Vector2(gridRight, sy), ColGridMajor);
                string lbl = FormatMm((yUm - yLabelBaseUm) / 1000f, majorMm);
                var tsz = ImGui.CalcTextSize(lbl);
                float lblX = rightSide ? rightX + 8f : rightX - 8f - tsz.X;
                dl.AddText(new Vector2(lblX, sy - tsz.Y / 2f), ColRulerLabel, lbl);
            }
        }

        static void DrawGridLines(ImDrawListPtr dl,
            float lineLeft, float lineRight, float lineTop, float lineBot,
            float viewLeft, float viewBot, float xLow, float yLow, float scaleXY)
        {
            if (scaleXY <= 0) return;
            float pxPerMm = scaleXY * 1000f;

            float[] majorCands = { 0.1f, 0.2f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f };
            float majorMm = majorCands[majorCands.Length - 1];
            foreach (var c in majorCands) { if (c * pxPerMm >= 60f) { majorMm = c; break; } }
            float majorUm = majorMm * 1000f;

            float[] minorCands = { 0.01f, 0.02f, 0.05f, 0.1f, 0.2f, 0.5f };
            float minorMm = -1f;
            foreach (var c in minorCands)
            {
                if (c >= majorMm) break;
                if (c * pxPerMm >= 8f) { minorMm = c; break; }
            }

            // Y (horizontal) grid lines
            float yMinV = yLow + (viewBot - lineBot) / scaleXY;
            float yMaxV = yLow + (viewBot - lineTop) / scaleXY;

            if (minorMm > 0f)
            {
                float minorUm = minorMm * 1000f;
                float first = (float)(Math.Ceiling((double)yMinV / minorUm) * minorUm);
                for (float yUm = first; yUm <= yMaxV + 1f; yUm += minorUm)
                {
                    float sy = viewBot - (yUm - yLow) * scaleXY;
                    if (sy < lineTop - 1f || sy > lineBot + 1f) continue;
                    DrawDottedHLine(dl, lineLeft, lineRight, sy, ColGridMinor);
                }
            }
            {
                float first = (float)(Math.Ceiling((double)yMinV / majorUm) * majorUm);
                for (float yUm = first; yUm <= yMaxV + 1f; yUm += majorUm)
                {
                    float sy = viewBot - (yUm - yLow) * scaleXY;
                    if (sy < lineTop - 1f || sy > lineBot + 1f) continue;
                    dl.AddLine(new Vector2(lineLeft, sy), new Vector2(lineRight, sy), ColGridMajor);
                }
            }

            // X (vertical) grid lines
            float xMinV = xLow + (lineLeft  - viewLeft) / scaleXY;
            float xMaxV = xLow + (lineRight - viewLeft) / scaleXY;

            if (minorMm > 0f)
            {
                float minorUm = minorMm * 1000f;
                float first = (float)(Math.Ceiling((double)xMinV / minorUm) * minorUm);
                for (float xUm = first; xUm <= xMaxV + 1f; xUm += minorUm)
                {
                    float sx = viewLeft + (xUm - xLow) * scaleXY;
                    if (sx < lineLeft - 1f || sx > lineRight + 1f) continue;
                    DrawDottedVLine(dl, sx, lineTop, lineBot, ColGridMinor);
                }
            }
            {
                float first = (float)(Math.Ceiling((double)xMinV / majorUm) * majorUm);
                for (float xUm = first; xUm <= xMaxV + 1f; xUm += majorUm)
                {
                    float sx = viewLeft + (xUm - xLow) * scaleXY;
                    if (sx < lineLeft - 1f || sx > lineRight + 1f) continue;
                    dl.AddLine(new Vector2(sx, lineTop), new Vector2(sx, lineBot), ColGridMajor);
                }
            }
        }

        static void DrawGridLabels(ImDrawListPtr dl,
            float lblLeft, float lblRight, float lblTop, float lblBot,
            float lineLeft, float lineRight, float lineTop, float lineBot,
            float viewLeft, float viewBot, float xLow, float yLow, float scaleXY)
        {
            if (scaleXY <= 0) return;
            float pxPerMm = scaleXY * 1000f;

            float[] majorCands = { 0.1f, 0.2f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f };
            float majorMm = majorCands[majorCands.Length - 1];
            foreach (var c in majorCands) { if (c * pxPerMm >= 60f) { majorMm = c; break; } }
            float majorUm = majorMm * 1000f;

            // Y (horizontal) labels
            float yMinV = yLow + (viewBot - lineBot) / scaleXY;
            float yMaxV = yLow + (viewBot - lineTop) / scaleXY;
            {
                float first = (float)(Math.Ceiling((double)yMinV / majorUm) * majorUm);
                for (float yUm = first; yUm <= yMaxV + 1f; yUm += majorUm)
                {
                    float sy = viewBot - (yUm - yLow) * scaleXY;
                    if (sy < lineTop - 1f || sy > lineBot + 1f) continue;
                    string lbl = FormatMm(yUm / 1000f, majorMm);
                    var tsz = ImGui.CalcTextSize(lbl);
                    DrawGridLabel(dl, lbl, tsz, lblLeft  + 8f,         sy - tsz.Y / 2f);
                    DrawGridLabel(dl, lbl, tsz, lblRight - tsz.X - 8f, sy - tsz.Y / 2f);
                }
            }

            // X (vertical) labels
            float xMinV = xLow + (lineLeft  - viewLeft) / scaleXY;
            float xMaxV = xLow + (lineRight - viewLeft) / scaleXY;
            {
                float first = (float)(Math.Ceiling((double)xMinV / majorUm) * majorUm);
                for (float xUm = first; xUm <= xMaxV + 1f; xUm += majorUm)
                {
                    float sx = viewLeft + (xUm - xLow) * scaleXY;
                    if (sx < lineLeft - 1f || sx > lineRight + 1f) continue;
                    string lbl = FormatMm(xUm / 1000f, majorMm);
                    var tsz = ImGui.CalcTextSize(lbl);
                    DrawGridLabel(dl, lbl, tsz, sx - tsz.X / 2f, lineTop - tsz.Y - 5f);
                    DrawGridLabel(dl, lbl, tsz, sx - tsz.X / 2f, lineBot + 5f);
                }
            }
        }

        static void DrawContourFill(ImDrawListPtr dl, Probe probe,
            float viewLeft, float viewBot, float xLow, float yLow, float scaleXY)
        {
            if (probe?.ProbePlanarContour == null || probe.ProbePlanarContour.Length < 3) return;
            var pts = new Vector2[probe.ProbePlanarContour.Length];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new Vector2(
                    viewLeft + ((float)probe.ProbePlanarContour[i][0] - xLow) * scaleXY,
                    viewBot  - ((float)probe.ProbePlanarContour[i][1] - yLow) * scaleXY);
            FillPolygon(dl, pts, ColContourFill);
        }

        static void FillPolygon(ImDrawListPtr dl, Vector2[] pts, uint col)
        {
            if (pts.Length < 3) return;
            float area = 0f;
            for (int i = 0; i < pts.Length; i++)
            {
                var a = pts[i]; var b = pts[(i + 1) % pts.Length];
                area += a.X * b.Y - b.X * a.Y;
            }
            var idx = new List<int>(Enumerable.Range(0, pts.Length));
            while (idx.Count >= 3)
            {
                bool found = false;
                int n = idx.Count;
                for (int i = 0; i < n; i++)
                {
                    var a = pts[idx[(i - 1 + n) % n]];
                    var b = pts[idx[i]];
                    var c = pts[idx[(i + 1) % n]];
                    float cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
                    if (area * cross < 0f) continue;
                    bool ear = true;
                    for (int j = 0; j < n && ear; j++)
                    {
                        if (j == (i - 1 + n) % n || j == i || j == (i + 1) % n) continue;
                        var p = pts[idx[j]];
                        float d1 = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
                        float d2 = (c.X - b.X) * (p.Y - b.Y) - (c.Y - b.Y) * (p.X - b.X);
                        float d3 = (a.X - c.X) * (p.Y - c.Y) - (a.Y - c.Y) * (p.X - c.X);
                        if (!((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0))) ear = false;
                    }
                    if (ear) { dl.AddTriangleFilled(a, b, c, col); idx.RemoveAt(i); found = true; break; }
                }
                if (!found) break;
            }
        }

        static void DrawGridLabel(ImDrawListPtr dl, string lbl, Vector2 tsz, float x, float y)
        {
            dl.AddText(new Vector2(x, y), ColRulerLabel, lbl);
        }

        static string FormatMm(float mm, float intervalMm)
        {
            if (intervalMm >= 1f)   return $"{mm:0}mm";
            if (intervalMm >= 0.1f) return $"{mm:0.0}mm";
            return $"{mm:0.00}mm";
        }

        void DrawLegend(ImDrawListPtr dl, Vector2 cp, Vector2 avail)
        {
            if (Legend.Length == 0) return;

            const float pad = 5f;
            float lineH = ImGui.CalcTextSize("A").Y;
            float swatchSz = lineH;
            float rowH = lineH + pad;

            float maxEntryW = 0f;
            foreach (var e in Legend)
                maxEntryW = Math.Max(maxEntryW, swatchSz + pad + ImGui.CalcTextSize(e.Label).X);
            float boxW = maxEntryW + pad * 4f;
            float boxH = Legend.Length * rowH + pad * 3f;

            // Anchor inside the grid contact area (bottom-left), clear of axis-label margins and above the mode label
            float boxLeft = cp.X + GridMarginHPx + pad;
            float boxBottom = cp.Y + avail.Y - GridMarginVPx - pad;
            float boxTop = boxBottom - boxH;
            float boxRight  = boxLeft + boxW;

            dl.AddRectFilled(new Vector2(boxLeft, boxTop), new Vector2(boxRight, boxBottom), ColCoordOverlayBg, 3f);

            float x = boxLeft + pad * 2f;
            float y = boxTop + pad * 2f;

            foreach (var e in Legend)
            {
                var sMin = new Vector2(x, y);
                var sMax = new Vector2(x + swatchSz, y + swatchSz);
                if (e.OutlineOnly)
                {
                    dl.AddRect(sMin, sMax, e.Color, 0f, ContactSelBorderWidth);
                }
                else
                {
                    dl.AddRectFilled(sMin, sMax, e.Color);
                    if (e.DottedOutline)
                        DrawDottedRect(dl, sMin, sMax, ColBlockedOutline);
                }
                dl.AddText(new Vector2(x + swatchSz + pad, y), ColCoordOverlayText, e.Label);
                y += rowH;
            }
        }

        static void DrawDottedHLine(ImDrawListPtr dl, float x0, float x1, float y, uint col)
        {
            const float dash = 3f, gap = 3f;
            for (float x = x0; x < x1; x += dash + gap)
                dl.AddLine(new Vector2(x, y), new Vector2(x + dash < x1 ? x + dash : x1, y), col, 1f);
        }

        static void DrawDottedVLine(ImDrawListPtr dl, float x, float y0, float y1, uint col)
        {
            const float dash = 3f, gap = 3f;
            for (float y = y0; y < y1; y += dash + gap)
                dl.AddLine(new Vector2(x, y), new Vector2(x, y + dash < y1 ? y + dash : y1), col, 1f);
        }

        static void DrawDottedRect(ImDrawListPtr dl, Vector2 min, Vector2 max, uint col)
        {
            DrawDottedHLine(dl, min.X, max.X, min.Y, col);
            DrawDottedHLine(dl, min.X, max.X, max.Y, col);
            DrawDottedVLine(dl, min.X, min.Y, max.Y, col);
            DrawDottedVLine(dl, max.X, min.Y, max.Y, col);
        }

        #endregion
    }
}
