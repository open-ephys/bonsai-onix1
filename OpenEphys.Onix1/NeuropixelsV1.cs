using System;
using System.Collections;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the probe as A or B.
    /// </summary>
    public enum NeuropixelsV1Probe
    {
        /// <summary>
        /// Specifies that this is Probe A.
        /// </summary>
        ProbeA = 0,
        /// <summary>
        /// Specifies that this is Probe B.
        /// </summary>
        ProbeB = 1
    }

    // Probe constants
    static class NeuropixelsV1
    {
        public const int ProbeI2CAddress = 0x70;
        public const int FlexEepromI2CAddress = 0x50;

        public const int FramesPerSuperFrame = 13;
        public const int FramesPerRoundRobin = 12;
        public const int AdcCount = 32;
        public const int ChannelCount = 384;
        public const int ElectrodeCount = 960;
        public const int FrameWords = 40;

        internal static BitArray MakeShankBits(NeuropixelsV1ProbeConfiguration configuration)
        {
            const int ShankConfigurationBitCount = 968;
            const int ShankBitExt1 = 965;
            const int ShankBitExt2 = 2;
            const int ShankBitTip1 = 484;
            const int ShankBitTip2 = 483;
            const int InternalReferenceChannel = 191;

            var shankBits = new BitArray(ShankConfigurationBitCount);

            foreach (var e in configuration.ChannelMap)
            {
                if (e.Index == InternalReferenceChannel) continue;

                int bitIndex = e.Index % 2 == 0 ?
                        485 + (e.Index / 2) : // even electrode
                        482 - (e.Index / 2);  // odd electrode

                shankBits[bitIndex] = true;
            }

            switch (configuration.Reference)
            {
                case NeuropixelsV1ReferenceSource.External:
                    {
                        shankBits[ShankBitExt1] = true;
                        shankBits[ShankBitExt2] = true;
                        break;
                    }
                case NeuropixelsV1ReferenceSource.Tip:
                    {
                        shankBits[ShankBitTip1] = true;
                        shankBits[ShankBitTip2] = true;
                        break;
                    }
            }

            return shankBits;
        }

        internal static BitArray[] MakeConfigBits(NeuropixelsV1ProbeConfiguration probeConfiguration, NeuropixelsV1Adc[] Adcs)
        {
            const int BaseConfigurationBitCount = 2448;
            const int BaseConfigurationConfigOffset = 576;

            BitArray[] BaseConfigs = { new(BaseConfigurationBitCount, false),   // Ch 0, 2, 4, ...
                                       new(BaseConfigurationBitCount, false) }; // Ch 1, 3, 5, ...

            // create base shift-register bit arrays
            for (int i = 0; i < NeuropixelsV1.ChannelCount; i++)
            {
                var configIdx = i % 2;

                // References
                var refIdx = configIdx == 0 ?
                    (382 - i) / 2 * 3 :
                    (383 - i) / 2 * 3;

                BaseConfigs[configIdx][refIdx + 0] = ((byte)probeConfiguration.Reference >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][refIdx + 1] = ((byte)probeConfiguration.Reference >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][refIdx + 2] = ((byte)probeConfiguration.Reference >> 2 & 0x1) == 1;

                var chanOptsIdx = BaseConfigurationConfigOffset + ((i - configIdx) * 4);

                // MSB [Full, standby, LFPGain(3 downto 0), APGain(3 downto 0)] LSB

                BaseConfigs[configIdx][chanOptsIdx + 0] = ((byte)probeConfiguration.SpikeAmplifierGain >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 1] = ((byte)probeConfiguration.SpikeAmplifierGain >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 2] = ((byte)probeConfiguration.SpikeAmplifierGain >> 2 & 0x1) == 1;

                BaseConfigs[configIdx][chanOptsIdx + 3] = ((byte)probeConfiguration.LfpAmplifierGain >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 4] = ((byte)probeConfiguration.LfpAmplifierGain >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 5] = ((byte)probeConfiguration.LfpAmplifierGain >> 2 & 0x1) == 1;

                BaseConfigs[configIdx][chanOptsIdx + 6] = false;
                BaseConfigs[configIdx][chanOptsIdx + 7] = !probeConfiguration.SpikeFilter; ; // Full bandwidth = 1, filter on = 0
            }

            int k = 0;
            foreach (var adc in Adcs)
            {
                if (adc.CompP < 0 || adc.CompP > 0x1F)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter CompP value of {adc.CompP} is invalid.");
                }

                if (adc.CompN < 0 || adc.CompN > 0x1F)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter CompN value of {adc.CompN} is invalid.");
                }

                if (adc.Cfix < 0 || adc.Cfix > 0xF)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter Cfix value of {adc.Cfix} is invalid.");
                }

                if (adc.Slope < 0 || adc.Slope > 0x7)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter Slope value of {adc.Slope} is invalid.");
                }

                if (adc.Coarse < 0 || adc.Coarse > 0x3)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter Coarse value of {adc.Coarse} is invalid.");
                }

                if (adc.Fine < 0 || adc.Fine > 0x3)
                {
                    throw new ArgumentOutOfRangeException($"ADC calibration parameter Fine value of {adc.Fine} is invalid.");
                }

                var configIdx = k % 2;
                int d = k++ / 2;

                int compOffset = 2406 - 42 * (d / 2) + (d % 2) * 10;
                int slopeOffset = compOffset + 20 + (d % 2);

                var compP = new BitArray(new byte[] { (byte)adc.CompP });
                var compN = new BitArray(new byte[] { (byte)adc.CompN });
                var cfix = new BitArray(new byte[] { (byte)adc.Cfix });
                var slope = new BitArray(new byte[] { (byte)adc.Slope });
                var coarse = new BitArray(new byte[] { (byte)adc.Coarse });
                var fine = new BitArray(new byte[] { (byte)adc.Fine });

                BaseConfigs[configIdx][compOffset + 0] = compP[0];
                BaseConfigs[configIdx][compOffset + 1] = compP[1];
                BaseConfigs[configIdx][compOffset + 2] = compP[2];
                BaseConfigs[configIdx][compOffset + 3] = compP[3];
                BaseConfigs[configIdx][compOffset + 4] = compP[4];

                BaseConfigs[configIdx][compOffset + 5] = compN[0];
                BaseConfigs[configIdx][compOffset + 6] = compN[1];
                BaseConfigs[configIdx][compOffset + 7] = compN[2];
                BaseConfigs[configIdx][compOffset + 8] = compN[3];
                BaseConfigs[configIdx][compOffset + 9] = compN[4];

                BaseConfigs[configIdx][slopeOffset + 0] = slope[0];
                BaseConfigs[configIdx][slopeOffset + 1] = slope[1];
                BaseConfigs[configIdx][slopeOffset + 2] = slope[2];

                BaseConfigs[configIdx][slopeOffset + 3] = fine[0];
                BaseConfigs[configIdx][slopeOffset + 4] = fine[1];

                BaseConfigs[configIdx][slopeOffset + 5] = coarse[0];
                BaseConfigs[configIdx][slopeOffset + 6] = coarse[1];

                BaseConfigs[configIdx][slopeOffset + 7] = cfix[0];
                BaseConfigs[configIdx][slopeOffset + 8] = cfix[1];
                BaseConfigs[configIdx][slopeOffset + 9] = cfix[2];
                BaseConfigs[configIdx][slopeOffset + 10] = cfix[3];
            }

            return BaseConfigs;
        }
    }
}
