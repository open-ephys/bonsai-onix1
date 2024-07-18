using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2eProbeGroup : ProbeGroup
    {
        const float shankOffsetX = 200f;
        const float shankWidthX = 70f;
        const float shankPitchX = 250f;
        const int numberOfShanks = 4;

        public NeuropixelsV2eProbeGroup()
            : base("probeinterface", "0.2.21",
                  new List<Probe>()
                  {
                      new(ProbeNdim.Two,
                          ProbeSiUnits.um,
                          new ProbeAnnotations("Neuropixels 2.0e", "IMEC"),
                          new ContactAnnotations(new string[0]),
                          DefaultContactPositions(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                          Probe.DefaultContactPlaneAxes(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                          Probe.DefaultContactShapes(NeuropixelsV2.ElectrodePerShank * numberOfShanks, ContactShape.Square),
                          Probe.DefaultSquareParams(NeuropixelsV2.ElectrodePerShank * numberOfShanks, 12.0f),
                          DefaultProbePlanarContourQuadShank(),
                          DefaultDeviceChannelIndices(NeuropixelsV2.ChannelCount, NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                          Probe.DefaultContactIds(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                          DefaultShankIds(NeuropixelsV2.ElectrodePerShank * numberOfShanks))
                  }.ToArray())
        {
        }

        [JsonConstructor]
        public NeuropixelsV2eProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
        }

        public NeuropixelsV2eProbeGroup(NeuropixelsV2eProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        public static float[][] DefaultContactPositions(int numberOfChannels)
        {
            if (numberOfChannels % 2 != 0)
            {
                throw new ArgumentException("Invalid number of channels given; must be a multiple of two");
            }

            float[][] contactPositions = new float[numberOfChannels][];

            for (int i = 0; i < numberOfChannels; i++)
            {
                contactPositions[i] = DefaultContactPosition(i);
            }

            return contactPositions;
        }

        public static float[] DefaultContactPosition(int index)
        {
            return new float[2] { ContactPositionX(index), index % NeuropixelsV2.ElectrodePerShank / 2 * 15 + 170 };
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
        /// Generates a default planar contour for the probe, based on the given probe index
        /// </summary>
        /// <returns></returns>
        public static float[][] DefaultProbePlanarContourQuadShank()
        {
            const int numberOfShanks = 4;
            const float shankTipY = 0f;
            const float shankBaseY = 155f;
            const float shankLengthY = 10000f;
            const float probeLengthY = 10155f;

            float[][] probePlanarContour = new float[25][];

            probePlanarContour[0] = new float[2] { 0f, probeLengthY };
            probePlanarContour[1] = new float[2] { 0f, shankLengthY };

            for (int i = 0; i < numberOfShanks; i++)
            {
                probePlanarContour[2 + i * 5] = new float[2] { shankOffsetX + (shankWidthX + shankPitchX) * i, shankLengthY };
                probePlanarContour[3 + i * 5] = new float[2] { shankOffsetX + (shankWidthX + shankPitchX) * i, shankBaseY };
                probePlanarContour[4 + i * 5] = new float[2] { shankOffsetX + (shankWidthX + shankPitchX) * i + shankWidthX / 2, shankTipY };
                probePlanarContour[5 + i * 5] = new float[2] { shankOffsetX + (shankWidthX + shankPitchX) * i + shankWidthX, shankBaseY };
                probePlanarContour[6 + i * 5] = new float[2] { shankOffsetX + (shankWidthX + shankPitchX) * i + shankWidthX, shankLengthY };
            }

            probePlanarContour[22] = new float[2] { shankOffsetX * 2 + (shankWidthX + shankPitchX) * (numberOfShanks - 1) + shankWidthX, shankLengthY };
            probePlanarContour[23] = new float[2] { shankOffsetX * 2 + (shankWidthX + shankPitchX) * (numberOfShanks - 1) + shankWidthX, probeLengthY };
            probePlanarContour[24] = new float[2] { 0f, probeLengthY };

            return probePlanarContour;
        }

        /// <summary>
        /// Generates a default planar contour for the probe, based on the given probe index
        /// </summary>
        /// <returns></returns>
        public static float[][] DefaultProbePlanarContourSingleShank()
        {
            float[][] probePlanarContour = new float[6][];

            probePlanarContour[0] = new float[2] { -11f, 155f };
            probePlanarContour[1] = new float[2] { 24f, 0f };
            probePlanarContour[2] = new float[2] { 59f, 155f };
            probePlanarContour[3] = new float[2] { 59f, 10000f };
            probePlanarContour[4] = new float[2] { -11f, 10000f };
            probePlanarContour[5] = new float[2] { -11f, 155f };

            return probePlanarContour;
        }

        /// <summary>
        /// Override of the DefaultDeviceChannelIndices function, which initializes a portion of the
        /// device channel indices, and leaves the rest at -1 to indicate they are not actively recorded
        /// </summary>
        /// <param name="channelCount">Number of contacts that are connected for recording</param>
        /// <param name="electrodeCount">Total number of physical contacts on the probe</param>
        /// <returns></returns>
        public static int[] DefaultDeviceChannelIndices(int channelCount, int electrodeCount)
        {
            int[] deviceChannelIndices = new int[electrodeCount];

            for (int i = 0; i < channelCount; i++)
            {
                deviceChannelIndices[i] = NeuropixelsV2QuadShankElectrode.GetChannelNumber(i / NeuropixelsV2.ElectrodePerShank,
                                                                                           i % NeuropixelsV2.ChannelCount / NeuropixelsV2.ElectrodePerBlock,
                                                                                           i % NeuropixelsV2.ElectrodePerBlock);
            }

            for (int i = channelCount; i < electrodeCount; i++)
            {
                deviceChannelIndices[i] = -1;
            }

            return deviceChannelIndices;
        }

        /// <summary>
        /// Generates an array of strings with the value "0" as the default shank ID
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single probe</param>
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
                    _ => throw new InvalidOperationException($"Too many shanks; expected four or less zero-indexed shanks, but received {shank} as an index.")
                };
            }

            return contactIds;
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV2eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV2QuadShankElectrode"/> electrodes</returns>
        public static List<NeuropixelsV2QuadShankElectrode> ToElectrodes(NeuropixelsV2eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV2QuadShankElectrode> electrodes = new();

            foreach (var c in channelConfiguration.GetContacts())
            {
                electrodes.Add(new NeuropixelsV2QuadShankElectrode(c));
            }

            return electrodes;
        }

        public static void UpdateElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes, NeuropixelsV2eProbeGroup channelConfiguration)
        {
            if (electrodes.Count != channelConfiguration.NumberOfContacts)
            {
                throw new InvalidOperationException($"Different number of electrodes found in {nameof(electrodes)} versus {nameof(channelConfiguration)}");
            }

            int index = 0;

            foreach (var c in channelConfiguration.GetContacts())
            {
                electrodes[index++] = new NeuropixelsV2QuadShankElectrode(c);
            }
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which only includes currently enabled electrodes
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV2eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV2QuadShankElectrode"/> electrodes that are enabled</returns>
        public static List<NeuropixelsV2QuadShankElectrode> ToChannelMap(NeuropixelsV2eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV2QuadShankElectrode> channelMap = new();

            var enabledContacts = channelConfiguration.GetContacts().Where(c => c.DeviceId != -1);

            if (enabledContacts.Count() != NeuropixelsV2.ChannelCount)
            {
                throw new InvalidOperationException($"Channel configuration must have {NeuropixelsV2.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            foreach (var c in enabledContacts)
            {
                channelMap.Add(new NeuropixelsV2QuadShankElectrode(c));
            }

            return channelMap.OrderBy(e => e.Channel).ToList();
        }

        public static void UpdateChannelMap(List<NeuropixelsV2QuadShankElectrode> channelMap, NeuropixelsV2eProbeGroup channelConfiguration)
        {
            var enabledElectrodes = channelConfiguration.GetContacts()
                                                        .Where(c => c.DeviceId != -1);

            if (channelMap.Count != enabledElectrodes.Count())
            {
                throw new InvalidOperationException($"Different number of enabled electrodes found in {nameof(channelMap)} versus {nameof(channelConfiguration)}");
            }

            int index = 0;

            foreach (var c in enabledElectrodes)
            {
                channelMap[index++] = new NeuropixelsV2QuadShankElectrode(c);
            }
        }

        /// <summary>
        /// Update the currently enabled contacts in the probe group, based on the currently selected contacts in 
        /// the given channel map. The only operation that occurs is an update of the DeviceChannelIndices field,
        /// where -1 indicates the contact is no longer enabled
        /// </summary>
        /// <param name="channelMap">List of <see cref="NeuropixelsV2QuadShankElectrode"/> objects, which contain the index of the selected contact</param>
        /// <param name="probeGroup"><see cref="NeuropixelsV2eProbeGroup"/></param>
        public static void UpdateProbeGroup(List<NeuropixelsV2QuadShankElectrode> channelMap, NeuropixelsV2eProbeGroup probeGroup)
        {
            int[] deviceChannelIndices = new int[probeGroup.NumberOfContacts];

            deviceChannelIndices = deviceChannelIndices.Select(i => i = -1).ToArray();

            foreach (var e in channelMap)
            {
                deviceChannelIndices[e.ElectrodeNumber] = e.Channel;
            }

            probeGroup.UpdateDeviceChannelIndices(0, deviceChannelIndices);
        }
    }
}
