using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the reference for a Neuropixels 2.0 probe.
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
        Tip4,
        /// <summary>
        /// Specifies that the Ground reference will be used.
        /// </summary>
        Ground
    }

    /// <summary>
    /// Defines a configuration for quad-shank, Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class NeuropixelsV2QuadShankProbeConfiguration : NeuropixelsV2ProbeConfiguration
    {
        internal const string XmlTypeName = nameof(NeuropixelsV2QuadShankProbeConfiguration);

        /// <summary>
        /// Initializes a default instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference)
        {
            Probe = probe;
            Reference = reference;
            ProbeGroup = new NeuropixelsV2eQuadShankProbeGroup();
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            ProbeGroup = probeConfiguration.ProbeGroup.Clone();
            Probe = probeConfiguration.Probe;
            InvertPolarity = probeConfiguration.InvertPolarity;
            GainCalibrationFileName = probeConfiguration.GainCalibrationFileName;
            probeInterfaceFileName = probeConfiguration.probeInterfaceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// values.
        /// </summary>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="invertPolarity">Boolean defining if the signal polarity should be inverted.</param>
        /// <param name="gainCalibrationFileName">String defining the path to the gain calibration file.</param>
        /// <param name="probeInterfaceFileName">String defining the path to the ProbeInterface file.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(
            NeuropixelsV2Probe probe,
            NeuropixelsV2QuadShankReference reference,
            bool invertPolarity,
            string gainCalibrationFileName,
            string probeInterfaceFileName)
        {
            Probe = probe;
            Reference = reference;
            InvertPolarity = invertPolarity;
            GainCalibrationFileName = gainCalibrationFileName;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eQuadShankProbeGroup"/> channel configuration. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eQuadShankProbeGroup"/> instance to use.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="invertPolarity">Boolean defining if the signal polarity should be inverted.</param>
        /// <param name="gainCalibrationFileName">String defining the path to the gain calibration file.</param>
        /// <param name="probeInterfaceFileName">String defining the path to the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(
            NeuropixelsV2eQuadShankProbeGroup probeGroup,
            NeuropixelsV2Probe probe,
            NeuropixelsV2QuadShankReference reference,
            bool invertPolarity,
            string gainCalibrationFileName,
            string probeInterfaceFileName)
        {
            ProbeGroup = probeGroup.Clone();
            Reference = reference;
            Probe = probe;
            InvertPolarity = invertPolarity;
            GainCalibrationFileName = gainCalibrationFileName;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        internal override NeuropixelsV2ProbeConfiguration Clone()
        {
            return new NeuropixelsV2QuadShankProbeConfiguration(this);
        }

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public override NeuropixelsV2Electrode[] ChannelMap
        {
            get => NeuropixelsV2eQuadShankProbeGroup.ToChannelMap((NeuropixelsV2eQuadShankProbeGroup)ProbeGroup);
        }

        NeuropixelsV2QuadShankReference reference;

        /// <inheritdoc/>
        /// <remarks>
        /// The available references for a quad-shank probe are <see cref="NeuropixelsV2QuadShankReference"/>.
        /// </remarks>
        [XmlIgnore]
        [Description("Defines the reference for the probe.")]
        [TypeConverter(typeof(NeuropixelsV2QuadShankReferenceConverter))]
        public override Enum Reference
        {
            get => reference;
            set => reference = (NeuropixelsV2QuadShankReference)value;
        }

        /// <inheritdoc/>
        [XmlElement(nameof(Reference))]
        [Browsable(false)]
        [Externalizable(false)]
        public override string ReferenceSerialized
        {
            get => Reference.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Reference = NeuropixelsV2QuadShankReference.External;
                    return;
                }

                Reference = Enum.TryParse<NeuropixelsV2QuadShankReference>(value, out var result)
                            ? result
                            : NeuropixelsV2QuadShankReference.External;
            }
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [Browsable(false)]
        [Externalizable(false)]
        public override string ProbeInterfaceLoadFileName
        {
            get => probeInterfaceFileName;
            set
            {
                probeInterfaceFileName = value;
                probeGroupTask = Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(probeInterfaceFileName))
                        return new NeuropixelsV2eQuadShankProbeGroup();

                    return ProbeInterfaceHelper.LoadExternalProbeInterfaceFile(probeInterfaceFileName, typeof(NeuropixelsV2eQuadShankProbeGroup)) as NeuropixelsV2eProbeGroup;
                });
            }
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming.")]
        [Browsable(false)]
        [Externalizable(false)]
        public override NeuropixelsV2eProbeGroup ProbeGroup
        {
            get
            {
                if (probeGroup == null)
                {
                    try
                    {
                        probeGroup = probeGroupTask?.Result ?? new NeuropixelsV2eQuadShankProbeGroup();
                    }
                    catch (AggregateException ae)
                    {
                        probeGroup = new NeuropixelsV2eQuadShankProbeGroup();
                        throw new InvalidOperationException($"There was an error loading the ProbeInterface file, loading the default configuration instead.\n\nError: {ae.InnerException.Message}", ae.InnerException);
                    }
                }

                return probeGroup;
            }
            set => probeGroup = value;
        }

        /// <inheritdoc/>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public override string ProbeGroupString
        {
            get
            {
                var jsonString = JsonConvert.SerializeObject(ProbeGroup);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV2eQuadShankProbeGroup>(jsonString);
            }
        }

        internal override BitArray[] CreateShankBits(Enum reference)
        {
            var shankBits = new BitArray[]
                {
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false),
                    new(RegistersPerShank, false)
                };

            NeuropixelsV2QuadShankReference quadShankReference = (NeuropixelsV2QuadShankReference)reference;

            if (quadShankReference != NeuropixelsV2QuadShankReference.External && quadShankReference != NeuropixelsV2QuadShankReference.Ground)
            {
                var shank = reference switch
                {
                    NeuropixelsV2QuadShankReference.Tip1 => 0,
                    NeuropixelsV2QuadShankReference.Tip2 => 1,
                    NeuropixelsV2QuadShankReference.Tip3 => 2,
                    NeuropixelsV2QuadShankReference.Tip4 => 3,
                    _ => throw new InvalidEnumArgumentException("Invalid reference chosen for quad-shank probe.")
                };

                // If tip reference is used, activate the tip electrode
                shankBits[shank][ShiftRegisterBitTipElectrode1] = true;
                shankBits[shank][ShiftRegisterBitTipElectrode0] = true;
            }
            else if (quadShankReference == NeuropixelsV2QuadShankReference.External)
            {
                // TODO: is this the right approach or should only those
                // connections to external reference on shanks with active
                // electrodes be activated?

                // If external electrode is used, activate on each shank
                shankBits[0][ShiftRegisterBitExternalElectrode1] = true;
                shankBits[0][ShiftRegisterBitExternalElectrode0] = true;
                shankBits[1][ShiftRegisterBitExternalElectrode1] = true;
                shankBits[1][ShiftRegisterBitExternalElectrode0] = true;
                shankBits[2][ShiftRegisterBitExternalElectrode1] = true;
                shankBits[2][ShiftRegisterBitExternalElectrode0] = true;
                shankBits[3][ShiftRegisterBitExternalElectrode1] = true;
                shankBits[3][ShiftRegisterBitExternalElectrode0] = true;
            }

            return shankBits;
        }

        internal override int GetReferenceBit(Enum reference)
        {
            var quadShankReference = (NeuropixelsV2QuadShankReference)reference;

            return quadShankReference switch
            {
                NeuropixelsV2QuadShankReference.External => 1,
                NeuropixelsV2QuadShankReference.Tip1 => 2,
                NeuropixelsV2QuadShankReference.Tip2 => 2,
                NeuropixelsV2QuadShankReference.Tip3 => 2,
                NeuropixelsV2QuadShankReference.Tip4 => 2,
                NeuropixelsV2QuadShankReference.Ground => 3,
                _ => throw new InvalidOperationException("Invalid reference selection."),
            };
        }

        internal override bool IsGroundReference() => (NeuropixelsV2QuadShankReference)Reference == NeuropixelsV2QuadShankReference.Ground;

        internal override int GetChannelNumber(int index)
        {
            return NeuropixelsV2QuadShankElectrode.GetChannelNumber(index);
        }
    }

    /// <summary>
    /// Provides a type converter for <see cref="NeuropixelsV2QuadShankReference"/> values.
    /// </summary>
    public class NeuropixelsV2QuadShankReferenceConverter : EnumConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankReferenceConverter"/> class with
        /// the <see cref="NeuropixelsV2QuadShankReference"/> type.
        /// </summary>
        public NeuropixelsV2QuadShankReferenceConverter() : base(typeof(NeuropixelsV2QuadShankReference)) { }
    }
}
