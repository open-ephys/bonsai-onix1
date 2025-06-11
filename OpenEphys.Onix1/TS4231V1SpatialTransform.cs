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
        public SpatialTransformProperties SpatialTransform { get; set; } = new();

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
                    Vector3.Transform(input.Position, SpatialTransform.M.GetValueOrDefault())));
        }
    }

    /// <summary>
    /// Data necessary to construct a spatial transform matrix as well as the
    /// spatial transform matrix itself.
    /// </summary>
    public readonly record struct SpatialTransformProperties
    {
        /// <summary>
        /// A set of coodinates before undergoing a spatial transform.
        /// </summary>
        public readonly Vector3[] Pre = { new(float.NaN), new(float.NaN), new(float.NaN), new(float.NaN) };
        /// <summary>
        /// A set of coodinates after undergoing a spatial transform.
        /// </summary>
        public readonly Vector3[] Post = { new(float.NaN), new(float.NaN), new(float.NaN), new(float.NaN) };
        /// <summary>
        /// The spatial transform matrix calculated from <see
        /// cref="SpatialTransformProperties.Pre"/> and <see
        /// cref="SpatialTransformProperties.Post"/>.
        /// </summary>
        public readonly Matrix4x4? M = null;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SpatialTransformProperties"/> struct with default values.
        /// </summary>
        public SpatialTransformProperties() { }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SpatialTransformProperties"/> with values specified using
        /// parameters.
        /// </summary>
        /// <param name="pre">
        /// The value used to set <see cref="SpatialTransformProperties.Pre"/>.
        /// </param>
        /// <param name="post">
        /// The value used to set <see cref="SpatialTransformProperties.Post"/>.
        /// </param>
        /// <param name="m">
        /// The value used to set <see cref="SpatialTransformProperties.M"/>.
        /// </param>
        public SpatialTransformProperties(Vector3[] pre, Vector3[] post, Matrix4x4 m)
        {
            Array.Copy(pre, Pre, 4);
            Array.Copy(post, Post, 4);
            M = m;
        }
    }
}
