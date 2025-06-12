using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Data necessary to construct a spatial transform matrix as well as the
    /// spatial transform matrix itself.
    /// </summary>
    public class SpatialTransformProperties
    {
        /// <summary>
        /// The set of coordinates before undergoing a spatial transform.
        /// </summary>
        public Vector3[] Pre { get; set; }

        /// <summary>
        /// The set of coordinates after undergoing a spatial transform.
        /// </summary>
        public Vector3[] Post { get; set; }

        /// <summary>
        /// The spatial transform matrix calculated from <see cref="Pre"/> and
        /// <see cref="Post"/>.
        /// </summary>
        public Matrix4x4? M { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SpatialTransformProperties"/> class with default values.
        /// </summary>
        public SpatialTransformProperties()
        {
            Pre = new Vector3[] { new(float.NaN), new(float.NaN), new(float.NaN), new(float.NaN) };
            Post = new Vector3[] { new(float.NaN), new(float.NaN), new(float.NaN), new(float.NaN) };
            M = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SpatialTransformProperties"/> class as a copy of an existing
        /// instance.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public SpatialTransformProperties(SpatialTransformProperties other)
        {
            Pre = new Vector3[4];
            Post = new Vector3[4];
            Array.Copy(other.Pre, Pre, 4);
            Array.Copy(other.Post, Post, 4);
            M = other.M;
        }
    }
}
