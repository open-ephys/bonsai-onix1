using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 onboard electrical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Headstage64ElectricalStimulatorTrigger"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 onboard electrical stimulator.")]
    public class ConfigureHeadstage64ElectricalStimulator : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64ElectricalStimulator"/> class.
        /// </summary>
        public ConfigureHeadstage64ElectricalStimulator()
            : base(typeof(Headstage64ElectricalStimulator))
        {
        }

        /// <summary>
        /// Configure a headstage-64 onboard electrical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 onboard electrical stimulator.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Headstage64ElectricalStimulator
    {
        public const int ID = 4;

        // NB: could be read from REZ but these are constant
        public const double DacBitDepth = 16;
        public const double AbsMaxMicroAmps = 2500;

        // managed registers
        public const uint NULLPARM = 0; // No command
        public const uint BIPHASIC = 1; // Biphasic pulse (0 = monophasic, 1 = biphasic; NB: currently ignored)
        public const uint CURRENT1 = 2; // Phase 1 current
        public const uint CURRENT2 = 3; // Phase 2 current
        public const uint PULSEDUR1 = 4; // Phase 1 duration, 1 microsecond steps
        public const uint INTERPHASEINTERVAL = 5; // Inter-phase interval, 10 microsecond steps
        public const uint PULSEDUR2 = 6; // Phase 2 duration, 1 microsecond steps
        public const uint INTERPULSEINTERVAL = 7; // Inter-pulse interval, 10 microsecond steps
        public const uint BURSTCOUNT = 8; // Burst duration, number of pulses in burst
        public const uint INTERBURSTINTERVAL = 9; // Inter-burst interval, microseconds
        public const uint TRAINCOUNT = 10; // Pulse train duration, number of bursts in train
        public const uint TRAINDELAY = 11; // Pulse train delay, microseconds
        public const uint TRIGGER = 12; // Trigger stimulation (1 = deliver)
        public const uint POWERON = 13; // Control estim sub-circuit power (0 = off, 1 = on)
        public const uint ENABLE = 14; // If 0 then stimulation triggers will be ignored, otherwise they will be applied 
        public const uint RESTCURR = 15; // Resting current between pulse phases
        public const uint RESET = 16; // Reset all parameters to default
        public const uint REZ = 17; // Internal DAC resolution in bits

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64ElectricalStimulator))
            {
            }
        }
    }
}
