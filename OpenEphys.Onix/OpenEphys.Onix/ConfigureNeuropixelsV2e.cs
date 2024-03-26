using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2e : SingleDeviceFactory
    {
        public ConfigureNeuropixelsV2e()
            : base(typeof(NeuropixelsV2e))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV2 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, DS90UB9x.ID);
                device.WriteRegister(DS90UB9x.ENABLE, enable ? 1u : 0);

                // configure deserializer aliases and serializer power supply
                ConfigureDeserializer(device);
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
                var gpo10Config = EnableProbeSupply(serializer);

                // read probe metadata
                var probeAMetadata = ReadProbeMetadata(serializer, NeuropixelsV2e.ProbeASelected);
                var probeBMetadata = ReadProbeMetadata(serializer, NeuropixelsV2e.ProbeBSelected);

                // issue full reset to both probes
                ResetProbes(serializer, gpo10Config);
                var probeControl = new I2CRegisterContext(device, NeuropixelsV2e.ProbeAddress);

                // configure probe A streaming
                if (probeAMetadata.ProbeSN != null)
                {
                    SelectProbe(serializer, NeuropixelsV2e.ProbeASelected);
                    ConfigureProbeStreaming(probeControl);
                }

                // configure probe B streaming
                if (probeBMetadata.ProbeSN != null)
                {
                    SelectProbe(serializer, NeuropixelsV2e.ProbeBSelected);
                    ConfigureProbeStreaming(probeControl);
                }

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, NeuropixelsV2e.DefaultGPO10Config);
                    SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);
                });
                return new CompositeDisposable(
                    shutdown,
                    disposable);
            });
        }

        static void ConfigureDeserializer(DeviceContext device)
        {
            // configure deserializer trigger mode
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.Continuous);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, (uint)DS90UB9xDataGate.Disabled);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);

            // configure two 4-bit magic word-triggered streams, one for each probe
            device.WriteRegister(DS90UB9x.READSZ, 0x0010_0009); // 16 frames/superframe, 8x 12-bit words + magic bits
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0xC000003F); // Enable inverse, wait for non-inverse, 14-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 0b0000_0000_0010_1110); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 0b0010_0000_0000_0000_0000_0010_1011_0101);
            device.WriteRegister(DS90UB9x.DATALINES0, 0xFFFFF8A6); // NP A
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFF97B); // NP B

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint coaxMode = 0x4 + (uint)DS90UB9xMode.Raw12BitHighFrequency; // 0x4 maintains coax mode
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.PortMode, coaxMode);

            uint alias = NeuropixelsV2e.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2e.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);
        }

        static uint EnableProbeSupply(I2CRegisterContext serializer)
        {
            var gpo10Config = NeuropixelsV2e.DefaultGPO10Config | NeuropixelsV2e.GPO10SupplyMask;
            SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);

            // turn on analog supply and wait for boot
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
            System.Threading.Thread.Sleep(20);
            return gpo10Config;
        }

        NeuropixelsV2eMetadata ReadProbeMetadata(I2CRegisterContext serializer, byte probeSelect)
        {
            SelectProbe(serializer, probeSelect);
            return new NeuropixelsV2eMetadata(serializer);
        }

        static void SelectProbe(I2CRegisterContext serializer, byte probeSelect)
        {
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, probeSelect);
            System.Threading.Thread.Sleep(20);
        }

        static void ResetProbes(I2CRegisterContext serializer, uint gpo10Config)
        {
            gpo10Config &= ~NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
            gpo10Config |= NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
        }

        static void ConfigureProbeStreaming(I2CRegisterContext i2cNP)
        {
            // Write super sync bits into ASIC
            i2cNP.WriteByte(0x15, 0b00011000);
            i2cNP.WriteByte(0x14, 0b01100001);
            i2cNP.WriteByte(0x13, 0b10000110);
            i2cNP.WriteByte(0x12, 0b00011000);
            i2cNP.WriteByte(0x11, 0b01100001);
            i2cNP.WriteByte(0x10, 0b10000110);
            i2cNP.WriteByte(0x0F, 0b00011000);
            i2cNP.WriteByte(0x0E, 0b01100001);
            i2cNP.WriteByte(0x0D, 0b10000110);
            i2cNP.WriteByte(0x0C, 0b00011000);
            i2cNP.WriteByte(0x0B, 0b01100001);
            i2cNP.WriteByte(0x0A, 0b10111001);

            // Activate recording mode on NP
            i2cNP.WriteByte(0, 0b0100_0000);
        }
    }

    static class NeuropixelsV2e
    {
        public const int ProbeAddress = 0x10;
        public const int FlexEEPROMAddress = 0x50;

        public const uint GPO10SupplyMask = 1 << 3; // Used to turn on VDDA analog supply
        public const uint GPO10ResetMask = 1 << 7; // Used to issue full reset commands to probes
        public const byte DefaultGPO10Config = 0b0001_0001; // NPs in reset, VDDA not enabled
        public const byte NoProbeSelected = 0b0001_0001; // No probes selected
        public const byte ProbeASelected = 0b0001_1001; // TODO: Changes in Rev. B of headstage
        public const byte ProbeBSelected = 0b1001_1001;

        public const int FramesPerSuperFrame = 16;
        public const int ADCsPerProbe = 24;
        public const int ChannelCount = 384;
        public const int FrameWords = 36; // TRASH TRASH TRASH 0 ADC0 ADC8 ADC16 0 ADC1 ADC9 ADC17 0 ... ADC7 ADC15 ADC23 0

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2e))
            {
            }
        }
    }
}
