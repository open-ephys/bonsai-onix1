namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that contains ADC calibration values for a NeuropixelsV1e device.
    /// </summary>
    public class NeuropixelsV1eAdc
    {
        /// <summary>
        /// 
        /// </summary>
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
