using System;
using System.Drawing;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;
using ZedGraph;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Simple dialog window that serves as the base class for all scaled Channel Configuration windows.
    /// Adds a scale bar in the same ZedGraph object as the original channel configuration.
    /// </summary>
    public abstract class ScaledChannelConfigurationDialog : ChannelConfigurationDialog
    {
        /// <summary>
        /// Constructs the scaled dialog window using the given probe group, and plots all contacts after loading.
        /// </summary>
        /// <param name="probeGroup">Channel configuration given as a <see cref="ProbeGroup"/></param>
        public ScaledChannelConfigurationDialog(ProbeGroup probeGroup) : base(probeGroup)
        {
            var pane = new GraphPane();

            InitializeScalePane(pane);

            zedGraphChannels.MasterPane.Add(pane);
            pane.Chart.Fill = zedGraphChannels.GraphPane.Chart.Fill;

            OnDrawProbeGroup += (sender, e) =>
            {
                DrawScale();
            };

            OnResizeZedGraph += (sender, e) =>
            {
                var rect = zedGraphChannels.MasterPane.Rect;

                float squareSize = rect.Height;

                zedGraphChannels.MasterPane.PaneList[0].Rect = new RectangleF(rect.Left, rect.Top, squareSize, squareSize);
                zedGraphChannels.MasterPane.PaneList[1].Rect = new RectangleF(rect.Left + squareSize, rect.Top, rect.Width - squareSize, squareSize);
            };

            OnMoveProbeGroup += (sender, e) =>
            {
                SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);
            };

            zedGraphChannels.MouseMoveEvent += (sender, e) =>
            {
                if (e.Button == MouseButtons.None)
                {
                    var currentPane = sender.MasterPane.FindPane(new PointF(e.X, e.Y));

                    if (currentPane == sender.MasterPane.PaneList[0])
                    {
                        EnablePan(sender);
                        EnableZoom(sender);
                    }
                    else if (currentPane == sender.MasterPane.PaneList[1])
                    {
                        DisablePan(sender);
                        DisableZoom(sender);
                    }
                }

                return false;
            };
        }

        static void InitializeScalePane(GraphPane pane)
        {
            pane.Title.IsVisible = false;
            pane.TitleGap = 0;
            pane.Border.IsVisible = false;
            pane.Border.Width = 0;
            pane.Chart.Border.IsVisible = false;
            pane.Margin.All = 0;

            pane.Y2Axis.IsVisible = false;

            pane.XAxis.IsVisible = false;
            pane.XAxis.IsAxisSegmentVisible = false;
            pane.XAxis.Scale.MaxAuto = true;
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.MajorGrid.IsZeroLine = false;
            pane.XAxis.CrossAuto = false;
            pane.XAxis.Cross = double.MinValue;

            pane.YAxis.IsVisible = true;
            pane.YAxis.IsAxisSegmentVisible = true;
            pane.YAxis.Scale.MaxAuto = true;
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.CrossAuto = false;
            pane.YAxis.Cross = double.MinValue;

            pane.YAxis.MajorGrid.IsZeroLine = false;
            pane.YAxis.MajorGrid.IsVisible = false;
            pane.YAxis.MinorGrid.IsVisible = false;

            pane.YAxis.Scale.IsPreventLabelOverlap = true;
            pane.YAxis.Scale.MajorStep = 100;
            pane.YAxis.Scale.IsLabelsInside = true;
            pane.YAxis.Scale.FontSpec.Size = 65f;
            pane.YAxis.Scale.FontSpec.IsBold = false;
            pane.YAxis.Scale.LabelGap = 0.6f;

            pane.YAxis.MinorTic.IsInside = false;
            pane.YAxis.MinorTic.IsOutside = false;
            pane.YAxis.MinorTic.IsOpposite = false;

            pane.YAxis.MajorTic.IsInside = true;
            pane.YAxis.MajorTic.IsOutside = false;
            pane.YAxis.MajorTic.IsOpposite = false;
            pane.YAxis.MajorTic.Size = 40f;
            pane.YAxis.MajorTic.PenWidth = 1.5f;
        }

        static void DisablePan(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableHPan = false;
            zedGraph.IsEnableVPan = false;
        }

        static void DisableZoom(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableZoom = false;
            zedGraph.IsEnableWheelZoom = false;
        }

        static void SyncYAxes(GraphPane source, GraphPane target)
        {
            target.YAxis.Scale.Min = source.YAxis.Scale.Min;
            target.YAxis.Scale.Max = source.YAxis.Scale.Max;
        }

        internal override void ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            base.ZoomEvent(sender, oldState, newState);

            SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);
        }

        internal override void ResizeAxes()
        {
            const float ScalingFactor = 1.15f;
            RectangleF rect = zedGraphChannels.MasterPane.Rect;

            float width = rect.Width;
            float height = rect.Height;

            float desiredWidth = height * ScalingFactor;

            if (width < desiredWidth)
            {
                height = width / ScalingFactor;
            }
            else
            {
                width = desiredWidth;
            }

            float x = MathF.Round(rect.Left + (rect.Width - width) / 2f);
            float y = MathF.Round(rect.Top + (rect.Height - height) / 2f);

            var newRect = new RectangleF(x, y, width, height);

            zedGraphChannels.MasterPane.Rect = newRect;
        }

        internal virtual void DrawScale() { }
    }
}
