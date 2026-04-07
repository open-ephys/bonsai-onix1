using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures the camera on a UCLA Miniscope V4.
    /// </summary>
    public class ConfigureUclaMiniscopeV4Camera : SingleDeviceFactory
    {
        readonly BehaviorSubject<double> ledBrightness = new(0);
        readonly BehaviorSubject<UclaMiniscopeV4SensorGain> sensorGain = new(UclaMiniscopeV4SensorGain.Low);
        readonly BehaviorSubject<double> focus = new(0);
        UclaMiniscopeV4FramesPerSecond frameRate = UclaMiniscopeV4FramesPerSecond.Fps30;

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureUclaMiniscopeV4Camera"/> class.
        /// </summary>
        public ConfigureUclaMiniscopeV4Camera()
            : base(typeof(UclaMiniscopeV4))
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the camera will produce image data.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="UclaMiniscopeV4CameraData"/> will produce image data. If set to false, <see
        /// cref="UclaMiniscopeV4CameraData"/> will not produce image data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the camera is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the camera video rate in frames per second.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Camera video rate in frames per second.")]
        public UclaMiniscopeV4FramesPerSecond FrameRate
        {
            get => frameRate;
            set
            {
                // NB: Required for backwards compatibility. The frameRate variable and get/set bodies can be
                // removed in v1.0.0 when the *Hz enums are removed.
                frameRate = value switch
                {
                    UclaMiniscopeV4FramesPerSecond.Fps10 or UclaMiniscopeV4FramesPerSecond.Fps10Hz => UclaMiniscopeV4FramesPerSecond.Fps10,
                    UclaMiniscopeV4FramesPerSecond.Fps15 or UclaMiniscopeV4FramesPerSecond.Fps15Hz => UclaMiniscopeV4FramesPerSecond.Fps15,
                    UclaMiniscopeV4FramesPerSecond.Fps20 or UclaMiniscopeV4FramesPerSecond.Fps20Hz => UclaMiniscopeV4FramesPerSecond.Fps20,
                    UclaMiniscopeV4FramesPerSecond.Fps25 or UclaMiniscopeV4FramesPerSecond.Fps25Hz => UclaMiniscopeV4FramesPerSecond.Fps25,
                    UclaMiniscopeV4FramesPerSecond.Fps30 or UclaMiniscopeV4FramesPerSecond.Fps30Hz => UclaMiniscopeV4FramesPerSecond.Fps30,
                    _ => UclaMiniscopeV4FramesPerSecond.Fps30
                };
            }
        }

        /// <summary>
        /// Gets or sets the camera sensor's analog gain.
        /// </summary>
        [Description("Camera sensor analog gain.")]
        [Category(AcquisitionCategory)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public UclaMiniscopeV4SensorGain SensorGain
        {
            get => sensorGain.Value;
            set => sensorGain.OnNext(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the excitation LED should turn on only when the camera
        /// shutter is open.
        /// </summary>
        /// <remarks>
        /// If set to true, the excitation LED will turn on briefly before, and turn off briefly after, the
        /// camera begins photon collection on its photodiode array. If set to false, the excitation LED will
        /// remain on at all times.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Only turn on excitation LED during camera exposures.")]
        public bool InterleaveLed { get; set; } = false;

        /// <summary>
        /// Gets or sets the excitation LED brightness level (0-100%).
        /// </summary>
        [Description("Excitation LED brightness level (0-100%).")]
        [Category(AcquisitionCategory)]
        [Range(0, 100)]
        [Precision(1, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double LedBrightness
        {
            get => ledBrightness.Value;
            set => ledBrightness.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the focal plane as a percentage up or down around its nominal depth.
        /// </summary>
        /// <remarks>
        /// The imaging focal plane is controlled by using a Max14574 high-voltage liquid lens driver. This
        /// chip produces pulse-width modulated, 5 kHz alternative electric field that deforms a liquid lens
        /// in order to change the Miniscope's focal plane. The strength of this field determines the degree
        /// of deformation and therefore the focal depth. The default setting of 0% corresponds to
        /// approximately mid-range with an excitation voltage of ~47 VRMS. -100% and 100% correspond to the
        /// minimum and maximum excitation voltage of ~24.4 and ~69.7 VRMS, respectively. 
        /// </remarks>
        [Description("Electro-wetting lens focal plane adjustment (percent of range around nominal depth).")]
        [Category(AcquisitionCategory)]
        [Range(-100, 100)]
        [Precision(1, 0.1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double Focus
        {
            get => focus.Value;
            set => focus.OnNext(value);
        }

        /// <summary>
        /// Configures the camera on a UCLA Miniscope V4.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds all
        /// configuration actions.</param>
        /// <returns>
        /// The original sequence but with each <see cref="ContextTask"/> instance now containing
        /// configuration actions required to use the Miniscope's camera.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var frameRate = FrameRate;
            var interleaveLed = InterleaveLed;

            return source.ConfigureAndLatchDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));

                // NB: required to get serdes link for miniscope, so this cannot be inside enable clause
                ConfigureSensor(device);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var shutdown = Disposable.Create(() =>
                {
                    // turn off EWL
                    var max14574 = new I2CRegisterContext(device, UclaMiniscopeV4.Max14574Address);
                    max14574.WriteByte(0x03, 0x00);

                    // turn off LED
                    var atMega = new I2CRegisterContext(device, UclaMiniscopeV4.AtMegaAddress);
                    atMega.WriteByte(0x01, 0xFF);
                });

                var disposables = new List<IDisposable>();

                // NB: the DS90UB9x supports multiple streams and we don't want to overwrite other streams'
                // enable state
                if (enable)
                {
                    device.WriteRegister(DS90UB9x.ENABLE, 1u);
                    ConfigureImager(device, frameRate, interleaveLed);

                    disposables.Add(ledBrightness.Subscribe(value => SetLedBrightness(device, value)));
                    disposables.Add(sensorGain.Subscribe(value => SetSensorGain(device, value)));
                    disposables.Add(focus.Subscribe(value => SetLiquidLensVoltage(device, value)));
                }

                disposables.Add(DeviceManager.RegisterDevice(deviceName, deviceInfo));
                disposables.Add(shutdown);

                return new CompositeDisposable(disposables);
            });
        }

        static void ConfigureSensor(DeviceContext device)
        {
            // NB: atMega (bit-banged I2C to SPI) requires that we talk slowly
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.AtMegaBitBangI2cClockRate);

            // set up Python480
            var atMega = new I2CRegisterContext(device, UclaMiniscopeV4.AtMegaAddress);
            WriteCameraRegister(atMega, 16, 3); // Turn on PLL
            WriteCameraRegister(atMega, 32, 0x7007); // Turn on clock management
            WriteCameraRegister(atMega, 199, 666); // Defines granularity (unit = 1/PLL clock) of exposure and reset_length
            WriteCameraRegister(atMega, 200, 3300); // Set frame rate to 30 Hz
            WriteCameraRegister(atMega, 201, 3000); // Set Exposure

            // NB: interaction with the atMega (bit-banged I2C to SPI) requires that we talk slowly, reset to
            // talk to normal chips
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.NominalI2cClockRate);
        }

        static void ConfigureImager(DeviceContext device, UclaMiniscopeV4FramesPerSecond frameRate, bool interleaveLed)
        {
            // set up potentiometer
            var tpl0102 = new I2CRegisterContext(device, UclaMiniscopeV4.Tpl0102Address);
            tpl0102.WriteByte(0x00, 0x72);
            tpl0102.WriteByte(0x01, 0x00);

            // turn on EWL
            var max14574 = new I2CRegisterContext(device, UclaMiniscopeV4.Max14574Address);
            max14574.WriteByte(0x03, 0x03);

            // configuration properties
            uint shutterWidth = frameRate switch
            {
                UclaMiniscopeV4FramesPerSecond.Fps10 => 10000,
                UclaMiniscopeV4FramesPerSecond.Fps15 => 6667,
                UclaMiniscopeV4FramesPerSecond.Fps20 => 5000,
                UclaMiniscopeV4FramesPerSecond.Fps25 => 4000,
                UclaMiniscopeV4FramesPerSecond.Fps30 => 3300,
                _ => 3300
            };

            // NB: atMega (bit-banged I2C to SPI) requires that we talk slowly
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.AtMegaBitBangI2cClockRate);

            var atMega = new I2CRegisterContext(device, UclaMiniscopeV4.AtMegaAddress);
            atMega.WriteByte(0x04, (uint)(interleaveLed ? 0x00 : 0x03));
            WriteCameraRegister(atMega, 200, shutterWidth);

            // NB: interaction with the atMega (bit-banged I2C to SPI) requires that we talk slowly, reset to
            // talk to normal chips
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.NominalI2cClockRate);
        }

        static void WriteCameraRegister(I2CRegisterContext i2c, uint register, uint value)
        {
            // ATMega -> Python480 passthrough protocol
            var regLow = register & 0xFF;
            var regHigh = (register >> 8) & 0xFF;
            var valLow = value & 0xFF;
            var valHigh = (value >> 8) & 0xFF;

            i2c.WriteByte(0x05, regHigh);
            i2c.WriteByte(regLow, valHigh);
            i2c.WriteByte(valLow, 0x00);
        }

        static void SetLedBrightness(DeviceContext device, double percent)
        {
            // NB: atMega (bit-banded i2c to SPI) requires that we talk slowly
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.AtMegaBitBangI2cClockRate);
            var atMega = new I2CRegisterContext(device, UclaMiniscopeV4.AtMegaAddress);
            atMega.WriteByte(0x01, (uint)((percent == 0) ? 0xFF : 0x08));

            var tpl0102 = new I2CRegisterContext(device, UclaMiniscopeV4.Tpl0102Address);
            tpl0102.WriteByte(0x01, (uint)(255 * ((100 - percent) / 100.0)));
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.NominalI2cClockRate);
        }

        static void SetSensorGain(DeviceContext device, UclaMiniscopeV4SensorGain gain)
        {
            // NB: atMega (bit-banded i2c to SPI) requires that we talk slowly
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.AtMegaBitBangI2cClockRate);
            var atMega = new I2CRegisterContext(device, UclaMiniscopeV4.AtMegaAddress);
            WriteCameraRegister(atMega, 204, (uint)gain);
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.NominalI2cClockRate);
        }

        internal static void SetLiquidLensVoltage(DeviceContext device, double focus)
        {
            var max14574 = new I2CRegisterContext(device, UclaMiniscopeV4.Max14574Address);
            var scaled = focus * 1.27;
            max14574.WriteByte(0x08, (byte)(127 + scaled));
            max14574.WriteByte(0x09, 0x02);
        }
    }

    static class UclaMiniscopeV4
    {
        public const int AtMegaAddress = 0x10;
        public const int Tpl0102Address = 0x50;
        public const int Max14574Address = 0x77;

        public const int SensorRows = 608;
        public const int SensorColumns = 608;

        internal const double NominalI2cClockRate = 200e3; // Maximum reliable serdes I2C clock rate in Hz
        internal const double AtMegaBitBangI2cClockRate = 80e3; // Maximum relable serdes I2C clock rate when using the Atmega bit-bang passthrough in Hz

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(UclaMiniscopeV4))
            {
            }
        }
    }

    /// <summary>
    /// Specifies analog gain of the Python-480 image sensor on a UCLA Miniscope V4.
    /// </summary>
    public enum UclaMiniscopeV4SensorGain
    {
        /// <summary>
        /// Specifies low gain.
        /// </summary>
        Low = 0x00E1,

        /// <summary>
        /// Specifies medium gain.
        /// </summary>
        Medium = 0x00E4,

        /// <summary>
        /// Specifies high gain.
        /// </summary>
        High = 0x0024,
    }

    /// <summary>
    /// Specifies the video frame rate of the Python-480 image sensor on a UCLA Miniscope V4.
    /// </summary>
    public enum UclaMiniscopeV4FramesPerSecond
    {
        /// <summary>
        /// Specifies 10 frames per second.
        /// </summary>
        Fps10,

        /// <summary>
        /// Specifies 15 frames per second.
        /// </summary>
        Fps15,

        /// <summary>
        /// Specifies 20 frames per second.
        /// </summary>
        Fps20,

        /// <summary>
        /// Specifies 25 frames per second.
        /// </summary>
        Fps25,

        /// <summary>
        /// Specifies 30 frames per second.
        /// </summary>
        Fps30,

        /// <summary>
        /// This value is deprecated. Please use the corresponding version without the Hz suffix. This will be removed in v1.0.0.
        /// </summary>
        [Browsable(false)]
        Fps10Hz,

        /// <summary>
        /// This value is deprecated. Please use the corresponding version without the Hz suffix. This will be removed in v1.0.0.
        /// </summary>
        [Browsable(false)]
        Fps15Hz,

        /// <summary>
        /// This value is deprecated. Please use the corresponding version without the Hz suffix. This will be removed in v1.0.0.
        /// </summary>
        [Browsable(false)]
        Fps20Hz,

        /// <summary>
        /// This value is deprecated. Please use the corresponding version without the Hz suffix. This will be removed in v1.0.0.
        /// </summary>
        [Browsable(false)]
        Fps25Hz,

        /// <summary>
        /// This value is deprecated. Please use the corresponding version without the Hz suffix. This will be removed in v1.0.0.
        /// </summary>
        [Browsable(false)]
        Fps30Hz,
    }
}
