namespace OpenEphys.Onix
{
    public class NeuropixelsV1eAdc
    {
        public int CompP { get; set; } = 16;
        public int CompN { get; set; } = 16;
        public int Slope { get; set; } = 0;
        public int Coarse { get; set; } = 0;
        public int Fine { get; set; } = 0;
        public int Cfix { get; set; } = 0;
        public int Offset { get; set; } = 0;
        public int Threshold { get; set; } = 512;
    }
}
