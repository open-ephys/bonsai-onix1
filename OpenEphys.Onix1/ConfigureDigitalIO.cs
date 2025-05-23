﻿using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <inheritdoc cref = "ConfigureDigitalIO"/>
    [Obsolete("Use ConfigureDigitalIO instead. This operator will be removed in version 1.0.0v")]
    public class ConfigureBreakoutDigitalIO : ConfigureDigitalIO { }

    /// <summary>
    /// Configures the ONIX breakout board's digital inputs and outputs.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to data IO operators, such as <see
    /// cref="DigitalInput"/> and <see cref="DigitalOutput"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures the ONIX breakout board's digital inputs and outputs.")]
    public class ConfigureDigitalIO : SingleDeviceFactory
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ConfigureDigitalIO"/> class.
        /// </summary>
        public ConfigureDigitalIO()
            : base(typeof(DigitalIO))
        {
            DeviceAddress = 7;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="DigitalInput"/> will produce data. If set to false, <see
        /// cref="DigitalInput"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the digital IO device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures the digital input and output device in the ONIX breakout board.
        /// </summary>
        /// <remarks>
        /// This will schedule digital IO hardware configuration actions that can be applied by a <see
        /// cref="StartAcquisition"/> object prior to data collection.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration
        /// actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure a
        /// digital IO device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(DigitalIO.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class DigitalIO
    {
        public const int ID = 18;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(DigitalIO))
            {
            }
        }
    }

    /// <summary>
    /// Specifies the state of the ONIX breakout board's digital input pins.
    /// </summary>
    [Flags]
    public enum DigitalPortState : byte
    {
        /// <summary>
        /// Specifies that pin 0 is high.
        /// </summary>
        Pin0 = 0x1,
        /// <summary>
        /// Specifies that pin 1 is high.
        /// </summary>
        Pin1 = 0x2,
        /// <summary>
        /// Specifies that pin 2 is high.
        /// </summary>
        Pin2 = 0x4,
        /// <summary>
        /// Specifies that pin 3 is high.
        /// </summary>
        Pin3 = 0x8,
        /// <summary>
        /// Specifies that pin 4 is high.
        /// </summary>
        Pin4 = 0x10,
        /// <summary>
        /// Specifies that pin 5 is high.
        /// </summary>
        Pin5 = 0x20,
        /// <summary>
        /// Specifies that pin 6 is high.
        /// </summary>
        Pin6 = 0x40,
        /// <summary>
        /// Specifies that pin 7 is high.
        /// </summary>
        Pin7 = 0x80,
    }

    /// <summary>
    /// Specifies the state of the ONIX breakout board's switches and buttons.
    /// </summary>
    [Flags]
    public enum BreakoutButtonState : ushort
    {
        /// <summary>
        /// Specifies that the ☾ key is depressed.
        /// </summary>
        Moon = 0x1,
        /// <summary>
        /// Specifies that the △ key is depressed.
        /// </summary>
        Triangle = 0x2,
        /// <summary>
        /// Specifies that the × key is depressed.
        /// </summary>
        X = 0x4,
        /// <summary>
        /// Specifies that the ✓ key is depressed.
        /// </summary>
        Check = 0x8,
        /// <summary>
        /// Specifies that the ◯ key is depressed.
        /// </summary>
        Circle = 0x10,
        /// <summary>
        /// Specifies that the □ key is depressed.
        /// </summary>
        Square = 0x20,
        /// <summary>
        /// Specifies that reserved bit 0 is high.
        /// </summary>
        Reserved0 = 0x40,
        /// <summary>
        /// Specifies that reserved bit 1 is high.
        /// </summary>
        Reserved1 = 0x80,
        /// <summary>
        /// Specifies that port D power switch is set to on.
        /// </summary>
        PortDOn = 0x100,
        /// <summary>
        /// Specifies that port C power switch is set to on.
        /// </summary>
        PortCOn = 0x200,
        /// <summary>
        /// Specifies that port B power switch is set to on.
        /// </summary>
        PortBOn = 0x400,
        /// <summary>
        /// Specifies that port A power switch is set to on.
        /// </summary>
        PortAOn = 0x800,
    }
}
