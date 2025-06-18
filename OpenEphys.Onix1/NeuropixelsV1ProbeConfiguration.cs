using System;
using System.ComponentModel;
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
        /// Bank C is defined as shank index 768 to 960 along each shank. Note that Bank C is not a full contingent
        /// of 384 channels; to compensate for this, electrodes from Bank B (starting at shank index 576) are used to
        /// generate a full 384 channel map.
        /// </remarks>
        C
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
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        /// <param name="probeInterfaceFile">String containing the filepath to an external configuration file.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1Gain spikeAmplifierGain, NeuropixelsV1Gain lfpAmplifierGain,
            NeuropixelsV1ReferenceSource reference, bool spikeFilter, string probeInterfaceFile)
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ProbeInterfaceFile = probeInterfaceFile;
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="probeGroup">Desired or current <see cref="NeuropixelsV1eProbeGroup"/> variable.</param>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        [Obsolete("Specifying a probe group is obsolete as of 0.6.1. Remove in 1.0.0")]
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1eProbeGroup probeGroup, NeuropixelsV1Gain spikeAmplifierGain, 
            NeuropixelsV1Gain lfpAmplifierGain, NeuropixelsV1ReferenceSource reference, bool spikeFilter)
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> 
        /// using the given <see cref="NeuropixelsV1ProbeConfiguration"/>
        /// values.
        /// </summary>
        /// <param name="probeConfiguration">Existing <see cref="NeuropixelsV1ProbeConfiguration"/> instance.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1ProbeConfiguration probeConfiguration)
        {
            SpikeAmplifierGain = probeConfiguration.SpikeAmplifierGain;
            LfpAmplifierGain = probeConfiguration.LfpAmplifierGain;
            Reference = probeConfiguration.Reference;
            SpikeFilter = probeConfiguration.SpikeFilter;
            ProbeInterfaceFile = probeConfiguration.ProbeInterfaceFile;
        }

        /// <summary>
        /// Gets or sets the amplifier gain for the spike-band.
        /// </summary>
        /// <remarks>
        /// The spike-band is from DC to 10 kHz if <see cref="SpikeFilter"/> is set to false, while the 
        /// spike-band is from 300 Hz to 10 kHz if <see cref="SpikeFilter"/> is set to true.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        /// <summary>
        /// Gets or sets the amplifier gain for the LFP-band.
        /// </summary>
        /// <remarks>
        /// The LFP band is from 0.5 to 500 Hz.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
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
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Reference selection.")]
        public NeuropixelsV1ReferenceSource Reference { get; set; } = NeuropixelsV1ReferenceSource.External;

        /// <summary>
        /// Gets or sets the state of the spike-band filter.
        /// </summary>
        /// <remarks>
        /// If set to true, the spike-band has a 300 Hz high-pass filter which will be activated. If set to
        /// false, the high-pass filter will not to be activated.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("If true, activates a 300 Hz high-pass filter in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        [Obsolete("This is now obsolete, as the Probe Group is now held in an externalized configuration file. Remove in 1.0.0.")]
        public NeuropixelsV1Electrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        [Obsolete("This method is obsolete as of 0.6.1, and should not be called from this class. Remove in 1.0.0")]
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
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines all aspects of the probe group, including probe contours, electrode size and location, enabled channels, etc.")]
        [Obsolete("Holding the class has been superseded by an externalized configuration file. Remove in 1.0.0.")]
        [Browsable(false)]
        [Externalizable(false)]
        public NeuropixelsV1eProbeGroup ProbeGroup { get; set; }

        /// <summary>
        /// Gets or sets the file path to a configuration file holding the Probe Interface JSON specifications for this probe.
        /// </summary>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to a configuration file holding the Probe Interface JSON specifications for this probe.")]
        [FileNameFilter($"Probe Interface files ({ProbeGroupHelper.ProbeInterfaceFileString}*.json)|*.json")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeInterfaceFile { get; set; }
    }
}
