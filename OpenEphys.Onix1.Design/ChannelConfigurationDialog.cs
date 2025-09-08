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
    /// Simple dialog window that serves as the base class for all Channel Configuration windows.
    /// Within, there are a number of useful methods for initializing, resizing, and drawing channels.
    /// Each device must implement their own ChannelConfigurationDialog.
    /// </summary>
    public partial class ChannelConfigurationDialog : Form
    {
        internal event EventHandler OnResizeZedGraph;
        internal event EventHandler OnDrawProbeGroup;

        ProbeGroup probeGroup;

        internal ProbeGroup ProbeGroup
        {
            get => probeGroup;
            set
            {
                probeGroup = value;
                SelectedContacts = new bool[probeGroup.NumberOfContacts];
            }
        }

        internal readonly List<int> ReferenceContacts = new();

        internal bool[] SelectedContacts { get; private set; } = null;

        [Obsolete("Designer only.", true)]
        ChannelConfigurationDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructs the dialog window using the given probe group, and plots all contacts after loading.
        /// </summary>
        /// <param name="probeGroup">Channel configuration given as a <see cref="ProbeInterface.NET.ProbeGroup"/></param>
        public ChannelConfigurationDialog(ProbeGroup probeGroup)
        {
            InitializeComponent();
            Shown += FormShown;

            if (probeGroup == null)
            {
                LoadDefaultChannelLayout();
            }
            else
            {
                ProbeGroup = probeGroup;
            }

            ReferenceContacts = new List<int>();

            zedGraphChannels.MouseDownEvent += MouseDownEvent;
            zedGraphChannels.MouseMoveEvent += MouseMoveEvent;
            zedGraphChannels.MouseUpEvent += MouseUpEvent;

            if (IsDrawScale())
            {
                var pane = new GraphPane();

                InitializeScalePane(pane);

                zedGraphChannels.MasterPane.Add(pane);
            }

            InitializeZedGraphControl(zedGraphChannels);
            InitializeProbePane(zedGraphChannels.GraphPane);

            foreach (var pane in zedGraphChannels.MasterPane.PaneList)
            {
                pane.Chart.Fill = new Fill(Color.WhiteSmoke);
            }

            DrawProbeGroup();
            RefreshZedGraph();
        }

        /// <summary>
        /// Return the default channel layout of the current device, which fully instantiates the probe group object
        /// </summary>
        /// <example>
        /// Using a class that inherits from ProbeGroup, the general usage would
        /// be the default constructor which should fully initialize a <see cref="ProbeInterface.NET.ProbeGroup"/> object.
        /// For example, if there was <code>SampleDeviceProbeGroup : ProbeGroup</code>, the body of this 
        /// function could be:
        /// <code>
        /// return new SampleDeviceProbeGroup();
        /// </code>
        /// </example>
        /// <returns>Returns an object that inherits from <see cref="ProbeInterface.NET.ProbeGroup"/></returns>
        internal virtual ProbeGroup DefaultChannelLayout()
        {
            throw new NotImplementedException();
        }

        internal virtual void LoadDefaultChannelLayout()
        {
            ProbeGroup = DefaultChannelLayout();
        }

        /// <summary>
        /// After every zoom event, check that the axis limits are equal to maintain the equal
        /// aspect ratio of the graph, ensuring that all contacts do not look smashed or stretched.
        /// </summary>
        /// <param name="sender">Incoming <see cref="ZedGraphControl"/> object</param>
        /// <param name="oldState"><code>null</code></param>
        /// <param name="newState">New state, of type <see cref="ZoomState"/></param>
        internal virtual void ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            if (newState.Type == ZoomState.StateType.Zoom || newState.Type == ZoomState.StateType.WheelZoom)
            {
                SetEqualAxisLimits(sender);

                if (CheckZoomBoundaries(sender))
                {
                    CenterAxesOnCursor(sender);
                }
                else
                {
                    sender.ZoomOut(sender.GraphPane);
                }
            }

            if (IsDrawScale())
                SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);
        }

        static void SyncYAxes(GraphPane source, GraphPane target)
        {
            target.YAxis.Scale.Min = source.YAxis.Scale.Min;
            target.YAxis.Scale.Max = source.YAxis.Scale.Max;
        }

        private void SetEqualAxisLimits(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale);

            if (rangeX == rangeY) return;

            if (rangeX > rangeY)
            {
                var diff = rangeX - rangeY;

                zedGraphControl.GraphPane.YAxis.Scale.Max += diff / 2;
                zedGraphControl.GraphPane.YAxis.Scale.Min -= diff / 2;
            }
            else if (rangeX < rangeY)
            {
                var diff = rangeY - rangeX;

                zedGraphControl.GraphPane.XAxis.Scale.Max += diff / 2;
                zedGraphControl.GraphPane.XAxis.Scale.Min -= diff / 2;
            }
        }

        private void CenterAxesOnCursor(ZedGraphControl zedGraphControl)
        {
            if ((zedGraphControl.GraphPane.XAxis.Scale.Min == ZoomOutBoundaryLeft &&
                zedGraphControl.GraphPane.XAxis.Scale.Max == ZoomOutBoundaryRight &&
                zedGraphControl.GraphPane.YAxis.Scale.Min == ZoomOutBoundaryBottom &&
                zedGraphControl.GraphPane.YAxis.Scale.Max == ZoomOutBoundaryTop) ||
                (CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) == ZoomInBoundaryX &&
                CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) == ZoomInBoundaryY))
            {
                return;
            }

            var mouseClientPosition = PointToClient(Cursor.Position);
            mouseClientPosition.X -= (zedGraphControl.Parent.Width - zedGraphControl.Width) / 2;

            var currentMousePosition = TransformPixelsToCoordinates(mouseClientPosition, zedGraphControl.GraphPane);

            var centerX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) / 2 +
                zedGraphControl.GraphPane.XAxis.Scale.Min;

            var centerY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) / 2 +
                zedGraphControl.GraphPane.YAxis.Scale.Min;

            var diffX = centerX - currentMousePosition.X;
            var diffY = centerY - currentMousePosition.Y;

            zedGraphControl.GraphPane.XAxis.Scale.Min += diffX;
            zedGraphControl.GraphPane.XAxis.Scale.Max += diffX;

            zedGraphControl.GraphPane.YAxis.Scale.Min += diffY;
            zedGraphControl.GraphPane.YAxis.Scale.Max += diffY;
        }

        internal static double CalculateScaleRange(Scale scale)
        {
            return scale.Max - scale.Min;
        }

        /// <summary>
        /// Gets the value of the zoom boundary on the x-axis.
        /// </summary>
        /// <remarks>
        /// When zooming in excessively, it is possible to lose view of the entire probe and make it
        /// difficult to zoom back out. This value is the boundary, where if the current zoom would make the x-axis
        /// less than <see cref="ZoomInBoundaryX"/>, <see cref="CheckZoomBoundaries(ZedGraphControl)"/> would 
        /// automatically zoom back out to match <see cref="ZoomInBoundaryX"/>.
        /// </remarks>
        public double ZoomInBoundaryX { get; internal set; } = 20;

        /// <summary>
        /// Gets the value of the zoom boundary on the y-axis.
        /// </summary>
        /// <remarks>
        /// When zooming in excessively, it is possible to lose view of the entire probe and make it
        /// difficult to zoom back out. This value is the boundary, where if the current zoom would make the y-axis
        /// less than <see cref="ZoomInBoundaryY"/>, <see cref="CheckZoomBoundaries(ZedGraphControl)"/> would 
        /// automatically zoom back out to match <see cref="ZoomInBoundaryY"/>.
        /// </remarks>
        public double ZoomInBoundaryY { get; internal set; } = 20;

        /// <summary>
        /// Checks if the <see cref="ZedGraphControl"/> is too zoomed in or out. If the graph is too zoomed in,
        /// reset the boundaries to match <see cref="ZoomInBoundaryX"/> and <see cref="ZoomInBoundaryY"/>. If the graph is too zoomed out,
        /// reset the boundaries to match the automatically generated boundaries based on the size of the probe.
        /// </summary>
        /// <param name="zedGraphControl">A <see cref="ZedGraphControl"/> object.</param>
        /// <returns>True if the zoom boundary has been correctly handled, False if the previous zoom state should be reinstated.</returns>
        private bool CheckZoomBoundaries(ZedGraphControl zedGraphControl)
        {
            var rangeX = CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale);

            if (rangeX < ZoomInBoundaryX || rangeY < ZoomInBoundaryY)
            {
                if (rangeX / ZoomInBoundaryX == zedGraphControl.ZoomStepFraction || rangeY / ZoomInBoundaryY == zedGraphControl.ZoomStepFraction)
                    return false;
                else
                {
                    var diff = (ZoomInBoundaryX - rangeX) / 2;
                    zedGraphControl.GraphPane.XAxis.Scale.Min -= diff;
                    zedGraphControl.GraphPane.XAxis.Scale.Max += diff;

                    diff = (ZoomInBoundaryY - rangeY) / 2;
                    zedGraphControl.GraphPane.YAxis.Scale.Min -= diff;
                    zedGraphControl.GraphPane.YAxis.Scale.Max += diff;

                    return true;
                }
            }
            else
            {
                if (zedGraphControl.GraphPane.XAxis.Scale.Min < ZoomOutBoundaryLeft && zedGraphControl.GraphPane.XAxis.Scale.Max > ZoomOutBoundaryRight ||
                    CalculateScaleRange(zedGraphControl.GraphPane.XAxis.Scale) >= ZoomOutBoundaryRight - ZoomOutBoundaryLeft)
                {
                    zedGraphControl.GraphPane.XAxis.Scale.Min = ZoomOutBoundaryLeft;
                    zedGraphControl.GraphPane.XAxis.Scale.Max = ZoomOutBoundaryRight;
                }
                if (zedGraphControl.GraphPane.YAxis.Scale.Min < ZoomOutBoundaryBottom && zedGraphControl.GraphPane.YAxis.Scale.Max > ZoomOutBoundaryTop ||
                    CalculateScaleRange(zedGraphControl.GraphPane.YAxis.Scale) >= ZoomOutBoundaryTop - ZoomOutBoundaryBottom)
                {
                    zedGraphControl.GraphPane.YAxis.Scale.Min = ZoomOutBoundaryBottom;
                    zedGraphControl.GraphPane.YAxis.Scale.Max = ZoomOutBoundaryTop;
                }

                return true;
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                menuStrip.Visible = false;
                ResizeZedGraph();
            }
        }

        internal virtual bool OpenFile<T>() where T : ProbeGroup
        {
            var newConfiguration = OpenAndParseConfigurationFile<T>();

            if (newConfiguration == null)
            {
                return false;
            }

            bool skipContactNumberMismatchCheck = false;

            if (ProbeGroup.Probes.First().Annotations.Name != newConfiguration.Probes.First().Annotations.Name)
            {
                var result = MessageBox.Show($"There is a mismatch between the current probe name ({ProbeGroup.Probes.First().Annotations.Name})" +
                    $" and the new probe name ({newConfiguration.Probes.First().Annotations.Name}). Continue loading?", "Probe Name Mismatch", MessageBoxButtons.YesNo);

                if (result == DialogResult.No)
                    return false;

                skipContactNumberMismatchCheck = true; // NB: If the probe names do not match, skip the check to see if the number of contacts match.
                                                       // Example: loading a Neuropixels single-shank 2.0 probe, but the current probe is a quad-shank 2.0 probe.
            }

            if (skipContactNumberMismatchCheck || ProbeGroup.NumberOfContacts == newConfiguration.NumberOfContacts)
            {
                newConfiguration.Validate();

                ProbeGroup = newConfiguration;

                return true;
            }
            else
            {
                MessageBox.Show($"Error: Number of contacts does not match; expected {ProbeGroup.NumberOfContacts} contacts" +
                    $", but found {newConfiguration.NumberOfContacts} contacts", "Contact Number Mismatch");

                return false;
            }
        }

        internal T OpenAndParseConfigurationFile<T>() where T : ProbeGroup
        {
            using OpenFileDialog ofd = new();

            ofd.Filter = "Probe Interface Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose probe interface file";

            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                var newConfiguration = DesignHelper.DeserializeString<T>(File.ReadAllText(ofd.FileName));

                return newConfiguration;
            }

            return null;
        }

        internal void DrawProbeGroup()
        {
            zedGraphChannels.GraphPane.GraphObjList.Clear();

            DrawProbeContour();
            DrawContacts();
            SetEqualAspectRatio();
            SetZoomOutBoundaries();
            HighlightEnabledContacts();
            HighlightSelectedContacts();
            DrawContactLabels();

            if (IsDrawScale())
            {
                DrawScale();
            }

            OnDrawProbeGroupHandler();
        }

        void OnDrawProbeGroupHandler()
        {
            OnDrawProbeGroup?.Invoke(this, EventArgs.Empty);
        }

        private double ZoomOutBoundaryLeft = default;
        private double ZoomOutBoundaryRight = default;
        private double ZoomOutBoundaryBottom = default;
        private double ZoomOutBoundaryTop = default;

        internal void DrawProbeContour()
        {
            if (ProbeGroup == null)
                return;

            foreach (var probe in ProbeGroup.Probes)
            {
                if (probe == null || probe.ProbePlanarContour == null) continue;

                PointD[] planarContours = ConvertFloatArrayToPointD(probe.ProbePlanarContour);
                PolyObj contour = new(planarContours, Color.LightGray, Color.White)
                {
                    ZOrder = ZOrder.C_BehindChartBorder
                };

                zedGraphChannels.GraphPane.GraphObjList.Add(contour);
            }
        }

        private void SetZoomOutBoundaries()
        {
            var rangeX = CalculateScaleRange(zedGraphChannels.GraphPane.XAxis.Scale);
            var rangeY = CalculateScaleRange(zedGraphChannels.GraphPane.YAxis.Scale);

            var margin = Math.Min(rangeX, rangeY) * 0.02;

            ZoomOutBoundaryLeft = zedGraphChannels.GraphPane.XAxis.Scale.Min - margin;
            ZoomOutBoundaryBottom = zedGraphChannels.GraphPane.YAxis.Scale.Min - margin;
            ZoomOutBoundaryRight = zedGraphChannels.GraphPane.XAxis.Scale.Max + margin;
            ZoomOutBoundaryTop = zedGraphChannels.GraphPane.YAxis.Scale.Max + margin;
        }

        internal void SetEqualAspectRatio()
        {
            if (zedGraphChannels.GraphPane.GraphObjList.Count == 0)
                return;

            var minX = GetProbeLeft(zedGraphChannels.GraphPane.GraphObjList);
            var minY = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            var maxX = GetProbeRight(zedGraphChannels.GraphPane.GraphObjList);
            var maxY = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

            if (rangeX == rangeY) return;

            if (rangeY < rangeX)
            {
                var diff = (rangeX - rangeY) / 2;
                minY -= diff;
                maxY += diff;
            }
            else
            {
                var diff = (rangeY - rangeX) / 2;
                minX -= diff;
                maxX += diff;
            }

            zedGraphChannels.GraphPane.XAxis.Scale.Min = minX;
            zedGraphChannels.GraphPane.XAxis.Scale.Max = maxX;

            zedGraphChannels.GraphPane.YAxis.Scale.Min = minY;
            zedGraphChannels.GraphPane.YAxis.Scale.Max = maxY;
        }

        private float contactSize = 0.0f; // NB: Store the size of a contact (radius or width, depending on the shape). Assumes that all contacts are uniform.

        internal void DrawContacts()
        {
            if (ProbeGroup == null)
                return;

            for (int probeNumber = 0; probeNumber < ProbeGroup.Probes.Count(); probeNumber++)
            {
                var probe = ProbeGroup.Probes.ElementAt(probeNumber);

                const int borderWidth = 3;

                for (int j = 0; j < probe.ContactPositions.Length; j++)
                {
                    Contact contact = probe.GetContact(j);

                    BoxObj contactObj;

                    if (contact.Shape.Equals(ContactShape.Circle))
                    {
                        var size = contact.ShapeParams.Radius.Value * 2;

                        if (contactSize == 0.0f) contactSize = contact.ShapeParams.Radius.Value;

                        contactObj = new EllipseObj(contact.PosX - size / 2, contact.PosY + size / 2, size, size, SelectedContactBorder, DisabledContactFill)
                        {
                            ZOrder = ZOrder.B_BehindLegend,
                            Tag = new ContactTag(probeNumber, contact.Index)
                        };
                    }
                    else if (contact.Shape.Equals(ContactShape.Square))
                    {
                        var size = contact.ShapeParams.Width.Value;

                        if (contactSize == 0.0f) contactSize = size / 2;

                        contactObj = new BoxObj(contact.PosX - size / 2, contact.PosY + size / 2, size, size, SelectedContactBorder, DisabledContactFill)
                        {
                            ZOrder = ZOrder.B_BehindLegend,
                            Tag = new ContactTag(probeNumber, contact.Index)
                        };
                    }
                    else if (contact.Shape.Equals(ContactShape.Rect))
                    {
                        var width = contact.ShapeParams.Width.Value;
                        var height = contact.ShapeParams.Height.Value;

                        if (contactSize == 0.0f) contactSize = width >= height ? width / 2 : height / 2;

                        contactObj = new BoxObj(contact.PosX - width / 2, contact.PosY + height / 2, width, height, SelectedContactBorder, DisabledContactFill)
                        {
                            ZOrder = ZOrder.B_BehindLegend,
                            Tag = new ContactTag(probeNumber, contact.Index)
                        };
                    }
                    else
                    {
                        MessageBox.Show("Invalid ContactShape value. Check the contact shape parameter.");
                        return;
                    }

                    contactObj.Border.Width = borderWidth;
                    contactObj.Border.IsVisible = false;
                    contactObj.Location.AlignV = AlignV.Center;
                    contactObj.Location.AlignH = AlignH.Center;

                    zedGraphChannels.GraphPane.GraphObjList.Add(contactObj);
                }
            }
        }

        internal readonly Color DisabledContactFill = Color.LightGray;
        internal readonly Color EnabledContactFill = Color.DarkBlue;
        internal readonly Color ReferenceContactFill = Color.Black;

        internal virtual void HighlightEnabledContacts()
        {
            if (ProbeGroup == null)
                return;

            var contactObjects = zedGraphChannels.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                        .Where(c => c is not PolyObj);

            var enabledContacts = contactObjects.Where(c => c.Fill.Color == EnabledContactFill);

            foreach (var contact in enabledContacts)
            {
                contact.Fill.Color = DisabledContactFill;
            }

            int previousNumberOfChannels = 0;

            foreach (var probe in ProbeGroup.Probes)
            {
                var indices = probe.DeviceChannelIndices;

                var contactsToEnable = contactObjects
                                       .Skip(previousNumberOfChannels)
                                       .Take(indices.Length)
                                       .Where((c, ind) => indices[ind] != -1);

                previousNumberOfChannels += indices.Length;

                foreach (var contact in contactsToEnable)
                {
                    contact.Fill.Color = EnabledContactFill;
                }
            }

            HighlightReferenceContacts();
        }

        internal void HighlightReferenceContacts()
        {
            if (ProbeGroup == null)
                return;

            var contactObjects = zedGraphChannels.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                        .Where(c => c is not PolyObj);

            var referenceContacts = contactObjects.Where(c =>
            {
                if (c.Tag is ContactTag tag)
                {
                    return ReferenceContacts.Any(r => tag.ContactIndex == r);
                }

                return false;
            });

            foreach (var contact in referenceContacts)
            {
                contact.Fill.Color = ReferenceContactFill;
            }
        }

        internal readonly Color DeselectedContactBorder = Color.LightGray;
        internal readonly Color SelectedContactBorder = Color.YellowGreen;

        internal virtual void HighlightSelectedContacts()
        {
            if (ProbeGroup == null)
                return;

            var contactObjects = zedGraphChannels.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                        .Where(c => c is not PolyObj);

            var selectedContacts = contactObjects.Where(c => c.Border.IsVisible);

            foreach (var contact in selectedContacts)
            {
                contact.Border.IsVisible = false;
            }

            var contactsToSelect = contactObjects.Where((c, ind) => SelectedContacts[ind]);

            if (!contactsToSelect.Any())
            {
                return;
            }

            foreach (var contact in contactsToSelect)
            {
                contact.Border.IsVisible = true;
            }
        }

        internal readonly Color DisabledContactTextColor = Color.Gray;
        internal readonly Color EnabledContactTextColor = Color.White;

        internal virtual void UpdateContactLabels()
        {
            DrawContactLabels();
        }

        internal virtual void DrawContactLabels()
        {
            if (ProbeGroup == null)
                return;

            zedGraphChannels.GraphPane.GraphObjList.RemoveAll(obj => obj is TextObj && obj.Tag is ContactTag);

            int probeNumber = 0;
            int indexOffset = 0;

            foreach (var probe in ProbeGroup.Probes)
            {
                var indices = probe.DeviceChannelIndices;
                var positions = probe.ContactPositions;

                for (int i = 0; i < indices.Length; i++)
                {
                    TextObj textObj = new(ContactString(indices[i], i + indexOffset), positions[i][0], positions[i][1], CoordType.AxisXYScale, AlignH.Center, AlignV.Center)
                    {
                        ZOrder = ZOrder.A_InFront,
                        Tag = new ContactTag(probeNumber, i)
                    };

                    SetTextObj(textObj);

                    textObj.FontSpec.FontColor = indices[i] == -1 ? DisabledContactTextColor : EnabledContactTextColor;

                    zedGraphChannels.GraphPane.GraphObjList.Add(textObj);
                }

                probeNumber++;
                indexOffset += indices.Length;
            }
        }

        internal void SetTextObj(TextObj textObj)
        {
            textObj.FontSpec.IsBold = true;
            textObj.FontSpec.Border.IsVisible = false;
            textObj.FontSpec.Fill.IsVisible = false;
            textObj.FontSpec.Fill.IsVisible = false;
        }

        const string DisabledContactString = "Off";

        internal virtual string ContactString(int deviceChannelIndex, int index)
        {
            return deviceChannelIndex == -1 ? DisabledContactString : index.ToString();
        }

        internal virtual bool IsDrawScale() => false;

        internal virtual void DrawScale() { }

        internal void UpdateFontSize()
        {
            var fontSize = CalculateFontSize();

            var textObjsToUpdate = zedGraphChannels.GraphPane.GraphObjList.OfType<TextObj>()
                                                                          .Where(t => t.Tag is ContactTag);

            foreach (var obj in textObjsToUpdate)
            {
                obj.FontSpec.Size = fontSize;
            }
        }

        internal virtual float CalculateFontSize(double scale = 1.0)
        {
            float rangeY = (float)(zedGraphChannels.GraphPane.YAxis.Scale.Max - zedGraphChannels.GraphPane.YAxis.Scale.Min);

            float contactSize = ContactSize();

            var fontSize = 250f * contactSize / rangeY;

            fontSize = fontSize < 1f ? 0.001f : fontSize;
            fontSize = fontSize > 100f ? 100f : fontSize;

            return (float)scale * fontSize;
        }

        internal float ContactSize()
        {
            var obj = zedGraphChannels.GraphPane.GraphObjList
                        .OfType<BoxObj>()
                        .Where(obj => obj is not PolyObj)
                        .FirstOrDefault();

            if (obj != null && obj != default(BoxObj))
            {
                return (float)obj.Location.Width;
            }

            return 1f;
        }

        internal static double GetProbeLeft(GraphObjList graphObjs)
        {
            if (graphObjs == null || graphObjs.Count == 0) return 0f;

            if (graphObjs.OfType<PolyObj>().Count() == 0)
            {
                return GetContactLeft(graphObjs);
            }
            else
            {
                return GetProbeContourLeft(graphObjs);
            }
        }

        internal static double GetContactLeft(GraphObjList graphObjs)
        {
            return graphObjs.OfType<BoxObj>()
                            .Min(obj => { return obj.Location.Rect.Left; });
        }

        internal static double GetProbeContourLeft(GraphObjList graphObjs)
        {
            return graphObjs.OfType<PolyObj>()
                            .Min(obj => { return obj.Points.Min(p => p.X); });
        }

        internal static double GetProbeBottom(GraphObjList graphObjs)
        {
            if (graphObjs == null || graphObjs.Count == 0) return 0f;

            if (graphObjs.OfType<PolyObj>().Count() == 0)
            {
                return GetContactBottom(graphObjs);
            }
            else
            {
                return GetProbeContourBottom(graphObjs);
            }
        }

        internal static double GetContactBottom(GraphObjList graphObjs)
        {
            return graphObjs.OfType<BoxObj>()
                            .Min(obj => { return obj.Location.Rect.Top - obj.Location.Height; });
        }

        internal static double GetProbeContourBottom(GraphObjList graphObjs)
        {
            return graphObjs.OfType<PolyObj>()
                            .Min(obj => { return obj.Points.Min(p => p.Y); });
        }

        internal static double GetProbeRight(GraphObjList graphObjs)
        {
            if (graphObjs == null || graphObjs.Count == 0) return 0f;

            if (graphObjs.OfType<PolyObj>().Count() == 0)
            {
                return GetContactRight(graphObjs);
            }
            else
            {
                return GetProbeContourRight(graphObjs);
            }
        }

        internal static double GetContactRight(GraphObjList graphObjs)
        {
            return graphObjs.OfType<BoxObj>()
                            .Max(obj => { return obj.Location.Rect.Right; });
        }

        internal static double GetProbeContourRight(GraphObjList graphObjs)
        {
            return graphObjs.OfType<PolyObj>()
                            .Max(obj => { return obj.Points.Max(p => p.X); });
        }

        internal static double GetProbeTop(GraphObjList graphObjs)
        {
            if (graphObjs == null || graphObjs.Count == 0) return 0f;

            if (graphObjs.OfType<PolyObj>().Count() == 0)
            {
                return GetContactTop(graphObjs);
            }
            else
            {
                return GetProbeContourTop(graphObjs);
            }
        }

        internal static double GetContactTop(GraphObjList graphObjs)
        {
            return graphObjs.OfType<BoxObj>()
                            .Max(obj => { return obj.Location.Rect.Bottom - obj.Location.Height; });
        }

        internal static double GetProbeContourTop(GraphObjList graphObjs)
        {
            return graphObjs.OfType<PolyObj>()
                            .Max(obj => { return obj.Points.Max(p => p.Y); });
        }

        /// <summary>
        /// Converts a two-dimensional <see cref="float"/> array into an array of <see cref="PointD"/>
        /// objects. Assumes that the float array is ordered so that the first index of each pair is 
        /// the X position, and the second index is the Y position.
        /// </summary>
        /// <param name="floats">Two-dimensional array of <see cref="float"/> values</param>
        /// <returns></returns>
        public static PointD[] ConvertFloatArrayToPointD(float[][] floats)
        {
            PointD[] pointD = new PointD[floats.Length];

            for (int i = 0; i < floats.Length; i++)
            {
                pointD[i] = new PointD(floats[i][0], floats[i][1]);
            }

            return pointD;
        }

        static void InitializeZedGraphControl(ZedGraphControl zedGraph)
        {
            zedGraph.IsZoomOnMouseCenter = true;
            zedGraph.IsAntiAlias = true;
            zedGraph.BorderStyle = BorderStyle.None;

            EnablePan(zedGraph);
            EnableZoom(zedGraph);
        }

        static void EnablePan(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableHPan = true;
            zedGraph.IsEnableVPan = true;
        }

        static void DisablePan(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableHPan = false;
            zedGraph.IsEnableVPan = false;
        }

        static void EnableZoom(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableZoom = true;
            zedGraph.IsEnableWheelZoom = true;
        }

        static void DisableZoom(ZedGraphControl zedGraph)
        {
            zedGraph.IsEnableZoom = false;
            zedGraph.IsEnableWheelZoom = false;
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

        static void InitializeProbePane(GraphPane graphPane)
        {
            graphPane.Title.IsVisible = false;
            graphPane.TitleGap = 0;
            graphPane.Border.IsVisible = false;
            graphPane.Border.Width = 0;
            graphPane.Chart.Border.IsVisible = false;
            graphPane.Margin.All = -1;
            graphPane.IsFontsScaled = true;

            graphPane.XAxis.IsVisible = false;
            graphPane.XAxis.IsAxisSegmentVisible = false;
            graphPane.XAxis.Scale.MaxAuto = true;
            graphPane.XAxis.Scale.MinAuto = true;

            graphPane.YAxis.IsVisible = false;
            graphPane.YAxis.IsAxisSegmentVisible = false;
            graphPane.YAxis.Scale.MaxAuto = true;
            graphPane.YAxis.Scale.MinAuto = true;
        }

        private void MenuItemSaveFile(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "Probe Interface Files (*.json)|*.json";
            sfd.FilterIndex = 1;
            sfd.Title = "Choose where to save the probe interface file";
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                DesignHelper.SerializeObject(ProbeGroup, sfd.FileName);
            }
        }

        internal void ConnectResizeEventHandler()
        {
            DisconnectResizeEventHandler();
            zedGraphChannels.Resize += ZedGraphChannels_Resize;
        }

        internal void DisconnectResizeEventHandler()
        {
            zedGraphChannels.Resize -= ZedGraphChannels_Resize;
        }

        private void ZedGraphChannels_Resize(object sender, EventArgs e)
        {
            ResizeZedGraph();
        }

        internal void ResizeZedGraph()
        {
            ResizeAxes();

            if (IsDrawScale())
            {
                var rect = zedGraphChannels.MasterPane.Rect;

                float squareSize = rect.Height;

                zedGraphChannels.MasterPane.PaneList[0].Rect = new RectangleF(rect.Left, rect.Top, squareSize, squareSize);
                zedGraphChannels.MasterPane.PaneList[1].Rect = new RectangleF(rect.Left + squareSize, rect.Top, rect.Width - squareSize, squareSize);
            }

            UpdateFontSize();
            RefreshZedGraph();
            Update();
            OnResizeHandler();
        }

        private void OnResizeHandler()
        {
            OnResizeZedGraph?.Invoke(this, EventArgs.Empty);
        }

        void ResizeAxes()
        {
            float scalingFactor = IsDrawScale() ? 1.15f : 1.0f;
            RectangleF rect = IsDrawScale() ? zedGraphChannels.MasterPane.Rect : zedGraphChannels.GraphPane.Rect;

            float width = rect.Width;
            float height = rect.Height;

            float desiredWidth = height * scalingFactor;

            if (width < desiredWidth)
            {
                height = width / scalingFactor;
            }
            else
            {
                width = desiredWidth;
            }

            float x = MathF.Round(rect.Left + (rect.Width - width) / 2f);
            float y = MathF.Round(rect.Top + (rect.Height - height) / 2f);

            var newRect = new RectangleF(x, y, width, height);

            if (IsDrawScale())
                zedGraphChannels.MasterPane.Rect = newRect;
            else
            {
                zedGraphChannels.GraphPane.Rect = newRect;
                DisconnectResizeEventHandler();
                zedGraphChannels.Size = new Size((int)newRect.Width, (int)newRect.Height);
                zedGraphChannels.Location = new Point((int)newRect.X, (int)newRect.Y);
                ConnectResizeEventHandler();
            }
        }

        private void MenuItemOpenFile(object sender, EventArgs e)
        {
            if (OpenFile<ProbeGroup>())
            {
                DrawProbeGroup();
                ResetZoom();
                UpdateFontSize();
                RefreshZedGraph();
            }
        }

        private void MenuItemLoadDefaultConfig(object sender, EventArgs e)
        {
            LoadDefaultChannelLayout();
            DrawProbeGroup();
            ResetZoom();
            UpdateFontSize();
            RefreshZedGraph();
        }

        private void ButtonOK(object sender, EventArgs e)
        {
            if (TopLevel)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        internal void ResetZoom()
        {
            zedGraphChannels.ZoomOutAll(zedGraphChannels.GraphPane);
            SetEqualAspectRatio();
            UpdateFontSize();
        }

        /// <summary>
        /// Shifts the whole ZedGraph to the given relative position, where 0.0 is the very bottom of the horizontal 
        /// space, and 1.0 is the very top. Note that this accounts for a buffer on the top and bottom, so giving a 
        /// value of 0.0 would have the minimum value of Y axis equal to the bottom of the graph, and keep the range 
        /// the same. Similarly, a value of 1.0 would set the maximum value of the Y axis to the top of the graph, 
        /// and keep the range the same.
        /// </summary>
        /// <param name="relativePosition">A float value defining the percentage of the graph to move to vertically</param>
        public void MoveToVerticalPosition(float relativePosition)
        {
            if (relativePosition < 0.0 || relativePosition > 1.0)
            {
                MessageBox.Show($"Warning: Invalid relative position given while moving. Expected values between 0.0 and 1.0, but received {relativePosition}.", "Invalid Relative Position");
                return;
            }

            var currentRange = zedGraphChannels.GraphPane.YAxis.Scale.Max - zedGraphChannels.GraphPane.YAxis.Scale.Min;

            var minY = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            var maxY = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            var newMinY = (maxY - minY - currentRange) * relativePosition;

            zedGraphChannels.GraphPane.YAxis.Scale.Min = newMinY;
            zedGraphChannels.GraphPane.YAxis.Scale.Max = newMinY + currentRange;

            if (IsDrawScale())
                SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);
        }

        internal float GetRelativeVerticalPosition()
        {
            var minY = GetProbeBottom(zedGraphChannels.GraphPane.GraphObjList);
            var maxY = GetProbeTop(zedGraphChannels.GraphPane.GraphObjList);

            var currentRange = zedGraphChannels.GraphPane.YAxis.Scale.Max - zedGraphChannels.GraphPane.YAxis.Scale.Min;

            if (zedGraphChannels.GraphPane.YAxis.Scale.Min <= minY)
                return 0.0f;
            else if (zedGraphChannels.GraphPane.YAxis.Scale.Min >= maxY - currentRange)
                return 1.0f;
            else
            {
                return (float)((zedGraphChannels.GraphPane.YAxis.Scale.Min - minY) / (maxY - minY - currentRange));
            }
        }

        internal void RefreshZedGraph()
        {
            zedGraphChannels.AxisChange();
            zedGraphChannels.Refresh();
        }

        PointD clickStart = new(0.0, 0.0);

        private bool MouseDownEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                clickStart = TransformPixelsToCoordinates(e.Location, sender.GraphPane);
            }

            return false;
        }

        const string SelectionAreaTag = "Selection";

        private bool MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                sender.Cursor = Cursors.Cross;

                if (clickStart.X == default && clickStart.Y == default)
                    return false;

                BoxObj oldArea = (BoxObj)sender.GraphPane.GraphObjList[SelectionAreaTag];
                if (oldArea != null)
                {
                    sender.GraphPane.GraphObjList.Remove(oldArea);
                }

                var mouseLocation = TransformPixelsToCoordinates(e.Location, sender.GraphPane);

                BoxObj selectionArea = new(
                    mouseLocation.X < clickStart.X ? mouseLocation.X : clickStart.X,
                    mouseLocation.Y > clickStart.Y ? mouseLocation.Y : clickStart.Y,
                    Math.Abs(mouseLocation.X - clickStart.X),
                    Math.Abs(mouseLocation.Y - clickStart.Y));
                selectionArea.Border.Color = Color.DarkSlateGray;
                selectionArea.Fill.IsVisible = false;
                selectionArea.ZOrder = ZOrder.A_InFront;
                selectionArea.Tag = SelectionAreaTag;

                sender.GraphPane.GraphObjList.Add(selectionArea);
                sender.Refresh();

                return true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                if (IsDrawScale())
                    SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);

                return false;
            }
            else if (e.Button == MouseButtons.None)
            {
                sender.Cursor = Cursors.Arrow;

                var currentPane = sender.MasterPane.FindPane(new PointF(e.X, e.Y));

                if (currentPane == sender.MasterPane.PaneList[0])
                {
                    EnablePan(sender);
                    EnableZoom(sender);
                }
                else if (IsDrawScale() && currentPane == sender.MasterPane.PaneList[1])
                {
                    DisablePan(sender);
                    DisableZoom(sender);
                }

                return true;
            }

            return false;
        }

        private void FindNearestContactToMouseClick(PointF mouseClick)
        {
            if (zedGraphChannels.GraphPane.FindNearestObject(mouseClick, CreateGraphics(), out object nearestObject, out int _))
            {
                if (nearestObject is TextObj textObj)
                {
                    ToggleSelectedContact(textObj.Tag as ContactTag);
                }
                else if (nearestObject is BoxObj boxObj)
                {
                    ToggleSelectedContact(boxObj.Tag as ContactTag);
                }
            }
            else
            {
                SetAllSelections(false);
            }
        }

        private bool MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            sender.Cursor = Cursors.Arrow;

            if (e.Button == MouseButtons.Left)
            {
                if (sender.GraphPane.GraphObjList[SelectionAreaTag] is BoxObj selectionArea && selectionArea != null && ProbeGroup != null)
                {
                    RectangleF rect = selectionArea.Location.Rect;

                    sender.GraphPane.GraphObjList.Remove(selectionArea);

                    if (!rect.IsEmpty && (rect.Width > contactSize || rect.Height > contactSize))
                    {
                        var selectedContacts = sender.GraphPane.GraphObjList.OfType<BoxObj>()
                                                                            .Where(c =>
                                                                            {
                                                                                var x = c.Location.X + c.Location.Width / 2;
                                                                                var y = c.Location.Y - c.Location.Height / 2;
                                                                                return c is not PolyObj &&
                                                                                        x >= rect.X &&
                                                                                        x <= rect.X + rect.Width &&
                                                                                        y <= rect.Y &&
                                                                                        y >= rect.Y - rect.Height;
                                                                            });

                        foreach (var contact in selectedContacts)
                        {
                            SetSelectedContact((ContactTag)contact.Tag, true);
                        }
                    }
                    else
                    {
                        FindNearestContactToMouseClick(new PointF(e.X, e.Y));
                    }

                    clickStart.X = default;
                    clickStart.Y = default;
                }
                else
                {
                    FindNearestContactToMouseClick(new PointF(e.X, e.Y));
                }

                HighlightSelectedContacts();
                SelectedContactChanged();
                RefreshZedGraph();

                return true;
            }

            return false;
        }

        private void ToggleSelectedContact(ContactTag tag)
        {
            if (tag == null)
                return;

            SetSelectedContact(tag, !GetContactStatus(tag));
        }

        private int GetContactIndex(ContactTag tag)
        {
            return tag.ProbeIndex == 0
                ? tag.ContactIndex
                : tag.ContactIndex + ProbeGroup.Probes.Take(tag.ProbeIndex).Aggregate(0, (total, next) => total + next.NumberOfContacts);
        }

        private void SetSelectedContact(ContactTag contactTag, bool status)
        {
            var index = GetContactIndex(contactTag);

            SetSelectedContact(index, status);
        }

        private void SetSelectedContact(int index, bool status)
        {
            SelectedContacts[index] = status;
        }

        internal virtual void SelectedContactChanged() { }

        internal void SetAllSelections(bool newStatus)
        {
            for (int i = 0; i < SelectedContacts.Length; i++)
            {
                SetSelectedContact(i, newStatus);
            }
        }

        private bool GetContactStatus(ContactTag tag)
        {
            if (tag == null)
            {
                MessageBox.Show($"Error: Attempted to check status of an object that is not a contact.", "Invalid Object Selected");
            }

            var index = GetContactIndex(tag);

            return SelectedContacts[index];
        }

        private static PointD TransformPixelsToCoordinates(Point pixels, GraphPane graphPane)
        {
            graphPane.ReverseTransform(pixels, out double x, out double y);

            return new PointD(x, y);
        }

        internal static bool HasContactAnnotations(ProbeGroup probeGroup)
        {
            foreach (var probe in probeGroup.Probes)
            {
                if (probe.ContactAnnotations != null
                    && probe.ContactAnnotations.Annotations != null
                    && probe.ContactAnnotations.Annotations.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void ButtonResetZoom_Click(object sender, EventArgs e)
        {
            ResetZoom();

            if (IsDrawScale())
                SyncYAxes(zedGraphChannels.MasterPane.PaneList[0], zedGraphChannels.MasterPane.PaneList[1]);

            RefreshZedGraph();
        }
    }
}
