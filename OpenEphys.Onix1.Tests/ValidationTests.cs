using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public class ValidationTests
    {
        const string Message = "The probe serial number (1) does not match the gain calibration file serial number (2).";

        // A check fails with a ValidationException, rather than any other type, so that callers such as the
        // survey can tell it apart from errors that retrying can fix.
        [Theory]
        [InlineData(ValidationLevel.Strict, ValidationLevel.Normal)]
        [InlineData(ValidationLevel.Strict, ValidationLevel.Permissive)]
        [InlineData(ValidationLevel.Normal, ValidationLevel.Permissive)]
        public void Validate_StricterThanRelaxedAt_ThrowsValidationException(ValidationLevel level, ValidationLevel relaxedAt)
        {
            using (ValidationScope.Enter(level))
            {
                var error = Assert.Throws<ValidationException>(() => ContextHelper.Validate(relaxedAt, Message));
                Assert.Equal(Message, error.Message);
            }
        }

        [Theory]
        [InlineData(ValidationLevel.Normal, ValidationLevel.Normal)]
        [InlineData(ValidationLevel.Permissive, ValidationLevel.Normal)]
        [InlineData(ValidationLevel.Permissive, ValidationLevel.Permissive)]
        public void Validate_AtOrAboveRelaxedAt_DoesNotThrow(ValidationLevel level, ValidationLevel relaxedAt)
        {
            using (ValidationScope.Enter(level))
            {
                ContextHelper.Validate(relaxedAt, Message);
            }
        }

        // Outside of any scope, checks are enforced at Normal.
        [Fact]
        public void Validate_OutsideScope_EnforcesNormal()
        {
            Assert.Throws<ValidationException>(() => ContextHelper.Validate(ValidationLevel.Permissive, Message));
            ContextHelper.Validate(ValidationLevel.Normal, Message);
        }
    }
}
