using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        /// Initializes a new instance of <see cref="NeuropixelsV1ChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV1ProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV1ChannelConfigurationDialog(NeuropixelsV1ProbeConfiguration probeConfiguration)
            : base(probeConfiguration.ProbeInterfaceFile)
        {
            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ReferenceContacts.AddRange(ReferenceContactsList);

            ZoomInBoundaryX = 400;
            ZoomInBoundaryY = 400;

            HighlightEnabledContacts();
            UpdateContactLabels();
            DrawScale();
            RefreshZedGraph();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return new NeuropixelsV1eProbeGroup();
        }

        internal override void LoadExternalProbeInterfaceFile()
        {
            var probeGroup = ProbeGroupHelper.LoadExternalProbeConfigurationFile<NeuropixelsV1eProbeGroup>(ProbeInterfaceFile);

            if (probeGroup.NumberOfContacts != ProbeGroup.NumberOfContacts)
                throw new InvalidOperationException("The ProbeInterface file has the wrong number of contacts. " +
                    $"Expected {ProbeGroup.NumberOfContacts} to exist, but there were {probeGroup.NumberOfContacts} found. " +
                    $"\n\nDouble check that the correct filepath is given for this probe. " +
                    $"\nFilepath: {ProbeInterfaceFile}" +
                    $"\nProbe Type: {nameof(NeuropixelsV1)}");

            ProbeGroup = probeGroup;
        }

        internal override void LoadDefaultChannelLayout()
        {
            base.LoadDefaultChannelLayout();

            OnFileOpenHandler();
        }

        internal override bool OpenFile<T>()
        {
            if (base.OpenFile<NeuropixelsV1eProbeGroup>())
            {
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
            if (ProbeGroup == null)
                return;

            const string ScalePointsTag = "scale_points";
            const string ScaleTextTag = "scale_text";

            zedGraphChannels.GraphPane.GraphObjList.RemoveAll(obj => obj is TextObj && obj.Tag is string tag && tag == ScaleTextTag);
            zedGraphChannels.GraphPane.CurveList.RemoveAll(curve => curve.Tag is string tag && tag == ScalePointsTag);

            const int MajorTickIncrement = 100;
            const int MajorTickLength = 10;
            const int MinorTickIncrement = 10;
            const int MinorTickLength = 5;

            if (ProbeGroup.Probes.ElementAt(0).SiUnits != ProbeSiUnits.um)
            {
                MessageBox.Show("Warning: Expected ProbeGroup units to be in microns, but it is in millimeters. Scale might not be accurate.");
            }

            var fontSize = CalculateFontSize();

            var zoomedOut = fontSize <= 2;

            fontSize = zoomedOut ? 6 : fontSize * 3;
            var majorTickOffset = MajorTickLength + CalculateScaleRange(zedGraphChannels.GraphPane.XAxis.Scale) * 0.015;
            majorTickOffset = majorTickOffset > 50 ? 50 : majorTickOffset;

            var x = GetProbeRight(zedGraphChannels.GraphPane.GraphObjList) + 40;
            var minY = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            var maxY = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            int textPosition = 0;

            PointPairList pointList = new();

            var countMajorTicks = 0;

            for (int i = (int)minY; i < maxY; i += MajorTickIncrement)
            {
                PointPair majorTickLocation = new(x + MajorTickLength, minY + MajorTickIncrement * countMajorTicks);

                pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks));
                pointList.Add(majorTickLocation);
                pointList.Add(new PointPair(x, minY + MajorTickIncrement * countMajorTicks));

                if (!zoomedOut || countMajorTicks % 5 == 0)
                {
                    TextObj textObj = new($"{textPosition} µm", majorTickLocation.X + 10, majorTickLocation.Y, CoordType.AxisXYScale, AlignH.Left, AlignV.Center)
                    {
                        Tag = ScaleTextTag
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

            var curve = zedGraphChannels.GraphPane.AddCurve(ScalePointsTag, pointList, Color.Black, SymbolType.None);

            const float scaleBarWidth = 1;

            curve.Line.Width = scaleBarWidth;
            curve.Label.IsVisible = false;
            curve.Symbol.IsVisible = false;
        }

        internal override void HighlightEnabledContacts()
        {
            if (ProbeGroup == null)
                return;

            var channelMap = ((NeuropixelsV1eProbeGroup)ProbeGroup).ToChannelMap();

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
                return channelMap[channel].Index == tag.ContactIndex;
            });

            foreach (var contact in contactsToEnable)
            {
                contact.Fill.Color = EnabledContactFill;
            }

            HighlightReferenceContacts();
        }

        internal override void UpdateContactLabels()
        {
            if (ProbeGroup == null)
                return;

            NeuropixelsV1Electrode[] channelMap = ((NeuropixelsV1eProbeGroup)ProbeGroup).ToChannelMap();

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

        internal void EnableElectrodes(NeuropixelsV1Electrode[] electrodes)
        {
            ((NeuropixelsV1eProbeGroup)ProbeGroup).SelectElectrodes(electrodes);
        }
    }
}
