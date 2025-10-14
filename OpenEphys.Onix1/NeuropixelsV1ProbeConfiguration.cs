﻿using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Bonsai;
using Newtonsoft.Json;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV1Bank
    {
        /// <summary>
        /// Specifies that Bank A is the current bank.
        /// </summary>
        /// <remarks>Bank A is defined as shank index 0 to 383 along each shank.</remarks>
        A = 0,
        /// <summary>
        /// Specifies that Bank B is the current bank.
        /// </summary>
        /// <remarks>Bank B is defined as shank index 384 to 767 along each shank.</remarks>
        B,
        /// <summary>
        /// Specifies that Bank C is the current bank.
        /// </summary>
        /// <remarks>
        /// Bank C is defined as shank index 768 to 960 along each shank. Note that Bank C is not a full contingent
        /// of 384 channels; to compensate for this, electrodes from Bank B (starting at shank index 576) are used to
        /// generate a full 384 channel map.
        /// </remarks>
        C
    }

    /// <summary>
    /// Defines a configuration for NeuropixelsV1e.
    /// </summary>
    public class NeuropixelsV1ProbeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default values.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using default <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filter the spike-band.</param>
        /// <param name="adcCalibrationFileName">Desired or current filepath to the ADC calibration file.</param>
        /// <param name="gainCalibrationFileName">Desired or current filepath to the gain calibration file.</param>
        /// <param name="invertPolarity">Desired or current option to invert the polarity of the signal.</param>
        /// <param name="probeInterfaceFileName">Desired or current filepath to the ProbeInterface file.</param>
        public NeuropixelsV1ProbeConfiguration(
            NeuropixelsV1Gain spikeAmplifierGain,
            NeuropixelsV1Gain lfpAmplifierGain,
            NeuropixelsV1ReferenceSource reference,
            bool spikeFilter,
            string adcCalibrationFileName,
            string gainCalibrationFileName,
            bool invertPolarity,
            string probeInterfaceFileName
        )
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            AdcCalibrationFileName = adcCalibrationFileName;
            GainCalibrationFileName = gainCalibrationFileName;
            InvertPolarity = invertPolarity;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1eProbeGroup"/>
        /// values and the given gain / reference / filter settings.
        /// </summary>
        /// <param name="probeGroup">Desired or current <see cref="NeuropixelsV1eProbeGroup"/> variable.</param>
        /// <param name="spikeAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the spike-band.</param>
        /// <param name="lfpAmplifierGain">Desired or current <see cref="NeuropixelsV1Gain"/> for the LFP-band.</param>
        /// <param name="reference">Desired or current <see cref="NeuropixelsV1ReferenceSource"/>.</param>
        /// <param name="spikeFilter">Desired or current option to filter the spike-band.</param>
        /// <param name="adcCalibrationFileName">Desired or current filepath to the ADC calibration file.</param>
        /// <param name="gainCalibrationFileName">Desired or current filepath to the gain calibration file.</param>
        /// <param name="invertPolarity">Desired or current option to invert the polarity of the signal.</param>
        /// <param name="probeInterfaceFileName">Desired or current filepath to the ProbeInterface file.</param>
        public NeuropixelsV1ProbeConfiguration(
            NeuropixelsV1eProbeGroup probeGroup,
            NeuropixelsV1Gain spikeAmplifierGain,
            NeuropixelsV1Gain lfpAmplifierGain,
            NeuropixelsV1ReferenceSource reference,
            bool spikeFilter,
            string adcCalibrationFileName,
            string gainCalibrationFileName,
            bool invertPolarity,
            string probeInterfaceFileName
        )
        {
            SpikeAmplifierGain = spikeAmplifierGain;
            LfpAmplifierGain = lfpAmplifierGain;
            Reference = reference;
            SpikeFilter = spikeFilter;
            ProbeGroup = probeGroup.Clone();
            AdcCalibrationFileName = adcCalibrationFileName;
            GainCalibrationFileName = gainCalibrationFileName;
            InvertPolarity = invertPolarity;
            ProbeInterfaceFileName = probeInterfaceFileName;
        }

        /// <summary>
        /// Copy constructor initializes a new instance of <see cref="NeuropixelsV1ProbeConfiguration"/> using the given <see cref="NeuropixelsV1ProbeConfiguration"/>
        /// values.
        /// </summary>
        /// <param name="probeConfiguration">Existing <see cref="NeuropixelsV1ProbeConfiguration"/> instance.</param>
        public NeuropixelsV1ProbeConfiguration(NeuropixelsV1ProbeConfiguration probeConfiguration)
        {
            SpikeAmplifierGain = probeConfiguration.SpikeAmplifierGain;
            LfpAmplifierGain = probeConfiguration.LfpAmplifierGain;
            Reference = probeConfiguration.Reference;
            SpikeFilter = probeConfiguration.SpikeFilter;
            ProbeGroup = probeConfiguration.ProbeGroup.Clone();
            AdcCalibrationFileName = probeConfiguration.AdcCalibrationFileName;
            GainCalibrationFileName = probeConfiguration.GainCalibrationFileName;
            InvertPolarity = probeConfiguration.InvertPolarity;
            ProbeInterfaceFileName = probeConfiguration.ProbeInterfaceFileName;
        }

        /// <summary>
        /// Gets or sets the amplifier gain for the spike-band.
        /// </summary>
        /// <remarks>
        /// The spike-band is from DC to 10 kHz if <see cref="SpikeFilter"/> is set to false, while the 
        /// spike-band is from 300 Hz to 10 kHz if <see cref="SpikeFilter"/> is set to true.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        /// <summary>
        /// Gets or sets the amplifier gain for the LFP-band.
        /// </summary>
        /// <remarks>
        /// The LFP band is from 0.5 to 500 Hz.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Amplifier gain for LFP-band.")]
        public NeuropixelsV1Gain LfpAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain50;

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        /// <remarks>
        /// All electrodes are set to the same reference, which can be either 
        /// <see cref="NeuropixelsV1ReferenceSource.External"/> or <see cref="NeuropixelsV1ReferenceSource.Tip"/>. 
        /// Setting to <see cref="NeuropixelsV1ReferenceSource.External"/> will use the external reference, while 
        /// <see cref="NeuropixelsV1ReferenceSource.Tip"/> sets the reference to the electrode at the tip of the probe.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Reference selection.")]
        public NeuropixelsV1ReferenceSource Reference { get; set; } = NeuropixelsV1ReferenceSource.External;

        /// <summary>
        /// Gets or sets the state of the spike-band filter.
        /// </summary>
        /// <remarks>
        /// If set to true, the spike-band has a 300 Hz high-pass filter which will be activated. If set to
        /// false, the high-pass filter will not to be activated.
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("If true, activates a 300 Hz high-pass filter in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        /// <summary>
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public NeuropixelsV1Electrode[] ChannelMap { get => NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup); }

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
        [Category(DeviceFactory.ConfigurationCategory)]
        public string GainCalibrationFileName { get; set; }

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
        [Category(DeviceFactory.ConfigurationCategory)]
        public string AdcCalibrationFileName { get; set; }

        /// <summary>
        /// Gets or sets the file path where the ProbeInterface configuration will be saved.
        /// </summary>
        /// <remarks>
        /// If left empty, the ProbeInterface configuration will not be saved.
        /// </remarks>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to where the ProbeInterface file will be saved for this probe. If the file exists, it will be overwritten.")]
        [FileNameFilter(ProbeInterfaceHelper.ProbeInterfaceFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeInterfaceFileName { get; set; } = "";

        /// <summary>
        /// Gets or sets a string defining the path to an external ProbeInterface JSON file.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeInterfaceFileName))]
        public string ProbeInterfaceFileNameSerialize
        {
            get
            {
                if (string.IsNullOrEmpty(ProbeInterfaceFileName) || (probeGroup == null && File.Exists(ProbeInterfaceFileName)))
                    return "";

                ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(probeGroup ?? new NeuropixelsV1eProbeGroup(), ProbeInterfaceFileName);

                return ProbeInterfaceFileName;
            }
            set
            {
                ProbeInterfaceFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets a value determining if the polarity of the electrode voltages acquired by the probe
        /// should be inverted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Neuropixels contain inverting amplifiers. This means that neural data that is captured by the probe
        /// will be inverted compared to the physical signal that occurs at the electrode: e.g., extracellular
        /// action potentials will tend to have positive deflections instead of negative. Setting this
        /// property to true will apply a gain of -1 to undo this effect.
        /// </para>
        /// </remarks>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Invert the polarity of the electrode voltages acquired by the probe.")]
        public bool InvertPolarity { get; set; }

        /// <summary>
        /// Enable the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being enabled.</param>
        public void SelectElectrodes(NeuropixelsV1Electrode[] electrodes)
        {
            var channelMap = ChannelMap;

            foreach (var e in electrodes)
            {
                try
                {
                    channelMap[e.Channel] = e;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException($"Electrode {e.Index} specifies channel {e.Channel} but only channels " +
                        $"0 to {channelMap.Length - 1} are supported.", ex);
                }
            }

            ProbeGroup.UpdateDeviceChannelIndices(channelMap);
        }

        NeuropixelsV1eProbeGroup probeGroup = null;

        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV1eProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Defines all aspects of the probe group, including probe contours, electrode size and location, enabled channels, etc.")]
        [Browsable(false)]
        [Externalizable(false)]
        public NeuropixelsV1eProbeGroup ProbeGroup
        {
            get
            {
                try
                {
                    probeGroup ??= ProbeInterfaceHelper.LoadExternalProbeInterfaceFile<NeuropixelsV1eProbeGroup>(ProbeInterfaceFileName);
                }
                catch (ArgumentNullException)
                {
                    probeGroup = new();
                }

                return probeGroup;
            }
            set => probeGroup = value;
        }

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
                ProbeGroup = JsonConvert.DeserializeObject<NeuropixelsV1eProbeGroup>(jsonString);
                SelectElectrodes(NeuropixelsV1eProbeGroup.ToChannelMap(ProbeGroup));
            }
        }

        /// <summary>
        /// Prevent the ProbeGroup property from being serialized.
        /// </summary>
        /// <returns>False</returns>
        [Obsolete]
        public bool ShouldSerializeProbeGroupString()
        {
            return false;
        }
    }
}
