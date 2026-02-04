using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using Bonsai;
using Bonsai.IO;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents an operator that writes each data frame in the sequence
    /// to an Apache Arrow file using <see cref="ArrowWriter"/>.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class FrameWriter : StreamSink<RecordBatch, ArrowWriter>
    {
        static readonly ConstructorInfo arrowBufferConstructor = typeof(ArrowBuffer).GetConstructor(new Type[] { typeof(ReadOnlyMemory<byte>) });
        static readonly ConstructorInfo arrayDataConstructor = typeof(ArrayData).GetConstructor(new[] {
                                                                    typeof(IArrowType),
                                                                    typeof(int),
                                                                    typeof(int),
                                                                    typeof(int),
                                                                    typeof(ArrowBuffer[]),
                                                                    typeof(ArrayData[]),
                                                                    typeof(ArrayData)
                                                                });
        /// <summary>
        /// Creates the <see cref="ArrowWriter"/> object used to write data to the specified stream.
        /// </summary>
        /// <param name="stream">The stream on which the elements should be written.</param>
        /// <returns>An <see cref="ArrowWriter"/> object.</returns>
        protected override ArrowWriter CreateWriter(Stream stream)
        {
            return new ArrowWriter(stream);
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
        /// there is an additional side effect of writing the arrays to an Apache Arrow file.
        /// </returns>
        public IObservable<BufferedDataFrame> Process(IObservable<BufferedDataFrame> source)
        {
            Schema schema = null;
            Func<BufferedDataFrame, Schema, RecordBatch> createRecordBatch = null;

            return Process(source, input =>
            {
                if (schema == null)
                {
                    var members = GetDataMembers(input.GetType());
                    schema = GenerateSchema(members, input);
                    createRecordBatch = CreateRecordBatchBuilder(input.GetType(), members).Compile();
                }

                return createRecordBatch(input, schema);
            });
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));

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
                else if (memberType == typeof(Mat))
                {
                    var mat = GetMemberValue(member, instance) as Mat ?? throw new NullReferenceException($"No valid Mat property on the {instance.GetType()} object.");

                    for (int i = 0; i < mat.Rows; i++)
                    {
                        // Note: Could add an attribute to data frame properties to specify custom field naming
                        fields.Add(new Field($"{member.Name}Ch{i}", GetArrowType(mat.Depth), false));
                    }
                }
                else
                {
                    throw new NotSupportedException($"The member type '{memberType.FullName}' is not supported for generating schemas.");
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

        static Expression<Func<BufferedDataFrame, Schema, RecordBatch>> CreateRecordBatchBuilder(Type frameType, IEnumerable<MemberInfo> members)
        {
            // Grab the RecordBatch constructor method info
            ConstructorInfo recordBatchConstructor = typeof(RecordBatch).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(Schema), typeof(IArrowArray[]), typeof(int) },
                null);

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
            var fieldsList = Expression.Property(schema, nameof(Schema.FieldsList));
            var fieldsListAsCollection = Expression.Convert(
                fieldsList,
                typeof(IReadOnlyCollection<Field>)
            );

            parameters.Add(arrowArrays);
            expressions.Add(
                Expression.Assign(
                    arrowArrays,
                    Expression.NewArrayBounds(
                        typeof(IArrowArray),
                        Expression.Property(fieldsListAsCollection, nameof(Schema.FieldsList.Count))
                    )
                )
            );

            // Initialize the arrowArrayIndex variable for tracking where we are in the IArrowArray[]
            var arrowArrayIndex = Expression.Variable(typeof(int), "arrowArrayIndex");
            parameters.Add(arrowArrayIndex);
            expressions.Add(Expression.Assign(arrowArrayIndex, Expression.Constant(0)));

            // Populate the IArrowArray[] with the appropriate arrays
            foreach (var member in members)
            {
                var memberType = GetMemberType(member);

                if (memberType.IsArray)
                {
                    var asMemoryMethod = typeof(System.MemoryExtensions)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m =>
                            m.Name == nameof(System.MemoryExtensions.AsMemory) &&
                            m.IsGenericMethodDefinition &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType.IsArray)
                        .MakeGenericMethod(memberType.GetElementType());

                    var asBytesMethod = typeof(CommunityToolkit.HighPerformance.MemoryExtensions)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m =>
                            m.Name == nameof(CommunityToolkit.HighPerformance.MemoryExtensions.AsBytes) &&
                            m.IsGenericMethodDefinition
                        )
                        .MakeGenericMethod(memberType.GetElementType());

                    var arrayProperty = Expression.Property(frame, member.Name);
                    var arrayAsMemory = Expression.Variable(typeof(Memory<byte>), "arrayAsMemory");
                    var arrowBuffer = Expression.Variable(typeof(ArrowBuffer), "arrowBuffer");
                    var arrayArrowType = Expression.Constant(GetArrowType(memberType.GetElementType()));

                    var block = Expression.Block(
                        new[] { arrayAsMemory, arrowBuffer },
                        Expression.Assign(
                            arrayAsMemory,
                            Expression.Call(
                                asBytesMethod,
                                Expression.Call(
                                    asMemoryMethod,
                                    arrayProperty
                                )
                            )
                        ),
                        Expression.Assign(
                            arrowBuffer,
                            Expression.New(
                                arrowBufferConstructor,
                                Expression.Convert(arrayAsMemory, typeof(ReadOnlyMemory<byte>))
                            )
                        ),
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                typeof(ArrowArrayFactory),
                                nameof(ArrowArrayFactory.BuildArray),
                                null,
                                Expression.New(
                                    arrayDataConstructor,
                                    arrayArrowType,
                                    length,
                                    Expression.Constant(0),
                                    Expression.Constant(0),
                                    Expression.NewArrayInit(
                                        typeof(ArrowBuffer),
                                        Expression.Property(null, typeof(ArrowBuffer), "Empty"),
                                        arrowBuffer
                                    ),
                                    Expression.Constant(null, typeof(ArrayData[])),
                                    Expression.Constant(null, typeof(ArrayData))
                                )
                            )
                        ),
                        Expression.PostIncrementAssign(arrowArrayIndex)
                    );

                    expressions.Add(block);
                }
                else if (memberType == typeof(Mat))
                {
                    var matRowMemoryManagerConstructor = typeof(MatRowMemoryManager).GetConstructor(new Type[] { typeof(Mat), typeof(int) });

                    // Extract all channels (rows) from the Mat and create an Arrow array for each row
                    var matProperty = Expression.Property(Expression.Convert(frame, frameType), member.Name);
                    var rowManager = Expression.Variable(typeof(MatRowMemoryManager), "rowManager");
                    var arrowBuffer = Expression.Variable(typeof(ArrowBuffer), "arrowBuffer");
                    var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
                    var rowIndex = Expression.Variable(typeof(int), "rowIndex");

                    var breakLabel = Expression.Label("break");

                    var loopBody = Expression.Block(
                        new[] { rowManager, arrowBuffer },
                        Expression.Assign(
                            rowManager,
                            Expression.New(
                                matRowMemoryManagerConstructor,
                                matProperty,
                                rowIndex
                            )
                        ),
                        Expression.Assign(
                            arrowBuffer,
                            Expression.New(
                                arrowBufferConstructor,
                                Expression.Convert(Expression.Property(rowManager, nameof(MatRowMemoryManager.Memory)), typeof(ReadOnlyMemory<byte>))
                            )
                        ),
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                typeof(ArrowArrayFactory),
                                nameof(ArrowArrayFactory.BuildArray),
                                null,
                                Expression.New(
                                    arrayDataConstructor,
                                    matElementType, // TODO: Potential inefficiency here, try to cache the element type in some way so it is not recomputed every frame.
                                    Expression.Property(matProperty, nameof(Mat.Cols)),
                                    Expression.Constant(0),
                                    Expression.Constant(0),
                                    Expression.NewArrayInit(
                                        typeof(ArrowBuffer),
                                        Expression.Property(null, typeof(ArrowBuffer), "Empty"),
                                        arrowBuffer
                                    ),
                                    Expression.Constant(null, typeof(ArrayData[])),
                                    Expression.Constant(null, typeof(ArrayData))
                                )
                            )
                        ),
                        Expression.PostIncrementAssign(arrowArrayIndex)
                    );

                    // Set the outer loop here
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
