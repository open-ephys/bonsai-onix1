using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

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
        public static readonly IReadOnlyList<NeuropixelsV2QuadShankElectrode> ProbeModel = CreateProbeModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration()
        {
            ChannelMap = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ChannelCount);
            for (int i = 0; i < NeuropixelsV2.ChannelCount; i++)
            {
                ChannelMap.Add(ProbeModel.FirstOrDefault(e => e.Channel == i));
            }
        }

        private static List<NeuropixelsV2QuadShankElectrode> CreateProbeModel()
        {
            var electrodes = new List<NeuropixelsV2QuadShankElectrode>(NeuropixelsV2.ElectrodePerShank * 4);
            for (int i = 0; i < NeuropixelsV2.ElectrodePerShank * 4; i++)
            {
                electrodes.Add(new NeuropixelsV2QuadShankElectrode() { ElectrodeNumber = i });
            }
            return electrodes;
        }

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
        public List<NeuropixelsV2QuadShankElectrode> ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public void SelectElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes)
        {
            foreach (var e in electrodes)
            {
                ChannelMap[e.Channel] = e;
            }
        }
    }

    /// <summary>
    /// Defines a configuration for quad-shank electrodes.
    /// </summary>
    public class NeuropixelsV2QuadShankElectrode
    {
        private int electrodeNumber = 0;

        /// <summary>
        /// Gets or sets the electrode number.
        /// </summary>
        /// <remarks>
        /// When the electrode number is updated, all other properties are automatically calculated based on
        /// the number given.
        /// </remarks>
        public int ElectrodeNumber
        {
            get => electrodeNumber;
            set
            {
                electrodeNumber = value;
                Shank = electrodeNumber / NeuropixelsV2.ElectrodePerShank;
                IntraShankElectrodeIndex = electrodeNumber % NeuropixelsV2.ElectrodePerShank;

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

                    _ => throw new ArgumentOutOfRangeException($"Invalid shank and/or electrode value: {(Shank, IntraShankElectrodeIndex)}"),
                };
            }
        }

        /// <summary>
        /// Gets the channel number of this electrode.
        /// </summary>
        /// <remarks>
        /// Channel number is automatically calculated from the electrode number, and will be between 0 and 383.
        /// </remarks>
        [XmlIgnore]
        public int Channel { get; private set; } = 0;

        /// <summary>
        /// Gets the shank of this electrode.
        /// </summary>
        /// <remarks>
        /// Shank is automatically determined from the electrode number, and will be between 0 and 3.
        /// </remarks>
        [XmlIgnore]
        public int Shank { get; private set; } = 0;

        /// <summary>
        /// Gets the index of the shank of this electrode.
        /// </summary>
        /// <remarks>
        /// Shank index is automatically determined from the electrode number, and will be between 0 and 1279.
        /// </remarks>
        [XmlIgnore]
        public int IntraShankElectrodeIndex { get; private set; } = 0;

        /// <summary>
        /// Gets the <see cref="NeuropixelsV2QuadShankBank"/> of this electrode.
        /// </summary>
        /// <remarks>
        /// The bank is automatically determined from the electrode number, and corresponds to one of four logical
        /// groupings along each shank. See <see cref="NeuropixelsV2QuadShankBank"/> for more details.
        /// </remarks>
        [XmlIgnore]
        public NeuropixelsV2QuadShankBank Bank { get; private set; } = NeuropixelsV2QuadShankBank.A;

        /// <summary>
        /// Gets the position of the electrode in relation to the probe.
        /// </summary>
        [XmlIgnore]
        public PointF Position { get; private set; } = new(0f, 0f);
    }
}
