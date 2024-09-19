using System;
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

        /// <summary>
        /// Initializes a new instance of <see cref="Rhs2116ChannelConfigurationDialog"/>.
        /// </summary>
        /// <param name="probeGroup">Channel configuration settings for a <see cref="Rhs2116ProbeGroup"/>.</param>
        public Rhs2116ChannelConfigurationDialog(Rhs2116ProbeGroup probeGroup)
            : base(probeGroup)
        {
            InitializeComponent();
            ProbeGroup = probeGroup;

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

        internal override float CalculateFontSize(double _)
        {
            return base.CalculateFontSize(1.35);
        }
    }
}
