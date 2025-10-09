using System;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A pair of stimulus sequences for two Rhs2116 devices.
    /// </summary>
    public class Rhs2116StimulusSequencePair
    {
        internal Rhs2116StimulusSequence StimulusSequenceA { get; }
        internal Rhs2116StimulusSequence StimulusSequenceB { get; }

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
        /// Initializes a new instance of the <see cref="Rhs2116StimulusSequencePair"/> by performing a
        /// shallow copy of the reference sequences.
        /// </summary>
        /// <param name="stimulusSequenceA">Existing <see cref="Rhs2116StimulusSequence"/> for sequence A.</param>
        /// <param name="stimulusSequenceB">Existing <see cref="Rhs2116StimulusSequence"/> for sequence B.</param>
        public Rhs2116StimulusSequencePair(Rhs2116StimulusSequence stimulusSequenceA, Rhs2116StimulusSequence stimulusSequenceB)
        {
            StimulusSequenceA = new Rhs2116StimulusSequence(stimulusSequenceA);
            StimulusSequenceB = new Rhs2116StimulusSequence(stimulusSequenceB);
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
        /// Updates the stimulus at the given index.
        /// </summary>
        /// <remarks>
        /// This is necessary to change the values of individual stimuli, since the implementation for getting <see cref="Stimuli"/> does not 
        /// allow for the underlying <see cref="Rhs2116StimulusSequence.Stimuli"/> to be updated.
        /// </remarks>
        /// <param name="stimulus">Current <see cref="Rhs2116Stimulus"/> to copy.</param>
        /// <param name="index">Zero-indexed value of the channel to update.</param>
        /// <exception cref="IndexOutOfRangeException">Index must be between 0 and the sum of the number of elements in
        /// <see cref="StimulusSequenceA"/> and <see cref="StimulusSequenceB"/>.</exception>
        internal void UpdateStimulus(Rhs2116Stimulus stimulus, int index)
        {
            if (index >= Stimuli.Length || index < 0)
            {
                throw new IndexOutOfRangeException("Index is outside of the range of stimuli. Must be less than " + Stimuli.Length.ToString() + ", and greater than zero.");
            }

            if (index < StimulusSequenceA.Stimuli.Length)
            {
                StimulusSequenceA.Stimuli[index] = stimulus.Clone();
            }
            else
            {
                StimulusSequenceB.Stimuli[index - StimulusSequenceA.Stimuli.Length] = stimulus.Clone();
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

        /// <summary>
        /// Gets the maximum peak-to-peak amplitude across all stimuli.
        /// </summary>
        /// <returns>Double containing the maximum peak-to-peak amplitude in µA.</returns>
        public double GetMaxPeakToPeakAmplitudeuA()
        {
            return Math.Max(StimulusSequenceA.GetMaxPeakToPeakAmplitudeuA(), StimulusSequenceB.GetMaxPeakToPeakAmplitudeuA());
        }
    }
}
