using System;
using System.Collections.Generic;
using OpenEphys.ProbeInterface.NET;
using Newtonsoft.Json;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A <see cref="ProbeGroup"/> class for NeuropixelsV1e.
    /// </summary>
    public class NeuropixelsV1eProbeGroup : ProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eProbeGroup"/> class.
        /// </summary>
        public NeuropixelsV1eProbeGroup()
            : base("probeinterface", "0.2.21",
                  new List<Probe>()
                  {
                      new(ProbeNdim.Two,
                          ProbeSiUnits.um,
                          new ProbeAnnotations("Neuropixels 1.0", "IMEC"),
                          new ContactAnnotations(new string[0]),
                          DefaultContactPositions(NeuropixelsV1.ElectrodeCount),
                          Probe.DefaultContactPlaneAxes(NeuropixelsV1.ElectrodeCount),
                          Probe.DefaultContactShapes(NeuropixelsV1.ElectrodeCount, ContactShape.Square),
                          Probe.DefaultSquareParams(NeuropixelsV1.ElectrodeCount, 12.0f),
                          DefaultProbePlanarContour(),
                          DefaultDeviceChannelIndices(NeuropixelsV1.ChannelCount, NeuropixelsV1.ElectrodeCount),
                          Probe.DefaultContactIds(NeuropixelsV1.ElectrodeCount),
                          DefaultShankIds(NeuropixelsV1.ElectrodeCount))
                  }.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eProbeGroup"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked with the <see cref="JsonConstructorAttribute"/>, and is the
        /// entry point for deserializing the JSON data into a C# class.
        /// </remarks>
        /// <param name="specification">String defining the <see cref="ProbeGroup.Specification"/>.</param>
        /// <param name="version">String defining the <see cref="ProbeGroup.Version"/>.</param>
        /// <param name="probes">Array of <see cref="Probe"/>s.</param>
        [JsonConstructor]
        public NeuropixelsV1eProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
        }

        /// <summary>
        /// Copy constructor that initializes a copied instance of the <see cref="NeuropixelsV1eProbeGroup"/> class.
        /// </summary>
        /// <param name="probeGroup">An existing <see cref="NeuropixelsV1eProbeGroup"/> instance.</param>
        public NeuropixelsV1eProbeGroup(NeuropixelsV1eProbeGroup probeGroup)
            : base(probeGroup)
        {
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
            if (numberOfContacts % 2 != 0)
            {
                throw new ArgumentException("Invalid number of channels given; must be a multiple of two");
            }

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
        /// <param name="index">Index of the contact.</param>
        /// <returns>A float array of size [2 x 1] with the X and Y coordinates, respectively.</returns>
        public static float[] DefaultContactPosition(int index)
        {
            return new float[2] { ContactPositionX(index), index / 2 * 20 + 170 };
        }

        private static float ContactPositionX(int index) => (index % 4) switch
        {
            0 => 27.0f,
            1 => 59.0f,
            2 => 11.0f,
            3 => 43.0f,
            _ => throw new ArgumentException("Invalid index given.")
        };

        /// <summary>
        /// Generates a default planar contour for the probe, based on the given probe index
        /// </summary>
        /// <returns></returns>
        public static float[][] DefaultProbePlanarContour()
        {
            float[][] probePlanarContour = new float[6][];

            probePlanarContour[0] = new float[2] { 0f, 155f };
            probePlanarContour[1] = new float[2] { 35f, 0f };
            probePlanarContour[2] = new float[2] { 70f, 155f };
            probePlanarContour[3] = new float[2] { 70f, 9770f };
            probePlanarContour[4] = new float[2] { 0f, 9770f };
            probePlanarContour[5] = new float[2] { 0f, 155f };

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
                deviceChannelIndices[i] = i;
            }

            for (int i = channelCount; i < electrodeCount; i++)
            {
                deviceChannelIndices[i] = -1;
            }

            return deviceChannelIndices;
        }

        /// <summary>
        /// Generates an array of strings with the value "0" as the default shank ID, since Neuropixel 1.0 only has one shank
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single probe</param>
        /// <returns></returns>
        public static string[] DefaultShankIds(int numberOfContacts)
        {
            string[] contactIds = new string[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactIds[i] = "0";
            }

            return contactIds;
        }

        /// <summary>
        /// Updates the <see cref="Probe.DeviceChannelIndices"/> based on the given channel map.
        /// </summary>
        /// <param name="channelMap">Existing <see cref="NeuropixelsV1ProbeConfiguration.ChannelMap"/>.</param>
        internal void UpdateDeviceChannelIndices(List<NeuropixelsV1Electrode> channelMap)
        {
            var numberOfContacts = NumberOfContacts;

            int[] newDeviceChannelIndices = new int[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
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
        /// Convert a <see cref="NeuropixelsV1eProbeGroup"/> object to a list of electrodes, which only includes currently enabled electrodes
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV1eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/>'s that are enabled</returns>
        public static List<NeuropixelsV1Electrode> ToChannelMap(NeuropixelsV1eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV1Electrode> channelMap = new();

            var enabledContacts = channelConfiguration.GetContacts().Where(c => c.DeviceId != -1);

            if (enabledContacts.Count() != NeuropixelsV1.ChannelCount)
            {
                throw new InvalidOperationException($"Channel configuration must have {NeuropixelsV1.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            foreach (var c in enabledContacts)
            {
                channelMap.Add(new NeuropixelsV1Electrode(c.Index));
            }

            return channelMap.OrderBy(e => e.Channel).ToList();
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes.
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV1eProbeGroup"/> object.</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/> electrodes.</returns>
        public static List<NeuropixelsV1Electrode> ToElectrodes(NeuropixelsV1eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV1Electrode> electrodes = new();

            foreach (var c in channelConfiguration.GetContacts())
            {
                electrodes.Add(new NeuropixelsV1Electrode(c.Index));
            }

            return electrodes;
        }
    }
}
