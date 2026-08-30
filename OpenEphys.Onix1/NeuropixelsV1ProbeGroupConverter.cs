using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Resolves <see cref="NeuropixelsV1ProbeGroup"/>'s concrete runtime type during JSON deserialization,
    /// based on the probe's part number.
    /// </summary>
    /// <remarks>
    /// Json.NET's default parameterized-constructor matching can only construct a concrete type, and which
    /// concrete <see cref="NeuropixelsV1ProbeGroup"/> subtype is correct (<see
    /// cref="NeuropixelsV1ChannelToContactProbeGroup"/> or <see
    /// cref="NeuropixelsV1ChannelGroupProbeGroup"/>) depends on the part number recorded in the data itself
    /// (<c>probes[0].annotations.model_name</c>, resolved the same way <see
    /// cref="NeuropixelsV1VariantRegistry.Resolve"/> already does). This converter reads just that field,
    /// resolves the variant, and delegates to normal deserialization against whichever concrete type is
    /// correct, so each subtype's own <c>[JsonConstructor]</c> still does the real work.
    /// <para>
    /// Registered explicitly (via a <see cref="JsonSerializerSettings.Converters"/> list) at the call sites
    /// that need it, <b>not</b> applied via a <c>[JsonConverter]</c> type attribute on the abstract base. NB:
    /// attribute-based application bypasses <see cref="CanConvert"/> and is inherited by derived types, so it
    /// would also fire for <see cref="ReadJson"/>'s own delegated call (<c>jObject.ToObject(concreteType,
    /// serializer)</c>), re-invoking this same converter on the concrete type and recursing until the stack
    /// overflows. Settings-based registration instead consults <see cref="CanConvert"/> to decide whether to
    /// use this converter at all, so the exact-type check below is what actually stops that inner call from
    /// re-triggering it: the concrete subtypes never match, so that call falls through to ordinary
    /// constructor-matching, unconditionally, regardless of what's in the serializer's converter list.
    /// </para>
    /// <para>
    /// Only intercepts reads (<see cref="CanWrite"/> is false): writing already uses the object's actual
    /// runtime type via ordinary reflection-based serialization, no resolution needed.
    /// </para>
    /// </remarks>
    sealed class NeuropixelsV1ProbeGroupConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(NeuropixelsV1ProbeGroup);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var modelName = jObject["probes"]?[0]?["annotations"]?["model_name"]?.Value<string>()
                ?? throw new JsonSerializationException(
                    $"Cannot resolve {nameof(NeuropixelsV1ProbeGroup)}'s concrete type: missing probes[0].annotations.model_name.");

            var variant = NeuropixelsV1VariantRegistry.Resolve(modelName);
            Type concreteType = variant.HasChannelGroupSelection
                ? typeof(NeuropixelsV1ChannelGroupProbeGroup)
                : typeof(NeuropixelsV1ChannelToContactProbeGroup);

            return jObject.ToObject(concreteType, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException($"{nameof(NeuropixelsV1ProbeGroupConverter)} does not support writing; {nameof(CanWrite)} is false.");
    }
}
