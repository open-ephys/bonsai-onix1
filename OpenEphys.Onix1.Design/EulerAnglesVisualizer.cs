using System;
using Bonsai;
using Bonsai.Design.Visualizers;
using OpenEphys.Onix1.Design;

[assembly: TypeVisualizer(typeof(EulerAnglesVisualizer), Target = typeof(TaitBryanAngles))]

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a type visualizer that displays a sequence of <see cref="TaitBryanAngles"/>
    /// values as a time series.
    /// </summary>
    public class EulerAnglesVisualizer : TimeSeriesVisualizer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EulerAnglesVisualizer"/> class.
        /// </summary>
        public EulerAnglesVisualizer()
            : base(numSeries: 3)
        {
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var v = (TaitBryanAngles)value;
            AddValue(DateTime.Now, v.Yaw, v.Pitch, v.Roll);
        }
    }
}
