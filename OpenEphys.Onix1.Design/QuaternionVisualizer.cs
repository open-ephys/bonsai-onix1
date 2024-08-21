using System;
using Bonsai;
using Bonsai.Design.Visualizers;
using System.Numerics;
using OpenEphys.Onix1.Design;

[assembly: TypeVisualizer(typeof(QuaternionVisualizer), Target = typeof(Quaternion))]

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a type visualizer that displays a sequence of <see cref="Quaternion"/>
    /// values as a time series.
    /// </summary>
    public class QuaternionVisualizer : TimeSeriesVisualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionVisualizer"/> class.
        /// </summary>
        public QuaternionVisualizer()
            : base(numSeries: 4)
        {
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var q = (Quaternion)value;
            AddValue(DateTime.Now, q.X, q.Y, q.Z, q.W);
        }
    }
}
