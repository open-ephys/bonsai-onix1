using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a configuration for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class NeuropixelsV2ProbeConfiguration : IProbeInterfaceConfiguration
    {
        /// <summary>
        /// Gets or sets a value determining if the polarity of the electrode voltages acquired by the probe
        /// should be inverted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Neuropixels contain inverting amplifiers. This means that neural data that is captured by the probe
        /// will be inverted compared to the physical signal that occurs at the electrode: e.g., extracellular
        /// action potentials will tend to have positive deflections instead of negative. Setting this
        /// property to true will apply a gain of -1 to undo this effect.
        /// </para>
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Invert the polarity of the electrode voltages acquired by the probe.")]
        public bool InvertPolarity { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across probes. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// <para>
        /// If this file is missing, or does not match the connected probe's serial number, an exception is
        /// thrown unless <see cref="ContextTask.ValidationLevel"/> is <see cref="ValidationLevel.Permissive"/>,
        /// in which case the probe is configured without gain correction instead.
        /// </para>
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for this probe.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileName { get; set; }

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// External and Ground are not shank-specific. When set to <see cref="NeuropixelsV2ReferenceSource.Tip"/>,
        /// which shanks actually contribute to the reference is given by <see cref="TipReferenceShanks"/>.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines the reference for the probe.")]
        public NeuropixelsV2ReferenceSource Reference { get; set; } = NeuropixelsV2ReferenceSource.External;

        /// <summary>
        /// Gets or sets the set of shanks whose tip electrodes are shorted together to form the
        /// reference. Only meaningful when <see cref="Reference"/> is
        /// <see cref="NeuropixelsV2ReferenceSource.Tip"/>; unused otherwise.
        /// </summary>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("The shanks whose tip electrodes form the reference, when Reference is Tip.")]
        public List<int> TipReferenceShanks { get; set; } = new();

        /// <summary>
        /// Gets or sets the file path where the ProbeInterface configuration will be saved.
        /// </summary>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to where the ProbeInterface file exists for this probe.")]
        [FileNameFilter(ProbeInterfaceHelper.ProbeInterfaceFileNameFilter)]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeInterfaceFileName { get; set; }

        const int ReferencePixelCount = 4;
        const int DummyRegisterCount = 4;

        const int RegistersPerShank = NeuropixelsV2.ElectrodesPerShank + ReferencePixelCount + DummyRegisterCount;

        const int ShiftRegisterBitExternalElectrode0 = 1285;
        const int ShiftRegisterBitExternalElectrode1 = 2;

        const int ShiftRegisterBitTipElectrode0 = 644;
        const int ShiftRegisterBitTipElectrode1 = 643;

        internal NeuropixelsV2ProbeConfiguration Clone() => new()
        {
            Reference = Reference,
            TipReferenceShanks = new List<int>(TipReferenceShanks),
            InvertPolarity = InvertPolarity,
            GainCalibrationFileName = GainCalibrationFileName,
            ProbeInterfaceFileName = ProbeInterfaceFileName,
        };

        /// <summary>
        /// Builds the per-shank reference shift-register bits for a probe with the given shank count.
        /// </summary>
        /// <param name="shankCount">The number of shanks on the probe this configuration is being applied
        /// to.</param>
        internal BitArray[] CreateShankBits(int shankCount)
        {
            var shankBits = new BitArray[shankCount];
            for (int s = 0; s < shankCount; s++)
                shankBits[s] = new BitArray(RegistersPerShank, false);

            if (Reference == NeuropixelsV2ReferenceSource.Tip)
            {
                if (TipReferenceShanks.Count == 0)
                    throw new InvalidOperationException("Reference is set to Tip, but no shanks are selected in TipReferenceShanks.");

                foreach (var shank in TipReferenceShanks)
                {
                    if (shank < 0 || shank >= shankCount)
                    {
                        throw new InvalidOperationException(
                            $"TipReferenceShanks contains shank index {shank}, which is out of range for a " +
                            $"{shankCount}-shank probe.");
                    }

                    shankBits[shank][ShiftRegisterBitTipElectrode1] = true;
                    shankBits[shank][ShiftRegisterBitTipElectrode0] = true;
                }
            }
            else if (Reference == NeuropixelsV2ReferenceSource.External)
            {
                for (int s = 0; s < shankCount; s++)
                {
                    shankBits[s][ShiftRegisterBitExternalElectrode1] = true;
                    shankBits[s][ShiftRegisterBitExternalElectrode0] = true;
                }
            }

            return shankBits;
        }

        internal int GetReferenceBit() => Reference switch
        {
            NeuropixelsV2ReferenceSource.External => 1,
            NeuropixelsV2ReferenceSource.Tip => 2,
            NeuropixelsV2ReferenceSource.Ground => 3,
            _ => throw new InvalidOperationException("Invalid reference.")
        };

        internal bool IsGroundReference() => Reference == NeuropixelsV2ReferenceSource.Ground;
    }
}
