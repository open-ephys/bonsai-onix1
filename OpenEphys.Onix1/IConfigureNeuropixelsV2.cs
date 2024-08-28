namespace OpenEphys.Onix1
{
    public interface IConfigureNeuropixelsV2
    {
        public bool Enable { get; set; }
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationA { get; set; }
        public string GainCalibrationFileA { get; set; }
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationB { get; set; }
        public string GainCalibrationFileB { get; set; }
    }
}
