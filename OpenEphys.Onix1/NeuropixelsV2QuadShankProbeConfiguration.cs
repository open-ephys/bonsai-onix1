using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;
using Newtonsoft.Json;
using System.Xml.Serialization;

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
        [Obsolete("Probe model no longer needed as of 0.6.1. Remove in 1.0.0.")]
        [Browsable(false)]
        public static readonly IReadOnlyList<NeuropixelsV2QuadShankElectrode> ProbeModel = CreateProbeModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        [Obsolete("Default constructor is no longer allowed, as the Probe is a required input. Remove in 1.0.0.")]
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
            Probe = NeuropixelsV2Probe.ProbeA;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe)
        {
            Probe = probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference)
        {
            Probe = probe;
            Reference = reference;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference, string probeInterfaceFile)
        {
            Probe = probe;
            Reference = reference;
            ProbeInterfaceFile = probeInterfaceFile;
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Probe = probeConfiguration.Probe;
            Reference = probeConfiguration.Reference;
            ProbeInterfaceFile = probeConfiguration.ProbeInterfaceFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eProbeGroup"/> channel configuration. The <see cref="ChannelMap"/> is automatically 
        /// generated from the <see cref="ProbeGroup"/>. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        [Obsolete("Probe Group is no longer used as of 0.6.1. Remove in 1.0.0.")]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2eProbeGroup probeGroup, NeuropixelsV2QuadShankReference reference, NeuropixelsV2Probe probe)
        {
            Reference = reference;
            Probe = probe;
        }

        [Obsolete("No longer used. Remove in 1.0.0.")]
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
        public NeuropixelsV2Probe Probe { get; }

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
        [Obsolete("This is now obsolete, as the Probe Group is now held in an externalized configuration file. Remove in 1.0.0.")]
        [Browsable(false)]
        [Externalizable(false)]
        public NeuropixelsV2QuadShankElectrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        [Obsolete("This method is obsolete as of 0.6.1, and should not be called from this class. Remove in 1.0.0")]
        public void SelectElectrodes(NeuropixelsV2QuadShankElectrode[] electrodes)
        {
            foreach (var e in electrodes)
            {
                try
                {
                    ChannelMap[e.Channel] = e;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {ChannelMap.Length - 1} are supported.", ex);
                }
            }

            ProbeGroup.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV2eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        [Obsolete("Holding the class has been superseded by an externalized configuration file. Remove in 1.0.0.")]
        [Browsable(false)]
        [Externalizable(false)]
        public NeuropixelsV2eProbeGroup ProbeGroup { get; private set; } = new();

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
