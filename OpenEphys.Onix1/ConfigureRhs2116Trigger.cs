using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an ONIX RHS 2116 Trigger device.
    /// </summary>
    /// <remarks>
    /// The RHS2116 Trigger device generates triggers for Intan RHS2116 bioamplifier and stimulator chip(s)
    /// either from a remote source via external SYNC pin or locally via GPIO or TRIGGER register. This 
    /// device can be used to synchronize stimulus application and recovery across chips.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.Rhs2116StimulusSequenceEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureRhs2116Trigger : SingleDeviceFactory
    {
        readonly BehaviorSubject<Rhs2116StimulusSequencePair> stimulusSequence = new(new Rhs2116StimulusSequencePair());

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureRhs2116Trigger"/> class.
        /// </summary>
        public ConfigureRhs2116Trigger()
            : base(typeof(Rhs2116Trigger))
        {
        }

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
        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("Defines the channel configuration")]
        public Rhs2116ProbeGroup ProbeGroup { get; set; } = new();

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// </summary>
        /// <remarks>
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </remarks>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public string ProbeGroupString
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
        /// Gets or sets the stimulus sequence.
        /// </summary>
        [Category(AcquisitionCategory)]
        [Description("Stimulus sequence.")]
        public Rhs2116StimulusSequencePair StimulusSequence
        {
            get => stimulusSequence.Value;
            set => stimulusSequence.OnNext(value);
        }

        /// <summary>
        /// Configures an RHS2116 Trigger device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// aN RHS2116 Trigger device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
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
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType));
            });
        }
    }

    static class Rhs2116Trigger
    {
        public const int ID = 32;

        // managed registers
        public const uint ENABLE = 0; // Writes and reads to ENABLE are ignored without error
        public const uint TRIGGERSOURCE = 1; // The LSB is used to determine the trigger source
        public const uint TRIGGER = 2; // Writing 0x1 to this register will trigger a stimulation sequence if the TRIGGERSOURCE is set to 0.

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
