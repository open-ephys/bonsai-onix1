using System;
using System.Linq;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A pair of stimulus sequences for two Rhs2116 devices.
    /// </summary>
    public class Rhs2116StimulusSequencePair
    {
        internal readonly Rhs2116StimulusSequence StimulusSequenceA;
        internal readonly Rhs2116StimulusSequence StimulusSequenceB;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116StimulusSequencePair"/> class with 16 default
        /// stimuli per sequence.
        /// </summary>
        public Rhs2116StimulusSequencePair()
        {
            StimulusSequenceA = new Rhs2116StimulusSequence();
            StimulusSequenceB = new Rhs2116StimulusSequence();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116StimulusSequencePair"/> by performing a
        /// shallow copy of a reference sequence.
        /// </summary>
        /// <param name="stimulusSequenceDual">Existing <see cref="Rhs2116StimulusSequencePair"/> to copy.</param>
        public Rhs2116StimulusSequencePair(Rhs2116StimulusSequencePair stimulusSequenceDual)
        {
            StimulusSequenceA = new Rhs2116StimulusSequence(stimulusSequenceDual.StimulusSequenceA);
            StimulusSequenceB = new Rhs2116StimulusSequence(stimulusSequenceDual.StimulusSequenceB);
        }

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116Stimulus"/> array of stimuli.
        /// </summary>
        public Rhs2116Stimulus[] Stimuli
        {
            get { return StimulusSequenceA.Stimuli.Concat(StimulusSequenceB.Stimuli).ToArray(); }
            set
            {
                StimulusSequenceA.Stimuli = value.Take(16).ToArray();
                StimulusSequenceB.Stimuli = value.Skip(16).Take(16).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116StepSize"/>.
        /// </summary>
        public Rhs2116StepSize CurrentStepSize
        {
            get { return StimulusSequenceA.CurrentStepSize; }
            set
            {
                StimulusSequenceA.CurrentStepSize = value;
                StimulusSequenceB.CurrentStepSize = value;
            }
        }

        /// <summary>
        /// Gets the maximum length of the sequence across all channels.
        /// </summary>
        [XmlIgnore]
        public uint SequenceLengthSamples => Math.Max(StimulusSequenceA.SequenceLengthSamples, StimulusSequenceB.SequenceLengthSamples);

        /// <summary>
        /// Gets the maximum peak to peak amplitude of the sequence across all channels.
        /// </summary>
        [XmlIgnore]
        public int MaximumPeakToPeakAmplitudeSteps => Math.Max(StimulusSequenceA.MaximumPeakToPeakAmplitudeSteps, StimulusSequenceB.MaximumPeakToPeakAmplitudeSteps);

        /// <summary>
        /// Gets the boolean indicating if the stimulus sequence is well defined.
        /// </summary>
        [XmlIgnore]
        public bool Valid => StimulusSequenceA.Valid && StimulusSequenceB.Valid;

        /// <summary>
        /// Gets a boolean indicating if the sequence will fit in hardware.
        /// </summary>
        public bool FitsInHardware => StimulusSequenceA.FitsInHardware && StimulusSequenceB.FitsInHardware;

        /// <summary>
        /// Gets the the maximum number of hardware memory slots available.
        /// </summary>
        [XmlIgnore]
        public int MaxMemorySlotsAvailable => Rhs2116.StimMemorySlotsAvailable;

        /// <summary>
        /// Gets the number of hardware memory slots required by the sequence.
        /// </summary>
        [XmlIgnore]
        public int StimulusSlotsRequired => Math.Max(StimulusSequenceA.DeltaTable.Count, StimulusSequenceB.DeltaTable.Count);

        /// <summary>
        /// Gets the current step size in µA.
        /// </summary>
        [XmlIgnore]
        public double CurrentStepSizeuA => StimulusSequenceA.CurrentStepSizeuA;
    }
}
