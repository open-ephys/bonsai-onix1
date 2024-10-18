using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an ONIX Rhs2116 Headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The Rhs2116 Headstage is a serialized headstage for small animals with 32 bi-directional channels 
    /// which each can be used to deliver electrical stimuli. The Rhs2116 Headstage can be used with passive 
    /// probes (e.g. silicon arrays, EEG/ECOG arrays, etc) using a 36-Channel Omnetics EIB. It provides the
    /// following features:
    /// <list type="bullet">
    /// <item><description>Two, synchronized Rhs2116 ICs for a combined 32 bidirectional ephys channels.</description></item>
    /// <item><description>Real-time control of stimulation sequences.</description></item>
    /// <item><description>Real-time control of filter settings and artifact recovery parameters.</description></item>
    /// <item><description>~1 millisecond active stimulus artifact recovery.</description></item>
    /// <item><description>Max stimulator current: 2.55mA @ +/-7V compliance.</description></item>
    /// <item><description>Sample rate: 30193.2367 Hz.</description></item>
    /// <item><description>Stimulus active and stimulus trigger pins.</description></item>
    /// <item><description>On-board Lattice Crosslink FPGA for real-time data arbitration.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.HeadstageRhs2116Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureHeadstageRhs2116 : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageRhs2116LinkController LinkController = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageRhs2116"/> class.
        /// </summary>
        public ConfigureHeadstageRhs2116()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
        }

        /// <summary>
        /// Gets or sets the Rhs2116Pair configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureRhs2116Pair Rhs2116Pair { get; set; } = new();

        /// <summary>
        /// Gets or sets the Stimulus Trigger configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureRhs2116Trigger StimulusTrigger { get; set; } = new();

        internal override void UpdateDeviceNames()
        {
            LinkController.DeviceName = GetFullDeviceName(nameof(LinkController));
            Rhs2116Pair.DeviceName = GetFullDeviceName(nameof(Rhs2116Pair));
            StimulusTrigger.DeviceName = GetFullDeviceName(nameof(StimulusTrigger));
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to
        /// operation.
        /// </remarks>
        [Category(ConfigurationCategory)]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhs2116Pair.DeviceAddress = HeadstageRhs2116.GetRhs2116ADeviceAddress(offset);
                StimulusTrigger.DeviceAddress = HeadstageRhs2116.GetRhs2116StimulusTriggerDeviceAddress(offset);
            }
        }

        /// <summary>
        /// Gets or sets the port voltage override.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If defined, it will override automated voltage discovery and apply the specified voltage to the
        /// headstage. If left blank, an automated headstage detection algorithm will attempt to communicate
        /// with the headstage and apply an appropriate voltage for stable operation. Because ONIX allows any
        /// coaxial tether to be used, some of which are thin enough to result in a significant voltage drop,
        /// its may be required to manually specify the port voltage.
        /// </para>
        /// <para>
        /// Warning: this device requires 3.4V to 4.4V, measured at the headstage, for proper operation.
        /// Supplying higher voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 3.4V to 4.4V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        [Category(ConfigurationCategory)]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhs2116Pair;
            yield return StimulusTrigger;
        }

        class ConfigureHeadstageRhs2116LinkController : ConfigurePortController
        {
            protected override bool ConfigurePortVoltage(DeviceContext device)
            {
                const double MinVoltage = 3.3;
                const double MaxVoltage = 4.4;
                const double VoltageOffset = 2.0;
                const double VoltageIncrement = 0.2;

                for (var voltage = MinVoltage; voltage <= MaxVoltage; voltage += VoltageIncrement)
                {
                    SetPortVoltage(device, voltage);
                    if (base.CheckLinkState(device))
                    {
                        SetPortVoltage(device, voltage + VoltageOffset);
                        return CheckLinkState(device);
                    }
                }

                return false;
            }

            private void SetPortVoltage(DeviceContext device, double voltage)
            {
                device.WriteRegister(PortController.PORTVOLTAGE, 0);
                Thread.Sleep(500);
                device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
                Thread.Sleep(500);
            }

            protected override bool CheckLinkState(DeviceContext device)
            {
                // NB: The Rhs2116 headstage needs an additional reset after power on to provide its device table.
                device.Context.Reset();
                var linkState = device.ReadRegister(PortController.LINKSTATE);
                return (linkState & PortController.LINKSTATE_SL) != 0;
            }
        }
    }

    internal static class HeadstageRhs2116
    {
        public static uint GetRhs2116ADeviceAddress(uint baseAddress) => baseAddress + 0;
        public static uint GetRhs2116BDeviceAddress(uint baseAddress) => baseAddress + 1;
        public static uint GetRhs2116StimulusTriggerDeviceAddress(uint baseAddress) => baseAddress + 2;
    }
}
