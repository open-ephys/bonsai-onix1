using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the reference for a quad-shank probe.
    /// </summary>
    public enum NeuropixelsV2QuadShankReference : uint
    {
        /// <summary>
        /// Specifies that the External reference will be used.
        /// </summary>
        External,
        /// <summary>
        /// Specifies that the tip reference of shank 1 will be used.
        /// </summary>
        Tip1,
        /// <summary>
        /// Specifies that the tip reference of shank 2 will be used.
        /// </summary>
        Tip2,
        /// <summary>
        /// Specifies that the tip reference of shank 3 will be used.
        /// </summary>
        Tip3,
        /// <summary>
        /// Specifies that the tip reference of shank 4 will be used.
        /// </summary>
        Tip4
    }

    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV2QuadShankBank
    {
        /// <summary>
        /// Specifies that Bank A is the current bank.
        /// </summary>
        /// <remarks>Bank A is defined as shank index 0 to 383 along each shank.</remarks>
        A,
        /// <summary>
        /// Specifies that Bank B is the current bank.
        /// </summary>
        /// <remarks>Bank B is defined as shank index 384 to 767 along each shank.</remarks>
        B,
        /// <summary>
        /// Specifies that Bank C is the current bank.
        /// </summary>
        /// <remarks>Bank C is defined as shank index 768 to 1151 along each shank.</remarks>
        C,
        /// <summary>
        /// Specifies that Bank D is the current bank.
        /// </summary>
        /// <remarks>
        /// Bank D is defined as shank index 1152 to 1279 along each shank. Note that Bank D is not a full contingent
        /// of 384 channels; to compensate for this, electrodes from Bank C (starting at shank index 896) are used to
        /// generate a full 384 channel map.
        /// </remarks>
        D,
    }

    /// <summary>
    /// Defines a configuration for quad-shank, Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    public class NeuropixelsV2QuadShankProbeConfiguration
    {
        /// <summary>
        /// Creates a model of the probe with all electrodes instantiated.
        /// </summary>
        [XmlIgnore]
        public static readonly IReadOnlyList<NeuropixelsV2QuadShankElectrode> ProbeModel = CreateProbeModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
            ChannelMap = new NeuropixelsV2QuadShankElectrode[NeuropixelsV2.ChannelCount];
            for (int i = 0; i < ChannelMap.Length; i++)
            {
                ChannelMap[i] = ProbeModel.FirstOrDefault(e => e.Channel == i);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe) : this()
        {
            Probe = probe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given probe and reference values.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2QuadShankReference reference) : this()
        {
            Probe = probe;
            Reference = reference;
        }

        /// <summary>
        /// Copy constructor for the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        /// <param name="probeConfiguration">The existing <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object to copy.</param>
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
        {
            Reference = probeConfiguration.Reference;
            var probes = probeConfiguration.ProbeGroup.Probes.ToList().Select(probe => new Probe(probe));
            ProbeGroup = new(probeConfiguration.ProbeGroup.Specification, probeConfiguration.ProbeGroup.Version, probes.ToArray());
            ChannelMap = NeuropixelsV2eProbeGroup.ToChannelMap(ProbeGroup);
            Probe = probeConfiguration.Probe;
            GainCalibrationFileName = probeConfiguration.GainCalibrationFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class with the given
        /// <see cref="NeuropixelsV2eProbeGroup"/> channel configuration. The <see cref="ChannelMap"/> is automatically 
        /// generated from the <see cref="ProbeGroup"/>. 
        /// </summary>
        /// <param name="probeGroup">The existing <see cref="NeuropixelsV2eProbeGroup"/> instance to use.</param>
        /// <param name="reference">The <see cref="NeuropixelsV2QuadShankReference"/> reference value.</param>
        /// <param name="probe">The <see cref="NeuropixelsV2Probe"/> for this probe.</param>
        /// <param name="gainCalibrationFileName">Filepath to the gain calibration file for this probe.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2eProbeGroup probeGroup, NeuropixelsV2QuadShankReference reference,
            NeuropixelsV2Probe probe, string gainCalibrationFileName)
        {
            ChannelMap = NeuropixelsV2eProbeGroup.ToChannelMap(probeGroup);
            ProbeGroup = probeGroup;
            Reference = reference;
            Probe = probe;
            GainCalibrationFileName = gainCalibrationFileName;
        }

        private static List<NeuropixelsV2QuadShankElectrode> CreateProbeModel()
        {
            var electrodes = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ElectrodePerShank * 4);
            for (int i = 0; i < NeuropixelsV2.ElectrodePerShank * 4; i++)
            {
                electrodes.Add(new NeuropixelsV2QuadShankElectrode(i));
            }
            return electrodes;
        }

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV2Probe"/> for this probe.
        /// </summary>
        public NeuropixelsV2Probe Probe { get; set; } = NeuropixelsV2Probe.ProbeA;

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// All electrodes are set to the same reference, which can be  
        /// <see cref="NeuropixelsV2QuadShankReference.External"/> or any of the tip references 
        /// (<see cref="NeuropixelsV2QuadShankReference.Tip1"/>, <see cref="NeuropixelsV2QuadShankReference.Tip2"/>, etc.). 
        /// Setting to <see cref="NeuropixelsV2QuadShankReference.External"/> will use the external reference, while 
        /// <see cref="NeuropixelsV2QuadShankReference.Tip1"/> sets the reference to the electrode at the tip of the first shank.
        /// </remarks>
        public NeuropixelsV2QuadShankReference Reference { get; set; } = NeuropixelsV2QuadShankReference.External;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public NeuropixelsV2QuadShankElectrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(NeuropixelsV2QuadShankElectrode[] electrodes)
        {
            foreach (var e in electrodes)
            {
                try
                {
                    ChannelMap[e.Channel] = e;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {ChannelMap.Length - 1} are supported.", ex);
                }
            }

            ProbeGroup.UpdateDeviceChannelIndices(ChannelMap);
        }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV2eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        public NeuropixelsV2eProbeGroup ProbeGroup { get; private set; } = new();

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
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
                return JsonConvert.SerializeObject(ProbeGroup);
                //var jsonString = JsonConvert.SerializeObject(ProbeGroup);
                //return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV2eProbeGroup>(value);
                SelectElectrodes(NeuropixelsV2eProbeGroup.ToChannelMap(ProbeGroup));
                //var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                //ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV2eProbeGroup>(jsonString);
                //SelectElectrodes(NeuropixelsV2eProbeGroup.ToChannelMap(ProbeGroup));
            }
        }

        ///// <summary>
        ///// Prevent the ProbeGroup property from being serialized.
        ///// </summary>
        ///// <returns>False</returns>
        //[Obsolete]
        //public bool ShouldSerializeProbeGroupSerialize()
        //{
        //    return false;
        //}

        /// <summary>
        /// Gets or sets the path to the gain calibration file name for this probe.
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
        [Category(DeviceFactory.ConfigurationCategory)]
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for this probe.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileName { get; set; }

        /// <summary>
        /// Gets or sets the file path to a configuration file holding the Probe Group JSON specifications for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to a configuration file holding the Probe Group JSON specifications for this probe. If left empty, a default file will be created next to the *.bonsai file when it is saved.")]
        [FileNameFilter(ProbeGroupHelper.ProbeGroupFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ProbeGroupHelper.ProbeGroupFileNameConverter))]
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
                var filename = "temp";

                ProbeGroupHelper.SaveExternalProbeGroupFile(ProbeGroup, filename);
                return ProbeGroupFileName;
            }
            set
            {
                ProbeGroupFileName = value;
                var filename = "temp";

                // NB: If a file does not exist at the default file path, leave the default probe group settings as-is
                if (string.IsNullOrEmpty(ProbeGroupFileName) && !File.Exists(filename))
                {
                    return;
                }

                ProbeGroup = new(ProbeGroupHelper.LoadExternalProbeGroupFile<NeuropixelsV2eProbeGroup>(filename));
            }
        }

        private string GetFileName()
        {
            if (string.IsNullOrEmpty(ProbeGroupFileName))
            {
                if (string.IsNullOrEmpty(defaultFileName))
                {
                    throw new InvalidOperationException($"The default filename for {nameof(NeuropixelsV2QuadShankProbeConfiguration)} is not set. Cannot save the ProbeGroup file automatically.");
                }

                return defaultFileName;
            }

            return ProbeGroupFileName;
        }

        private string defaultFileName = "";

        internal void SetProbeGroupFileName(string deviceName, uint deviceAddress, NeuropixelsV2Probe probe)
        {
            var name = probe switch
            {
                NeuropixelsV2Probe.ProbeA => deviceName + "_probeA",
                NeuropixelsV2Probe.ProbeB => deviceName + "_probeB",
                _ => throw new NotImplementedException(),
            };

            defaultFileName = ProbeGroupHelper.GenerateProbeGroupFileName(deviceAddress, name);
        }

    }
}
