using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Reactive.Linq;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    public partial class SpatialTransformMatrixDialog : Form
    {
        const byte NumMeasurements = 100;
        readonly Matrix4x4 inverseM;

        readonly bool[] InputsValid = { false, false, false, false, false, false, false, false };
        readonly IObservable<TS4231V1PositionDataFrame> PositionDataSource;
        readonly Vector3[] TS4231Coordinates = { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
        readonly Button[] MeasureButtons;

        IDisposable TextBoxStatusUpdateSubscription;
        IDisposable MeasurementCalculationSubscription;

        internal Matrix4x4 NewSpatialTransform { get; private set; }

        internal bool ApplySpatialTransform { get; private set; }

        internal SpatialTransformMatrixDialog(IObservable<TS4231V1PositionDataFrame> positionDataSource, Matrix4x4 currentM)
        {
            InitializeComponent();
            if (!Matrix4x4.Invert(currentM, out inverseM)) 
            {
                throw new ArgumentException("Current spatial transform matrix is non-invertible. " +
                    "You can set M to the identity matrix if you want to start anew.");
            }
            PositionDataSource = positionDataSource;
            MeasureButtons = new Button[] { buttonMeasure0, buttonMeasure1, buttonMeasure2, buttonMeasure3 };
        }

        private void enableButtons(bool enable, byte index)
        {
            for (byte i = 0; i < MeasureButtons.Length; i++)
            {
                MeasureButtons[i].Enabled = enable || (i == index);
            }
            buttonCalculate.Enabled = enable && InputsValid.All(inputValid => inputValid);
        }

        private void buttonMeasure_Click(object sender, EventArgs e)
        {
            TextBox[] ts4231TextBoxes = { textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var index = byte.Parse((string)((Button)sender).Tag);

            if (MeasureButtons[index].Text == "Measure")
            {
                textBoxStatus.AppendText(string.Format("Measuring coordinate {0}...", index) + Environment.NewLine);
                MeasureButtons[index].Text = "Cancel";
                enableButtons(false, index);

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
                        enableButtons(true, index);
                    });

                MeasurementCalculationSubscription = sharedPositionDataGroups
                    .Aggregate(
                        (Sum: Vector3.Zero, Count: 0),
                        (acc, current) => (acc.Sum + Vector3.Transform(current.Position, inverseM), acc.Count + 1),
                        acc =>
                        {
                            TS4231Coordinates[index] = acc.Sum / NumMeasurements;
                            return (Position: TS4231Coordinates[index], Valid: acc.Count == NumMeasurements);
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
                enableButtons(true, index);
            }
        }

        private void textBoxUserCoordinate_TextChanged(object sender, EventArgs e)
        {
            var index = int.Parse((string)((TextBox)sender).Tag);
            string[] serInputSplit = ((TextBox)sender).Text.Split(',');
            InputsValid[index] = serInputSplit.Length == 3 && serInputSplit.All(floatCandidate => float.TryParse(floatCandidate, out _));
            buttonCalculate.Enabled = InputsValid.All(inputValid => inputValid);
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
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

            checkBoxApplySpatialTransform.Enabled = true;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBoxApplySpatialTransform_CheckedChanged(object sender, EventArgs e)
        {
            ApplySpatialTransform = checkBoxApplySpatialTransform.Checked;
        }
    }
}
