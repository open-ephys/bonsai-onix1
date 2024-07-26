using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that produces a sequence of 3D positions from an array of Triad Semiconductor TS4231 receivers beneath
    /// a pair of SteamVR V1 base stations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureTS4231V1"/>,
    /// in order to stream data.
    /// </para>
    /// <para>
    /// The data produced by this class contains naïve geometric estimates of positions of photodiodes attached to each TS4231 chip.
    /// This operator makes the following assumptions about the setup:
    /// <list type="bullet">
    /// <item><description>Two SteamVR V1 base stations are used.</description></item>
    /// <item><description>The base stations have been synchronized with a patch cable and their modes set to ‘A’ and ‘b’, respectively.</description></item>
    /// <item><description>The base stations are pointed in the same direction.</description></item>
    /// <item><description>The Z-axis extends away the emitting face of lighthouses, X along the direction of the text on the back label,
    /// and Y from bottom to top text on the back label.</description></item>
    /// </list>
    /// This operator collects a sequence of <see cref="oni.Frame"/> objects from each TS3231 receiver that are used to determine the ray from each
    /// base station to the TS3231's photodiode. A simple geometric inversion is performed to determine the photodiodes 3D position from the values
    /// <see cref="P"/> and <see cref="Q"/>. It does not use a predictive model or integrate data from an IMU and is therefore quite sensitive to
    /// obstructions in and will require post-hoc processing to correct systematic errors due to optical aberrations and nonlinearities. The the
    /// <see cref="TS4231V1Data"/> operator provides access to individual lighthouse signals that is useful for a creating more robust position
    /// estimates using downstream processing.
    /// </para>
    /// </remarks>
    public class TS4231V1GeometricPositionData : Source<TS4231V1GeometricPositionDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(TS4231V1.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the position of the first base station in arbitrary units.
        /// </summary>
        /// <remarks>
        /// The units used will determine the units of <see cref="TS4231V1GeometricPositionDataFrame.Position"/> and must match those used in <see cref="Q"/>.
        /// Typically this value is used to define the origin and remains at (0, 0, 0).
        /// </remarks>
        public Point3d P { get; set; } = new(0, 0, 0);

        /// <summary>
        /// Gets or sets the position of the first base station in arbitrary units.
        /// </summary>
        /// <remarks>
        /// The units used will determine the units of <see cref="TS4231V1GeometricPositionDataFrame.Position"/> and must match those used in <see cref="P"/>.
        /// </remarks>
        public Point3d Q { get; set; } = new(1, 0, 0);

        /// <summary>
        /// Generates a sequence of <see cref="TS4231V1GeometricPositionDataFrame"/> objects, each of which contains the 3D position of single photodiode.
        /// </summary>
        /// <returns>A sequence of <see cref="TS4231V1GeometricPositionDataFrame"/> objects.</returns>
        public unsafe override IObservable<TS4231V1GeometricPositionDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<TS4231V1GeometricPositionDataFrame>(observer =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(TS4231V1));
                    var pulseConverter = new TS4231V1GeometricPositionConverter(device.Hub.ClockHz, P, Q);

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var position = pulseConverter.Convert(frame);
                            if (position != null)
                            {
                                observer.OnNext(position);
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);

                    return deviceInfo.Context
                        .GetDeviceFrames(device.Address)
                        .SubscribeSafe(frameObserver);
                }));
        }
    }
}
