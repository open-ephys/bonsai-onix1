using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A stimulus sequence for a Rhs2116 device.
    /// </summary>
    public class Rhs2116StimulusSequence
    {
        const int NumberOfChannelsPerDevice = 16;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116StimulusSequence"/> class with 16 default
        /// stimuli.
        /// </summary>
        public Rhs2116StimulusSequence()
        {
            Stimuli = new Rhs2116Stimulus[NumberOfChannelsPerDevice];

            for (var i = 0; i < Stimuli.Length; i++)
            {
                Stimuli[i] = new Rhs2116Stimulus();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116StimulusSequence"/> by performing a
        /// shallow copy of a reference sequence.
        /// </summary>
        /// <param name="sequence">Existing <see cref="Rhs2116StimulusSequence"/> to copy.</param>
        public Rhs2116StimulusSequence(Rhs2116StimulusSequence sequence)
        {
            Stimuli = Array.ConvertAll(sequence.Stimuli, stimulus => stimulus.Clone());
            CurrentStepSize = sequence.CurrentStepSize;
        }

        /// <summary>
        /// Gets or sets the array of <see cref="Rhs2116Stimulus"/> sequences.
        /// </summary>
        public Rhs2116Stimulus[] Stimuli { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116StepSize"/>. 
        /// </summary>
        public Rhs2116StepSize CurrentStepSize { get; set; } = Rhs2116StepSize.Step5000nA;

        /// <summary>
        /// Maximum length of the sequence across all channels.
        /// </summary>
        [XmlIgnore]
        public uint SequenceLengthSamples
        {
            get
            {
                uint max = 0;

                foreach (var stim in Stimuli)
                {
                    var len = stim.DelaySamples + stim.NumberOfStimuli * (stim.AnodicWidthSamples + stim.CathodicWidthSamples + stim.DwellSamples + stim.InterStimulusIntervalSamples);
                    max = len > max ? len : max;

                }

                return max;
            }
        }

        /// <summary>
        /// Maximum peak to peak amplitude of the sequence across all channels.
        /// </summary>
        [XmlIgnore]
        public int MaximumPeakToPeakAmplitudeSteps
        {
            get
            {
                int max = 0;

                foreach (var stim in Stimuli)
                {
                    var p2p = stim.CathodicAmplitudeSteps + stim.AnodicAmplitudeSteps;
                    max = p2p > max ? p2p : max;

                }

                return max;
            }
        }

        /// <summary>
        /// Is the stimulus sequence well defined.
        /// </summary>
        [XmlIgnore]
        public bool Valid => Stimuli.ToList().All(s => s.Valid);

        /// <summary>
        /// Does the sequence fit in hardware.
        /// </summary>
        [XmlIgnore]
        public bool FitsInHardware => StimulusSlotsRequired <= Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// The maximum number of memory slots available.
        /// </summary>
        [XmlIgnore]
        public int MaxMemorySlotsAvailable => Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// Number of hardware memory slots required by the sequence.
        /// </summary>
        [XmlIgnore]
        public int StimulusSlotsRequired => DeltaTable.Count;

        /// <summary>
        /// Gets the current step size in µA.
        /// </summary>
        [XmlIgnore]
        public double CurrentStepSizeuA
        {
            get => GetStepSizeuA(CurrentStepSize);
        }

        /// <summary>
        /// Gets the step size in microamps of a given <see cref="Rhs2116StepSize"/>.
        /// </summary>
        /// <param name="stepSize">The <see cref="Rhs2116StepSize"/> value to convert to microamps.</param>
        /// <returns>Returns a double corresponding to the given step size in microamps.</returns>
        /// <exception cref="ArgumentException">Invalid stimulus step size selection.</exception>
        public static double GetStepSizeuA(Rhs2116StepSize stepSize)
        {
            return stepSize switch
            {
                Rhs2116StepSize.Step10nA => 0.01,
                Rhs2116StepSize.Step20nA => 0.02,
                Rhs2116StepSize.Step50nA => 0.05,
                Rhs2116StepSize.Step100nA => 0.1,
                Rhs2116StepSize.Step200nA => 0.2,
                Rhs2116StepSize.Step500nA => 0.5,
                Rhs2116StepSize.Step1000nA => 1.0,
                Rhs2116StepSize.Step2000nA => 2.0,
                Rhs2116StepSize.Step5000nA => 5.0,
                Rhs2116StepSize.Step10000nA => 10.0,
                _ => throw new ArgumentException("Invalid stimulus step size selection."),
            };
        }

        /// <summary>
        /// Gets the maximum possible amplitude for a single pulse, in µA.
        /// </summary>
        [XmlIgnore]
        public double MaxPossibleAmplitudePerPhaseMicroAmps => CurrentStepSizeuA * 255;

        internal IEnumerable<byte> AnodicAmplitudes => Stimuli.ToList().Select(x => x.AnodicAmplitudeSteps);

        internal IEnumerable<byte> CathodicAmplitudes => Stimuli.ToList().Select(x => x.CathodicAmplitudeSteps);

        /// <summary>
        /// Generate the delta-table representation of this stimulus sequence that can be uploaded to the
        /// RHS2116 device. The resultant dictionary has a time, in samples, as the key and a combined
        /// [polarity, enable] bit field as the value.
        /// </summary>
        [XmlIgnore]
        internal Dictionary<uint, uint> DeltaTable
        {
            get => GetDeltaTable();
        }

        private Dictionary<uint, uint> GetDeltaTable()
        {
            var table = new Dictionary<uint, BitArray>();

            for (int i = 0; i < NumberOfChannelsPerDevice; i++)
            {
                var s = Stimuli[i];

                var e0 = s.AnodicFirst ? s.AnodicAmplitudeSteps > 0 : s.CathodicAmplitudeSteps > 0;
                var e1 = s.AnodicFirst ? s.CathodicAmplitudeSteps > 0 : s.AnodicAmplitudeSteps > 0;
                var d0 = s.AnodicFirst ? s.AnodicWidthSamples : s.CathodicWidthSamples;
                var d1 = d0 + s.DwellSamples;
                var d2 = d1 + (s.AnodicFirst ? s.CathodicWidthSamples : s.AnodicWidthSamples);

                var t0 = s.DelaySamples;

                for (int j = 0; j < s.NumberOfStimuli; j++)
                {
                    AddOrInsert(ref table, i, t0, s.AnodicFirst, e0);
                    AddOrInsert(ref table, i, t0 + d0, s.AnodicFirst, false);
                    AddOrInsert(ref table, i, t0 + d1, !s.AnodicFirst, e1);
                    AddOrInsert(ref table, i, t0 + d2, !s.AnodicFirst, false);

                    t0 += d2 + s.InterStimulusIntervalSamples;
                }
            }

            return table.ToDictionary(d => d.Key, d =>
            {
                int[] i = new int[1];
                d.Value.CopyTo(i, 0);
                return (uint)i[0];
            });
        }

        private static void AddOrInsert(ref Dictionary<uint, BitArray> table, int channel, uint key, bool polarity, bool enable)
        {
            if (table.ContainsKey(key))
            {
                table[key][channel] = enable;
                table[key][channel + 16] = polarity;
            }
            else
            {
                table.Add(key, new BitArray(32, false));
                table[key][channel] = enable;
                table[key][channel + 16] = polarity;
            }
        }
    }
}
