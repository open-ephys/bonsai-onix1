using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a single stimulus sequence for a channel on an Rhs2116 device.
    /// </summary>
    public class Rhs2116Stimulus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116Stimulus"/> class.
        /// </summary>
        public Rhs2116Stimulus() { }

        /// <summary>
        /// Construct a new <see cref="Rhs2116Stimulus"/> instance with the same parameters as the given <see cref="Rhs2116Stimulus"/> object.
        /// </summary>
        /// <param name="stimulus"></param>
        public Rhs2116Stimulus(Rhs2116Stimulus stimulus)
        {
            NumberOfStimuli = stimulus.NumberOfStimuli;
            AnodicFirst = stimulus.AnodicFirst;
            DelaySamples = stimulus.DelaySamples;
            DwellSamples = stimulus.DwellSamples;
            AnodicAmplitudeSteps = stimulus.AnodicAmplitudeSteps;
            AnodicWidthSamples = stimulus.AnodicWidthSamples;
            CathodicAmplitudeSteps = stimulus.CathodicAmplitudeSteps;
            CathodicWidthSamples = stimulus.CathodicWidthSamples;
            InterStimulusIntervalSamples = stimulus.InterStimulusIntervalSamples;
        }

        /// <summary>
        /// Number of stimuli delivered for each trigger.
        /// </summary>
        [DisplayName("Number of Stimuli")]
        public uint NumberOfStimuli { get; set; } = 0;

        /// <summary>
        /// Send an anodic pulse first if true.
        /// </summary>
        [DisplayName("Anodic First")]
        public bool AnodicFirst { get; set; } = true;

        /// <summary>
        /// Number of samples to delay before sending the first pulse after a trigger is received.
        /// </summary>
        [DisplayName("Delay (samples)")]
        public uint DelaySamples { get; set; } = 0;

        /// <summary>
        /// Number of samples between anodic and cathodic pulses (inter-pulse interval).
        /// </summary>
        [DisplayName("Dwell (samples)")]
        public uint DwellSamples { get; set; } = 0;

        /// <summary>
        /// Number of steps defining the amplitude of the anodic pulse. See <see cref="Rhs2116StepSize"/>
        /// to see the amplitude per step.
        /// </summary>
        [DisplayName("Anodic Current (steps)")]
        public byte AnodicAmplitudeSteps { get; set; } = 0;

        /// <summary>
        /// Number of samples the anodic pulse is delivered.
        /// </summary>
        [DisplayName("Anodic Width (samples)")]
        public uint AnodicWidthSamples { get; set; } = 0;

        /// <summary>
        /// Number of steps defining the amplitude of the cathodic pulse. See <see cref="Rhs2116StepSize"/>
        /// to see the amplitude per step.
        /// </summary>
        [DisplayName("Cathodic Current (steps)")]
        public byte CathodicAmplitudeSteps { get; set; } = 0;

        /// <summary>
        /// Number of samples the cathodic pulse is delivered.
        /// </summary>
        [DisplayName("Cathodic Width (samples)")]
        public uint CathodicWidthSamples { get; set; } = 0;

        /// <summary>
        /// Number of samples between pairs of pulses.
        /// </summary>
        [DisplayName("Inter Stimulus Interval (samples)")]
        public uint InterStimulusIntervalSamples { get; set; } = 0;

        /// <summary>
        /// Validates the stimulus sequence and returns a result containing
        /// all reasons for invalidity, if any.
        /// </summary>
        Rhs2116StimulusValidationResult Validate()
        {
            var reasons = new List<string>();

            if (NumberOfStimuli == 0)
            {
                // A zeroed-out / disabled stimulus is valid but only if
                // every other field is also zeroed out.
                if (CathodicWidthSamples != 0)
                    reasons.Add("Stimuli = 0, Cathodic Width > 0");
                if (AnodicWidthSamples != 0)
                    reasons.Add("Stimuli = 0, Anodic Width > 0");
                if (InterStimulusIntervalSamples != 0)
                    reasons.Add("Stimuli = 0, ISI > 0");
                if (AnodicAmplitudeSteps != 0)
                    reasons.Add("Stimuli = 0, Anodic Steps > 0");
                if (CathodicAmplitudeSteps != 0)
                    reasons.Add("Stimuli = 0, Cathodic Steps > 0");
                if (DelaySamples != 0)
                    reasons.Add("Stimuli = 0, Delay > 0");
                if (DwellSamples != 0)
                    reasons.Add("Stimuli = 0, Dwell (Inter-Pulse) > 0");
            }
            else
            {
                if (AnodicWidthSamples == 0 && AnodicAmplitudeSteps > 0)
                    reasons.Add("Anodic Width = 0, Anodic Steps > 0");
                if (CathodicWidthSamples == 0 && CathodicAmplitudeSteps > 0)
                    reasons.Add("Cathodic Width = 0, Cathodic Steps > 0");
                if (AnodicWidthSamples == 0 && CathodicWidthSamples == 0)
                    reasons.Add("Pulse has zero width");
                if (NumberOfStimuli > 1 && InterStimulusIntervalSamples == 0)
                    reasons.Add("ISI = 0, Stimuli > 1");
            }

            return new Rhs2116StimulusValidationResult(reasons);
        }

        /// <summary>
        /// Returns true if the sequence is valid.
        /// </summary>
        public bool IsValid() => Validate().IsValid;

        /// <summary>
        /// Returns true if the sequence is valid, and provides the first reason
        /// for invalidity if not. Kept for backwards compatibility.
        /// </summary>
        public bool IsValid(out string reasonInvalid)
        {
            var result = Validate();
            reasonInvalid = result.IsValid ? "" : result.Reasons[0];
            return result.IsValid;
        }

        [XmlIgnore]
        internal bool Valid => Validate().IsValid;

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public void Clear()
        {
            NumberOfStimuli = 0;
            AnodicFirst = true;
            DelaySamples = 0;
            DwellSamples = 0;
            AnodicAmplitudeSteps = 0;
            AnodicWidthSamples = 0;
            CathodicAmplitudeSteps = 0;
            CathodicWidthSamples = 0;
            InterStimulusIntervalSamples = 0;
        }

        /// <summary>
        /// Performs a shallow copy of the sequence using <c>MemberwiseClone()</c>.
        /// </summary>
        /// <returns>An identical sequence to the current sequence.</returns>
        public Rhs2116Stimulus Clone()
        {
            return (Rhs2116Stimulus)MemberwiseClone();
        }
    }

    /// <summary>
    /// Holds the outcome of a stimulus sequence validation, including
    /// all reasons for invalidity.
    /// </summary>
    public sealed class Rhs2116StimulusValidationResult
    {
        /// <summary>
        /// Represents a Rhs2116StimulusValidationResult with <see cref="IsValid"/> <lang>true</lang>.
        /// </summary>
        public static readonly Rhs2116StimulusValidationResult Ok = new(new List<string>());

        internal Rhs2116StimulusValidationResult(IReadOnlyList<string> reasons)
        {
            Reasons = reasons;
        }

        /// <summary>True if the sequence is valid.</summary>
        public bool IsValid => Reasons.Count == 0;

        /// <summary>All reasons the sequence is invalid. Empty when valid.</summary>
        public IReadOnlyList<string> Reasons { get; }

        /// <summary>
        /// Convenience: all reasons joined into a single human-readable string.
        /// </summary>
        public string Summary => string.Join("; ", Reasons);
    }
}
