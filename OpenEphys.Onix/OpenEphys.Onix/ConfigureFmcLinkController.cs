using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace OpenEphys.Onix
{
    public class ConfigureFmcLinkController : SingleDeviceFactory
    {
        public ConfigureFmcLinkController()
            : base(typeof(FmcLinkController))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the link controller device is enabled.")]
        public bool Enable { get; set; } = true;

        public bool Passthrough { get; set; }

        public bool GPO1 { get; set; }

        public int MinVoltage { get; set; }

        public int MaxVoltage { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceAddress = DeviceAddress;
            return source.ConfigureLink(context =>
            {
                var device = context.GetDevice(deviceAddress, FmcLinkController.ID);
                context.WriteRegister(deviceAddress, FmcLinkController.ENABLE, 1);
                context.WriteRegister(deviceAddress, FmcLinkController.PORTVOLTAGE, 0);

                for (int v = MinVoltage; v < MaxVoltage; v += 2)
                {
                    context.ReadRegister(deviceAddress, FmcLinkController.LINKSTATE);
                }
            }).ConfigureDevice(context =>
            {
                return Disposable.Empty;
            });
        }

        static class FmcLinkController
        {
            public const int ID = 23;

            public const uint ENABLE = 0; // The LSB is used to enable or disable the device data stream
            public const uint GPOSTATE = 1; // GPO output state (bits 31 downto 3: ignore. bits 2 downto 0: ‘1’ = high, ‘0’ = low)
            public const uint DESPWR = 2; // Set link deserializer PDB state, 0 = deserializer power off else on. Does not affect port voltage.
            public const uint PORTVOLTAGE = 3; // 10 * link voltage
            public const uint SAVEVOLTAGE = 4; // Save link voltage to non-volatile EEPROM if greater than 0. This voltage will be applied after POR.
            public const uint LINKSTATE = 5; // bit 1 pass; bit 0 lock
        }
    }
}
