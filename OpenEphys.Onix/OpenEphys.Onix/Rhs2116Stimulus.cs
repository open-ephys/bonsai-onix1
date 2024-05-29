using System.ComponentModel;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    public class Rhs2116Stimulus
    {
        [DisplayName("Number of Stimuli")]
        public uint NumberOfStimuli { get; set; } = 0;

        [DisplayName("Anodic First")]
        public bool AnodicFirst { get; set; } = true;

        [DisplayName("Delay (samples)")]
        public uint DelaySamples { get; set; } = 0;

        [DisplayName("Dwell (samples)")]
        public uint DwellSamples { get; set; } = 0;

        [DisplayName("Anodic Current (steps)")]
        public byte AnodicAmplitudeSteps { get; set; } = 0;

        [DisplayName("Anodic Width (samples)")]
        public uint AnodicWidthSamples { get; set; } = 0;

        [DisplayName("Cathodic Current (steps)")]
        public byte CathodicAmplitudeSteps { get; set; } = 0;

        [DisplayName("Cathodic Width (samples)")]
        public uint CathodicWidthSamples { get; set; } = 0;

        [DisplayName("Inter Stimulus Interval (samples)")]
        public uint InterStimulusIntervalSamples { get; set; } = 0;

        public bool IsValid()
        {
            return Valid;
        }

        public bool Clear()
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

            return true;
        }

        [XmlIgnore]
        internal bool Valid
        {
            get
            {
                return NumberOfStimuli == 0
                    ? DelaySamples == 0 && CathodicWidthSamples == 0 && InterStimulusIntervalSamples == 0 && AnodicAmplitudeSteps == 0 && CathodicAmplitudeSteps == 0
                    : !(AnodicWidthSamples == 0 && AnodicAmplitudeSteps > 0)
                      &&
                      !(AnodicWidthSamples > 0 && AnodicAmplitudeSteps == 0)
                      &&
                      !(CathodicWidthSamples == 0 && CathodicAmplitudeSteps > 0)
                      &&
                      !(CathodicWidthSamples > 0 && CathodicAmplitudeSteps == 0)
                      &&
                      //           Non-zero anodic                          or               Non-zero cathodic
                      ((AnodicWidthSamples > 0 && AnodicAmplitudeSteps > 0) || (CathodicWidthSamples > 0 && CathodicAmplitudeSteps > 0))
                      &&
                      //         Single pulse and possibly 0 ISI                   or          Multiple pulse and positive ISI
                      ((NumberOfStimuli == 1 && InterStimulusIntervalSamples >= 0) || (NumberOfStimuli > 1 && InterStimulusIntervalSamples > 0));

            }
        }

        public Rhs2116Stimulus Clone()
        {
            return (Rhs2116Stimulus)MemberwiseClone();
        }
    }

}
