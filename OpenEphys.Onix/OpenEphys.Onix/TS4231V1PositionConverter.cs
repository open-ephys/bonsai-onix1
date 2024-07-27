using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace OpenEphys.Onix
{
    class TS4231V1PulseQueue
    {
        public Queue<ulong> PulseFrameClock { get; } = new(new ulong[TS4231V1PositionConverter.ValidPulseSequenceTemplate.Length / 4]);
        public Queue<ulong> PulseHubClock { get; } = new(new ulong[TS4231V1PositionConverter.ValidPulseSequenceTemplate.Length / 4]);
        public Queue<double> PulseWidths { get; } = new(new double[TS4231V1PositionConverter.ValidPulseSequenceTemplate.Length / 4]);
        public Queue<bool> PulseParse { get; } = new(new bool[TS4231V1PositionConverter.ValidPulseSequenceTemplate.Length]);
    }

    class TS4231V1PositionConverter
    {
        const double SweepFrequencyHz = 60;
        readonly double HubClockFrequencyPeriod;
        readonly Mat p;
        readonly Mat q;

        // Template pattern
        internal static readonly bool[] ValidPulseSequenceTemplate = {
        //  bad    skip   axis   sweep
            false, false, false, false,
            false, true,  false, false,
            false, false, false, true, // axis 0, station 0
            false, false, true,  false,
            false, true,  true,  false,
            false, false, false, true, // axis 1, station 0
            false, true,  false, false,
            false, false, false, false,
            false, false, false, true, // axis 0, station 1
            false, true,  true,  false,
            false, false, true,  false,
            false, false, false, true  // axis 1, station 1
        };

        Dictionary<int, TS4231V1PulseQueue> PulseQueues = new();

        public TS4231V1PositionConverter(uint hubClockFrequencyHz, Point3d baseStation1Origin, Point3d baseStation2Origin)
        {
            HubClockFrequencyPeriod = 1d / hubClockFrequencyHz;

            p = new Mat(3, 1, Depth.F64, 1);
            p[0] = new Scalar(baseStation1Origin.X);
            p[1] = new Scalar(baseStation1Origin.Y);
            p[2] = new Scalar(baseStation1Origin.Z);

            q = new Mat(3, 1, Depth.F64, 1);
            q[0] = new Scalar(baseStation2Origin.X);
            q[1] = new Scalar(baseStation2Origin.Y);
            q[2] = new Scalar(baseStation2Origin.Z);
        }

        public unsafe TS4231V1PositionDataFrame Convert(oni.Frame frame)
        {
            var payload = (TS4231V1Payload*)frame.Data.ToPointer();

            if (!PulseQueues.ContainsKey(payload->SensorIndex))
                PulseQueues.Add(payload->SensorIndex, new TS4231V1PulseQueue());

            var queues = PulseQueues[payload->SensorIndex];

            // Push pulse time into buffer and pop oldest
            queues.PulseFrameClock.Dequeue();
            queues.PulseFrameClock.Enqueue(frame.Clock);

            queues.PulseHubClock.Dequeue();
            queues.PulseHubClock.Enqueue(payload->HubClock);

            // Push pulse width into buffer and pop oldest
            queues.PulseWidths.Dequeue();
            queues.PulseWidths.Enqueue(HubClockFrequencyPeriod * payload->EnvelopeWidth);

            // push pulse code categorization into buffer and pop oldest 4x
            queues.PulseParse.Dequeue();
            queues.PulseParse.Dequeue();
            queues.PulseParse.Dequeue();
            queues.PulseParse.Dequeue();
            queues.PulseParse.Enqueue(payload->EnvelopeType == TS4231V1Envelope.Bad);
            queues.PulseParse.Enqueue(payload->EnvelopeType >= TS4231V1Envelope.J2 & payload->EnvelopeType != TS4231V1Envelope.Sweep); // skip
            queues.PulseParse.Enqueue((int)payload->EnvelopeType % 2 == 1 & payload->EnvelopeType != TS4231V1Envelope.Sweep); // axis
            queues.PulseParse.Enqueue(payload->EnvelopeType == TS4231V1Envelope.Sweep); // sweep

            // convert to arrays
            var times = queues.PulseHubClock.Select(x => HubClockFrequencyPeriod * x).ToArray();
            var widths = queues.PulseWidths.ToArray();

            // test template match and make sure time between pulses does not integrate to more than two periods
            if (!queues.PulseParse.SequenceEqual(ValidPulseSequenceTemplate) ||
                 times.Last() - times.First() > 2 / SweepFrequencyHz)
            {
                return null;
            }

            var t11 = times[2] + widths[2] / 2 - times[0];
            var t21 = times[5] + widths[5] / 2 - times[3];
            var theta0 = 2 * Math.PI * SweepFrequencyHz * t11 - Math.PI / 2;
            var gamma0 = 2 * Math.PI * SweepFrequencyHz * t21 - Math.PI / 2;

            var u = new Mat(3, 1, Depth.F64, 1);
            u[0] = new Scalar(Math.Tan(theta0));
            u[1] = new Scalar(Math.Tan(gamma0));
            u[2] = new Scalar(1);
            CV.Normalize(u, u);

            var t12 = times[8] + widths[8] / 2 - times[7];
            var t22 = times[11] + widths[11] / 2 - times[10];
            var theta1 = 2 * Math.PI * SweepFrequencyHz * t12 - Math.PI / 2;
            var gamma1 = 2 * Math.PI * SweepFrequencyHz * t22 - Math.PI / 2;

            var v = new Mat(3, 1, Depth.F64, 1);
            v[0] = new Scalar(Math.Tan(theta1));
            v[1] = new Scalar(Math.Tan(gamma1));
            v[2] = new Scalar(1);
            CV.Normalize(v, v);

            // Base station origin vector
            var d = q - p;

            // Linear transform
            // A = [a11 a12]
            //     [a21 a22]
            var a11 = 1.0;
            var a12 = -CV.DotProduct(u, v);
            var a21 = CV.DotProduct(u, v);
            var a22 = -1.0;

            // Result
            // B = [b1]
            //     [b2]
            var b1 = CV.DotProduct(u, d);
            var b2 = CV.DotProduct(v, d);

            // Solve Ax = B
            var x2 = (b2 - (b1 * a21) / a11) / (a22 - (a12 * a21) / a11);
            var x1 = (b1 - a12 * x2) / a11;

            // If singular, return null
            if (double.IsNaN(x1) ||
                double.IsNaN(x2) ||
                double.IsInfinity(x1) ||
                double.IsInfinity(x2))
            {
                return null;
            }

            // calculate position
            var p1 = p + x1 * u;
            var q1 = q + x2 * v;
            var position = 0.5 * (p1 + q1);

            return new TS4231V1PositionDataFrame(
                queues.PulseHubClock.ElementAt(ValidPulseSequenceTemplate.Length / 8),
                queues.PulseFrameClock.ElementAt(ValidPulseSequenceTemplate.Length / 8),
                payload->SensorIndex,
                new Vector3((float)position[0].Val0, (float)position[1].Val0, (float)position[2].Val0));
        }
    }
}
