using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows.Forms;
using ZedGraph;
using System;

namespace OpenEphys.Onix.Design
{
    public partial class ChannelConfigurationDialog : Form
    {
        public static readonly string ContactStringFormat = "Contact_{0}";
        public static readonly string TextStringFormat = "TextContact_{0}";

        public ProbeGroup ChannelConfiguration;

        public ChannelConfigurationDialog(ProbeGroup probeGroup)
        {
            InitializeComponent();

            ChannelConfiguration = new(probeGroup);

            if (ChannelConfiguration != null && !ChannelConfiguration.IsValid)
            {
                LoadDefaultChannelLayout();
            }

            InitializeZedGraphChannels();
            DrawChannels(zedGraphChannels, ChannelConfiguration);
        }

        private void LoadDefaultChannelLayout()
        {
            ChannelConfiguration = DeserializeString(Properties.Resources.simple_rhs2116_headstage_probe_interface);
        }

        private void LoadChannelLayout()
        {
            using OpenFileDialog ofd = new();

            ofd.Filter = "Probe Interface Files (*.json)|*.json";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.Title = "Choose probe interface file";

            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                textBoxFilePath.Text = ofd.FileName;

                string channelLayoutString = File.ReadAllText(ofd.FileName);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true,
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                ChannelConfiguration = JsonSerializer.Deserialize<ProbeGroup>(channelLayoutString, options);

                if (ChannelConfiguration == null)
                {
                    MessageBox.Show("Error opening the JSON file.");
                    return;
                }
            }
        }

        public static void DrawChannels(ZedGraphControl zedGraph, ProbeGroup probeGroup)
        {
            if (probeGroup == null)
                return;

            zedGraph.GraphPane.GraphObjList.Clear();

            double minX = 1e3, minY = 1e3, maxX = -1e3, maxY = -1e3;

            for (int i = 0; i < probeGroup.Probes.Length; i++)
            {
                PointD[] planarContours = ConvertFloatArrayToPointD(probeGroup.Probes[i].Probe_Planar_Contour);
                PolyObj contour = new(planarContours, Color.LightGreen, Color.LightGreen)
                {
                    ZOrder = ZOrder.E_BehindCurves
                };

                zedGraph.GraphPane.GraphObjList.Add(contour);

                var tmp = planarContours.Min(p => p.X);
                minX = tmp < minX ? tmp : minX;

                tmp = planarContours.Min(p => p.Y);
                minY = tmp < minY ? tmp : minY;

                tmp = planarContours.Max(p => p.X);
                maxX = tmp > maxX ? tmp : maxX;

                tmp = planarContours.Max(p => p.Y);
                maxY = tmp > maxY ? tmp : maxY;

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

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

            //if (rangeY < rangeX / 2.68)
            //{
            //    minY -= rangeX / (2.68 * 2) - rangeY;
            //    maxY += rangeX / (2.68 * 2) - rangeY;
            //    rangeY = maxY - minY;
            //}

            var margin = Math.Min(rangeX, rangeY) * 0.1;

            zedGraph.GraphPane.XAxis.Scale.Min = minX - margin;
            zedGraph.GraphPane.XAxis.Scale.Max = maxX + margin;
            zedGraph.GraphPane.YAxis.Scale.Min = minY - margin;
            zedGraph.GraphPane.YAxis.Scale.Max = maxY + margin;
            //zedGraph.GraphPane.XAxis.Scale.Min = minX - rangeX * 0.02;
            //zedGraph.GraphPane.XAxis.Scale.Max = maxX + rangeX * 0.02;
            //zedGraph.GraphPane.YAxis.Scale.Min = minY - rangeY * 0.02;
            //zedGraph.GraphPane.YAxis.Scale.Max = maxY + rangeY * 0.02;

            zedGraph.AxisChange();
            zedGraph.Refresh();
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

        private void InitializeZedGraphChannels()
        {
            zedGraphChannels.GraphPane.Title.IsVisible = false;
            zedGraphChannels.GraphPane.TitleGap = 0;
            zedGraphChannels.GraphPane.Border.IsVisible = false;
            zedGraphChannels.GraphPane.Chart.Border.IsVisible = false;
            zedGraphChannels.GraphPane.IsFontsScaled = true;

            zedGraphChannels.GraphPane.XAxis.IsVisible = false;
            zedGraphChannels.GraphPane.YAxis.IsVisible = false;
        }

        private void ButtonNewChannelConfiguration_Click(object sender, System.EventArgs e)
        {
            LoadChannelLayout();
            DrawChannels(zedGraphChannels, ChannelConfiguration);
        }
    }
}
