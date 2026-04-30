using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents Rhd2000 sample rate in kHz.
    /// </summary>
    [TypeConverter(typeof(Rhd2000PsbDecoderSampleRateConverter))]
    public class Rhd2000PsbDecoderSampleRate
    {
        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 30.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate ThirtyKiloHertz = new(30.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 15.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate FifteenKiloHertz = new(15.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 10.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate TenKiloHertz = new(10.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 7.5 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate SevenPointFiveKiloHertz = new(7.5f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 5.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate FiveKiloHertz = new(5.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 3.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate ThreeKiloHertz = new(3.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 2.5 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate TwoPointFiveKiloHertz = new(2.5f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 2.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate TwoKiloHertz = new(2.0f);

        /// <summary>Gets a <see cref="Rhd2000PsbDecoderSampleRate"/> representing 1.0 kHz.</summary>
        public static readonly Rhd2000PsbDecoderSampleRate OneKiloHertz = new(1.0f);

        /// <summary>
        /// Gets all valid <see cref="Rhd2000PsbDecoderSampleRate"/> values in descending order.
        /// </summary>
        public static readonly IReadOnlyList<Rhd2000PsbDecoderSampleRate> All =
            new[] { ThirtyKiloHertz,
                    FifteenKiloHertz,
                    TenKiloHertz,
                    SevenPointFiveKiloHertz,
                    FiveKiloHertz,
                    ThreeKiloHertz,
                    TwoPointFiveKiloHertz,
                    TwoKiloHertz,
                    OneKiloHertz };

        /// <summary>
        /// Maps a sample rate value in kHz to its corresponding hardware register byte.
        /// </summary>
        /// <param name="value">The sample rate in kHz. Must be one of the predefined valid <see
        /// cref="Rhd2000PsbDecoderSampleRate"/> values.</param>
        /// <returns>The register byte corresponding to the given sample rate.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="value"/> does not match any valid sample rate.
        /// </exception>
        static public uint ToRegister(float value) => value switch
        {
            30.0f => 0x00,
            15.0f => 0x01,
            10.0f => 0x02,
            7.5f => 0x03,
            5.0f => 0x05,
            3.0f => 0x09,
            2.5f => 0x0B,
            2.0f => 0x0E,
            1.0f => 0x1D,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        /// <summary>
        /// Gets or sets the sample rate value in kHz.
        /// </summary>
        /// <remarks>
        /// The setter exists to support XML serialization. Prefer using the predefined
        /// static instances rather than setting this value directly.
        /// </remarks>
        public float Value { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Rhd2000PsbDecoderSampleRate"/> with a default
        /// value of <see cref="ThirtyKiloHertz"/>.
        /// </summary>
        /// <remarks>
        /// This constructor exists to support XML serialization and should not be used directly.
        /// Use the predefined static instances instead.
        /// </remarks>
        public Rhd2000PsbDecoderSampleRate() => Value = ThirtyKiloHertz.Value;
        private Rhd2000PsbDecoderSampleRate(float value) => Value = value;

        /// <summary>
        /// Implicitly converts a <see cref="Rhd2000PsbDecoderSampleRate"/> to a <see cref="float"/>.
        /// </summary>
        /// <param name="r">The sample rate to convert.</param>
        public static implicit operator float(Rhd2000PsbDecoderSampleRate r) => r.Value;

        /// <summary>
        /// Returns a string representation of the sample rate in the form "{Value} kHz".
        /// </summary>
        public override string ToString() => $"{Value} kHz";
    }

    /// <summary>
    /// Provides type conversion for <see cref="Rhd2000PsbDecoderSampleRate"/>.
    /// </summary>
    public class Rhd2000PsbDecoderSampleRateConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext ctx) => true;

        /// <inheritdoc/>
        /// <remarks>
        /// Returns <see langword="true"/> to restrict input to the predefined set of valid values,
        /// matching the behaviour of an enum.
        /// </remarks>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext ctx) => true;

        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext ctx)
            => new(Rhd2000PsbDecoderSampleRate.All.ToArray());

        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type t) => t == typeof(string);

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo culture, object value)
            => Rhd2000PsbDecoderSampleRate.All.First(r => r.ToString() == (string)value);

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo culture, object value, Type destType)
            => ((Rhd2000PsbDecoderSampleRate)value).ToString();
    }
}
