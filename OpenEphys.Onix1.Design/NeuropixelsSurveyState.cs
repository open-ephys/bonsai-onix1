using System.Threading;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Electrode-survey configuration and run state, shared across every probe a headstage hosts. Per-probe
    /// results/status live each probe.
    /// </summary>
    internal sealed class NeuropixelsSurveyState
    {
        /// <summary>
        /// Time at the start of each bank, in seconds, during which the amplifiers settle after the probe is
        /// configured. Data from this period is filtered (so the filter's own start-up transient falls inside
        /// it too) but left out of the bank's statistics.
        /// </summary>
        internal const float SettlingSeconds = 5f;


        internal string Driver { get; set; } = "riffa";
        internal int HubIndex { get; set; }
        internal PortName Port { get; set; } = PortName.PortA;
        internal double? PortVoltage { get; set; }
        internal bool EditingHardwareAddr { get; set; }
        internal double SpikeThreshold { get; set; } = SpikeDetection.DefaultThreshold;
        internal float TimePerBankSeconds { get; set; } = 5f;
        internal bool RecordSurveyData { get; set; }
        internal CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// How strictly hardware validation checks are enforced during a scan or survey. Independent of the
        /// workflow's <see cref="CreateContext.ValidationLevel"/>, and reset each time the window opens.
        /// </summary>
        internal ValidationLevel ValidationLevel { get; set; } = ValidationLevel.Normal;
    }
}
