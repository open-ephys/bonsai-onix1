using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for the <see cref="ConfigureNeuropixelsV2e"/> class.
    /// </summary>
    public partial class NeuropixelsV1ChannelConfigurationDialog : ChannelConfigurationDialog
    {
        internal event EventHandler OnZoom;
        internal event EventHandler OnFileLoad;

        readonly IReadOnlyList<int> ReferenceContactsList = new List<int> { 191, 575, 959 };

        /// <summary>
        /// Public <see cref="NeuropixelsV1ProbeConfiguration"/> object that is modified by
        /// <see cref="NeuropixelsV1ChannelConfigurationDialog"/>.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV1ProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV1ChannelConfigurationDialog(NeuropixelsV1ProbeConfiguration probeConfiguration)
            : base(probeConfiguration.ProbeGroup)
        {
            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ReferenceContacts.AddRange(ReferenceContactsList);

            ProbeConfiguration = probeConfiguration;

            ZoomInBoundaryX = 200;
            ZoomInBoundaryY = 200;

            HighlightEnabledContacts();
            UpdateContactLabels();
            DrawScale();
            RefreshZedGraph();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return new NeuropixelsV1eProbeGroup();
        }

        internal override void LoadDefaultChannelLayout()
        {
            ProbeConfiguration = new(ProbeConfiguration.SpikeAmplifierGain, ProbeConfiguration.LfpAmplifierGain, ProbeConfiguration.Reference, ProbeConfiguration.SpikeFilter);
            ProbeGroup = ProbeConfiguration.ProbeGroup;

            OnFileOpenHandler();
        }

        internal override bool OpenFile<T>()
        {
            if (base.OpenFile<NeuropixelsV1eProbeGroup>())
            {
                ProbeConfiguration = new((NeuropixelsV1eProbeGroup)ProbeGroup, ProbeConfiguration.SpikeAmplifierGain, ProbeConfiguration.LfpAmplifierGain, ProbeConfiguration.Reference, ProbeConfiguration.SpikeFilter);
                ProbeGroup = ProbeConfiguration.ProbeGroup;

                OnFileOpenHandler();

                return true;
            }

            return false;
        }

        private void OnFileOpenHandler()
        {
            OnFileLoad?.Invoke(this, EventArgs.Empty);
        }

        internal override void ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            base.ZoomEvent(sender, oldState, newState);

            UpdateFontSize();
            RefreshZedGraph();

            OnZoomHandler();
        }

        private void OnZoomHandler()
        {
            OnZoom?.Invoke(this, EventArgs.Empty);
        }

        internal override bool IsDrawScale() => true;

        internal override void DrawScale()
        {
            if (ProbeConfiguration == null || zedGraphChannels.MasterPane.PaneList.Count < 2)
                return;

            var pane = zedGraphChannels.MasterPane.PaneList[1];

            pane.YAxis.Scale.Min = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            pane.YAxis.Scale.Max = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            pane.YAxis.Scale.Format = "#####0' " + ProbeGroup.Probes.First().SiUnits.ToString() + "'";
            pane.YAxis.Scale.Mag = 0;
            pane.YAxis.Scale.MagAuto = false;
        }

        internal override void HighlightEnabledContacts()
        {
            if (ProbeConfiguration == null || ProbeConfiguration.ChannelMap == null)
                return;

            var contactObjects = zedGraphChannels.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                        .Where(c => c is not PolyObj);

            var enabledContacts = contactObjects.Where(c => c.Fill.Color == EnabledContactFill);

            foreach (var contact in enabledContacts)
            {
                contact.Fill.Color = DisabledContactFill;
            }

            var contactsToEnable = contactObjects.Where(c =>
            {
                var tag = c.Tag as ContactTag;
                var channel = NeuropixelsV1Electrode.GetChannelNumber(tag.ContactIndex);
                return ProbeConfiguration.ChannelMap[channel].Index == tag.ContactIndex;
            });

            foreach (var contact in contactsToEnable)
            {
                contact.Fill.Color = EnabledContactFill;
            }

            HighlightReferenceContacts();
        }

        internal override void UpdateContactLabels()
        {
            if (ProbeConfiguration.ProbeGroup == null)
                return;

            var textObjs = zedGraphChannels.GraphPane.GraphObjList.OfType<TextObj>()
                                                                  .Where(t => t.Tag is ContactTag);

            var textObjsToUpdate = textObjs.Where(t => t.FontSpec.FontColor != DisabledContactTextColor);

            foreach (var textObj in textObjsToUpdate)
            {
                textObj.FontSpec.FontColor = DisabledContactTextColor;
            }

            textObjsToUpdate = textObjs.Where(c =>
            {
                var tag = c.Tag as ContactTag;
                var channel = NeuropixelsV1Electrode.GetChannelNumber(tag.ContactIndex);
                return ProbeConfiguration.ChannelMap[channel].Index == tag.ContactIndex;
            });

            foreach (var textObj in textObjsToUpdate)
            {
                textObj.FontSpec.FontColor = EnabledContactTextColor;
            }
        }

        internal override string ContactString(int deviceChannelIndex, int index)
        {
            return index.ToString();
        }

        internal void EnableElectrodes(NeuropixelsV1Electrode[] electrodes)
        {
            ProbeConfiguration.SelectElectrodes(electrodes);
        }
    }
}
