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
        const int DefaultBufferSize = 1000;
        const int MinimumBufferSize = 100;

        static readonly Dictionary<Type, IArrowType> ArrowTypeMap = new()
        {
            [typeof(byte)] = UInt8Type.Default,
            [typeof(sbyte)] = Int8Type.Default,
            [typeof(ushort)] = UInt16Type.Default,
            [typeof(short)] = Int16Type.Default,
            [typeof(uint)] = UInt32Type.Default,
            [typeof(int)] = Int32Type.Default,
            [typeof(ulong)] = UInt64Type.Default,
            [typeof(long)] = Int64Type.Default,
            [typeof(float)] = FloatType.Default,
            [typeof(double)] = DoubleType.Default,
            [typeof(bool)] = BooleanType.Default,
            [typeof(string)] = StringType.Default
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

        internal static IArrowType GetArrowType(Type type) => ArrowTypeMap.TryGetValue(type, out var arrowType)
            ? arrowType
            : throw new NotSupportedException($"The type '{type}' is not supported for mapping to an ArrowType.");

        internal static IArrowType GetArrowType(Depth depth)
        {
            return depth switch
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

        static object GetMemberValue(MemberInfo member, object instance)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(instance),
                PropertyInfo propertyInfo => propertyInfo.GetValue(instance),
                _ => throw new InvalidOperationException($"Cannot get value of {member.GetType()} member from {instance.GetType()} object."),
            };
        }

        internal static Schema GenerateSchema(IEnumerable<MemberInfo> members, object instance)
        {
            var fields = new List<Field>();
            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = GetMemberType(current.Member);

                if (memberType.IsPrimitive)
                {
                    fields.Add(new Field(current.GetFullName(), GetArrowType(memberType), false));
                }
                else if (memberType.IsArray)
                {
                    fields.Add(new Field(current.GetFullName(), GetArrowType(memberType.GetElementType()), false));
                }
                else if (memberType.IsEnum)
                {
                    // TODO: See if the Dictionary type in Arrow would be better for enums
                    fields.Add(new Field(current.GetFullName(), GetArrowType(Enum.GetUnderlyingType(memberType)), false));
                }
                else if (memberType.IsValueType)
                {
                    var structMembers = GetDataMembers(memberType); 
                    foreach (var structMember in structMembers.Reverse())
                    {
                        if (IsMemberIgnored(current.Member, structMember))
                            continue;

                        stack.Push(new MemberNode
                        {
                            Member = structMember,
                            Parent = current
                        });
                    }
                }
                else if (memberType == typeof(Mat))
                {
                    var mat = GetMemberValue(current.Member, instance) as Mat ?? throw new NullReferenceException($"No valid Mat property on the {instance.GetType()} object.");

                    for (int i = 0; i < mat.Rows; i++)
                    {
                        // Note: Could add an attribute to data frames properties to specify custom field naming
                        fields.Add(new Field($"{current.GetFullName()}Ch{i}", GetArrowType(mat.Depth), false));
                    }
                }
                else
                {
                    throw new NotSupportedException($"The member type '{memberType}' is not supported for generating schemas.");
                }
            }

            return new Schema(fields, null);
        }

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
