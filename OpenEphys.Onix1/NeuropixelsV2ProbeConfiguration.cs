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
    /// Specifies the reference for a Neuropixels 2.0 probe.
    /// </summary>
    public enum NeuropixelsV2ShankReference : uint
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
        Tip4,
        /// <summary>
        /// Specifies that the tip reference of a single-shank probe will be used.
        /// </summary>
        Tip,
        /// <summary>
        /// Specifies that the Ground reference will be used.
        /// </summary>
        Ground
    }

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
    /// Specifies the current probe type.
    /// </summary>
    public enum NeuropixelsV2ProbeType
    {
        /// <summary>
        /// Specifies that there are four shanks.
        /// </summary>
        QuadShank = 0,
        /// <summary>
        /// Specifies that there is one shank.
        /// </summary>
        SingleShank
    }

    /// <summary>
    /// Defines a configuration for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    public class NeuropixelsV2ProbeConfiguration
    {
        private NeuropixelsV2ProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2ProbeType type, NeuropixelsV2ShankReference reference)
        {
            Probe = probe;
            ProbeType = type;
            Reference = reference;
            ProbeGroup = new(ProbeType);
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2ProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2ProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2ProbeConfiguration(NeuropixelsV2ProbeConfiguration probeConfiguration)
        {
            ProbeType = probeConfiguration.ProbeType;
            Reference = probeConfiguration.Reference;
            ProbeGroup = probeConfiguration.ProbeGroup.Clone();
            Probe = probeConfiguration.Probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eProbeGroup"/> channel configuration. The <see cref="ChannelMap"/> is automatically 
        /// generated from the <see cref="ProbeGroup"/>. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2ProbeConfiguration"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        /// <param name="type">The <see cref="ProbeType"/> for this probe.</param>
        [JsonConstructor]
        public NeuropixelsV2ProbeConfiguration(NeuropixelsV2eProbeGroup probeGroup, NeuropixelsV2Probe probe, NeuropixelsV2ProbeType type, NeuropixelsV2ShankReference reference)
        {
            ProbeType = type;
            ProbeGroup = probeGroup.Clone();
            Reference = reference;
            Probe = probe;
        }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV2Probe"/> for this probe.
        /// </summary>
        [Browsable(false)]
        public NeuropixelsV2Probe Probe { get; set; } = NeuropixelsV2Probe.ProbeA;

        /// <summary>
        /// Gets or sets the <see cref="ProbeType"/> for this probe.
        /// </summary>
        [Browsable(false)]
        [Description("Defines the type of probe, differentiated by the number of shanks present.")]
        public NeuropixelsV2ProbeType ProbeType { get; set; } = NeuropixelsV2ProbeType.QuadShank;

        internal static Array FilterNeuropixelsV2ShankReference(NeuropixelsV2ProbeType probeType)
        {
            if (probeType == NeuropixelsV2ProbeType.SingleShank)
            {
                return Enum.GetValues(typeof(NeuropixelsV2ShankReference))
                           .Cast<NeuropixelsV2ShankReference>()
                           .Where(r =>
                           {
                               return r == NeuropixelsV2ShankReference.External
                                      || r == NeuropixelsV2ShankReference.Tip
                                      || r == NeuropixelsV2ShankReference.Ground;
                           })
                           .ToArray();
            }
            else if (probeType == NeuropixelsV2ProbeType.QuadShank)
            {
                return Enum.GetValues(typeof(NeuropixelsV2ShankReference))
                           .Cast<NeuropixelsV2ShankReference>()
                           .Where(r =>
                           {
                               return r == NeuropixelsV2ShankReference.External
                                      || r == NeuropixelsV2ShankReference.Tip1
                                      || r == NeuropixelsV2ShankReference.Tip2
                                      || r == NeuropixelsV2ShankReference.Tip3
                                      || r == NeuropixelsV2ShankReference.Tip4
                                      || r == NeuropixelsV2ShankReference.Ground;
                           })
                           .ToArray();
            }

            throw new InvalidEnumArgumentException("Unknown probe type given.");
        }

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// All electrodes are set to the same reference, which can be  
        /// <see cref="NeuropixelsV2ShankReference.External"/> or any of the tip references 
        /// (<see cref="NeuropixelsV2ShankReference.Tip1"/>, <see cref="NeuropixelsV2ShankReference.Tip2"/>, etc.). 
        /// Setting to <see cref="NeuropixelsV2ShankReference.External"/> will use the external reference, while 
        /// <see cref="NeuropixelsV2ShankReference.Tip1"/> sets the reference to the electrode at the tip of the first shank.
        /// </remarks>
        [Description("Defines what the reference for the probe will be, whether it is external, on a shank tip, or the ground reference.")]
        [TypeConverter(typeof(ReferenceTypeConverter))]
        public NeuropixelsV2ShankReference Reference { get; set; } = NeuropixelsV2ShankReference.External;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public NeuropixelsV2Electrode[] ChannelMap { get => NeuropixelsV2eProbeGroup.ToChannelMap(ProbeGroup, ProbeType); }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(NeuropixelsV2Electrode[] electrodes)
        {
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
        /// Gets the <see cref="NeuropixelsV2eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        public NeuropixelsV2eProbeGroup ProbeGroup { get; private set; } = new(NeuropixelsV2ProbeType.QuadShank);

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
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV2eProbeGroup>(jsonString);
                SelectElectrodes(NeuropixelsV2eProbeGroup.ToChannelMap(ProbeGroup, ProbeType));
            }
        }
    }

    internal class ReferenceTypeConverter : EnumConverter
    {
        public ReferenceTypeConverter()
            : base(typeof(NeuropixelsV2ShankReference))
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance == null)
                return base.GetStandardValues(context);

            var probeConfiguration = (NeuropixelsV2ProbeConfiguration)context.Instance;

            return new StandardValuesCollection(NeuropixelsV2ProbeConfiguration.FilterNeuropixelsV2ShankReference(probeConfiguration.ProbeType));
        }
    }
}
