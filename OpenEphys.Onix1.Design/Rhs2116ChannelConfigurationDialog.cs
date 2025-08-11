using System;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="ConfigureRhs2116Trigger"/>.
    /// </summary>
    public partial class Rhs2116ChannelConfigurationDialog : ChannelConfigurationDialog
    {
        internal event EventHandler OnSelect;
        internal event EventHandler OnZoom;

        /// <summary>
        /// Initializes a new instance of <see cref="Rhs2116ChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeGroup">Channel configuration settings for a <see cref="Rhs2116ProbeGroup"/>.</param>
        public Rhs2116ChannelConfigurationDialog(Rhs2116ProbeGroup probeGroup)
            : base(probeGroup)
        {
            InitializeComponent();

            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ZoomInBoundaryX = 5;
            ZoomInBoundaryY = 5;

            RefreshZedGraph();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return new Rhs2116ProbeGroup();
        }

        internal override bool OpenFile<T>()
        {
            return base.OpenFile<Rhs2116ProbeGroup>();
        }

        internal override void SelectedContactChanged()
        {
            OnSelectHandler();
        }

        private void OnSelectHandler()
        {
            OnSelect?.Invoke(this, EventArgs.Empty);
        }

        internal override void ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            base.ZoomEvent(sender, oldState, newState);
            OnZoomHandler();

            RefreshZedGraph();
        }

        private void OnZoomHandler()
        {
            OnZoom?.Invoke(this, EventArgs.Empty);
        }

        internal override float CalculateFontSize(double scale)
        {
            scale *= HasContactAnnotations(ProbeGroup) ? 0.5 : 1;
            return base.CalculateFontSize(1.35 * scale);
        }

        internal override string ContactString(int deviceChannelIndex, int index)
        {
            string s = base.ContactString(deviceChannelIndex, index);

            int indexOffset = 0;
            int probeIndex = 0;

            foreach (var probe in ProbeGroup.Probes)
            {
                if (probe.NumberOfContacts - 1 + indexOffset < index)
                {
                    indexOffset += probe.NumberOfContacts;
                    probeIndex++;
                }
                else break;
            }

            int currentIndex = index - indexOffset;

            var currentProbe = ProbeGroup.Probes.ElementAt(probeIndex);

            if (currentProbe.ContactAnnotations != null
                && currentProbe.ContactAnnotations.Annotations != null
                && currentProbe.ContactAnnotations.Annotations.Length > currentIndex)
            {
                s += "\n" + currentProbe.ContactAnnotations.Annotations[currentIndex];
            }

            return s;
        }

        // NB: Currently there is only a text label drawn as the scale for this dialog, used to denote the
        // absolute orientation of the default probe group
        internal override void DrawScale()
        {
            const string scaleTag = "scale";

            zedGraphChannels.GraphPane.GraphObjList.RemoveAll(obj => obj.Tag is string tag && tag == scaleTag);

            bool isDefault = JsonConvert.SerializeObject(ProbeGroup) == JsonConvert.SerializeObject(new Rhs2116ProbeGroup());

            if (isDefault)
            {
                var middle = GetProbeContourLeft(zedGraphChannels.GraphPane.GraphObjList)
                    + (GetProbeContourRight(zedGraphChannels.GraphPane.GraphObjList) - GetProbeContourLeft(zedGraphChannels.GraphPane.GraphObjList)) / 2;
                var top = GetProbeContourTop(zedGraphChannels.GraphPane.GraphObjList);

                TextObj textObj = new("Tether Side", middle, top + 0.5, CoordType.AxisXYScale, AlignH.Center, AlignV.Center)
                {
                    ZOrder = ZOrder.A_InFront,
                    Tag = scaleTag
                };

                SetTextObj(textObj);

                textObj.FontSpec.Size = CalculateFontSize(4.0);

                zedGraphChannels.GraphPane.GraphObjList.Add(textObj);
            }
        }
    }
}
