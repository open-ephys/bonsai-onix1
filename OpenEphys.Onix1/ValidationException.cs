using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// The exception that is thrown when hardware configuration fails a validation check, such as a
    /// calibration file with an invalid format or one whose serial number does not match the probe.
    /// </summary>
    /// <remarks>
    /// A validation check tests a condition that must hold for the hardware to work as configured. If it
    /// fails, configuring or acquiring from the hardware is known in advance to fail or to produce incorrect
    /// or undefined data, so it is not attempted. Unlike a communication error, a validation failure happens again each
    /// time the same configuration is applied, so retrying it does not help. Some validation checks can be
    /// relaxed to warnings with <see cref="CreateContext.ValidationLevel"/>.
    /// </remarks>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a message that
        /// describes the failed check.
        /// </summary>
        /// <param name="message">The message that describes the failed check.</param>
        public ValidationException(string message)
            : base(message)
        {
        }
    }
}
