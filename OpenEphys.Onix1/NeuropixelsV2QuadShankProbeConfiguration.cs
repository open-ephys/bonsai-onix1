using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the reference for a quad-shank probe.
    /// </summary>
    public enum NeuropixelsV2QuadShankReference : uint
    {
        /// <summary>
        /// Specifies that the External reference will be used.
        /// </summary>
        External,
        /// <summary>
        /// Specifies that the tip reference of shank 1 will be used.
        /// </summary>
        Tip1,
        /// <summary>
        /// Specifies that the tip reference of shank 2 will be used.
        /// </summary>
        Tip2,
        /// <summary>
        /// Specifies that the tip reference of shank 3 will be used.
        /// </summary>
        Tip3,
        /// <summary>
        /// Specifies that the tip reference of shank 4 will be used.
        /// </summary>
        Tip4
    }

    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV2QuadShankBank
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
    /// Defines a configuration for quad-shank, Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    public class NeuropixelsV2QuadShankProbeConfiguration
    {
        /// <summary>
        /// Creates a model of the probe with all electrodes instantiated.
        /// </summary>
        [XmlIgnore]
        public static readonly IReadOnlyList<NeuropixelsV2QuadShankElectrode> ProbeModel = CreateProbeModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
            ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ChannelCount);
            for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            {
                ChannelMap.Add(ProbeModel.FirstOrDefault(e => e.Channel == i));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe)
        {
            ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ChannelCount);
            for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            {
                ChannelMap.Add(ProbeModel.FirstOrDefault(e => e.Channel == i));
            }
            Probe = probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference)
        {
            ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ChannelCount);
            for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            {
                ChannelMap.Add(ProbeModel.FirstOrDefault(e => e.Channel == i));
            }
            Probe = probe;
            Reference = reference;
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            var probes = probeConfiguration.ChannelConfiguration.Probes.ToList().Select(probe => new Probe(probe));
            ChannelConfiguration = new(probeConfiguration.ChannelConfiguration.Specification, probeConfiguration.ChannelConfiguration.Version, probes.ToArray());
            ChannelMap = NeuropixelsV2eProbeGroup.ToChannelMap(ChannelConfiguration);
            Probe = probeConfiguration.Probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eProbeGroup"/> channel configuration. The <see cref="ChannelMap"/> is automatically 
        /// generated from the <see cref="ChannelConfiguration"/>. 
        /// </summary>
        /// <param name="channelConfiguration">The existing <see cref="NeuropixelsV2eProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2eProbeGroup channelConfiguration, NeuropixelsV2QuadShankReference reference, NeuropixelsV2Probe probe)
        {
            ChannelMap = NeuropixelsV2eProbeGroup.ToChannelMap(channelConfiguration);
            ChannelConfiguration = channelConfiguration;
            Reference = reference;
            Probe = probe;
        }

        private static List<NeuropixelsV2QuadShankElectrode> CreateProbeModel()
        {
            var electrodes = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ElectrodePerShank * 4);
            for (int i = 0; i < NeuropixelsV2.ElectrodePerShank * 4; i++)
            {
                electrodes.Add(new NeuropixelsV2QuadShankElectrode(i));
            }
            return electrodes;
        }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV2Probe"/> for this probe.
        /// </summary>
        public NeuropixelsV2Probe Probe { get; set; } = NeuropixelsV2Probe.ProbeA;

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// All electrodes are set to the same reference, which can be  
        /// <see cref="NeuropixelsV2QuadShankReference.External"/> or any of the tip references 
        /// (<see cref="NeuropixelsV2QuadShankReference.Tip1"/>, <see cref="NeuropixelsV2QuadShankReference.Tip2"/>, etc.). 
        /// Setting to <see cref="NeuropixelsV2QuadShankReference.External"/> will use the external reference, while 
        /// <see cref="NeuropixelsV2QuadShankReference.Tip1"/> sets the reference to the electrode at the tip of the first shank.
        /// </remarks>
        public NeuropixelsV2QuadShankReference Reference { get; set; } = NeuropixelsV2QuadShankReference.External;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public List<NeuropixelsV2QuadShankElectrode> ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes)
        {
            foreach (var e in electrodes)
            {
                ChannelMap[e.Channel] = e;
            }

            if (ChannelMap.Count != NeuropixelsV2.ChannelCount)
            {
                throw new InvalidOperationException($"Channel map does not match the expected number of active channels " +
                    $"for a NeuropixelsV2 probe. Expected {NeuropixelsV2.ChannelCount}, but there are {ChannelMap.Count} values.");
            }

            ChannelConfiguration.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV2eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        public NeuropixelsV2eProbeGroup ChannelConfiguration { get; private set; } = new();

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
                ChannelConfiguration = JsonConvert.DeserializeObject<NeuropixelsV2eProbeGroup>(jsonString);
                SelectElectrodes(NeuropixelsV2eProbeGroup.ToChannelMap(ChannelConfiguration));
            }
        }
    }
}
