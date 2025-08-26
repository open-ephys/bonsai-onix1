using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an Rhs2116 trigger device.
    /// </summary>
    /// <remarks>
    /// The Rhs2116 Trigger device generates triggers for Intan Rhs2116 bioamplifier and stimulator chip(s)
    /// either from a remote source via external SYNC pin or locally via GPIO or TRIGGER register. This device
    /// can be used to synchronize stimulus application and recovery across chips. This configuration operator
    /// can be linked to a data IO operator, such as <see cref="Rhs2116TriggerData"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.Rhs2116StimulusSequenceEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureRhs2116Trigger : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhs2116StimulusSequencePair> stimulusSequence = new(new Rhs2116StimulusSequencePair());
        readonly BehaviorSubject<bool> triggerArmed = new(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureRhs2116Trigger"/> class.
        /// </summary>
        public ConfigureRhs2116Trigger()
            : base(typeof(Rhs2116Trigger))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="Rhs2116TriggerData"/> instance that is linked to this configuration
        /// will produce data. If set to false, it will not produce data. Note that this does not affect the
        /// ability of the device to trigger stimuli, but only affects if trigger event information is
        /// streamed back from the device. To disable the trigger see the <see cref="Armed"/> property.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the stimulus trigger device will stream stimulus delivery information.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the trigger source.
        /// </summary>
        /// <remarks>
        /// If set to <see cref="Rhs2116TriggerSource.Local"/>, respect local triggers (e.g. via GPIO 
        /// or TRIGGER register) and broadcast via sync pin. If multiple chips are connected to the SYNC pin, 
        /// then this device becomes a transmitter to trigger stimulation sequences on other RHS2116 Trigger 
        /// devices with TRIGGERSOURCE = 0x1 (receiver).
        /// If set to <see cref="Rhs2116TriggerSource.External"/>, only respect triggers received from sync pin.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the trigger source is local or external.")]
        public Rhs2116TriggerSource TriggerSource { get; set; } = Rhs2116TriggerSource.Local;

        /// <summary>
        /// Gets or sets the <see cref="Rhs2116ProbeGroup"/> channel configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        [XmlIgnore]
        public Rhs2116ProbeGroup ProbeGroup { get; set; } = new();

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// </summary>
        /// <remarks>
        /// [Obsolete]. Cannot tag this property with the Obsolete attribute due to https://github.com/dotnet/runtime/issues/100453
        /// </remarks>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public string ProbeGroupSerialize
        {
            get
            {
                var jsonString = JsonConvert.SerializeObject(ProbeGroup);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ProbeGroup = JsonConvert.DeserializeObject<Rhs2116ProbeGroup>(jsonString);
            }
        }

        /// <summary>
        /// Prevent the ProbeGroup property from being serialized.
        /// </summary>
        /// <returns>False</returns>
        [Obsolete]
        public bool ShouldSerializeProbeGroupSerialize()
        {
            return false;
        }

        /// <summary>
        /// Gets or sets the file path to a configuration file holding the Probe Group JSON specifications for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("File path to a configuration file holding the Probe Group JSON specifications for this probe. If left empty, a default file will be created next to the *.bonsai file when it is saved.")]
        [FileNameFilter(ProbeGroupHelper.ProbeGroupFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeGroupFileName { get; set; } = "";

        /// <summary>
        /// Gets or sets a string defining the path to an external ProbeGroup JSON file.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroupFileName))]
        public string ProbeGroupFileNameSerialize
        {
            get
            {
                var filename = string.IsNullOrEmpty(ProbeGroupFileName)
                                ? ProbeGroupHelper.GenerateProbeGroupFileName(DeviceAddress, DeviceType.Name)
                                : ProbeGroupFileName;

                ProbeGroupHelper.SaveExternalProbeGroupFile(ProbeGroup, filename);
                return ProbeGroupFileName;
            }
            set
            {
                var filename = string.IsNullOrEmpty(value)
                                ? ProbeGroupHelper.GenerateProbeGroupFileName(DeviceAddress, DeviceType.Name)
                                : value;

                // NB: If a file does not exist at the default file path, leave the default probe group settings as-is
                if (string.IsNullOrEmpty(ProbeGroupFileName) && !File.Exists(filename))
                {
                    return;
                }

                ProbeGroup = ProbeGroupHelper.LoadExternalProbeGroupFile<Rhs2116ProbeGroup>(filename);

                ProbeGroupFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets if trigger is armed.
        /// </summary>
        /// <remarks>
        /// If true, this device will respect triggers from the selected <see cref="TriggerSource"/>.
        /// Otherwise, triggers will be ignored. 
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("If true, respect triggers. Otherwise, triggers will not be applied.")]
        public bool Armed
        {
            get => triggerArmed.Value;
            set => triggerArmed.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the stimulus sequence.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Stimulus sequence.")]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public Rhs2116StimulusSequencePair StimulusSequence
        {
            get => stimulusSequence.Value;
            set => stimulusSequence.OnNext(value);
        }

        /// <summary>
        /// Configures an Rhs2116 Trigger device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// aN Rhs2116 Trigger device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var triggerSource = TriggerSource;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;

            return source.ConfigureDevice((context, observer) =>
            {
                var rhs2116AAddress = HeadstageRhs2116.GetRhs2116ADeviceAddress(GenericHelper.GetHubAddressFromDeviceAddress(deviceAddress));
                var rhs2116A = context.GetDeviceContext(rhs2116AAddress, typeof(Rhs2116));
                var rhs2116BAddress = HeadstageRhs2116.GetRhs2116BDeviceAddress(GenericHelper.GetHubAddressFromDeviceAddress(deviceAddress));
                var rhs2116B = context.GetDeviceContext(rhs2116BAddress, typeof(Rhs2116));

                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Rhs2116Trigger.ENABLE, enable ? 1u : 0u);
                device.WriteRegister(Rhs2116Trigger.TRIGGERSOURCE, (uint)triggerSource);

                static void WriteStimulusSequence(DeviceContext device, Rhs2116StimulusSequence sequence)
                {
                    if (!sequence.Valid)
                    {
                        throw new WorkflowException("The requested stimulus sequence is invalid.");
                    }

                    if (!sequence.FitsInHardware)
                    {
                        throw new WorkflowException(string.Format("The requested stimulus is too complex. {0}/{1} memory slots are required.",
                        sequence.StimulusSlotsRequired,
                        Rhs2116.StimMemorySlotsAvailable));
                    }

                    var registerValue = Rhs2116Config.StimulatorStepSizeToRegisters[sequence.CurrentStepSize];
                    device.WriteRegister(Rhs2116.STEPSZ, registerValue[2] << 13 | registerValue[1] << 7 | registerValue[0]);

                    var registerAddress = Rhs2116.POS00;
                    int i = 0;
                    foreach (var a in sequence.AnodicAmplitudes)
                    {
                        device.WriteRegister((uint)(registerAddress + i++), a);
                    }

                    registerAddress = Rhs2116.NEG00;
                    i = 0;
                    foreach (var a in sequence.CathodicAmplitudes)
                    {
                        device.WriteRegister((uint)(registerAddress + i++), a);
                    }

                    var dt = sequence.DeltaTable;
                    device.WriteRegister(Rhs2116.NUMDELTAS, (uint)dt.Count);

                    uint j = 0;
                    foreach (var d in dt)
                    {
                        uint idxTime = j++ << 22 | (d.Key & 0x003FFFFF);
                        device.WriteRegister(Rhs2116.DELTAIDXTIME, idxTime);
                        device.WriteRegister(Rhs2116.DELTAPOLEN, d.Value);
                    }
                }

                return new CompositeDisposable(
                    stimulusSequence.SubscribeSafe(observer, newValue =>
                    {
                        WriteStimulusSequence(rhs2116A, newValue.StimulusSequenceA);
                        WriteStimulusSequence(rhs2116B, newValue.StimulusSequenceB);
                    }),
                    triggerArmed.SubscribeSafe(observer, newValue =>
                    {
                        device.WriteRegister(Rhs2116Trigger.TRIGGERARMED, newValue ? 1u : 0u);
                    }),
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType));
            });
        }
    }

    static class Rhs2116Trigger
    {
        public const int ID = 32;

        // managed registers
        public const uint ENABLE = 0; // Enable or disable the trigger event datastream
        public const uint TRIGGERSOURCE = 1; // The LSB is used to determine the trigger source
        public const uint TRIGGER = 2; // Writing 0x1 to this register will trigger a stimulation sequence if the TRIGGERSOURCE is set to 0.
        public const uint TRIGGERARMED = 3; // 0x0: Ignore all trigger inputs regardless of TRIGGERSOURCE.
                                            // 0x1: Respect the trigger input specified by TRIGGERSOURCE

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116Trigger))
            {
            }
        }
    }

    /// <summary>
    /// Specifies the source for triggers.
    /// </summary>
    public enum Rhs2116TriggerSource
    {
        /// <summary>
        /// Respect local triggers (e.g. via GPIO or TRIGGER register) and broadcast via sync pin.
        /// </summary>
        [Description("Respect local triggers (e.g. via GPIO or TRIGGER register) and broadcast via sync pin.")]
        Local = 0,
        /// <summary>
        /// Receiver. Only respect triggers received from sync pin.
        /// </summary>
        [Description("Receiver. Only respect triggers received from sync pin.")]
        External = 1,
    }
}
