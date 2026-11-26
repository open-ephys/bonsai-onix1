using System;
using System.ComponentModel;
using System.Globalization;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public class PortVoltageConverterTests
    {
        static readonly PortVoltageConverter Converter = new();

        // NB: The property grid parses what the user types with ConvertFromString, which uses the current culture
        static double? ReadAsTyped(string text) => ((AutoPortVoltage)Converter.ConvertFromString(text)).Requested;

        // Bonsai sets workflow properties given on the command line (-p:Name=Value) with InvariantCulture, so
        // "5.5" must mean 5.5 V on any computer
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void ConvertFrom_InvariantCulture_ReadsDecimalPoint(string culture)
        {
            using var scope = new CultureScope(culture);
            var voltage = (AutoPortVoltage)Converter.ConvertFrom(null, CultureInfo.InvariantCulture, "5.5");
            Assert.Equal(5.5, voltage.Requested);
        }

        // On the command line, "5,5" used to be read as 55 V (',' is the invariant group separator)
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void ConvertFrom_InvariantCulture_RejectsComma(string culture)
        {
            using var scope = new CultureScope(culture);
            Assert.Throws<FormatException>(() => Converter.ConvertFrom(null, CultureInfo.InvariantCulture, "5,5"));
        }

        // The converter must format with the culture it is given, so that converting a value to a string and back
        // with the same culture returns it. Formatting with the current culture instead turned 5.5 into "5,5",
        // which InvariantCulture then read as 55
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void InvariantStringRoundTrip_AnyCulture_KeepsValue(string culture)
        {
            using var scope = new CultureScope(culture);
            var text = Converter.ConvertToInvariantString(new AutoPortVoltage(5.5));
            Assert.Equal("5.5", text);
            Assert.Equal(5.5, ((AutoPortVoltage)Converter.ConvertFromInvariantString(text)).Requested);
        }

        // The property grid converts with the current culture both ways: this already worked and must keep working
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void CurrentCultureStringRoundTrip_AnyCulture_KeepsValue(string culture)
        {
            using var scope = new CultureScope(culture);
            var text = Converter.ConvertToString(null, CultureInfo.CurrentCulture, new AutoPortVoltage(5.5));
            Assert.Equal(5.5, ((AutoPortVoltage)Converter.ConvertFromString(null, CultureInfo.CurrentCulture, text)).Requested);
        }

        // The automatically applied voltage is shown with an "(Auto)" suffix, in the requested culture
        [Fact]
        public void ConvertTo_AppliedVoltage_UsesGivenCulture()
        {
            var voltage = new AutoPortVoltage(null) { Applied = 5.5 };
            using var scope = new CultureScope("es-ES");
            Assert.Equal("5.5 (Auto)", Converter.ConvertTo(null, CultureInfo.InvariantCulture, voltage, typeof(string)));
            Assert.Equal("5,5 (Auto)", Converter.ConvertTo(null, CultureInfo.GetCultureInfo("es-ES"), voltage, typeof(string)));
        }

        // A voltage that was never requested nor applied is shown empty
        [Fact]
        public void ConvertTo_NoVoltage_IsEmpty()
        {
            Assert.Equal("", Converter.ConvertToString(new AutoPortVoltage(null)));
        }

        // Typing a voltage with the computer's own decimal separator
        [Theory]
        [InlineData("en-US", "5.5", 5.5)]
        [InlineData("es-ES", "5,5", 5.5)]
        [InlineData("de-DE", "5,5", 5.5)]
        [InlineData("fr-FR", "5,5", 5.5)]
        [InlineData("en-US", "5", 5.0)]
        [InlineData("es-ES", "5", 5.0)]
        [InlineData("es-ES", " 5,5 ", 5.5)]
        [InlineData("es-ES", ",5", 0.5)]
        public void ConvertFrom_OwnDecimalSeparator_ReadsValue(string culture, string text, double expected)
        {
            using var scope = new CultureScope(culture);
            Assert.Equal(expected, ReadAsTyped(text));
        }

        // Typing the decimal separator of another culture used to be read as a group separator, giving a voltage
        // ten times larger ("5.5" became 55 V in es-ES and de-DE, "5,5" became 55 V in en-US). It must be rejected
        [Theory]
        [InlineData("en-US", "5,5")]
        [InlineData("es-ES", "5.5")]
        [InlineData("de-DE", "5.5")]
        [InlineData("fr-FR", "5.5")]
        [InlineData("en-US", "1,000")]
        [InlineData("es-ES", "1.000")]
        public void ConvertFrom_OtherDecimalSeparator_IsRejected(string culture, string text)
        {
            using var scope = new CultureScope(culture);
            Assert.Throws<FormatException>(() => ReadAsTyped(text));
        }

        // Windows users can change the separators of their culture in the regional settings
        [Fact]
        public void ConvertFrom_CustomDecimalSeparator_ReadsOwnAndRejectsOther()
        {
            using var scope = new CultureScope(CultureScope.CustomSeparators());
            Assert.Equal(5.5, ReadAsTyped("5,5"));
            Assert.Throws<FormatException>(() => ReadAsTyped("5.5"));
        }

        // Clearing the value selects the automatic voltage
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void ConvertFrom_Empty_IsAutomatic(string culture)
        {
            using var scope = new CultureScope(culture);
            Assert.Null(ReadAsTyped(""));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("5 V")]
        [InlineData(" ")]
        [InlineData("5,5 (Auto)")]
        public void ConvertFrom_NotANumber_IsRejected(string text)
        {
            using var scope = new CultureScope("es-ES");
            Assert.Throws<FormatException>(() => ReadAsTyped(text));
        }
    }
}
