using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eChannelConfigurationDialog : ChannelConfigurationDialog
    {
        internal event EventHandler OnZoom;
        internal event EventHandler OnFileLoad;

        /// <summary>
        /// Public <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object that is manipulated by
        /// <see cref="NeuropixelsV2eChannelConfigurationDialog"/>.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfiguration;

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eChannelConfigurationDialog(NeuropixelsV2QuadShankProbeConfiguration probeConfiguration)
            : base(probeConfiguration.ChannelConfiguration)
        {
            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ProbeConfiguration = probeConfiguration;

            ZoomInBoundaryX = 600;
            ZoomInBoundaryY = 600;

            HighlightEnabledContacts();
            UpdateContactLabels();
            RefreshZedGraph();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return new NeuropixelsV2eProbeGroup();
        }

        internal override void LoadDefaultChannelLayout()
        {
            ProbeConfiguration = new(ProbeConfiguration.Probe, ProbeConfiguration.Reference);
            ProbeGroup = ProbeConfiguration.ChannelConfiguration;

            OnFileOpenHandler();
        }

        internal override bool OpenFile<T>()
        {
            if (base.OpenFile<NeuropixelsV2eProbeGroup>())
            {
                ProbeConfiguration = new((NeuropixelsV2eProbeGroup)ProbeGroup, ProbeConfiguration.Reference, ProbeConfiguration.Probe);

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
            DrawScale();
            RefreshZedGraph();

            OnZoomHandler();
        }

        private void OnZoomHandler()
        {
            OnZoom?.Invoke(this, EventArgs.Empty);
        }

        internal override void DrawScale()
        {
            if (ProbeConfiguration == null)
                return;

            const string ScalePointsTag = "scale_points";
            const string ScaleTextTag = "scale_text";

            zedGraphChannels.GraphPane.GraphObjList.RemoveAll(obj => obj is TextObj && obj.Tag is string tag && tag == ScaleTextTag);
            zedGraphChannels.GraphPane.CurveList.RemoveAll(curve => curve.Tag is string tag && tag == ScalePointsTag);

            const int MajorTickIncrement = 100;
            const int MajorTickLength = 10;
            const int MinorTickIncrement = 10;
            const int MinorTickLength = 5;

            if (ProbeConfiguration.ChannelConfiguration.Probes.ElementAt(0).SiUnits != ProbeSiUnits.um)
            {
                MessageBox.Show("Warning: Expected ProbeGroup units to be in microns, but it is in millimeters. Scale might not be accurate.");
            }

            var fontSize = CalculateFontSize();

            var zoomedOut = fontSize <= 2;

            fontSize = zoomedOut ? 8 : fontSize * 4;
            var majorTickOffset = MajorTickLength + CalculateScaleRange(zedGraphChannels.GraphPane.XAxis.Scale) * 0.015;
            majorTickOffset = majorTickOffset > 50 ? 50 : majorTickOffset;

            var x = GetProbeRight(zedGraphChannels.GraphPane.GraphObjList) + 50;
            var minY = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            var maxY = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            int textPosition = 0;

            PointPairList pointList = new();

            var countMajorTicks = 0;

            for (int i = (int)minY; i < maxY; i += MajorTickIncrement)
            {
                PointPair majorTickLocation = new(x + majorTickOffset, minY + MajorTickIncrement * countMajorTicks);

                pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks));
                pointList.Add(majorTickLocation);
                pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks));

                if (!zoomedOut || countMajorTicks % 5 == 0)
                {
                    TextObj textObj = new($"{textPosition} µm\n", majorTickLocation.X + 5, majorTickLocation.Y, CoordType.AxisXYScale, AlignH.Left, AlignV.Center)
                    {
                        Tag = ScaleTextTag,
                    };
                    textObj.FontSpec.Border.IsVisible = false;
                    textObj.FontSpec.Size = fontSize;
                    zedGraphChannels.GraphPane.GraphObjList.Add(textObj);

                    textPosition += zoomedOut ? 5 * MajorTickIncrement : MajorTickIncrement;
                }

                if (!zoomedOut)
                {
                    var countMinorTicks = 1;

                    for (int j = i + MinorTickIncrement; j < i + MajorTickIncrement && i + MinorTickIncrement * countMinorTicks < maxY; j += MinorTickIncrement)
                    {
                        pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks + MinorTickIncrement * countMinorTicks));
                        pointList.Add(new PointPair(x + MinorTickLength, minY + MajorTickIncrement * countMajorTicks + MinorTickIncrement * countMinorTicks));
                        pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks + MinorTickIncrement * countMinorTicks));

                        countMinorTicks++;
                    }
                }

                countMajorTicks++;
            }

            var curve = zedGraphChannels.GraphPane.AddCurve("", pointList, Color.Black, SymbolType.None);

            const float scaleBarWidth = 1;

            curve.Line.Width = scaleBarWidth; 
            curve.Label.IsVisible = false;
            curve.Symbol.IsVisible = false;
            curve.Tag = ScalePointsTag;
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
                                                            var channel = NeuropixelsV2QuadShankElectrode.GetChannelNumber(tag.ContactIndex);
                                                            return ProbeConfiguration.ChannelMap[channel].Index == tag.ContactIndex;
                                                        });

            foreach (var contact in contactsToEnable)
            {
                var tag = (ContactTag)contact.Tag;

                contact.Fill.Color = ReferenceContacts.Any(x => x == tag.ContactIndex) ? ReferenceContactFill : EnabledContactFill;
            }
        }

        internal override void UpdateContactLabels()
        {
            if (ProbeConfiguration.ChannelConfiguration == null)
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
                                                  var channel = NeuropixelsV2QuadShankElectrode.GetChannelNumber(tag.ContactIndex);
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

        internal void EnableElectrodes(List<NeuropixelsV2QuadShankElectrode> electrodes)
        {
            ProbeConfiguration.SelectElectrodes(electrodes);
        }
    }
}
