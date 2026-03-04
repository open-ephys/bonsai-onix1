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
    public enum NeuropixelsV2SingleShankReference : uint
    {
        /// <summary>
        /// Specifies that the External reference will be used.
        /// </summary>
        External,
        /// <summary>
        /// Specifies that the tip reference will be used.
        /// </summary>
        Tip,
        /// <summary>
        /// Specifies that the Ground reference will be used.
        /// </summary>
        Ground
    }

    /// <summary>
    /// Defines a configuration for single-shank, Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class NeuropixelsV2SingleShankProbeConfiguration : NeuropixelsV2ProbeConfiguration
    {
        const string XmlTypeName = nameof(NeuropixelsV2SingleShankProbeConfiguration);

        /// <summary>
        /// Initializes a default instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2SingleShankProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2SingleShankProbeConfiguration(NeuropixelsV2SingleShankReference reference)
        {
            Reference = reference;
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2SingleShankProbeConfiguration(NeuropixelsV2SingleShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            InvertPolarity = probeConfiguration.InvertPolarity;
            GainCalibrationFileName = probeConfiguration.GainCalibrationFileName;
            ProbeInterfaceFileName = probeConfiguration.ProbeInterfaceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eSingleShankProbeGroup"/> channel configuration. 
        /// </summary>
        /// <param name="reference">The <see cref="NeuropixelsV2SingleShankReference"/> reference value.</param>
        /// <param name="invertPolarity">Boolean defining if the signal polarity should be inverted.</param>
        /// <param name="gainCalibrationFileName">String defining the path to the gain calibration file.</param>
        /// <param name="probeInterfaceFileName">String defining the path to the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2SingleShankProbeConfiguration(
            NeuropixelsV2SingleShankReference reference,
            bool invertPolarity,
            string gainCalibrationFileName,
            string probeInterfaceFileName)
        {
            Reference = reference;
            InvertPolarity = invertPolarity;
            GainCalibrationFileName = gainCalibrationFileName;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        internal override NeuropixelsV2ProbeConfiguration Clone()
        {
            return new NeuropixelsV2SingleShankProbeConfiguration(this);
        }

        NeuropixelsV2SingleShankReference reference;

        /// <inheritdoc/>
        /// <remarks>
        /// The available references for a single-shank probe are <see cref="NeuropixelsV2SingleShankReference"/>.
        /// </remarks>
        [XmlIgnore]
        [Description("Defines the reference for the probe.")]
        [TypeConverter(typeof(NeuropixelsV2SingleShankReferenceConverter))]
        public override Enum Reference
        {
            get => reference;
            set => reference = (NeuropixelsV2SingleShankReference)value;
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
                    Reference = NeuropixelsV2SingleShankReference.External;
                    return;
                }

                Reference = Enum.TryParse<NeuropixelsV2SingleShankReference>(value, out var result)
                            ? result
                            : NeuropixelsV2SingleShankReference.External;
            }
        }

        internal override BitArray[] CreateShankBits(Enum reference)
        {
            BitArray[] shankBits;

            shankBits = new BitArray[]
            {
                new(RegistersPerShank, false)
            };
            const int Shank = 0;

            NeuropixelsV2SingleShankReference singleShankReference = (NeuropixelsV2SingleShankReference)reference;

            if (singleShankReference == NeuropixelsV2SingleShankReference.Tip)
            {
                shankBits[Shank][ShiftRegisterBitTipElectrode1] = true;
                shankBits[Shank][ShiftRegisterBitTipElectrode0] = true;
            }
            else if (singleShankReference == NeuropixelsV2SingleShankReference.External)
            {
                shankBits[Shank][ShiftRegisterBitExternalElectrode0] = true;
                shankBits[Shank][ShiftRegisterBitExternalElectrode1] = true;
            }

            return shankBits;
        }

        internal override int GetReferenceBit(Enum reference)
        {
            var singleShankReference = (NeuropixelsV2SingleShankReference)reference;

            return singleShankReference switch
            {
                NeuropixelsV2SingleShankReference.External => 1,
                NeuropixelsV2SingleShankReference.Tip => 2,
                NeuropixelsV2SingleShankReference.Ground => 3,
                _ => throw new InvalidEnumArgumentException($"Invalid {nameof(NeuropixelsV2SingleShankReference)} selection."),
            };
        }

        internal override bool IsGroundReference() => (NeuropixelsV2SingleShankReference)Reference == NeuropixelsV2SingleShankReference.Ground;

        internal override int GetChannelNumber(int index)
        {
            return NeuropixelsV2SingleShankElectrode.GetChannelNumber(index);
        }
    }

    /// <summary>
    /// Provides a type converter for <see cref="NeuropixelsV2SingleShankReference"/> values.
    /// </summary>
    public class NeuropixelsV2SingleShankReferenceConverter : EnumConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankReferenceConverter"/> class with
        /// the <see cref="NeuropixelsV2SingleShankReference"/> type.
        /// </summary>
        public NeuropixelsV2SingleShankReferenceConverter() : base(typeof(NeuropixelsV2SingleShankReference)) { }
    }
}
