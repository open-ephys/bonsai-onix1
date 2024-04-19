using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2eBeta : SingleDeviceFactory
    {
        public ConfigureNeuropixelsV2eBeta()
            : base(typeof(NeuropixelsV2eBeta))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV2Beta device is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Enable headstage LED when acquiring data.")]
        public bool EnableLed { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Specifies which reference to use for all probe electrodes.")]
        public NeuropixelsV2eBetaChannelReference Reference { get; set; }

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

                // Change all the GPIOs to locally-controlled outputs; output state set to default
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
                var gpo10Config = NeuropixelsV2eBeta.DefaultGPO10Config;
                var gpo32Config = NeuropixelsV2eBeta.DefaultGPO32Config;
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, gpo32Config);

                // read probe metadata
                var probeAMetadata = ReadProbeMetadata(serializer, ref gpo32Config, NeuropixelsV2eBeta.SelectProbeA);
                var probeBMetadata = ReadProbeMetadata(serializer, ref gpo32Config, NeuropixelsV2eBeta.SelectProbeB);

                // toggle probe LED
                gpo32Config = (gpo32Config & ~NeuropixelsV2eBeta.GPO32LedMask) | (EnableLed ? 0 : NeuropixelsV2eBeta.GPO32LedMask);
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, gpo32Config);

                // REC_NRESET and NRESET go high on both probes to take the ASIC out of reset
                // TODO: not sure if REC_NRESET and NRESET are tied together on flex
                gpo10Config |= NeuropixelsV2eBeta.GPO10ResetMask | NeuropixelsV2eBeta.GPO10NResetMask;
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
                System.Threading.Thread.Sleep(20);

                // configure probe streaming
                var probeControl = new NeuropixelsV2BetaRegisterContext(device, NeuropixelsV2eBeta.ProbeAddress);

                // configure probe A streaming
                if (probeAMetadata.Version != byte.MaxValue)
                {
                    SelectProbe(serializer, ref gpo32Config, NeuropixelsV2eBeta.SelectProbeA);
                    probeControl.WriteConfiguration(Reference);
                    ConfigureProbeStreaming(probeControl);
                }

                // configure probe B streaming
                if (probeBMetadata.Version != byte.MaxValue)
                {
                    SelectProbe(serializer, ref gpo32Config, NeuropixelsV2eBeta.SelectProbeB);
                    probeControl.WriteConfiguration(Reference);
                    ConfigureProbeStreaming(probeControl);
                }

                // Both probes are now streaming, hit them with a mux reset to (roughly) sync.
                // NB: We have found that this gives PCLK-level synchronization MOST of the time.
                // However, this is not required since we have a decoder that can handle async streams.
                // Still its good to get them roughly (i.e. within 10 PCLKs) started at the same time.
                SyncProbes(serializer, gpo10Config);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, NeuropixelsV2eBeta.DefaultGPO10Config);
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, NeuropixelsV2eBeta.DefaultGPO32Config);
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
            device.WriteRegister(DS90UB9x.READSZ, 0x0010_0007); // 16 frames/superframe, 8x 12-bit words + magic bits
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0b1100000000000000_0011111111111111); // Enable inverse, wait for non-inverse, 14-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 0b0011_0011_0011_0000); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 0b10_1101_0101);
            device.WriteRegister(DS90UB9x.DATALINES0, 0x00007654); // NP A
            device.WriteRegister(DS90UB9x.DATALINES1, 0x00000123); // NP B

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint coaxMode = 0x4 + (uint)DS90UB9xMode.Raw12BitHighFrequency; // 0x4 maintains coax mode
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.PortMode, coaxMode);

            uint alias = NeuropixelsV2eBeta.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2eBeta.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);
        }

        NeuropixelsV2eMetadata ReadProbeMetadata(I2CRegisterContext serializer, ref uint gpo32Config, byte probeSelect)
        {
            SelectProbe(serializer, ref gpo32Config, probeSelect);
            return new NeuropixelsV2eMetadata(serializer);
        }

        static void SelectProbe(I2CRegisterContext serializer, ref uint gpo32Config, byte probeSelect)
        {
            gpo32Config = probeSelect switch
            {
                NeuropixelsV2eBeta.SelectProbeA => gpo32Config | NeuropixelsV2eBeta.ProbeSelectMask,
                NeuropixelsV2eBeta.SelectProbeB => gpo32Config & ~NeuropixelsV2eBeta.ProbeSelectMask,
                _ => gpo32Config
            };
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, gpo32Config);
            System.Threading.Thread.Sleep(20);
        }

        static void SyncProbes(I2CRegisterContext serializer, uint gpo10Config)
        {
            gpo10Config &= ~NeuropixelsV2eBeta.GPO10NResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);

            gpo10Config |= NeuropixelsV2eBeta.GPO10NResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
        }

        static void ConfigureProbeStreaming(I2CRegisterContext i2cNP)
        {
            // Activate recording mode on NP
            i2cNP.WriteByte(0, 0b0100_0000);

            // Set default SR configurations (comment to use custom configuration)
            i2cNP.WriteByte(1, 0b1110_1000);

            // Set global ADC settings
            i2cNP.WriteByte(3, 0b0000_1000);
        }
    }

    static class NeuropixelsV2eBeta
    {
        public const int ProbeAddress = 0x70;
        public const int FlexEEPROMAddress = 0x50;

        public const uint GPO10ResetMask = 1 << 3;  // Used to issue full reset commands to probes
        public const uint GPO10NResetMask = 1 << 7; // Used to issue full reset commands to probes
        public const uint DefaultGPO10Config = 0b0001_0001; // NPs in reset, VDDA not enabled
        public const uint DefaultGPO32Config = 0b1001_1001; // LED off, NP_A selected
        public const uint ProbeSelectMask = 1 << 3; // Used to select which probe is active
        public const uint GPO32LedMask = 1 << 7; // Used to toggle probe LED state
        public const byte SelectProbeA = 0;
        public const byte SelectProbeB = 1;

        public const int FramesPerSuperFrame = 16;
        public const int ADCsPerProbe = 24;
        public const int SyncsPerFrame = 2;
        public const int CountersPerFrame = 2;
        public const int ChannelCount = 384;
        public const int FrameWords = 28;

        public const int PixelCount = 1280;
        public const int ReferencePixelCount = 4;
        public const int DummyRegisterCount = 4;
        public const int RegistersPerShank = PixelCount + ReferencePixelCount + DummyRegisterCount;
        public const int BaseBitsPerChannel = 4;

        // memory map
        public const int STATUS = 0x09;
        public const int SR_CHAIN6 = 0x0C;
        public const int SR_CHAIN5 = 0x0D;
        public const int SR_CHAIN4 = 0x0E;
        public const int SR_CHAIN3 = 0x0F;
        public const int SR_CHAIN2 = 0x10;
        public const int SR_CHAIN1 = 0x11;
        public const int SR_LENGTH2 = 0x12;
        public const int SR_LENGTH1 = 0x13;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2eBeta))
            {
            }
        }
    }

    [Flags]
    enum NeuropixelsV2eBetaStatus : uint
    {
        SR_OK = 1 << 7 // Indicates the SR chain comparison is OK
    }

    public enum NeuropixelsV2eBetaChannelReference : uint
    {
        External,
        Tip,
        Internal
    }
}
