using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A <see cref="NeuropixelsV2eQuadShankProbeGroup"/> class for NeuropixelsV2e.
    /// </summary>
    public class NeuropixelsV2eQuadShankProbeGroup : NeuropixelsV2eProbeGroup
    {
        const float shankOffsetX = 200f;
        const float shankWidthX = 70f;
        const float shankPitchX = 250f;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2eQuadShankProbeGroup"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor will initialize the new <see cref="NeuropixelsV2eQuadShankProbeGroup"/> with
        /// the default settings for all contacts, including their positions, shapes, and IDs.
        /// </remarks>
        public NeuropixelsV2eQuadShankProbeGroup()
            : base("probeinterface", "0.2.21", DefaultProbes())
        {
        }

        static Probe[] DefaultProbes()
        {
            var probe = new Probe[1];

            const int numberOfShanks = 4;

            probe[0] = new(ProbeNdim.Two,
                           ProbeSiUnits.um,
                           new ProbeAnnotations("Neuropixels 2.0 - multishank", "IMEC"),
                           null,
                           DefaultContactPositions(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                           Probe.DefaultContactPlaneAxes(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                           Probe.DefaultContactShapes(NeuropixelsV2.ElectrodePerShank * numberOfShanks, ContactShape.Square),
                           Probe.DefaultSquareParams(NeuropixelsV2.ElectrodePerShank * numberOfShanks, 12.0f),
                           DefaultProbePlanarContour(),
                           DefaultDeviceChannelIndices(NeuropixelsV2.ChannelCount, NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                           Probe.DefaultContactIds(NeuropixelsV2.ElectrodePerShank * numberOfShanks),
                           DefaultShankIds(NeuropixelsV2.ElectrodePerShank * numberOfShanks));

            return probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2eQuadShankProbeGroup"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked with the <see cref="JsonConstructorAttribute"/>, and is the
        /// entry point for deserializing the JSON data into a C# class.
        /// </remarks>
        /// <param name="specification">String defining the <see cref="ProbeGroup.Specification"/>.</param>
        /// <param name="version">String defining the <see cref="ProbeGroup.Version"/>.</param>
        /// <param name="probes">Array of <see cref="Probe">Probes</see>.</param>
        [JsonConstructor]
        public NeuropixelsV2eQuadShankProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
        }

        /// <summary>
        /// Copy constructor that initializes a copied instance of the <see cref="NeuropixelsV2eQuadShankProbeGroup"/> class.
        /// </summary>
        /// <param name="probeGroup">An existing <see cref="NeuropixelsV2eQuadShankProbeGroup"/> object.</param>
        public NeuropixelsV2eQuadShankProbeGroup(NeuropixelsV2eQuadShankProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        internal override NeuropixelsV2eProbeGroup Clone()
        {
            return new NeuropixelsV2eQuadShankProbeGroup(Specification, Version, Probes.Select(probe => new Probe(probe)).ToArray());
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
            var offset = shankOffsetX + shankPitchX * shank + 11;

            return (index % 2) switch
            {
                0 => offset + 8.0f,
                1 => offset + 40.0f,
                _ => throw new ArgumentException("Invalid index given.")
            };
        }

        /// <summary>
        /// Generates a default planar contour for the type of probe that is given.
        /// </summary>
        /// <returns></returns>
        public static float[][] DefaultProbePlanarContour()
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
                probePlanarContour[2 + i * 5] = new float[2] { shankOffsetX + shankPitchX * i, shankLengthY };
                probePlanarContour[3 + i * 5] = new float[2] { shankOffsetX + shankPitchX * i, shankBaseY };
                probePlanarContour[4 + i * 5] = new float[2] { shankOffsetX + shankPitchX * i + shankWidthX / 2, shankTipY };
                probePlanarContour[5 + i * 5] = new float[2] { shankOffsetX + shankPitchX * i + shankWidthX, shankBaseY };
                probePlanarContour[6 + i * 5] = new float[2] { shankOffsetX + shankPitchX * i + shankWidthX, shankLengthY };
            }

            probePlanarContour[22] = new float[2] { shankOffsetX * 2 + shankPitchX * (numberOfShanks - 1) + shankWidthX, shankLengthY };
            probePlanarContour[23] = new float[2] { shankOffsetX * 2 + shankPitchX * (numberOfShanks - 1) + shankWidthX, probeLengthY };
            probePlanarContour[24] = new float[2] { 0f, probeLengthY };

            return probePlanarContour;
        }

        /// <summary>
        /// Generates the first <paramref name="electrodeCount"/> default device channel indices, and
        /// marks the rest as -1 to indicate they are not actively recorded.
        /// </summary>
        /// <param name="channelCount">Number of contacts that are connected for recording.</param>
        /// <param name="electrodeCount">Total number of physical contacts on the probe.</param>
        /// <returns></returns>
        public static int[] DefaultDeviceChannelIndices(int channelCount, int electrodeCount)
        {
            int[] deviceChannelIndices = new int[electrodeCount];

            for (int i = 0; i < channelCount; i++)
            {
                deviceChannelIndices[i] = NeuropixelsV2QuadShankElectrode.GetChannelNumber(i);
            }

            for (int i = channelCount; i < electrodeCount; i++)
            {
                deviceChannelIndices[i] = -1;
            }

            return deviceChannelIndices;
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
        /// <returns>List of <see cref="NeuropixelsV2QuadShankElectrode"/> electrodes.</returns>
        public override List<NeuropixelsV2Electrode> ToElectrodes()
        {
            List<NeuropixelsV2Electrode> electrodes = new();

            foreach (var c in GetContacts())
            {
                electrodes.Add(new NeuropixelsV2QuadShankElectrode(c.Index));
            }

            return electrodes;
        }

        /// <summary>
        /// Convert a <see cref="NeuropixelsV2eQuadShankProbeGroup"/> object to a list of electrodes,
        /// which only includes currently enabled electrodes.
        /// </summary>
        /// <param name="probeGroup">A <see cref="NeuropixelsV2eQuadShankProbeGroup"/> object.</param>
        /// <returns>List of <see cref="NeuropixelsV2QuadShankElectrode"/> electrodes that are enabled.</returns>
        public static NeuropixelsV2QuadShankElectrode[] ToChannelMap(NeuropixelsV2eQuadShankProbeGroup probeGroup)
        {
            var enabledContacts = probeGroup.GetContacts().Where(c => c.DeviceId != -1);

            if (enabledContacts.Count() != NeuropixelsV2.ChannelCount)
            {
                throw new InvalidOperationException($"Channel configuration must have {NeuropixelsV2.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            return enabledContacts.Select(c => new NeuropixelsV2QuadShankElectrode(c.Index))
                                  .OrderBy(e => e.Channel)
                                  .ToArray();
        }
    }
}
