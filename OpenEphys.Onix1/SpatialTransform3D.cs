using System;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Data necessary to construct a spatial transform matrix as well as the
    /// spatial transform matrix itself.
    /// </summary>
    public class SpatialTransform3D
    {

        Matrix4x4 a, b;

        /// <summary>
        /// The A matrix in A * <see cref="M"/> = <see cref="B"/>. It is
        /// constructed from a set of four pre-transform Cartesian coordinates.
        /// </summary>
        public Matrix4x4 A { get => a; set { a = value; M = UpdateM(A, B); } }

        /// <summary>
        /// The B matrix in <see cref="A"/> * <see cref="M"/> = B. It is
        /// constructed from a set of four post-transform Cartesian coordinates.
        /// </summary>
        public Matrix4x4 B { get => b; set { b = value; M = UpdateM(A, B); } }

        /// <summary>
        /// The M matrix in <see cref="A"/> * <see cref="B"/> = M. It is the
        /// spatial transform matrix. It calculated as M = A.inv * B.
        /// </summary>
        [XmlIgnore]
        public Matrix4x4 M { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialTransform3D"/>
        /// class with default values.
        /// </summary>
        public SpatialTransform3D()
        {
            A = B = new(float.NaN, float.NaN, float.NaN, 1,
                        float.NaN, float.NaN, float.NaN, 1,
                        float.NaN, float.NaN, float.NaN, 1,
                        float.NaN, float.NaN, float.NaN, 1);
            M = new(float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialTransform3D"/>
        /// class as a copy of an existing instance.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public SpatialTransform3D(SpatialTransform3D other)
        {
            A = other.A;
            B = other.B;
        }

        static Matrix4x4 UpdateM(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4.Invert(a, out var aInverted);
            var m = Matrix4x4.Multiply(aInverted, b);
            if (Matrix4x4.Invert(m, out _) && !new float[] { m.M11, m.M12, m.M13, m.M14,
                                                             m.M21, m.M22, m.M23, m.M24,
                                                             m.M31, m.M32, m.M33, m.M34,
                                                             m.M41, m.M42, m.M43, m.M44 }.Any(float.IsNaN))
                return m;
            else
                return new(float.NaN, float.NaN, float.NaN, float.NaN,
                           float.NaN, float.NaN, float.NaN, float.NaN,
                           float.NaN, float.NaN, float.NaN, float.NaN,
                           float.NaN, float.NaN, float.NaN, float.NaN);
        }
    }
}
