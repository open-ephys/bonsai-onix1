using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix.Design
{
    public partial class ChannelConfigurationDialog : Form
    {
        public static readonly string ContactStringFormat = "Contact_{0}";
        public static readonly string TextStringFormat = "TextContact_{0}";

        public Rhs2116ProbeGroup ChannelConfiguration;

        public ChannelConfigurationDialog(Rhs2116ProbeGroup probeGroup)
        {
            InitializeComponent();

            if (probeGroup == null)
            {
                LoadDefaultChannelLayout();
            }
            else
            {
                ChannelConfiguration = new(probeGroup);
            }

            InitializeZedGraphChannels(zedGraphChannels);
            DrawChannels(zedGraphChannels, ChannelConfiguration);
        }

        private void LoadDefaultChannelLayout()
        {
            ChannelConfiguration = new();
        }

        private void OpenFile()
        {
            using OpenFileDialog ofd = new();

            ofd.Filter = "Probe Interface Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose probe interface file";

            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                var channelConfiguration = DesignHelper.DeserializeString<Rhs2116ProbeGroup>(File.ReadAllText(ofd.FileName));
                
                if (channelConfiguration == null || channelConfiguration.NumContacts != 32)
                {
                    MessageBox.Show("Error opening the JSON file. Incorrect number of contacts.");
                    return;
                }
                else
                {
                    ChannelConfiguration = channelConfiguration;
                }
            }
        }

        public static void DrawChannels(ZedGraphControl zedGraph, ProbeGroup probeGroup)
        {
            if (probeGroup == null)
                return;

            zedGraph.GraphPane.GraphObjList.Clear();

            for (int i = 0; i < probeGroup.Probes.Length; i++)
            {
                PointD[] planarContours = ConvertFloatArrayToPointD(probeGroup.Probes[i].Probe_Planar_Contour);
                PolyObj contour = new(planarContours, Color.LightGreen, Color.LightGreen)
                {
                    ZOrder = ZOrder.E_BehindCurves
                };

                zedGraph.GraphPane.GraphObjList.Add(contour);

                for (int j = 0; j < probeGroup.Probes[i].Contact_Positions.Length; j++)
                {
                    Contact contact = probeGroup.Probes[i].GetContact(j);

                    if (contact.Shape.Equals("circle"))
                    {
                        EllipseObj contactObj = new(contact.PosX - contact.ShapeParams.Radius, contact.PosY + contact.ShapeParams.Radius,
                            contact.ShapeParams.Radius * 2, contact.ShapeParams.Radius * 2, Color.DarkGray, Color.WhiteSmoke)
                        {
                            ZOrder = ZOrder.B_BehindLegend,
                            Tag = string.Format(ContactStringFormat, contact.ContactId)
                        };


                        zedGraph.GraphPane.GraphObjList.Add(contactObj);

                        TextObj textObj = new(contact.ContactId, contact.PosX, contact.PosY)
                        {
                            ZOrder = ZOrder.A_InFront,
                            Tag = string.Format(TextStringFormat, contact.ContactId)
                        };

                        textObj.FontSpec.Size = 22;
                        textObj.FontSpec.Border.IsVisible = false;
                        textObj.FontSpec.Fill.IsVisible = false;

                        zedGraph.GraphPane.GraphObjList.Add(textObj);
                    }
                    else
                    {
                        MessageBox.Show("Contact shapes other than 'circle' not implemented yet.");
                    }
                }
            }

            ResizeAxes(zedGraph);
          
            zedGraph.Refresh();
        }

        public static void ResizeAxes(ZedGraphControl zedGraph)
        {
            var minX = zedGraph.GraphPane.GraphObjList.Min<GraphObj, double>(obj =>
            {
                if (obj is PolyObj polyObj)
                {
                    return polyObj.Points.Min(p => p.X);
                }

                return double.MaxValue;
            });

            var minY = zedGraph.GraphPane.GraphObjList.Min<GraphObj, double>(obj =>
            {
                if (obj is PolyObj polyObj)
                {
                    return polyObj.Points.Min(p => p.Y);
                }

                return double.MaxValue;
            });

            var maxX = zedGraph.GraphPane.GraphObjList.Max<GraphObj, double>(obj =>
            {
                if (obj is PolyObj polyObj)
                {
                    return polyObj.Points.Max(p => p.X);
                }

                return double.MinValue;
            });

            var maxY = zedGraph.GraphPane.GraphObjList.Max<GraphObj, double>(obj =>
            {
                if (obj is PolyObj polyObj)
                {
                    return polyObj.Points.Max(p => p.Y);
                }

                return double.MinValue;
            });

            var min = Math.Min(minX, minY);
            var max = Math.Max(maxX, maxY);

            var margin = (max - min) * 0.05;

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

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

            zedGraph.GraphPane.XAxis.Scale.Min = minX;
            zedGraph.GraphPane.XAxis.Scale.Max = maxX;

            zedGraph.GraphPane.YAxis.Scale.Min = minY;
            zedGraph.GraphPane.YAxis.Scale.Max = maxY;

            RectangleF axisRect = zedGraph.GraphPane.Rect;

            if (axisRect.Width > axisRect.Height)
            {
                axisRect.X += (axisRect.Width - axisRect.Height) / 2;
                axisRect.Width = axisRect.Height;
            }
            else if (axisRect.Height > axisRect.Width)
            {
                axisRect.Y += (axisRect.Height - axisRect.Width) / 2;
                axisRect.Height = axisRect.Width;
            }

            zedGraph.GraphPane.Rect = axisRect;
        }

        public static PointD[] ConvertFloatArrayToPointD(float[][] floats)
        {
            PointD[] pointD = new PointD[floats.Length];

            for (int i = 0; i < floats.Length; i++)
            {
                pointD[i] = new PointD(floats[i][0], floats[i][1]);
            }

            return pointD;
        }

        public static void InitializeZedGraphChannels(ZedGraphControl zedGraph)
        {
            zedGraph.GraphPane.Title.IsVisible = false;
            zedGraph.GraphPane.TitleGap = 0;
            zedGraph.GraphPane.Border.IsVisible = false;
            zedGraph.GraphPane.Chart.Border.IsVisible = false;
            zedGraph.GraphPane.IsFontsScaled = true;

            zedGraph.IsAutoScrollRange = true;

            zedGraph.GraphPane.XAxis.IsVisible = false;
            zedGraph.GraphPane.XAxis.IsAxisSegmentVisible = false;
            zedGraph.GraphPane.XAxis.Scale.MaxAuto = true;
            zedGraph.GraphPane.XAxis.Scale.MinAuto = true;

            zedGraph.GraphPane.YAxis.IsVisible = false;
            zedGraph.GraphPane.YAxis.IsAxisSegmentVisible = false;
            zedGraph.GraphPane.YAxis.Scale.MaxAuto = true;
            zedGraph.GraphPane.YAxis.Scale.MinAuto = true;
        }

        private void MenuItemSaveFile_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "Probe Interface Files (*.json)|*.json";
            sfd.FilterIndex = 1;
            sfd.Title = "Choose where to save the probe interface file";
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                DesignHelper.SerializeObject(ChannelConfiguration, sfd.FileName);
            }
        }

        private void ZedGraphChannels_Resize(object sender, EventArgs e)
        {
            ResizeAxes(zedGraphChannels);
            zedGraphChannels.Refresh();
        }

        private void MenuItemOpenFile_Click(object sender, EventArgs e)
        {
            OpenFile();
            DrawChannels(zedGraphChannels, ChannelConfiguration);
        }

        private void LoadDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDefaultChannelLayout();
            DrawChannels(zedGraphChannels, ChannelConfiguration);
        }
    }
}
