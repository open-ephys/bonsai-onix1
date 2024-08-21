﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that produces a sequence of port status information frames.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate headstage,
    /// miniscope, etc. configuration whose communication is dictated by
    /// a <see cref="PortController"/>.
    /// </remarks>
    [Description("Produces a sequence of port status information.")]
    public class PortStatus : Source<PortStatusFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(PortController.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="PortStatusFrame"/> objects, which contains information
        /// about the the a headstage port communication status.
        /// </summary>
        /// <remarks>
        /// A <see cref="PortStatusFrame"/> will be produced only in exceptional circumstances. For
        /// instance,  when the headstage becomes disconnected, a packet fails a CRC check, etc.
        /// </remarks>
        /// <returns>A sequence of <see cref="PortStatusFrame"/> objects.</returns>
        public override IObservable<PortStatusFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(PortController));

                return deviceInfo.Context
                    .GetDeviceFrames(device.Address)
                    .Select(frame => new PortStatusFrame(frame));
            });
        }
    }
}