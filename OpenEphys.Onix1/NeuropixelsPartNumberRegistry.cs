using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Resolves a raw probe part number string, case-insensitively, against a table of known
    /// aliases for a probe family.
    /// </summary>
    /// <remarks>
    /// Holds no formula or bank logic of its own. Each probe family keeps its own descriptor type and table
    /// (e.g. <see cref="NeuropixelsV1VariantRegistry"/>, <see cref="NeuropixelsV2VariantRegistry"/>) and only
    /// routes part-number lookup through this shared helper.
    /// </remarks>
    internal static class NeuropixelsPartNumberRegistry
    {
        /// <summary>
        /// Resolves <paramref name="partNumber"/> to the descriptor whose alias list contains it.
        /// </summary>
        /// <param name="partNumber">The raw part number string to resolve.</param>
        /// <param name="entries">The family's alias-to-descriptor table.</param>
        /// <param name="familyName">The probe family name, used only in the exception message.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partNumber"/> is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="partNumber"/> does not match any alias.</exception>
        internal static T Resolve<T>(
            string partNumber,
            IReadOnlyList<(string[] Aliases, T Descriptor)> entries,
            string familyName)
        {
            if (string.IsNullOrEmpty(partNumber))
                throw new ArgumentException("A Neuropixels probe part number must be provided.", nameof(partNumber));

            foreach (var (Aliases, Descriptor) in entries)
            {
                foreach (var alias in Aliases)
                {
                    if (string.Equals(alias, partNumber, StringComparison.OrdinalIgnoreCase))
                        return Descriptor;
                }
            }

            throw new NotSupportedException($"Unrecognized Neuropixels {familyName} part number '{partNumber}'.");
        }
    }
}
