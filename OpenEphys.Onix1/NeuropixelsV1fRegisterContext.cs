using System;
using System.Collections;
using System.IO;
using System.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1fRegisterContext : I2CRegisterContext
    {
        public double ApGainCorrection { get; }
        public double LfpGainCorrection { get; }
        public ushort[] AdcThresholds { get; }
        public ushort[] AdcOffsets { get; }

        readonly NeuropixelsV1Adc[] Adcs = new NeuropixelsV1Adc[NeuropixelsV1.AdcCount];

        const uint ShiftRegisterSuccess = 1 << 7;

        readonly DeviceContext device;
        readonly BitArray ShankConfig;
        readonly BitArray[] BaseConfigs;

        public NeuropixelsV1fRegisterContext(DeviceContext deviceContext, NeuropixelsV1ProbeConfiguration configuration, string gainCalibrationFile, string adcCalibrationFile)
            : base(deviceContext, NeuropixelsV1.ProbeI2CAddress)
        {

            device = deviceContext;
            var metaData = new NeuropixelsV1fMetadata(device);

            if (!File.Exists(gainCalibrationFile))
            {
                throw new ArgumentException($"A gain calibration file must be specified for the probe with serial number " +
                    $"{metaData.ProbeSerialNumber}");
            }

            if (!File.Exists(adcCalibrationFile))
            {
                throw new ArgumentException($"An ADC calibration file must be specified for the probe with serial number " +
                    $"{metaData.ProbeSerialNumber}");
            }

            var adcCalibration = NeuropixelsV1Helper.TryParseAdcCalibrationFile(adcCalibrationFile);

            if (!adcCalibration.HasValue)
            {
                throw new ArgumentException($"The calibration file \"{adcCalibrationFile}\" is invalid.");
            }

            if (adcCalibration.Value.SerialNumber != metaData.ProbeSerialNumber)
            {
                throw new ArgumentException($"The probe serial number ({metaData.ProbeSerialNumber}) does not " +
                    $"match the ADC calibration file serial number ({adcCalibration.Value.SerialNumber}).");
            }

            var gainCorrection = NeuropixelsV1Helper.TryParseGainCalibrationFile(gainCalibrationFile,
                configuration.SpikeAmplifierGain, configuration.LfpAmplifierGain, NeuropixelsV1.ElectrodeCount);


            if (!gainCorrection.HasValue)
            {
                throw new ArgumentException($"The calibration file \"{gainCalibrationFile}\" is invalid.");
            }

            if (gainCorrection.Value.SerialNumber != metaData.ProbeSerialNumber)
            {
                throw new ArgumentException($"The probe serial number ({metaData.ProbeSerialNumber}) does not " +
                    $"match the gain calibration file serial number ({gainCorrection.Value.SerialNumber}).");
            }

            ApGainCorrection = gainCorrection.Value.ApGainCorrectionFactor;
            LfpGainCorrection = gainCorrection.Value.LfpGainCorrectionFactor;

            Adcs = adcCalibration.Value.Adcs;
            AdcThresholds = Adcs.ToList().Select(a => (ushort)a.Threshold).ToArray();
            AdcOffsets = Adcs.ToList().Select(a => (ushort)a.Offset).ToArray();

            // create Configuration bit arrays
            ShankConfig = NeuropixelsV1.MakeShankBits(configuration);
            BaseConfigs = NeuropixelsV1.MakeConfigBits(configuration, Adcs);
        }

        internal void InitializeProbe()
        {
            // turn off calibration mode
            WriteByte(NeuropixelsV1f.CAL_MOD, (uint)NeuropixelsV1CalibrationRegisterValues.CAL_OFF);
            WriteByte(NeuropixelsV1f.SYNC, 0);

            // perform digital and channel reset
            WriteByte(NeuropixelsV1f.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.DIG_CH_RESET);

            // change operation state to Recording
            WriteByte(NeuropixelsV1f.OP_MODE, (uint)NeuropixelsV1OperationRegisterValues.RECORD);

            // start acquisition
            WriteByte(NeuropixelsV1f.REC_MOD, (uint)NeuropixelsV1RecordRegisterValues.ACTIVE);
        }

        internal void WriteShiftRegisters()
        {
            // shank configuration
            // NB: no read check because of ASIC bug that is documented in IMEC-API comments
            var shankBytes = BitHelper.ToBitReversedBytes(ShankConfig);

            WriteByte(NeuropixelsV1f.SR_LENGTH1, (uint)shankBytes.Length % 0x100);
            WriteByte(NeuropixelsV1f.SR_LENGTH2, (uint)shankBytes.Length / 0x100);

            foreach (var b in shankBytes)
            {
                WriteByte(NeuropixelsV1e.SR_CHAIN1, b);
            }

            // base configuration
            for (int i = 0; i < BaseConfigs.Length; i++)
            {
                var srAddress = i == 0 ? NeuropixelsV1f.SR_CHAIN2 : NeuropixelsV1f.SR_CHAIN3;

                for (int j = 0; j < 2; j++)
                {
                    var baseBytes = BitHelper.ToBitReversedBytes(BaseConfigs[i]);

                    WriteByte(NeuropixelsV1f.SR_LENGTH1, (uint)baseBytes.Length % 0x100);
                    WriteByte(NeuropixelsV1f.SR_LENGTH2, (uint)baseBytes.Length / 0x100);

                    foreach (var b in baseBytes)
                    {
                        WriteByte(srAddress, b);
                    }
                }

                if (ReadByte(NeuropixelsV1f.STATUS) != ShiftRegisterSuccess)
                {
                    throw new WorkflowException($"Shift register {srAddress} status check failed.");
                }
            }

            // Adc correction parameters
            for (uint i = 0; i < Adcs.Length; i += 2)
            {
                device.WriteRegister(NeuropixelsV1f.ADC01_00_OFF_THRESH + i, (uint)(Adcs[i + 1].Offset << 26 | Adcs[i + 1].Threshold << 16 | Adcs[i].Offset << 10 | Adcs[i].Threshold));
            }

            var fixedPointLfPGain = (uint)(LfpGainCorrection * (1 << 14)) & 0xFFFF;
            var fixedPointApGain = (uint)(ApGainCorrection * (1 << 14)) & 0xFFFF;

            for (uint i = 0; i < NeuropixelsV1.ChannelCount / 2; i++)
            {
                device.WriteRegister(NeuropixelsV1f.CHAN001_000_LFPGAIN + i, fixedPointLfPGain << 16 | fixedPointLfPGain);
                device.WriteRegister(NeuropixelsV1f.CHAN001_000_APGAIN + i, fixedPointApGain << 16 | fixedPointApGain);
            }
        }
    }
}
