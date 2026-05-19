using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using OpenCV.Net;

namespace OpenEphys.Onix1.DataFrameWriter
{
    static class DataFrameWriterHelper
    {
        /// <summary>
        /// Represents the default number of <see cref="DataFrame">DataFrames</see> in a <see
        /// cref="RecordBatch"/>.
        /// </summary>
        const int DefaultBufferSize = 1000;

        /// <summary>
        /// Represents the mimumum number of <see cref="DataFrame">DataFrames</see> in a <see
        /// cref="RecordBatch"/>.
        /// </summary>
        const int MinimumBufferSize = 100;

        internal static IArrowType GetArrowType(Type type) => type switch
        {
            _ when type == typeof(byte) => UInt8Type.Default,
            _ when type == typeof(sbyte) => Int8Type.Default,
            _ when type == typeof(ushort) => UInt16Type.Default,
            _ when type == typeof(short) => Int16Type.Default,
            _ when type == typeof(uint) => UInt32Type.Default,
            _ when type == typeof(int) => Int32Type.Default,
            _ when type == typeof(ulong) => UInt64Type.Default,
            _ when type == typeof(long) => Int64Type.Default,
            _ when type == typeof(float) => FloatType.Default,
            _ when type == typeof(double) => DoubleType.Default,
            _ when type == typeof(bool) => BooleanType.Default,
            _ when type == typeof(string) => StringType.Default,
            _ => throw new NotSupportedException($"The type '{type}' is not supported for mapping to an ArrowType.")
        };

        internal static IArrowType GetArrowType(Depth depth) => depth switch
        {
            Depth.U8 => GetArrowType(typeof(byte)),
            Depth.S8 => GetArrowType(typeof(sbyte)),
            Depth.U16 => GetArrowType(typeof(ushort)),
            Depth.S16 => GetArrowType(typeof(short)),
            Depth.S32 => GetArrowType(typeof(int)),
            Depth.F32 => GetArrowType(typeof(float)),
            Depth.F64 => GetArrowType(typeof(double)),
            _ => throw new NotSupportedException($"Cannot get the ArrowType for the given depth value '{depth}'.")
        };

        internal static IArrowArray ConvertArrayToArrowArray<T>(T[] array, IArrowType arrowType, int length) where T : unmanaged
        {
            var memory = array.AsMemory();
            var memoryAsBytes = CommunityToolkit.HighPerformance.MemoryExtensions.AsBytes(memory);
            var arrowBuffer = new ArrowBuffer(memoryAsBytes);

            var arrayData = new ArrayData(
                arrowType,
                length,
                0,
                0,
                new[] { ArrowBuffer.Empty, arrowBuffer },
                null,
                null
            );

            return ArrowArrayFactory.BuildArray(arrayData);
        }

        static MemberExpression CreateMemberAccess(Expression instance, MemberInfo member)
        {
            return member is PropertyInfo property
                ? Expression.Property(instance, property)
                : Expression.Field(instance, (FieldInfo)member);
        }

        internal static MemberExpression CreateMemberAccess(Expression instance, MemberNode member)
        {
            if (member.Parent == null)
                return CreateMemberAccess(instance, member.Member);

            return CreateMemberAccess(CreateMemberAccess(instance, member.Parent), member.Member);
        }

        internal static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));

            return members
                .Where(prop => prop.GetCustomAttribute(typeof(DataFrameWriterIgnoreAttribute)) == null)
                .OrderBy(member => member.MetadataToken);
        }

        internal static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new InvalidOperationException($"Unsupported member type ({member.GetType()})."),
            };
        }

        internal static bool IsMemberIgnored(MemberInfo rootMember, MemberInfo member)
        {
            var attr = rootMember.GetCustomAttribute<DataFrameWriterIgnoreMembersAttribute>();

            if (attr == null)
                return false;

            if (attr.MemberType.HasFlag(MemberType.Properties) && member is PropertyInfo)
                return true;

            else if (attr.MemberType.HasFlag(MemberType.Fields) && member is FieldInfo)
                return true;

            return false;
        }

        static object GetMemberValue(MemberInfo member, object instance) => member switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(instance),
            PropertyInfo propertyInfo => propertyInfo.GetValue(instance),
            _ => throw new InvalidOperationException($"Cannot get value of {member.GetType()} member from {instance.GetType()} object."),

        };

        static Field CreatePrimitiveField(MemberNode node, Type type) =>
            new(node.GetFullName(), GetArrowType(type), false);

        static Field CreateArrayField(MemberNode node, Type arrayType) =>
            new(node.GetFullName(), GetArrowType(arrayType.GetElementType()), false);

        static Field CreateEnumField(MemberNode node, Type enumType) =>
            new(node.GetFullName(), GetArrowType(Enum.GetUnderlyingType(enumType)), false);

        static IReadOnlyList<Field> CreateMatFields(MemberNode node, object instance)
        {
            var mat = GetMemberValue(node.Member, instance) as Mat
                ?? throw new NullReferenceException($"No valid Mat property on the {instance.GetType()} object.");
            var reeAttr = node.Member.GetCustomAttribute<SubsampledAttribute>();
            var fields = new List<Field>(mat.Rows);

            for (int i = 0; i < mat.Rows; i++)
            {
                var fieldName = $"{node.GetFullName()}{i}";
                fields.Add(reeAttr != null
                    ? new Field(fieldName, new RunEndEncodedType(
                        new Field($"runEnds{i}", Int32Type.Default, false),
                        new Field($"values{i}", GetArrowType(mat.Depth), false)), false)
                    : new Field(fieldName, GetArrowType(mat.Depth), false));
            }

            return fields;
        }

        internal static IReadOnlyList<MemberFieldGroup> BuildFieldMappings(IEnumerable<MemberInfo> members, object instance)
        {
            var groups = new List<MemberFieldGroup>();
            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = GetMemberType(current.Member);

                switch (memberType)
                {
                    case { IsPrimitive: true }:
                        groups.Add(new MemberFieldGroup(current, new[] { CreatePrimitiveField(current, memberType) }));
                        break;

                    case { IsArray: true }:
                        groups.Add(new MemberFieldGroup(current, new[] { CreateArrayField(current, memberType) }));
                        break;

                    case { IsEnum: true }:
                        groups.Add(new MemberFieldGroup(current, new[] { CreateEnumField(current, memberType) }));
                        break;

                    case { IsValueType: true }:
                        foreach (var structMember in GetDataMembers(memberType).Reverse())
                        {
                            if (!IsMemberIgnored(current.Member, structMember))
                                stack.Push(new MemberNode { Member = structMember, Parent = current });
                        }
                        break;

                    case var t when t == typeof(Mat):
                        groups.Add(new MemberFieldGroup(current, CreateMatFields(current, instance)));
                        break;

                    default:
                        throw new NotSupportedException($"The member type '{memberType}' is not supported for generating schema mappings.");
                }
            }

            return groups;
        }

        internal static Schema BuildSchema(IReadOnlyList<MemberFieldGroup> fieldGroups) => 
            new(fieldGroups.SelectMany(g => g.Fields), null);

        internal static int GetBufferSize(Type frameType)
        {
            var sampleRateAttribute = frameType.GetCustomAttribute<ExpectedSampleRateAttribute>();
            if (sampleRateAttribute != null)
            {
                const double BufferDurationSeconds = 1.0;
                var bufferSize = (int)(sampleRateAttribute.SampleRateHz * BufferDurationSeconds);
                return bufferSize >= MinimumBufferSize ? bufferSize : MinimumBufferSize;
            }

            return DefaultBufferSize;
        }
    }
}
