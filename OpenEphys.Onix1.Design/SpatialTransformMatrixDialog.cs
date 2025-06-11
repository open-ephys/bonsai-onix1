using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Reactive.Linq;
using Bonsai.Design;
using System.Collections.Generic;
using System.Drawing;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a spatial-calibration GUI for <see cref="TS4231V1SpatialTransform.SpatialTransform"/>.
    /// </summary>
    public partial class SpatialTransformMatrixDialog : Form
    {
        internal SpatialTransformProperties SpatialTransform;
        const byte NumMeasurements = 100;
        readonly IObservable<TS4231V1PositionDataFrame> PositionDataSource;
        readonly Vector3[] UserCoordinates = { default, default, default, default };
        readonly Vector3[] TS4231Coordinates = { default, default, default, default };
        Matrix4x4? M;
        IDisposable richTextBoxStatusUpdateSubscription;
        IDisposable MeasurementCalculationSubscription;

        internal SpatialTransformMatrixDialog(IObservable<TS4231V1PositionDataFrame> dataSource, SpatialTransformProperties transformProperties)
        {
            InitializeComponent();
            PositionDataSource = dataSource;
            M = transformProperties.M.GetValueOrDefault();          

            Array.Copy(transformProperties.Pre, TS4231Coordinates, 4);
            var ts4231TextBoxes = new TextBox[] { 
                textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, 
                textBoxTS4231Coordinate2, textBoxTS4231Coordinate3};
            foreach (var (textBox, v) in Enumerable.Zip(ts4231TextBoxes, TS4231Coordinates, (tb, v) => (tb, v)))
                textBox.Text = checkVector3ForNaN(v) ? "" : $"{v.X}, {v.Y}, {v.Z}";

            Array.Copy(transformProperties.Post, UserCoordinates, 4);
            var userTextBoxes = new TextBox[] {
                textBoxUserCoordinate0X, textBoxUserCoordinate0Y, textBoxUserCoordinate0Z,
                textBoxUserCoordinate1X, textBoxUserCoordinate1Y, textBoxUserCoordinate1Z,
                textBoxUserCoordinate2X, textBoxUserCoordinate2Y, textBoxUserCoordinate2Z,
                textBoxUserCoordinate3X, textBoxUserCoordinate3Y, textBoxUserCoordinate3Z};
            for (byte i = 0; i < 12; i++)
            {
                ref var component = ref GetComponent(ref UserCoordinates[i / 3], i % 3);
                userTextBoxes[i].Text = float.IsNaN(component) ? "" : component.ToString();
            }

            CalculatePrintMatrix();
        }

        private void TextBoxUserCoordinate_TextChanged(object sender, EventArgs e)
        {
            var tag = Convert.ToByte(((TextBox)sender).Tag);
            ref var coordinateComponent = ref GetComponent(ref UserCoordinates[tag / 3], tag % 3);
            try { coordinateComponent = float.Parse(((TextBox)sender).Text); }
            catch { coordinateComponent = float.NaN; }
            M = null;
            CalculatePrintMatrix();
        }

        private void ButtonMeasure_Click(object sender, EventArgs e)
        {
            TextBox[] ts4231TextBoxes = { textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var index = Convert.ToByte(((Button)sender).Tag);
            ts4231TextBoxes[index].Text = "";
            TS4231Coordinates[index] = new(float.NaN);
            if (((Button)sender).Text == "Measure")
            {
                richTextBoxStatus.SelectionColor = Color.Blue;
                richTextBoxStatus.AppendText($"Measurement at coordinate {index} initiated.\n");
                M = null;
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
                            TS4231Coordinates[index] = acc.Sum / NumMeasurements;
                            return (Position: TS4231Coordinates[index], Valid: acc.Count == NumMeasurements);
                        })
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(measurement =>
                    {
                        ((Button)sender).Text = "Measure";
                        if (measurement.Valid)
                        {
                            ts4231TextBoxes[index].Text = $"{measurement.Position.X}, {measurement.Position.Y}, {measurement.Position.Z}";
                            CalculatePrintMatrix();
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

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SpatialTransform = new SpatialTransformProperties(TS4231Coordinates, UserCoordinates, M.GetValueOrDefault());
            if (M == null) 
            {
                var confirmationMessage = "";
                var incompleteInput = false;
                if (UserCoordinates.Any(userCoordinate => checkVector3ForNaN(userCoordinate)))
                {
                    incompleteInput = true;
                    var axes = new char[] { 'X', 'Y', 'Z' };
                    var coordinates = new byte[] { 0, 1, 2, 3 };
                    confirmationMessage += "At least one coordinate component is empty or invalid:\n";
                    for (byte i = 0; i < 12; i++)
                    {
                        ref var component = ref GetComponent(ref UserCoordinates[i / 3], i % 3);
                        if (float.IsNaN(component))
                            confirmationMessage += $" • Coordinate {coordinates[i / 3]} {axes[i % 3]} component\n";
                    }
                    confirmationMessage += "\n";
                }
                if (TS4231Coordinates.Any(TS4231Coordinate => checkVector3ForNaN(TS4231Coordinate)))
                {
                    incompleteInput = true;
                    confirmationMessage += "At least one coordinate measurement is empty:\n";
                    foreach (var (i, v) in TS4231Coordinates.Select((i, v) => (v, i)))
                        if (checkVector3ForNaN(v))
                            confirmationMessage += $" • Coordinate {i}\n";
                    confirmationMessage += "\n";
                }

                if (incompleteInput)
                    confirmationMessage += "They will not be saved and position data won't properly output.\n\n";
                else if (!Matrix4x4.Invert(Vector3sToMatrix4x4(UserCoordinates), out _))
                    confirmationMessage = "The spatial transform matrix is non-invertible " +
                        "(i.e. not all three axes are spanned in your coordinate selection or some coordinates are repeated). " +
                        "Position information will be incorrect.\n\n";

                confirmationMessage += "Would you like to continue?";

                if (MessageBox.Show(confirmationMessage, "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    DialogResult = DialogResult.OK;
            }
            else
                DialogResult = DialogResult.OK;
        }   

        private readonly Func<Vector3, bool> checkVector3ForNaN = v => new[] { v.X, v.Y, v.Z }.Any(float.IsNaN);

        private void EnableButtons(bool enable, byte index)
        {
            var buttons = new Button[] { buttonMeasure0, buttonMeasure1, buttonMeasure2, buttonMeasure3, buttonOK, buttonCancel };
            Array.ForEach(buttons, button => button.Enabled = enable || (Convert.ToByte(button.Tag) == index));
        }

        private void CalculatePrintMatrix()
        {
            if (!UserCoordinates.Any(userCoordinate => checkVector3ForNaN(userCoordinate)) &&
            !TS4231Coordinates.Any(TS4231Coordinate => checkVector3ForNaN(TS4231Coordinate)))
            {
                if (Matrix4x4.Invert(Vector3sToMatrix4x4(UserCoordinates), out _))
                {
                    var ts4231V1CoordinatesMatrix = Vector3sToMatrix4x4(TS4231Coordinates);
                    var userCoordinatesMatrix = Vector3sToMatrix4x4(UserCoordinates);
                    Matrix4x4.Invert(ts4231V1CoordinatesMatrix, out var ts4231V1CoordinatesMatrixInverted);
                    M = Matrix4x4.Multiply(ts4231V1CoordinatesMatrixInverted, userCoordinatesMatrix);
                    textBoxSpatialTransformMatrix.Text = M.Value.ToString();
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
                toolStripStatusLabel.Image = Properties.Resources.StatusWarningImage;
                toolStripStatusLabel.Text = "All fields must be properly populated.";
            }
        }

        private static ref float GetComponent(ref Vector3 v, int index)
        {
            switch (index)
            {
                case 0: return ref v.X;
                case 1: return ref v.Y;
                case 2: return ref v.Z;
                default: throw new IndexOutOfRangeException();
            };
        }

        private Matrix4x4 Vector3sToMatrix4x4(IList<Vector3> rows)
        {
            return new Matrix4x4(
                rows[0].X, rows[0].Y, rows[0].Z, 1,
                rows[1].X, rows[1].Y, rows[1].Z, 1,
                rows[2].X, rows[2].Y, rows[2].Z, 1,
                rows[3].X, rows[3].Y, rows[3].Z, 1);
        }
    }
}
