using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

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
        public NeuropixelsV1eProbeGroup(NeuropixelsV1ProbeType probeType)
            : base("probeinterface", "0.2.21", DefaultProbes(probeType))
        {
        }

        internal static string Neuropixels1ProbeName = "Neuropixels 1.0";
        internal static string Neuropixels1UhdProbeName = "Neuropixels Ultra (16 banks)";

        internal static string GetProbeName(NeuropixelsV1ProbeType probeType)
        {
            return probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => Neuropixels1ProbeName,
                NeuropixelsV1ProbeType.UHD => Neuropixels1UhdProbeName,
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };
        }

        internal static NeuropixelsV1ProbeType GetProbeTypeFromProbeName(string probeName)
        {
            if (probeName == Neuropixels1ProbeName)
                return NeuropixelsV1ProbeType.NP1;

            else if (probeName == Neuropixels1UhdProbeName)
                return NeuropixelsV1ProbeType.UHD;

            else
                throw new ArgumentException($"The name '{probeName}' does not match any known Neuropixels 1.0 probe names.");
        }

        private static Probe[] DefaultProbes(NeuropixelsV1ProbeType probeType)
        {
            var probe = new Probe[1];

            var electrodeCount = probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => NeuropixelsV1.ElectrodeCount,
                NeuropixelsV1ProbeType.UHD => NeuropixelsV1.ElectrodeCountUHD,
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };

            float contactSize = probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => 12.0f,
                NeuropixelsV1ProbeType.UHD => 5.0f,
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };

            probe[0] = new(ProbeNdim.Two,
                           ProbeSiUnits.um,
                           new ProbeAnnotations(GetProbeName(probeType), "IMEC"),
                           null,
                           DefaultContactPositions(electrodeCount, probeType),
                           Probe.DefaultContactPlaneAxes(electrodeCount),
                           Probe.DefaultContactShapes(electrodeCount, ContactShape.Square),
                           Probe.DefaultSquareParams(electrodeCount, contactSize),
                           DefaultProbePlanarContour(probeType),
                           DefaultDeviceChannelIndices(NeuropixelsV1.ChannelCount, electrodeCount, probeType),
                           Probe.DefaultContactIds(electrodeCount),
                           DefaultShankIds(electrodeCount));

            return probe;
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
        /// <param name="probes">Array of <see cref="Probe">Probes</see>.</param>
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

        internal static float[][] DefaultContactPositions(NeuropixelsV1ProbeType probeType)
        {
            return probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => DefaultContactPositions(NeuropixelsV1.ElectrodeCount, probeType),
                NeuropixelsV1ProbeType.UHD => DefaultContactPositions(NeuropixelsV1.ElectrodeCountUHD, probeType),
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };
        }

        /// <summary>
        /// Generates a 2D array of default contact positions based on the given number of channels.
        /// </summary>
        /// <param name="numberOfContacts">Value defining the number of contacts to create positions for.</param>
        /// <param name="probeType">The <see cref="NeuropixelsV1ProbeType"/> of the current probe.</param>
        /// <returns>
        /// 2D array of floats [N x 2], where the first dimension is the contact index [N] and the second dimension [2]
        /// contains the X and Y values, respectively.
        /// </returns>
        public static float[][] DefaultContactPositions(int numberOfContacts, NeuropixelsV1ProbeType probeType)
        {
            if (numberOfContacts % 2 != 0)
            {
                throw new ArgumentException("Invalid number of channels given; must be a multiple of two");
            }

            float[][] contactPositions = new float[numberOfContacts][];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactPositions[i] = DefaultContactPosition(i, probeType);
            }

            return contactPositions;
        }


        /// <summary>
        /// Generates a float array containing the X and Y position of a single contact.
        /// </summary>
        /// <param name="index">Index of the contact.</param>
        /// <param name="probeType">The <see cref="NeuropixelsV1ProbeType"/> of the current probe.</param>
        /// <returns>A float array of size [2 x 1] with the X and Y coordinates, respectively.</returns>
        public static float[] DefaultContactPosition(int index, NeuropixelsV1ProbeType probeType)
        {
            return new float[2] { ContactPositionX(index, probeType), ContactPositionY(index, probeType) };
        }

        static int GetSiteSpacing(NeuropixelsV1ProbeType probeType) => probeType switch
        {
            NeuropixelsV1ProbeType.NP1 => 20,
            NeuropixelsV1ProbeType.UHD => 6,
            _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
        };

        internal static int GetNumberOfColumns(NeuropixelsV1ProbeType probeType) => probeType switch
        {
            NeuropixelsV1ProbeType.NP1 => 2,
            NeuropixelsV1ProbeType.UHD => 8,
            _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
        };

        static float ContactPositionY(int index, NeuropixelsV1ProbeType probeType)
        {
            int numColumns = GetNumberOfColumns(probeType);
            int siteSpacing = GetSiteSpacing(probeType);

            const int shankOffsetY = 170;

            return probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => index / numColumns * siteSpacing + shankOffsetY,
                NeuropixelsV1ProbeType.UHD => (index - (index % numColumns)) * siteSpacing / numColumns,
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };
        }

        static float ContactPositionX(int index, NeuropixelsV1ProbeType probeType)
        {
            if (probeType == NeuropixelsV1ProbeType.NP1)
            {
                return (index % 4) switch
                {
                    0 => 27.0f,
                    1 => 59.0f,
                    2 => 11.0f,
                    3 => 43.0f,
                    _ => throw new ArgumentException("Invalid index given.")
                };
            }
            else if (probeType == NeuropixelsV1ProbeType.UHD)
            {
                var columnIndex = NeuropixelsV1Electrode.GetColumnIndex(index, probeType);

                return columnIndex * GetSiteSpacing(probeType);
            }
            else
                throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType));
        }


        /// <summary>
        /// Generates a default planar contour for the probe, based on the given probe index
        /// </summary>
        /// <returns></returns>
        public static float[][] DefaultProbePlanarContour(NeuropixelsV1ProbeType probeType)
        {
            float[][] probePlanarContour = new float[6][];

            if (probeType == NeuropixelsV1ProbeType.NP1)
            {
                const float shankTipY = 0f;
                const float shankBaseY = 155f;
                const float shankLengthY = 9770f;
                const float shankBaseX = 0f;
                const float shankTipX = 35f;
                const float shankLengthX = 70f;

                probePlanarContour[0] = new float[2] { shankBaseX, shankBaseY };
                probePlanarContour[1] = new float[2] { shankTipX, shankTipY };
                probePlanarContour[2] = new float[2] { shankLengthX, shankBaseY };
                probePlanarContour[3] = new float[2] { shankLengthX, shankLengthY };
                probePlanarContour[4] = new float[2] { shankBaseX, shankLengthY };
                probePlanarContour[5] = new float[2] { shankBaseX, shankBaseY };
            }
            else if (probeType == NeuropixelsV1ProbeType.UHD)
            {
                const float shankTipY = -186f;
                const float shankBaseY = -11f;
                const float shankLengthY = 9989f;
                const float shankBaseX = -14f;
                const float shankTipX = 21f;
                const float shankLengthX = 56f;

                probePlanarContour[0] = new float[2] { shankBaseX, shankBaseY };
                probePlanarContour[1] = new float[2] { shankTipX, shankTipY };
                probePlanarContour[2] = new float[2] { shankLengthX, shankBaseY };
                probePlanarContour[3] = new float[2] { shankLengthX, shankLengthY };
                probePlanarContour[4] = new float[2] { shankBaseX, shankLengthY };
                probePlanarContour[5] = new float[2] { shankBaseX, shankBaseY };
            }
            else
            {
                throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType));
            }

            return probePlanarContour;
        }

        /// <summary>
        /// Override of the DefaultDeviceChannelIndices function, which initializes a portion of the
        /// device channel indices, and leaves the rest at -1 to indicate they are not actively recorded
        /// </summary>
        /// <param name="channelCount">Number of contacts that are connected for recording</param>
        /// <param name="electrodeCount">Total number of physical contacts on the probe</param>
        /// <param name="probeType">The <see cref="NeuropixelsV1ProbeType"/> of the current probe.</param>
        /// <returns></returns>
        public static int[] DefaultDeviceChannelIndices(int channelCount, int electrodeCount, NeuropixelsV1ProbeType probeType = NeuropixelsV1ProbeType.NP1)
        {
            int[] deviceChannelIndices = new int[electrodeCount];

            for (int i = 0; i < channelCount; i++)
            {
                deviceChannelIndices[i] = NeuropixelsV1Electrode.GetChannelNumber(i, probeType);
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
        internal void UpdateDeviceChannelIndices(NeuropixelsV1Electrode[] channelMap)
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
        /// Convert a <see cref="NeuropixelsV1eProbeGroup"/> object to a list of electrodes, which only includes currently enabled electrodes
        /// </summary>
        /// <param name="probeGroup">A <see cref="NeuropixelsV1eProbeGroup"/> object</param>
        /// <param name="probeType">The <see cref="NeuropixelsV1ProbeType"/> of the current probe.</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/>'s that are enabled</returns>
        public static NeuropixelsV1Electrode[] ToChannelMap(NeuropixelsV1eProbeGroup probeGroup, NeuropixelsV1ProbeType probeType)
        {
            var enabledContacts = probeGroup.GetContacts().Where(c => c.DeviceId != -1);

            if (enabledContacts.Count() != NeuropixelsV1.ChannelCount)
            {
                throw new InvalidOperationException($"Channel configuration must have {NeuropixelsV1.ChannelCount} contacts enabled." +
                    $"Instead there are {enabledContacts.Count()} contacts enabled. Enabled contacts are designated by a device channel" +
                    $"index >= 0.");
            }

            return enabledContacts.Select(c => new NeuropixelsV1Electrode(c.Index, probeType))
                                  .OrderBy(e => e.Channel)
                                  .ToArray();
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes.
        /// </summary>
        /// <param name="probeGroup">A <see cref="NeuropixelsV1eProbeGroup"/> object.</param>
        /// <param name="probeType">The <see cref="NeuropixelsV1ProbeType"/> of the current probe.</param>
        /// <returns>List of <see cref="NeuropixelsV1Electrode"/> electrodes.</returns>
        public static List<NeuropixelsV1Electrode> ToElectrodes(NeuropixelsV1eProbeGroup probeGroup, NeuropixelsV1ProbeType probeType)
        {
            List<NeuropixelsV1Electrode> electrodes = new();

            foreach (var c in probeGroup.GetContacts())
            {
                electrodes.Add(new NeuropixelsV1Electrode(c.Index, probeType));
            }

            return electrodes;
        }
    }
}
