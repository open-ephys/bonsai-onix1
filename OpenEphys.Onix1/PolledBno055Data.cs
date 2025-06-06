using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> from Bosch Bno055
    /// 9-axis inertial measurement unit (IMU) by polling it from the host computer.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigurePolledBno055"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Polls a Bno055 9-axis IMU to produce a sequence Bno055 data frames.")]
    public class PolledBno055Data : Source<Bno055DataFrame>
    {
        static readonly HashSet<string> registeredValues = new();
        static readonly object registrationLock = new();

        /// <summary>
        /// Gets or sets a unique device name. Only a single <see cref="PolledBno055Data"/> operator can be
        /// used per distinct device name. 
        /// </summary>
        /// <remarks>
        /// The device name provides a unique, human-readable identifier that is used to link software
        /// elements for configuration, control, and data streaming to hardware. For instance, it can be used
        /// to link configuration operators to data IO operators within a workflow. This value is
        /// usually not set manually, but is assigned in a <see cref="MultiDeviceFactory"/> to correspond to a
        /// fixed address with a piece of hardware such as a headstage. This address is used for software
        /// communication.
        /// </remarks>
        [TypeConverter(typeof(PolledBno055.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets which data registers should be collected from the Bno055.
        /// </summary>
        /// <remarks>
        /// The rate that data is sampled is limited by communication with the Bno055 by the host PC (rather
        /// than a dedicated controller on the headstage). If the user wishes to maximize the sampling rate of
        /// particular measurements, they can select which should be sampled using this property. For
        /// instance, specifying "Quaternion, Calibration" means that only the quaternion and sensor
        /// calibration status registers will be polled and the rest of the members of <see
        /// cref="Bno055DataFrame"/>, with the exception of <c>Clock</c> and <c>HubClock</c>, will be set to
        /// 0.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Specifies which data registers should be read from the chip.")]
        [TypeConverter(typeof(FlagsConverter))]
        public PolledBno055Registers PolledRegisters { get; set; } = PolledBno055Registers.All;

        /// <summary>
        /// Use a ~100 Hz internal timer to generate a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see>.
        /// </summary>
        /// <remarks>
        /// An internal timer will be used to poll the Bno055 in order to generate a sequence of <see
        /// cref="Bno055DataFrame">Bno055DataFrames</see> at approximately 100 Hz. This rate may be limited by
        /// hardware communication speeds (see <see cref="PolledRegisters"/>).
        /// </remarks>
        /// <returns>A sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see>.</returns>
        public override IObservable<Bno055DataFrame> Generate()
        {
            // NB: NewThreadScheduler runs polling on dedicated thread and uses StopWatch for high resolution
            // periodic measurements. This results in much better performance than the default scheduler. 
            var source = Observable.Interval(TimeSpan.FromMilliseconds(10), new NewThreadScheduler());
            return Generate(source);
        }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> that is driven by an
        /// input sequence.
        /// </summary>
        /// <remarks>
        /// This will attempt to produce a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> that
        /// is updated whenever an item in the <paramref name="source"/> sequence is received. This rate may
        /// be limited by the hardware communication speeds (see <see cref="PolledRegisters"/>) and has a
        /// maximum meaningful rate of 100 Hz, which is the update rate of the sensor fusion algorithm on the
        /// Bno055 itself.
        /// </remarks>
        /// <param name="source">A sequence to drive sampling.</param>
        /// <returns>A sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see>.</returns>
        public unsafe IObservable<Bno055DataFrame> Generate<TSource>(IObservable<TSource> source)
        {
            var deviceName = DeviceName;
            var polled = PolledRegisters;

            return DeviceManager.GetDevice(deviceName).SelectMany(
                deviceInfo =>
                {
                    return !((PolledBno055DeviceInfo)deviceInfo).Enable
                        ? Observable.Empty<Bno055DataFrame>()
                        : Observable.Create<Bno055DataFrame>(observer =>
                        {
                            var device = deviceInfo.GetDeviceContext(typeof(PolledBno055));
                            var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                            var i2c = new I2CRegisterContext(passthrough, PolledBno055.BNO055Address);

                            // NB: Only allow one PolledBno055Data operator per unique DeviceName
                            lock (registrationLock)
                            {
                                if (registeredValues.Contains(deviceName))
                                {
                                    throw new InvalidOperationException($"There is already a {nameof(PolledBno055Data)} linked to '{deviceName}'. " +
                                        $"Only one {nameof(PolledBno055Data)} operator is allowed for each distinct {nameof(DeviceName)}.");
                                }

                                registeredValues.Add(deviceName);
                            }

                            // NB: Preallocate the array to avoid unnecesary allocation each frame
                            byte[] data = new byte[28];
                            var s = source.SubscribeSafe(observer, _ =>
                            {
                                Bno055DataFrame frame = default;
                                device.Context.EnsureContext(() =>
                                {
                                      if (polled.HasFlag(PolledBno055Registers.EulerAngle))
                                      {
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 0, 4, data, 0);
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 4, 2, data, 4);
                                      }
                                      else
                                      {
                                          Array.Clear(data, 0, 6);
                                      }

                                      if (polled.HasFlag(PolledBno055Registers.Quaternion))
                                      {
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 6, 4, data, 6);
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 10, 4, data, 10);
                                      }
                                      else
                                      {
                                          Array.Clear(data, 6, 8);
                                      }

                                      if (polled.HasFlag(PolledBno055Registers.Acceleration))
                                      {
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 14, 4, data, 14);
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 18, 2, data, 18);
                                      }
                                      else
                                      {
                                          Array.Clear(data, 0, 6);
                                      }

                                      if (polled.HasFlag(PolledBno055Registers.Gravity))
                                      {
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 20, 4, data, 20);
                                          i2c.ReadWord(PolledBno055.EulerHeadingLsbAddress + 24, 2, data, 24);
                                      }
                                      else
                                      {
                                          Array.Clear(data, 0, 6);
                                      }

                                      //TODO: actual performance would be a little better if we merged these two so they could be read together
                                      data[26] = polled.HasFlag(PolledBno055Registers.Temperature) ? i2c.ReadByte(PolledBno055.EulerHeadingLsbAddress + 26) : (byte)0;
                                      data[27] = polled.HasFlag(PolledBno055Registers.Calibration) ? i2c.ReadByte(PolledBno055.EulerHeadingLsbAddress + 27) : (byte)0;

                                    ulong clock = passthrough.ReadRegister(DS90UB9x.LASTI2CL);
                                    clock += (ulong)passthrough.ReadRegister(DS90UB9x.LASTI2CH) << 32;
                                    fixed (byte* dataPtr = data)
                                    {
                                        frame = new Bno055DataFrame(clock, (Bno055DataPayload*)dataPtr);
                                    }
                                });

                                if (frame != null)
                                {
                                    observer.OnNext(frame);
                                }
                            });
                            
                            return Disposable.Create(() =>
                            {
                                lock (registrationLock)
                                {
                                    registeredValues.Remove(deviceName);
                                }
                                s.Dispose();
                            });
                        });
                });
        }

        class FlagsConverter : EnumConverter
        {
            public FlagsConverter(Type type)
                : base(type)
            {
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] {
                    PolledBno055Registers.Quaternion,
                    PolledBno055Registers.EulerAngle,
                    PolledBno055Registers.Acceleration,
                    PolledBno055Registers.Gravity,
                    PolledBno055Registers.Temperature,
                    PolledBno055Registers.Calibration,
                    PolledBno055Registers.All
                });
            }
        }
    }

    /// <summary>
    /// Specifies which data registers will be read from the Bno055 during each polling cycle.
    /// </summary>
    [Flags]
    public enum PolledBno055Registers
    {
        /// <summary>
        /// Specifies that the Euler angles will be polled.
        /// </summary>
        EulerAngle = 0x1,

        /// <summary>
        /// Specifies that the quaternion will be polled.
        /// </summary>
        Quaternion = 0x2,

        /// <summary>
        /// Specifies that the linear acceleration will be polled.
        /// </summary>
        Acceleration = 0x4,

        /// <summary>
        /// Specifies that the gravity vector will be polled.
        /// </summary>
        Gravity = 0x8,

        /// <summary>
        /// Specifies that the temperature measurement will be polled.
        /// </summary>
        Temperature = 0x10,

        /// <summary>
        /// Specifies that the sensor calibration status will be polled.
        /// </summary>
        Calibration = 0x20,

        /// <summary>
        /// Specifies that all sensor measurements and calibration status will be polled.
        /// </summary>
        All = EulerAngle | Quaternion | Acceleration | Gravity | Temperature | Calibration,
    }

    /// <inheritdoc cref = "PolledBno055Data"/>v
    [Obsolete("This operator is obsolete. Use PolledBno055Data instead. Will be removed in version 1.0.0.")]
    public class NeuropixelsV1eBno055Data : PolledBno055Data { }

    /// <inheritdoc cref = "PolledBno055Data"/>v
    [Obsolete("This operator is obsolete. Use PolledBno055Data instead. Will be removed in version 1.0.0.")]
    public class NeuropixelsV2eBno055Data : PolledBno055Data { }

    /// <inheritdoc cref = "PolledBno055Data"/>v
    [Obsolete("This operator is obsolete. Use PolledBno055Data instead. Will be removed in version 1.0.0.")]
    public class NeuropixelsV2eBetaBno055Data : PolledBno055Data { }
}
