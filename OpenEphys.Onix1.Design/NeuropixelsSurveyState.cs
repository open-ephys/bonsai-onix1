using System.Threading;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Electrode-survey configuration and run state, shared across every probe a headstage hosts. Per-probe
    /// results/status live each probe.
    /// </summary>
    internal sealed class NeuropixelsSurveyState
    {
        internal string Driver { get; set; } = "riffa";
        internal int HubIndex { get; set; }
        internal PortName Port { get; set; } = PortName.PortA;
        internal double? PortVoltage { get; set; }
        internal bool EditingHardwareAddr { get; set; }
        internal double SpikeThreshold { get; set; } = -50;
        internal float TimePerBankSeconds { get; set; } = 5f;
        internal bool RecordSurveyData { get; set; }
        internal CancellationTokenSource Cts { get; set; }
    }
}
