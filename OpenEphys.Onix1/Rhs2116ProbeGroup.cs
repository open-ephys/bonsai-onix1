using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A <see cref="ProbeGroup"/> class for RHS2116.
    /// </summary>
    public class Rhs2116ProbeGroup : ProbeGroup
    {
        const int DefaultNumberOfChannelsPerProbe = 16;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116ProbeGroup"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor will initialize the new <see cref="Rhs2116ProbeGroup"/> with
        /// the default settings for two probes, including the contact positions, shapes, and IDs.
        /// </remarks>
        public Rhs2116ProbeGroup()
            : base("probeinterface", "0.2.21",
                new List<Probe>()
                {
                    new(
                        ProbeNdim.Two,
                        ProbeSiUnits.mm,
                        new ProbeAnnotations("Rhs2116A", ""),
                        new ContactAnnotations(new string[0]),
                        DefaultContactPositions(DefaultNumberOfChannelsPerProbe, 0),
                        Probe.DefaultContactPlaneAxes(DefaultNumberOfChannelsPerProbe),
                        Probe.DefaultContactShapes(DefaultNumberOfChannelsPerProbe, ContactShape.Circle),
                        Probe.DefaultCircleParams(DefaultNumberOfChannelsPerProbe, 0.3f),
                        DefaultProbePlanarContour(0),
                        Probe.DefaultDeviceChannelIndices(DefaultNumberOfChannelsPerProbe, 0),
                        Probe.DefaultContactIds(DefaultNumberOfChannelsPerProbe),
                        Probe.DefaultShankIds(DefaultNumberOfChannelsPerProbe)),
                    new(
                        ProbeNdim.Two,
                        ProbeSiUnits.mm,
                        new ProbeAnnotations("Rhs2116B", ""),
                        new ContactAnnotations(new string[0]),
                        DefaultContactPositions(DefaultNumberOfChannelsPerProbe, 1),
                        Probe.DefaultContactPlaneAxes(DefaultNumberOfChannelsPerProbe),
                        Probe.DefaultContactShapes(DefaultNumberOfChannelsPerProbe, ContactShape.Circle),
                        Probe.DefaultCircleParams(DefaultNumberOfChannelsPerProbe, 0.3f),
                        DefaultProbePlanarContour(1),
                        Probe.DefaultDeviceChannelIndices(DefaultNumberOfChannelsPerProbe, DefaultNumberOfChannelsPerProbe),
                        Probe.DefaultContactIds(DefaultNumberOfChannelsPerProbe),
                        Probe.DefaultShankIds(DefaultNumberOfChannelsPerProbe))
                }.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116ProbeGroup"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is marked with the <see cref="JsonConstructorAttribute"/>, and is the
        /// entry point for deserializing the JSON data into a C# class.
        /// </remarks>
        /// <param name="specification">String defining the <see cref="ProbeGroup.Specification"/>.</param>
        /// <param name="version">String defining the <see cref="ProbeGroup.Version"/>.</param>
        /// <param name="probes">Array of <see cref="Probe"/>s.</param>
        [JsonConstructor]
        public Rhs2116ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116ProbeGroup"/> class from an existing instance.
        /// </summary>
        /// <param name="probeGroup">An existing <see cref="Rhs2116ProbeGroup"/> object.</param>
        public Rhs2116ProbeGroup(Rhs2116ProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        internal static float[][] DefaultContactPositions(int numberOfChannels, int probeIndex)
        {
            float[][] contactPositions = new float[numberOfChannels][];

            if (probeIndex == 0)
            {
                for (int i = 0; i < numberOfChannels; i++)
                {
                    contactPositions[i] = new float[2] { numberOfChannels - i, 3.0f };
                }
            }
            else if (probeIndex == 1)
            {
                for (int i = 0; i < numberOfChannels; i++)
                {
                    contactPositions[i] = new float[2] { i + 1.0f, 1.0f };
                }
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default contact positions for {nameof(Rhs2116ProbeGroup)}");
            }

            return contactPositions;
        }

        /// <summary>
        /// Generates a default planar contour for the probe, based on the given probe index
        /// </summary>
        /// <param name="probeIndex">Zero-based index of the probe to generate a contour for</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Probe indices less than 0 and greater than 1 are not allowed.</exception>
        public static float[][] DefaultProbePlanarContour(int probeIndex)
        {
            float[][] probePlanarContour = new float[5][];

            if (probeIndex == 0)
            {
                probePlanarContour[0] = new float[2] { 0.5f, 2.5f };
                probePlanarContour[1] = new float[2] { 16.5f, 2.5f };
                probePlanarContour[2] = new float[2] { 16.5f, 3.5f };
                probePlanarContour[3] = new float[2] { 0.5f, 3.5f };
                probePlanarContour[4] = new float[2] { 0.5f, 2.5f };
            }
            else if (probeIndex == 1)
            {
                probePlanarContour[0] = new float[2] { 0.5f, 0.5f };
                probePlanarContour[1] = new float[2] { 16.5f, 0.5f };
                probePlanarContour[2] = new float[2] { 16.5f, 1.5f };
                probePlanarContour[3] = new float[2] { 0.5f, 1.5f };
                probePlanarContour[4] = new float[2] { 0.5f, 0.5f };
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default probe planar contours for {nameof(Rhs2116ProbeGroup)}");
            }

            return probePlanarContour;
        }
    }
}
