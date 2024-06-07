using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix
{
    public class ConfigureHeadstage64 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstage64LinkController LinkController = new();

        public ConfigureHeadstage64()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the voltage required for full
            // functionality. Locking occurs at around 2V on the headstage (enough to turn 1.8V on). Full functionality is at 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureTS4231 TS4231 { get; set; } = new() { Enable = false };

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureHeadstage64ElectricalStimulator ElectricalStimulator { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhd2164.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
                TS4231.DeviceAddress = offset + 2;
                ElectricalStimulator.DeviceAddress = offset + 3;
            }
        }

        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 5.5V to 6.0V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhd2164;
            yield return Bno055;
            yield return TS4231;
            yield return ElectricalStimulator;
        }

        class ConfigureHeadstage64LinkController : ConfigureFmcLinkController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                // TODO: It takes a huge amount of time to get to 0, almost 10 seconds.
                // The best we can do at the moment is drive port voltage to minimum which
                // is an active process and then settle from there to zero volts.
                const uint MinVoltage = 33;
                const uint MaxVoltage = 60;
                const uint VoltageOffset = 34;
                const uint VoltageIncrement = 02;

                // Start with highest voltage and ramp it down to find lowest lock voltage
                var voltage = MaxVoltage;
                for (; voltage >= MinVoltage; voltage -= VoltageIncrement)
                {
                    device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage);
                    Thread.Sleep(200);
                    if (!CheckLinkState(device))
                    {
                        if (voltage == MaxVoltage) return false;
                        else break;
                    }
                }

                device.WriteRegister(FmcLinkController.PORTVOLTAGE, MinVoltage);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                Thread.Sleep(1000);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage + VoltageOffset);
                Thread.Sleep(200);
                return CheckLinkState(device);
            }
        }
    }

    public enum PortName
    {
        PortA = 1,
        PortB = 2
    }
}
