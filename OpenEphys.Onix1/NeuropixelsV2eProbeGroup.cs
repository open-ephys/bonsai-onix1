using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Abstract class that defines a Neuropixels 2.0 Probe Group
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2eQuadShankProbeGroup))]
    [XmlInclude(typeof(NeuropixelsV2eSingleShankProbeGroup))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2eProbeGroup : ProbeGroup
    {
        /// <summary>
        /// Shank X-axis offset.
        /// </summary>
        protected const float shankOffsetX = 200f;

        /// <summary>
        /// Shank X-axis width.
        /// </summary>
        protected const float shankWidthX = 70f;

        /// <summary>
        /// Shank X-axis pitch.
        /// </summary>
        protected const float shankPitchX = 250f;

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
        /// Generates a 2D array of default contact positions based on the given number of channels.
        /// </summary>
        /// <param name="numberOfContacts">Value defining the number of contacts to create positions for.</param>
        /// <returns>
        /// 2D array of floats [N x 2], where the first dimension is the contact index [N] and the second dimension [2]
        /// contains the X and Y values, respectively.
        /// </returns>
        public static float[][] DefaultContactPositions(int numberOfContacts)
        {
            float[][] contactPositions = new float[numberOfContacts][];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactPositions[i] = DefaultContactPosition(i);
            }

            return contactPositions;
        }

        /// <summary>
        /// Generates a float array containing the X and Y position of a single contact.
        /// </summary>
        /// <param name="contactIndex">Index of the contact.</param>
        /// <returns>A float array of size [2 x 1] with the X and Y coordinates, respectively.</returns>
        public static float[] DefaultContactPosition(int contactIndex)
        {
            return new float[2] { ContactPositionX(contactIndex), contactIndex % NeuropixelsV2.ElectrodePerShank / 2 * 15 + 170 };
        }

        private static float ContactPositionX(int index)
        {
            var shank = index / NeuropixelsV2.ElectrodePerShank;
            var offset = shankOffsetX + (shankWidthX + shankPitchX) * shank + 11;

            return (index % 2) switch
            {
                0 => offset + 8.0f,
                1 => offset + 40.0f,
                _ => throw new ArgumentException("Invalid index given.")
            };
        }

        /// <summary>
        /// Generates an array of strings with the shank value as the default shank ID.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single probe.</param>
        /// <returns></returns>
        public static string[] DefaultShankIds(int numberOfContacts)
        {
            string[] contactIds = new string[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                var shank = i / NeuropixelsV2.ElectrodePerShank;
                contactIds[i] = shank switch
                {
                    0 => "0",
                    1 => "1",
                    2 => "2",
                    3 => "3",
                    _ => throw new InvalidOperationException($"Too many shanks; expected four shanks, but received {shank} as an index.")
                };
            }

            return contactIds;
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes.
        /// </summary>
        /// <returns>List of <see cref="NeuropixelsV2Electrode"/> electrodes</returns>
        public abstract List<NeuropixelsV2Electrode> ToElectrodes();
    }
}
