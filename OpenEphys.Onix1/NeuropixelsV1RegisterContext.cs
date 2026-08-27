using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1RegisterContext : I2CRegisterContext
    {
        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }

        const uint ShiftRegisterSuccess = 1 << 7;

        readonly NeuropixelsV1Adc[] Adcs = new NeuropixelsV1Adc[NeuropixelsV1.AdcCount];
        readonly BitArray ShankConfig;
        readonly BitArray[] BaseConfigs;

        public NeuropixelsV1RegisterContext(DeviceContext deviceContext, uint i2cAddress, ulong probeSerialNumber,
            NeuropixelsV1ProbeConfiguration probeConfiguration, NeuropixelsV1ProbeGroup probeGroup)
            : base(deviceContext, i2cAddress)
        {
            NeuropixelsV1AdcCalibration? adcCalibration = null;
            if (!File.Exists(probeConfiguration.AdcCalibrationFileName))
            {
                ContextHelper.Validate(ValidationLevel.Permissive, new ArgumentException(
                    $"No ADC calibration file was specified for the probe with serial number {probeSerialNumber}."));
            }
            else
            {
                adcCalibration = NeuropixelsV1Helper.TryParseAdcCalibrationFile(probeConfiguration.AdcCalibrationFileName);

                if (!adcCalibration.HasValue)
                {
                    throw new ArgumentException(
                        $"The calibration file \"{probeConfiguration.AdcCalibrationFileName}\" has an invalid format.");
                }
                else if (adcCalibration.Value.SerialNumber != probeSerialNumber)
                {
                    ContextHelper.Validate(ValidationLevel.Permissive, new ArgumentException(
                        $"The probe serial number ({probeSerialNumber}) does not " +
                        $"match the ADC calibration file serial number ({adcCalibration.Value.SerialNumber})."));
                    adcCalibration = null;
                }
            }

            NeuropixelsV1eGainCorrection? gainCorrection = null;
            if (!File.Exists(probeConfiguration.GainCalibrationFileName))
            {
                ContextHelper.Validate(ValidationLevel.Permissive, new ArgumentException(
                    $"No gain calibration file was specified for the probe with serial number {probeSerialNumber}."));
            }
            else
            {
                gainCorrection = NeuropixelsV1Helper.TryParseGainCalibrationFile(probeConfiguration.GainCalibrationFileName,
                    probeConfiguration.SpikeAmplifierGain, probeConfiguration.LfpAmplifierGain, NeuropixelsV1.ElectrodeCount);

                if (!gainCorrection.HasValue)
                {
                    throw new ArgumentException(
                        $"The calibration file \"{probeConfiguration.GainCalibrationFileName}\" has an invalid format.");
                }
                else if (gainCorrection.Value.SerialNumber != probeSerialNumber)
                {
                    ContextHelper.Validate(ValidationLevel.Permissive, new ArgumentException(
                        $"The probe serial number ({probeSerialNumber}) does not " +
                        $"match the gain calibration file serial number ({gainCorrection.Value.SerialNumber})."));
                    gainCorrection = null;
                }
            }

            ApGainCorrection = gainCorrection?.ApGainCorrectionFactor ?? 1.0;
            LfpGainCorrection = gainCorrection?.LfpGainCorrectionFactor ?? 1.0;

            Adcs = adcCalibration?.Adcs ?? Enumerable.Range(0, NeuropixelsV1.AdcCount).Select(_ => new NeuropixelsV1Adc()).ToArray();
            AdcThresholds = Adcs.Select(a => (ushort)a.Threshold).ToArray();
            AdcOffsets = Adcs.Select(a => (ushort)a.Offset).ToArray();

            // Create Configuration bit arrays
            ShankConfig = NeuropixelsV1.MakeShankBits(probeConfiguration, probeGroup);
            BaseConfigs = NeuropixelsV1.MakeConfigBits(probeConfiguration, Adcs);
        }

        public void InitializeProbe()
        {
            // get probe set up to receive configuration
            WriteByte(NeuropixelsV1.CAL_MOD, (uint)NeuropixelsV1CalibrationRegisterValues.CAL_OFF);
            WriteByte(NeuropixelsV1.TEST_CONFIG1, 0);
            WriteByte(NeuropixelsV1.TEST_CONFIG2, 0);
            WriteByte(NeuropixelsV1.TEST_CONFIG3, 0);
            WriteByte(NeuropixelsV1.TEST_CONFIG4, 0);
            WriteByte(NeuropixelsV1.TEST_CONFIG5, 0);
            WriteByte(NeuropixelsV1.SYNC, 0);
            WriteByte(NeuropixelsV1.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.ACTIVE);
            WriteByte(NeuropixelsV1.OP_MODE, (uint)NeuropixelsV1OperationRegisterValues.RECORD);
        }

        public void WriteConfiguration()
        {
            // shank configuration
            // NB: no read check, because of an ASIC bug affecting this register
            var shankBytes = BitHelper.ToBitReversedBytes(ShankConfig);

            WriteByte(NeuropixelsV1.SR_LENGTH1, (uint)shankBytes.Length % 0x100);
            WriteByte(NeuropixelsV1.SR_LENGTH2, (uint)shankBytes.Length / 0x100);

            foreach (var b in shankBytes)
            {
               WriteByte(NeuropixelsV1.SR_CHAIN1, b);
            }

            // base configuration
            for (int i = 0; i < BaseConfigs.Length; i++)
            {
                var srAddress = i == 0 ? NeuropixelsV1.SR_CHAIN2 : NeuropixelsV1.SR_CHAIN3;

                for (int j = 0; j < 2; j++)
                {
                    // WONTFIX: Without this reset, the ShiftRegisterSuccess check below will always fail
                    // on whatever the second shift register write sequence regardless of order or
                    // contents. Could be increased current draw during internal process causes MCLK
                    // to droop and mess up internal state. Or that MCLK is just not good enough to
                    // prevent metastability in some logic in the ASIC that is only entered in between
                    // SR accesses.
                    WriteByte(NeuropixelsV1.SOFT_RESET, 0xFF);
                    WriteByte(NeuropixelsV1.SOFT_RESET, 0x00);

                    var baseBytes = BitHelper.ToBitReversedBytes(BaseConfigs[i]);

                    WriteByte(NeuropixelsV1.SR_LENGTH1, (uint)baseBytes.Length % 0x100);
                    WriteByte(NeuropixelsV1.SR_LENGTH2, (uint)baseBytes.Length / 0x100);

                    foreach (var b in baseBytes)
                    {
                        WriteByte(srAddress, b);
                    }
                }

                if (ReadByte(NeuropixelsV1.STATUS) != ShiftRegisterSuccess)
                {
                    ContextHelper.Validate(ValidationLevel.Permissive, new InvalidOperationException(
                        $"Shift register 0x{srAddress:X2} status check failed."));
                }
            }
        }

        public void StartAcquisition()
        {
            // WONTFIX: Soft reset inside settings.WriteShiftRegisters() above puts probe in reset set that
            // needs to be undone here
            WriteByte(NeuropixelsV1.OP_MODE, (uint)NeuropixelsV1OperationRegisterValues.RECORD);
            WriteByte(NeuropixelsV1.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.ACTIVE);
        }

    }
}
