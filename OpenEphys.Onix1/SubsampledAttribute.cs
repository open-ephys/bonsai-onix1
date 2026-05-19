using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Marks a <see cref="BufferedDataFrame"/> property as sub-sampled relative to the frame's clock arrays.
    /// </summary>
    /// <remarks>
    /// Apply this attribute when a property contains data that is sampled at a fixed integer fraction of the
    /// rate used by other members in the same frame. For example, if the primary rate is 30 kHz and this
    /// member is sampled at 2.5 kHz, set <see cref="Divisor"/> to 12.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SubsampledAttribute : Attribute
    {
        /// <summary>
        /// The number of primary-rate samples that correspond to each sub-sampled value.
        /// </summary>
        public int Divisor { get; }

        /// <param name="divisor">
        /// Number of primary-rate samples per sub-sampled value. Must be greater than zero.
        /// </param>
        public SubsampledAttribute(int divisor)
        {
            if (divisor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be greater than zero.");

            Divisor = divisor;
        }
    }
}
