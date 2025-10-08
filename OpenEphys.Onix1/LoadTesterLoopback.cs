using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Sends loopback data to the load testing device for closed-loop latency measurement.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureLoadTester"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Sends loopback data to an ONIX breakout board.")]
    public class LoadTesterLoopback : Sink<ulong>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(LoadTester.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Creates and sends a loopback frame to the load testing device.
        /// </summary>
        /// <remarks>
        /// A loopback frame consists of the <c>ulong</c> loopback value provided by <paramref name="source"/>
        /// that is prepended to a <see cref="ConfigureLoadTester.TransmittedWords"/>-element <c>ushort</c>
        /// array of dummy data. When the frame is received by hardware, the loopback value is subtracted from
        /// the current hub clock count on the load testing device and stored. Therefore, if the loopback
        /// value is that of a previous <see cref="DataFrame.HubClock"/> from the <see
        /// cref="LoadTesterData"/> with the same <see cref="DeviceName"/> as this operator, this difference will provide a
        /// hardware-timed measurement of real-time latency. The variably-sized dummy data in the loopback
        /// frame is used for testing the effect of increasing the frame size, and thus the write
        /// communication bandwidth, on real-time latency.
        /// </remarks>
        /// <param name="source">A sequence of loopback values to send to the device</param>
        /// <returns> A sequence of loopback values to send to the device.</returns>
        public unsafe override IObservable<ulong> Process(IObservable<ulong> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (LoadTesterDeviceInfo)deviceInfo;
                var device = info.GetDeviceContext(typeof(LoadTester));
                var transmittedWords = info.TransmittedWords;

                if (transmittedWords > 0)
                {
                    var payload = new uint[transmittedWords + 2];

                    return source.Do(loopbackValue =>
                    {
                        payload[0] = (uint)loopbackValue;
                        payload[1] = (uint)(loopbackValue >> 32);
                        device.Write(payload);
                    });
                }
                else
                {
                    return source.Do(device.Write);
                }
            });
        }
    }
}
