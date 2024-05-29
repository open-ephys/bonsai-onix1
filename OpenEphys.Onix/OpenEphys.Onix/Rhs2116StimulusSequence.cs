using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using System;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Defines a class that holds the Stimulus Sequence for an Rhs2116 device.
    /// Able to be expanded so that it contains two combined sequences for HeadstageRhs2116,
    /// with an Rhs2116A and Rhs2116B.
    /// </summary>
    public class Rhs2116StimulusSequence
    {
        private readonly bool DualSequences;

        public static readonly int NumStimuliPerDevice = 16;

        /// <summary>
        /// Default constructor for Rhs2116StimulusSequence. Initializes with 16 stimuli
        /// </summary>
        public Rhs2116StimulusSequence()
        {
            Stimuli = new Rhs2116Stimulus[NumStimuliPerDevice];

            for (var i = 0; i < Stimuli.Length; i++)
            {
                Stimuli[i] = new Rhs2116Stimulus();
            }

            DualSequences = false;
        }

        /// <summary>
        /// Constructor for Rhs2116StimulusSequence that takes a boolean value; if true, 
        /// construct the sequence with 32 channels of stimuli, and if false generates the same 
        /// as the default constructor
        /// </summary>
        /// <param name="dualSequences">If true, create 32 channels of stimuli; if false, create 16 chanels of stimuli</param>
        public Rhs2116StimulusSequence(bool dualSequences)
        {
            var numStimuli = dualSequences ? NumStimuliPerDevice * 2 : NumStimuliPerDevice;

            Stimuli = new Rhs2116Stimulus[numStimuli];

            for (var i = 0; i < Stimuli.Length; i++)
            {
                Stimuli[i] = new Rhs2116Stimulus();
            }

            DualSequences = dualSequences;
        }

        /// <summary>
        /// Copy construct for Rhs2116StimulusSequence; performs a shallow copy using MemberwiseClone()
        /// </summary>
        /// <param name="sequence"></param>
        public Rhs2116StimulusSequence(Rhs2116StimulusSequence sequence)
        {
            Stimuli = Array.ConvertAll(sequence.Stimuli, stimulus => stimulus.Clone());
            CurrentStepSize = sequence.CurrentStepSize;
            DualSequences = sequence.DualSequences;
        }

        public Rhs2116Stimulus[] Stimuli { get; set; }

        public Rhs2116StepSize CurrentStepSize { get; set; } = Rhs2116StepSize.Step5000nA;

        /// <summary>
        /// Maximum length of the sequence across all channels
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
        /// Is the stimulus sequence well defined
        /// </summary>
        [XmlIgnore]
        public bool Valid => Stimuli.ToList().All(s => s.Valid);

        /// <summary>
        /// Does the sequence fit in hardware
        /// </summary>
        [XmlIgnore]
        public bool FitsInHardware => StimulusSlotsRequired <= Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// The maximum number of memory slots available
        /// </summary>
        [XmlIgnore]
        public int MaxMemorySlotsAvailable => Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// Number of hardware memory slots required by the sequence
        /// </summary>
        [XmlIgnore]
        public int StimulusSlotsRequired => Math.Max(DeltaTable.Count, DeltaTableB.Count);

        [XmlIgnore]
        public double CurrentStepSizeuA
        {
            get
            {
                return CurrentStepSize switch
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
        }

        [XmlIgnore]
        public double MaxPossibleAmplitudePerPhaseMicroAmps => CurrentStepSizeuA * 255;

        internal IEnumerable<byte> AnodicAmplitudes => Stimuli.ToList().Select(x => x.AnodicAmplitudeSteps);

        internal IEnumerable<byte> CathodicAmplitudes => Stimuli.ToList().Select(x => x.CathodicAmplitudeSteps);

        /// <summary>
        /// Generate the delta-table representation of this stimulus sequence that can be uploaded to the RHS2116 device.
        /// The resultant dictionary has a time, in samples, as the key and a combined [polarity, enable] bit field as the value.
        /// </summary>
        [XmlIgnore]
        internal Dictionary<uint, uint> DeltaTable
        {
            get => GetDeltaTable(true);
        }

        /// <summary>
        /// Generate the delta-table representation of this stimulus sequence that can be uploaded to the RHS2116B device.
        /// The resultant dictionary has a time, in samples, as the key and a combined [polarity, enable] bit field as the value.
        /// This only returns a delta-table if the StimulusSequence was initialized using DualSequences = true, otherwise it returns
        /// an empty Dictionary
        /// </summary>
        [XmlIgnore]
        internal Dictionary<uint, uint> DeltaTableB
        {
            get
            {
                if (!DualSequences)
                {
                    return new Dictionary<uint, uint>();
                }

                return GetDeltaTable(false);
            }
        }

        private Dictionary<uint, uint> GetDeltaTable(bool deltaTableA)
        {
            var table = new Dictionary<uint, BitArray>();

            var offset = deltaTableA ? 0 : NumStimuliPerDevice;

            for (int i = 0; i < NumStimuliPerDevice; i++)
            {
                var s = Stimuli[i + offset];

                var e0 = s.AnodicFirst ? s.AnodicAmplitudeSteps > 0 : s.CathodicAmplitudeSteps > 0;
                var e1 = s.AnodicFirst ? s.CathodicAmplitudeSteps > 0 : s.AnodicAmplitudeSteps > 0;
                var d0 = s.AnodicFirst ? s.AnodicWidthSamples : s.CathodicWidthSamples;
                var d1 = d0 + s.DwellSamples;
                var d2 = d1 + (s.AnodicFirst ? s.CathodicWidthSamples : s.AnodicWidthSamples);

                var t0 = s.DelaySamples;

                for (int j = 0; j<s.NumberOfStimuli; j++)
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
