namespace OpenEphys.Onix1.Design
{
    internal class Rhs2116ProbeConfiguration : IProbeInterfaceConfiguration
    {
        public string ProbeInterfaceFileName { get; set; }

        public Rhs2116ProbeConfiguration(string probeInterfaceFileName)
        {
            ProbeInterfaceFileName = probeInterfaceFileName;
        }
    }
}
