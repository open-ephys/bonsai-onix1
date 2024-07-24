using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;


namespace OpenEphys.Onix
{
    internal class Ts4231PulseQueue
    {
        internal Ts4231PulseQueue()
        {
            var fill0 = new double[Ts4231PulseConverter.Template.Length / 4];
            var fill1 = new bool[Ts4231PulseConverter.Template.Length];
            var fill2 = new ulong[Ts4231PulseConverter.Template.Length / 4];

            PulseTimes = new Queue<double>(fill0);
            PulseWidths = new Queue<double>(fill0);
            PulseParse = new Queue<bool>(fill1);
            PulseDataClock = new Queue<ulong>(fill2);
            PulseFrameClock = new Queue<ulong>(fill2);
        }

        public Queue<double> PulseTimes { get; }

        public Queue<double> PulseWidths { get; }

        public Queue<bool> PulseParse { get; }

        public Queue<ulong> PulseDataClock { get; }

        public Queue<ulong> PulseFrameClock { get; }

    }

    internal class Ts4231PulseConverter
    {
        readonly double HubClockFrequencyPeriod;
        readonly Mat p;
        readonly Mat q;

        // Template pattern
        internal static readonly bool[] Template = {
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

        const double SweepFrequencyHz = 60;

        Dictionary<int, Ts4231PulseQueue> PulseQueues = new();

        public Ts4231PulseConverter(uint hubClockFrequencyHz, Point3d origin1, Point3d origin2)
        {
            HubClockFrequencyPeriod = 1d / hubClockFrequencyHz;

            p = new Mat(3, 1, Depth.F64, 1);
            p[0] = new Scalar(origin1.X);
            p[1] = new Scalar(origin1.Y);
            p[2] = new Scalar(origin1.Z);

            q = new Mat(3, 1, Depth.F64, 1);
            q[0] = new Scalar(origin2.X);
            q[1] = new Scalar(origin2.Y);
            q[2] = new Scalar(origin2.Z);
        }

        public unsafe Ts4231DataFrame Convert(oni.Frame frame)
        {
            var payload = (Ts4231Payload*)frame.Data.ToPointer();

            var i = payload->SensorIndex;
            if (!PulseQueues.ContainsKey(i))
                PulseQueues.Add(i, new Ts4231PulseQueue());

            // Push pulse time into buffer and pop oldest
            PulseQueues[i].PulseTimes.Dequeue();
            PulseQueues[i].PulseTimes.Enqueue(HubClockFrequencyPeriod * payload->HubClock);

            PulseQueues[i].PulseDataClock.Dequeue();
            PulseQueues[i].PulseDataClock.Enqueue(payload->HubClock);

            PulseQueues[i].PulseFrameClock.Dequeue();
            PulseQueues[i].PulseFrameClock.Enqueue(frame.Clock);

            // Push pulse width into buffer and pop oldest
            PulseQueues[i].PulseWidths.Dequeue();
            PulseQueues[i].PulseWidths.Enqueue(HubClockFrequencyPeriod * payload->EnvelopeWidth);

            // Push pulse parse info into buffer and pop oldest 4x
            PulseQueues[i].PulseParse.Dequeue();
            PulseQueues[i].PulseParse.Dequeue();
            PulseQueues[i].PulseParse.Dequeue();
            PulseQueues[i].PulseParse.Dequeue();

            PulseQueues[i].PulseParse.Enqueue(payload->EnvelopeType == Ts4231Envelope.Bad);
            PulseQueues[i].PulseParse.Enqueue(payload->EnvelopeType >= Ts4231Envelope.J2 & payload->EnvelopeType != Ts4231Envelope.Sweep); // skip
            PulseQueues[i].PulseParse.Enqueue((int)payload->EnvelopeType % 2 == 1 & payload->EnvelopeType != Ts4231Envelope.Sweep); // axis
            PulseQueues[i].PulseParse.Enqueue(payload->EnvelopeType == Ts4231Envelope.Sweep); // sweep

            // test template match and make sure time between pulses does not integrate to more than two periods
            if (!PulseQueues[i].PulseParse.SequenceEqual(Template) ||
                PulseQueues[i].PulseTimes.Last() - PulseQueues[i].PulseTimes.First() > 2 / SweepFrequencyHz)
            {
                return null;
            }

            // position measurement time is defined to be the mean of the data used
            var time = PulseQueues[i].PulseTimes.ToArray();
            var width = PulseQueues[i].PulseWidths.ToArray();

            var t11 = time[2] + width[2] / 2 - time[0];
            var t21 = time[5] + width[5] / 2 - time[3];
            var theta0 = 2 * Math.PI * SweepFrequencyHz * t11 - Math.PI / 2;
            var gamma0 = 2 * Math.PI * SweepFrequencyHz * t21 - Math.PI / 2;

            var u = new Mat(3, 1, Depth.F64, 1);
            u[0] = new Scalar(Math.Tan(theta0));
            u[1] = new Scalar(Math.Tan(gamma0));
            u[2] = new Scalar(1);
            CV.Normalize(u, u);

            var t12 = time[8] + width[8] / 2 - time[7];
            var t22 = time[11] + width[11] / 2 - time[10];
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

            // TODO: If non-singular solution else send NaNs
            //if (x)
            //{
            var p1 = p + x1 * u;
            var q1 = q + x2 * v;
            var position = 0.5 * (p1 + q1);
            //}

            return new Ts4231DataFrame(PulseQueues[i].PulseDataClock.ElementAt(Template.Length / 8),
                PulseQueues[i].PulseFrameClock.ElementAt(Template.Length / 8),
                i,
                new Vector3((float)position[0].Val0, (float)position[1].Val0, (float)position[2].Val0));

        }
    }
}
