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

        private Matrix4x4 _a, _b;

        /// <summary>
        /// The A matrix in A * <see cref="M"/> = <see cref="B"/>. It is
        /// constructed from a set of four Cartesian coordinates before
        /// undergoing a spatial transformation.
        /// </summary>
        public Matrix4x4 A { get => _a; set { _a = value; UpdateM(); } }

        /// <summary>
        /// The B matrix in <see cref="A"/> * <see cref="M"/> = B. It is
        /// constructed from a set of four Cartesian coordinates after
        /// undergoing a spatial transformation.
        /// </summary>
        public Matrix4x4 B { get => _b ; set { _b = value; UpdateM(); } }

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

        /// <summary>
        /// Sets a component (X, Y, or Z) in one of the coordinates in
        /// PreTransformCoordinates.
        /// </summary>
        public void SetMatrixAElement(float value, int coordinate, int component) =>
            SetMatrixElement(ref _a, value, coordinate, component);

        /// <summary>
        /// Sets a component (X, Y, or Z) in one of the coordinates in
        /// PostTransformCoordinates.
        /// </summary>
        public void SetMatrixBElement(float value, int coordinate, int component) =>
            SetMatrixElement(ref _b, value, coordinate, component);

        private void SetMatrixElement(ref Matrix4x4 m, float value, int coordinate, int component)
        {
            if (coordinate is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(coordinate) + " must be 0, 1, 2, or 3.");
            if (component is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(component) + " must be 0, 1, or 2.");

            switch ((coordinate, component))
            {
                case (0, 0): m.M11 = value; break; case (0, 1): m.M12 = value; break; case (0, 2): m.M13 = value; break;
                case (1, 0): m.M21 = value; break; case (1, 1): m.M22 = value; break; case (1, 2): m.M23 = value; break;
                case (2, 0): m.M31 = value; break; case (2, 1): m.M32 = value; break; case (2, 2): m.M33 = value; break;
                case (3, 0): m.M41 = value; break; case (3, 1): m.M42 = value; break; case (3, 2): m.M43 = value; break;
            }
            UpdateM();
        }

        private void UpdateM()
        {
            
            Matrix4x4.Invert(A, out var AInverted);
            var m = Matrix4x4.Multiply(AInverted, B);
            M = !ContainsNaN(m) && Matrix4x4.Invert(m, out _) ? m : 
                new(float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN);
        }

        /// <summary>
        /// Convert coordinates from matrix to a float array.
        /// </summary>
        public float[] MatrixToFloatArray(Matrix4x4 m) => 
            new float[] { m.M11, m.M12, m.M13,
                          m.M21, m.M22, m.M23,
                          m.M31, m.M32, m.M33,
                          m.M41, m.M42, m.M43 };

        /// <summary>
        /// Checks if matrix contains one or more NaNs.
        /// </summary>
        public bool ContainsNaN(Matrix4x4 m) => MatrixToFloatArray(m).Any(float.IsNaN);
    }
}
