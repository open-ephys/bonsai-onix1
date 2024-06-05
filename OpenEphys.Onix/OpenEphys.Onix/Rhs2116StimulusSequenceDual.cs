using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Defines a class that holds the Stimulus Sequence for two Rhs2116 devices.
    /// </summary>
    public class Rhs2116StimulusSequenceDual
    {
        private readonly Rhs2116StimulusSequence stimulusSequenceA;
        private readonly Rhs2116StimulusSequence stimulusSequenceB;

        /// <summary>
        /// Default constructor for Rhs2116StimulusSequenceDual. Initializes with 16 stimuli per sequence
        /// </summary>
        public Rhs2116StimulusSequenceDual()
        {
            stimulusSequenceA = new Rhs2116StimulusSequence();
            stimulusSequenceB = new Rhs2116StimulusSequence();
        }

        /// <summary>
        /// Copy construct for Rhs2116StimulusSequenceDual; performs a shallow copy using MemberwiseClone()
        /// </summary>
        /// <param name="stimulusSequenceDual">Existing Dual Stimulus Sequence</param>
        public Rhs2116StimulusSequenceDual(Rhs2116StimulusSequenceDual stimulusSequenceDual)
        {
            stimulusSequenceA = new Rhs2116StimulusSequence(stimulusSequenceDual.stimulusSequenceA);
            stimulusSequenceB = new Rhs2116StimulusSequence(stimulusSequenceDual.stimulusSequenceB);
        }

        public Rhs2116Stimulus[] Stimuli
        {
            get { return stimulusSequenceA.Stimuli.Concat(stimulusSequenceB.Stimuli).ToArray(); }
            set
            {
                stimulusSequenceA.Stimuli = value.Take(16).ToArray();
                stimulusSequenceB.Stimuli = value.Skip(16).Take(16).ToArray();
            }
        }

        public Rhs2116StepSize CurrentStepSize
        {
            get { return stimulusSequenceA.CurrentStepSize; }
            set
            {
                stimulusSequenceA.CurrentStepSize = value;
                stimulusSequenceB.CurrentStepSize = value;
            }
        }

        /// <summary>
        /// Maximum length of the sequence across all channels
        /// </summary>
        [XmlIgnore]
        public uint SequenceLengthSamples => Math.Max(stimulusSequenceA.SequenceLengthSamples, stimulusSequenceB.SequenceLengthSamples);

        /// <summary>
        /// Maximum peak to peak amplitude of the sequence across all channels.
        /// </summary>
        [XmlIgnore]
        public int MaximumPeakToPeakAmplitudeSteps => Math.Max(stimulusSequenceA.MaximumPeakToPeakAmplitudeSteps, stimulusSequenceB.MaximumPeakToPeakAmplitudeSteps);

        /// <summary>
        /// Is the stimulus sequence well defined
        /// </summary>
        [XmlIgnore]
        public bool Valid => stimulusSequenceA.Valid && stimulusSequenceB.Valid;

        /// <summary>
        /// Does the sequence fit in hardware
        /// </summary>
        public bool FitsInHardware => stimulusSequenceA.FitsInHardware && stimulusSequenceB.FitsInHardware;

        /// <summary>
        /// The maximum number of memory slots available
        /// </summary>
        [XmlIgnore]
        public int MaxMemorySlotsAvailable => Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// Number of hardware memory slots required by the sequence
        /// </summary>
        [XmlIgnore]
        public int StimulusSlotsRequired => Math.Max(stimulusSequenceA.DeltaTable.Count, stimulusSequenceB.DeltaTable.Count);

        [XmlIgnore]
        public double CurrentStepSizeuA => stimulusSequenceA.CurrentStepSizeuA;

        [XmlIgnore]
        public double MaxPossibleAmplitudePerPhaseMicroAmps => CurrentStepSizeuA * 255;

        internal IEnumerable<byte> AnodicAmplitudes => stimulusSequenceA.AnodicAmplitudes.Concat(stimulusSequenceB.AnodicAmplitudes);
        
        internal IEnumerable<byte> CathodicAmplitudes => stimulusSequenceA.CathodicAmplitudes.Concat(stimulusSequenceB.CathodicAmplitudes);

        internal Dictionary<uint, uint> DeltaTableA => stimulusSequenceA.DeltaTable;
        
        internal Dictionary<uint, uint> DeltaTableB => stimulusSequenceB.DeltaTable;

        internal Dictionary<uint, uint> DeltaTable => DeltaTableA;

    }
}
