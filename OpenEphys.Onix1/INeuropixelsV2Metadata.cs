namespace OpenEphys.Onix1
{
    interface INeuropixelsV2Metadata
    {
        public string ProbePartNumber { get; }

        public ulong? ProbeSerialNumber { get; }

        public string FlexPartNumber { get; }

        public string FlexVersion { get; }
    }
}
