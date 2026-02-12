using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using Bonsai;
using Bonsai.IO;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents an operator that writes each data frames in the sequence
    /// to an Apache Arrow file using <see cref="ArrowWriter"/>.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class FrameWriter : FileSink
    {
        static readonly ConstructorInfo recordBatchConstructor = typeof(RecordBatch)
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(Schema), typeof(IArrowArray[]), typeof(int) },
                null);

        class BufferedDataFrameSink : FileSink<RecordBatch, ArrowWriter>
        {
            protected override ArrowWriter CreateWriter(string filename, RecordBatch batch)
            {
                return new ArrowWriter(filename, batch.Schema);
            }

            protected override void Write(ArrowWriter writer, RecordBatch input)
            {
                writer.Write(input);
            }

            public IObservable<BufferedDataFrame> Process(IObservable<BufferedDataFrame> source, Func<BufferedDataFrame, RecordBatch> selector)
            {
                return base.Process(source, selector);
            }
        }

        BufferedDataFrameSink CreateBufferedDataFrameSink()
        {
            return new BufferedDataFrameSink
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        /// <summary>
        /// Writes all of the data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <param name="source">The sequence of <see cref="BufferedDataFrame">BufferedDataFrame's</see> to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the frames to an Apache Arrow file.
        /// </returns>
        public IObservable<BufferedDataFrame> Process(IObservable<BufferedDataFrame> source)
        {
            Schema schema = null;
            Func<BufferedDataFrame, Schema, RecordBatch> createRecordBatch = null;

            return source.Replay(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = GetDataMembers(frameType);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateBufferedFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Observable.Defer(() => CreateBufferedDataFrameSink().Process(
                    frames,
                    frame => createRecordBatch(frame, schema)
                ))
            ), 1);
        }

        class DataFrameSink : FileSink<DataFrame, ArrowBatchWriter<DataFrame>>
        {
            readonly Schema schema;
            readonly Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch;
            readonly int bufferSize;

            public DataFrameSink(
                Schema schema,
                Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
                int bufferSize)
            {
                this.schema = schema;
                this.createRecordBatch = createRecordBatch;
                this.bufferSize = bufferSize;
            }

            protected override ArrowBatchWriter<DataFrame> CreateWriter(string filename, DataFrame frame)
            {
                return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, createRecordBatch);
            }

            protected override void Write(ArrowBatchWriter<DataFrame> writer, DataFrame input)
            {
                writer.Write(input);
            }
        }

        DataFrameSink CreateDataFrameSink(
            Schema schema,
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize)
        {
            return new DataFrameSink(schema, createRecordBatch, bufferSize)
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        /// <summary>
        /// Writes all of the data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <param name="source">The sequence of <see cref="DataFrame">DataFrame's</see> to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the frames to an Apache Arrow file.
        /// </returns>
        public IObservable<DataFrame> Process(IObservable<DataFrame> source)
        {
            const int BufferSize = 50;
            Schema schema = null;
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch = null;

            return source.Replay(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = GetDataMembers(frameType);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateDataFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Observable.Defer(() => CreateDataFrameSink(schema, createRecordBatch, BufferSize).Process(frames))
            ), 1);
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));

            return members
                .Where(prop => prop.GetCustomAttribute(typeof(FrameWriterIgnoreAttribute)) == null)
                .OrderBy(member => member.MetadataToken);
        }

        static bool IsMemberIgnored(MemberInfo rootMember, MemberInfo member)
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

        class MemberNode
        {
            public MemberInfo Member { get; set; }
            public MemberNode Parent { get; set; }

            public IEnumerable<MemberInfo> GetPath()
            {
                var current = this;
                while (current != null)
                {
                    yield return current.Member;
                    current = current.Parent;
                }
            }

            public string GetFullName()
            {
                return string.Join("_", GetPath().Select(m => m.Name).Reverse());
            }
        }

        static Schema GenerateSchema(IEnumerable<MemberInfo> members, object instance)
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

        static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new InvalidOperationException($"Unsupported member type ({member.GetType()})."),
            };
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

        static IArrowType GetArrowType(Type type)
        {
            if (type == typeof(sbyte))
                return Int8Type.Default;
            else if (type == typeof(short))
                return Int16Type.Default;
            else if (type == typeof(int))
                return Int32Type.Default;
            else if (type == typeof(long))
                return Int64Type.Default;
            else if (type == typeof(byte))
                return UInt8Type.Default;
            else if (type == typeof(ushort))
                return UInt16Type.Default;
            else if (type == typeof(uint))
                return UInt32Type.Default;
            else if (type == typeof(ulong))
                return UInt64Type.Default;
            else if (type == typeof(float))
                return FloatType.Default;
            else if (type == typeof(double))
                return DoubleType.Default;
            else if (type == typeof(bool))
                return BooleanType.Default;
            else if (type == typeof(string))
                return StringType.Default;
            else
                throw new NotSupportedException($"Cannot get the ArrowType for '{type.FullName}'.");
        }

        static IArrowType GetArrowType(Depth depth)
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

        static IArrowArray ConvertArrayToArrowArray<T>(T[] array, IArrowType arrowType, int length) where T : unmanaged
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

        static IArrowArray ConvertMatRowToArrowArray(Mat mat, int rowIndex, IArrowType elementType)
        {
            var rowManager = new MatRowMemoryManager(mat, rowIndex);
            var arrowBuffer = new ArrowBuffer(rowManager.Memory);

            var arrayData = new ArrayData(
                elementType,
                mat.Cols,
                0,
                0,
                new[] { ArrowBuffer.Empty, arrowBuffer },
                null,
                null
            );

            return ArrowArrayFactory.BuildArray(arrayData);
        }

        static Expression InitializeArrowArrayFromSchema(ParameterExpression schema, Expression arrowArrays)
        {
            var fieldsList = Expression.Property(schema, nameof(Schema.FieldsList));
            var fieldsListAsCollection = Expression.Convert(
                fieldsList,
                typeof(IReadOnlyCollection<Field>)
            );

            return Expression.Assign(
                    arrowArrays,
                    Expression.NewArrayBounds(
                        typeof(IArrowArray),
                        Expression.Property(fieldsListAsCollection, nameof(Schema.FieldsList.Count))
                    )
                );
        }

        static IArrowArray ConvertFrameMemberToArrowArray<TMember>(IList<DataFrame> frames, Func<DataFrame, TMember> getter, IArrowType arrowType, int length) where TMember : unmanaged
        {
            var array = new TMember[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = getter(frames[i]);
            }

            return ConvertArrayToArrowArray(array, arrowType, length);
        }

        static Expression ConvertFrameMemberExpressionBuilder(
            Type memberType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression frames,
            MemberExpression count,
            Expression memberAccessor)
        {
            var convertFrameMemberMethod = typeof(FrameWriter)
                        .GetMethod(nameof(ConvertFrameMemberToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(memberType);

            var arrayArrowType = Expression.Constant(GetArrowType(memberType));
            var getter = Expression.Lambda(
                            typeof(Func<,>).MakeGenericType(typeof(DataFrame), memberType),
                            memberAccessor,
                            frameParameter
                        ).Compile();

            var block = Expression.Block(
                Expression.Assign(
                    Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                    Expression.Call(
                        convertFrameMemberMethod,
                        frames,
                        Expression.Constant(getter, getter.GetType()),
                        arrayArrowType,
                        count
                    )
                ),
                Expression.PostIncrementAssign(arrowArrayIndex)
            );

            return block;
        }

        static MemberExpression CreateMemberAccess(Expression instance, MemberInfo member)
        {
            return member is PropertyInfo property
                ? Expression.Property(instance, property)
                : Expression.Field(instance, (FieldInfo)member);
        }

        /// <summary>
        /// Provides the type-specific logic for building RecordBatch expressions.
        /// </summary>
        interface IRecordBatchExpressionProvider
        {
            /// <summary>
            /// Gets the input parameter for the expression.
            /// </summary>
            ParameterExpression InputParameter { get; }

            /// <summary>
            /// Creates the expression that gets the number of records.
            /// </summary>
            Expression GetLengthExpression();

            /// <summary>
            /// Builds the expressions that populate the arrow arrays from frame members.
            /// </summary>
            List<Expression> GetArrayPopulationExpressions(
                ParameterExpression arrowArrays,
                ParameterExpression arrowArrayIndex,
                Expression length,
                Type frameType,
                IEnumerable<MemberInfo> members);
        }

        /// <summary>
        /// Provider for building <see cref="RecordBatch"/> expressions from <see cref="IList{DataFrame}"/>.
        /// </summary>
        class DataFrameExpressionProvider : IRecordBatchExpressionProvider
        {
            readonly ParameterExpression inputParameter;

            public DataFrameExpressionProvider()
            {
                inputParameter = Expression.Parameter(typeof(IList<DataFrame>), "frames");
            }

            public ParameterExpression InputParameter => inputParameter;

            public Expression GetLengthExpression()
            {
                return Expression.Property(
                        Expression.Convert(inputParameter, typeof(ICollection<DataFrame>)),
                        nameof(ICollection<DataFrame>.Count));
            }

            public List<Expression> GetArrayPopulationExpressions(
                ParameterExpression arrowArrays,
                ParameterExpression arrowArrayIndex,
                Expression length,
                Type frameType,
                IEnumerable<MemberInfo> members)
            {
                var frameParameter = Expression.Parameter(typeof(DataFrame), "df");
                List<Expression> expressions = new();

                // TODO: Convert to a stack here
                foreach (var member in members)
                {
                    var memberType = GetMemberType(member);

                    if (memberType.IsPrimitive)
                    {
                        var memberAccessor = CreateMemberAccess(
                            Expression.Convert(frameParameter, frameType),
                            member);

                        expressions.Add(ConvertFrameMemberExpressionBuilder(
                            memberType, frameParameter, arrowArrays, arrowArrayIndex,
                            inputParameter, (MemberExpression)length, memberAccessor));
                    }
                    else if (memberType.IsEnum)
                    {
                        var memberAccessor = Expression.Convert(
                            CreateMemberAccess(
                                Expression.Convert(frameParameter, frameType),
                                member),
                            Enum.GetUnderlyingType(memberType));

                        expressions.Add(ConvertFrameMemberExpressionBuilder(
                            Enum.GetUnderlyingType(memberType), frameParameter,
                            arrowArrays, arrowArrayIndex, inputParameter,
                            (MemberExpression)length, memberAccessor));
                    }
                    else if (memberType.IsValueType)
                    {
                        var structMembers = GetDataMembers(memberType);

                        foreach (var structMember in structMembers)
                        {
                            if (IsMemberIgnored(member, structMember))
                                continue;

                            var structMemberType = GetMemberType(structMember);

                            if (structMemberType.IsPrimitive)
                            {
                                // TODO: HERE, need to add a helper method to chain member access calls for struct members (e.g. df.Member1.StructMemberA)
                                var memberAccessor = CreateMemberAccess(
                                    CreateMemberAccess(
                                        Expression.Convert(frameParameter, frameType),
                                        member),
                                    structMember);

                                expressions.Add(ConvertFrameMemberExpressionBuilder(
                                    structMemberType, frameParameter, arrowArrays,
                                    arrowArrayIndex, inputParameter,
                                    (MemberExpression)length, memberAccessor));
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    $"The struct member type '{member.Name}_{structMember.Name}' is not supported for generating schemas.");
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                    }
                }

                return expressions;
            }
        }

        /// <summary>
        /// Provider for building <see cref="RecordBatch"/> expressions from <see cref="BufferedDataFrame"/>.
        /// </summary>
        class BufferedDataFrameExpressionProvider : IRecordBatchExpressionProvider
        {
            readonly ParameterExpression inputParameter;

            public BufferedDataFrameExpressionProvider()
            {
                inputParameter = Expression.Parameter(typeof(BufferedDataFrame), "frame");
            }

            public ParameterExpression InputParameter => inputParameter;

            public Expression GetLengthExpression()
            {
                return Expression.ArrayLength(
                        Expression.Property(
                            inputParameter,
                            nameof(BufferedDataFrame.Clock)));
            }

            public List<Expression> GetArrayPopulationExpressions(
                ParameterExpression arrowArrays,
                ParameterExpression arrowArrayIndex,
                Expression length,
                Type frameType,
                IEnumerable<MemberInfo> members)
            {
                List<Expression> expressions = new();

                // TODO: Convert to a stack
                foreach (var member in members)
                {
                    var memberType = GetMemberType(member);

                    if (memberType.IsArray)
                    {
                        var convertMethod = typeof(FrameWriter)
                            .GetMethod(nameof(ConvertArrayToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(memberType.GetElementType());

                        var arrayProperty = Expression.Property(inputParameter, member.Name);
                        var arrayArrowType = Expression.Constant(GetArrowType(memberType.GetElementType()));

                        var block = Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                                Expression.Call(
                                    convertMethod,
                                    arrayProperty,
                                    arrayArrowType,
                                    length)),
                            Expression.PostIncrementAssign(arrowArrayIndex));

                        expressions.Add(block);
                    }
                    else if (memberType == typeof(Mat))
                    {
                        var convertMatRowMethod = typeof(FrameWriter)
                            .GetMethod(nameof(ConvertMatRowToArrowArray), BindingFlags.Static | BindingFlags.NonPublic);

                        var matProperty = Expression.Property(
                            Expression.Convert(inputParameter, frameType), member.Name);
                        var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
                        var rowIndex = Expression.Variable(typeof(int), "rowIndex");
                        var breakLabel = Expression.Label("break");

                        var loopBody = Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                                Expression.Call(
                                    convertMatRowMethod,
                                    matProperty,
                                    rowIndex,
                                    matElementType)),
                            Expression.PostIncrementAssign(arrowArrayIndex));

                        var forLoop = Expression.Block(
                            new[] { matElementType, rowIndex },
                            Expression.Assign(rowIndex, Expression.Constant(0)),
                            Expression.Assign(
                                matElementType,
                                Expression.Call(
                                    typeof(FrameWriter).GetMethod(
                                        nameof(GetArrowType),
                                        BindingFlags.Static | BindingFlags.NonPublic,
                                        null,
                                        new[] { typeof(Depth) },
                                        null),
                                    Expression.Property(matProperty, nameof(Mat.Depth)))),
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.LessThan(
                                        rowIndex,
                                        Expression.Property(matProperty, nameof(Mat.Rows))),
                                    Expression.Block(
                                        loopBody,
                                        Expression.PostIncrementAssign(rowIndex)),
                                    Expression.Break(breakLabel)),
                                breakLabel));

                        expressions.Add(forLoop);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                    }
                }

                return expressions;
            }
        }

        /// <summary>
        /// Factory for creating <see cref="RecordBatch"/> builder expressions using a provider.
        /// </summary>
        static class RecordBatchExpressionFactory
        {
            public static Expression<TDelegate> CreateBuilder<TDelegate>(
                IRecordBatchExpressionProvider provider,
                Type frameType,
                IEnumerable<MemberInfo> members)
            {
                var expressions = new List<Expression>();
                var parameters = new List<ParameterExpression>();

                var arrowArrays = Expression.Variable(typeof(IArrowArray[]), "arrowArrays");
                var arrowArrayIndex = Expression.Variable(typeof(int), "arrowArrayIndex");
                var schemaParameter = Expression.Parameter(typeof(Schema), "schema");
                var lengthParameter = Expression.Parameter(typeof(int), "length");
                var length = provider.GetLengthExpression();

                parameters.Add(lengthParameter);
                expressions.Add(length);

                parameters.Add(arrowArrays);
                expressions.Add(InitializeArrowArrayFromSchema(schemaParameter, arrowArrays));

                parameters.Add(arrowArrayIndex);
                expressions.Add(Expression.Assign(arrowArrayIndex, Expression.Constant(0)));

                expressions.AddRange(
                    provider.GetArrayPopulationExpressions(
                        arrowArrays, arrowArrayIndex, length, frameType, members));

                var recordBatch = Expression.Variable(typeof(RecordBatch), "recordBatch");
                parameters.Add(recordBatch);
                expressions.Add(Expression.Assign(
                    recordBatch,
                    Expression.New(recordBatchConstructor, schemaParameter, arrowArrays, length)));

                var createRecordBatch = Expression.Block(parameters, expressions);

                return Expression.Lambda<TDelegate>(
                    createRecordBatch,
                    provider.InputParameter,
                    schemaParameter
                );
            }
        }

        static Expression<Func<IList<DataFrame>, Schema, RecordBatch>> CreateDataFrameRecordBatchBuilder(
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            return RecordBatchExpressionFactory.CreateBuilder<Func<IList<DataFrame>, Schema, RecordBatch>>(
                new DataFrameExpressionProvider(),
                frameType,
                members);
        }

        static Expression<Func<BufferedDataFrame, Schema, RecordBatch>> CreateBufferedFrameRecordBatchBuilder(
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            return RecordBatchExpressionFactory.CreateBuilder<Func<BufferedDataFrame, Schema, RecordBatch>>(
                new BufferedDataFrameExpressionProvider(),
                frameType,
                members);
        }
    }
}
