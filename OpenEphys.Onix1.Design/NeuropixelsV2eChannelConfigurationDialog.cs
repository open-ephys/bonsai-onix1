using System;
using System.Linq;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eChannelConfigurationDialog : ScaledChannelConfigurationDialog
    {
        internal event EventHandler OnZoom;
        internal event EventHandler OnFileLoad;

        internal NeuropixelsV2ProbeConfiguration ProbeConfiguration;

        readonly Func<int, int> GetChannelNumberFunc;

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV2ProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eChannelConfigurationDialog(NeuropixelsV2ProbeConfiguration probeConfiguration)
            : base(probeConfiguration.ProbeGroup)
        {
            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ProbeConfiguration = probeConfiguration;

            GetChannelNumberFunc = ProbeConfiguration.ChannelMap[0].GetChannelNumberFunc();

            HighlightEnabledContacts();
            UpdateContactLabels();
            DrawScale();
            RefreshZedGraph();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return Activator.CreateInstance(ProbeConfiguration.ProbeGroup.GetType()) as NeuropixelsV2eProbeGroup ?? throw new InvalidOperationException("Could not create new probe group of type " + ProbeConfiguration.ProbeGroup.GetType().Name);
        }

        internal override void LoadDefaultChannelLayout()
        {
            try
            {
                ProbeConfiguration.ProbeGroup = DefaultChannelLayout() as NeuropixelsV2eProbeGroup;
                ProbeGroup = ProbeConfiguration.ProbeGroup;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Unable to Load Default", ex.Message);
                return;
            }

            OnFileOpenHandler();
        }

        internal override bool OpenFile(Type type)
        {
            if (base.OpenFile(type))
            {
                ProbeConfiguration.ProbeGroup = (NeuropixelsV2eProbeGroup)ProbeGroup;
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
            if (ProbeConfiguration == null)
                return;

            var contactObjects = zedGraphChannels.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                        .Where(c => c is not PolyObj);

            var enabledContacts = contactObjects.Where(c => c.Fill.Color == EnabledContactFill);

            foreach (var contact in enabledContacts)
            {
                contact.Fill.Color = DisabledContactFill;
            }

            var channelMap = ProbeConfiguration.ChannelMap;

            var contactsToEnable = contactObjects.Where(c =>
            {
                var tag = c.Tag as ContactTag;
                var channel = GetChannelNumberFunc(tag.ContactIndex);
                return channelMap[channel].Index == tag.ContactIndex;
            });

            foreach (var contact in contactsToEnable)
            {
                var tag = (ContactTag)contact.Tag;

                contact.Fill.Color = ReferenceContacts.Any(x => x == tag.ContactIndex) ? ReferenceContactFill : EnabledContactFill;
            }
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

            var channelMap = ProbeConfiguration.ChannelMap;

            textObjsToUpdate = textObjs.Where(c =>
            {
                var tag = c.Tag as ContactTag;
                var channel = GetChannelNumberFunc(tag.ContactIndex);
                return channelMap[channel].Index == tag.ContactIndex;
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

        internal void EnableElectrodes(NeuropixelsV2Electrode[] electrodes)
        {
            ProbeConfiguration.SelectElectrodes(electrodes);
        }
    }
}
