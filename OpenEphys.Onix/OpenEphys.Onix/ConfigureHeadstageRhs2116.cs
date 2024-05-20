using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Drawing.Design;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix
{
    [DefaultProperty(nameof(ChannelConfiguration))]
    public class ConfigureHeadstageRhs2116 : HubDeviceFactory
    {
        PortName port;
        ProbeGroup probeGroup = null;
        readonly ConfigureHeadstageRhs2116LinkController LinkController = new();

        public ConfigureHeadstageRhs2116()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the voltage required for full
            // functionality. Locking occurs at around 2V on the headstage (enough to turn 1.8V on). Full functionality is at 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            ChannelConfiguration = probeGroup;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhs2116 Rhs2116A { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhs2116 Rhs2116B { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhs2116Trigger StimulusTrigger { get; set; } = new();

        internal override void UpdateDeviceNames()
        {
            LinkController.DeviceName = GetFullDeviceName(nameof(LinkController));
            Rhs2116A.DeviceName = GetFullDeviceName(nameof(Rhs2116A));
            Rhs2116B.DeviceName = GetFullDeviceName(nameof(Rhs2116B));
            StimulusTrigger.DeviceName = GetFullDeviceName(nameof(StimulusTrigger));
        }

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhs2116A.DeviceAddress = HeadstageRhs2116.GetRhs2116ADeviceAddress(offset);
                Rhs2116B.DeviceAddress = HeadstageRhs2116.GetRhs2116BDeviceAddress(offset);
                StimulusTrigger.DeviceAddress = HeadstageRhs2116.GetRhs2116StimulusTriggerDeviceAddress(offset);
            }
        }

        [Editor("OpenEphys.Onix.Design.ChannelConfigurationEditor, OpenEphys.Onix.Design", typeof(UITypeEditor))]
        public ProbeGroup ChannelConfiguration
        {
            get { return probeGroup; }
            set
            {
                probeGroup = value;
                StimulusTrigger.ChannelConfiguration = value;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhs2116A;
            yield return Rhs2116B;
            yield return StimulusTrigger;
        }

        class ConfigureHeadstageRhs2116LinkController : ConfigureFmcLinkController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const uint MinVoltage = 33;
                const uint MaxVoltage = 50;
                const uint VoltageOffset = 25;
                const uint VoltageIncrement = 02;

                for (uint voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (CheckLinkState(device))
                    {
                        SetPortVoltage(device, voltage + VoltageOffset);
                        if (CheckLinkState(device))
                        {
                            // TODO: The RHS2116 headstage needs an additional reset after power on
                            // to provide its device table.
                            Thread.Sleep(500);
                            device.Context.Reset();
                            return true;
                        }
                        else break;
                    }
                }

                return false;
            }

            private void SetPortVoltage(DeviceContext device, uint voltage)
            {
                const int WaitUntilVoltageOffSettles = 500;
                const int WaitUntilVoltageOnSettles = 500;
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, 0);
                Thread.Sleep(WaitUntilVoltageOffSettles);
                device.WriteRegister(FmcLinkController.PORTVOLTAGE, voltage);
                Thread.Sleep(WaitUntilVoltageOnSettles);
            }
        }
    }

    internal static class HeadstageRhs2116
    {
      public static uint GetRhs2116ADeviceAddress(uint baseaddress) => baseaddress + 0;
      public static uint GetRhs2116BDeviceAddress(uint baseaddress) => baseaddress + 1;
      public static uint GetRhs2116StimulusTriggerDeviceAddress(uint baseaddress) => baseaddress + 2;
    }
}
