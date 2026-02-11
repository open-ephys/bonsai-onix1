using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Trigger a headstage-64 stimulator using the port controller's general purpose output (GPO).
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstage64.Headstage64PortController"/>, using a shared <c>DeviceName</c>. Each
    /// headstage port has a GPO interface for sending digital signals to the headstage with low latency. This
    /// operator uses a dedicated GPO line rather than a register write to trigger stimulation and therefore
    /// has lower latencies than <see cref="Headstage64ElectricalStimulatorTrigger"/> or <see
    /// cref="Headstage64OpticalStimulatorTrigger"/>. However, the trigger will be delivered to both of the
    /// stimulators so care must be taken to ensure only the appropriate stimulator is armed when the trigger
    /// is sent. 
    /// </remarks>
    [Description("Triggers a headstage-64 stimulator using the port controller's general purpose output (GPO)")]
    public class Headstage64GpoTrigger : Sink<bool>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(ConfigureHeadstage64.Headstage64PortController.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Issue a stimulus trigger using the port controller's GPO.
        /// </summary>
        /// <param name="source"> A sequence of boolean values indicating if a stimulus trigger should be
        /// issued.</param>
        /// <returns> A sequence that is identical to <paramref name="source"/>.</returns>
        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(ConfigureHeadstage64.Headstage64PortController));
                return source.Do(value => {
                    if (value)
                    {
                        device.WriteRegister(PortController.GPOSTATE, (byte)PortControllerGpioState.Pin1);
                        device.WriteRegister(PortController.GPOSTATE, 0);
                    }
                });
            });
        }
    }
}
