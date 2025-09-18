using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV1Bank
    {
        /// <summary>
        /// Specifies that Bank A is the current bank.
        /// </summary>
        /// <remarks>Bank A is defined as shank index 0 to 383 along each shank.</remarks>
        A = 0,
        /// <summary>
        /// Specifies that Bank B is the current bank.
        /// </summary>
        /// <remarks>Bank B is defined as shank index 384 to 767 along each shank.</remarks>
        B,
        /// <summary>
        /// Specifies that Bank C is the current bank.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For Neuropixels 1.0 probes, Bank C is defined as shank index 768 to 960 along each shank. Note that
        /// Bank C is not a full contingent of 384 channels; to compensate for this, electrodes from Bank B 
        /// (starting at shank index 576) are used to generate a full 384 channel map.
        /// </para>
        /// <para>
        /// For Neuropixels 1.0 UHD probes, Bank C is shank index 768 to 1151
        /// </para>
        /// </remarks>
        C,
        /// <summary>
        /// Specifies that Bank D of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank D is defined as shank index 1152 to 1535 along each shank.</remarks>
        D,
        /// <summary>
        /// Specifies that Bank E of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank E is defined as shank index 1536 to 1919 along each shank.</remarks>
        E,
        /// <summary>
        /// Specifies that Bank F of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank F is defined as shank index 1920 to 2303 along each shank.</remarks>
        F,
        /// <summary>
        /// Specifies that Bank G of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank G is defined as shank index 2304 to 2687 along each shank.</remarks>
        G,
        /// <summary>
        /// Specifies that Bank H of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank H is defined as shank index 2688 to 3071 along each shank.</remarks>
        H,
        /// <summary>
        /// Specifies that Bank I of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank I is defined as shank index 3072 to 3455 along each shank.</remarks>
        I,
        /// <summary>
        /// Specifies that Bank J of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank J is defined as shank index 3456 to 3839 along each shank.</remarks>
        J,
        /// <summary>
        /// Specifies that Bank K of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank K is defined as shank index 3840 to 4223 along each shank.</remarks>
        K,
        /// <summary>
        /// Specifies that Bank L of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank L is defined as shank index 4224 to 4607 along each shank.</remarks>
        L,
        /// <summary>
        /// Specifies that Bank M of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank M is defined as shank index 4608 to 4991 along each shank.</remarks>
        M,
        /// <summary>
        /// Specifies that Bank N of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank N is defined as shank index 4992 to 5375 along each shank.</remarks>
        N,
        /// <summary>
        /// Specifies that Bank O of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank O is defined as shank index 5376 to 5759 along each shank.</remarks>
        O,
        /// <summary>
        /// Specifies that Bank P of the UHD probe is the current bank.
        /// </summary>
        /// <remarks>Bank P is defined as shank index 5760 to 6143 along each shank.</remarks>
        P
    }

    /// <summary>
    /// Specifies the current probe type. 
    /// </summary>
    public enum NeuropixelsV1ProbeType
    {
        /// <summary>
        /// Specifies that this is a Neuropixels 1.0 probe.
        /// </summary>
        NP1,
        /// <summary>
        /// Specifies that this is a Neuropixels 1.0 Ultra-High Density probe.
        /// </summary>
        UHD
    }

    /// <summary>
    /// Defines a configuration for NeuropixelsV1e.
    /// </summary>
    public class NeuropixelsV1ProbeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default values.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration()
        {
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup, ProbeType);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="probeType">Desired or current <see cref="NeuropixelsV1ProbeType"/>.</param>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        public NeuropixelsV1ProbeConfiguration(
            NeuropixelsV1ProbeType probeType,
            NeuropixelsV1Gain spikeAmplifierGain,
            NeuropixelsV1Gain lfpAmplifierGain,
            NeuropixelsV1ReferenceSource reference,
            bool spikeFilter)
        {
            ProbeType = probeType;
            ProbeGroup = new(probeType);
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup, ProbeType);
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="probeGroup">Desired or current <see cref="NeuropixelsV1eProbeGroup"/> variable.</param>
        /// <param name="probeType">Desired or current <see cref="NeuropixelsV1ProbeType"/>.</param>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        public NeuropixelsV1ProbeConfiguration(
            NeuropixelsV1eProbeGroup probeGroup,
            NeuropixelsV1ProbeType probeType,
            NeuropixelsV1Gain spikeAmplifierGain,
            NeuropixelsV1Gain lfpAmplifierGain,
            NeuropixelsV1ReferenceSource reference,
            bool spikeFilter)
        {
            ProbeType = probeType;
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ProbeGroup = probeGroup;
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup, ProbeType);
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using
        /// the given <see cref="NeuropixelsV1ProbeConfiguration"/> values.
        /// </summary>
        /// <param name="probeConfiguration">Existing <see cref="NeuropixelsV1ProbeConfiguration"/> instance.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1ProbeConfiguration probeConfiguration)
        {
            ProbeType = probeConfiguration.ProbeType;
            SpikeAmplifierGain = probeConfiguration.SpikeAmplifierGain;
            LfpAmplifierGain = probeConfiguration.LfpAmplifierGain;
            Reference = probeConfiguration.Reference;
            SpikeFilter = probeConfiguration.SpikeFilter;
            ProbeGroup = probeConfiguration.ProbeGroup;
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup, ProbeType);
        }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV1ProbeType"/> for this probe.
        /// </summary>
        [Browsable(false)]
        [Description("Defines the type of probe that is being configured.")]
        public NeuropixelsV1ProbeType ProbeType { get; set; } = NeuropixelsV1ProbeType.NP1;

        /// <summary>
        /// Gets or sets the amplifier gain for the spike-band.
        /// </summary>
        /// <remarks>
        /// The spike-band is from DC to 10 kHz if <see cref="SpikeFilter"/> is set to false, while the 
        /// spike-band is from 300 Hz to 10 kHz if <see cref="SpikeFilter"/> is set to true.
        /// </remarks>
        [Category("Configuration")]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        /// <summary>
        /// Gets or sets the amplifier gain for the LFP-band.
        /// </summary>
        /// <remarks>
        /// The LFP band is from 0.5 to 500 Hz.
        /// </remarks>
        [Category("Configuration")]
        [Description("Amplifier gain for LFP-band.")]
        public NeuropixelsV1Gain LfpAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain50;

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// All electrodes are set to the same reference, which can be either 
        /// <see cref="NeuropixelsV1ReferenceSource.External"/> or <see cref="NeuropixelsV1ReferenceSource.Tip"/>. 
        /// Setting to <see cref="NeuropixelsV1ReferenceSource.External"/> will use the external reference, while 
        /// <see cref="NeuropixelsV1ReferenceSource.Tip"/> sets the reference to the electrode at the tip of the probe.
        /// </remarks>
        [Category("Configuration")]
        [Description("Reference selection.")]
        public NeuropixelsV1ReferenceSource Reference { get; set; } = NeuropixelsV1ReferenceSource.External;

        /// <summary>
        /// Gets or sets the state of the spike-band filter.
        /// </summary>
        /// <remarks>
        /// If set to true, the spike-band has a 300 Hz high-pass filter which will be activated. If set to
        /// false, the high-pass filter will not to be activated.
        /// </remarks>
        [Category("Configuration")]
        [Description("If true, activates a 300 Hz high-pass filter in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public NeuropixelsV1Electrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(NeuropixelsV1Electrode[] electrodes)
        {
            foreach (var e in electrodes)
            {
                try
                {
                    ChannelMap[e.Channel] = e;
                } catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {ChannelMap.Length - 1} are supported.", ex);
                }
            }

            ProbeGroup.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV1eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines all aspects of the probe group, including probe contours, electrode size and location, enabled channels, etc.")]
        public NeuropixelsV1eProbeGroup ProbeGroup { get; set; } = new(NeuropixelsV1ProbeType.NP1);

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public string ProbeGroupString
        {
            get
            {
                var jsonString = JsonConvert.SerializeObject(ProbeGroup);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV1eProbeGroup>(jsonString);
                SelectElectrodes(NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup, ProbeType));
            }
        }
    }
}
