using System;
using System.Collections;
using System.ComponentModel;
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
        const string XmlTypeName = nameof(NeuropixelsV2QuadShankProbeConfiguration);

        /// <summary>
        /// Initializes a default instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankReference reference)
        {
            Reference = reference;
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            InvertPolarity = probeConfiguration.InvertPolarity;
            GainCalibrationFileName = probeConfiguration.GainCalibrationFileName;
            ProbeInterfaceFileName = probeConfiguration.ProbeInterfaceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eQuadShankProbeGroup"/> channel configuration. 
        /// </summary>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="invertPolarity">Boolean defining if the signal polarity should be inverted.</param>
        /// <param name="gainCalibrationFileName">String defining the path to the gain calibration file.</param>
        /// <param name="probeInterfaceFileName">String defining the path to the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(
            NeuropixelsV2QuadShankReference reference,
            bool invertPolarity,
            string gainCalibrationFileName,
            string probeInterfaceFileName)
        {
            Reference = reference;
            InvertPolarity = invertPolarity;
            GainCalibrationFileName = gainCalibrationFileName;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        internal override NeuropixelsV2ProbeConfiguration Clone() // TODO: is this needed?
        {
            return new NeuropixelsV2QuadShankProbeConfiguration(this);
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
                _ => throw new InvalidEnumArgumentException($"Invalid {nameof(NeuropixelsV2QuadShankReference)} selection."),
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
