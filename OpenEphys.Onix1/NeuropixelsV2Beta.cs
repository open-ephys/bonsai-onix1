
namespace OpenEphys.Onix1
{
    static class NeuropixelsV2Beta
    {
        public const int ProbeAddress = 0x70;
        public const int FlexEEPROMAddress = 0x50;

        public const int ChannelCount = 384;
        public const int AdcBits = 14;
        public const int AdcMidpoint = 1 << (AdcBits - 1);
        public const int BaseBitsPerChannel = 4;
        public const int ElectrodePerShank = 1280;

        public const int FramesPerSuperFrame = 16;
        public const int AdcsPerProbe = 24;
        public const int SyncsPerFrame = 2;
        public const int CountersPerFrame = 2;
        public const int FrameWords = 28;

        // register map
        public const int OP_MODE = 0x00;
        public const int REC_MODE = 0x01;
        public const int CAL_MODE = 0x02;
        public const int ADC_CONFIG = 0x03;
        public const int TEST_CONFIG1 = 0x04;
        public const int TEST_CONFIG2 = 0x05;
        public const int TEST_CONFIG3 = 0x06;
        public const int TEST_CONFIG4 = 0x07;
        public const int TEST_CONFIG5 = 0x08;
        public const int STATUS = 0x09;
        public const int SYNC2 = 0x0A;
        public const int SYNC1 = 0x0B;
        public const int SR_CHAIN6 = 0x0C; // Odd channel base config
        public const int SR_CHAIN5 = 0x0D; // Even channel base config
        public const int SR_CHAIN4 = 0x0E; // Shank 4
        public const int SR_CHAIN3 = 0x0F; // Shank 3
        public const int SR_CHAIN2 = 0x10; // Shank 2
        public const int SR_CHAIN1 = 0x11; // Shank 1
        public const int SR_LENGTH2 = 0x12;
        public const int SR_LENGTH1 = 0x13;
        public const int PROBE_ID = 0x14;
        public const int SOFT_RESET = 0x15;

        internal static int[][] AdcChannelGroups()
        {
            const int channelsPerAdc = ChannelCount / AdcsPerProbe;
            var groups = new int[AdcsPerProbe][];
            for (int a = 0; a < AdcsPerProbe; a++)
            {
                int block = a / 2, parity = a % 2;
                groups[a] = new int[channelsPerAdc];
                for (int i = 0; i < channelsPerAdc; i++)
                    groups[a][i] = block * 32 + parity + i * 2;
            }

            return groups;
        }

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2Beta))
            {
            }
        }
    }
}
