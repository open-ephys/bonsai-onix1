using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    public enum NeuropixelsV2QuadShankReference : uint
    {
        External,
        Tip1,
        Tip2,
        Tip3,
        Tip4
    }
    public enum NeuropixelsV2QuadShankBank
    {
        A,
        B,
        C,
        D,
    }

    public class NeuropixelsV2QuadShankProbeConfiguration
    {
        //public static readonly IReadOnlyList<NeuropixelsV2QuadShankElectrode> ProbeModel = CreateProbeModel();

        public NeuropixelsV2QuadShankReference Reference { get; set; } = NeuropixelsV2QuadShankReference.External;

        [XmlIgnore]
        public List<NeuropixelsV2QuadShankElectrode> ChannelMap { get; }


        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming")]
        public NeuropixelsV2eProbeGroup ChannelConfiguration { get; private set; } = new();

        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ChannelConfiguration))]
        public string ChannelConfigurationString
        {
            get
            {
                var jsonString = JsonConvert.SerializeObject(ChannelConfiguration);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }
            set
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ChannelConfiguration = JsonConvert.DeserializeObject<NeuropixelsV2eProbeGroup>(jsonString);
            }
        }

        public NeuropixelsV2QuadShankProbeConfiguration()
        {
            //ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ChannelCount);
            //for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            //{
            //    ChannelMap.Add(ProbeModel.FirstOrDefault(e => e.Channel == i));
            //}
        }

        //private static List<NeuropixelsV2QuadShankElectrode> CreateProbeModel()
        //{
        //    var electrodes = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ElectrodePerShank * 4);
        //    for (int i = 0; i < NeuropixelsV2.ElectrodePerShank * 4; i++)
        //    {
        //        electrodes.Add(new NeuropixelsV2QuadShankElectrode(i));
        //    }
        //    return electrodes;
        //}

        //public void SelectElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes)
        //{
        //    foreach (var e in electrodes)
        //    {
        //        ChannelMap[e.Channel] = e;
        //    }
        //}
    }
}
