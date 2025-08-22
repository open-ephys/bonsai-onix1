using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a spatial-calibration GUI for <see cref="TS4231V1SpatialTransform.SpatialTransform"/>.
    /// </summary>
    public partial class SpatialTransformMatrixDialog : Form
    {
        internal SpatialTransform3D SpatialTransform;
        const byte NumMeasurements = 100;
        readonly IObservable<TS4231V1PositionDataFrame> PositionDataSource;
        IDisposable richTextBoxStatusUpdateSubscription;
        IDisposable MeasurementCalculationSubscription;

        internal SpatialTransformMatrixDialog(IObservable<TS4231V1PositionDataFrame> dataSource, SpatialTransform3D transformProperties)
        {
            InitializeComponent();

            richTextBoxInstructions.Clear();
            richTextBoxInstructions.BulletIndent = 16;
            richTextBoxInstructions.SelectedText = "The following is a list of bulleted items:\n\n";
            richTextBoxInstructions.SelectionBullet = true;
            richTextBoxInstructions.SelectedText = "Determine a set of 4, well separated XYZ positions in the space in which the headstage will move. These positions should explore a large region of the territory that the headstage will explore and not be confined to a particular plane. Each position defined in this step corresponds to a row in the table below.\n";
            richTextBoxInstructions.SelectedText = "For the first position, place the headstage and click the first measure button on the GUI. After the TS4231 coordinate is obtained from the headstage, enter the known User coordinates in the X, Y, and Z text boxes to provide your spatial mapping. Repeat this process for the second, third, and fourth positions to populate the second, third, and fourth rows of the table.\n";
            richTextBoxInstructions.SelectedText = "Click \"OK\" to close this GUI and set the spatial transform properties in the workflow.\n";
            richTextBoxInstructions.SelectionBullet = false;
            richTextBoxInstructions.SelectedText = "\nFor more in-depth instructions, find the corresponding tutorial in Open Ephys' online documentation.";

            SpatialTransform = transformProperties;
            PositionDataSource = dataSource;

            var ts4231TextBoxes = new TextBox[] {
                textBoxTS4231Coordinate0, textBoxTS4231Coordinate1,
                textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var preTransformCoordinates = SpatialTransform.MatrixToFloatArray(SpatialTransform.A);
            for (byte i = 0; i < 4; i++)
                ts4231TextBoxes[i].Text = float.IsNaN(preTransformCoordinates[i * 3]) ? "" :  $"{preTransformCoordinates[i * 3]}, " +
                                                                                              $"{preTransformCoordinates[i * 3 + 1]}, " +
                                                                                              $"{preTransformCoordinates[i * 3 + 2]}";

            var userTextBoxes = new TextBox[] {
                textBoxUserCoordinate0X, textBoxUserCoordinate0Y, textBoxUserCoordinate0Z,
                textBoxUserCoordinate1X, textBoxUserCoordinate1Y, textBoxUserCoordinate1Z,
                textBoxUserCoordinate2X, textBoxUserCoordinate2Y, textBoxUserCoordinate2Z,
                textBoxUserCoordinate3X, textBoxUserCoordinate3Y, textBoxUserCoordinate3Z };
            var postTransformCoordinates = SpatialTransform.MatrixToFloatArray(SpatialTransform.B);
            foreach (var (tb, comp) in Enumerable.Zip(userTextBoxes, postTransformCoordinates, (tb, comp) => (tb, comp)))
                tb.Text = float.IsNaN(comp) ? "" : comp.ToString();

            IndicateSpatialTransformStatus();
        }

        void TextBoxUserCoordinate_TextChanged(object sender, EventArgs e)
        {
            var tag = Convert.ToByte(((TextBox)sender).Tag);
            try { SpatialTransform.SetMatrixBElement(float.Parse(((TextBox)sender).Text), tag / 3, tag % 3); }
            catch { SpatialTransform.SetMatrixBElement(float.NaN, tag / 3, tag % 3); }
            IndicateSpatialTransformStatus();
        }

        void ButtonMeasure_Click(object sender, EventArgs e)
        {
            TextBox[] ts4231TextBoxes = { textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var index = Convert.ToByte(((Button)sender).Tag);

            for (byte i = 0; i < 3; i++)
                SpatialTransform.SetMatrixAElement(float.NaN, index, i);
            ts4231TextBoxes[index].Text = "";
            
            if (((Button)sender).Text == "Measure")
            {
                richTextBoxStatus.SelectionColor = Color.Blue;
                richTextBoxStatus.AppendText($"Measurement at coordinate {index} initiated.\n");
                IndicateSpatialTransformStatus();
                textBoxSpatialTransformMatrix.Text = "";
                ((Button)sender).Text = "Cancel";
                EnableButtons(false, index);

                var sharedPositionDataGroups = PositionDataSource
                    .Take(NumMeasurements)
                    .Timeout(new TimeSpan(0, 0, 5), Observable.Empty<TS4231V1PositionDataFrame>())
                    .Publish();

                richTextBoxStatusUpdateSubscription = sharedPositionDataGroups
                    .GroupBy(dataFrame => dataFrame.SensorIndex, dataFrame => dataFrame.Position)
                    .SelectMany(group => group.Count().Select(count => new { Index = group.Key, MeasurementCount = count }))
                    .Aggregate(
                        (richTextBoxStatusUpdate: "", Count: 0),
                        (acc, sensor) =>
                        {
                            var richTextBoxStatusUpdateString = $"{acc.richTextBoxStatusUpdate}{sensor.MeasurementCount} samples from sensor {sensor.Index}.\n";
                            return (richTextBoxStatusUpdateString, acc.Count + sensor.MeasurementCount);
                        },
                        acc => (acc.richTextBoxStatusUpdate, Valid: acc.Count == NumMeasurements))
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(finalResult =>
                    {
                        if (finalResult.Valid)
                        {
                            richTextBoxStatus.SelectionColor = Color.Black;
                            richTextBoxStatus.AppendText($"{finalResult.richTextBoxStatusUpdate}Measurement at coordinate {index} complete.\n\n");
                        }
                        else
                        {
                            richTextBoxStatus.SelectionColor = Color.Red;
                            richTextBoxStatus.AppendText($"Measurement at coordinate {index} timed out.\n" +
                                "Confirm the Lighthouse receivers are within range of and unobstructed from Lighthouse transmitters.\n\n");
                        }
                        EnableButtons(true, index);
                    });

                MeasurementCalculationSubscription = sharedPositionDataGroups
                    .Aggregate(
                        (Sum: Vector3.Zero, Count: 0),
                        (acc, current) => (acc.Sum + current.Position, acc.Count + 1),
                        acc =>
                        {
                            var measurement = acc.Sum / NumMeasurements;
                            SpatialTransform.SetMatrixAElement(measurement.X, index, 0);
                            SpatialTransform.SetMatrixAElement(measurement.Y, index, 1);
                            SpatialTransform.SetMatrixAElement(measurement.Z, index, 2);
                            return (Position: measurement, Valid: acc.Count == NumMeasurements);
                        })
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(measurement =>
                    {
                        ((Button)sender).Text = "Measure";
                        if (measurement.Valid)
                        {
                            ts4231TextBoxes[index].Text = $"{measurement.Position.X}, {measurement.Position.Y}, {measurement.Position.Z}";
                            IndicateSpatialTransformStatus();
                        }
                    });

                sharedPositionDataGroups.Connect();
            }
            else
            {
                richTextBoxStatusUpdateSubscription.Dispose();
                MeasurementCalculationSubscription.Dispose();
                richTextBoxStatus.SelectionColor = Color.Red;
                richTextBoxStatus.AppendText($"Measurement at coordinate {index} cancelled by user.\n\n");
                ((Button)sender).Text = "Measure";
                EnableButtons(true, index);
            }
        }

        void ButtonOK_Click(object sender, EventArgs e)
        {
            var confirmationMessage = "";
            var invalidInput = false;
            if (SpatialTransform.ContainsNaN(SpatialTransform.A) || SpatialTransform.ContainsNaN(SpatialTransform.B))
            {
                confirmationMessage = $"At least one entry in the {Name} is invalid for calculating a proper 3D spatial transform:\n";

                var axes = new char[] { 'X', 'Y', 'Z' };
                var coordinates = new byte[] { 0, 1, 2, 3 };

                for (byte i = 0; i < 12; i++)
                    if (float.IsNaN(SpatialTransform.MatrixToFloatArray(SpatialTransform.B)[i]))
                        confirmationMessage += $" • Component {axes[i % 3]} from user coordinate {coordinates[i / 3]}\n";

                for (byte i = 0; i < 4; i++)
                    if (float.IsNaN(SpatialTransform.MatrixToFloatArray(SpatialTransform.A)[i * 3]))
                        confirmationMessage += $" • TS4231 Coordinate {i}\n";

                confirmationMessage += "\nThese invalid entries will not be saved. ";
                invalidInput = true;
            }
            else if (!Matrix4x4.Invert(SpatialTransform.M, out _))
            { 
                confirmationMessage = $"The calculated spatial transform matrix is non-invertible. ";
                invalidInput = true;
            }

            if (invalidInput)
            {
                confirmationMessage += "The transformed position data will be NaNs until these entries are fixed.\n\n" +
                    "Would you like to continue?";
                if (MessageBox.Show(confirmationMessage, "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    DialogResult = DialogResult.OK;
            }
            else
                DialogResult = DialogResult.OK;
        }   

        void EnableButtons(bool enable, byte index)
        {
            var buttons = new Button[] { buttonMeasure0, buttonMeasure1, buttonMeasure2, buttonMeasure3, buttonOK, buttonCancel };
            Array.ForEach(buttons, button => button.Enabled = enable || (Convert.ToByte(button.Tag) == index));
        }

        void IndicateSpatialTransformStatus()
        {
            if (SpatialTransform.ContainsNaN(SpatialTransform.A) || SpatialTransform.ContainsNaN(SpatialTransform.B))
            {
                toolStripStatusLabel.Image = Properties.Resources.StatusWarningImage;
                toolStripStatusLabel.Text = "All fields must be properly populated.";
                textBoxSpatialTransformMatrix.Text = "";
            }
            else if (!Matrix4x4.Invert(SpatialTransform.M, out _))
            {
                toolStripStatusLabel.Image = Properties.Resources.StatusWarningImage;
                toolStripStatusLabel.Text = "The calculated spatial transform matrix must be invertible.";
                textBoxSpatialTransformMatrix.Text = "";
            }
            else 
            {
                toolStripStatusLabel.Image = Properties.Resources.StatusReadyImage;
                toolStripStatusLabel.Text = "Spatial transform matrix is calculated.";
                textBoxSpatialTransformMatrix.Text = SpatialTransform.M.ToString();
            }
        }
        void richTextBoxInstructions_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            ((RichTextBox)sender).Height = e.NewRectangle.Height;
        }
    }
}
