using System;
using System.Collections;
using System.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Nric1384Settings
    {
        readonly double ApGainCorrection;
        readonly double LfpGainCorrection;
        readonly Nric1384Adc[] Adcs = new Nric1384Adc[Nric1384.AdcCount];

        const int BaseConfigurationBitCount = 2448;
        const int BaseConfigurationConfigOffset = 576;
        const int NumberOfGains = 8;

        readonly BitArray[] BaseConfigs = { new BitArray(BaseConfigurationBitCount, false),   // Ch 0, 2, 4, ...
                                            new BitArray(BaseConfigurationBitCount, false) }; // Ch 1, 3, 5, ...

        public enum ReferenceSource : byte
        {
            All = 0b001,
            Par = 0b010
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

        public Nric1384Settings(Gain apGain, Gain lfpGain, ReferenceSource refSource, bool apFilter, string gainCalibrationFile, string adcCalibrationFile)
        {

            if (gainCalibrationFile == null || adcCalibrationFile == null)
            {
                throw new ArgumentException("Calibraiton files must be specified.");
            }

            System.IO.StreamReader gainFile = new(gainCalibrationFile);
            var sn = UInt64.Parse(gainFile.ReadLine());

            System.IO.StreamReader adcFile = new(adcCalibrationFile);
            if (sn != UInt64.Parse(adcFile.ReadLine()))
                throw new ArgumentException("Calibraiton file serial numbers do not match.");

            // parse gain correction file
            var gainCorrections = gainFile.ReadLine().Split(',').Skip(1);

            if (gainCorrections.Count() != 2 * NumberOfGains)
                throw new ArgumentException("Incorrectly formmatted gain correction calibration file.");

            ApGainCorrection = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(Gain)), apGain)));
            LfpGainCorrection = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(Gain)), lfpGain) + 8));

            // parse ADC calibration file
            for (var i = 0; i < Nric1384.AdcCount; i++)
            {
                var adcCal = adcFile.ReadLine().Split(',').Skip(1);
                if (adcCal.Count() != NumberOfGains)
                {
                    throw new ArgumentException("Incorrectly formmatted ADC calibration file.");
                }

                Adcs[i] = new Nric1384Adc
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

            // create shift-register bit arrays
            for (int i = 0; i < Nric1384.ChannelCount; i++)
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

        public void WriteShiftRegisters(DeviceContext device)
        {

            var i2cNric1384 = new I2CRegisterContext(device, Nric1384.I2cAddress);

            // Even channels
            var bytes = BitArrayToBytes(BaseConfigs[0]);

            i2cNric1384.WriteByte(Nric1384.SR_LENGTH1, (uint)bytes.Length % 0x100);
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH2, (uint)bytes.Length / 0x100);

            // First time to program
            foreach (var b in bytes)
            {
                i2cNric1384.WriteByte(Nric1384.SR_CHAIN2, b);
            }

            // Second time for parity check
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH1, (uint)bytes.Length % 0x100);
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH2, (uint)bytes.Length / 0x100);

            foreach (var b in bytes)
            {
                i2cNric1384.WriteByte(Nric1384.SR_CHAIN2, b);
            }

            if (i2cNric1384.ReadByte(Nric1384.STATUS) != 1 << 7)
            {
                throw new WorkflowException("Shift register programming check failed.");
            }

            // Odd channels
            bytes = BitArrayToBytes(BaseConfigs[1]);

            i2cNric1384.WriteByte(Nric1384.SR_LENGTH1, (uint)bytes.Length % 0x100);
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH2, (uint)bytes.Length / 0x100);

            // First time to program
            foreach (var b in bytes)
            {
                i2cNric1384.WriteByte(Nric1384.SR_CHAIN3, b);
            }

            // Second time for parity check
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH1, (uint)bytes.Length % 0x100);
            i2cNric1384.WriteByte(Nric1384.SR_LENGTH2, (uint)bytes.Length / 0x100);

            foreach (var b in bytes)
            {
                i2cNric1384.WriteByte(Nric1384.SR_CHAIN3, b);
            }

            if (i2cNric1384.ReadByte(Nric1384.STATUS) != 1 << 7)
            {
                throw new WorkflowException("Shift register programming check failed.");
            }

            // Adc correction parameters
            for (uint i = 0; i < Adcs.Length; i++)
            {
                device.WriteRegister(Nric1384.ADC00_OFF_THRESH + i, (uint)(Adcs[i].Offset << 10 | Adcs[i].Threshold));
            }

            device.WriteRegister(Nric1384.LFP_GAIN, (uint)(LfpGainCorrection * (1 << 14)));
            device.WriteRegister(Nric1384.AP_GAIN, (uint)(ApGainCorrection * (1 << 14)));

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
