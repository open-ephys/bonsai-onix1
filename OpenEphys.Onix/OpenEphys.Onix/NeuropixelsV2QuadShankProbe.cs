using System;
using System.Collections.Generic;
using System.Drawing;
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

    public class NeuropixelsV2QuadShankProbe
    {
        [XmlIgnore]
        public readonly List<NeuropixelsV2QuadShankElectrode> Electrodes;

        public NeuropixelsV2QuadShankProbe()
        {
            Electrodes = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2Definitions.ElectrodePerShank * 4);
            for (int i = 0; i < NeuropixelsV2Definitions.ElectrodePerShank * 4; i++)
            {
                Electrodes.Add(new NeuropixelsV2QuadShankElectrode() { ElectrodeNumber = i });  
            }

            ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2Definitions.ChannelCount);
            for (int i = 0; i < NeuropixelsV2Definitions.ChannelCount; i++)
            {
                ChannelMap.Add(Electrodes.Find(e => e.Channel == i));
            }
        }

        public NeuropixelsV2QuadShankReference Reference { get; set; } = NeuropixelsV2QuadShankReference.External;

        public List<NeuropixelsV2QuadShankElectrode> ChannelMap { get;  }

        public void SelectElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes)
        {
            foreach (var e in electrodes)
            {
                ChannelMap[e.Channel] = e;
            }
        }
    }

    public class NeuropixelsV2QuadShankElectrode
    {
        private int electrodeNumber = 0;
        public int ElectrodeNumber
        {
            get
            {
                return electrodeNumber;
            }
            set
            {
                electrodeNumber = value;
                Shank = electrodeNumber / NeuropixelsV2Definitions.ElectrodePerShank;
                IntraShankElectrodeIndex = electrodeNumber % NeuropixelsV2Definitions.ElectrodePerShank;

                Position = new PointF(x: electrodeNumber % 2 * 32.0f + 8.0f, y: (IntraShankElectrodeIndex - (IntraShankElectrodeIndex % 2)) * 7.5f);

                if (IntraShankElectrodeIndex < 384)
                    Bank = NeuropixelsV2QuadShankBank.A;
                else if (IntraShankElectrodeIndex >= 384 && IntraShankElectrodeIndex < 768)
                    Bank = NeuropixelsV2QuadShankBank.B;
                else if (IntraShankElectrodeIndex >= 768 && IntraShankElectrodeIndex < 1152)
                    Bank = NeuropixelsV2QuadShankBank.C;
                else
                    Bank = NeuropixelsV2QuadShankBank.D;

                var block = IntraShankElectrodeIndex % 384 / 48;
                var blockIndex = IntraShankElectrodeIndex % 48;

                Channel = (Shank, block) switch
                {
                    (0, 0) => blockIndex + 48 * 0,
                    (0, 1) => blockIndex + 48 * 2,
                    (0, 2) => blockIndex + 48 * 4,
                    (0, 3) => blockIndex + 48 * 6,
                    (0, 4) => blockIndex + 48 * 5,
                    (0, 5) => blockIndex + 48 * 7,
                    (0, 6) => blockIndex + 48 * 1,
                    (0, 7) => blockIndex + 48 * 3,

                    (1, 0) => blockIndex + 48 * 1,
                    (1, 1) => blockIndex + 48 * 3,
                    (1, 2) => blockIndex + 48 * 5,
                    (1, 3) => blockIndex + 48 * 7,
                    (1, 4) => blockIndex + 48 * 4,
                    (1, 5) => blockIndex + 48 * 6,
                    (1, 6) => blockIndex + 48 * 0,
                    (1, 7) => blockIndex + 48 * 2,

                    (2, 0) => blockIndex + 48 * 4,
                    (2, 1) => blockIndex + 48 * 6,
                    (2, 2) => blockIndex + 48 * 0,
                    (2, 3) => blockIndex + 48 * 2,
                    (2, 4) => blockIndex + 48 * 1,
                    (2, 5) => blockIndex + 48 * 3,
                    (2, 6) => blockIndex + 48 * 5,
                    (2, 7) => blockIndex + 48 * 7,

                    (3, 0) => blockIndex + 48 * 5,
                    (3, 1) => blockIndex + 48 * 7,
                    (3, 2) => blockIndex + 48 * 1,
                    (3, 3) => blockIndex + 48 * 3,
                    (3, 4) => blockIndex + 48 * 0,
                    (3, 5) => blockIndex + 48 * 2,
                    (3, 6) => blockIndex + 48 * 4,
                    (3, 7) => blockIndex + 48 * 6,

                    _ => throw new ArgumentOutOfRangeException($"Invalid shank and/or block value: {(Shank, block)}"),
                };
            }
        }

        [XmlIgnore]
        public int Channel { get; private set; } = 0;
        [XmlIgnore]
        public int Shank { get; private set; } = 0;
        [XmlIgnore]
        public int IntraShankElectrodeIndex { get; private set; } = 0;
        [XmlIgnore]
        public NeuropixelsV2QuadShankBank Bank { get; private set; } = NeuropixelsV2QuadShankBank.A;
        [XmlIgnore]
        public PointF Position { get; private set; } = new(0f, 0f);
    }
}
