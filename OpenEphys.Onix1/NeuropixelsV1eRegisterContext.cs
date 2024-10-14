using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1eRegisterContext : I2CRegisterContext
    {
        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }

        const uint ShiftRegisterSuccess = 1 << 7;

        readonly NeuropixelsV1Adc[] Adcs = new NeuropixelsV1Adc[NeuropixelsV1.AdcCount];
        readonly BitArray ShankConfig; 
        readonly BitArray[] BaseConfigs;

        public NeuropixelsV1eRegisterContext(DeviceContext deviceContext, uint i2cAddress, ulong probeSerialNumber,
            NeuropixelsV1ProbeConfiguration probeConfiguration, string gainCalibrationFile, string adcCalibrationFile)
            : base(deviceContext, i2cAddress)
        {
            if (!File.Exists(gainCalibrationFile))
            {
                throw new ArgumentException($"A gain calibration file must be specified for the probe with serial number " +
                    $"{probeSerialNumber}");
            }
            
            if (!File.Exists(adcCalibrationFile))
            {
                throw new ArgumentException($"An ADC calibration file must be specified for the probe with serial number " +
                    $"{probeSerialNumber}");
            }

            var adcCalibration = NeuropixelsV1Helper.TryParseAdcCalibrationFile(adcCalibrationFile);

            if (!adcCalibration.HasValue)
            {
                throw new ArgumentException($"The calibration file \"{adcCalibrationFile}\" is invalid.");
            }

            if (adcCalibration.Value.SerialNumber != probeSerialNumber)
            {
                throw new ArgumentException($"The probe serial number ({probeSerialNumber}) does not " +
                    $"match the ADC calibration file serial number ({adcCalibration.Value.SerialNumber}).");
            }

            var gainCorrection = NeuropixelsV1Helper.TryParseGainCalibrationFile(gainCalibrationFile, 
                probeConfiguration.SpikeAmplifierGain, probeConfiguration.LfpAmplifierGain, NeuropixelsV1.ElectrodeCount);

            if (!gainCorrection.HasValue)
            {
                throw new ArgumentException($"The calibration file \"{gainCalibrationFile}\" is invalid.");
            }

            if (gainCorrection.Value.SerialNumber != probeSerialNumber)
            {
                throw new ArgumentException($"The probe serial number ({probeSerialNumber}) does not " +
                    $"match the gain calibration file serial number ({gainCorrection.Value.SerialNumber}).");
            }

            ApGainCorrection = gainCorrection.Value.ApGainCorrectionFactor;
            LfpGainCorrection = gainCorrection.Value.LfpGainCorrectionFactor;

            Adcs = adcCalibration.Value.Adcs;
            AdcThresholds = Adcs.ToList().Select(a => (ushort)a.Threshold).ToArray();
            AdcOffsets = Adcs.ToList().Select(a => (ushort)a.Offset).ToArray();

            // Create Configuration bit arrays
            ShankConfig = NeuropixelsV1.MakeShankBits(probeConfiguration);
            BaseConfigs = NeuropixelsV1.MakeConfigBits(probeConfiguration, Adcs);
           
        }

        public void InitializeProbe()
        {
            // get probe set up to receive configuration
            WriteByte(NeuropixelsV1e.CAL_MOD, (uint)NeuropixelsV1CalibrationRegisterValues.CAL_OFF);
            WriteByte(NeuropixelsV1e.TEST_CONFIG1, 0);
            WriteByte(NeuropixelsV1e.TEST_CONFIG2, 0);
            WriteByte(NeuropixelsV1e.TEST_CONFIG3, 0);
            WriteByte(NeuropixelsV1e.TEST_CONFIG4, 0);
            WriteByte(NeuropixelsV1e.TEST_CONFIG5, 0);
            WriteByte(NeuropixelsV1e.SYNC, 0);
            WriteByte(NeuropixelsV1e.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.ACTIVE);
            WriteByte(NeuropixelsV1e.OP_MODE, (uint)NeuropixelsV1OperationRegisterValues.RECORD);
        }

        public void WriteConfiguration()
        {
            // shank configuration
            // NB: no read check because of ASIC bug that is documented in IMEC-API comments
            var shankBytes = BitHelper.ToBitReversedBytes(ShankConfig);

            WriteByte(NeuropixelsV1e.SR_LENGTH1, (uint)shankBytes.Length % 0x100);
            WriteByte(NeuropixelsV1e.SR_LENGTH2, (uint)shankBytes.Length / 0x100);

            foreach (var b in shankBytes)
            {
               WriteByte(NeuropixelsV1e.SR_CHAIN1, b);
            }

            // base configuration
            for (int i = 0; i < BaseConfigs.Length; i++)
            {
                var srAddress = i == 0 ? NeuropixelsV1e.SR_CHAIN2 : NeuropixelsV1e.SR_CHAIN3;

                for (int j = 0; j < 2; j++)
                {
                    // WONTFIX: Without this reset, the ShiftRegisterSuccess check below will always fail
                    // on whatever the second shift register write sequence regardless of order or
                    // contents. Could be increased current draw during internal process causes MCLK
                    // to droop and mess up internal state. Or that MCLK is just not good enough to
                    // prevent metastability in some logic in the ASIC that is only entered in between
                    // SR accesses.
                    WriteByte(NeuropixelsV1e.SOFT_RESET, 0xFF);
                    WriteByte(NeuropixelsV1e.SOFT_RESET, 0x00);

                    var baseBytes = BitHelper.ToBitReversedBytes(BaseConfigs[i]);

                    WriteByte(NeuropixelsV1e.SR_LENGTH1, (uint)baseBytes.Length % 0x100);
                    WriteByte(NeuropixelsV1e.SR_LENGTH2, (uint)baseBytes.Length / 0x100);

                    foreach (var b in baseBytes)
                    {
                        WriteByte(srAddress, b);
                    }
                }

                if (ReadByte(NeuropixelsV1e.STATUS) != ShiftRegisterSuccess)
                {
                    throw new InvalidOperationException($"Shift register {srAddress} status check failed.");
                }
            }
        }

        public void StartAcquisition()
        {
            // WONTFIX: Soft reset inside settings.WriteShiftRegisters() above puts probe in reset set that
            // needs to be undone here
            WriteByte(NeuropixelsV1e.OP_MODE, (uint)NeuropixelsV1OperationRegisterValues.RECORD);
            WriteByte(NeuropixelsV1e.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.ACTIVE);
        }

    }
}
