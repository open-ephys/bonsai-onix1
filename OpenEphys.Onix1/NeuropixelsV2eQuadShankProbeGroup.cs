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
                probePlanarContour[2 + i * 5] = new float[2] { ShankOffsetX + (ShankWidthX + ShankPitchX) * i, shankLengthY };
                probePlanarContour[3 + i * 5] = new float[2] { ShankOffsetX + (ShankWidthX + ShankPitchX) * i, shankBaseY };
                probePlanarContour[4 + i * 5] = new float[2] { ShankOffsetX + (ShankWidthX + ShankPitchX) * i + ShankWidthX / 2, shankTipY };
                probePlanarContour[5 + i * 5] = new float[2] { ShankOffsetX + (ShankWidthX + ShankPitchX) * i + ShankWidthX, shankBaseY };
                probePlanarContour[6 + i * 5] = new float[2] { ShankOffsetX + (ShankWidthX + ShankPitchX) * i + ShankWidthX, shankLengthY };
            }

            probePlanarContour[22] = new float[2] { ShankOffsetX * 2 + (ShankWidthX + ShankPitchX) * (numberOfShanks - 1) + ShankWidthX, shankLengthY };
            probePlanarContour[23] = new float[2] { ShankOffsetX * 2 + (ShankWidthX + ShankPitchX) * (numberOfShanks - 1) + ShankWidthX, probeLengthY };
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
                    _ => throw new ArgumentOutOfRangeException($"Too many shanks; expected four shanks, but received {shank} as an index.")
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
                throw new ArgumentOutOfRangeException($"Channel configuration must have {NeuropixelsV2.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            return enabledContacts.Select(c => new NeuropixelsV2QuadShankElectrode(c.Index))
                                  .OrderBy(e => e.Channel)
                                  .ToArray();
        }
    }
}
