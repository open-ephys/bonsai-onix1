using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    // NB: Sets the current thread's culture for the lifetime of the scope, so a test can check code that must
    // not depend on the computer's regional settings (on Windows, CurrentCulture follows them)
    sealed class CultureScope : IDisposable
    {
        readonly CultureInfo previous;

        public CultureScope(string name)
            : this(CultureInfo.GetCultureInfo(name))
        {
        }

        public CultureScope(CultureInfo culture)
        {
            previous = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        public void Dispose() => Thread.CurrentThread.CurrentCulture = previous;

        // NB: Cultures with different number formats: en-US uses '.' as the decimal separator, es-ES and de-DE
        // use '.' as the group separator ("1.05" would read as 105), fr-FR uses ',' as the decimal separator and
        // a space as the group separator ("1.05" would not parse)
        public static TheoryData<string> Names => new() { "en-US", "es-ES", "de-DE", "fr-FR" };

        // NB: Windows lets users change the separators of any culture in the regional settings, so code that
        // only works for known culture names still fails on some computers
        public static CultureInfo CustomSeparators()
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();
            culture.NumberFormat.NumberDecimalSeparator = ",";
            culture.NumberFormat.NumberGroupSeparator = ".";
            return culture;
        }
    }
}
