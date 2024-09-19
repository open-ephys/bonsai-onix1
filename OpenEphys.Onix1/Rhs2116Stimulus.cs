using System.ComponentModel;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a single stimulus sequence for a channel on an RHS2116 device.
    /// </summary>
    public class Rhs2116Stimulus
    {
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
        /// Gets the status of this sequence.
        /// </summary>
        /// <returns>True if the sequence is valid, false if it is not valid.</returns>
        public bool IsValid()
        {
            return Valid;
        }

        /// <summary>
        /// Gets the status of this sequence.
        /// </summary>
        /// <param name="reasonInvalid">If the return value is true, this value is an empty 
        /// string. If the return is false, this will list the first reason why the sequence is invalid</param>
        /// <returns>True if the sequence is valid, false if it is not valid.</returns>
        public bool IsValid(out string reasonInvalid)
        {
            reasonInvalid = "";

            var valid = Valid;

            if (valid == false)
            {
                if (NumberOfStimuli == 0)
                {
                    if (CathodicWidthSamples != 0)
                        reasonInvalid = "Stimuli = 0, Cathodic Width > 0";

                    else if (AnodicWidthSamples != 0)
                        reasonInvalid = "Stimuli = 0, Anodic Width > 0";

                    else if (InterStimulusIntervalSamples != 0)
                        reasonInvalid = "Stimuli = 0, ISI > 0";

                    else if (AnodicAmplitudeSteps != 0)
                        reasonInvalid = "Stimuli = 0, Anodic Steps > 0";

                    else if (CathodicAmplitudeSteps != 0)
                        reasonInvalid = "Stimuli = 0, Cathodic Steps > 0";

                    else if (DelaySamples != 0)
                        reasonInvalid = "Stimuli = 0, Delay > 0";

                    else if (DwellSamples != 0)
                        reasonInvalid = "Stimuli = 0, Dwell (Inter-Pulse) > 0";
                }
                else if (AnodicWidthSamples == 0 && AnodicAmplitudeSteps > 0)
                    reasonInvalid = "Anodic Width = 0, Anodic Steps > 0";

                else if (CathodicWidthSamples == 0 && CathodicAmplitudeSteps > 0)
                    reasonInvalid = "Cathodic Width = 0, Cathodic Steps > 0";

                else if (NumberOfStimuli > 1 && InterStimulusIntervalSamples == 0)
                    reasonInvalid = "ISI = 0, Stimuli > 1";

            }

            return valid;
        }

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

        [XmlIgnore]
        internal bool Valid
        {
            get
            {
                return NumberOfStimuli == 0
                    ? CathodicWidthSamples == 0 && AnodicWidthSamples == 0 && InterStimulusIntervalSamples == 0 &&
                      AnodicAmplitudeSteps == 0 && CathodicAmplitudeSteps == 0 && DelaySamples == 0 && DwellSamples == 0
                    : !(AnodicWidthSamples == 0 && AnodicAmplitudeSteps > 0) &&
                      !(CathodicWidthSamples == 0 && CathodicAmplitudeSteps > 0) &&
                      ((NumberOfStimuli == 1 && InterStimulusIntervalSamples >= 0) || (NumberOfStimuli > 1 && InterStimulusIntervalSamples > 0));
            }
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
}
