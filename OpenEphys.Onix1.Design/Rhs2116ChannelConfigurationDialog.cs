using System;
using System.Linq;
using System.Windows.Forms;
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
        internal event EventHandler OnFileLoad;

        internal override string ProbeName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Rhs2116ChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeGroup">Channel configuration settings for a <see cref="Rhs2116ProbeGroup"/>.</param>
        /// <param name="probeName">The name of the probe.</param>
        // TODO: Call the new ChannelConfigurationDialog constructor with the ProbeInterface file
        // name instead of the probe group
        public Rhs2116ChannelConfigurationDialog(Rhs2116ProbeGroup probeGroup, string probeName)
            : base(probeGroup)
        {
            InitializeComponent();
            ProbeGroup = probeGroup;
            ProbeName = probeName;

            zedGraphChannels.ZoomButtons = MouseButtons.None;
            zedGraphChannels.ZoomButtons2 = MouseButtons.None;

            zedGraphChannels.ZoomStepFraction = 0.5;

            ZoomInBoundaryX = 2;
            ZoomInBoundaryY = 2;

            DrawProbeGroup();
            RefreshZedGraph();
        }

        internal override void LoadDefaultChannelLayout()
        {
            ProbeGroup = DefaultChannelLayout();

            OnFileOpenHandler();
        }

        internal override ProbeGroup DefaultChannelLayout()
        {
            return new Rhs2116ProbeGroup();
        }

        internal override bool OpenNewFile(bool updateFileName = false)
        {
            if (base.OpenNewFile(updateFileName))
            {
                OnFileOpenHandler();

                return true;
            }

            return false;
        }

        void OnFileOpenHandler()
        {
            OnFileLoad?.Invoke(this, EventArgs.Empty);
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
    }
}
