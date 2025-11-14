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
        const string XmlTypeName = "QuadShank";

        /// <summary>
        /// Initializes a default instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eQuadShankProbeGroup"/> channel configuration. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eQuadShankProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2eQuadShankProbeGroup probeGroup, NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference)
        {
            ProbeGroup = probeGroup.Clone();
            Reference = reference;
            Probe = probe;
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

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public override void SelectElectrodes(NeuropixelsV2Electrode[] electrodes)
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

        internal override BitArray[] CreateShankBits(Enum reference)
        {
            const int ReferencePixelCount = 4;
            const int DummyRegisterCount = 4;
            const int RegistersPerShank = NeuropixelsV2.ElectrodePerShank + ReferencePixelCount + DummyRegisterCount;

            const int ShiftRegisterBitExternalElectrode0 = 1285;
            const int ShiftRegisterBitExternalElectrode1 = 2;

            const int ShiftRegisterBitTipElectrode0 = 644;
            const int ShiftRegisterBitTipElectrode1 = 643;

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
                    _ => throw new InvalidOperationException($"Invalid reference chosen for quad-shank probe.")
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
    }
}
