using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the expected sample rate for a data frame.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ExpectedSampleRateAttribute : Attribute
    {
        /// <summary>
        /// Gets the expected sample rate in hertz.
        /// </summary>
        public double SampleRateHz { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedSampleRateAttribute"/> class with the specified sample rate in hertz.
        /// </summary>
        /// <param name="sampleRateHz">The expected sample rate in hertz.</param>
        public ExpectedSampleRateAttribute(double sampleRateHz)
        {
            SampleRateHz = sampleRateHz;
        }
    }
}
