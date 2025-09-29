using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 optical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Headstage64OpticalStimulatorTrigger"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 optical stimulator.")]
    public class ConfigureHeadstage64OpticalStimulator : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64OpticalStimulator"/> class.
        /// </summary>
        public ConfigureHeadstage64OpticalStimulator()
            : base(typeof(Headstage64OpticalStimulator))
        {
        }

        /// <summary>
        /// Gets or sets the data enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Headstage64OpticalStimulatorData"/> will produce data. If set to
        /// false, <see cref="Headstage64OpticalStimulatorData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the headstage-64 optical stimulator will produce stimulus reports.")]
        public bool Enable { get; set; }

        /// <summary>
        /// Configure a headstage-64 dual-channel optical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 dual-channel optical stimulator.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var enable = Enable;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Headstage64OpticalStimulator.ENABLE, enable ? 1u : 0u);
                device.WriteRegister(Headstage64OpticalStimulator.STIMENABLE, 0u);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Headstage64OpticalStimulator
    {
        public const int ID = 5;
        public const uint MinimumVersion = 3;

        // NB: can be read with MINRHEOR and POTRES, but will not change
        public const uint MinRheostatResistanceOhms = 590;
        public const uint PotResistanceOhms = 100_000;

        // managed registers
        public const uint ENABLE = 0; // Enable stimulus report stream
        public const uint MAXCURRENT = 1; // Max LED/LD current, (0 to 255 = 800mA to 0 mA.See fig XX of CAT4016 datasheet)
        public const uint PULSEMASK = 2; // Bitmask determining which of the(up to 32) channels is affected by trigger
        public const uint PULSEDUR = 3; // Pulse duration, microseconds
        public const uint PULSEPERIOD = 4; // Inter-pulse interval, microseconds
        public const uint BURSTCOUNT = 5; // Number of pulses in burst
        public const uint IBI = 6; // Inter-burst interval, microseconds
        public const uint TRAINCOUNT = 7; // Number of bursts in train
        public const uint TRAINDELAY = 8; // Stimulus start delay, microseconds
        public const uint TRIGGER = 9; // Trigger stimulation (0 = off, 1 = deliver)
        public const uint STIMENABLE = 10; // 1: enables the stimulator, 0: stimulator ignores triggers (so that a common trigger can be used)
        public const uint RESTMASK = 11; // Bitmask determining the off state of the up to 32 current channels
        public const uint RESET = 12; // None If 1, Reset all parameters to default (not implemented)
        public const uint MINRHEOR = 13; // The series resistor between the potentiometer (rheostat) and RSET bin on the CAT4016
        public const uint POTRES = 14; // The resistance value of the potentiometer connected in rheostat config to RSET on CAT4016

        // NB: fit from Fig. 10 of CAT4016 datasheet
        // x = (y/a)^(1/b)
        // a = 3.833e+05
        // b = -0.9632
        internal static uint MilliampsToPotSetting(double currentMa)
        {
            double R = Math.Pow(currentMa / 3.833e+05, 1 / -0.9632);
            uint s = (uint)Math.Round(256 * (R - MinRheostatResistanceOhms) / PotResistanceOhms);
            return s > 255 ? 255 : s < 0 ? 0 :s;
        }

        internal static double PotSettingToMilliamps(uint potSetting)
        {
            var R = MinRheostatResistanceOhms + PotResistanceOhms * potSetting / 256; 
            return 3.833e+05 * Math.Pow(R, -0.9632);
        }

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64OpticalStimulator))
            {
            }
        }
    }
}
