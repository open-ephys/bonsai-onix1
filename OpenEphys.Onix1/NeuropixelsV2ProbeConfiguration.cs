using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV2Bank
    {
        /// <summary>
        /// Specifies that Bank A is the current bank.
        /// </summary>
        /// <remarks>Bank A is defined as shank index 0 to 383 along each shank.</remarks>
        A,
        /// <summary>
        /// Specifies that Bank B is the current bank.
        /// </summary>
        /// <remarks>Bank B is defined as shank index 384 to 767 along each shank.</remarks>
        B,
        /// <summary>
        /// Specifies that Bank C is the current bank.
        /// </summary>
        /// <remarks>Bank C is defined as shank index 768 to 1151 along each shank.</remarks>
        C,
        /// <summary>
        /// Specifies that Bank D is the current bank.
        /// </summary>
        /// <remarks>
        /// Bank D is defined as shank index 1152 to 1279 along each shank. Note that Bank D is not a full contingent
        /// of 384 channels; to compensate for this, electrodes from Bank C (starting at shank index 896) are used to
        /// generate a full 384 channel map.
        /// </remarks>
        D,
    }

    /// <summary>
    /// Defines a configuration for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2QuadShankProbeConfiguration))]
    [XmlInclude(typeof(NeuropixelsV2SingleShankProbeConfiguration))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2ProbeConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV2Probe"/> for this probe.
        /// </summary>
        [Browsable(false)]
        public NeuropixelsV2Probe Probe { get; set; }

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
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for probe A.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileName { get; set; }

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        [XmlIgnore]
        [Description("Defines the reference for the probe.")]
        public abstract Enum Reference { get; set; }

        /// <summary>
        /// Gets or sets the serialized reference value.
        /// </summary>
        /// <remarks>
        /// Ensures that XML serialization can occur for the generic Enum type <see cref="Reference"/>.
        /// </remarks>
        [XmlElement(nameof(Reference))]
        [Browsable(false)]
        [Externalizable(false)]
        public abstract string ReferenceSerialized { get; set; }

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public abstract NeuropixelsV2Electrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(NeuropixelsV2Electrode[] electrodes)
        {
            if (electrodes.Length == 0) return;

            var channelMap = ChannelMap;

            foreach (var e in electrodes)
            {
                try
                {
                    channelMap[e.Channel] = e;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {channelMap.Length - 1} are supported.", ex);
                }
            }

            ProbeGroup.UpdateDeviceChannelIndices(channelMap);
        }

        /// <summary>
        /// Protected task that loads the ProbeInterface file asynchronously.
        /// </summary>
        protected Task<NeuropixelsV2eProbeGroup> probeGroupTask = null;

        /// <summary>
        /// Protected <see cref="NeuropixelsV2eProbeGroup"/> class.
        /// </summary>
        protected NeuropixelsV2eProbeGroup probeGroup = null;

        /// <summary>
        /// Gets the <see cref="ProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming.")]
        [Browsable(false)]
        [Externalizable(false)]
        public abstract NeuropixelsV2eProbeGroup ProbeGroup { get; set; }

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        /// <remarks>
        /// [Obsolete]. Cannot tag this property with the Obsolete attribute due to https://github.com/dotnet/runtime/issues/100453
        /// </remarks>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public abstract string ProbeGroupString { get; set; }

        /// <summary>
        /// Prevent the ProbeGroup property from being serialized.
        /// </summary>
        /// <returns>False</returns>
        [Obsolete]
        public bool ShouldSerializeProbeGroupString()
        {
            return false;
        }

        /// <summary>
        /// Protected ProbeInterface file name.
        /// </summary>
        protected string probeInterfaceFileName;

        /// <summary>
        /// Gets or sets the file path where the ProbeInterface configuration will be saved.
        /// </summary>
        /// <remarks>
        /// If left empty, the ProbeInterface configuration will not be saved.
        /// </remarks>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to where the ProbeInterface file will be saved for this probe. If the file exists, it will be overwritten.")]
        [FileNameFilter(ProbeInterfaceHelper.ProbeInterfaceFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeInterfaceFileName
        {
            get => probeInterfaceFileName;
            set => probeInterfaceFileName = value;
        }

        /// <summary>
        /// Gets or sets the ProbeInterface file name, loading the given file asynchronously when set.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        [Externalizable(false)]
        public abstract string ProbeInterfaceLoadFileName { get; set; }

        /// <summary>
        /// Gets or sets a string defining the path to an external ProbeInterface JSON file.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeInterfaceFileName))]
        public string ProbeInterfaceFileNameSerialize
        {
            get
            {
                if (string.IsNullOrEmpty(ProbeInterfaceFileName))
                    return "";

                if (probeGroup != null)
                    ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(ProbeGroup, ProbeInterfaceFileName);

                return ProbeInterfaceFileName;
            }
            set => ProbeInterfaceLoadFileName = value;
        }
        
        const int ReferencePixelCount = 4;
        const int DummyRegisterCount = 4;

        /// <summary>
        /// Number of registers per shank.
        /// </summary>
        protected const int RegistersPerShank = NeuropixelsV2.ElectrodePerShank + ReferencePixelCount + DummyRegisterCount;

        /// <summary>
        /// Index of the shift register bit for external electrode 0.
        /// </summary>
        protected const int ShiftRegisterBitExternalElectrode0 = 1285;
        /// <summary>
        /// Index of the shift register bit for external electrode 1.
        /// </summary>
        protected const int ShiftRegisterBitExternalElectrode1 = 2;

        /// <summary>
        /// Index of the shift register bit for tip electrode 0.
        /// </summary>
        protected const int ShiftRegisterBitTipElectrode0 = 644;
        /// <summary>
        /// Index of the shift register bit for tip electrode 1.
        /// </summary>
        protected const int ShiftRegisterBitTipElectrode1 = 643;

        internal abstract BitArray[] CreateShankBits(Enum reference);

        internal abstract int GetReferenceBit(Enum reference);

        internal abstract bool IsGroundReference();

        internal abstract NeuropixelsV2ProbeConfiguration Clone();

        internal abstract Func<int, int> GetChannelNumberFunc();
    }
}
