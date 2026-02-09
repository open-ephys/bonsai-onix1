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
    public class FrameWriter : FileSink<RecordBatch, ArrowWriter>
    {
        static readonly ConstructorInfo recordBatchConstructor = typeof(RecordBatch)
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(Schema), typeof(IArrowArray[]), typeof(int) },
                null);

        /// <summary>
        /// Creates the <see cref="ArrowWriter"/> object used to write data to the specified stream.
        /// </summary>
        /// <param name="filename">The name of the file on which the elements should be written.</param>
        /// <param name="batch">The first input element that needs to be pushed into the file.</param>
        /// <returns>An <see cref="ArrowWriter"/> object.</returns>
        protected override ArrowWriter CreateWriter(string filename, RecordBatch batch)
        {
            return new ArrowWriter(filename, batch);
        }

        /// <summary>
        /// Writes a new <see cref="RecordBatch"/> element using the specified <see cref="ArrowWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="ArrowWriter"/> used to push elements into the stream.</param>
        /// <param name="input">The input <see cref="RecordBatch"/> that should be pushed into the stream.</param>
        protected override void Write(ArrowWriter writer, RecordBatch input)
        {
            writer.Write(input);
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

            return source.Publish(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = GetDataMembers(frameType);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateBufferedFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Process(source, frame => createRecordBatch(frame, schema)))
            );
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

            return source.Publish(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = GetDataMembers(frameType);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateDataFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Process(
                    frames.Buffer(BufferSize),
                    buffer => createRecordBatch(buffer, schema)
                ).SelectMany(batch => batch) // TODO: Figure out how to pass each frame as it comes in, not after the buffer fills
            ));
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));

            // TODO: Figure out a way to filter Quaternion.IsIdentity out of the schema, and subsequently the data
            //          - One idea: For value types, values are fields not properties. Not always true necessarily, need to check, but
            //          could filter Quaternion this way?
            // TODO: Add in the FrameWriterIgnoreAttribute Aaron originally implemented

            return members.OrderBy(member => member.MetadataToken);
        }

        static Schema GenerateSchema(IEnumerable<MemberInfo> members, object instance)
        {
            var fields = new List<Field>();

            foreach (var member in members)
            {
                var memberType = GetMemberType(member);

                if (memberType.IsPrimitive)
                {
                    fields.Add(new Field(member.Name, GetArrowType(memberType), false));
                }
                else if (memberType.IsArray)
                {
                    fields.Add(new Field(member.Name, GetArrowType(memberType.GetElementType()), false));
                }
                else if (memberType.IsEnum)
                {
                    fields.Add(new Field(member.Name, GetArrowType(Enum.GetUnderlyingType(memberType)), false));
                }
                else if (memberType.IsValueType)
                {
                    var structMembers = GetDataMembers(memberType);

                    // TODO: See if this should be converted to a recursive call?
                    //  Or, see CsvWriter for inspiration, use a Stack and add the member types to the stack,
                    //  keeping track of the parents so that we can build the full name for the field
                    foreach (var structMember in structMembers)
                    {
                        var structMemberType = GetMemberType(structMember);

                        if (structMemberType.IsPrimitive)
                        {
                            fields.Add(new Field($"{member.Name}_{structMember.Name}", GetArrowType(structMemberType), false));
                        }
                        else
                        {
                            throw new NotSupportedException($"The struct member type '{member.Name}_{structMember.Name}' is not supported for generating schemas.");
                        }
                    }
                }
                else if (memberType == typeof(Mat))
                {
                    var mat = GetMemberValue(member, instance) as Mat ?? throw new NullReferenceException($"No valid Mat property on the {instance.GetType()} object.");

                    for (int i = 0; i < mat.Rows; i++)
                    {
                        // Note: Could add an attribute to data frames properties to specify custom field naming
                        fields.Add(new Field($"{member.Name}Ch{i}", GetArrowType(mat.Depth), false));
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

        static Expression<Func<IList<DataFrame>, Schema, RecordBatch>> CreateDataFrameRecordBatchBuilder(Type frameType, IEnumerable<MemberInfo> members)
        {
            var expressions = new List<Expression>();
            var parameters = new List<ParameterExpression>();

            // Initialize parameters and variables
            var frames = Expression.Parameter(typeof(IList<DataFrame>), "frames");
            var schema = Expression.Parameter(typeof(Schema), "schema");
            var arrowArrays = Expression.Variable(typeof(IArrowArray[]), "arrowArrays");
            var recordBatch = Expression.Variable(typeof(RecordBatch), "recordBatch");
            var count = Expression.Property(
                Expression.Convert(
                    frames,
                    typeof(ICollection<DataFrame>)
                ),
                nameof(ICollection<DataFrame>.Count)
            );

            // Initialize the IArrowArray[] by getting the schema fields count
            parameters.Add(arrowArrays);
            expressions.Add(InitializeArrowArrayFromSchema(schema, arrowArrays));

            // Initialize the arrowArrayIndex variable for tracking where we are in the IArrowArray[]
            var arrowArrayIndex = Expression.Variable(typeof(int), "arrowArrayIndex");
            parameters.Add(arrowArrayIndex);
            expressions.Add(Expression.Assign(arrowArrayIndex, Expression.Constant(0)));

            var frameParameter = Expression.Parameter(typeof(DataFrame), "df");

            // Populate the IArrowArray[] with the appropriate arrays
            foreach (var member in members)
            {
                var memberType = GetMemberType(member);

                if (memberType.IsPrimitive)
                {
                    var memberAccessor = CreateMemberAccess(Expression.Convert(frameParameter, frameType), member);

                    expressions.Add(ConvertFrameMemberExpressionBuilder(memberType, frameParameter, arrowArrays, arrowArrayIndex, frames, count, memberAccessor));
                }
                else if (memberType.IsEnum)
                {
                    var memberAccessor = Expression.Convert(
                                            CreateMemberAccess(
                                                Expression.Convert(frameParameter, frameType),
                                                member
                                            ),
                                            Enum.GetUnderlyingType(memberType)
                                         );

                    expressions.Add(ConvertFrameMemberExpressionBuilder(Enum.GetUnderlyingType(memberType), frameParameter, arrowArrays, arrowArrayIndex, frames, count, memberAccessor));
                }
                else if (memberType.IsValueType)
                {
                    var structMembers = GetDataMembers(memberType);

                    // TODO: See if this should be converted to a recursive call?
                    //  Or, see CsvWriter for inspiration, use a Stack and add the member types to the stack
                    foreach (var structMember in structMembers)
                    {
                        var structMemberType = GetMemberType(structMember);

                        if (structMemberType.IsPrimitive)
                        {
                            var memberAccessor = CreateMemberAccess(
                                                    CreateMemberAccess(
                                                        Expression.Convert(frameParameter, frameType),
                                                        member
                                                    ),
                                                    structMember
                                                 );

                            expressions.Add(ConvertFrameMemberExpressionBuilder(structMemberType, frameParameter, arrowArrays, arrowArrayIndex, frames, count, memberAccessor));
                        }
                        else
                        {
                            throw new NotSupportedException($"The struct member type '{member.Name}_{structMember.Name}' is not supported for generating schemas.");
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException($"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            // Create the new RecordBatch from the schema, arrays, and length
            parameters.Add(recordBatch);
            expressions.Add(Expression.Assign(
                recordBatch,
                Expression.New(recordBatchConstructor, schema, arrowArrays, count)));

            // Build the expression tree body
            var createRecordBatch = Expression.Block(
                parameters,
                expressions);

            return Expression.Lambda<Func<IList<DataFrame>, Schema, RecordBatch>>(createRecordBatch, frames, schema);
        }

        static Expression<Func<BufferedDataFrame, Schema, RecordBatch>> CreateBufferedFrameRecordBatchBuilder(Type frameType, IEnumerable<MemberInfo> members)
        {
            var expressions = new List<Expression>();
            var parameters = new List<ParameterExpression>();

            // Initialize parameters and variables
            var frame = Expression.Parameter(typeof(BufferedDataFrame), "frame");
            var schema = Expression.Parameter(typeof(Schema), "schema");
            var arrowArrays = Expression.Variable(typeof(IArrowArray[]), "arrowArrays");
            var length = Expression.Variable(typeof(int), "length");
            var recordBatch = Expression.Variable(typeof(RecordBatch), "recordBatch");

            // Get the number of samples from the Clock array length
            parameters.Add(length);
            expressions.Add(Expression.Assign(
                length,
                Expression.ArrayLength(
                    Expression.Property(frame, nameof(BufferedDataFrame.Clock)))));

            // Initialize the IArrowArray[] by getting the schema fields count
            parameters.Add(arrowArrays);
            expressions.Add(InitializeArrowArrayFromSchema(schema, arrowArrays));

            // Initialize the arrowArrayIndex variable for tracking where we are in the IArrowArray[]
            var arrowArrayIndex = Expression.Variable(typeof(int), "arrowArrayIndex");
            parameters.Add(arrowArrayIndex);
            expressions.Add(Expression.Assign(arrowArrayIndex, Expression.Constant(0)));

            // Populate the IArrowArray[] with the appropriate arrays
            foreach (var member in members)
            {
                // TODO: Make recursive
                var memberType = GetMemberType(member);

                if (memberType.IsArray)
                {
                    var convertMethod = typeof(FrameWriter)
                        .GetMethod(nameof(ConvertArrayToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(memberType.GetElementType());

                    var arrayProperty = Expression.Property(frame, member.Name); // TODO: Test this where the member is not a part of BufferedDataFrame
                    var arrayArrowType = Expression.Constant(GetArrowType(memberType.GetElementType()));

                    var block = Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                convertMethod,
                                arrayProperty,
                                arrayArrowType,
                                length
                            )
                        ),
                        Expression.PostIncrementAssign(arrowArrayIndex)
                    );

                    expressions.Add(block);
                }
                else if (memberType == typeof(Mat))
                {
                    var convertMatRowMethod = typeof(FrameWriter)
                        .GetMethod(nameof(ConvertMatRowToArrowArray), BindingFlags.Static | BindingFlags.NonPublic);

                    // Extract all channels (rows) from the Mat and create an Arrow array for each row
                    var matProperty = Expression.Property(Expression.Convert(frame, frameType), member.Name);
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
                                matElementType
                            )
                        ),
                        Expression.PostIncrementAssign(arrowArrayIndex)
                    );

                    // Set the for loop here
                    var forLoop = Expression.Block(
                        new[] { matElementType, rowIndex },
                        Expression.Assign(rowIndex, Expression.Constant(0)),
                        Expression.Assign(
                            matElementType,
                            Expression.Call(
                                typeof(FrameWriter).GetMethod(nameof(GetArrowType), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Depth) }, null),
                                Expression.Property(matProperty, nameof(Mat.Depth))
                            )
                        ),
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(rowIndex, Expression.Property(matProperty, nameof(Mat.Rows))),
                                Expression.Block(
                                    loopBody,
                                    Expression.PostIncrementAssign(rowIndex)
                                ),
                                Expression.Break(breakLabel)
                            ),
                            breakLabel
                        )
                    );

                    expressions.Add(forLoop);
                }
                else
                {
                    throw new NotSupportedException($"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            // Create the new RecordBatch from the schema, arrays, and length
            parameters.Add(recordBatch);
            expressions.Add(Expression.Assign(
                recordBatch,
                Expression.New(recordBatchConstructor, schema, arrowArrays, length)));

            // Build the expression tree body
            var createRecordBatch = Expression.Block(
                parameters,
                expressions);

            return Expression.Lambda<Func<BufferedDataFrame, Schema, RecordBatch>>(createRecordBatch, frame, schema);
        }
    }
}
