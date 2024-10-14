using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV1 device attached to an ONIX NeuropixelsV1f headstage.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="NeuropixelsV1fData"/>,
    /// using a shared <c>DeviceName</c>.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV1 device attached to an ONIX NeuropixelsV1f headstage.")]
    public class ConfigureNeuropixelsV1f : SingleDeviceFactory, IConfigureNeuropixelsV1
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1f"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1f()
            : base(typeof(NeuropixelsV1f))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1f"/> class with the specified <see cref="NeuropixelsV1Probe"/> name.
        /// </summary>
        public ConfigureNeuropixelsV1f(NeuropixelsV1Probe probeName)
            : base(typeof(NeuropixelsV1f))
        {
            ProbeName = probeName;
            ProbeConfiguration = new();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ConfigureNeuropixelsV1f"/> class with public
        ///  properties copied from the specified configuration.
        /// </summary>
        /// <param name="configureNeuropixelsV1f">Existing <see cref="ConfigureNeuropixelsV1f"/> instance.</param>
        public ConfigureNeuropixelsV1f(ConfigureNeuropixelsV1f configureNeuropixelsV1f)
            : base(typeof(NeuropixelsV1f))
        {
            ProbeName = configureNeuropixelsV1f.ProbeName;
            Enable = configureNeuropixelsV1f.Enable;
            GainCalibrationFile = configureNeuropixelsV1f.GainCalibrationFile;
            AdcCalibrationFile = configureNeuropixelsV1f.AdcCalibrationFile;
            ProbeConfiguration = new(configureNeuropixelsV1f.ProbeConfiguration);
            DeviceName = configureNeuropixelsV1f.DeviceName;
            DeviceAddress = configureNeuropixelsV1f.DeviceAddress;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV1fData"/> will produce data. If set to false, 
        /// <see cref="NeuropixelsV1fData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Neuro data stream is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the NeuropixelsV1 probe configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Neuropixels 1.0e probe configuration")]
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; } = new();

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across probes. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the 
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration 
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the Neuropixels 1.0 gain calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Category(ConfigurationCategory)]
        public string GainCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the ADC calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe must be provided with an ADC calibration file that contains probe-specific hardware settings that is
        /// created by IMEC during factory calibration. These files are used to set internal bias currents, correct for ADC
        /// nonlinearities, correct ADC-zero crossing non-monotonicities, etc. Using the correct calibration file is mandatory
        /// for the probe to operate correctly. 
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the 
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration 
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv")]
        [Description("Path to the Neuropixels 1.0 ADC calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Category(ConfigurationCategory)]
        public string AdcCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV1Probe"/> for this probe.
        /// </summary>
        [Browsable(false)]
        public NeuropixelsV1Probe ProbeName { get; set; } = NeuropixelsV1Probe.ProbeA;

        /// <summary>
        /// Configures a NeuropixelsV1 device on an ONIX NeuropixelsV1f headstage.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV1 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, typeof(NeuropixelsV1f));
                device.WriteRegister(NeuropixelsV1f.ENABLE, enable ? 1u : 0);

                if (enable)
                {
                    var probeControl = new NeuropixelsV1fRegisterContext(device, ProbeConfiguration, GainCalibrationFile, AdcCalibrationFile);
                    probeControl.InitializeProbe();
                    probeControl.WriteShiftRegisters();
                }

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class NeuropixelsV1f
    {
        public const int ID = 11;

        public const int I2cAddress = 0x70;
        public const int EepromI2cAddress = 0x55;

        public const int WordsPerFrame = 36;

        // managed registers

        // # autogeneration script
        // start = 0x8011
        // for i, c in enumerate(range(0, 384, 2)):
        //     print('public const uint CHAN{odd:03d}_{even:03d}_LFPGAIN = 0x{addr:02X};'.format(odd = c + 1, even = c, addr= start + i))
        // start = 0x80D1
        // for i, c in enumerate(range(0, 384, 2)):
        //     print('public const uint CHAN{odd:03d}_{even:03d}_APGAIN = 0x{addr:02X};'.format(odd = c + 1, even = c, addr= start + i))

        public const uint ENABLE = 0x8000; // Enable or disable the data output stream

        public const uint ADC01_00_OFF_THRESH = 0x8001; // ADC 1 and 0 offset and threshold parameters: [6-bit ADC 01 Offset, 10-bit ADC 01 Threshold, 6-bit ADC 00 Offset, 10-bit ADC 00 Threshold]
        public const uint ADC03_02_OFF_THRESH = 0x8002;
        public const uint ADC05_04_OFF_THRESH = 0x8003;
        public const uint ADC07_06_OFF_THRESH = 0x8004;
        public const uint ADC09_08_OFF_THRESH = 0x8005;
        public const uint ADC11_10_OFF_THRESH = 0x8006;
        public const uint ADC13_12_OFF_THRESH = 0x8007;
        public const uint ADC15_14_OFF_THRESH = 0x8008;
        public const uint ADC17_16_OFF_THRESH = 0x8009;
        public const uint ADC19_18_OFF_THRESH = 0x800a;
        public const uint ADC21_20_OFF_THRESH = 0x800b;
        public const uint ADC23_22_OFF_THRESH = 0x800c;
        public const uint ADC25_24_OFF_THRESH = 0x800d;
        public const uint ADC27_26_OFF_THRESH = 0x800e;
        public const uint ADC29_28_OFF_THRESH = 0x800f;
        public const uint ADC31_30_OFF_THRESH = 0x8010;

        public const uint CHAN001_000_LFPGAIN = 0x8011;
        public const uint CHAN003_002_LFPGAIN = 0x8012;
        public const uint CHAN005_004_LFPGAIN = 0x8013;
        public const uint CHAN007_006_LFPGAIN = 0x8014;
        public const uint CHAN009_008_LFPGAIN = 0x8015;
        public const uint CHAN011_010_LFPGAIN = 0x8016;
        public const uint CHAN013_012_LFPGAIN = 0x8017;
        public const uint CHAN015_014_LFPGAIN = 0x8018;
        public const uint CHAN017_016_LFPGAIN = 0x8019;
        public const uint CHAN019_018_LFPGAIN = 0x801A;
        public const uint CHAN021_020_LFPGAIN = 0x801B;
        public const uint CHAN023_022_LFPGAIN = 0x801C;
        public const uint CHAN025_024_LFPGAIN = 0x801D;
        public const uint CHAN027_026_LFPGAIN = 0x801E;
        public const uint CHAN029_028_LFPGAIN = 0x801F;
        public const uint CHAN031_030_LFPGAIN = 0x8020;
        public const uint CHAN033_032_LFPGAIN = 0x8021;
        public const uint CHAN035_034_LFPGAIN = 0x8022;
        public const uint CHAN037_036_LFPGAIN = 0x8023;
        public const uint CHAN039_038_LFPGAIN = 0x8024;
        public const uint CHAN041_040_LFPGAIN = 0x8025;
        public const uint CHAN043_042_LFPGAIN = 0x8026;
        public const uint CHAN045_044_LFPGAIN = 0x8027;
        public const uint CHAN047_046_LFPGAIN = 0x8028;
        public const uint CHAN049_048_LFPGAIN = 0x8029;
        public const uint CHAN051_050_LFPGAIN = 0x802A;
        public const uint CHAN053_052_LFPGAIN = 0x802B;
        public const uint CHAN055_054_LFPGAIN = 0x802C;
        public const uint CHAN057_056_LFPGAIN = 0x802D;
        public const uint CHAN059_058_LFPGAIN = 0x802E;
        public const uint CHAN061_060_LFPGAIN = 0x802F;
        public const uint CHAN063_062_LFPGAIN = 0x8030;
        public const uint CHAN065_064_LFPGAIN = 0x8031;
        public const uint CHAN067_066_LFPGAIN = 0x8032;
        public const uint CHAN069_068_LFPGAIN = 0x8033;
        public const uint CHAN071_070_LFPGAIN = 0x8034;
        public const uint CHAN073_072_LFPGAIN = 0x8035;
        public const uint CHAN075_074_LFPGAIN = 0x8036;
        public const uint CHAN077_076_LFPGAIN = 0x8037;
        public const uint CHAN079_078_LFPGAIN = 0x8038;
        public const uint CHAN081_080_LFPGAIN = 0x8039;
        public const uint CHAN083_082_LFPGAIN = 0x803A;
        public const uint CHAN085_084_LFPGAIN = 0x803B;
        public const uint CHAN087_086_LFPGAIN = 0x803C;
        public const uint CHAN089_088_LFPGAIN = 0x803D;
        public const uint CHAN091_090_LFPGAIN = 0x803E;
        public const uint CHAN093_092_LFPGAIN = 0x803F;
        public const uint CHAN095_094_LFPGAIN = 0x8040;
        public const uint CHAN097_096_LFPGAIN = 0x8041;
        public const uint CHAN099_098_LFPGAIN = 0x8042;
        public const uint CHAN101_100_LFPGAIN = 0x8043;
        public const uint CHAN103_102_LFPGAIN = 0x8044;
        public const uint CHAN105_104_LFPGAIN = 0x8045;
        public const uint CHAN107_106_LFPGAIN = 0x8046;
        public const uint CHAN109_108_LFPGAIN = 0x8047;
        public const uint CHAN111_110_LFPGAIN = 0x8048;
        public const uint CHAN113_112_LFPGAIN = 0x8049;
        public const uint CHAN115_114_LFPGAIN = 0x804A;
        public const uint CHAN117_116_LFPGAIN = 0x804B;
        public const uint CHAN119_118_LFPGAIN = 0x804C;
        public const uint CHAN121_120_LFPGAIN = 0x804D;
        public const uint CHAN123_122_LFPGAIN = 0x804E;
        public const uint CHAN125_124_LFPGAIN = 0x804F;
        public const uint CHAN127_126_LFPGAIN = 0x8050;
        public const uint CHAN129_128_LFPGAIN = 0x8051;
        public const uint CHAN131_130_LFPGAIN = 0x8052;
        public const uint CHAN133_132_LFPGAIN = 0x8053;
        public const uint CHAN135_134_LFPGAIN = 0x8054;
        public const uint CHAN137_136_LFPGAIN = 0x8055;
        public const uint CHAN139_138_LFPGAIN = 0x8056;
        public const uint CHAN141_140_LFPGAIN = 0x8057;
        public const uint CHAN143_142_LFPGAIN = 0x8058;
        public const uint CHAN145_144_LFPGAIN = 0x8059;
        public const uint CHAN147_146_LFPGAIN = 0x805A;
        public const uint CHAN149_148_LFPGAIN = 0x805B;
        public const uint CHAN151_150_LFPGAIN = 0x805C;
        public const uint CHAN153_152_LFPGAIN = 0x805D;
        public const uint CHAN155_154_LFPGAIN = 0x805E;
        public const uint CHAN157_156_LFPGAIN = 0x805F;
        public const uint CHAN159_158_LFPGAIN = 0x8060;
        public const uint CHAN161_160_LFPGAIN = 0x8061;
        public const uint CHAN163_162_LFPGAIN = 0x8062;
        public const uint CHAN165_164_LFPGAIN = 0x8063;
        public const uint CHAN167_166_LFPGAIN = 0x8064;
        public const uint CHAN169_168_LFPGAIN = 0x8065;
        public const uint CHAN171_170_LFPGAIN = 0x8066;
        public const uint CHAN173_172_LFPGAIN = 0x8067;
        public const uint CHAN175_174_LFPGAIN = 0x8068;
        public const uint CHAN177_176_LFPGAIN = 0x8069;
        public const uint CHAN179_178_LFPGAIN = 0x806A;
        public const uint CHAN181_180_LFPGAIN = 0x806B;
        public const uint CHAN183_182_LFPGAIN = 0x806C;
        public const uint CHAN185_184_LFPGAIN = 0x806D;
        public const uint CHAN187_186_LFPGAIN = 0x806E;
        public const uint CHAN189_188_LFPGAIN = 0x806F;
        public const uint CHAN191_190_LFPGAIN = 0x8070;
        public const uint CHAN193_192_LFPGAIN = 0x8071;
        public const uint CHAN195_194_LFPGAIN = 0x8072;
        public const uint CHAN197_196_LFPGAIN = 0x8073;
        public const uint CHAN199_198_LFPGAIN = 0x8074;
        public const uint CHAN201_200_LFPGAIN = 0x8075;
        public const uint CHAN203_202_LFPGAIN = 0x8076;
        public const uint CHAN205_204_LFPGAIN = 0x8077;
        public const uint CHAN207_206_LFPGAIN = 0x8078;
        public const uint CHAN209_208_LFPGAIN = 0x8079;
        public const uint CHAN211_210_LFPGAIN = 0x807A;
        public const uint CHAN213_212_LFPGAIN = 0x807B;
        public const uint CHAN215_214_LFPGAIN = 0x807C;
        public const uint CHAN217_216_LFPGAIN = 0x807D;
        public const uint CHAN219_218_LFPGAIN = 0x807E;
        public const uint CHAN221_220_LFPGAIN = 0x807F;
        public const uint CHAN223_222_LFPGAIN = 0x8080;
        public const uint CHAN225_224_LFPGAIN = 0x8081;
        public const uint CHAN227_226_LFPGAIN = 0x8082;
        public const uint CHAN229_228_LFPGAIN = 0x8083;
        public const uint CHAN231_230_LFPGAIN = 0x8084;
        public const uint CHAN233_232_LFPGAIN = 0x8085;
        public const uint CHAN235_234_LFPGAIN = 0x8086;
        public const uint CHAN237_236_LFPGAIN = 0x8087;
        public const uint CHAN239_238_LFPGAIN = 0x8088;
        public const uint CHAN241_240_LFPGAIN = 0x8089;
        public const uint CHAN243_242_LFPGAIN = 0x808A;
        public const uint CHAN245_244_LFPGAIN = 0x808B;
        public const uint CHAN247_246_LFPGAIN = 0x808C;
        public const uint CHAN249_248_LFPGAIN = 0x808D;
        public const uint CHAN251_250_LFPGAIN = 0x808E;
        public const uint CHAN253_252_LFPGAIN = 0x808F;
        public const uint CHAN255_254_LFPGAIN = 0x8090;
        public const uint CHAN257_256_LFPGAIN = 0x8091;
        public const uint CHAN259_258_LFPGAIN = 0x8092;
        public const uint CHAN261_260_LFPGAIN = 0x8093;
        public const uint CHAN263_262_LFPGAIN = 0x8094;
        public const uint CHAN265_264_LFPGAIN = 0x8095;
        public const uint CHAN267_266_LFPGAIN = 0x8096;
        public const uint CHAN269_268_LFPGAIN = 0x8097;
        public const uint CHAN271_270_LFPGAIN = 0x8098;
        public const uint CHAN273_272_LFPGAIN = 0x8099;
        public const uint CHAN275_274_LFPGAIN = 0x809A;
        public const uint CHAN277_276_LFPGAIN = 0x809B;
        public const uint CHAN279_278_LFPGAIN = 0x809C;
        public const uint CHAN281_280_LFPGAIN = 0x809D;
        public const uint CHAN283_282_LFPGAIN = 0x809E;
        public const uint CHAN285_284_LFPGAIN = 0x809F;
        public const uint CHAN287_286_LFPGAIN = 0x80A0;
        public const uint CHAN289_288_LFPGAIN = 0x80A1;
        public const uint CHAN291_290_LFPGAIN = 0x80A2;
        public const uint CHAN293_292_LFPGAIN = 0x80A3;
        public const uint CHAN295_294_LFPGAIN = 0x80A4;
        public const uint CHAN297_296_LFPGAIN = 0x80A5;
        public const uint CHAN299_298_LFPGAIN = 0x80A6;
        public const uint CHAN301_300_LFPGAIN = 0x80A7;
        public const uint CHAN303_302_LFPGAIN = 0x80A8;
        public const uint CHAN305_304_LFPGAIN = 0x80A9;
        public const uint CHAN307_306_LFPGAIN = 0x80AA;
        public const uint CHAN309_308_LFPGAIN = 0x80AB;
        public const uint CHAN311_310_LFPGAIN = 0x80AC;
        public const uint CHAN313_312_LFPGAIN = 0x80AD;
        public const uint CHAN315_314_LFPGAIN = 0x80AE;
        public const uint CHAN317_316_LFPGAIN = 0x80AF;
        public const uint CHAN319_318_LFPGAIN = 0x80B0;
        public const uint CHAN321_320_LFPGAIN = 0x80B1;
        public const uint CHAN323_322_LFPGAIN = 0x80B2;
        public const uint CHAN325_324_LFPGAIN = 0x80B3;
        public const uint CHAN327_326_LFPGAIN = 0x80B4;
        public const uint CHAN329_328_LFPGAIN = 0x80B5;
        public const uint CHAN331_330_LFPGAIN = 0x80B6;
        public const uint CHAN333_332_LFPGAIN = 0x80B7;
        public const uint CHAN335_334_LFPGAIN = 0x80B8;
        public const uint CHAN337_336_LFPGAIN = 0x80B9;
        public const uint CHAN339_338_LFPGAIN = 0x80BA;
        public const uint CHAN341_340_LFPGAIN = 0x80BB;
        public const uint CHAN343_342_LFPGAIN = 0x80BC;
        public const uint CHAN345_344_LFPGAIN = 0x80BD;
        public const uint CHAN347_346_LFPGAIN = 0x80BE;
        public const uint CHAN349_348_LFPGAIN = 0x80BF;
        public const uint CHAN351_350_LFPGAIN = 0x80C0;
        public const uint CHAN353_352_LFPGAIN = 0x80C1;
        public const uint CHAN355_354_LFPGAIN = 0x80C2;
        public const uint CHAN357_356_LFPGAIN = 0x80C3;
        public const uint CHAN359_358_LFPGAIN = 0x80C4;
        public const uint CHAN361_360_LFPGAIN = 0x80C5;
        public const uint CHAN363_362_LFPGAIN = 0x80C6;
        public const uint CHAN365_364_LFPGAIN = 0x80C7;
        public const uint CHAN367_366_LFPGAIN = 0x80C8;
        public const uint CHAN369_368_LFPGAIN = 0x80C9;
        public const uint CHAN371_370_LFPGAIN = 0x80CA;
        public const uint CHAN373_372_LFPGAIN = 0x80CB;
        public const uint CHAN375_374_LFPGAIN = 0x80CC;
        public const uint CHAN377_376_LFPGAIN = 0x80CD;
        public const uint CHAN379_378_LFPGAIN = 0x80CE;
        public const uint CHAN381_380_LFPGAIN = 0x80CF;
        public const uint CHAN383_382_LFPGAIN = 0x80D0;

        public const uint CHAN001_000_APGAIN = 0x80D1;
        public const uint CHAN003_002_APGAIN = 0x80D2;
        public const uint CHAN005_004_APGAIN = 0x80D3;
        public const uint CHAN007_006_APGAIN = 0x80D4;
        public const uint CHAN009_008_APGAIN = 0x80D5;
        public const uint CHAN011_010_APGAIN = 0x80D6;
        public const uint CHAN013_012_APGAIN = 0x80D7;
        public const uint CHAN015_014_APGAIN = 0x80D8;
        public const uint CHAN017_016_APGAIN = 0x80D9;
        public const uint CHAN019_018_APGAIN = 0x80DA;
        public const uint CHAN021_020_APGAIN = 0x80DB;
        public const uint CHAN023_022_APGAIN = 0x80DC;
        public const uint CHAN025_024_APGAIN = 0x80DD;
        public const uint CHAN027_026_APGAIN = 0x80DE;
        public const uint CHAN029_028_APGAIN = 0x80DF;
        public const uint CHAN031_030_APGAIN = 0x80E0;
        public const uint CHAN033_032_APGAIN = 0x80E1;
        public const uint CHAN035_034_APGAIN = 0x80E2;
        public const uint CHAN037_036_APGAIN = 0x80E3;
        public const uint CHAN039_038_APGAIN = 0x80E4;
        public const uint CHAN041_040_APGAIN = 0x80E5;
        public const uint CHAN043_042_APGAIN = 0x80E6;
        public const uint CHAN045_044_APGAIN = 0x80E7;
        public const uint CHAN047_046_APGAIN = 0x80E8;
        public const uint CHAN049_048_APGAIN = 0x80E9;
        public const uint CHAN051_050_APGAIN = 0x80EA;
        public const uint CHAN053_052_APGAIN = 0x80EB;
        public const uint CHAN055_054_APGAIN = 0x80EC;
        public const uint CHAN057_056_APGAIN = 0x80ED;
        public const uint CHAN059_058_APGAIN = 0x80EE;
        public const uint CHAN061_060_APGAIN = 0x80EF;
        public const uint CHAN063_062_APGAIN = 0x80F0;
        public const uint CHAN065_064_APGAIN = 0x80F1;
        public const uint CHAN067_066_APGAIN = 0x80F2;
        public const uint CHAN069_068_APGAIN = 0x80F3;
        public const uint CHAN071_070_APGAIN = 0x80F4;
        public const uint CHAN073_072_APGAIN = 0x80F5;
        public const uint CHAN075_074_APGAIN = 0x80F6;
        public const uint CHAN077_076_APGAIN = 0x80F7;
        public const uint CHAN079_078_APGAIN = 0x80F8;
        public const uint CHAN081_080_APGAIN = 0x80F9;
        public const uint CHAN083_082_APGAIN = 0x80FA;
        public const uint CHAN085_084_APGAIN = 0x80FB;
        public const uint CHAN087_086_APGAIN = 0x80FC;
        public const uint CHAN089_088_APGAIN = 0x80FD;
        public const uint CHAN091_090_APGAIN = 0x80FE;
        public const uint CHAN093_092_APGAIN = 0x80FF;
        public const uint CHAN095_094_APGAIN = 0x8100;
        public const uint CHAN097_096_APGAIN = 0x8101;
        public const uint CHAN099_098_APGAIN = 0x8102;
        public const uint CHAN101_100_APGAIN = 0x8103;
        public const uint CHAN103_102_APGAIN = 0x8104;
        public const uint CHAN105_104_APGAIN = 0x8105;
        public const uint CHAN107_106_APGAIN = 0x8106;
        public const uint CHAN109_108_APGAIN = 0x8107;
        public const uint CHAN111_110_APGAIN = 0x8108;
        public const uint CHAN113_112_APGAIN = 0x8109;
        public const uint CHAN115_114_APGAIN = 0x810A;
        public const uint CHAN117_116_APGAIN = 0x810B;
        public const uint CHAN119_118_APGAIN = 0x810C;
        public const uint CHAN121_120_APGAIN = 0x810D;
        public const uint CHAN123_122_APGAIN = 0x810E;
        public const uint CHAN125_124_APGAIN = 0x810F;
        public const uint CHAN127_126_APGAIN = 0x8110;
        public const uint CHAN129_128_APGAIN = 0x8111;
        public const uint CHAN131_130_APGAIN = 0x8112;
        public const uint CHAN133_132_APGAIN = 0x8113;
        public const uint CHAN135_134_APGAIN = 0x8114;
        public const uint CHAN137_136_APGAIN = 0x8115;
        public const uint CHAN139_138_APGAIN = 0x8116;
        public const uint CHAN141_140_APGAIN = 0x8117;
        public const uint CHAN143_142_APGAIN = 0x8118;
        public const uint CHAN145_144_APGAIN = 0x8119;
        public const uint CHAN147_146_APGAIN = 0x811A;
        public const uint CHAN149_148_APGAIN = 0x811B;
        public const uint CHAN151_150_APGAIN = 0x811C;
        public const uint CHAN153_152_APGAIN = 0x811D;
        public const uint CHAN155_154_APGAIN = 0x811E;
        public const uint CHAN157_156_APGAIN = 0x811F;
        public const uint CHAN159_158_APGAIN = 0x8120;
        public const uint CHAN161_160_APGAIN = 0x8121;
        public const uint CHAN163_162_APGAIN = 0x8122;
        public const uint CHAN165_164_APGAIN = 0x8123;
        public const uint CHAN167_166_APGAIN = 0x8124;
        public const uint CHAN169_168_APGAIN = 0x8125;
        public const uint CHAN171_170_APGAIN = 0x8126;
        public const uint CHAN173_172_APGAIN = 0x8127;
        public const uint CHAN175_174_APGAIN = 0x8128;
        public const uint CHAN177_176_APGAIN = 0x8129;
        public const uint CHAN179_178_APGAIN = 0x812A;
        public const uint CHAN181_180_APGAIN = 0x812B;
        public const uint CHAN183_182_APGAIN = 0x812C;
        public const uint CHAN185_184_APGAIN = 0x812D;
        public const uint CHAN187_186_APGAIN = 0x812E;
        public const uint CHAN189_188_APGAIN = 0x812F;
        public const uint CHAN191_190_APGAIN = 0x8130;
        public const uint CHAN193_192_APGAIN = 0x8131;
        public const uint CHAN195_194_APGAIN = 0x8132;
        public const uint CHAN197_196_APGAIN = 0x8133;
        public const uint CHAN199_198_APGAIN = 0x8134;
        public const uint CHAN201_200_APGAIN = 0x8135;
        public const uint CHAN203_202_APGAIN = 0x8136;
        public const uint CHAN205_204_APGAIN = 0x8137;
        public const uint CHAN207_206_APGAIN = 0x8138;
        public const uint CHAN209_208_APGAIN = 0x8139;
        public const uint CHAN211_210_APGAIN = 0x813A;
        public const uint CHAN213_212_APGAIN = 0x813B;
        public const uint CHAN215_214_APGAIN = 0x813C;
        public const uint CHAN217_216_APGAIN = 0x813D;
        public const uint CHAN219_218_APGAIN = 0x813E;
        public const uint CHAN221_220_APGAIN = 0x813F;
        public const uint CHAN223_222_APGAIN = 0x8140;
        public const uint CHAN225_224_APGAIN = 0x8141;
        public const uint CHAN227_226_APGAIN = 0x8142;
        public const uint CHAN229_228_APGAIN = 0x8143;
        public const uint CHAN231_230_APGAIN = 0x8144;
        public const uint CHAN233_232_APGAIN = 0x8145;
        public const uint CHAN235_234_APGAIN = 0x8146;
        public const uint CHAN237_236_APGAIN = 0x8147;
        public const uint CHAN239_238_APGAIN = 0x8148;
        public const uint CHAN241_240_APGAIN = 0x8149;
        public const uint CHAN243_242_APGAIN = 0x814A;
        public const uint CHAN245_244_APGAIN = 0x814B;
        public const uint CHAN247_246_APGAIN = 0x814C;
        public const uint CHAN249_248_APGAIN = 0x814D;
        public const uint CHAN251_250_APGAIN = 0x814E;
        public const uint CHAN253_252_APGAIN = 0x814F;
        public const uint CHAN255_254_APGAIN = 0x8150;
        public const uint CHAN257_256_APGAIN = 0x8151;
        public const uint CHAN259_258_APGAIN = 0x8152;
        public const uint CHAN261_260_APGAIN = 0x8153;
        public const uint CHAN263_262_APGAIN = 0x8154;
        public const uint CHAN265_264_APGAIN = 0x8155;
        public const uint CHAN267_266_APGAIN = 0x8156;
        public const uint CHAN269_268_APGAIN = 0x8157;
        public const uint CHAN271_270_APGAIN = 0x8158;
        public const uint CHAN273_272_APGAIN = 0x8159;
        public const uint CHAN275_274_APGAIN = 0x815A;
        public const uint CHAN277_276_APGAIN = 0x815B;
        public const uint CHAN279_278_APGAIN = 0x815C;
        public const uint CHAN281_280_APGAIN = 0x815D;
        public const uint CHAN283_282_APGAIN = 0x815E;
        public const uint CHAN285_284_APGAIN = 0x815F;
        public const uint CHAN287_286_APGAIN = 0x8160;
        public const uint CHAN289_288_APGAIN = 0x8161;
        public const uint CHAN291_290_APGAIN = 0x8162;
        public const uint CHAN293_292_APGAIN = 0x8163;
        public const uint CHAN295_294_APGAIN = 0x8164;
        public const uint CHAN297_296_APGAIN = 0x8165;
        public const uint CHAN299_298_APGAIN = 0x8166;
        public const uint CHAN301_300_APGAIN = 0x8167;
        public const uint CHAN303_302_APGAIN = 0x8168;
        public const uint CHAN305_304_APGAIN = 0x8169;
        public const uint CHAN307_306_APGAIN = 0x816A;
        public const uint CHAN309_308_APGAIN = 0x816B;
        public const uint CHAN311_310_APGAIN = 0x816C;
        public const uint CHAN313_312_APGAIN = 0x816D;
        public const uint CHAN315_314_APGAIN = 0x816E;
        public const uint CHAN317_316_APGAIN = 0x816F;
        public const uint CHAN319_318_APGAIN = 0x8170;
        public const uint CHAN321_320_APGAIN = 0x8171;
        public const uint CHAN323_322_APGAIN = 0x8172;
        public const uint CHAN325_324_APGAIN = 0x8173;
        public const uint CHAN327_326_APGAIN = 0x8174;
        public const uint CHAN329_328_APGAIN = 0x8175;
        public const uint CHAN331_330_APGAIN = 0x8176;
        public const uint CHAN333_332_APGAIN = 0x8177;
        public const uint CHAN335_334_APGAIN = 0x8178;
        public const uint CHAN337_336_APGAIN = 0x8179;
        public const uint CHAN339_338_APGAIN = 0x817A;
        public const uint CHAN341_340_APGAIN = 0x817B;
        public const uint CHAN343_342_APGAIN = 0x817C;
        public const uint CHAN345_344_APGAIN = 0x817D;
        public const uint CHAN347_346_APGAIN = 0x817E;
        public const uint CHAN349_348_APGAIN = 0x817F;
        public const uint CHAN351_350_APGAIN = 0x8180;
        public const uint CHAN353_352_APGAIN = 0x8181;
        public const uint CHAN355_354_APGAIN = 0x8182;
        public const uint CHAN357_356_APGAIN = 0x8183;
        public const uint CHAN359_358_APGAIN = 0x8184;
        public const uint CHAN361_360_APGAIN = 0x8185;
        public const uint CHAN363_362_APGAIN = 0x8186;
        public const uint CHAN365_364_APGAIN = 0x8187;
        public const uint CHAN367_366_APGAIN = 0x8188;
        public const uint CHAN369_368_APGAIN = 0x8189;
        public const uint CHAN371_370_APGAIN = 0x818A;
        public const uint CHAN373_372_APGAIN = 0x818B;
        public const uint CHAN375_374_APGAIN = 0x818C;
        public const uint CHAN377_376_APGAIN = 0x818D;
        public const uint CHAN379_378_APGAIN = 0x818E;
        public const uint CHAN381_380_APGAIN = 0x818F;
        public const uint CHAN383_382_APGAIN = 0x8190;

        public const uint PROBE_SN_LSB = 0x8191;
        public const uint PROBE_SN_MSB = 0x8192;

        // unmanaged registers
        public const uint OP_MODE = 0X00;
        public const uint REC_MOD = 0X01;
        public const uint CAL_MOD = 0X02;
        public const uint TEST_CONFIG1 = 0x03;
        public const uint TEST_CONFIG2 = 0x04;
        public const uint TEST_CONFIG3 = 0x05;
        public const uint TEST_CONFIG4 = 0x06;
        public const uint TEST_CONFIG5 = 0x07;
        public const uint STATUS = 0X08;
        public const uint SYNC = 0X09;
        public const uint SR_CHAIN1 = 0X0E; // Shank configuration
        public const uint SR_CHAIN3 = 0X0C; // Odd channels
        public const uint SR_CHAIN2 = 0X0D; // Even channels
        public const uint SR_LENGTH2 = 0X0F;
        public const uint SR_LENGTH1 = 0X10;
        public const uint SOFT_RESET = 0X11;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV1f))
            {
            }
        }
    }
}
