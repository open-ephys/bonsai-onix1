namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// House color swatches to be layered on top of a default Dear ImGui theme. Colors are defined Dear
    /// ImGui's native packed ABGR <c>uint</c> format (<c>0xAABBGGRR</c>). Every entry here is a pure color
    /// identity (what the color *is*) with no knowledge of what it's used for. Consuming files should assign
    /// their own descriptive, purpose-named constant to one of these rather than referencing this class
    /// directly at every call site.
    /// </summary>
    internal static class ImGuiPalette
    {
        // Flat
        public const uint Black  = 0xFF000000u;
        public const uint White  = 0xFFFFFFFFu;
        public const uint Red    = 0xFF0000FFu;
        public const uint Green  = 0xFF00FF00u;
        public const uint Blue   = 0xFFFF0000u;
        public const uint Yellow = 0xFF00FFFFu;

        // Vibrant
        public const uint AzureBlue    = 0xFFFF8800u;
        public const uint VibrantCoral = 0xFF5E59FFu;
        public const uint GoldenPollen = 0xFF3ACAFFu;
        public const uint YellowGreen  = 0xFF26C98Au;
        public const uint SteelBlue    = 0xFFC48219u;
        public const uint DustyGrape   = 0xFF934C6Au;

        // Bright
        public const uint Marigold     = 0xFF00CCFFu;
        public const uint MintGreen    = 0xFF66FF66u;

        // Greyscale ramp, darkest to lightest.
        public const uint Grey0x30 = 0xFF303030u;
        public const uint Grey0x4D = 0xFF4D4D4Du;
        public const uint Grey0x55 = 0xFF555555u;
        public const uint Grey0x77 = 0xFF777777u;
        public const uint Grey0x80 = 0xFF808080u;
        public const uint Grey0x88 = 0xFF888888u;
        public const uint Grey0x99 = 0xFF999999u;
        public const uint Grey0xCC = 0xFFCCCCCCu;

        /// <summary>
        /// Returns <paramref name="color"/> with its alpha byte replaced.
        /// </summary>
        public static uint WithAlpha(uint color, byte alpha) =>
            (color & 0x00FFFFFFu) | ((uint)alpha << 24);
    }
}
