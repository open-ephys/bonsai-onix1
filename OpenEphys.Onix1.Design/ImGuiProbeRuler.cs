using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenEphys.Onix1.Design
{
    internal class ImGuiProbeRuler
    {
        readonly record struct RulerLine(Vector2 Start, Vector2 End);
        enum PlacementState { None, Dragging, ClickMode }

        readonly List<RulerLine> locked = new();
        PlacementState placement = PlacementState.None;
        Vector2 draftStartUm;
        Vector2 draftStartScreen;
        Vector2 draftEndUm;
        int hoveredIdx = -1;

        readonly float majorTickSpacingUm;
        readonly int   minorTicksPerMajor;

        public string Units { get; set; } = "um";

        public ImGuiProbeRuler(float majorTickSpacingUm = 100f, int minorTicksPerMajor = 5)
        {
            this.majorTickSpacingUm = majorTickSpacingUm;
            this.minorTicksPerMajor = minorTicksPerMajor;
        }

        // visual constants
        const uint ColorLine = ImGuiPalette.AzureBlue;
        const uint ColorHovered = ImGuiPalette.Yellow;
        const uint ColorLabel = ImGuiPalette.White;
        static readonly uint ColorLabelBg = ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0xBB);
        const uint ColorTickLabel = ImGuiPalette.Grey0xCC;
        const float LineWidth = 1.5f;
        const float LabelPad = 3f;
        const float HoverDist = 15f;
        const float MajorTickHalfLen = 8f;
        const float MinorTickHalfLen = 4f;

        /// <summary>
        /// Call each frame while ruler mode is active. Does NOT signal exit —
        /// ruler mode is exited only by pressing r again.
        /// </summary>
        public void HandleRulerModeInput(Vector2 mouseProbeCoord, bool inCanvas,
            Vector2 mouseScreenPos, float viewLeft, float viewBot,
            float xLow, float yLow, float scaleXY)
        {
            // Hover: only scan when not actively placing
            if (placement == PlacementState.None)
            {
                hoveredIdx = -1;
                for (int i = 0; i < locked.Count; i++)
                {
                    var s = ToScreen(locked[i].Start, viewLeft, viewBot, xLow, yLow, scaleXY);
                    var e = ToScreen(locked[i].End, viewLeft, viewBot, xLow, yLow, scaleXY);
                    if (PointToSegmentDist(mouseScreenPos, s, e) <= HoverDist)
                    { hoveredIdx = i; break; }
                }
            }

            // Delete hovered ruler (only when not placing)
            if (placement == PlacementState.None && hoveredIdx >= 0 &&
                ImGui.IsKeyPressed(ImGuiKey.Delete, false))
            {
                locked.RemoveAt(hoveredIdx);
                hoveredIdx = -1;
                return;
            }

            // Start placement
            if (placement == PlacementState.None &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left) && inCanvas)
            {
                draftStartUm     = mouseProbeCoord;
                draftEndUm       = mouseProbeCoord;
                draftStartScreen = mouseScreenPos;
                placement        = PlacementState.Dragging;
            }

            // Track end point every frame while placing
            if (placement != PlacementState.None)
                draftEndUm = mouseProbeCoord;

            // Drag release
            if (placement == PlacementState.Dragging &&
                ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                var delta = mouseScreenPos - draftStartScreen;
                if (delta.LengthSquared() >= 16f)
                {
                    LockDraft();
                    placement = PlacementState.None;
                }
                else
                    placement = PlacementState.ClickMode;
            }

            // Click-click mode: second click locks
            if (placement == PlacementState.ClickMode &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                LockDraft();
                placement = PlacementState.None;
            }
        }

        public void ResetHover() => hoveredIdx = -1;

        public void Draw(ImDrawListPtr dl, float viewLeft, float viewBot,
            float xLow, float yLow, float scaleXY, bool rulerMode)
        {
            for (int i = 0; i < locked.Count; i++)
                DrawSingleRuler(dl, locked[i].Start, locked[i].End,
                    viewLeft, viewBot, xLow, yLow, scaleXY, i == hoveredIdx);

            if (placement != PlacementState.None)
                DrawSingleRuler(dl, draftStartUm, draftEndUm,
                    viewLeft, viewBot, xLow, yLow, scaleXY, false);
        }

        void LockDraft()
        {
            if ((draftEndUm - draftStartUm).LengthSquared() > 0f)
                locked.Add(new RulerLine(draftStartUm, draftEndUm));
        }

        void DrawSingleRuler(ImDrawListPtr dl, Vector2 startUm, Vector2 endUm,
            float viewLeft, float viewBot, float xLow, float yLow, float scaleXY, bool isHovered)
        {
            var s = ToScreen(startUm, viewLeft, viewBot, xLow, yLow, scaleXY);
            var e = ToScreen(endUm,   viewLeft, viewBot, xLow, yLow, scaleXY);

            uint lineCol = isHovered ? ColorHovered : ColorLine;
            dl.AddLine(s, e, lineCol, LineWidth);

            var dir = e - s;
            float lenPx = dir.Length();

            if (lenPx >= 0.5f)
            {
                var dirUnit = dir / lenPx;
                // perp points 90° CCW from ruler direction in screen space.
                // For an upward ruler (screen dir (0,-1)): perp = (1,0) = screen right.
                // The "left" side of the ruler = -perp.
                var perp = new Vector2(-dirUnit.Y, dirUnit.X);

                float tickSpacingPx = majorTickSpacingUm * scaleXY;
                int   minorN        = Math.Max(1, minorTicksPerMajor);
                // If ticks too dense, suppress intermediates (only start + endpoint)
                float minorSpacingPx = tickSpacingPx >= 0.5f
                    ? tickSpacingPx / minorN
                    : lenPx * 2f;

                // Draw ticks from start up to but not including the endpoint
                int totalMinors = (int)(lenPx / minorSpacingPx);
                for (int k = 0; k <= totalMinors; k++)
                {
                    float pos = k * minorSpacingPx;
                    if (pos >= lenPx - 0.5f) break;

                    bool  isMajor = k % minorN == 0;
                    float halfLen = isMajor ? MajorTickHalfLen : MinorTickHalfLen;
                    var   center  = s + dirUnit * pos;
                    dl.AddLine(center - perp * halfLen, center + perp * halfLen, lineCol, LineWidth);

                    if (isMajor)
                        DrawTickLabel(dl, center, $"{pos / scaleXY:0}", perp, halfLen);
                }

                // Always end with a major tick at the exact endpoint (no label — redundant with summary)
                dl.AddLine(e - perp * MajorTickHalfLen, e + perp * MajorTickHalfLen, lineCol, LineWidth);
            }

            // Floating summary label at end
            float dX  = endUm.X - startUm.X;
            float dY  = endUm.Y - startUm.Y;
            float r   = (float)Math.Sqrt(dX * dX + dY * dY);
            float deg = (float)(Math.Atan2(dY, dX) * 180.0 / Math.PI);

            string line1 = $"x: {dX:0} {Units}";
            string line2 = $"y: {dY:0} {Units}";
            string line3 = $"r: {r:0} {Units}";
            string line4 = $"deg: {deg:0}";

            float lineH = ImGui.CalcTextSize(line1).Y;
            float lblW  = Math.Max(Math.Max(Math.Max(
                ImGui.CalcTextSize(line1).X,
                ImGui.CalcTextSize(line2).X),
                ImGui.CalcTextSize(line3).X),
                ImGui.CalcTextSize(line4).X);
            float lblH = lineH * 4f;
            float lx   = e.X + 8f;
            float ly   = e.Y - lblH / 2f;

            dl.AddRectFilled(
                new Vector2(lx - LabelPad, ly - LabelPad),
                new Vector2(lx + lblW + LabelPad, ly + lblH + LabelPad),
                ColorLabelBg, 3f);
            dl.AddText(new Vector2(lx, ly),              ColorLabel, line1);
            dl.AddText(new Vector2(lx, ly + lineH),      ColorLabel, line2);
            dl.AddText(new Vector2(lx, ly + lineH * 2f), ColorLabel, line3);
            dl.AddText(new Vector2(lx, ly + lineH * 3f), ColorLabel, line4);
        }

        // Label is placed on the -perp (left) side of the ruler, horizontal.
        // Offset is computed so the text clears the tick end by 2 px regardless of ruler angle.
        static void DrawTickLabel(ImDrawListPtr dl, Vector2 tickCenter, string text,
            Vector2 perp, float halfLen)
        {
            var   tsz      = ImGui.CalcTextSize(text);
            var   leftDir  = new Vector2(-perp.X, -perp.Y);
            float halfExt  = (tsz.X / 2f) * Math.Abs(leftDir.X)
                           + (tsz.Y / 2f) * Math.Abs(leftDir.Y);
            var   center   = tickCenter + leftDir * (halfLen + 2f + halfExt);
            var   textPos  = center - new Vector2(tsz.X / 2f, tsz.Y / 2f);
            const float pad = 1f;
            dl.AddRectFilled(textPos - new Vector2(pad, pad),
                             textPos + tsz + new Vector2(pad, pad), ImGuiPalette.WithAlpha(ImGuiPalette.Black, 0x88), 2f);
            dl.AddText(textPos, ColorTickLabel, text);
        }

        static float PointToSegmentDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float lenSq = ab.X * ab.X + ab.Y * ab.Y;
            if (lenSq < 0.0001f) return (p - a).Length();
            float t = Math.Max(0f, Math.Min(1f,
                ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / lenSq));
            return new Vector2(p.X - (a.X + t * ab.X), p.Y - (a.Y + t * ab.Y)).Length();
        }

        static Vector2 ToScreen(Vector2 probeUm, float viewLeft, float viewBot,
            float xLow, float yLow, float scaleXY)
        {
            return new Vector2(
                viewLeft + (probeUm.X - xLow) * scaleXY,
                viewBot  - (probeUm.Y - yLow) * scaleXY);
        }
    }
}
