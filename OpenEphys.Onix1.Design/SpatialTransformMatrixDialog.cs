using System;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Collections.Generic;
using Bonsai.Reactive;
using System.Reflection;

namespace OpenEphys.Onix1.Design
{
    public partial class SpatialTransformMatrixDialog : Form
    {
        private const byte NumMeasurements = 100;

        private bool[] InputsValid = { false, false, false, false, false, false, false, false };

        private IObservable<Tuple<int, Vector3>> PositionDataSource;

        private Vector3[] TS4231Coordinates = { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };

        internal Matrix4x4 SpatialTransform { get; private set; }

        internal bool ApplySpatialTransform { get; private set; }

        internal SpatialTransformMatrixDialog(IObservable<Tuple<int, Vector3>> positionDataSource)
        {
            InitializeComponent();
            PositionDataSource = positionDataSource;
        }

        private void DisableButtons()
        {
            buttonMeasure0.Enabled = false;
            buttonMeasure1.Enabled = false;
            buttonMeasure2.Enabled = false;
            buttonMeasure3.Enabled = false;
            buttonCalculate.Enabled = false;
        }

        private void EnableButtons()
        {
            buttonMeasure0.Invoke((Action)delegate
            {
                buttonMeasure0.Enabled = true;
            });
            buttonMeasure1.Invoke((Action)delegate
            {
                buttonMeasure1.Enabled = true;
            });
            buttonMeasure2.Invoke((Action)delegate
            {
                buttonMeasure2.Enabled = true;
            });
            buttonMeasure3.Invoke((Action)delegate
            {
                buttonMeasure3.Enabled = true;
            });
            buttonCalculate.Invoke((Action)delegate
            {
                buttonCalculate.Enabled = InputsValid.All(inputValid => inputValid);
            });
        }

        private void buttonMeasure_Click(object sender, EventArgs e)
        {
            TextBox[] ts4231TextBoxes = { textBoxTS4231Coordinate0, textBoxTS4231Coordinate1, textBoxTS4231Coordinate2, textBoxTS4231Coordinate3 };
            var index = int.Parse((string)((Button)sender).Tag);

            textBoxStatus.AppendText(string.Format("Measuring coordinate {0}...", index) + Environment.NewLine);

            DisableButtons();

            var sharedPositionDataGroups = PositionDataSource.Take(NumMeasurements)
                .GroupBy(dataFrame => dataFrame.Item1, dataFrame => dataFrame.Item2)
                .Publish();

            sharedPositionDataGroups
                .SelectMany(group => group.Count().Select(count => new { index = group.Key, measurementCount = count }))
                .Finally(() =>
                {
                    textBoxStatus.Invoke((Action)delegate
                    {
                        textBoxStatus.AppendText(string.Format("Measurements at coordinate {0} complete.", index)
                            + Environment.NewLine + Environment.NewLine + "Awaiting user input..." + Environment.NewLine);
                    });
                    EnableButtons();
                })
                .Subscribe(sensor =>
                {
                    textBoxStatus.Invoke((Action)delegate
                    {
                        textBoxStatus.AppendText(string.Format("{1} measurements from sensor {0}.", sensor.index, sensor.measurementCount) + Environment.NewLine);
                    });
                });

            sharedPositionDataGroups
                .Merge()
                .Aggregate(
                    new Vector3(0, 0, 0),
                    (acc, current) => acc + current,
                    acc =>
                    {
                        TS4231Coordinates[index] = acc / NumMeasurements;
                        ts4231TextBoxes[index].Invoke((Action)delegate
                        {
                            ts4231TextBoxes[index].Text = string.Format("{0}, {1}, {2}",
                                TS4231Coordinates[index].X,
                                TS4231Coordinates[index].Y,
                                TS4231Coordinates[index].Z);
                        });
                        return TS4231Coordinates[index];
                    })
                .Subscribe();

            sharedPositionDataGroups.Connect();
         }

        private void textBoxTS4231Coordinate_TextChanged(object sender, EventArgs e)
        {
            var index = int.Parse((string)((TextBox)sender).Tag);
            InputsValid[index] = true;
            buttonCalculate.Enabled = InputsValid.All(inputValid => inputValid);
        }

        private void textBoxUserCoordinate_TextChanged(object sender, EventArgs e)
        {
            var index = int.Parse((string)((TextBox)sender).Tag);
            string[] serInputSplit = ((TextBox)sender).Text.Split(',');
            InputsValid[index] = serInputSplit.Length == 3 ? serInputSplit.All(floatCandidate => float.TryParse(floatCandidate, out _)) : false;
            buttonCalculate.Enabled = InputsValid.All(inputValid => inputValid);
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            var ts4231V1CoordinatesMatrix = new Matrix4x4(
                TS4231Coordinates[0].X, TS4231Coordinates[0].Y, TS4231Coordinates[0].Z, 1,
                TS4231Coordinates[1].X, TS4231Coordinates[1].Y, TS4231Coordinates[1].Y, 1,
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
            SpatialTransform = Matrix4x4.Multiply(ts4231V1CoordinatesMatrixInverted, userCoordinatesMatrix);

            textBoxStatus.AppendText("The spatial transform matrix for the above coordinates is:" + Environment.NewLine);
            textBoxStatus.AppendText(SpatialTransform.ToString() + Environment.NewLine + Environment.NewLine);
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
