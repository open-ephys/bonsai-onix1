using Bonsai;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Numerics;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents an operator that groups the elements of an observable
    /// sequence according to the specified key.
    /// </summary>
    [DefaultProperty(nameof(SpatialTransformMatrix))]
    [Description("Transforms 3D coordinates from one reference frame to another.")]
    public class SpatialTransform : Transform<Tuple<int, Vector3>, Vector3>
    {
        /// <summary>
        /// Gets or sets a value specifying the inner properties used as key for
        /// each element in the sequence.
        /// </summary>
        [Description("Spatial Transform Matrix")]
        [Editor("OpenEphys.Onix1.Design.SpatialTransformMatrixEditor, OpenEphys.Onix1.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(NumericRecordConverter))]
        public Matrix4x4 SpatialTransformMatrix { get; set; } = new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        public override IObservable<Vector3> Process(IObservable<Tuple<int, Vector3>> source)
        {
            return source.Select(input => Vector3.Transform(input.Item2, this.SpatialTransformMatrix));
        }
    }
}
