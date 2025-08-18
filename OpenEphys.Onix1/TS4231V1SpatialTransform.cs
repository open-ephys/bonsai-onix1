using System;
using System.ComponentModel;
using System.Numerics;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Transforms a sequence of 3D positions from <see cref="TS4231V1DataFrame"/> to an external coordinate system.
    /// </summary>
    [DefaultProperty(nameof(SpatialTransform))]
    public class TS4231V1SpatialTransform : Transform<TS4231V1PositionDataFrame, TS4231V1PositionDataFrame>
    {
        /// <summary>
        /// Gets or sets the pre- and post- transform coordinates to calculate
        /// the spatial transform matrix as well as the spatial transform matrix
        /// itself.
        /// </summary>
        [Editor("OpenEphys.Onix1.Design.SpatialTransformMatrixEditor, OpenEphys.Onix1.Design", DesignTypes.UITypeEditor)]
        [Description("Data for transforming position measurements to another reference frame.")]
        public SpatialTransform3D SpatialTransform { get; set; } = new();

        /// <summary>
        /// Transforms a sequence of <see cref="TS4231V1PositionDataFrame"/>
        /// objects, each of which contains transformed 3D position of single
        /// photodiode.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="TS4231V1PositionDataFrame"/> objects with
        /// transformed position data.
        /// </returns>
        public override IObservable<TS4231V1PositionDataFrame> Process(IObservable<TS4231V1PositionDataFrame> source)
        {
            return source.Select(input =>
                new TS4231V1PositionDataFrame(input.Clock, input.HubClock, input.SensorIndex, 
                    Vector3.Transform(input.Position, SpatialTransform.M)));
        }
    }
}
