using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Bonsai;

namespace OpenEphys.Onix
{
    public class NeuropixelsV1eSettings
    {
        // TODO: is this reallyt the best way to do this?
        internal readonly double ApGainCorrection;
        internal readonly double LfpGainCorrection;
        internal readonly NeuropixelsV1eAdc[] Adcs = new NeuropixelsV1eAdc[NeuropixelsV1e.AdcCount];

        const int ShankConfigurationBitCount = 968;
        const int BaseConfigurationBitCount = 2448;
        const int BaseConfigurationConfigOffset = 576;
        const int NumberOfGains = 8;
        const uint ShiftRegisterSuccess = 1 << 7;
        const int ShankBitExt1 = 965;
        const int ShankBitExt2 = 2;
        const int ShankBitTip1 = 484;
        const int ShankBitTip2 = 483;
        const int InternalReferenceChannel = 191;

        readonly BitArray ShankConfig = new(ShankConfigurationBitCount, false);
        readonly BitArray[] BaseConfigs = { new(BaseConfigurationBitCount, false),   // Ch 0, 2, 4, ...
                                            new(BaseConfigurationBitCount, false) }; // Ch 1, 3, 5, ...

        public enum ReferenceSource : byte
        {
            Ext = 0b001,
            Tip = 0b010
        }

        public enum Gain : byte
        {
            x50 = 0b000,
            x125 = 0b001,
            x250 = 0b010,
            x500 = 0b011,
            x1000 = 0b100,
            x1500 = 0b101,
            x2000 = 0b110,
            x3000 = 0b111
        }

        public static float GainToFloat(Gain gain) => gain switch
        {
            Gain.x50 => 50f,
            Gain.x125 => 125f,
            Gain.x250 => 250f,
            Gain.x500 => 500f,
            Gain.x1000 => 1000f,
            Gain.x1500 => 1500f,
            Gain.x2000 => 2000f,
            Gain.x3000 => 3000f,
            _ => throw new ArgumentOutOfRangeException(nameof(gain), $"Unexpected gain value: {gain}"),
        };

        // TODO: accept and apply channel config type
        public NeuropixelsV1eSettings(Gain apGain, Gain lfpGain, ReferenceSource refSource, bool apFilter, string gainCalibrationFile, string adcCalibrationFile)
        {

            if (gainCalibrationFile == null || adcCalibrationFile == null)
            {
                throw new ArgumentException("Calibraiton files must be specified.");
            }

            System.IO.StreamReader gainFile = new(gainCalibrationFile);
            var sn = ulong.Parse(gainFile.ReadLine());

            System.IO.StreamReader adcFile = new(adcCalibrationFile);
            if (sn != ulong.Parse(adcFile.ReadLine()))
                throw new ArgumentException("Calibraiton file serial numbers do not match.");

            // parse gain correction file
            var gainCorrections = gainFile.ReadLine().Split(',').Skip(1);

            if (gainCorrections.Count() != 2 * NumberOfGains)
                throw new ArgumentException("Incorrectly formmatted gain correction calibration file.");

            ApGainCorrection = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(Gain)), apGain)));
            LfpGainCorrection = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(Gain)), lfpGain) + 8));

            // parse ADC calibration file
            for (var i = 0; i < NeuropixelsV1e.AdcCount; i++)
            {
                var adcCal = adcFile.ReadLine().Split(',').Skip(1);
                if (adcCal.Count() != NumberOfGains)
                {
                    throw new ArgumentException("Incorrectly formmatted ADC calibration file.");
                }

                Adcs[i] = new NeuropixelsV1eAdc
                {
                    CompP = int.Parse(adcCal.ElementAt(0)),
                    CompN = int.Parse(adcCal.ElementAt(1)),
                    Slope = int.Parse(adcCal.ElementAt(2)),
                    Coarse = int.Parse(adcCal.ElementAt(3)),
                    Fine = int.Parse(adcCal.ElementAt(4)),
                    Cfix = int.Parse(adcCal.ElementAt(5)),
                    Offset = int.Parse(adcCal.ElementAt(6)),
                    Threshold = int.Parse(adcCal.ElementAt(7))
                };
            }

            switch (refSource)
            {
                case ReferenceSource.Ext:
                    {
                        ShankConfig[ShankBitExt1] = true;
                        ShankConfig[ShankBitExt2] = true;
                        break;
                    }
                case ReferenceSource.Tip:
                    {
                        ShankConfig[ShankBitTip1] = true;
                        ShankConfig[ShankBitTip2] = true;
                        break;
                    }
            }

            // Update active channels
            for (int i = 0; i < NeuropixelsV1e.ChannelCount; i++)
            {
                // Reference bits always remain zero
                if (i == InternalReferenceChannel)
                {
                    continue;
                }

                var e = i; // TODO: Electrode map

                int bitIndex = e % 2 == 0 ?
                    485 + ((int)e / 2) : // even electrode
                    482 - ((int)e / 2);  // odd electrode
                ShankConfig[bitIndex] = true;
            }

            // create base shift-register bit arrays
            for (int i = 0; i < NeuropixelsV1e.ChannelCount; i++)
            {
                var configIdx = i % 2;

                // References
                var refIdx = configIdx == 0 ?
                    (382 - i) / 2 * 3 :
                    (383 - i) / 2 * 3;

                BaseConfigs[configIdx][refIdx + 0] = ((byte)refSource >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][refIdx + 1] = ((byte)refSource >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][refIdx + 2] = ((byte)refSource >> 2 & 0x1) == 1;

                var chanOptsIdx = BaseConfigurationConfigOffset + ((i - configIdx) * 4);

                // MSB [Full, standby, LFPGain(3 downto 0), APGain(3 downto0)] LSB

                BaseConfigs[configIdx][chanOptsIdx + 0] = ((byte)apGain >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 1] = ((byte)apGain >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 2] = ((byte)apGain >> 2 & 0x1) == 1;

                BaseConfigs[configIdx][chanOptsIdx + 3] = ((byte)lfpGain >> 0 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 4] = ((byte)lfpGain >> 1 & 0x1) == 1;
                BaseConfigs[configIdx][chanOptsIdx + 5] = ((byte)lfpGain >> 2 & 0x1) == 1;

                BaseConfigs[configIdx][chanOptsIdx + 6] = false;
                BaseConfigs[configIdx][chanOptsIdx + 7] = !apFilter; // Full bandwidth = 1, filter on = 0

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
                var coarse = (new BitArray(new byte[] { (byte)adc.Coarse }));
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
        }

        // TODO: There is an issue getting these SR write sequences to complete correctly.
        // We have a suspicion it is due to the nature of the MCLK signal and that this
        // headstage needs either a different oscillator with even more drive strength or
        // a clock buffer (second might be easiest).
        internal void WriteShiftRegisters(I2CRegisterContext i2cNpx)
        {
            // shank
            // NB: no read check because of ASIC bug
            var shankBytes = BitArrayToBytes(ShankConfig);

            i2cNpx.WriteByte(NeuropixelsV1e.SR_LENGTH1, (uint)shankBytes.Length % 0x100);
            i2cNpx.WriteByte(NeuropixelsV1e.SR_LENGTH2, (uint)shankBytes.Length / 0x100);

            foreach (var b in shankBytes)
            {
                i2cNpx.WriteByte(NeuropixelsV1e.SR_CHAIN1, b);
            }

            // base
            for (int i = 0; i < BaseConfigs.Length; i++)
            {
                var srAddress = i == 0 ? NeuropixelsV1e.SR_CHAIN2 : NeuropixelsV1e.SR_CHAIN3;

                for (int j = 0; j < 2; j++)
                {
                    // TODO: HACK HACK HACK
                    // If we do not do this, the ShiftRegisterSuccess check below will always fail
                    // on whatever the second shift register write sequnece regardless of order or
                    // contents. Could be increased current draw during internal process causes MCLK
                    // to droop and mess up internal state. Or that MCLK is just not good enough to
                    // prevent metastability in some logic in the ASIC that is only entered in between
                    // SR accesses.
                    i2cNpx.WriteByte(NeuropixelsV1e.SOFT_RESET, 0xFF);
                    i2cNpx.WriteByte(NeuropixelsV1e.SOFT_RESET, 0x00);

                    var baseBytes = BitArrayToBytes(BaseConfigs[i]);

                    i2cNpx.WriteByte(NeuropixelsV1e.SR_LENGTH1, (uint)baseBytes.Length % 0x100);
                    i2cNpx.WriteByte(NeuropixelsV1e.SR_LENGTH2, (uint)baseBytes.Length / 0x100);

                    foreach (var b in baseBytes)
                    {
                        i2cNpx.WriteByte(srAddress, b);
                    }
                }

                if (i2cNpx.ReadByte(NeuropixelsV1e.STATUS) != ShiftRegisterSuccess)
                {
                    throw new WorkflowException($"Shift register {srAddress} status check failed.");
                }
            }
        }

        // Bits go into the shift registers MSB first
        // This creates a *bit-reversed* byte array from a bit array
        private static byte[] BitArrayToBytes(BitArray bits)
        {
            if (bits.Length == 0)
            {
                throw new ArgumentException("Shift register data is empty", nameof(bits));
            }

            var bytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                // NB: http://graphics.stanford.edu/~seander/bithacks.html
                bytes[i] = (byte)((bytes[i] * 0x0202020202ul & 0x010884422010ul) % 1023);
            }

            return bytes;
        }
    }
}
