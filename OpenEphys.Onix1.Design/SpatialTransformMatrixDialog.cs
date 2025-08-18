using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Reactive.Linq;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a spatial-calibration GUI for <see cref="TS4231V1PositionData.M"/>.
    /// </summary>
    public partial class SpatialTransformMatrixDialog : Form
    {
        const byte NumMeasurements = 100;
        readonly Matrix4x4 inverseM;

        readonly bool[] InputsValid = { false, false, false, false, false, false, false, false };
        readonly IObservable<TS4231V1PositionDataFrame> PositionDataSource;
        IDisposable richTextBoxStatusUpdateSubscription;
        IDisposable MeasurementCalculationSubscription;

        internal Matrix4x4 NewSpatialTransform { get; private set; }

        internal SpatialTransformMatrixDialog(IObservable<TS4231V1PositionDataFrame> positionDataSource, Matrix4x4 currentM)
        {
            InitializeComponent();
            SpatialTransform = transformProperties;
            PositionDataSource = dataSource;

            var ts4231TextBoxes = new TextBox[] { 
                textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, 
                textBoxTS4231Coordinate2, textBoxTS4231Coordinate3};
            foreach (var (textBox, v) in Enumerable.Zip(ts4231TextBoxes, SpatialTransform.Pre, (tb, v) => (tb, v)))
                textBox.Text = checkVector3ForNaN(v) ? "" : $"{v.X}, {v.Y}, {v.Z}";

            var userTextBoxes = new TextBox[] {
                textBoxUserCoordinate0X, textBoxUserCoordinate0Y, textBoxUserCoordinate0Z,
                textBoxUserCoordinate1X, textBoxUserCoordinate1Y, textBoxUserCoordinate1Z,
                textBoxUserCoordinate2X, textBoxUserCoordinate2Y, textBoxUserCoordinate2Z,
                textBoxUserCoordinate3X, textBoxUserCoordinate3Y, textBoxUserCoordinate3Z};
            for (byte i = 0; i < 12; i++)
            {
                ref var component = ref GetComponent(ref SpatialTransform.Post[i / 3], i % 3);
                userTextBoxes[i].Text = float.IsNaN(component) ? "" : component.ToString();
            }

            CalculatePrintMatrix();
        }

        private void TextBoxUserCoordinate_TextChanged(object sender, EventArgs e)
        {
            var tag = Convert.ToByte(((TextBox)sender).Tag);
            ref var coordinateComponent = ref GetComponent(ref SpatialTransform.Post[tag / 3], tag % 3);
            try { coordinateComponent = float.Parse(((TextBox)sender).Text); }
            catch { coordinateComponent = float.NaN; }
            CalculatePrintMatrix();
        }

        private void ButtonMeasure_Click(object sender, EventArgs e)
        {
            TextBox[] ts4231TextBoxes = { textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var index = Convert.ToByte(((Button)sender).Tag);
            ts4231TextBoxes[index].Text = "";
            SpatialTransform.Pre[index] = new(float.NaN);
            if (((Button)sender).Text == "Measure")
            {
                richTextBoxStatus.SelectionColor = Color.Blue;
                richTextBoxStatus.AppendText($"Measurement at coordinate {index} initiated.\n");
                SpatialTransform.M = null;
                textBoxSpatialTransformMatrix.Text = "";
                ((Button)sender).Text = "Cancel";
                EnableButtons(false, index);

                var sharedPositionDataGroups = PositionDataSource
                    .Take(NumMeasurements)
                    .Timeout(new TimeSpan(0, 0, 5), Observable.Empty<TS4231V1PositionDataFrame>())
                    .Publish();

                TextBoxStatusUpdateSubscription = sharedPositionDataGroups
                    .GroupBy(dataFrame => dataFrame.SensorIndex, dataFrame => dataFrame.Position)
                    .SelectMany(group => group.Count().Select(count => new { Index = group.Key, MeasurementCount = count }))
                    .Aggregate(
                        (TextBoxStatusUpdate: "", Count: 0),
                        (acc, sensor) =>
                        {
                            var textBoxStatusUpdateString = acc.TextBoxStatusUpdate;
                            textBoxStatusUpdateString += string.Format("{0} measurements from sensor {1}.",
                                sensor.MeasurementCount, sensor.Index);
                            textBoxStatusUpdateString += Environment.NewLine;
                            return (textBoxStatusUpdateString, acc.Count + sensor.MeasurementCount);
                        },
                        acc => (acc.TextBoxStatusUpdate, Valid: acc.Count == NumMeasurements))
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(finalResult =>
                    {
                        if (finalResult.Valid)
                        {
                            textBoxStatus.AppendText(finalResult.TextBoxStatusUpdate);
                            textBoxStatus.AppendText(string.Format("Measurements at coordinate {0} complete.", index)
                                + Environment.NewLine + Environment.NewLine + "Awaiting user input..." + Environment.NewLine);
                        }
                        else
                        {
                            textBoxStatus.AppendText(string.Format("Measurements at coordinate {0} timed out. ", index)
                                + "Confirm the Lighthouse receivers are within range and unobstructed from Lighthouse transmitters."
                                + Environment.NewLine + Environment.NewLine + "Awaiting user input..." + Environment.NewLine);
                        }
                        EnableButtons(true, index);
                    });

                MeasurementCalculationSubscription = sharedPositionDataGroups
                    .Aggregate(
                        (Sum: Vector3.Zero, Count: 0),
                        (acc, current) => (acc.Sum + current.Position, acc.Count + 1),
                        acc =>
                        {
                            SpatialTransform.Pre[index] = acc.Sum / NumMeasurements;
                            return (Position: SpatialTransform.Pre[index], Valid: acc.Count == NumMeasurements);
                        })
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(finalMeasurement =>
                    {
                        MeasureButtons[index].Text = "Measure";
                        if (finalMeasurement.Valid)
                        {
                            ts4231TextBoxes[index].Text = string.Format("{0}, {1}, {2}",
                                finalMeasurement.Position.X,
                                finalMeasurement.Position.Y,
                                finalMeasurement.Position.Z);
                            InputsValid[index] = true;
                            if (InputsValid.Take(4).All(ts4231InputValid => ts4231InputValid))
                            {
                                toolStripStatusLabelTS4231.Image = OpenEphys.Onix1.Design.Properties.Resources.StatusReadyImage;
                                toolStripStatusLabelTS4231.Text = "All TS4231 coordinates are valid.";
                            }
                            else
                            {
                                toolStripStatusLabelTS4231.Image = OpenEphys.Onix1.Design.Properties.Resources.StatusBlockedImage;
                                toolStripStatusLabelTS4231.Text = "At least one TS4231 coordinate is invalid.";
                            }
                            buttonCalculate.Enabled = InputsValid.All(inputValid => inputValid);
                        }
                    });

                sharedPositionDataGroups.Connect();
            }
            else
            {
                TextBoxStatusUpdateSubscription.Dispose();
                MeasurementCalculationSubscription.Dispose();
                textBoxStatus.AppendText(string.Format("Measurements at coordinate {0} cancelled by user.", index)
                    + Environment.NewLine + Environment.NewLine + "Awaiting user input..." + Environment.NewLine);
                MeasureButtons[index].Text = "Measure";
                EnableButtons(true, index);
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (SpatialTransform.M.HasValue)
                DialogResult = DialogResult.OK;
            else
            {
                var confirmationMessage = "";
                var incompleteInput = false;
                if (SpatialTransform.Post.Any(userCoordinate => checkVector3ForNaN(userCoordinate)))
                {
                    incompleteInput = true;
                    var axes = new char[] { 'X', 'Y', 'Z' };
                    var coordinates = new byte[] { 0, 1, 2, 3 };
                    confirmationMessage += "At least one coordinate component is empty or invalid:\n";
                    for (byte i = 0; i < 12; i++)
                    {
                        ref var component = ref GetComponent(ref SpatialTransform.Post[i / 3], i % 3);
                        if (float.IsNaN(component))
                            confirmationMessage += $" • Coordinate {coordinates[i / 3]} {axes[i % 3]} component\n";
                    }
                    confirmationMessage += "\n";
                }
                if (SpatialTransform.Pre.Any(TS4231Coordinate => checkVector3ForNaN(TS4231Coordinate)))
                {
                    incompleteInput = true;
                    confirmationMessage += "At least one coordinate measurement is empty:\n";
                    foreach (var (i, v) in SpatialTransform.Pre.Select((i, v) => (v, i)))
                        if (checkVector3ForNaN(v))
                            confirmationMessage += $" • Coordinate {i}\n";
                    confirmationMessage += "\n";
                }

                if (incompleteInput)
                    confirmationMessage += "They will not be saved and transformed position data won't be properly output.\n\n";
                else if (!Matrix4x4.Invert(Vector3sToMatrix4x4(SpatialTransform.Post), out _))
                    confirmationMessage = "The spatial transform matrix is non-invertible. The transformed position data won't be properly output.\n\n";

                confirmationMessage += "Would you like to continue?";

                if (MessageBox.Show(confirmationMessage, "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    DialogResult = DialogResult.OK;
            }                
        }   

        private readonly Func<Vector3, bool> checkVector3ForNaN = v => new[] { v.X, v.Y, v.Z }.Any(float.IsNaN);

        private void EnableButtons(bool enable, byte index)
        {
            var buttons = new Button[] { buttonMeasure0, buttonMeasure1, buttonMeasure2, buttonMeasure3, buttonOK, buttonCancel };
            Array.ForEach(buttons, button => button.Enabled = enable || (Convert.ToByte(button.Tag) == index));
        }

        private void CalculatePrintMatrix()
        {
            SpatialTransform.M = null;
            if (!SpatialTransform.Post.Any(userCoordinate => checkVector3ForNaN(userCoordinate)) &&
            !SpatialTransform.Pre.Any(TS4231Coordinate => checkVector3ForNaN(TS4231Coordinate)))
            {
                if (Matrix4x4.Invert(Vector3sToMatrix4x4(SpatialTransform.Post), out _))
                {
                    var ts4231V1CoordinatesMatrix = Vector3sToMatrix4x4(SpatialTransform.Pre);
                    var userCoordinatesMatrix = Vector3sToMatrix4x4(SpatialTransform.Post);
                    Matrix4x4.Invert(ts4231V1CoordinatesMatrix, out var ts4231V1CoordinatesMatrixInverted);
                    SpatialTransform.M = Matrix4x4.Multiply(ts4231V1CoordinatesMatrixInverted, userCoordinatesMatrix);
                    toolStripStatusLabel.Image = Properties.Resources.StatusReadyImage;
                    toolStripStatusLabel.Text = "Spatial transform matrix is calculated.";
                }
                else
                {
                    toolStripStatusLabel.Image = Properties.Resources.StatusWarningImage;
                    toolStripStatusLabel.Text = "The resulting spatial transform matrix must be non-invertible.";
                }
            }
            else
            {
                toolStripStatusLabelUser.Image = OpenEphys.Onix1.Design.Properties.Resources.StatusBlockedImage;
                toolStripStatusLabelUser.Text = "At least one user-defined coordinate is invalid.";
            }
            if (SpatialTransform.M.HasValue)
                textBoxSpatialTransformMatrix.Text = SpatialTransform.M.Value.ToString();  
            else
                textBoxSpatialTransformMatrix.Text = "";
        }

        private void ButtonCalculate_Click(object sender, EventArgs e)
        {
            var ts4231V1CoordinatesMatrix = new Matrix4x4(
                TS4231Coordinates[0].X, TS4231Coordinates[0].Y, TS4231Coordinates[0].Z, 1,
                TS4231Coordinates[1].X, TS4231Coordinates[1].Y, TS4231Coordinates[1].Z, 1,
                TS4231Coordinates[2].X, TS4231Coordinates[2].Y, TS4231Coordinates[2].Z, 1,
                TS4231Coordinates[3].X, TS4231Coordinates[3].Y, TS4231Coordinates[3].Z, 1);

            float[][] userCoordinates = {
                textBoxUserCoordinate0.Text.Split(',').Select(item => float.Parse(item)).ToArray(),
                textBoxUserCoordinate1.Text.Split(',').Select(item => float.Parse(item)).ToArray(),
                textBoxUserCoordinate2.Text.Split(',').Select(item => float.Parse(item)).ToArray(),
                textBoxUserCoordinate3.Text.Split(',').Select(item => float.Parse(item)).ToArray()};

            var userCoordinatesMatrix = new Matrix4x4(
                userCoordinates[0][0], userCoordinates[0][1], userCoordinates[0][2], 1,
                userCoordinates[1][0], userCoordinates[1][1], userCoordinates[1][2], 1,
                userCoordinates[2][0], userCoordinates[2][1], userCoordinates[2][2], 1,
                userCoordinates[3][0], userCoordinates[3][1], userCoordinates[3][2], 1);

            Matrix4x4.Invert(ts4231V1CoordinatesMatrix, out var ts4231V1CoordinatesMatrixInverted);
            NewSpatialTransform = Matrix4x4.Multiply(ts4231V1CoordinatesMatrixInverted, userCoordinatesMatrix);

            textBoxStatus.AppendText("The spatial transform matrix for the above coordinates is:" + Environment.NewLine);
            textBoxStatus.AppendText(NewSpatialTransform.ToString() + Environment.NewLine + Environment.NewLine);
            textBoxStatus.AppendText("Awaiting user input..." + Environment.NewLine);

            buttonOK.Enabled = true;
        }

        private void ButtonOKOrCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
