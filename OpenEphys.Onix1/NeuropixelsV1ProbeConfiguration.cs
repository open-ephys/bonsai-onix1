using System;
using System.Collections.Generic;
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
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ChannelConfiguration);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1Gain spikeAmplifierGain, NeuropixelsV1Gain lfpAmplifierGain, NeuropixelsV1ReferenceSource reference, bool spikeFilter)
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ChannelConfiguration = new();
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ChannelConfiguration);
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="channelConfiguration">Desired or current <see cref="NeuropixelsV1eProbeGroup"/> variable.</param>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filer the spike-band.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1eProbeGroup channelConfiguration, NeuropixelsV1Gain spikeAmplifierGain, NeuropixelsV1Gain lfpAmplifierGain, NeuropixelsV1ReferenceSource reference, bool spikeFilter)
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(channelConfiguration);
            ChannelConfiguration = new();
            ChannelConfiguration.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1ProbeConfiguration"/>
        /// values.
        /// </summary>
        /// <param name="probeConfiguration">Existing <see cref="NeuropixelsV1ProbeConfiguration"/> instance.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1ProbeConfiguration probeConfiguration)
        {
            SpikeAmplifierGain = probeConfiguration.SpikeAmplifierGain;
            LfpAmplifierGain = probeConfiguration.LfpAmplifierGain;
            Reference = probeConfiguration.Reference;
            SpikeFilter = probeConfiguration.SpikeFilter;
            ChannelConfiguration = new();
            ChannelConfiguration.UpdateDeviceChannelIndices(probeConfiguration.ChannelMap);
            ChannelMap = NeuropixelsV1eProbeGroup.ToChannelMap(ChannelConfiguration);
        }

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
        public List<NeuropixelsV1Electrode> ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(List<NeuropixelsV1Electrode> electrodes)
        {
            foreach (var e in electrodes)
            {
                ChannelMap[e.Channel] = e;
            }

            if (ChannelMap.Count != NeuropixelsV1.ChannelCount)
            {
                throw new InvalidOperationException($"Channel map does not match the expected number of active channels " +
                    $"for a NeuropixelsV2 probe. Expected {NeuropixelsV1.ChannelCount}, but there are {ChannelMap.Count} values.");
            }

            ChannelConfiguration.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV1eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        public NeuropixelsV1eProbeGroup ChannelConfiguration { get; set; } = new();

        /// <summary>
        /// Gets or sets a string defining the <see cref="ChannelConfiguration"/> in Base64.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ChannelConfiguration))]
        public string ChannelConfigurationString
        {
            get
            {
                var jsonString = JsonConvert.SerializeObject(ChannelConfiguration);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ChannelConfiguration = JsonConvert.DeserializeObject<NeuropixelsV1eProbeGroup>(jsonString);
                SelectElectrodes(NeuropixelsV1eProbeGroup.ToChannelMap(ChannelConfiguration));
            }
        }
    }
}
