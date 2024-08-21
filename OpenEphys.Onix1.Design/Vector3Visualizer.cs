using System;
using Bonsai;
using Bonsai.Design.Visualizers;
using System.Numerics;
using OpenEphys.Onix1.Design;

[assembly: TypeVisualizer(typeof(Vector3Visualizer), Target = typeof(Vector3))]

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a type visualizer that displays a sequence of <see cref="Vector3"/>
    /// values as a time series.
    /// </summary>
    public class Vector3Visualizer : TimeSeriesVisualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Visualizer"/> class.
        /// </summary>
        public Vector3Visualizer()
            : base(numSeries: 3)
        {
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var v = (Vector3)value;
            AddValue(DateTime.Now, v.X, v.Y, v.Z);
        }
    }
}
