using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using OpenCV.Net;

namespace OpenEphys.Onix1.FrameWriter
{
    static class FrameWriterHelper
    {
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
                .Where(prop => prop.GetCustomAttribute(typeof(FrameWriterIgnoreAttribute)) == null)
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
            var attr = rootMember.GetCustomAttribute<FrameWriterIgnoreMembersAttribute>();

            if (attr == null)
                return false;

            if (attr.MemberType.HasFlag(MemberType.Properties) && member is PropertyInfo)
                return true;

            else if (attr.MemberType.HasFlag(MemberType.Fields) && member is FieldInfo)
                return true;

            return false;
        }
    }
}
