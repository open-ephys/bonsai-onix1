using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Abstract class that defines a Neuropixels 2.0 Probe Group
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2eQuadShankProbeGroup))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2eProbeGroup : ProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eProbeGroup"/>.
        /// </summary>
        /// <inheritdoc/>
        public NeuropixelsV2eProbeGroup(string specification, string version, IEnumerable<Probe> probes)
            : base(specification, version, probes)
        {
        }

        /// <summary>
        /// Copy constructor that initializes a copied instance of the <see cref="NeuropixelsV2eProbeGroup"/> class.
        /// </summary>
        /// <param name="probeGroup">Existing <see cref="NeuropixelsV2eProbeGroup"/> object.</param>
        public NeuropixelsV2eProbeGroup(NeuropixelsV2eProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        internal abstract NeuropixelsV2eProbeGroup Clone();

        internal void UpdateDeviceChannelIndices(NeuropixelsV2Electrode[] channelMap)
        {
            int[] newDeviceChannelIndices = new int[NumberOfContacts];

            for (int i = 0; i < newDeviceChannelIndices.Length; i++)
            {
                newDeviceChannelIndices[i] = -1;
            }

            foreach (var e in channelMap)
            {
                newDeviceChannelIndices[e.Index] = e.Channel;
            }

            UpdateDeviceChannelIndices(0, newDeviceChannelIndices);
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes.
        /// </summary>
        /// <returns>List of <see cref="NeuropixelsV2Electrode"/> electrodes</returns>
        public abstract List<NeuropixelsV2Electrode> ToElectrodes();
    }
}
