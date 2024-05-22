using System;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix
{
    public class Rhs2116ProbeGroup : ProbeGroup
    {
        private const int NumChannelsPerProbe = 16;

        public Rhs2116ProbeGroup()
        {
            Specification = "probeinterface";
            Version = "0.2.21";

            Probes = new Probe[2];

            Probes[0] = new(2,
                            "mm",
                            new ProbeAnnotations(),
                            new ContactAnnotations(),
                            DefaultContactPositions(0),
                            DefaultContactPlaneAxes(),
                            DefaultContactShapes(),
                            DefaultContactShapeParams(),
                            DefaultProbePlanarContour(0),
                            DefaultDeviceChannelIndices(0),
                            DefaultContactIds(),
                            DefaultShankIds());

            Probes[1] = new(2,
                            "mm",
                            new ProbeAnnotations(),
                            new ContactAnnotations(),
                            DefaultContactPositions(1),
                            DefaultContactPlaneAxes(),
                            DefaultContactShapes(),
                            DefaultContactShapeParams(),
                            DefaultProbePlanarContour(1),
                            DefaultDeviceChannelIndices(1),
                            DefaultContactIds(),
                            DefaultShankIds());
        }

        public Rhs2116ProbeGroup(Rhs2116ProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        private float[][] DefaultContactPositions(int probeIndex)
        {
            float[][] contactPositions = new float[NumChannelsPerProbe][];

            if (probeIndex == 0)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    contactPositions[i] = new float[2] { i + 1.0f, 3.0f };
                }
            }
            else if (probeIndex == 1)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
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

        private float[][][] DefaultContactPlaneAxes()
        {
            float[][][] contactPlaneAxes = new float[NumChannelsPerProbe][][];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactPlaneAxes[i] = new float[2][] { new float[2] { 1.0f, 0.0f }, new float[2] { 0.0f, 1.0f } };
            }

            return contactPlaneAxes;
        }

        private string[] DefaultContactShapes()
        {
            string[] contactShapes = new string[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactShapes[i] = "circle";
            }

            return contactShapes;
        }

        private ContactShapeParam[] DefaultContactShapeParams()
        {
            ContactShapeParam[] contactShapeParams = new ContactShapeParam[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactShapeParams[i] = new ContactShapeParam(0.3f);
            }

            return contactShapeParams;
        }

        private float[][] DefaultProbePlanarContour(int probeIndex)
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

        private int[] DefaultDeviceChannelIndices(int probeIndex)
        {
            int[] deviceChannelIndices = new int[NumChannelsPerProbe];

            if (probeIndex == 0)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    deviceChannelIndices[i] = i;
                }
            }
            else if (probeIndex == 1)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    deviceChannelIndices[i] = i + NumChannelsPerProbe;
                }
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default device channel indices for {nameof(Rhs2116ProbeGroup)}");
            }

            return deviceChannelIndices;
        }

        private string[] DefaultContactIds()
        {
            string[] contactIds = new string[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactIds[i] = i.ToString();
            }

            return contactIds;
        }

        private string[] DefaultShankIds()
        {
            string[] contactIds = new string[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactIds[i] = "";
            }

            return contactIds;
        }
    }
}
