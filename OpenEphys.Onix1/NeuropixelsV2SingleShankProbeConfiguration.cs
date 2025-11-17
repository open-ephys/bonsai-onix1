using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
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
        internal const string XmlTypeName = "SingleShank";

        /// <summary>
        /// Initializes a default instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2SingleShankProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2SingleShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2SingleShankReference reference)
        {
            Probe = probe;
            Reference = reference;
            ProbeGroup = new NeuropixelsV2eSingleShankProbeGroup();
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2SingleShankProbeConfiguration(NeuropixelsV2SingleShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            ProbeGroup = probeConfiguration.ProbeGroup.Clone();
            Probe = probeConfiguration.Probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eSingleShankProbeGroup"/> channel configuration. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eSingleShankProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2SingleShankReference"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        [JsonConstructor]
        public NeuropixelsV2SingleShankProbeConfiguration(NeuropixelsV2eSingleShankProbeGroup probeGroup, NeuropixelsV2Probe probe, NeuropixelsV2SingleShankReference reference)
        {
            ProbeGroup = probeGroup.Clone();
            Reference = reference;
            Probe = probe;
        }

        internal override NeuropixelsV2ProbeConfiguration Clone()
        {
            return new NeuropixelsV2SingleShankProbeConfiguration(this);
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
            get => NeuropixelsV2eSingleShankProbeGroup.ToChannelMap((NeuropixelsV2eSingleShankProbeGroup)ProbeGroup);
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
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV2eSingleShankProbeGroup>(jsonString);
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
                _ => throw new InvalidOperationException("Invalid reference selection."),
            };
        }

        internal override bool IsGroundReference() => (NeuropixelsV2SingleShankReference)Reference == NeuropixelsV2SingleShankReference.Ground;

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

        internal override Func<int, int> GetChannelNumberFunc()
        {
            return NeuropixelsV2SingleShankElectrode.GetChannelNumber;
        }
    }
}
